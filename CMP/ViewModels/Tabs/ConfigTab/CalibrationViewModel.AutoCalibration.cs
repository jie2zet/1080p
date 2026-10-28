using CMP.Constants;
using CMP.Services.Calibration;
using HandyControl.Controls;

namespace CMP.ViewModels.Tabs.ConfigTab
{
    /// <summary>
    /// CalibrationViewModel 的自动校准部分（从第一项目 CMP-DDD 迁移）。
    /// 流程：逐行取 Set → 写设定压力并加压 → 等 5 秒 → 读 PLC 反馈(Read) + DPI 实测(Act) → 收尾排气/恢复公式开关。
    /// </summary>
    public partial class CalibrationViewModel
    {
        /// <summary>DPI 串口同时只允许一路校准使用（每个工位各有一个 VM 实例，所以用静态锁）。</summary>
        private static readonly SemaphoreSlim DpiCalibrationLock = new(1, 1);

        private CancellationTokenSource? _autoCalibrationCancellation;
        private bool _isAutoCalibrating;
        private string _autoCalibrationStatus = string.Empty;

        /// <summary>自动校准期间暂停后台实时读取，避免与校准读取争用同一个 PLC 连接。</summary>
        private volatile bool _suspendLiveRead;

        /// <summary>当前工位是否支持自动校准。</summary>
        public bool SupportsAutoCalibration => CalibrationPressure.SupportsStation(_polishIdentifier);

