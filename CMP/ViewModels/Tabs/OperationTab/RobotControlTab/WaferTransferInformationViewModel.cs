using System.Windows.Threading;
using CMP.Constants.Plc.RobotControl;
using CMP.Lang;
using CMP.Services.Log;
using CMP.Services.Plc;
using CMP.ViewModels.Common;

namespace CMP.ViewModels.Tabs.OperationTab.RobotControlTab
{
    public class WaferTransferInformationViewModel : StationOpViewModelBase
    {
        private readonly MitsubishiPlcService _plc;
        private readonly DispatcherTimer _readTimer;

        public WaferTransferInformationViewModel() : base(0, Resources.WaferTransferInformationMap)
        {
            _plc = MitsubishiPlcService.RobotCtrlInstance;

            _readTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _readTimer.Tick += ReadTimer_Tick;
            _readTimer.Start();
        }

        private async void ReadTimer_Tick(object? sender, EventArgs e)
        {
            if (_isReading)
            {
                return;
            }

            if (!_plc.IsConnected && !MitsubishiPlcService.DebugMode) return;

            _isReading = true;

            try
            {
                await ReadPlcDataAsync();
            }
            catch (Exception ex)
            {
                Logger.For(Logger.Category.RobotControl).Error("Error reading PLC data", ex);
            }
            finally
            {
                _isReading = false;
            }
        }

        private async Task ReadPlcDataAsync()
        {
            // Robot1 Lower
            Robot1LowerArmVal1 = await ReadAsync(WaferTransferInformationAddress.Robot1LowerArmVal1);
            Robot1LowerArmVal2 = await ReadAsync(WaferTransferInformationAddress.Robot1LowerArmVal2);

            // Transfer Module
            TransferModuleVal1 = await ReadAsync(WaferTransferInformationAddress.TransferModuleVal1);
            TransferModuleVal2 = await ReadAsync(WaferTransferInformationAddress.TransferModuleVal2);

            // Robot2 Upper
            Robot2UpperArmVal1 = await ReadAsync(WaferTransferInformationAddress.Robot2UpperArmVal1);
            Robot2UpperArmVal2 = await ReadAsync(WaferTransferInformationAddress.Robot2UpperArmVal2);

            // A Side Transfer
            ASideTransferVal1 = await ReadAsync(WaferTransferInformationAddress.ASideTransferVal1);
            ASideTransferVal2 = await ReadAsync(WaferTransferInformationAddress.ASideTransferVal2);

            // Robot2 Lower
            Robot2LowerArmVal1 = await ReadAsync(WaferTransferInformationAddress.Robot2LowerArmVal1);
            Robot2LowerArmVal2 = await ReadAsync(WaferTransferInformationAddress.Robot2LowerArmVal2);

            // LTP1
            LTP1Val1 = await ReadAsync(WaferTransferInformationAddress.Ltp1Val1);
            LTP1Val2 = await ReadAsync(WaferTransferInformationAddress.Ltp1Val2);

            // PolishA
            PolishAVal1 = await ReadAsync(WaferTransferInformationAddress.PolishAVal1);
            PolishAVal2 = await ReadAsync(WaferTransferInformationAddress.PolishAVal2);

            // Roll1A1
            Roll1A1Val1 = await ReadAsync(WaferTransferInformationAddress.Roll1A1Val1);
            Roll1A1Val2 = await ReadAsync(WaferTransferInformationAddress.Roll1A1Val2);

            // LTP2
            LTP2Val1 = await ReadAsync(WaferTransferInformationAddress.Ltp2Val1);
            LTP2Val2 = await ReadAsync(WaferTransferInformationAddress.Ltp2Val2);

            // PolishB
            PolishBVal1 = await ReadAsync(WaferTransferInformationAddress.PolishBVal1);
            PolishBVal2 = await ReadAsync(WaferTransferInformationAddress.PolishBVal2);

            // Roll1A2
            Roll1A2Val1 = await ReadAsync(WaferTransferInformationAddress.Roll1A2Val1);
            Roll1A2Val2 = await ReadAsync(WaferTransferInformationAddress.Roll1A2Val2);

            // LTP3
            LTP3Val1 = await ReadAsync(WaferTransferInformationAddress.Ltp3Val1);
            LTP3Val2 = await ReadAsync(WaferTransferInformationAddress.Ltp3Val2);

            // Robot3 Upper
            Robot3UpperArmVal1 = await ReadAsync(WaferTransferInformationAddress.Robot3UpperArmVal1);
            Robot3UpperArmVal2 = await ReadAsync(WaferTransferInformationAddress.Robot3UpperArmVal2);

            // Robot1 Upper
            Robot1UpperArmVal1 = await ReadAsync(WaferTransferInformationAddress.Robot1UpperArmVal1);
            Robot1UpperArmVal2 = await ReadAsync(WaferTransferInformationAddress.Robot1UpperArmVal2);

            // Robot3 Lower
            Robot3LowerArmVal1 = await ReadAsync(WaferTransferInformationAddress.Robot3LowerArmVal1);
            Robot3LowerArmVal2 = await ReadAsync(WaferTransferInformationAddress.Robot3LowerArmVal2);

            // DryA
            DryAVal1 = await ReadAsync(WaferTransferInformationAddress.DryAVal1);
            DryAVal2 = await ReadAsync(WaferTransferInformationAddress.DryAVal2);

            // Robot4 Upper
            Robot4UpperArmVal1 = await ReadAsync(WaferTransferInformationAddress.Robot4UpperArmVal1);
            Robot4UpperArmVal2 = await ReadAsync(WaferTransferInformationAddress.Robot4UpperArmVal2);

            // PenA1
            PenA1Val1 = await ReadAsync(WaferTransferInformationAddress.PenA1Val1);
            PenA1Val2 = await ReadAsync(WaferTransferInformationAddress.PenA1Val2);

            // Robot4 Lower
            Robot4LowerArmVal1 = await ReadAsync(WaferTransferInformationAddress.Robot4LowerArmVal1);
            Robot4LowerArmVal2 = await ReadAsync(WaferTransferInformationAddress.Robot4LowerArmVal2);

            // Roll1A3
            Roll1A3Val1 = await ReadAsync(WaferTransferInformationAddress.Roll1A3Val1);
            Roll1A3Val2 = await ReadAsync(WaferTransferInformationAddress.Roll1A3Val2);
        }

