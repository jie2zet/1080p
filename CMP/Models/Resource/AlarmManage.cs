using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using KHT = CMP.Models.kvsPlc.KvHostLink;
using KHST = CMP.Models.kvsPlc.Alarm.KvStruct;
using KHSTPolishAlarm = CMP.Models.kvsPlc.Polish.KvStructPolishAlarm;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using System.Net.NetworkInformation;
using CMP.Models.Warn;
using static CMP.Models.kvsPlc.Alarm.KvStruct;
using Dm.util;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Reflection;
using DryIoc.ImTools;
using DocumentFormat.OpenXml.Wordprocessing;
using CMP.Services.Log;
using HandyControl.Controls;
using CMP.Services.Reflection;

namespace CMP.Models.Resource
{
    public abstract class AlarmManage
    {
        public string alarmName = string.Empty;
        public KVLink kvlink;
        public int err;
        public Dictionary<string,Alarm > dicAlarm;
        //public FieldInfo[] fields;

        public AlarmManage(string name, KVLink link)
        {
            alarmName = name;
            this.kvlink = link;
            dicAlarm = new Dictionary<string, Alarm>();
            //fields = typeof(stPolishAlarm).GetFields();
            if ("Robot".Equals(name))
            {
                InitRobotAlarm();
            }
            else {
                InitAlarm();
            }
                
        }