        /// <summary>是否正在自动校准。</summary>
        public bool IsAutoCalibrating
        {
            get => _isAutoCalibrating;
            private set
            {
                if (!SetProperty(ref _isAutoCalibrating, value)) return;

                RaisePropertyChanged(nameof(CanEditCalibration));
                AutoCalibrationCommand?.RaiseCanExecuteChanged();
                StopAutoCalibrationCommand?.RaiseCanExecuteChanged();
                SaveZoneCommand?.RaiseCanExecuteChanged();
                CopyToZoneCommand?.RaiseCanExecuteChanged();
                AddDataCommand?.RaiseCanExecuteChanged();
                AssignCommand?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>校准期间禁止编辑表格。</summary>
        public bool CanEditCalibration => !IsAutoCalibrating;

        /// <summary>自动校准状态文本。</summary>
        public string AutoCalibrationStatus
        {
            get => _autoCalibrationStatus;
            private set => SetProperty(ref _autoCalibrationStatus, value);
        }

        /// <summary>分区卡片上的“Auto Calibration”按钮命令。</summary>
        public DelegateCommand<string> AutoCalibrationCommand { get; private set; } = null!;

        /// <summary>停止自动校准命令。</summary>
        public DelegateCommand StopAutoCalibrationCommand { get; private set; } = null!;

        /// <summary>后台实时读取循环据此跳过读取。</summary>
        internal bool SuspendLiveRead => _suspendLiveRead;

        private void InitializeAutoCalibration()
        {
            AutoCalibrationCommand = new DelegateCommand<string>(RunAutoCalibration, CanRunAutoCalibration);
            StopAutoCalibrationCommand = new DelegateCommand(
                () => _autoCalibrationCancellation?.Cancel(),
                () => IsAutoCalibrating);
        }

        private bool CanRunAutoCalibration(string name)
        {
            if (!SupportsAutoCalibration || IsAutoCalibrating || string.IsNullOrWhiteSpace(name))
                return false;

            if (CalibrationPressure.GetTarget(_polishIdentifier, name) == null)
                return false;

            return CalibModelDictionary != null &&
                   CalibModelDictionary.TryGetValue(name, out var rows) &&
                   rows.Count > 0;
        }

        private async void RunAutoCalibration(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (!AutoCalibrationCommand.CanExecute(name)) return;

            var station = _polishIdentifier;
            var target = CalibrationPressure.GetTarget(station, name);
            if (target == null) return;

            var link = kvLink;
            if (link == null || link.sock < 0)
            {
                AutoCalibrationStatus = $"{station} {name}: PLC 未连接，无法开始自动校准。";
                Growl.Error(AutoCalibrationStatus);
                return;
            }

            if (!CalibModelDictionary.TryGetValue(name, out var models)) return;

            // Set 为 0 表示有效数据结束，只采集第一个 0 之前的数据行。
            var rows = models.TakeWhile(row => row.Set != 0f)
                .Select(row => (Row: row, Set: row.Set))
                .ToArray();
            if (rows.Length == 0)
            {
                AutoCalibrationStatus = $"{station} {name}: Set=0，没有需要校准的数据。";
                return;
            }

            var portName = ChamberExponentMap.DpiSerialPort.PortName.Trim();
            var baudRate = ChamberExponentMap.DpiSerialPort.BaudRate;

            using var cancellation = new CancellationTokenSource();
            _autoCalibrationCancellation = cancellation;
            IsAutoCalibrating = true;
            _suspendLiveRead = true;

            var restoreCalibrationSwitch = false;
            var pressureMayBeActive = false;
            var completed = false;
            var lockAcquired = false;

            try
            {
                if (rows.Any(row => !float.IsFinite(row.Set)))
                    throw new InvalidOperationException("Set 必须是有效数值。");

                if (string.IsNullOrWhiteSpace(portName) || baudRate <= 0)
                    throw new InvalidOperationException("DPI 串口配置无效，请检查 ChamberExponentMap.DpiSerialPort。");

                cancellation.Token.ThrowIfCancellationRequested();

                lockAcquired = await DpiCalibrationLock.WaitAsync(0, cancellation.Token);
                if (!lockAcquired)
                    throw new InvalidOperationException("DPI 正被另一项校准占用。");

                AutoCalibrationStatus = $"{station} {name}: 打开 DPI 串口 {portName}";
                using var connection = new DpiSerialConnection(portName, baudRate);
                await Task.Run(connection.Open, cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();

                // 校准期间关闭 PLC 压力校准公式（若配置了开关），让 PLC 按原始设定值加压。
                restoreCalibrationSwitch = target.CalibrationSwitchHandle.HasValue;
                if (restoreCalibrationSwitch)
                {
                    await Task.Run(() => CalibrationPressure.SetCalibrationEnabled(link, target, false), cancellation.Token);
                    cancellation.Token.ThrowIfCancellationRequested();
                }

                for (var i = 0; i < rows.Length; i++)
                {
                    cancellation.Token.ThrowIfCancellationRequested();

                    if (i > 0) rows[i - 1].Row.IsAutoCalibrationActive = false;
                    rows[i].Row.IsAutoCalibrationActive = true;
                    AutoCalibrationStatus = $"{station} {name}: {i + 1}/{rows.Length}, Set={rows[i].Set}";

                    var item = rows[i];
                    await Task.Run(() =>
                    {
                        cancellation.Token.ThrowIfCancellationRequested();
                        pressureMayBeActive = true;
                        CalibrationPressure.Set(link, target, item.Set);
                    }, cancellation.Token);

                    // 等气路稳定后再读取（与原项目一致的 5 秒）。
                    await Task.Delay(5000, cancellation.Token);

                    var read = await Task.Run(() => CalibrationPressure.Read(link, target), cancellation.Token);
                    var dpi = await connection.ReadPressureAsync(cancellation.Token);

                    cancellation.Token.ThrowIfCancellationRequested();
                    item.Row.Read = MathF.Round(read, 3, MidpointRounding.AwayFromZero);
                    item.Row.Act = MathF.Round(dpi, 3, MidpointRounding.AwayFromZero);
                }

                completed = true;
            }
            catch (OperationCanceledException)
            {
                completed = false;
                AutoCalibrationStatus = $"{station} {name}: 已停止。";
            }
            catch (Exception ex)
            {
                completed = false;
                AutoCalibrationStatus = $"{AutoCalibrationStatus} — 失败：{ex.Message}";
                Growl.Error(AutoCalibrationStatus);
            }
            finally
            {
                // 停止或失败时也收尾，避免气缸留在加压状态。
                if (pressureMayBeActive)
                {
                    try
                    {
                        await Task.Run(() => CalibrationPressure.Vent(link, target));
                    }
                    catch (Exception ex)
                    {
                        completed = false;
                        AutoCalibrationStatus = $"{station} {name}: 排气失败：{ex.Message}";
                        Growl.Error(AutoCalibrationStatus);
                    }
                }

                if (restoreCalibrationSwitch)
                {
                    try
                    {
                        await Task.Run(() => CalibrationPressure.SetCalibrationEnabled(link, target, true));
                    }
                    catch (Exception ex)
                    {
                        completed = false;
                        AutoCalibrationStatus = $"{station} {name}: 恢复压力校准公式失败：{ex.Message}";
                        Growl.Error(AutoCalibrationStatus);
                    }
                }

                if (completed)
                {
                    AutoCalibrationStatus = $"{station} {name}: 已完成（{rows.Length}/{rows.Length}）。请点击 Save 保存结果。";
                    Growl.Success("自动校准完成，请点击 Save 保存结果。");
                }

                if (lockAcquired) DpiCalibrationLock.Release();
                _autoCalibrationCancellation = null;
                foreach (var row in rows) row.Row.IsAutoCalibrationActive = false;
                _suspendLiveRead = false;
                IsAutoCalibrating = false;
            }
        }
    }
}