        private async Task<string> ReadAsync(string address)
        {
            var result = await _plc.ReadAsync<int>(address);
            return result.Content.ToString();
        }

        #region Properties

        private string _robot1LowerArmVal1 = "0000";
        public string Robot1LowerArmVal1 { get => _robot1LowerArmVal1; set => SetProperty(ref _robot1LowerArmVal1, value); }

        private string _robot1LowerArmVal2 = "0000";
        public string Robot1LowerArmVal2 { get => _robot1LowerArmVal2; set => SetProperty(ref _robot1LowerArmVal2, value); }

        private string _transferModuleVal1 = "0000";
        public string TransferModuleVal1 { get => _transferModuleVal1; set => SetProperty(ref _transferModuleVal1, value); }

        private string _transferModuleVal2 = "0000";
        public string TransferModuleVal2 { get => _transferModuleVal2; set => SetProperty(ref _transferModuleVal2, value); }

        private string _robot2UpperArmVal1 = "0000";
        public string Robot2UpperArmVal1 { get => _robot2UpperArmVal1; set => SetProperty(ref _robot2UpperArmVal1, value); }

        private string _robot2UpperArmVal2 = "0000";
        public string Robot2UpperArmVal2 { get => _robot2UpperArmVal2; set => SetProperty(ref _robot2UpperArmVal2, value); }

        private string _aSideTransferVal1 = "0000";
        public string ASideTransferVal1 { get => _aSideTransferVal1; set => SetProperty(ref _aSideTransferVal1, value); }

        private string _aSideTransferVal2 = "0000";
        public string ASideTransferVal2 { get => _aSideTransferVal2; set => SetProperty(ref _aSideTransferVal2, value); }

        private string _robot2LowerArmVal1 = "0000";
        public string Robot2LowerArmVal1 { get => _robot2LowerArmVal1; set => SetProperty(ref _robot2LowerArmVal1, value); }

        private string _robot2LowerArmVal2 = "0000";
        public string Robot2LowerArmVal2 { get => _robot2LowerArmVal2; set => SetProperty(ref _robot2LowerArmVal2, value); }

        private string _ltp1Val1 = "0000";
        public string LTP1Val1 { get => _ltp1Val1; set => SetProperty(ref _ltp1Val1, value); }

        private string _ltp1Val2 = "0000";
        public string LTP1Val2 { get => _ltp1Val2; set => SetProperty(ref _ltp1Val2, value); }

        private string _polishAVal1 = "0000";
        public string PolishAVal1 { get => _polishAVal1; set => SetProperty(ref _polishAVal1, value); }

        private string _polishAVal2 = "0000";
        public string PolishAVal2 { get => _polishAVal2; set => SetProperty(ref _polishAVal2, value); }