        public void InitAlarm()
        {
            string alarmCSVPath = $"D:\\CMP\\Configuration\\Alarm\\{alarmName}.csv";
            using (var reader = new StreamReader(alarmCSVPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // 跳过标题行
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    Alarm alarm = new Alarm();
                    alarm.AlarmId = Convert.ToInt32(csv.GetField<string>(0));
                    if (csv.TryGetField(1, out string? nameValue))
                    {
                        if (nameValue != null)
                        {
                            alarm.AlarmName = nameValue;
                            alarm.PropertyName = nameValue.Replace(" ", "").TrimEnd();
                        }
                    }
                    if (csv.TryGetField(3, out bool Enabledvalue))
                    {
                        alarm.Enabled = Enabledvalue;
                    }
                    if(csv.TryGetField(4, out string typeValue))
                    {
                        alarm.Type = typeValue;
                    }
                    if (csv.TryGetField(5, out string unitValue))
                    {
                        alarm.Unit = (Unit)Enum.Parse(typeof(Unit), unitValue); 
                    }
                    if (dicAlarm.ContainsKey(alarm.PropertyName))
                    {
                        dicAlarm[alarm.PropertyName] = alarm;
                    }
                    else
                    {
                        dicAlarm.Add(alarm.PropertyName, alarm);
                    }
                }
            }
        }
        public void InitRobotAlarm()
        {
            string alarmCSVPath = $"D:\\CMP\\Configuration\\Alarm\\{alarmName}.csv";
            using (var reader = new StreamReader(alarmCSVPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // 跳过标题行
                csv.Read();
                csv.ReadHeader();
                Alarm alarm = null;
                while (csv.Read())
                {
                    alarm = new Alarm();
                    alarm.AlarmCode = csv.GetField<string>(0);

                    if (csv.TryGetField(5, out bool EnabledValue))
                    {
                        alarm.Enabled = EnabledValue;
                    }
                    if (csv.TryGetField(3, out string? DescValue))
                    {
                        alarm.Description = DescValue;
                    }
                    if (csv.TryGetField(6, out string? TypeValue))
                    {
                        alarm.Type = TypeValue;
                    }
                    if (csv.TryGetField(7, out string unitValue))
                    {
                        alarm.Unit = (Unit)Enum.Parse(typeof(Unit), unitValue);
                    }
                    if (csv.TryGetField(1, out string? NameValue))
                    {
                        alarm.AlarmName = NameValue;
                    }
                    if (csv.TryGetField(0, out string? value))
                    {
                        if (value != null)
                        {

                            alarm.PropertyName = value.Replace(" ", "");
                        }
                    }
                    dicAlarm.Add(alarm.PropertyName, alarm);
                }
            }
        }

        public abstract List<Alarm> CollectAlarm();

        protected static bool CanRaiseAlarm(Alarm? alarm)
        {
            return alarm != null
                && alarm.Enabled
                && ChamberAvailability.IsEnabled(alarm.Unit)
                && !alarm.IsAlarmed;
        }

        //public static class Keys
        //{
        //    public const string PolishARecipeRequestTimeout = "Polish A Recipe Request Timeout";
        //    public const string PolishARecipeDataError = "Polish A Recipe Data Error";
        //    public const string PolishARecipeDataTransmissionFromHMIAbnormalEnd = "Polish A Recipe Data Transmission From HMI Abnormal End";
        //    public const string PolishAStepActionTimeout = "Polish A Step Action Timeout";
        //    public const string PolishAECRunningParameterInvailid = "Polish A EC Running Parameter Invailid";
        //    public const string PolishAHeadWaferSensorAndInformationNotMatch = "Polish A Head Wafer Sensor And Information Not Match";
        //    public const string PolishAProcessLoadRetryFail = "Polish A Process Load Retry Fail";
        //    public const string PolishAProcessUnloadRetryFail = "Polish A Process Unload Retry Fail";
        //    public const string PolishAProcessDechuckRetryFail = "Polish A Process Dechuck Retry Fail";
        //    public const string PolishAPLCSystemError = "Polish A PLC System Error";
        //    public const string PolishATransferPLCCommunicationError = "Polish A Transfer PLC Communication Error";
        //    public const string PolishAXH16ECMotionUnitMotionEnableFault = "Polish A XH16EC Motion Unit Motion Enable Fault";
        //    public const string PolishAXH16ECMotionUnitFault = "Polish A XH16EC Motion Unit Fault";
        //    public const string PolishAXH16ECMotionUnitAxisFault = "Polish A XH16EC Motion Unit Axis Fault";
        //    public const string PolishAXH16ECMotionUnitAxisWarning = "Polish A XH16EC Motion Unit Axis Warning";
        //    public const string PolishAXH16ECEtherCATLinkOff = "Polish A XH16EC EtherCAT Link Off";
        //    public const string PolishAPlatenRotationMotorAxisWarning = "Polish A Platen Rotation Motor Axis Warning";
        //    public const string PolishAPlatenRotationMotorDriverWarning = "Polish A Platen Rotation Motor Driver Warning";
        //    public const string PolishAPlatenRotationMotorAxisFault = "Polish A Platen Rotation Motor Axis Fault";
        //    public const string PolishAPlatenRotationMotorDriverAlarm = "Polish A Platen Rotation Motor Driver Alarm";
        //    public const string PolishAHeadRotationMotorAxisWarning = "Polish A Head Rotation Motor Axis Warning";
        //    public const string PolishAHeadRotationMotorDriverWarning = "Polish A Head Rotation Motor Driver Warning";
        //    public const string PolishAHeadRotationMotorAxisFault = "Polish A Head Rotation Motor Axis Fault";
        //    public const string PolishAHeadRotationMotorDriverAlarm = "Polish A Head Rotation Motor Driver Alarm";
        //    public const string PolishAHeadSweepMotorAxisWarning = "Polish A Head Sweep Motor Axis Warning";
        //    public const string PolishAHeadSweepMotorDriverWarning = "Polish A Head Sweep Motor Driver Warning";
        //    public const string PolishAHeadSweepMotorAxisFault = "Polish A Head Sweep Motor Axis Fault";
        //    public const string PolishAHeadSweepMotorDriverAlarm = "Polish A Head Sweep Motor Driver Alarm";
        //    public const string PolishAHeadUpDownMotorAxisWarning = "Polish A Head UpDown Motor Axis Warning";
        //    public const string PolishAHeadUpDownMotorDriverWarning = "Polish A Head UpDown Motor Driver Warning";
        //    public const string PolishAHeadUpDownMotorAxisFault = "Polish A Head UpDown Motor Axis Fault";
        //    public const string PolishAHeadUpDownMotorAxisAlarm = "Polish A Head UpDown Motor Axis Alarm";
        //    public const string PolishAConditionerHeadRotationMotorAxisWarning = "Polish A Conditioner Head Rotation Motor Axis Warning";
        //    public const string PolishAConditionerHeadRotationMotorDriverWarning = "Polish A Conditioner Head Rotation Motor Driver Warning";
        //    public const string PolishAConditionerHeadRotationMotorAxisFault = "Polish A Conditioner Head Rotation Motor Axis Fault";
        //    public const string PolishAConditionerHeadRotationMotorDriverAlarm = "Polish A Conditioner Head Rotation Motor Driver Alarm";
        //    public const string PolishAConditionerSweepMotorAxisWarning = "Polish A Conditioner Sweep Motor Axis Warning";
        //    public const string PolishAConditionerSweepMotorDriverWarning = "Polish A Conditioner Sweep Motor Driver Warning";
        //    public const string PolishAConditionerSweepMotorAxisFault = "Polish A Conditioner Sweep Motor Axis Fault";
        //    public const string PolishAConditionerSweepMotorDriverAlarm = "Polish A Conditioner Sweep Motor Driver Alarm";
        //    public const string PolishAChemicalArmMotorAxisWarning = "Polish A Chemical Arm Motor Axis Warning";
        //    public const string PolishAChemicalArmMotorDriverWarning = "Polish A Chemical Arm Motor Driver Warning";
        //    public const string PolishAChemicalArmMotorAxisFault = "Polish A Chemical Arm Motor Axis Fault";
        //    public const string PolishAChemicalArmMotorDriverAlarm = "Polish A Chemical Arm Motor Driver Alarm";
        //    public const string PolishAPlatenRotationMotorRunActionTimeout = "Polish A Platen Rotation Motor Run Action Timeout";
        //    public const string PolishAPlatenRotationMotorStopActionTimeout = "Polish A Platen Rotation Motor Stop Action Timeout";
        //    public const string PolishAPlatenRotationMotorInchingActionTimeout = "Polish A Platen Rotation Motor Inching Action Timeout";
        //    public const string PolishAPlatenRotationMotorCoordinateChangeActionTimeout = "Polish A Platen Rotation Motor Coordinate Change Action Timeout";
        //    public const string PolishAPlatenRotationMotorDriverServoUnready = "Polish A Platen Rotation Motor Driver Servo Unready";
        //    public const string PolishAHeadRotationMotorRunActionTimeout = "Polish A Head Rotation Motor Run Action Timeout";
        //    public const string PolishAHeadRotationMotorStopActionTimeout = "Polish A Head Rotation Motor Stop Action Timeout";
        //    public const string PolishAHeadRotationMotorInchingActionTimeout = "Polish A Head Rotation Motor Inching Action Timeout";
        //    public const string PolishAHeadRotationMotorCoordinateChangeActionTimeout = "Polish A Head Rotation Motor Coordinate Change Action Timeout";
        //    public const string PolishAHeadRotationMotorDriverServoUnready = "Polish A Head Rotation Motor Driver Servo Unready";
        //    public const string PolishAHeadSweepMotorMoveToAnyPositionActionTimeout = "Polish A Head Sweep Motor Move To Any Position Action Timeout";
        //    public const string PolishAHeadSweepMotorStopActionTimeout = "Polish A Head Sweep Motor Stop Action Timeout";
        //    public const string PolishAHeadSweepMotorInchingActionTimeout = "Polish A Head Sweep Motor Inching Action Timeout";
        //    public const string PolishAHeadSweepMotorMoveToHomePositionActionTimeout = "Polish A Head Sweep Motor Move To Home Position Action Timeout";
        //    public const string PolishAHeadSweepMotorMoveToPolishNearPositionActionTimeout = "Polish A Head Sweep Motor Move To Polish Near Position Action Timeout";
        //    public const string PolishAHeadSweepMotorMoveToPolishFarPositionActionTimeout = "Polish A Head Sweep Motor Move To Polish Far PositionAction Timeout";
        //    public const string PolishAHeadSweepMotorMoveToDechuckPositionActionTimeout = "Polish A Head Sweep Motor Move To Dechuck Position Action Timeout";
        //    public const string PolishAHeadSweepMotorCoordinateChangeActionTimeout = "Polish A Head Sweep Motor Coordinate Change Action Timeout";
        //    public const string PolishAHeadSweepMotorDriverServoUnready = "Polish A Head Sweep Motor Driver Servo Unready";
        //    public const string PolishAHeadUpDownMotorMoveToAnyPositionActionTimeout = "Polish A Head UpDown Motor Move To Any Position Action Timeout";
        //    public const string PolishAHeadUpDownMotorStopActionTimeout = "Polish A Head UpDown Motor Stop Action Timeout";
        //    public const string PolishAHeadUpDownMotorInchingActionTimeout = "Polish A Head UpDown Motor Inching Action Timeout";
        //    public const string PolishAHeadUpDownMotorMoveToHighPositionActionTimeout = "Polish A Head UpDown Motor Move To High Position Action Timeout";
        //    public const string PolishAHeadUpDownMotorMoveToLoadCupPositionActionTimeout = "Polish A Head UpDown Motor Move To Load Cup Position Action Timeout";
        //    public const string PolishAHeadUpDownMotorMoveToPolishPositionActionTimeout = "Polish A Head UpDown Motor Move To Polish Position Action Timeout";
        //    public const string PolishAHeadUpDownMotorMoveToCleanPositionActionTimeout = "Polish A Head UpDown Motor Move To Clean Position Action Timeout";
        //    public const string PolishAHeadUpDownMotorCoordinateChangeActionTimeout = "Polish A Head UpDown Motor Coordinate Change Action Timeout";
        //    public const string PolishAHeadUpDownMotorDriverServoUnready = "Polish A Head UpDown Motor Driver Servo Unready";
        //    public const string PolishAConditionerHeadRotationMotorRunActionTimeout = "Polish A Conditioner Head Rotation Motor Run Action Timeout";
        //    public const string PolishAConditionerHeadRotationMotorStopActionTimeout = "Polish A Conditioner Head Rotation Motor Stop Action Timeout";
        //    public const string PolishAConditionerHeadRotationMotorInchingActionTimeout = "Polish A Conditioner Head Rotation Motor Inching Action Timeout";
        //    public const string PolishAConditionerHeadRotationMotorCoordinateChangeActionTimeout = "Polish A Conditioner Head Rotation Motor Coordinate Change Action Timeout";
        //    public const string PolishAConditionerHeadRotationMotorDriverServoUnready = "Polish A Conditioner Head Rotation Motor Driver Servo Unready";
        //    public const string PolishAConditionerSweepMotorRunActionTimeout = "Polish A Conditioner Sweep Motor Run Action Timeout";
        //    public const string PolishAConditionerSweepMotorStopActionTimeout = "Polish A Conditioner Sweep Motor Stop Action Timeout";
        //    public const string PolishAConditionerSweepMotorInchingActionTimeout = "Polish A Conditioner Sweep Motor Inching Action Timeout";
        //    public const string PolishAConditionerSweepMotorMoveToHomePositionActionTimeout = "Polish A Conditioner Sweep Motor Move To Home Position Action Timeout";
        //    public const string PolishAConditionerSweepMotorMoveToStartPositionActionTimeout = "Polish A Conditioner Sweep Motor Move To Start Position Action Timeout";
        //    public const string PolishAConditionerSweepMotorMoveToEndPositionActionTimeout = "Polish A Conditioner Sweep Motor Move To End Position Action Timeout";
        //    public const string PolishAConditionerSweepMotorCoordinateChangeActionTimeout = "Polish A Conditioner Sweep Motor Coordinate Change Action Timeout";
        //    public const string PolishAConditionerSweepMotorDriverServoUnready = "Polish A Conditioner Sweep Motor Driver Servo Unready";
        //    public const string PolishAChemicalArmMotorRunActionTimeout = "Polish A Chemical Arm Motor Run Action Timeout";
        //    public const string PolishAChemicalArmMotorStopActionTimeout = "Polish A Chemical Arm Motor Stop Action Timeout";
        //    public const string PolishAChemicalArmMotorInchingActionTimeout = "Polish A Chemical Arm Motor Inching Action Timeout";
        //    public const string PolishAChemicalArmMotorMoveToHomePositionActionTimeout = "Polish A Chemical Arm Motor Move To Home Position Action Timeout";
        //    public const string PolishAChemicalArmMotorMoveToSlurryPositionActionTimeout = "Polish A Chemical Arm Motor Move To Slurry Position Action Timeout";
        //    public const string PolishAChemicalArmMotorMoveToEndPositionActionTimeout = "Polish A Chemical Arm Motor Move To End Position Action Timeout";
        //    public const string PolishAChemicalArmMotorCoordinateChangeActionTimeout = "Polish A Chemical Arm Motor Coordinate Change Action Timeout";
        //    public const string PolishAChemicalArmMotorDriverServoUnready = "Polish A Chemical Arm Motor Driver Servo Unready";
        //    public const string PolishAPlatenRotationMotorVelocityUpperLimit1 = "Polish A Platen Rotation Motor Velocity Upper Limit1";
        //    public const string PolishAPlatenRotationMotorVelocityUpperLimit2 = "Polish A Platen Rotation Motor Velocity Upper Limit2";
        //    public const string PolishAPlatenRotationMotorVelocityLowerLimit1 = "Polish A Platen Rotation Motor Velocity Lower Limit1";
        //    public const string PolishAPlatenRotationMotorVelocityLowerLimit2 = "Polish A Platen Rotation Motor Velocity Lower Limit2";
        //    public const string PolishAHeadRotationMotorVelocityUpperLimit1 = "Polish A Head Rotation Motor Velocity Upper Limit1";
        //    public const string PolishAHeadRotationMotorVelocityUpperLimit2 = "Polish A Head Rotation Motor Velocity Upper Limit2";
        //    public const string PolishAHeadRotationMotorVelocityLowerLimit1 = "Polish A Head Rotation Motor Velocity Lower Limit1";
        //    public const string PolishAHeadRotationMotorVelocityLowerLimit2 = "Polish A Head Rotation Motor Velocity Lower Limit2";
        //    public const string PolishAConditionerHeadRotationMotorVelocityUpperLimit1 = "Polish A Conditioner Head Rotation Motor Velocity Upper Limit1";
        //    public const string PolishAConditionerHeadRotationMotorVelocityUpperLimit2 = "Polish A Conditioner Head Rotation Motor Velocity Upper Limit2";
        //    public const string PolishAConditionerHeadRotationMotorVelocityLowerLimit1 = "Polish A Conditioner Head Rotation Motor Velocity Lower Limit1";
        //    public const string PolishAConditionerHeadRotationMotorVelocityLowerLimit2 = "Polish A Conditioner Head Rotation Motor Velocity Lower Limit2";
        //    public const string PolishAConditionerHeadCylinderLimitSensor = "Polish A Conditioner Head Cylinder Limit Sensor";
        //    public const string PolishAPlatenBreakwaterCylinderLimitSensor = "Polish A Platen Breakwater Cylinder Limit Sensor";
        //    public const string PolishAConditionerHeadCylinderUpActionTimeOut = "Polish A Conditioner Head Cylinder Up Action TimeOut";
        //    public const string PolishAConditionerHeadCylinderDownActionTimeOut = "Polish A Conditioner Head Cylinder Down Action TimeOut";
        //    public const string PolishAPlatenBreakwaterCylinderUpActionTimeOut = "Polish A Platen Breakwater Cylinder Up Action TimeOut";
        //    public const string PolishAPlatenBreakwaterCylinderDownActionTimeOut = "Polish A Platen Breakwater Cylinder Down Action TimeOut";
        //    public const string PolishASlurry1DIWFlowrateUpperLimit1 = "Polish A Slurry1DIW Flowrate Upper Limit1";
        //    public const string PolishASlurry1DIWFlowrateUpperLimit2 = "Polish A Slurry1DIW Flowrate Upper Limit2";
        //    public const string PolishASlurry1DIWFlowrateLowerLimit1 = "Polish A Slurry1DIW Flowrate Lower Limit1";
        //    public const string PolishASlurry1DIWFlowrateLowerLimit2 = "Polish A Slurry1DIW Flowrate Lower Limit2";
        //    public const string PolishASlurry2DIWFlowrateUpperLimit1 = "Polish A Slurry2DIW Flowrate Upper Limit1";
        //    public const string PolishASlurry2DIWFlowrateUpperLimit2 = "Polish A Slurry2DIW Flowrate Upper Limit2";
        //    public const string PolishASlurry2DIWFlowrateLowerLimit1 = "Polish A Slurry2DIW Flowrate Lower Limit1";
        //    public const string PolishASlurry2DIWFlowrateLowerLimit2 = "Polish A Slurry2DIW Flowrate Lower Limit2";
        //    public const string PolishASlurry3DIWFlowrateUpperLimit1 = "Polish A Slurry3DIW Flowrate Upper Limit1";
        //    public const string PolishASlurry3DIWFlowrateUpperLimit2 = "Polish A Slurry3DIW Flowrate Upper Limit2";
        //    public const string PolishASlurry3DIWFlowrateLowerLimit1 = "Polish A Slurry3DIW Flowrate Lower Limit1";
        //    public const string PolishASlurry3DIWFlowrateLowerLimit2 = "Polish A Slurry3DIW Flowrate Lower Limit2";
        //    public const string PolishASlurry1Press1UpperLimit = "Polish A Slurry1 Press1 Upper Limit";
        //    public const string PolishASlurry1Press1LowerLimit = "Polish A Slurry1 Press1 Lower Limit";
        //    public const string PolishASlurry1Press2UpperLimit = "Polish A Slurry1 Press2 Upper Limit";
        //    public const string PolishASlurry1Press2LowerLimit = "Polish A Slurry1 Press2 Lower Limit";
        //    public const string PolishASlurry2Press1UpperLimit = "Polish A Slurry2 Press1 Upper Limit";
        //    public const string PolishASlurry2Press1LowerLimit = "Polish A Slurry2 Press1 Lower Limit";
        //    public const string PolishASlurry2Press2UpperLimit = "Polish A Slurry2 Press2 Upper Limit";
        //    public const string PolishASlurry2Press2LowerLimit = "Polish A Slurry2 Press2 Lower Limit";
        //    public const string PolishASlurry3Press1UpperLimit = "Polish A Slurry3 Press1 Upper Limit";
        //    public const string PolishASlurry3Press1LowerLimit = "Polish A Slurry3 Press1 Lower Limit";
        //    public const string PolishASlurry3Press2UpperLimit = "Polish A Slurry3 Press2 Upper Limit";
        //    public const string PolishASlurry3Press2LowerLimit = "Polish A Slurry3 Press2 Lower Limit";
        //    public const string PolishADIWFlowrateUpperLimit = "Polish A DIW Flowrate Upper Limit";
        //    public const string PolishADIWFlowrateLowerLimit = "Polish A DIW Flowrate Lower Limit";
        //    public const string PolishAPlatenWashTubePressUpperLimit = "Polish A Platen Wash Tube Press Upper Limit";
        //    public const string PolishAPlatenWashTubePressLowerLimit = "Polish A Platen Wash Tube Press Lower Limit";
        //    public const string PolishAPlatenWashTubeFlowrateUpperLimit = "Polish A Platen Wash Tube Flowrate Upper Limit";
        //    public const string PolishAPlatenWashTubeFlowrateLowerLimit = "Polish A Platen Wash Tube Flowrate Lower Limit";
        //    public const string PolishAMovingHeadDIWWashTubeFlowrateUpperLimit = "Polish A Moving Head DIW Wash Tube Flowrate Upper Limit";
        //    public const string PolishAMovingHeadDIWWashTubeFlowrateLowerLimit = "Polish A Moving Head DIW Wash Tube Flowrate Lower Limit";
        //    public const string PolishAMovingHeadExternalDIWWashTubeFlowrateUpperLimit = "Polish A Moving Head External DIW Wash Tube Flowrate Upper Limit";
        //    public const string PolishAMovingHeadExternalDIWWashTubeFlowrateLowerLimit = "Polish A Moving Head External DIW Wash Tube Flowrate Lower Limit";
        //    public const string PolishAConditionerDIWWashTubeFlowrateUpperLimit = "Polish A Conditioner DIW Wash Tube Flowrate Upper Limit";
        //    public const string PolishAConditionerDIWWashTubeFlowrateLowerLimit = "Polish A Conditioner DIW Wash Tube Flowrate Lower Limit";
        //    public const string PolishAConditionerArmDIWWashTubeFlowrateUpperLimit = "Polish A Conditioner Arm DIW Wash Tube Flowrate Upper Limit";
        //    public const string PolishAConditionerArmDIWWashTubeFlowrateLowerLimit = "Polish A Conditioner Arm DIW Wash Tube Flowrate Lower Limit";
        //    public const string PolishACheimcalArmDIWWashTubeFlowrateUpperLimit = "Polish A Cheimcal Arm DIW Wash Tube Flowrate Upper Limit";
        //    public const string PolishACheimcalArmDIWWashTubeFlowrateLowerLimit = "Polish A Cheimcal Arm DIW Wash Tube Flowrate Lower Limit";
        //    public const string PolishAHeadMiddleWashTubeFlowrateUpperLimit = "Polish A Head Middle Wash Tube Flowrate Upper Limit";
        //    public const string PolishAHeadMiddleWashTubeFlowrateLowerLimit = "Polish A Head Middle Wash Tube Flowrate Lower Limit";
        //    public const string PolishAHeadExternalWashTubeFlowrateUpperLimit = "Polish A Head External Wash Tube Flowrate Upper Limit";
        //    public const string PolishAHeadExternalWashTubeFlowrateLowerLimit = "Polish A Head External Wash Tube Flowrate Lower Limit";
        //    public const string PolishAWFUpperFaceWashTubeFlowrateUpperLimit = "Polish A WF Upper Face Wash Tube Flowrate Upper Limit";
        //    public const string PolishAWFUpperFaceWashTubeFlowrateLowerLimit = "Polish A WF Upper Face Wash Tube Flowrate Lower Limit";
        //    public const string PolishAWFLowerFaceWashTubeFlowrateUpperLimit = "Polish A WF Lower Face Wash Tube Flowrate Upper Limit";
        //    public const string PolishAWFLowerFaceWashTubeFlowrateLowerLimit = "Polish A WF Lower Face Wash Tube Flowrate Lower Limit";
        //    public const string PolishAPlatenCoolWaterFlowrateUpperLimit = "Polish A Platen Cool Water Flowrate Upper Limit";
        //    public const string PolishAPlatenCoolWaterFlowrateLowerLimit = "Polish A Platen Cool Water Flowrate Lower Limit";
        //    public const string PolishAPlatenMotorCoolWaterFlowrateUpperLimit = "Polish A Platen Motor Cool Water Flowrate Upper Limit";
        //    public const string PolishAPlatenMotorCoolWaterFlowrateLowerLimit = "Polish A Platen Motor Cool Water Flowrate Lower Limit";
        //    public const string PolishAZone1PressureUpperLimit1 = "Polish A Zone1 Pressure Upper Limit1";
        //    public const string PolishAZone1PressureUpperLimit2 = "Polish A Zone1 Pressure Upper Limit2";
        //    public const string PolishAZone1PressureLowerLimit1 = "Polish A Zone1 Pressure Lower Limit1";
        //    public const string PolishAZone1PressureLowerLimit2 = "Polish A Zone1 Pressure Lower Limit2";
        //    public const string PolishAZone2PressureUpperLimit1 = "Polish A Zone2 Pressure Upper Limit1";
        //    public const string PolishAZone2PressureUpperLimit2 = "Polish A Zone2 Pressure Upper Limit2";
        //    public const string PolishAZone2PressureLowerLimit1 = "Polish A Zone2 Pressure Lower Limit1";
        //    public const string PolishAZone2PressureLowerLimit2 = "Polish A Zone2 Pressure Lower Limit2";
        //    public const string PolishAZone3PressureUpperLimit1 = "Polish A Zone3 Pressure Upper Limit1";
        //    public const string PolishAZone3PressureUpperLimit2 = "Polish A Zone3 Pressure Upper Limit2";
        //    public const string PolishAZone3PressureLowerLimit1 = "Polish A Zone3 Pressure Lower Limit1";
        //    public const string PolishAZone3PressureLowerLimit2 = "Polish A Zone3 Pressure Lower Limit2";
        //    public const string PolishAZone4PressureUpperLimit1 = "Polish A Zone4 Pressure Upper Limit1";
        //    public const string PolishAZone4PressureUpperLimit2 = "Polish A Zone4 Pressure Upper Limit2";
        //    public const string PolishAZone4PressureLowerLimit1 = "Polish A Zone4 Pressure Lower Limit1";
        //    public const string PolishAZone4PressureLowerLimit2 = "Polish A Zone4 Pressure Lower Limit2";
        //    public const string PolishAZone5PressureUpperLimit1 = "Polish A Zone5 Pressure Upper Limit1";
        //    public const string PolishAZone5PressureUpperLimit2 = "Polish A Zone5 Pressure Upper Limit2";
        //    public const string PolishAZone5PressureLowerLimit1 = "Polish A Zone5 Pressure Lower Limit1";
        //    public const string PolishAZone5PressureLowerLimit2 = "Polish A Zone5 Pressure Lower Limit2";
        //    public const string PolishAZone6PressureUpperLimit1 = "Polish A Zone6 Pressure Upper Limit1";
        //    public const string PolishAZone6PressureUpperLimit2 = "Polish A Zone6 Pressure Upper Limit2";
        //    public const string PolishAZone6PressureLowerLimit1 = "Polish A Zone6 Pressure Lower Limit1";
        //    public const string PolishAZone6PressureLowerLimit2 = "Polish A Zone6 Pressure Lower Limit2";
        //    public const string PolishAZone7PressureUpperLimit1 = "Polish A Zone7 Pressure Upper Limit1";
        //    public const string PolishAZone7PressureUpperLimit2 = "Polish A Zone7 Pressure Upper Limit2";
        //    public const string PolishAZone7PressureLowerLimit1 = "Polish A Zone7 Pressure Lower Limit1";
        //    public const string PolishAZone7PressureLowerLimit2 = "Polish A Zone7 Pressure Lower Limit2";
        //    public const string PolishAZone8PressureUpperLimit1 = "Polish A Zone8 Pressure Upper Limit1";
        //    public const string PolishAZone8PressureUpperLimit2 = "Polish A Zone8 Pressure Upper Limit2";
        //    public const string PolishAZone8PressureLowerLimit1 = "Polish A Zone8 Pressure Lower Limit1";
        //    public const string PolishAZone8PressureLowerLimit2 = "Polish A Zone8 Pressure Lower Limit2";
        //    public const string PolishATopRingPressureUpperLimit1 = "Polish A Top Ring Pressure Upper Limit1";
        //    public const string PolishATopRingPressureUpperLimit2 = "Polish A Top Ring Pressure Upper Limit2";
        //    public const string PolishATopRingPressureLowerLimit1 = "Polish A Top Ring Pressure Lower Limit1";
        //    public const string PolishATopRingPressureLowerLimit2 = "Polish A Top Ring Pressure Lower Limit2";
        //    public const string PolishAConditionHeadPressureUpperLimit1 = "Polish A Condition Head Pressure Upper Limit1";
        //    public const string PolishAConditionHeadPressureUpperLimit2 = "Polish A Condition Head Pressure Upper Limit2";
        //    public const string PolishAConditionHeadPressureLowerLimit1 = "Polish A Condition Head Pressure Lower Limit1";
        //    public const string PolishAConditionHeadPressureLowerLimit2 = "Polish A Condition Head Pressure Lower Limit2";
        //    public const string PolishAZone1FlowrateUpperLimit = "Polish A Zone1 Flowrate Upper Limit";
        //    public const string PolishAZone1FlowrateLowerLimit = "Polish A Zone1 Flowrate Lower Limit";
        //    public const string PolishAZone2FlowrateUpperLimit = "Polish A Zone2 Flowrate Upper Limit";
        //    public const string PolishAZone2FlowrateLowerLimit = "Polish A Zone2 Flowrate Lower Limit";
        //    public const string PolishAZone3FlowrateUpperLimit = "Polish A Zone3 Flowrate Upper Limit";
        //    public const string PolishAZone3FlowrateLowerLimit = "Polish A Zone3 Flowrate Lower Limit";
        //    public const string PolishAZone4FlowrateUpperLimit = "Polish A Zone4 Flowrate Upper Limit";
        //    public const string PolishAZone4FlowrateLowerLimit = "Polish A Zone4 Flowrate Lower Limit";
        //    public const string PolishAZone5FlowrateUpperLimit = "Polish A Zone5 Flowrate Upper Limit";
        //    public const string PolishAZone5FlowrateLowerLimit = "Polish A Zone5 Flowrate Lower Limit";
        //    public const string PolishAZone6FlowrateUpperLimit = "Polish A Zone6 Flowrate Upper Limit";
        //    public const string PolishAZone6FlowrateLowerLimit = "Polish A Zone6 Flowrate Lower Limit";
        //    public const string PolishAZone7FlowrateUpperLimit = "Polish A Zone7 Flowrate Upper Limit";
        //    public const string PolishAZone7FlowrateLowerLimit = "Polish A Zone7 Flowrate Lower Limit";
        //    public const string PolishAZone8FlowrateUpperLimit = "Polish A Zone8 Flowrate Upper Limit";
        //    public const string PolishAZone8FlowrateLowerLimit = "Polish A Zone8 Flowrate Lower Limit";
        //    public const string PolishAZone4N2PressUpperLimit = "Polish A Zone 4 N2 Press Upper Limit";
        //    public const string PolishAZone4N2PressLowerLimit = "Polish A Zone 4 N2 Press Lower Limit";
        //    public const string PolishATopRingFlowrateUpperLimit = "Polish A Top Ring Flowrate Upper Limit";
        //    public const string PolishATopRingFlowrateLowerLimit = "Polish A Top Ring Flowrate Lower Limit";
        //    public const string PolishAVacuumPressUpperLimit = "Polish A Vacuum Press Upper Limit";
        //    public const string PolishAVacuumPressLowerLimit = "Polish A Vacuum Press Lower Limit";
        //    public const string PolishAHeadBlowN2FlowrateUpperLimit = "Polish A Head Blow N2 Flowrate Upper Limit";
        //    public const string PolishAHeadBlowN2FlowrateLowerLimit = "Polish A Head Blow N2 Flowrate Lower Limit";
        //    public const string PolishAConditionerHeadUpDownPositionUpperLimit = "Polish A Conditioner Head UpDown Position Upper Limit";
        //    public const string PolishAConditionerHeadUpDownPositionLowerLimit = "Polish A Conditioner Head UpDown Position Lower Limit";
        //    public const string PolishAWFOut = "Polish A WF Out ";
        //    public const string PolishAWaterSeparatorLiquidLevelExceed = "Polish A Water Separator Liquid Level Exceed";
        //    public const string PolishAHeadLifeTimeUpperLimit1 = "Polish A Head Life Time Upper Limit1";
        //    public const string PolishAHeadLifeTimeUpperLimit2 = "Polish A Head Life Time Upper Limit2";
        //    public const string PolishAMembraneLifeTimeUpperLimit1 = "Polish A Membrane Life Time Upper Limit1";
        //    public const string PolishAMembraneLifeTimeUpperLimit2 = "Polish A Membrane Life Time Upper Limit2";
        //    public const string PolishARetainerRingLifeTimeUpperLimit1 = "Polish A Retainer Ring Life Time Upper Limit1";
        //    public const string PolishARetainerRingLifeTimeUpperLimit2 = "Polish A Retainer Ring Life Time Upper Limit2";
        //    public const string PolishAPadLifeTimeUpperLimit1 = "Polish A Pad Life Time Upper Limit1";
        //    public const string PolishAPadLifeTimeUpperLimit2 = "Polish A Pad Life Time Upper Limit2";
        //    public const string PolishADiskLifeTimeUpperLimit1 = "Polish A Disk Life Time Upper Limit1";
        //    public const string PolishADiskLifeTimeUpperLimit2 = "Polish A Disk Life Time Upper Limit2";
        //    public const string PolishAConditionerSpringUpperLimit1 = "Polish A Conditioner Spring Upper Limit1";
        //    public const string PolishAConditionerSpringUpperLimit2 = "Polish A Conditioner Spring Upper Limit2";
        //    public const string PolishAHeadWashLaterWaitRobotTimeOutLimit = "Polish A Head Wash Later Wait Robot TimeOut Limit";
        //    public const string PolishAZone4SuctionPressure = "Polish A Zone 4 Suction Pressure";
        //}
    }

