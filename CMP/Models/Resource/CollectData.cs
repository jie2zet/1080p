using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using KHT = CMP.Models.kvsPlc.KvHostLink;
using KHST = CMP.Models.kvsPlc.Polish.KvStruct;
using KHSTRollerCollectData = CMP.Models.kvsPlc.Roll.KvsStructRollerCollectData;
using KHSTRollerIOData = CMP.Models.kvsPlc.Roll.KvsStructRollerIOData;
using KHSTPenCollectData = CMP.Models.kvsPlc.Pen.KvsStructPenCollectData;
using KHSTPenIOData = CMP.Models.kvsPlc.Pen.KvsStructPenIOData;
using KHSTCLEAN = CMP.Models.kvsPlc.Clean.KvStruct;
using KHSTSV = CMP.Models.kvsPlc.SV.KvStruct;
using KHSTIO = CMP.Models.kvsPlc.IO.KvStruct;
using CMP.Models.Operation.RoboticArm;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Wordprocessing;
using static CMP.Models.Operation.Roller.RollViewPoco;
using static CMP.Models.Recipe.RecipeEdit.Recipe;
using System.Drawing;
using System.Security.Policy;
using static CMP.Models.kvsPlc.Polish.KvStruct;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO.Ports;
using System.Runtime.Serialization;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Reflection;
using CMP.Services.Reflection;
using static CMP.Models.kvsPlc.IO.KvStruct;
using static CMP.Models.kvsPlc.SV.KvStruct;

namespace CMP.Models.Resource
{
    public abstract class CollectDataChamber : BindableBase
    {
        string name;
        public readonly Dictionary<string, SV> ListSV;
        public KVLink kvLink;
        public byte[] readBuf;
        public int err;

        public CollectDataChamber(string name, KVLink link)
        {
            this.name = name;
            this.kvLink = link;
            ListSV = new Dictionary<string, SV>();
            readBuf = new byte[2048];
            //_stCollectData = new KHST.stCollectData();
        }

        public void SetSv(string key, object? value)
        {
            if (ListSV.TryGetValue(key, out var sv))
            {
                sv.Value = value;
            }
            else
            {
                ListSV[key] = new SV { Name = key, Value = value };
            }
        }

        public SV? GetSv(string key)
        {
            return ListSV.GetValueOrDefault(key);
        }

        public abstract void CollectData();
    }

    public class Polish : CollectDataChamber
    {
        const uint MotorStatusAddress = 530;
        const uint MotorCount = 7;
        public string polishName = string.Empty;

        KHST.stCollectData _stCollectData;
        KHST.stIOData _stIOData;
        public FieldInfo[] collectDataFields;
        public FieldInfo[] ioDataFields;

        public Polish(string name, KVLink link) : base(name, link)
        {
            _stCollectData = new KHST.stCollectData();
            _stIOData = new KHST.stIOData();
            collectDataFields = typeof(KHSTSV.stPolishCollectData).GetFields();
            ioDataFields = typeof(KHSTIO.stPolishIOData).GetFields();
            polishName = name;
        }

        public override void CollectData()
        {
            //KHST.stCollectData stCollectData = kvsPlcComm.ReadVariable(IP);
            CollectProcessData1();
            CollectIOData1();
            CollectMotorServoStatus();
        }

