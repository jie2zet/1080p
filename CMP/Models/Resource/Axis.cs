using CMP.Models.DataCenter;
using CMP.Models.Opertion.LoadPort;
using CMP.Services.Plc;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using HslCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CMP.Models.Resource
{
    public class Axis
    {
        const int AutoSpeed1BaseAddressParameter = 10000;
        const int AutoSpeed2BaseAddressParameter = 10002;
        const int AutoSpeed3BaseAddressParameter = 10004;
        const int AutoSpeed4BaseAddressParameter = 10006;
        const int HomeHSpeedBaseAddressParameter = 10008;
        const int HomeLSpeedBaseAddressParameter = 10010;
        static readonly int[] StationPositionBaseAddressParameter = new int[] { 10018, 10020, 10022, 10024, 10026, 10028, 10030, 10032, 10034, 10036, 10038, 10040, 10042, 10044, 10046, 10048 };
        const int OffsetParameter = 100;

        const int ServonBaseAddressCommand = 1000;
        const int JogPositiveBaseAddressCommand = 1001;
        const int JogNegtiveBaseAddressCommand = 1002;
        const int GoHomeBaseAddressCommand = 1003;
        static readonly int[] GoToStationIDBaseAddressCommand = new int[] { 1016, 1017, 1018, 1019, 1020, 1021, 1022, 1023, 1024, 1025, 1026, 1027, 1028, 1029, 1030, 1031 };
        const int JogVelocityBaseAddressCommand = 1002;
        const int OffsetCommandForBite = 32;
        const int OffsetCommandForDouble = 20;

        const int EnabledIndex = 0;
        const int HomedoneIndex = 1;
        const int ReadyIndex = 2;
        const int BusyIndex = 3;
        const int UpperLimitIndex = 4;
        const int LowerLimitIndex = 5;
        const int OriginIndex = 6;
        const int LimitAlarmIndex = 7;
        const int WarningIndex = 8;
        const int DriveAlarm = 9;
        const int ActionTimeOut = 10;
        const int SaftyOffIndex = 11;
        static readonly int[] StationPositionDone = new int[] { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27 };
        const int CurrentPositionIndex = 28;
        const int CurrentVelocityIndex = 29;
        const int CurrentTorqueIndex = 30;
        const int AlarmCodeIndex = 31;
        const int AlarmCode2Index = 32;

        public string axisName = string.Empty;
        int axisID = 0;
        public List<SV_PLC> listDataInSingleAxis;
        MitsubishiPlcService _plcService;
        public event EventHandler<AxisDataRefreshedEventArgs>? AxisDataRefreshed;
        
        protected virtual void OnAxisDataRefreshed()
        {
            AxisDataRefreshed?.Invoke(this, new AxisDataRefreshedEventArgs(axisName, axisID, DateTime.Now));
        }
        
        public Axis(string name, int axisID)
        {
            this.axisName = name;
            this.axisID = axisID;
            listDataInSingleAxis = new List<SV_PLC>();
            this._plcService = GlobalService.plcComm;
            // _plcService = MitsubishiPlcService.GetOrCreateForPort(...);
        }

        public void RefreshAxisData()  //更新单个轴
        {
            for (int i = 0; i < listDataInSingleAxis.Count; i++)
            {
                if (listDataInSingleAxis[i].Type == "Bool")
                {
                    listDataInSingleAxis[i].Value = _plcService.ReadBoolVari(listDataInSingleAxis[i].PLCLink);
                }
                if (listDataInSingleAxis[i].Type == "INT_32")
                {
                    listDataInSingleAxis[i].Value = _plcService.ReadIntVari(listDataInSingleAxis[i].PLCLink);
                }
                if (listDataInSingleAxis[i].Type == "UINT_16")
                {
                    listDataInSingleAxis[i].Value = _plcService.ReadShortVari(listDataInSingleAxis[i].PLCLink);
                }
            }
            OnAxisDataRefreshed();
        }

        public void RefreshSingleAxisDataByIndex(int index)
        {
            if (listDataInSingleAxis[index].Type == "Bool")
            {
                listDataInSingleAxis[index].Value = _plcService.ReadBoolVari(listDataInSingleAxis[index].PLCLink);
            }
            if (listDataInSingleAxis[index].Type == "INT_32")
            {
                listDataInSingleAxis[index].Value = _plcService.ReadIntVari(listDataInSingleAxis[index].PLCLink);
            }
            if (listDataInSingleAxis[index].Type == "UINT_16")
            {
                listDataInSingleAxis[index].Value = _plcService.ReadShortVari(listDataInSingleAxis[index].PLCLink);
            }
        }

        //public void WriteSingleData(int index, string value)
        //{
        //    if (listDataInSingleAxis[index].Type == "Bool")
        //    {
        //        _plcService.Write<bool>(listDataInSingleAxis[index].PLCLink, Convert.ToBoolean(value));
        //    }
        //    if (listDataInSingleAxis[index].Type == "INT_32")
        //    {
        //        mcBinaryPlcComm.WriteIntVari(listDataInSingleAxis[index].PLCLink, value);
        //    }
        //    if (listDataInSingleAxis[index].Type == "UINT_16")
        //    {
        //        mcBinaryPlcComm.WriteUShortVari(listDataInSingleAxis[index].PLCLink, value);
        //    }
        //}

        public OperateResult ServerControl(bool OnOff)
        {
            return _plcService.WriteBoolVari(GetCommandPLCAddress(ServonBaseAddressCommand, PLCDataType.Bite), OnOff);
        }

        public OperateResult ModifyVelocity(int velocity)
        {
            return _plcService.WriteIntVari(GetCommandPLCAddress(JogVelocityBaseAddressCommand, PLCDataType.Double), velocity);
        }

        public OperateResult GoToStationPosition(int posID)
        {
            return _plcService.WriteBoolVari(GetCommandPLCAddress(GoToStationIDBaseAddressCommand[posID - 1], PLCDataType.Bite), true);
        }

        public OperateResult GoHome()
        {
            return _plcService.WriteBoolVari(GetCommandPLCAddress(GoHomeBaseAddressCommand, PLCDataType.Bite), true);
        }

        public OperateResult Stop()
        {
            OperateResult stopHomeResult = _plcService.WriteBoolVari(GetCommandPLCAddress(GoHomeBaseAddressCommand, PLCDataType.Bite), false);
            bool[] stopValue = new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
            OperateResult stopGoToPosResult = _plcService.WriteBoolVariContinuous(GetCommandPLCAddress(GoToStationIDBaseAddressCommand[0], PLCDataType.Bite), stopValue);
            return new OperateResult
            {
                IsSuccess = stopHomeResult.IsSuccess && stopGoToPosResult.IsSuccess,
                Message = stopHomeResult.Message + "    " + stopGoToPosResult.Message
            };
        }

        public OperateResult StartJog(string direction)
        {
            if(direction == "JogPlus")
            {
                return _plcService.WriteBoolVari(GetCommandPLCAddress(JogPositiveBaseAddressCommand, PLCDataType.Bite), true); 
            }
            else
            {
                return _plcService.WriteBoolVari(GetCommandPLCAddress(JogNegtiveBaseAddressCommand, PLCDataType.Bite), true);
            }
        }

        public OperateResult StopJog(string direction)
        {
            if (direction == "JogPlus")
            {
                return _plcService.WriteBoolVari(GetCommandPLCAddress(JogPositiveBaseAddressCommand, PLCDataType.Bite), false);
            }
            else
            {
                return _plcService.WriteBoolVari(GetCommandPLCAddress(JogNegtiveBaseAddressCommand, PLCDataType.Bite), false);
            }
        }

        public OperateResult SetStationPosition(int posID, int currentPosition)
        {
            return _plcService.WriteIntVari(GetParameterPLCAddress(StationPositionBaseAddressParameter[posID - 1]), currentPosition);
        }

        // 在Axis类中添加此方法
        public int GetCurrentPosition()
        {
            try
            {
                // 查找当前位置数据点
                var currentPositionSV = listDataInSingleAxis.FirstOrDefault(sv => sv.Name.Contains("CurrentPosition"));
                if (currentPositionSV != null && !string.IsNullOrEmpty(currentPositionSV.PLCLink))
                {
                    var intResult = _plcService.ReadIntVari(currentPositionSV.PLCLink);
                    if (intResult.IsSuccess)
                    {
                        return intResult.Content;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取轴{axisName}当前位置时出错: {ex.Message}");
            }

            return 0;
        }
        public string GetParameterPLCAddress(int baseAddress)
        {
            return "D" + (baseAddress + axisID * OffsetParameter);
        }

        public string GetCommandPLCAddress(int baseAddress, PLCDataType type)
        {
            switch (type)
            {
                case PLCDataType.Bite:
                    return "M" + (baseAddress + axisID * OffsetCommandForBite);
                case PLCDataType.Double:
                    return "D" + (baseAddress + axisID * OffsetCommandForDouble);
                default:
                    return "Undefined Type";
            }
        }
    }

    public enum PLCDataType
    {
        Bite,
        word,
        Double,
    }
    
    public class AxisDataRefreshedEventArgs : EventArgs
    {
        public string AxisName { get; }
        public int AxisId { get; }
        public DateTime RefreshTime { get; }

        public AxisDataRefreshedEventArgs(string axisName, int axisId, DateTime refreshTime)
        {
            AxisName = axisName;
            AxisId = axisId;
            RefreshTime = refreshTime;
        }
    }
}