        private string _roll1A1Val1 = "0000";
        public string Roll1A1Val1 { get => _roll1A1Val1; set => SetProperty(ref _roll1A1Val1, value); }

        private string _roll1A1Val2 = "0000";
        public string Roll1A1Val2 { get => _roll1A1Val2; set => SetProperty(ref _roll1A1Val2, value); }

        private string _ltp2Val1 = "0000";
        public string LTP2Val1 { get => _ltp2Val1; set => SetProperty(ref _ltp2Val1, value); }

        private string _ltp2Val2 = "0000";
        public string LTP2Val2 { get => _ltp2Val2; set => SetProperty(ref _ltp2Val2, value); }

        private string _polishBVal1 = "0000";
        public string PolishBVal1 { get => _polishBVal1; set => SetProperty(ref _polishBVal1, value); }

        private string _polishBVal2 = "0000";
        public string PolishBVal2 { get => _polishBVal2; set => SetProperty(ref _polishBVal2, value); }

        private string _roll1A2Val1 = "0000";
        public string Roll1A2Val1 { get => _roll1A2Val1; set => SetProperty(ref _roll1A2Val1, value); }

        private string _roll1A2Val2 = "0000";
        public string Roll1A2Val2 { get => _roll1A2Val2; set => SetProperty(ref _roll1A2Val2, value); }

        private string _ltp3Val1 = "0000";
        public string LTP3Val1 { get => _ltp3Val1; set => SetProperty(ref _ltp3Val1, value); }

        private string _ltp3Val2 = "0000";
        public string LTP3Val2 { get => _ltp3Val2; set => SetProperty(ref _ltp3Val2, value); }

        private string _robot3UpperArmVal1 = "0000";
        public string Robot3UpperArmVal1 { get => _robot3UpperArmVal1; set => SetProperty(ref _robot3UpperArmVal1, value); }

        private string _robot3UpperArmVal2 = "0000";
        public string Robot3UpperArmVal2 { get => _robot3UpperArmVal2; set => SetProperty(ref _robot3UpperArmVal2, value); }

        private string _robot1UpperArmVal1 = "0000";
        public string Robot1UpperArmVal1 { get => _robot1UpperArmVal1; set => SetProperty(ref _robot1UpperArmVal1, value); }

        private string _robot1UpperArmVal2 = "0000";
        public string Robot1UpperArmVal2 { get => _robot1UpperArmVal2; set => SetProperty(ref _robot1UpperArmVal2, value); }

        private string _robot3LowerArmVal1 = "0000";
        public string Robot3LowerArmVal1 { get => _robot3LowerArmVal1; set => SetProperty(ref _robot3LowerArmVal1, value); }

        private string _robot3LowerArmVal2 = "0000";
        public string Robot3LowerArmVal2 { get => _robot3LowerArmVal2; set => SetProperty(ref _robot3LowerArmVal2, value); }

        private string _dryAVal1 = "0000";
        public string DryAVal1 { get => _dryAVal1; set => SetProperty(ref _dryAVal1, value); }

        private string _dryAVal2 = "0000";
        public string DryAVal2 { get => _dryAVal2; set => SetProperty(ref _dryAVal2, value); }

        private string _robot4UpperArmVal1 = "0000";
        public string Robot4UpperArmVal1 { get => _robot4UpperArmVal1; set => SetProperty(ref _robot4UpperArmVal1, value); }

        private string _robot4UpperArmVal2 = "0000";
        public string Robot4UpperArmVal2 { get => _robot4UpperArmVal2; set => SetProperty(ref _robot4UpperArmVal2, value); }

        private string _penA1Val1 = "0000";
        public string PenA1Val1 { get => _penA1Val1; set => SetProperty(ref _penA1Val1, value); }

        private string _penA1Val2 = "0000";
        public string PenA1Val2 { get => _penA1Val2; set => SetProperty(ref _penA1Val2, value); }

        private string _robot4LowerArmVal1 = "0000";
        public string Robot4LowerArmVal1 { get => _robot4LowerArmVal1; set => SetProperty(ref _robot4LowerArmVal1, value); }

        private string _robot4LowerArmVal2 = "0000";
        public string Robot4LowerArmVal2 { get => _robot4LowerArmVal2; set => SetProperty(ref _robot4LowerArmVal2, value); }

        private string _roll1A3Val1 = "0000";
        public string Roll1A3Val1 { get => _roll1A3Val1; set => SetProperty(ref _roll1A3Val1, value); }

        private string _roll1A3Val2 = "0000";
        public string Roll1A3Val2 { get => _roll1A3Val2; set => SetProperty(ref _roll1A3Val2, value); }

        #endregion
    }
}