        private void CollectProcessData1()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTSV.stPolishCollectData _stPolishCollectData = new KHSTSV.stPolishCollectData();
                KHSTSV.ByteTo_stPolishCollectData(ref _stPolishCollectData, _readBuf);
                if (_stPolishCollectData != null)
                {
                    CompiledFieldAccessor<stPolishCollectData>.ProcessAllFields(_stPolishCollectData, (name, value) =>
                    {
                        SetSv(this.polishName + name, value);
                    });
                }
            }
        }

        private void CollectIOData1()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "structIOData", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTIO.stPolishIOData _stPolishIOData = new KHSTIO.stPolishIOData();
                KHSTIO.ByteTo_stPolishIOData(ref _stPolishIOData, _readBuf);
                if (_stPolishIOData != null)
                {
                    CompiledFieldAccessor<stPolishIOData>.ProcessAllFields(_stPolishIOData, (name, value) =>
                    {
                        SetSv(this.polishName + name, value);
                    });
                }
            }
        }

        private void CollectMotorServoStatus()
        {
            bool[] motorServoStatus = new bool[MotorCount];
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, MotorStatusAddress, 0, MotorCount, readBuf);
            if(err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                SetSv(Keys.PlatenRotationMotorServo, motorServoStatus[0]);
                SetSv(Keys.HeadRotationMotorServo, motorServoStatus[1]);
                SetSv(Keys.HeadSweepMotorServo, motorServoStatus[2]);
                SetSv(Keys.HeadUpDownMotorServo, motorServoStatus[3]);
                SetSv(Keys.ConditionHeadRotationMotorServo, motorServoStatus[4]);
                SetSv(Keys.ConditionSweepMotorServo, motorServoStatus[5]);
                SetSv(Keys.SlurrySweepMotorServo, motorServoStatus[6]);
            }
        }
        
        public static class Keys
        {
            public const string ProcessStepAct = "Process Step Act";
            public const string ProcessStepSet = "Process Step Set";
            public const string ProcessTypeAct = "Process Type Act";
            public const string ProcessTypeSet = "Process Type Set";
            public const string RecipeTimeAct = "Recipe Time Act";
            public const string RecipeTimeSet = "Recipe Time Set";
            public const string ExsituConditionProcessStepAct = "ExsituCondition Process Step Act";
            public const string ExsituConditionProcessStepSet = "ExsituCondition Process Step Set";
            public const string ExsituConditionRecipeTimeAct = "ExsituCondition Recipe Time Act";
            public const string ExsituConditionRecipeTimeSet = "ExsituCondition Recipe Time Set";
            public const string MainPolishRecipeTimeAct = "Main Polish Recipe Time Act";
            public const string MainPolishRecipeTimeSet = "Main Polish Recipe Time Set";
            public const string MainPolishCumulativeTimeAct = "Main Polish Cumulative Time Act";
            public const string MainPolishCumulativeTimeSet = "Main Polish Cumulative Time Set";
            public const string JumpStepNoAct = "Jump Step No Act";
            public const string JumpStepNoSet = "Jump Step No Set";
            public const string StepControlAct = "Step Control Act";
            public const string StepControlSet = "Step Control Set";
            public const string ZoneMinPressureAct = "Zone Min Pressure Act";
            public const string ZoneMinPressureSet = "Zone Min Pressure Set";
            public const string ZoneMaxPressureAct = "Zone Max Pressure Act";
            public const string ZoneMaxPressureSet = "Zone Max Pressure Set";
            public const string MaxTimeAct = "Max Time Act";
            public const string MaxTimeSet = "Max Time Set";
            public const string HeadSweepRecipeAct = "Head Sweep Recipe Act";
            public const string HeadSweepRecipeSet = "Head Sweep Recipe Set";
            public const string ConditionRecipeAct = "Condition Recipe Act";
            public const string ConditionRecipeSet = "Condition Recipe Set";
            public const string ConditionEnableAct = "Condition Enable Act";
            public const string ConditionEnableSet = "Condition Enable Set";
            public const string HeadSweepTypeAct = "Head Sweep Type Act";
            public const string HeadSweepTypeSet = "Head Sweep Type Set";
            public const string ZoneCountSetValueAct = "Zone Count Set Value Act";
            public const string ZoneCountSetValueSet = "Zone Count Set Value Set";
            public const string SweepStopTimeTypeSetValueAct = "Sweep Stop Time Type Set Value Act";
            public const string SweepStopTimeTypeSetValueSet = "Sweep Stop Time Type Set Value Set";
            public const string RatioSetValueAct = "Ratio Set Value Act";
            public const string RatioSetValueSet = "Ratio Set Value Set";
            public const string ConditionSweepTypeSetValueAct = "Condition Sweep Type Set Value Act";
            public const string ConditionSweepTypeSetValueSet = "Condition Sweep Type Set Value Set";
            public const string PlatenRotationMotorVelocityAct = "Platen Rotation Motor Velocity Act";
            public const string PlatenRotationMotorVelocitySet = "Platen Rotation Motor Velocity Set";
            public const string HeadRotationMotorVelocityAct = "Head Rotation Motor Velocity Act";
            public const string HeadRotationMotorVelocitySet = "Head Rotation Motor Velocity Set";
            public const string HeadSweepMotorVelocityAct = "Head Sweep Motor Velocity Act";
            public const string HeadSweepMotorVelocitySet = "Head Sweep Motor Velocity Set";
            public const string HeadSweepMotorPositionAct = "Head Sweep Motor Position Act";
            public const string HeadSweepMotorPositionSet = "Head Sweep Motor Position Set";
            public const string HeadSweepCycleSetValueAct = "Head Sweep Cycle Set Value Act";
            public const string HeadSweepCycleSetValueSet = "Head Sweep Cycle Set Value Set";
            public const string HeadUpDownMotorVelocityAct = "Head UpDown Motor Velocity Act";
            public const string HeadUpDownMotorVelocitySet = "Head UpDown Motor Velocity Set";
            public const string HeadUpDownMotorPositionAct = "Head UpDown Motor Position Act";
            public const string HeadUpDownMotorPositionSet = "Head UpDown Motor Position Set";
            public const string ConditionRotationVelocityAct = "Condition Rotation Velocity Act";
            public const string ConditionRotationVelocitySet = "Condition Rotation Velocity Set";
            public const string ConditionSweepMotorVelocityAct = "Condition Sweep Motor Velocity Act";
            public const string ConditionSweepMotorVelocitySet = "Condition Sweep Motor Velocity Set";
            public const string ConditionSweepMotorPositionAct = "Condition Sweep Motor Position Act";
            public const string ConditionSweepMotorPositionSet = "Condition Sweep Motor Position Set";
            public const string ConditionSweepCycleSetValueAct = "Condition Sweep Cycle Set Value Act";
            public const string ConditionSweepCycleSetValueSet = "Condition Sweep Cycle Set Value Set";
            public const string SlurryLine1Type = "Slurry Line 1 Type";
            public const string SlurryLine2Type = "Slurry Line 2 Type";
            public const string SlurryLine3Type = "Slurry Line 3 Type";
            public const string Slurry1DIWFlowrateAct = "Slurry1DIW Flowrate Act";
            public const string Slurry1DIWFlowrateSet = "Slurry1DIW Flowrate Set";
            public const string Slurry2DIWFlowrateAct = "Slurry1DIW Flowrate Act";
            public const string Slurry2DIWFlowrateSet = "Slurry1DIW Flowrate Set";
            public const string Slurry3DIWFlowrateAct = "Slurry2DIW Flowrate Act";
            public const string Slurry3DIWFlowrateSet = "Slurry2DIW Flowrate Set";
            public const string Zone1Type = "Zone 1 Type";
            public const string Zone2Type = "Zone 2 Type";
            public const string Zone3Type = "Zone 3 Type";
            public const string Zone4Type = "Zone 4 Type";
            public const string Zone5Type = "Zone 5 Type";
            public const string Zone6Type = "Zone 6 Type";
            public const string Zone7Type = "Zone 7 Type";
            public const string Zone8Type = "Zone 8 Type";
            public const string TopRingType = "Top Ring Type";
            public const string Zone1PressureAct = "Zone 1 Pressure Act";
            public const string Zone1PressureSet = "Zone 1 Pressure Set";
            public const string Zone2PressureAct = "Zone 2 Pressure Act";
            public const string Zone2PressureSet = "Zone 2 Pressure Set";
            public const string Zone3PressureAct = "Zone 3 Pressure Act";
            public const string Zone3PressureSet = "Zone 3 Pressure Set";
            public const string Zone4PressureAct = "Zone 4 Pressure Act";
            public const string Zone4PressureSet = "Zone 4 Pressure Set";
            public const string Zone5PressureAct = "Zone 5 Pressure Act";
            public const string Zone5PressureSet = "Zone 5 Pressure Set";
            public const string Zone6PressureAct = "Zone 6 Pressure Act";
            public const string Zone6PressureSet = "Zone 6 Pressure Set";
            public const string Zone7PressureAct = "Zone 7 Pressure Act";
            public const string Zone7PressureSet = "Zone 7 Pressure Set";
            public const string Zone8PressureAct = "Zone 8 Pressure Act";
            public const string Zone8PressureSet = "Zone 8 Pressure Set";
            public const string TopRingType1Act = "Top Ring Type1 Act";
            public const string TopRingType1Set = "Top Ring Type1 Set";
            public const string ConditionHeadPressureAct = "Condition Head Pressure Act";
            public const string ConditionHeadPressureSet = "Condition Head Pressure Set";
            public const string DIWAct = "DIW Act";
            public const string DIWSet = "DIW Set";
            public const string HeadMiddleWashAct = "Head Middle Wash Act";
            public const string HeadMiddleWashSet = "Head Middle Wash Set";
            public const string PlantWashAct = "Plant Wash Act";
            public const string PlantWashSet = "Plant Wash Set";
            public const string ChemicalArmMotorVelocityAct = "Chemical Arm Motor Velocity Act";
            public const string ChemicalArmMotorVelocitySet = "Chemical Arm Motor Velocity Set";
            public const string ChemicalArmMotorPositionAct = "Chemical Arm Motor Position Act";
            public const string ChemicalArmMotorPositionSet = "Chemical Arm Motor Position Set";
            public const string PlatenRotationMotorAngle = "Platen Rotation Motor Angle";
            public const string HeadRotationMotorAngle = "Head Rotation Motor Angle";
            public const string ConditionRotationMotorAngle = "Condition Rotation Motor Angle";
            public const string Slurry1Press1 = "Slurry1 Press1";
            public const string Slurry1Press2 = "Slurry1 Press2";
            public const string Slurry2Press1 = "Slurry2 Press1";
            public const string Slurry2Press2 = "Slurry2 Press2";
            public const string Slurry3Press1 = "Slurry3 Press1";
            public const string Slurry3Press2 = "Slurry3 Press2";
            public const string DIWFlowrate = "DIW Flowrate";
            public const string PlatenWashTubePress = "Platen Wash Tube Press";
            public const string PlatenWashTubeFlowrate = "Platen Wash Tube Flowrate";
            public const string MovingHeadDIWWashTubeFlowrate = "Moving Head DIW Wash Tube Flowrate";
            public const string MovingHeadExternalDIWTubeFlowrate = "Moving Head External DIW Tube Flowrate";
            public const string ConditionerDIWWashTubeFlowrate = "Conditioner DIW Wash Tube Flowrate";
            public const string ConditionerArmDIWWashTubeFlowrate = "Conditioner Arm DIW Wash Tube Flowrate";
            public const string CheimcalArmDIWWashTubeFlowrate = "Cheimcal Arm DIW Wash Tube Flowrate";
            public const string HeadMiddleWashTubeFlowrate = "Head Middle Wash Tube Flowrate";
            public const string HeadExternalWashTubeFlowrate = "Head External Wash Tube Flowrate";
            public const string WFUpperFaceWashTubeFlowrate = "WF Upper Face Wash Tube Flowrate";
            public const string WFLowerFaceWashTubeFlowrate = "WF Lower Face Wash Tube Flowrate";
            public const string PlatenCoolWaterFlowrate = "Platen Cool Water Flowrate";
            public const string PlatenMotorCoolWaterFlowrate = "Platen Motor Cool Water Flowrate";
            public const string Zone1Flowrate = "Zone 1 Flowrate";
            public const string Zone2Flowrate = "Zone 2 Flowrate";
            public const string Zone3Flowrate = "Zone 3 Flowrate";
            public const string Zone4Flowrate = "Zone 4 Flowrate";
            public const string Zone5Flowrate = "Zone 5 Flowrate";
            public const string Zone6Flowrate = "Zone 6 Flowrate";
            public const string Zone7Flowrate = "Zone 7 Flowrate";
            public const string Zone8Flowrate = "Zone 8 Flowrate";
            public const string Zone4N2Press = "Zone 4 N2 Press";
            public const string TopRingFlowrate = "Top Ring Flowrate";
            public const string VacuumPress = "Vacuum Press";
            public const string HeadBlowN2Flowrate = "Head Blow N2 Flowrate";
            public const string ConditionerHeadDownwardPressPositionDetection = "Conditioner Head Downward Press Position Detection";
            public const string WaferThickness = "Wafer Thickness";
            //New 20251114
            public const string Zone1ProportionalValvePressureAct = "Zone 1 Proportional Valve Pressure Act";
            public const string Zone1ProportionalValvePressureSet = "Zone 1 Proportional Valve Pressure Set";
            public const string Zone2ProportionalValvePressureAct = "Zone 2 Proportional Valve Pressure Act";
            public const string Zone2ProportionalValvePressureSet = "Zone 2 Proportional Valve Pressure Set";
            public const string Zone3ProportionalValvePressureAct = "Zone 3 Proportional Valve Pressure Act";
            public const string Zone3ProportionalValvePressureSet = "Zone 3 Proportional Valve Pressure Set";
            public const string Zone4ProportionalValvePressureAct = "Zone 4 Proportional Valve Pressure Act";
            public const string Zone4ProportionalValvePressureSet = "Zone 4 Proportional Valve Pressure Set";
            public const string Zone5ProportionalValvePressureAct = "Zone 5 Proportional Valve Pressure Act";
            public const string Zone5ProportionalValvePressureSet = "Zone 5 Proportional Valve Pressure Set";
            public const string Zone6ProportionalValvePressureAct = "Zone 6 Proportional Valve Pressure Act";
            public const string Zone6ProportionalValvePressureSet = "Zone 6 Proportional Valve Pressure Set";
            public const string Zone7ProportionalValvePressureAct = "Zone 7 Proportional Valve Pressure Act";
            public const string Zone7ProportionalValvePressureSet = "Zone 7 Proportional Valve Pressure Set";
            public const string Zone8ProportionalValvePressureAct = "Zone 8 Proportional Valve Pressure Act";
            public const string Zone8ProportionalValvePressureSet = "Zone 8 Proportional Valve Pressure Set";
            public const string TopRingProportionalValvePressureAct = "Top Ring Proportional Valve Pressure Act";
            public const string TopRingProportionalValvePressureSet = "Top Ring Proportional Valve Pressure Set";
            public const string ConditionHeadProportionalValvePressureAct = "Condition Head Proportional Valve Pressure Act";
            public const string ConditionHeadProportionalValvePressureSet = "Condition Head Proportional Valve Pressure Set";
            public const string ConditionPressureSensorAct = "Condition Pressure Sensor Act";
            public const string HeadPressureSensorAct = "Head Pressure Sensor Act";
            public const string HeadDistanceSensorAct = "Head Distance Sensor Act";
            public const string PlantenTemperatureAct = "Planten Temperature Act";
            public const string ExhaustAct = "Exhaust Act";
            public const string RadiusForConditionCenterAndPlatenCenterAct = "Radius For Condition Center And Platen Center Act";
            public const string PlatenBreakwaterUpLimitSensorIn = "Platen Breakwater Up Limit Sensor In";
            public const string PlatenBreakwaterDownLimitSensorIn = "Platen Breakwater Down Limit Sensor In";
            public const string WaterSeparatorLiquidLevelSensorIn = "Water Separator Liquid Level Sensor In";
            public const string WaferOutSensorIn = "Wafer Out Sensor In";
            public const string PlatenBreakwaterUpControlOut = "Platen Breakwater Up Control Out";
            public const string PlatenBreakwaterDownControlOut = "Platen Breakwater Down Control Out";
            public const string ConditionerHeadUpControlOut = "Conditioner Head Up Control Out";
            public const string ConditionerHeadDownControlOut = "Conditioner Head Down Control Out";
            public const string Slurry1Three_wayTotalInletValveControlOut = "Slurry1 Three_way Total Inlet Valve Control Out";
            public const string Slurry1Three_wayReturnValveControlOut = "Slurry1 Three_way Return Valve Control Out";
            public const string Slurry1Three_wayIntletValveControlOut = "Slurry1 Three_way Intlet Valve Control Out";
            public const string WaterThree_wayIntletValve1ControlOut = "Water  Three_way Intlet Valve1 Control Out";
            public const string Slurry1OrDIWOutletValveControlOut = "Slurry1 Or DIW Outlet Valve Control Out";
            public const string Slurry2Three_wayTotalInletValveOut = "Slurry2 Three_way Total Inlet Valve Out";
            public const string Slurry2Three_wayReturnValveControlOut = "Slurry2 Three_way Return Valve Control Out";
            public const string Slurry2Three_wayIntletValveControlOut = "Slurry2 Three_way Intlet Valve Control Out";
            public const string WaterThree_wayIntletValve2ControlOut = "Water  Three_way Intlet Valve2 Control Out";
            public const string Slurry2OrDIWOutletValveControlOut = "Slurry2 Or DIW Outlet Valve Control Out";
            public const string Slurry3Three_wayTotalInletValveControlOut = "Slurry3 Three_way Total Inlet Valve Control Out";
            public const string Slurry3Three_wayReturnValveControlOut = "Slurry3 Three_way Return Valve Control Out";
            public const string Slurry3Three_wayIntletValveControlOut = "Slurry3 Three_way Intlet Valve Control Out";
            public const string WaterThree_wayIntletValve3ControlOut = "Water  Three_way Intlet Valve3 Control Out";
            public const string Slurry3OrDIWOutletValveControlOut = "Slurry3 Or DIW Outlet Valve Control Out";
            public const string DIWOutletValveControlOut = "DIW Outlet Valve Control Out";
            public const string PlatenWashValveControlOut = "Platen Wash Valve Control Out";
            public const string MovingHeadDIWWashValveControlOut = "Moving Head DIW Wash Valve Control Out ";
            public const string MovingHeadExternalDIWWashValveControlOut = "Moving Head External DIW Wash Valve Control Out";
            public const string HeadChemicalWashValveControlOut = "Head Chemical Wash Valve Control Out";
            public const string ConditionerDIWWashValveControlOut = "Conditioner DIW Wash Valve Control Out";
            public const string ConditionerArmDIWWashValveControlOut = "Conditioner Arm DIW Wash Valve Control Out";
            public const string ChemicalArmDIWWashValveControlOut = "Chemical Arm DIW Wash Valve Control Out";
            public const string HeadMiddleWashValveControlOut = "Head Middle Wash Valve Control Out";
            public const string HeadExternalWashValveControlOut = "Head External Wash Valve Control Out";
            public const string WFUpperFaceChemicalWashValveControlOut = "WF Upper Face Chemical Wash Valve Control Out";
            public const string WFUpperFaceDIWWashValveControlOut = "WF Upper Face DIW Wash Valve Control Out";
            public const string WFLowerFaceChemicalWashValveControlOut = "WF Lower Face Chemical Wash Valve Control Out";
            public const string WFLowerFaceDIWWashValveControlOut = "WF Lower Face DIW Wash Valve Control Out";
            public const string PlatenCoolWaterValveControl = "Platen Cool Water Valve Control ";
            public const string PlatenMotorCoolWaterValveControl = "Platen Motor Cool Water Valve Control ";
            public const string Zone1PressValve1ControlOut = "Zone 1 Press Valve1 Control Out";
            public const string Zone1PressValve2ControlOut = "Zone 1 Press Valve2 Control Out";
            public const string Zone1VacuumValveControlOut = "Zone 1 Vacuum Valve Control Out";
            public const string Zone2PressValve1ControlOut = "Zone 2 Press Valve1 Control Out";
            public const string Zone2PressValve2ControlOut = "Zone 2 Press Valve2 Control Out";
            public const string Zone2VacuumValveControlOut = "Zone 2 Vacuum Valve Control Out";
            public const string Zone3PressValve1ControlOut = "Zone 3 Press Valve1 Control Out";
            public const string Zone3PressValve2ControlOut = "Zone 3 Press Valve2 Control Out";
            public const string Zone3VacuumValveControlOut = "Zone 3 Vacuum Valve Control Out";
            public const string Zone4PressValve1ControlOut = "Zone 4 Press Valve1 Control Out";
            public const string Zone4PressValve2ControlOut = "Zone 4 Press Valve2 Control Out";
            public const string Zone4VacuumValveControlOut = "Zone 4 Vacuum Valve Control Out";
            public const string Zone5PressValve1ControlOut = "Zone 5 Press Valve1 Control Out";
            public const string Zone5PressValve2ControlOut = "Zone 5 Press Valve2 Control Out";
            public const string Zone5VacuumValveControlOut = "Zone 5 Vacuum Valve Control Out";
            public const string Zone6PressValve1ControlOut = "Zone 6 Press Valve1 Control Out";
            public const string Zone6PressValve2ControlOut = "Zone 6 Press Valve2 Control Out";
            public const string Zone6VacuumValveControlOut = "Zone 6 Vacuum Valve Control Out";
            public const string Zone7PressValve1ControlOut = "Zone 7 Press Valve1 Control Out";
            public const string Zone7PressValve2ControlOut = "Zone 7 Press Valve2 Control Out";
            public const string Zone7VacuumValveControlOut = "Zone 7 Vacuum Valve Control Out";
            public const string Zone8PressValve1ControlOut = "Zone 8 Press Valve1 Control Out";
            public const string Zone8PressValve2ControlOut = "Zone 8 Press Valve2 Control Out";
            public const string Zone8VacuumValveControlOut = "Zone 8 Vacuum Valve Control Out";
            public const string TopRingPressValve1ControlOut = "Top Ring Press Valve1 Control Out";
            public const string TopRingPressValve2ControlOut = "Top Ring Press Valve2 Control Out";
            public const string TopRingVacuumValveControlOut = "Top Ring Vacuum Valve Control Out";
            public const string Zone4BlowValve1ControlOut = "Zone 4 Blow Valve1 Control Out";
            public const string Zone4BlowValve2ControlOut = "Zone 4 Blow Valve2 Control Out";
            public const string Zone4AtmosphericValveControlOut = "Zone 4 Atmospheric Valve Control Out";
            public const string AirBagVacuumGeneratorIntletValveControlOut = "Air Bag Vacuum Generator Intlet Valve Control Out";
            public const string AirBagVacuumGeneratorOutletValveControlOut = "Air Bag Vacuum Generator Outlet Valve Control Out";
            public const string VacuumGeneratorIntletValveControlOut = "Vacuum Generator Intlet Valve Control Out";
            public const string VacuumGeneratorOutletValveControlOut = "Vacuum Generator Outlet Valve Control Out";
            public const string CatchWaterBlowValveControlOut = "Catch Water Blow Valve Control Out";
            public const string CatchWaterDrainValveControlOut = "Catch Water Drain Valve Control Out";
            public const string HeadExternalBlowN2ValveControlOut = "Head External Blow N2 Valve Control Out";
            public const string Zone1VentValveControlOut = "Zone 1 Vent Valve Control Out";
            public const string Zone2VentValveControlOut = "Zone 2 Vent Valve Control Out";
            public const string Zone3VentValveControlOut = "Zone 3 Vent Valve Control Out";
            public const string Zone5VentValveControlOut = "Zone 5 Vent Valve Control Out";
            public const string Zone6VentValveControlOut = "Zone 6 Vent Valve Control Out";
            public const string Zone7VentValveControlOut = "Zone 7 Vent Valve Control Out";
            public const string Zone8VentValveControlOut = "Zone 8 Vent Valve Control Out";
            public const string TopRingVentValveControlOut = "Top Ring Vent Valve Control Out";
            //New 20251124
            public const string PlatenRotationMotorServo = "Platen Rotation Motor Servo";
            public const string HeadRotationMotorServo = "Head Rotation Motor Servo";
            public const string HeadSweepMotorServo = "Head Sweep Motor Servo";
            public const string HeadUpDownMotorServo = "Head UpDown Motor Servo";
            public const string ConditionHeadRotationMotorServo = "Condition Head Rotation Motor Servo";
            public const string ConditionSweepMotorServo = "Condition Sweep Motor Servo";
            public const string SlurrySweepMotorServo = "Slurry Sweep Motor Servo";
        }
    }

    public enum PolishProcessType
    {
        None = 100,
        Polish = 101,
        Load = 103,
        Unload = 104,
        Dechuck = 105,
        ExsituCondition = 240
    }

    public class Roller:CollectDataChamber
    {
        KHSTRollerCollectData.stRollerCollectData _stRollerCollectData;
        public FieldInfo[] collectDataFields;
        public string rollerName = string.Empty;
        public FieldInfo[] ioDataFields;

        public Roller(string name, KVLink link) : base(name, link)
        {
            _stRollerCollectData = new KHSTRollerCollectData.stRollerCollectData();
            collectDataFields = typeof(KHSTRollerCollectData.stRollerCollectData).GetFields();
            rollerName = name;
            ioDataFields = typeof(KHSTRollerIOData.stRollerIOData).GetFields();
        }

        public override void CollectData()
        {
            CollectProcessData();
            CollectIOData();
            CollectMotorServoStatus();
        }

        private void CollectProcessData()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTRollerCollectData.stRollerCollectData _stRollerCollectData = new KHSTRollerCollectData.stRollerCollectData();
                KHSTRollerCollectData.ByteTo_stRollerCollectData(ref _stRollerCollectData, _readBuf);
                if (_stRollerCollectData != null)
                {
                    CompiledFieldAccessor<KHSTRollerCollectData.stRollerCollectData>.ProcessAllFields(_stRollerCollectData, (name, value) =>
                    {
                        SetSv(this.rollerName + name, value);
                    });
                }
            }
        }

        private void CollectIOData()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "IOData1", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTRollerIOData.stRollerIOData _stRollerIOData = new KHSTRollerIOData.stRollerIOData();
                KHSTRollerIOData.ByteTo_stRollerIOData(ref _stRollerIOData, _readBuf);
                if (_stRollerIOData != null)
                {
                    CompiledFieldAccessor<KHSTRollerIOData.stRollerIOData>.ProcessAllFields(_stRollerIOData, (name, value) =>
                    {
                        SetSv(this.rollerName + "_1" + name, value);
                    });
                }
            }
            err = KHT.KHLReadVariable(kvLink.sock, "IOData2", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTRollerIOData.stRollerIOData _stRollerIOData = new KHSTRollerIOData.stRollerIOData();
                KHSTRollerIOData.ByteTo_stRollerIOData(ref _stRollerIOData, _readBuf);
                if (_stRollerIOData != null)
                {
                    CompiledFieldAccessor<KHSTRollerIOData.stRollerIOData>.ProcessAllFields(_stRollerIOData, (name, value) =>
                    {
                        SetSv(this.rollerName + "_2" + name, value);
                    });
                }
            }
            err = KHT.KHLReadVariable(kvLink.sock, "IOData3", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTRollerIOData.stRollerIOData _stRollerIOData = new KHSTRollerIOData.stRollerIOData();
                KHSTRollerIOData.ByteTo_stRollerIOData(ref _stRollerIOData, _readBuf);
                if (_stRollerIOData != null)
                {
                    CompiledFieldAccessor<KHSTRollerIOData.stRollerIOData>.ProcessAllFields(_stRollerIOData, (name, value) =>
                    {
                        SetSv(this.rollerName + "_3" + name, value);
                    });
                }
            }
        }

        private void CollectMotorServoStatus()
        {
            bool[] motorServoStatus = new bool[4];
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 534, 0, 4, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                SetSv(Keys.RollerA1LeftPillarMotorServo, motorServoStatus[0]);
                SetSv(Keys.RollerA1RightPillarMotorServo, motorServoStatus[1]);
                SetSv(Keys.RollerA1UpperRollerMotorServo, motorServoStatus[2]);
                SetSv(Keys.RollerA1LowerRollerMotorServo, motorServoStatus[3]);
            }
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 534, 8, 4, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                SetSv(Keys.RollerA2LeftPillarMotorServo, motorServoStatus[0]);
                SetSv(Keys.RollerA2RightPillarMotorServo, motorServoStatus[1]);
                SetSv(Keys.RollerA2UpperRollerMotorServo, motorServoStatus[2]);
                SetSv(Keys.RollerA2LowerRollerMotorServo, motorServoStatus[3]);
            }
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 535, 0, 4, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                SetSv(Keys.RollerA3LeftPillarMotorServo, motorServoStatus[0]);
                SetSv(Keys.RollerA3RightPillarMotorServo, motorServoStatus[1]);
                SetSv(Keys.RollerA3UpperRollerMotorServo, motorServoStatus[2]);
                SetSv(Keys.RollerA3LowerRollerMotorServo, motorServoStatus[3]);
            }
        }

        public static class Keys
        {
            public static string RollerA1ProcessStepAct = "RollerA1 Process Step Act";
            public static string RollerA1ProcessStepSet = "RollerA1 Process Step Set";
            public static string RollerA1ProcessTypeAct = "RollerA1 Process Type Act";
            public static string RollerA1ProcessTypeSet = "RollerA1 Process Type Set";
            public static string RollerA1RecipeTimeAct = "RollerA1 Recipe Time Act";
            public static string RollerA1RecipeTimeSet = "RollerA1 Recipe Time Set";
            public static string RollerA1UpperRollerRotateVelocityAct = "RollerA1 Upper Roller Rotate Velocity Act";
            public static string RollerA1UpperRollerRotateVelocitySet = "RollerA1 Upper Roller Rotate Velocity Set";
            public static string RollerA1LowerRollerRotateVelocityAct = "RollerA1 Lower Roller Rotate Velocity Act";
            public static string RollerA1LowerRollerRotateVelocitySet = "RollerA1 Lower Roller Rotate Velocity Set";
            public static string RollerA1WFLeftPillarRotateMotorVelocityAct = "RollerA1 WF Left Pillar Rotate Motor Velocity Act";
            public static string RollerA1WFLeftPillarRotateMotorVelocitySet = "RollerA1 WF Left Pillar Rotate Motor Velocity Set";
            public static string RollerA1WFRightPillarRotateMotorVelocityAct = "RollerA1 WF Right Pillar Rotate Motor Velocity Act ";
            public static string RollerA1WFRightPillarRotateMotorVelocitySet = "RollerA1 WF Right Pillar Rotate Motor Velocity Set ";
            public static string RollerA1UpperRollerPressureAct = "RollerA1 Upper Roller Pressure Act";
            public static string RollerA1UpperRollerPressureSet = "RollerA1 Upper Roller Pressure Set";
            public static string RollerA1LowerRollerPressureAct = "RollerA1 Lower Roller Pressure Act";
            public static string RollerA1LowerRollerPressureSet = "RollerA1 Lower Roller Pressure Set";
            public static string RollerA1WFUpperFaceDIWFlowrateAct = "RollerA1 WF Upper Face DIW Flowrate Act";
            public static string RollerA1WFUpperFaceDIWFlowrateSet = "RollerA1 WF Upper Face DIW Flowrate Set";
            public static string RollerA1WFLowerFaceDIWFlowrateAct = "RollerA1 WF Lower Face DIW Flowrate Act";
            public static string RollerA1WFLowerFaceDIWFlowrateSet = "RollerA1 WF Lower Face DIW Flowrate Set";
            public static string RollerA1WFUpperFaceChemicalSupplyAct = "RollerA1 WF Upper Face Chemical Supply Act";
            public static string RollerA1WFUpperFaceChemicalSupplySet = "RollerA1 WF Upper Face Chemical Supply Set";
            public static string RollerA1WFLowerFaceChemicalSupplyAct = "RollerA1 WF Lower Face Chemical Supply Act";
            public static string RollerA1WFLowerFaceChemicalSupplySet = "RollerA1 WF Lower Face Chemical Supply Set";
            public static string RollerA1MegaSonicSupplyAct = "RollerA1 Mega Sonic Supply Act";
            public static string RollerA1MegaSonicSupplySet = "RollerA1 Mega Sonic Supply Set";
            public static string RollerA1ChemicalEnterFlowratemeteAct = "RollerA1 Chemical Enter Flowratemete  Act";
            public static string RollerA1ChemicalEnterFlowratemeteSet = "RollerA1 Chemical Enter Flowratemete Set ";
            public static string RollerA1DIWEnterFlowratemeteAct = "RollerA1 DIW  Enter Flowratemete Act ";
            public static string RollerA1DIWEnterFlowratemeteSet = "RollerA1 DIW  Enter Flowratemete Set ";
            public static string RollerA1WFLeftPillarRotateMotorAngleAct = "RollerA1 WF Left Pillar Rotate Motor Angle Act ";
            public static string RollerA1WFRightPillarRotateMotorAngleAct = "RollerA1 WF Right Pillar Rotate Motor Angle Act ";
            public static string RollerA1UpperRollerRotateMotorAngleAct = "RollerA1 Upper Roller Rotate Motor Angle Act ";
            public static string RollerA1LowerRollerRotateMotorAngleAct = "RollerA1 Lower Roller Rotate Motor Angle Act ";
            public static string RollerA1ChemicalEntrancePressureAct = "RollerA1 Chemical Entrance  Pressure Act";
            public static string RollerA1WFLowerFaceChemicalWashPressureAct = "RollerA1 WF Lower Face Chemical Wash Pressure Act ";
            public static string RollerA1WFUpperFaceChemicalWashFlowrateAct = "RollerA1 WF Upper Face Chemical Wash Flowrate Act ";
            public static string RollerA1QuartzChemicalWashFlowrateAct = "RollerA1 Quartz Chemical Wash Flowrate Act ";
            public static string RollerA1WFLowerFaceDIWWashPressureAct = "RollerA1 WF Lower Face DIW Wash Pressure Act";
            public static string RollerA1UpperRollerDIWWashFlowrateAct = "RollerA1 Upper Roller DIW Wash Flowrate Act ";
            public static string RollerA1QuartzDIWWashFlowrateAct = "RollerA1 Quartz DIW Wash Flowrate Act ";
            public static string RollerA1UpperRollerInsideDIWWashFlowrateAct = "RollerA1 Upper Roller Inside DIW Wash Flowrate Act ";
            public static string RollerA1LowerRollerInsideDIWWashFlowrateAct = "RollerA1 Lower Roller Inside DIW Wash Flowrate Act";
            public static string RollerA1MageSonicDIWWashFlowrateAct = "RollerA1 Mage Sonic DIW Wash Flowrate Act ";
            public static string RollerA1ChamberDIWWashFlowrateAct = "RollerA1 Chamber DIW Wash Flowrate Act ";
            public static string RollerA1PillarWashFlowrateAct = "RollerA1 Pillar Wash Flowrate Act ";
            public static string RollerA1CoolingWaterCirculationFlowrateAct = "RollerA1 Cooling Water Circulation Flowrate Act ";
            public static string RollerA1LowerRollerDownPressureAct = "RollerA1 Lower Roller Down Pressure Act";
            public static string RollerA1ClampCylinderPressureAct = "RollerA1 Clamp Cylinder Pressure Act ";
            public static string RollerA1UpperRollerPressureAct1 = "RollerA1 Upper Roller Pressure Act1";
            public static string RollerA1ClampPressureAct = "RollerA1 Clamp Pressure Act ";
            public static string RollerA1ClampPressureSet = "RollerA1 Clamp Pressure Set";
            public static string RollerA2ProcessStepAct = "RollerA2 Process Step Act";
            public static string RollerA2ProcessStepSet = "RollerA2 Process Step Set";
            public static string RollerA2ProcessTypeAct = "RollerA2 Process Type Act";
            public static string RollerA2ProcessTypeSet = "RollerA2 Process Type Set";
            public static string RollerA2RecipeTimeAct = "RollerA2 Recipe Time Act";
            public static string RollerA2RecipeTimeSet = "RollerA2 Recipe Time Set";
            public static string RollerA2UpperRollerRotateVelocityAct = "RollerA2 Upper Roller Rotate Velocity Act";
            public static string RollerA2UpperRollerRotateVelocitySet = "RollerA2 Upper Roller Rotate Velocity Set";
            public static string RollerA2LowerRollerRotateVelocityAct = "RollerA2 Lower Roller Rotate Velocity Act";
            public static string RollerA2LowerRollerRotateVelocitySet = "RollerA2 Lower Roller Rotate Velocity Set";
            public static string RollerA2WFLeftPillarRotateMotorVelocityAct = "RollerA2 WF Left Pillar Rotate Motor Velocity Act";
            public static string RollerA2WFLeftPillarRotateMotorVelocitySet = "RollerA2 WF Left Pillar Rotate Motor Velocity Set";
            public static string RollerA2WFRightPillarRotateMotorVelocityAct = "RollerA2 WF Right Pillar Rotate Motor Velocity Act ";
            public static string RollerA2WFRightPillarRotateMotorVelocitySet = "RollerA2 WF Right Pillar Rotate Motor Velocity Set ";
            public static string RollerA2UpperRollerPressureAct = "RollerA2 Upper Roller Pressure Act";
            public static string RollerA2UpperRollerPressureSet = "RollerA2 Upper Roller Pressure Set";
            public static string RollerA2LowerRollerPressureAct = "RollerA2 Lower Roller Pressure Act";
            public static string RollerA2LowerRollerPressureSet = "RollerA2 Lower Roller Pressure Set";
            public static string RollerA2WFUpperFaceDIWFlowrateAct = "RollerA2 WF Upper Face DIW Flowrate Act";
            public static string RollerA2WFUpperFaceDIWFlowrateSet = "RollerA2 WF Upper Face DIW Flowrate Set";
            public static string RollerA2WFLowerFaceDIWFlowrateAct = "RollerA2 WF Lower Face DIW Flowrate Act";
            public static string RollerA2WFLowerFaceDIWFlowrateSet = "RollerA2 WF Lower Face DIW Flowrate Set";
            public static string RollerA2WFUpperFaceChemicalSupplyAct = "RollerA2 WF Upper Face Chemical Supply Act";
            public static string RollerA2WFUpperFaceChemicalSupplySet = "RollerA2 WF Upper Face Chemical Supply Set";
            public static string RollerA2WFLowerFaceChemicalSupplyAct = "RollerA2 WF Lower Face Chemical Supply Act";
            public static string RollerA2WFLowerFaceChemicalSupplySet = "RollerA2 WF Lower Face Chemical Supply Set";
            public static string RollerA2MegaSonicSupplyAct = "RollerA2 Mega Sonic Supply Act";
            public static string RollerA2MegaSonicSupplySet = "RollerA2 Mega Sonic Supply Set";
            public static string RollerA2ChemicalEnterFlowratemeteAct = "RollerA2 Chemical Enter Flowratemete Act ";
            public static string RollerA2ChemicalEnterFlowratemeteSet = "RollerA2 Chemical Enter Flowratemete Set ";
            public static string RollerA2DIWEnterFlowratemeteAct = "RollerA2 DIW  Enter Flowratemete Act ";
            public static string RollerA2DIWEnterFlowratemeteSet = "RollerA2 DIW  Enter Flowratemete Set ";
            public static string RollerA2WFLeftPillarRotateMotorAngleAct = "RollerA2 WF Left Pillar Rotate Motor Angle Act  ";
            public static string RollerA2WFRightPillarRotateMotorAngleAct = "RollerA2 WF Right Pillar Rotate Motor Angle Act ";
            public static string RollerA2UpperRollerRotateMotorAngleAct = "RollerA2 Upper Roller Rotate Motor Angle Act ";
            public static string RollerA2LowerRollerRotateMotorAngleAct = "RollerA2 Lower Roller Rotate Motor Angle Act ";
            public static string RollerA2ChemicalEntrancePressureAct = "RollerA2 Chemical Entrance  Pressure Act";
            public static string RollerA2WFLowerFaceChemicalWashPressureAct = "RollerA2 WF Lower Face Chemical Wash Pressure Act ";
            public static string RollerA2WFUpperFaceChemicalWashFlowrateAct = "RollerA2 WF Upper Face Chemical Wash Flowrate Act ";
            public static string RollerA2QuartzChemicalWashFlowrateAct = "RollerA2 Quartz Chemical Wash Flowrate Act ";
            public static string RollerA2WFLowerFaceDIWWashPressureAct = "RollerA2 WF Lower Face DIW Wash Pressure Act";
            public static string RollerA2UpperRollerDIWWashFlowrateAct = "RollerA2 Upper Roller DIW Wash Flowrate Act ";
            public static string RollerA2QuartzDIWWashFlowrateAct = "RollerA2 Quartz DIW Wash Flowrate Act ";
            public static string RollerA2UpperRollerInsideDIWWashFlowrateAct = "RollerA2 Upper Roller Inside DIW Wash Flowrate Act ";
            public static string RollerA2LowerRollerInsideDIWWashFlowrateAct = "RollerA2 Lower Roller Inside DIW Wash Flowrate Act";
            public static string RollerA2MageSonicDIWWashFlowrateAct = "RollerA2 Mage Sonic DIW Wash Flowrate Act ";
            public static string RollerA2ChamberDIWWashFlowrateAct = "RollerA2 Chamber DIW Wash Flowrate Act ";
            public static string RollerA2PillarWashFlowrateAct = "RollerA2 Pillar Wash Flowrate Act";
            public static string RollerA2CoolingWaterCirculationFlowrateAct = "RollerA2 Cooling Water Circulation Flowrate Act ";
            public static string RollerA2LowerRollerDownPressureAct = "RollerA2 Lower Roller Down Pressure Act";
            public static string RollerA2ClampCylinderPressureAct = "RollerA2 Clamp Cylinder Pressure Act ";
            public static string RollerA2UpperRollerPressureAct1 = "RollerA2 Upper Roller Pressure Act1";
            public static string RollerA2ClampPressureAct = "RollerA2 Clamp Pressure Act ";
            public static string RollerA2ClampPressureSet = "RollerA2 Clamp Pressure Set";
            public static string RollerA3ProcessStepAct = "RollerA3 Process Step Act";
            public static string RollerA3ProcessStepSet = "RollerA3 Process Step Set";
            public static string RollerA3ProcessTypeAct = "RollerA3 Process Type Act";
            public static string RollerA3ProcessTypeSet = "RollerA3 Process Type Set";
            public static string RollerA3RecipeTimeAct = "RollerA3 Recipe Time Act";
            public static string RollerA3RecipeTimeSet = "RollerA3 Recipe Time Set";
            public static string RollerA3UpperRollerRotateVelocityAct = "RollerA3 Upper Roller Rotate Velocity Act";
            public static string RollerA3UpperRollerRotateVelocitySet = "RollerA3 Upper Roller Rotate Velocity Set";
            public static string RollerA3LowerRollerRotateVelocityAct = "RollerA3 Lower Roller Rotate Velocity Act";
            public static string RollerA3LowerRollerRotateVelocitySet = "RollerA3 Lower Roller Rotate Velocity Set";
            public static string RollerA3WFLeftPillarRotateMotorVelocityAct = "RollerA3 WF Left Pillar Rotate Motor Velocity Act";
            public static string RollerA3WFLeftPillarRotateMotorVelocitySet = "RollerA3 WF Left Pillar Rotate Motor Velocity Set";
            public static string RollerA3WFRightPillarRotateMotorVelocityAct = "RollerA3 WF Right Pillar Rotate Motor Velocity Act ";
            public static string RollerA3WFRightPillarRotateMotorVelocitySet = "RollerA3 WF Right Pillar Rotate Motor Velocity Set ";
            public static string RollerA3UpperRollerPressureAct = "RollerA3 Upper Roller Pressure Act";
            public static string RollerA3UpperRollerPressureSet = "RollerA3 Upper Roller Pressure Set";
            public static string RollerA3LowerRollerPressureAct = "RollerA3 Lower Roller Pressure Act";
            public static string RollerA3LowerRollerPressureSet = "RollerA3 Lower Roller Pressure Set";
            public static string RollerA3WFUpperFaceDIWFlowrateAct = "RollerA3 WF Upper Face DIW Flowrate Act";
            public static string RollerA3WFUpperFaceDIWFlowrateSet = "RollerA3 WF Upper Face DIW Flowrate Set";
            public static string RollerA3WFLowerFaceDIWFlowrateAct = "RollerA3 WF Lower Face DIW Flowrate Act";
            public static string RollerA3WFLowerFaceDIWFlowrateSet = "RollerA3 WF Lower Face DIW Flowrate Set";
            public static string RollerA3WFUpperFaceChemicalSupplyAct = "RollerA3 WF Upper Face Chemical Supply Act";
            public static string RollerA3WFUpperFaceChemicalSupplySet = "RollerA3 WF Upper Face Chemical Supply Set";
            public static string RollerA3WFLowerFaceChemicalSupplyAct = "RollerA3 WF Lower Face Chemical Supply Act";
            public static string RollerA3WFLowerFaceChemicalSupplySet = "RollerA3 WF Lower Face Chemical Supply Set";
            public static string RollerA3MegaSonicSupplyAct = "RollerA3 Mega Sonic Supply Act";
            public static string RollerA3MegaSonicSupplySet = "RollerA3 Mega Sonic Supply Set";
            public static string RollerA3ChemicalEnterFlowratemeteAct = "RollerA3 Chemical Enter Flowratemete Act ";
            public static string RollerA3ChemicalEnterFlowratemeteSet = "RollerA3 Chemical Enter Flowratemete Set ";
            public static string RollerA3DIWEnterFlowratemeteAct = "RollerA3 DIW  Enter Flowratemete Act ";
            public static string RollerA3DIWEnterFlowratemeteSet = "RollerA3 DIW  Enter Flowratemete Set";
            public static string RollerA3WFLeftPillarRotateMotorAngleAct = "RollerA3 WF Left Pillar Rotate Motor Angle Act ";
            public static string RollerA3WFRightPillarRotateMotorAngleAct = "RollerA3 WF Right Pillar Rotate Motor Angle Act ";
            public static string RollerA3UpperRollerRotateMotorAngleAct = "RollerA3 Upper Roller Rotate Motor Angle Act ";
            public static string RollerA3LowerRollerRotateMotorAngleAct = "RollerA3 Lower Roller Rotate Motor Angle Act ";
            public static string RollerA3ChemicalEntrancePressureAct = "RollerA3 Chemical Entrance  Pressure Act";
            public static string RollerA3WFLowerFaceChemicalWashPressureAct = "RollerA3 WF Lower Face Chemical Wash Pressure Act ";
            public static string RollerA3WFUpperFaceChemicalWashFlowrateAct = "RollerA3 WF Upper Face Chemical Wash Flowrate Act ";
            public static string RollerA3QuartzChemicalWashFlowrateAct = "RollerA3 Quartz Chemical Wash Flowrate Act ";
            public static string RollerA3WFLowerFaceDIWWashPressureAct = "RollerA3 WF Lower Face DIW Wash Pressure Act";
            public static string RollerA3UpperRollerDIWWashFlowrateAct = "RollerA3 Upper Roller DIW Wash Flowrate Act ";
            public static string RollerA3QuartzDIWWashFlowrateAct = "RollerA3 Quartz DIW Wash Flowrate Act ";
            public static string RollerA3UpperRollerInsideDIWWashFlowrateAct = "RollerA3 Upper Roller Inside DIW Wash Flowrate Act ";
            public static string RollerA3LowerRollerInsideDIWWashFlowrateAct = "RollerA3 Lower Roller Inside DIW Wash Flowrate Act";
            public static string RollerA3MageSonicDIWWashFlowrateAct = "RollerA3 Mage Sonic DIW Wash Flowrate Act ";
            public static string RollerA3ChamberDIWWashFlowrateAct = "RollerA3 Chamber DIW Wash Flowrate Act ";
            public static string RollerA3PillarWashFlowrateAct = "RollerA3 Pillar Wash Flowrate Act ";
            public static string RollerA3CoolingWaterCirculationFlowrateAct = "RollerA3 Cooling Water Circulation Flowrate Act ";
            public static string RollerA3LowerRollerDownPressureAct = "RollerA3 Lower Roller Down Pressure Act";
            public static string RollerA3ClampCylinderPressureAct = "RollerA3 Clamp Cylinder Pressure Act ";
            public static string RollerA3UpperRollerPressureAct1 = "RollerA3 Upper Roller Pressure Act1";
            public static string RollerA3ClampPressureAct = "RollerA3 Clamp Pressure Act ";
            public static string RollerA3ClampPressureSet = "RollerA3 Clamp Pressure Set";
            //New 20251124
            public static string RollerA1LeftPillarMotorServo = "RollerA1 Left Pillar Motor Servo";
            public static string RollerA1RightPillarMotorServo = "RollerA1 Right Pillar Motor Servo";
            public static string RollerA1UpperRollerMotorServo = "RollerA1 Upper Roller Motor Servo";
            public static string RollerA1LowerRollerMotorServo = "RollerA1 Lower Roller Motor Servo";
            public static string RollerA2LeftPillarMotorServo = "RollerA2 Left Pillar Motor Servo";
            public static string RollerA2RightPillarMotorServo = "RollerA2 Right Pillar Motor Servo";
            public static string RollerA2UpperRollerMotorServo = "RollerA2 Upper Roller Motor Servo";
            public static string RollerA2LowerRollerMotorServo = "RollerA2 Lower Roller Motor Servo";
            public static string RollerA3LeftPillarMotorServo = "RollerA3 Left Pillar Motor Servo";
            public static string RollerA3RightPillarMotorServo = "RollerA3 Right Pillar Motor Servo";
            public static string RollerA3UpperRollerMotorServo = "RollerA3 Upper Roller Motor Servo";
            public static string RollerA3LowerRollerMotorServo = "RollerA3 Lower Roller Motor Servo";
        }
    }

    public class Pen:CollectDataChamber
    {
        KHSTPenCollectData.stPenCollectData _stPenCollectData;
        public FieldInfo[] collectDataFields;
        public string penName = string.Empty;
        public FieldInfo[] ioDataFields;
        public Pen(string name, KVLink link) : base(name, link)
        {
            _stPenCollectData = new KHSTPenCollectData.stPenCollectData();
            collectDataFields = typeof(KHSTPenCollectData.stPenCollectData).GetFields();
            penName = name;
            ioDataFields = typeof(KHSTIO.stPenIOData).GetFields();
        }

        public override void CollectData()
        {
            CollectProcessData();
            CollectIOData();
            CollectMotorServoStatus();
        }

        private void CollectProcessData()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTPenCollectData.stPenCollectData _stPenCollectData = new KHSTPenCollectData.stPenCollectData();
                KHSTPenCollectData.ByteTo_stPenCollectData(ref _stPenCollectData, _readBuf);
                if (_stPenCollectData != null)
                {
                    CompiledFieldAccessor<KHSTPenCollectData.stPenCollectData>.ProcessAllFields(_stPenCollectData, (name, value) =>
                    {
                        SetSv(this.penName + name, value);
                    });
                }
            }
        }

        private void CollectIOData()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "IOData1", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTIO.stPenIOData _stPenIOData = new KHSTIO.stPenIOData();
                KHSTIO.ByteTo_stPenIOData(ref _stPenIOData, _readBuf);
                if (_stPenIOData != null)
                {
                    CompiledFieldAccessor<stPenIOData>.ProcessAllFields(_stPenIOData, (name, value) =>
                    {
                        SetSv(this.penName + "_1" + name, value);
                    });
                }
            }
            err = KHT.KHLReadVariable(kvLink.sock, "IOData2", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTIO.stPenIOData _stPenIOData = new KHSTIO.stPenIOData();
                KHSTIO.ByteTo_stPenIOData(ref _stPenIOData, _readBuf);
                if (_stPenIOData != null)
                {
                    CompiledFieldAccessor<stPenIOData>.ProcessAllFields(_stPenIOData, (name, value) =>
                    {
                        SetSv(this.penName + "_2" + name, value);
                    });
                }
            }
        }

        private void CollectMotorServoStatus()
        {
            bool[] motorServoStatus = new bool[3];
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 537, 0, 3, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                SetSv(Keys.PenA1ChuckMotorServo, motorServoStatus[0]);
                SetSv(Keys.PenA1PenArmSweepMotorServo, motorServoStatus[1]);
                SetSv(Keys.PenA1PenRotationMotorServo, motorServoStatus[2]);
            }
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 537, 8, 3, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                SetSv(Keys.PenA2ChuckMotorServo, motorServoStatus[0]);
                SetSv(Keys.PenA2PenArmSweepMotorServo, motorServoStatus[1]);
                SetSv(Keys.PenA2PenRotationMotorServo, motorServoStatus[2]);
            }
        }

        public static class Keys
        {
            public static string PenA1ProcessStepAct = "PenA1 Process Step Act";
            public static string PenA1ProcessStepSet = "PenA1 Process Step Set";
            public static string PenA1ProcessTypeAct = "PenA1 Process Type Act";
            public static string PenA1ProcessTypeSet = "PenA1 Process Type Set";
            public static string PenA1RecipeTimeAct = "PenA1 Recipe Time Act";
            public static string PenA1RecipeTimeSet = "PenA1 Recipe Time Set";
            public static string PenA1ChuckRotationMotorVelocityAct = "PenA1 Chuck Rotation Motor Velocity Act";
            public static string PenA1ChuckRotationMotorVelocitySet = "PenA1 Chuck Rotation Motor Velocity Set";
            public static string PenA1PenArmSweepMotorVelocityAct = "PenA1 Pen Arm Sweep Motor Velocity Act";
            public static string PenA1PenArmSweepMotorVelocitySet = "PenA1 Pen Arm Sweep Motor Velocity Set";
            public static string PenA1PenArmSweepMotorPositionAct = "PenA1 Pen Arm Sweep Motor Position Act";
            public static string PenA1PenArmSweepMotorPositionSet = "PenA1 Pen Arm Sweep Motor Position Set";
            public static string PenA1PenArmSweepMotorCycleAct = "PenA1 Pen Arm Sweep Motor Cycle Act";
            public static string PenA1PenArmSweepMotorCycleSet = "PenA1 Pen Arm Sweep Motor Cycle Set";
            public static string PenA1PenRotationMotorVelocityAct = "PenA1 Pen Rotation Motor Velocity Act";
            public static string PenA1PenRotationMotorVelocitySet = "PenA1 Pen Rotation Motor Velocity Set";
            public static string PenA1PenPressureSetValueAct = "PenA1 Pen Pressure Set Value Act";
            public static string PenA1PenPressureSetValueSet = "PenA1 Pen Pressure Set Value Set";
            public static string PenA1WFUpperFaceN2OutletFlowrateAct = "PenA1 WF Upper Face N2 Outlet Flowrate Act";
            public static string PenA1WFUpperFaceN2OutletFlowrateSet = "PenA1 WF Upper Face N2 Outlet Flowrate Set";
            public static string PenA1WFUpperFaceDIWFlowrateAct = "PenA1 WF Upper Face DIW Flowrate Act";
            public static string PenA1WFUpperFaceDIWFlowrateSet = "PenA1 WF Upper Face DIW Flowrate Set";
            public static string PenA1WFLowerFaceDIWFlowrateAct = "PenA1 WF Lower Face DIW Flowrate Act";
            public static string PenA1WFLowerFaceDIWFlowrateSet = "PenA1 WF Lower Face DIW Flowrate Set";
            public static string PenA1WFLowerFaceN2SupplyAct = "PenA1 WF Lower Face N2 Supply Act";
            public static string PenA1WFLowerFaceN2SupplySet = "PenA1 WF Lower Face N2 Supply Set";
            public static string PenA1WFUpperLowerFaceChemicalSupplyAct = "PenA1 WF UpperLower Face Chemical Supply Act";
            public static string PenA1WFUpperLowerFaceChemicalSupplySet = "PenA1 WF UpperLower Face Chemical Supply Set";
            public static string PenA1WFUpperFaceChemicalSupplyAct = "PenA1 WF Upper Face Chemical Supply Act";
            public static string PenA1WFUpperFaceChemicalSupplySet = "PenA1 WF Upper Face Chemical Supply Set";
            public static string PenA1WFUpperFaceDICO2SupplyAct = "PenA1 WF Upper Face DICO2 Supply Act";
            public static string PenA1WFUpperFaceDICO2SupplySet = "PenA1 WF Upper Face DICO2 Supply Set";
            public static string PenA1DIWInletFlowrateAct = "PenA1 DIW Inlet Flowrate Act";
            public static string PenA1DIWInletFlowrateSet = "PenA1 DIW Inlet Flowrate Set";
            public static string PenA1DIWSupplyPressureAct = "PenA1 DIW Supply Pressure Act";
            public static string PenA1DIWSupplyPressureSet = "PenA1 DIW Supply Pressure Set";
            public static string PenA1ChemicalFlowrateAct = "PenA1 Chemical Flowrate Act";
            public static string PenA1ChemicalFlowrateSet = "PenA1 Chemical Flowrate Set";
            public static string PenA1CO2SupplyPressureAct = "PenA1 CO2 Supply Pressure Act";
            public static string PenA1CO2SupplyPressureSet = "PenA1 CO2 Supply Pressure Set";
            public static string PenA1ChuckRotationMotorPositionAct = "PenA1 Chuck Rotation Motor Position Act";
            public static string PenA1PenRotationMotorPositionAct = "PenA1 Pen Rotation Motor Position Act";
            public static string PenA1DIWSupplyPressureAct1 = "PenA1 DIW Supply Pressure Act1";
            public static string PenA1CarbonatedDIWResistanceAct = "PenA1 Carbonated DIW Resistance Act";
            public static string PenA1CarbonatedDIWFlowrateAct = "PenA1 Carbonated DIW Flowrate Act";
            public static string PenA1DIWChamberFlowrateAct = "PenA1 DIW Chamber Flowrate Act";
            public static string PenA1PenDIWWashFlowrateAct = "PenA1 Pen DIW Wash Flowrate Act";
            public static string PenA1QuartzDIWWashFlowrateAct = "PenA1 Quartz DIW Wash Flowrate Act";
            public static string PenA1ChemicalSupplyPressureAct = "PenA1 Chemical Supply Pressure Act";
            public static string PenA1QuartzChemicalWashFlowrateAct = "PenA1 Quartz Chemical Wash Flowrate Act";
            public static string PenA1WFUpperFaceWashChemicalFlowrateAct = "PenA1 WF Upper Face Wash Chemical Flowrate Act";
            public static string PenA1WFLowerFaceWashChemicalFlowrateAct = "PenA1 WF Lower Face Wash Chemical Flowrate Act";
            public static string PenA1WFUpperFaceN2PressureAct = "PenA1 WF Upper Face N2 Pressure Act";
            public static string PenA1WFLowerFaceN2PressureAct = "PenA1 WF Lower Face N2 Pressure Act";
            public static string PenA1WFLowerFaceN2OutletFlowAct = "PenA1 WF Lower Face N2 Outlet Flow Act";
            public static string PenA1CUPPurgeFlowAct = "PenA1 CUP Purge Flow Act";
            public static string PenA1CO2SupplyPressureAct1 = "PenA1 CO2 Supply Pressure Act1";
            public static string PenA2ProcessStepAct = "PenA2 Process Step Act";
            public static string PenA2ProcessStepSet = "PenA2 Process Step Set";
            public static string PenA2ProcessTypeAct = "PenA2 Process Type Act";
            public static string PenA2ProcessTypeSet = "PenA2 Process Type Set";
            public static string PenA2RecipeTimeAct = "PenA2 Recipe Time Act";
            public static string PenA2RecipeTimeSet = "PenA2 Recipe Time Set";
            public static string PenA2ChuckRotationMotorVelocityAct = "PenA2 Chuck Rotation Motor Velocity Act";
            public static string PenA2ChuckRotationMotorVelocitySet = "PenA2 Chuck Rotation Motor Velocity Set";
            public static string PenA2PenArmSweepMotorVelocityAct = "PenA2 Pen Arm Sweep Motor Velocity Act";
            public static string PenA2PenArmSweepMotorVelocitySet = "PenA2 Pen Arm Sweep Motor Velocity Set";
            public static string PenA2PenArmSweepMotorPositionAct = "PenA2 Pen Arm Sweep Motor Position Act";
            public static string PenA2PenArmSweepMotorPositionSet = "PenA2 Pen Arm Sweep Motor Position Set";
            public static string PenA2PenArmSweepMotorCycleAct = "PenA2 Pen Arm Sweep Motor Cycle Act";
            public static string PenA2PenArmSweepMotorCycleSet = "PenA2 Pen Arm Sweep Motor Cycle Set";
            public static string PenA2PenRotationMotorVelocityAct = "PenA2 Pen Rotation Motor Velocity Act";
            public static string PenA2PenRotationMotorVelocitySet = "PenA2 Pen Rotation Motor Velocity Set";
            public static string PenA2PenPressureSetValueAct = "PenA2 Pen Pressure Set Value Act";
            public static string PenA2PenPressureSetValueSet = "PenA2 Pen Pressure Set Value Set";
            public static string PenA2WFUpperFaceN2OutletFlowrateAct = "PenA2 WF Upper Face N2 Outlet Flowrate Act";
            public static string PenA2WFUpperFaceN2OutletFlowrateSet = "PenA2 WF Upper Face N2 Outlet Flowrate Set";
            public static string PenA2WFUpperFaceDIWFlowrateAct = "PenA2 WF Upper Face DIW Flowrate Act";
            public static string PenA2WFUpperFaceDIWFlowrateSet = "PenA2 WF Upper Face DIW Flowrate Set";
            public static string PenA2WFLowerFaceDIWFlowrateAct = "PenA2 WF Lower Face DIW Flowrate Act";
            public static string PenA2WFLowerFaceDIWFlowrateSet = "PenA2 WF Lower Face DIW Flowrate Set";
            public static string PenA2WFLowerFaceN2SupplyAct = "PenA2 WF Lower Face N2 Supply Act";
            public static string PenA2WFLowerFaceN2SupplySet = "PenA2 WF Lower Face N2 Supply Set";
            public static string PenA2WFUpperLowerFaceChemicalSupplyAct = "PenA2 WF UpperLower Face Chemical Supply Act";
            public static string PenA2WFUpperLowerFaceChemicalSupplySet = "PenA2 WF UpperLower Face Chemical Supply Set";
            public static string PenA2WFUpperFaceChemicalSupplyAct = "PenA2 WF Upper Face Chemical Supply Act";
            public static string PenA2WFUpperFaceChemicalSupplySet = "PenA2 WF Upper Face Chemical Supply Set";
            public static string PenA2WFUpperFaceDICO2SupplyAct = "PenA2 WF Upper Face DICO2 Supply Act";
            public static string PenA2WFUpperFaceDICO2SupplySet = "PenA2 WF Upper Face DICO2 Supply Set";
            public static string PenA2DIWInletFlowrateAct = "PenA2 DIW Inlet Flowrate Act";
            public static string PenA2DIWInletFlowrateSet = "PenA2 DIW Inlet Flowrate Set";
            public static string PenA2DIWSupplyPressureAct = "PenA2 DIW Supply Pressure Act";
            public static string PenA2DIWSupplyPressureSet = "PenA2 DIW Supply Pressure Set";
            public static string PenA2ChemicalFlowrateAct = "PenA2 Chemical Flowrate Act";
            public static string PenA2ChemicalFlowrateSet = "PenA2 Chemical Flowrate Set";
            public static string PenA2CO2SupplyPressure1Act = "PenA2 CO2 Supply Pressure1 Act";
            public static string PenA2CO2SupplyPressure1Set = "PenA2 CO2 Supply Pressure1 Set";
            public static string PenA2ChuckRotationMotorPositionAct = "PenA2 Chuck Rotation Motor Position Act";
            public static string PenA2PenRotationMotorPositionAct = "PenA2 Pen Rotation Motor Position Act";
            public static string PenA2DIWSupplyPressureAct1 = "PenA2 DIW Supply Pressure Act1";
            public static string PenA2CarbonatedDIWResistanceAct = "PenA2 Carbonated DIW Resistance Act";
            public static string PenA2CarbonatedDIWFlowrateAct = "PenA2 Carbonated DIW Flowrate Act";
            public static string PenA2DIWChamberFlowrateAct = "PenA2 DIW Chamber Flowrate Act";
            public static string PenA2PenDIWWashFlowrateAct = "PenA2 Pen DIW Wash Flowrate Act";
            public static string PenA2QuartzDIWWashFlowrateAct = "PenA2 Quartz DIW Wash Flowrate Act";
            public static string PenA2ChemicalSupplyPressureAct = "PenA2 Chemical Supply Pressure Act";
            public static string PenA2QuartzChemicalWashFlowrateAct = "PenA2 Quartz Chemical Wash Flowrate Act";
            public static string PenA2WFUpperFaceWashChemicalFlowrateAct = "PenA2 WF Upper Face Wash Chemical Flowrate Act";
            public static string PenA2WFLowerFaceWashChemicalFlowrateAct = "PenA2 WF Lower Face Wash Chemical Flowrate Act";
            public static string PenA2WFUpperFaceN2PressureAct = "PenA2 WF Upper Face N2 Pressure Act";
            public static string PenA2WFLowerFaceN2PressureAct = "PenA2 WF Lower Face N2 Pressure Act";
            public static string PenA2WFLowerFaceN2OutletFlowAct = "PenA2 WF Lower Face N2 Outlet Flow Act";
            public static string PenA2CUPPurgeFlowAct = "PenA2 CUP Purge Flow Act";
            public static string PenA2CO2SupplyPressure2Act = "PenA2 CO2 Supply Pressure2 Act";
            //New 20251124
            public static string PenA1ChuckMotorServo = "PenA1 Chuck Motor Servo";
            public static string PenA1PenArmSweepMotorServo = "PenA1 Pen Arm Sweep Motor Servo";
            public static string PenA1PenRotationMotorServo = "PenA1 Pen Rotation Motor Servo";
            public static string PenA2ChuckMotorServo = "PenA2 Chuck Motor Servo";
            public static string PenA2PenArmSweepMotorServo = "PenA2 Pen Arm Sweep Motor Servo";
            public static string PenA2PenRotationMotorServo = "PenA2 Pen Rotation Motor Servo";
        }
    }

    public class Dry:CollectDataChamber
    {
        KHSTCLEAN.stDryCollectData _stDryCollectData;
        public FieldInfo[] collectDataFields;
        public string dryName = string.Empty;
        public FieldInfo[] ioDataFields;
        public Dry(string name, KVLink link) : base(name, link)
        {
           _stDryCollectData = new KHSTCLEAN.stDryCollectData();
            collectDataFields = typeof(KHSTSV.stDryCollectData).GetFields();
            dryName = name;
            ioDataFields = typeof(KHSTIO.stDryIOData).GetFields();
        }

        public override void CollectData()
        {
            CollectProcessData();
            CollectProcessData1();
            CollectIOData1();
            CollectMotorServoStatus();
        }

        private void CollectProcessData()
        {
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTCLEAN.ByteTo_stDryCollectData(ref _stDryCollectData, readBuf);
                SetSv(Keys.ProcessStepAct, _stDryCollectData.ProcessStepAct);
                SetSv(Keys.ProcessStepSet, _stDryCollectData.ProcessStepSet);
                SetSv(Keys.ProcessTypeAct, _stDryCollectData.ProcessTypeAct);
                SetSv(Keys.ProcessTypeSet, _stDryCollectData.ProcessTypeSet);
                SetSv(Keys.RecipeTimeAct, _stDryCollectData.RecipeTimeAct);
                SetSv(Keys.RecipeTimeSet, _stDryCollectData.RecipeTimeSet);
                SetSv(Keys.ChuckRotationMotorVelocityValuAct, _stDryCollectData.ChuckRotationMotorVelocityValuAct);
                SetSv(Keys.ChuckRotationMotorVelocityValuSet, _stDryCollectData.ChuckRotationMotorVelocityValuSet);
                SetSv(Keys.ArmSweepMotorVelocityAct, _stDryCollectData.ArmSweepMotorVelocityAct);
                SetSv(Keys.ArmSweepMotorVelocitySet, _stDryCollectData.ArmSweepMotorVelocitySet);
                SetSv(Keys.ArmSweepMotorPositionAct, _stDryCollectData.ArmSweepMotorPositionAct);
                SetSv(Keys.ArmSweepMotorPositionSet, _stDryCollectData.ArmSweepMotorPositionSet);
                SetSv(Keys.ArmSweepMotorCycleAct, _stDryCollectData.ArmSweepMotorCycleAct);
                SetSv(Keys.ArmSweepMotorCycleSet, _stDryCollectData.ArmSweepMotorCycleSet);
                SetSv(Keys.WFUpperFaceN2PuegeFlowrateAct, _stDryCollectData.WFUpperFaceN2PuegeFlowrateAct);
                SetSv(Keys.WFUpperFaceN2PuegeFlowrateSet, _stDryCollectData.WFUpperFaceN2PuegeFlowrateSet);
                SetSv(Keys.WFLowerFaceN2Puege1FlowrateAct, _stDryCollectData.WFLowerFaceN2Puege1FlowrateAct);
                SetSv(Keys.WFLowerFaceN2Puege1FlowrateSet, _stDryCollectData.WFLowerFaceN2Puege1FlowrateSet);
                SetSv(Keys.WFUpperFaceDIWWashInlet2FlowrateAct, _stDryCollectData.WFUpperFaceDIWWashInlet2FlowrateAct);
                SetSv(Keys.WFUpperFaceDIWWashInlet2FlowrateSet, _stDryCollectData.WFUpperFaceDIWWashInlet2FlowrateSet);
                SetSv(Keys.WFUpperFaceIPASupplyAct, _stDryCollectData.WFUpperFaceIPASupplyAct);
                SetSv(Keys.WFUpperFaceIPASupplySet, _stDryCollectData.WFUpperFaceIPASupplySet);
                SetSv(Keys.WFLowerFaceN2Supply2Act, _stDryCollectData.WFLowerFaceN2Supply2Act);
                SetSv(Keys.WFLowerFaceN2Supply2Set, _stDryCollectData.WFLowerFaceN2Supply2Set);
                SetSv(Keys.WFUpperFaceDIWSupply1Act, _stDryCollectData.WFUpperFaceDIWSupply1Act);
                SetSv(Keys.WFUpperFaceDIWSupply1Set, _stDryCollectData.WFUpperFaceDIWSupply1Set);
                SetSv(Keys.WFLowerFaceDIWSupplyAct, _stDryCollectData.WFLowerFaceDIWSupplyAct);
                SetSv(Keys.WFLowerFaceDIWSupplySet, _stDryCollectData.WFLowerFaceDIWSupplySet);
                SetSv(Keys.N2MixOutletFlowrateAct, _stDryCollectData.N2MixOutletFlowrateAct);
                SetSv(Keys.N2MixOutletFlowrateSet, _stDryCollectData.N2MixOutletFlowrateSet);
                SetSv(Keys.ChuckRotationMotorPositionAct, _stDryCollectData.ChuckRotationMotorPositionAct);
                SetSv(Keys.WFUpperFaceDIWWash1FlowrateAct, _stDryCollectData.WFUpperFaceDIWWash1FlowrateAct);
                SetSv(Keys.WFLowerFaceDIWWashFlowrateAct, _stDryCollectData.WFLowerFaceDIWWashFlowrateAct);
                SetSv(Keys.IPAWeighAct, _stDryCollectData.IPAWeighAct);
                SetSv(Keys.IPAPressureAct, _stDryCollectData.IPAPressureAct);
                SetSv(Keys.IPALiquidLevelAct, _stDryCollectData.IPALiquidLevelAct);
            }
        }

        private void CollectProcessData1()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTSV.stDryCollectData _stDryCollectData = new KHSTSV.stDryCollectData();
                KHSTSV.ByteTo_stDryCollectData(ref _stDryCollectData, _readBuf);
                if (_stDryCollectData != null)
                {
                    CompiledFieldAccessor<stDryCollectData>.ProcessAllFields(_stDryCollectData, (name, value) =>
                    {
                        SetSv(this.dryName + name, value);
                    });
                }
            }
        }

        private void CollectIOData1()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "IOData", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTIO.stDryIOData _stDryIOData = new KHSTIO.stDryIOData();
                KHSTIO.ByteTo_stDryIOData(ref _stDryIOData, _readBuf);
                if (_stDryIOData != null)
                {
                    CompiledFieldAccessor<stDryIOData>.ProcessAllFields(_stDryIOData, (name, value) =>
                    {
                        SetSv(this.dryName + name, value);
                    });
                }
            }
        }

        private void CollectMotorServoStatus()
        {
            bool[] motorServoStatus = new bool[2];
            err = KHT.KHLReadDevicesAsBits(kvLink.sock, KHLDevType.DEV_W, 539, 0, 2, readBuf);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHST.ByteToBool(ref motorServoStatus, readBuf, motorServoStatus.Length);
                SetSv(Keys.ChuckMotorServo, motorServoStatus[0]);
                SetSv(Keys.ArmSweepMotorServo, motorServoStatus[1]);
            }
        }

        public static class Keys
        {
            public static string ProcessStepAct = "Process Step Act";
            public static string ProcessStepSet = "Process Step Set";
            public static string ProcessTypeAct = "Process Type Act";
            public static string ProcessTypeSet = "Process Type Set";
            public static string RecipeTimeAct = "Recipe Time Act";
            public static string RecipeTimeSet = "Recipe Time Set";
            public static string ChuckRotationMotorVelocityValuAct = "Chuck Rotation Motor Velocity Valu Act";
            public static string ChuckRotationMotorVelocityValuSet = "Chuck Rotation Motor Velocity Valu Set";
            public static string ArmSweepMotorVelocityAct = "Arm Sweep Motor Velocity Act";
            public static string ArmSweepMotorVelocitySet = "Arm Sweep Motor Velocity Set";
            public static string ArmSweepMotorPositionAct = "Arm Sweep Motor Position Act  ";
            public static string ArmSweepMotorPositionSet = "Arm Sweep Motor Position Set  ";
            public static string ArmSweepMotorCycleAct = "Arm Sweep Motor Cycle Act  ";
            public static string ArmSweepMotorCycleSet = "Arm Sweep Motor Cycle Set  ";
            public static string WFUpperFaceN2PuegeFlowrateAct = "WF Upper Face N2 Puege Flowrate Act  ";
            public static string WFUpperFaceN2PuegeFlowrateSet = "WF Upper Face N2 Puege Flowrate Set";
            public static string WFLowerFaceN2Puege1FlowrateAct = "WF Lower Face N2 Puege1 Flowrate Act  ";
            public static string WFLowerFaceN2Puege1FlowrateSet = "WF Lower Face N2 Puege1 Flowrate Set  ";
            public static string WFUpperFaceDIWWashInlet2FlowrateAct = "WF Upper Face DIW Wash Inlet2 Flowrate Act  ";
            public static string WFUpperFaceDIWWashInlet2FlowrateSet = "WF Upper Face DIW Wash Inlet2 Flowrate Set  ";
            public static string WFUpperFaceIPASupplyAct = "WF Upper Face IPA Supply Act";
            public static string WFUpperFaceIPASupplySet = "WF Upper Face IPA Supply Set";
            public static string WFLowerFaceN2Supply2Act = "WF Lower Face N2 Supply2 Act";
            public static string WFLowerFaceN2Supply2Set = "WF Lower Face N2 Supply2 Set";
            public static string WFUpperFaceDIWSupply1Act = "WF Upper Face DIW Supply1 Act";
            public static string WFUpperFaceDIWSupply1Set = "WF Upper Face DIW Supply1 Set";
            public static string WFLowerFaceDIWSupplyAct = "WF Lower Face DIW Supply Act";
            public static string WFLowerFaceDIWSupplySet = "WF Lower Face DIW Supply Set";
            public static string N2MixOutletFlowrateAct = "N2 Mix Outlet Flowrate Act  ";
            public static string N2MixOutletFlowrateSet = "N2 Mix Outlet Flowrate Set ";
            public static string ChuckRotationMotorPositionAct = "Chuck Rotation Motor Position Act   ";
            public static string WFUpperFaceDIWWash1FlowrateAct = "WF Upper Face DIW Wash1 Flowrate Act   ";
            public static string WFLowerFaceDIWWashFlowrateAct = "WF Lower Face DIW Wash Flowrate Act   ";
            public static string IPAWeighAct = "IPA Weigh Act   ";
            public static string IPAPressureAct = "IPA Pressure Act   ";
            public static string IPALiquidLevelAct = "IPA Liquid Level Act   ";
            //New 20251124
            public static string ChuckMotorServo = "Chuck Motor Servo";
            public static string ArmSweepMotorServo = "Arm Sweep Motor Servo";
        }
    }
}