    public class PolishAlarmManage : AlarmManage
    {
        //stPolishAlarm polishAlarm;
        public PolishAlarmManage(string name, KVLink link) : base(name, link)
        {

            //fields = typeof(stPolishAlarm).GetFields();
        }

        public override List<Alarm> CollectAlarm()
        {
            List<Alarm> listAlarm = new List<Alarm>();
            byte[] _readBuf = new byte[256];
            err = KHT.KHLReadVariable(kvlink.sock, "alarm", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTPolishAlarm.stPolishAlarm polishAlarm = new KHSTPolishAlarm.stPolishAlarm();
                KHSTPolishAlarm.ByteTo_stPolishAlarm(ref polishAlarm, _readBuf);
                object[] allValues = CompiledFieldAccessor<KHSTPolishAlarm.stPolishAlarm>.GetAllFieldValues(polishAlarm);
                var fields = typeof(KHSTPolishAlarm.stPolishAlarm).GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < allValues.Length; i++)
                {
                    if (Convert.ToBoolean(allValues[i]))
                    {
                        string propertyName = this.alarmName + fields[i].Name;
                        Alarm alarmTemp = dicAlarm[propertyName];
                        if (CanRaiseAlarm(alarmTemp))
                        {
                            alarmTemp.AlarmTime = DateTime.Now;
                            alarmTemp.DefaultOperation = "None";
                            listAlarm.Add(dicAlarm[propertyName]);
                            alarmTemp.IsAlarmed = true;
                        }
                    }
                }

            }
            return listAlarm;
        }
    }

