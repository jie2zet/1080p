using CMP;
using CMP.Models.Resource;
using CMP.Services;
using CMP.Services.LoadPort;
using HandyControl.Controls;
using Prism.Commands;
using System.Windows.Input;

namespace CMP.Models.Operation.LoadPort
{
    public partial class LoadPortData
    {
        private bool _isStopLoad;

        /// <summary>
        /// 本口已停止入料：不再从本口取新片，机内片继续加工后放回。
        /// </summary>
        public bool IsStopLoad
        {
            get => _isStopLoad;
            private set => SetProperty(ref _isStopLoad, value);
        }

        public ICommand StopLoadCommand { get; private set; }

        private void InitStopLoadCommand()
        {
            StopLoadCommand = new DelegateCommand(ExecuteStopLoad, () => !IsStopLoad)
                .ObservesProperty(() => IsStopLoad);
        }

        /// <summary>
        /// 停止本口继续入料（纯软件）。不影响其它口，不写全局 M170。
        /// </summary>
        public void ExecuteStopLoad()
        {
            ExecuteStopLoad(notify: true);
        }

        public void ExecuteStopLoad(bool notify)
        {
            if (IsStopLoad)
                return;

            var loadportId = GetLoadportId();
            if (loadportId < 0)
            {
                AddLogMessage("StopLoad 失败：无法解析 LoadportID");
                return;
            }

            IsStopLoad = true;
            NotifyHostUnloadRequested();

            var inFlight = FindInFlightPickWafer(loadportId);
            var removed = WaferPickQueue.RemoveByLoadportIdExcept(loadportId, inFlight);
            var inFlightHint = inFlight != null
                ? $"，保留进行中取片 L{inFlight.LoadportID}S{inFlight.SlotID}"
                : string.Empty;

            AddLogMessage($"StopLoad：已停止 {TabName} 入料，移除待取 {removed} 片{inFlightHint}。机内片继续加工后放回。");
            if (notify)
                Growl.InfoGlobal($"{TabName} 已停止入料");
            WaferRuntimeStore.Instance.RequestSave();
        }

        /// <summary>
        /// 本口重新 Load / Assign / Unload 完成后恢复入料。
        /// </summary>
        public void ClearStopLoad()
        {
            if (!IsStopLoad)
                return;

            IsStopLoad = false;
            AddLogMessage($"StopLoad：{TabName} 已恢复允许入料");
            WaferRuntimeStore.Instance.RequestSave();
        }

        internal Wafer GetProcessingWafer()
        {
            lock (_processingLock)
            {
                return _processingWafer;
            }
        }

        internal static bool IsInFlightPickWafer(Wafer wafer)
        {
            if (wafer == null)
                return false;

            foreach (var lp in LoadPortManager.Instance.GetAllLoadPorts().Values)
            {
                var processing = lp.GetProcessingWafer();
                if (processing != null &&
                    processing.LoadportID == wafer.LoadportID &&
                    processing.SlotID == wafer.SlotID)
                {
                    return true;
                }
            }

            return false;
        }

        private static Wafer? FindInFlightPickWafer(int loadportId)
        {
            foreach (var lp in LoadPortManager.Instance.GetAllLoadPorts().Values)
            {
                var processing = lp.GetProcessingWafer();
                if (processing != null && processing.LoadportID == loadportId)
                    return processing;
            }

            return null;
        }
    }
}