    public class RollerAlarmManage : AlarmManage
    {
        //stPolishAlarm polishAlarm;
        public RollerAlarmManage(string name, KVLink link) : base(name, link)
        {
            //fields = typeof(stRollerAlarm).GetFields();
        }

        public override List<Alarm> CollectAlarm()
        {
            List<Alarm> listAlarm = new List<Alarm>();
            byte[] _readBuf = new byte[256];
            err = KHT.KHLReadVariable(kvlink.sock, "alarm", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                stRollerAlarm rollerAlarm = new stRollerAlarm();
                KHST.ByteTo_stRollerAlarm(ref rollerAlarm, _readBuf);
                object[] allValues = CompiledFieldAccessor<stRollerAlarm>.GetAllFieldValues(rollerAlarm);
                var fields = typeof(stRollerAlarm).GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < allValues.Length; i++)
                {
                    if (Convert.ToBoolean(allValues[i]))
                    {
                        string propertyName = this.alarmName + fields[i].Name;
                        Alarm alarmTemp = dicAlarm[propertyName];
                        if (CanRaiseAlarm(alarmTemp))
                        {
                            alarmTemp.AlarmTime = DateTime.Now;
                            alarmTemp.DefaultOperation = "None";
                            listAlarm.Add(dicAlarm[propertyName]);
                            alarmTemp.IsAlarmed = true;
                        }
                    }
                }
            }
            return listAlarm;
        }
    }

    public class PenAlarmManage : AlarmManage
    {
        //stPolishAlarm polishAlarm;
        public PenAlarmManage(string name, KVLink link) : base(name, link)
        {
            //fields = typeof(stPenAlarm).GetFields();
        }

        public override List<Alarm> CollectAlarm()
        {
            List<Alarm> listAlarm = new List<Alarm>();
            byte[] _readBuf = new byte[256];
            err = KHT.KHLReadVariable(kvlink.sock, "alarm", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                stPenAlarm penAlarm = new stPenAlarm();
                KHST.ByteTo_stPenAlarm(ref penAlarm, _readBuf);
                object[] allValues = CompiledFieldAccessor<stPenAlarm>.GetAllFieldValues(penAlarm);
                var fields = typeof(stPenAlarm).GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < allValues.Length; i++)
                {
                    if (Convert.ToBoolean(allValues[i]))
                    {
                        string propertyName = this.alarmName + fields[i].Name;
                        Alarm alarmTemp = dicAlarm[propertyName];
                        if (CanRaiseAlarm(alarmTemp))
                        {
                            alarmTemp.AlarmTime = DateTime.Now;
                            alarmTemp.DefaultOperation = "None";
                            listAlarm.Add(dicAlarm[propertyName]);
                            alarmTemp.IsAlarmed = true;
                        }
                    }
                }
            }
            return listAlarm;
        }
    }

    public class DryAlarmManage : AlarmManage
    {
        //stPolishAlarm polishAlarm;
        public DryAlarmManage(string name, KVLink link) : base(name, link)
        {
            //fields = typeof(stDryAlarm).GetFields();
        }

        public override List<Alarm> CollectAlarm()
        {
            List<Alarm> listAlarm = new List<Alarm>();
            byte[] _readBuf = new byte[256];
            err = KHT.KHLReadVariable(kvlink.sock, "alarm", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                stDryAlarm dryAlarm = new stDryAlarm();
                KHST.ByteTo_stDryAlarm(ref dryAlarm, _readBuf);
                object[] allValues = CompiledFieldAccessor<stDryAlarm>.GetAllFieldValues(dryAlarm);
                var fields = typeof(stDryAlarm).GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < allValues.Length; i++)
                {
                    if (Convert.ToBoolean(allValues[i]))
                    {
                        string propertyName = this.alarmName + fields[i].Name;
                        Alarm alarmTemp = dicAlarm[propertyName];
                        if (CanRaiseAlarm(alarmTemp))
                        {
                            alarmTemp.AlarmTime = DateTime.Now;
                            alarmTemp.DefaultOperation = "None";
                            listAlarm.Add(dicAlarm[propertyName]);
                            alarmTemp.IsAlarmed = true;
                        }
                    }
                }
            }
            return listAlarm;
        }
    }
    public class RobotAlarmManage : AlarmManage
    {
        public RobotAlarmManage(string name, KVLink link) : base(name, link)
        {

        }

        public override List<Alarm> CollectAlarm()
        {
            List<Alarm> listAlarm = new List<Alarm>();
            try
            {
                // D5600-D5677 共78个D寄存器，每个寄存器包含16个字（words）
                for (int i = 0; i < 78; i++) // 78个D寄存器
                {
                    string address = $"D{5600 + i}";
                    // 读取1个字（words），每个word为2个字节，总共32字节
                    byte[] result = GlobalService.plcComm.ReadBytes(address, 2);
                    if (result != null)
                    {
                        // 将两个字节组合成一个16位的值
                        ushort registerValue = (ushort)((result[1] << 8) | result[0]);
                        for (int wordIndex = 0; wordIndex < 16; wordIndex++) // 每个寄存器16bit
                        {

                            // 计算实际的报警地址
                            string propertyName = $"D{5600 + i}.{wordIndex}";

                            // 检查该字是否非零（报警触发）
                            if ((registerValue & (1 << wordIndex)) != 0)
                            {
                                if (dicAlarm.TryGetValue(propertyName, out Alarm alarmTemp))
                                {
                                    if (CanRaiseAlarm(alarmTemp))
                                    {
                                        //alarmTemp.Type = "Alarm";
                                        alarmTemp.AlarmId = Convert.ToInt32(alarmTemp.AlarmName.substring(1, alarmTemp.AlarmName.length()));
                                        alarmTemp.DefaultOperation = "None";
                                        alarmTemp.AlarmTime = DateTime.Now;
                                        listAlarm.Add(alarmTemp);
                                        alarmTemp.IsAlarmed = true;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                // 记录异常日志
                Logger.Error($"读取机器人报警信息时发生错误: {ex.Message}");
            }
            return listAlarm;
        }

    }

}
