using CMP.Models.DataCenter;
using CMP.Models.kvsPlc;
using KHT = CMP.Models.kvsPlc.KvHostLink;
using KHST = CMP.Models.kvsPlc.Polish.KvStruct;
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
        KHSTCLEAN.stRollerCollectData _stRollerCollectData;
        public FieldInfo[] collectDataFields;
        public string rollerName = string.Empty;
        public FieldInfo[] ioDataFields;

        public Roller(string name, KVLink link) : base(name, link)
        {
            _stRollerCollectData = new KHSTCLEAN.stRollerCollectData();
            collectDataFields = typeof(KHSTSV.stRollerCollectData).GetFields();
            rollerName = name;
            ioDataFields = typeof(KHSTIO.stRollerIOData).GetFields();
        }

        public override void CollectData()
        {
            CollectProcessData();
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
                KHSTSV.stRollerCollectData _stRollerCollectData = new KHSTSV.stRollerCollectData();
                KHSTSV.ByteTo_stRollerCollectData(ref _stRollerCollectData, _readBuf);
                if (_stRollerCollectData != null)
                {
                    CompiledFieldAccessor<stRollerCollectData>.ProcessAllFields(_stRollerCollectData, (name, value) =>
                    {
                        SetSv(this.rollerName + name, value);
                    });
                }
            }
        }

        private void CollectProcessData()
        {
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTCLEAN.ByteTo_stRollerCollectData(ref _stRollerCollectData, readBuf);
                SetSv(Keys.RollerA1ProcessStepAct, _stRollerCollectData.RollerA1ProcessStepAct);
                SetSv(Keys.RollerA1ProcessStepSet, _stRollerCollectData.RollerA1ProcessStepSet);
                SetSv(Keys.RollerA1ProcessTypeAct, _stRollerCollectData.RollerA1ProcessTypeAct);
                SetSv(Keys.RollerA1ProcessTypeSet, _stRollerCollectData.RollerA1ProcessTypeSet);
                SetSv(Keys.RollerA1RecipeTimeAct, _stRollerCollectData.RollerA1RecipeTimeAct);
                SetSv(Keys.RollerA1RecipeTimeSet, _stRollerCollectData.RollerA1RecipeTimeSet);
                SetSv(Keys.RollerA1UpperRollerRotateVelocityAct, _stRollerCollectData.RollerA1UpperRollerRotateVelocityAct);
                SetSv(Keys.RollerA1UpperRollerRotateVelocitySet, _stRollerCollectData.RollerA1UpperRollerRotateVelocitySet);
                SetSv(Keys.RollerA1LowerRollerRotateVelocityAct, _stRollerCollectData.RollerA1LowerRollerRotateVelocityAct);
                SetSv(Keys.RollerA1LowerRollerRotateVelocitySet, _stRollerCollectData.RollerA1LowerRollerRotateVelocitySet);
                SetSv(Keys.RollerA1WFLeftPillarRotateMotorVelocityAct, _stRollerCollectData.RollerA1WFLeftPillarRotateMotorVelocityAct);
                SetSv(Keys.RollerA1WFLeftPillarRotateMotorVelocitySet, _stRollerCollectData.RollerA1WFLeftPillarRotateMotorVelocitySet);
                SetSv(Keys.RollerA1WFRightPillarRotateMotorVelocityAct, _stRollerCollectData.RollerA1WFRightPillarRotateMotorVelocityAct);
                SetSv(Keys.RollerA1WFRightPillarRotateMotorVelocitySet, _stRollerCollectData.RollerA1WFRightPillarRotateMotorVelocitySet);
                SetSv(Keys.RollerA1UpperRollerPressureAct, _stRollerCollectData.RollerA1UpperRollerPressureAct);
                SetSv(Keys.RollerA1UpperRollerPressureSet, _stRollerCollectData.RollerA1UpperRollerPressureSet);
                SetSv(Keys.RollerA1LowerRollerPressureAct, _stRollerCollectData.RollerA1LowerRollerPressureAct);
                SetSv(Keys.RollerA1LowerRollerPressureSet, _stRollerCollectData.RollerA1LowerRollerPressureSet);
                SetSv(Keys.RollerA1WFUpperFaceDIWFlowrateAct, _stRollerCollectData.RollerA1WFUpperFaceDIWFlowrateAct);
                SetSv(Keys.RollerA1WFUpperFaceDIWFlowrateSet, _stRollerCollectData.RollerA1WFUpperFaceDIWFlowrateSet);
                SetSv(Keys.RollerA1WFLowerFaceDIWFlowrateAct, _stRollerCollectData.RollerA1WFLowerFaceDIWFlowrateAct);
                SetSv(Keys.RollerA1WFLowerFaceDIWFlowrateSet, _stRollerCollectData.RollerA1WFLowerFaceDIWFlowrateSet);
                SetSv(Keys.RollerA1WFUpperFaceChemicalSupplyAct, _stRollerCollectData.RollerA1WFUpperFaceChemicalSupplyAct);
                SetSv(Keys.RollerA1WFUpperFaceChemicalSupplySet, _stRollerCollectData.RollerA1WFUpperFaceChemicalSupplySet);
                SetSv(Keys.RollerA1WFLowerFaceChemicalSupplyAct, _stRollerCollectData.RollerA1WFLowerFaceChemicalSupplyAct);
                SetSv(Keys.RollerA1WFLowerFaceChemicalSupplySet, _stRollerCollectData.RollerA1WFLowerFaceChemicalSupplySet);
                SetSv(Keys.RollerA1MegaSonicSupplyAct, _stRollerCollectData.RollerA1MegaSonicSupplyAct);
                SetSv(Keys.RollerA1MegaSonicSupplySet, _stRollerCollectData.RollerA1MegaSonicSupplySet);
                SetSv(Keys.RollerA1ChemicalEnterFlowratemeteAct, _stRollerCollectData.RollerA1ChemicalEnterFlowratemeteAct);
                SetSv(Keys.RollerA1ChemicalEnterFlowratemeteSet, _stRollerCollectData.RollerA1ChemicalEnterFlowratemeteSet);
                SetSv(Keys.RollerA1DIWEnterFlowratemeteAct, _stRollerCollectData.RollerA1DIWEnterFlowratemeteAct);
                SetSv(Keys.RollerA1DIWEnterFlowratemeteSet, _stRollerCollectData.RollerA1DIWEnterFlowratemeteSet);
                SetSv(Keys.RollerA1WFLeftPillarRotateMotorAngleAct, _stRollerCollectData.RollerA1WFLeftPillarRotateMotorAngleAct);
                SetSv(Keys.RollerA1WFRightPillarRotateMotorAngleAct, _stRollerCollectData.RollerA1WFRightPillarRotateMotorAngleAct);
                SetSv(Keys.RollerA1UpperRollerRotateMotorAngleAct, _stRollerCollectData.RollerA1UpperRollerRotateMotorAngleAct);
                SetSv(Keys.RollerA1LowerRollerRotateMotorAngleAct, _stRollerCollectData.RollerA1LowerRollerRotateMotorAngleAct);
                SetSv(Keys.RollerA1ChemicalEntrancePressureAct, _stRollerCollectData.RollerA1ChemicalEntrancePressureAct);
                SetSv(Keys.RollerA1WFLowerFaceChemicalWashPressureAct, _stRollerCollectData.RollerA1WFLowerFaceChemicalWashPressureAct);
                SetSv(Keys.RollerA1WFUpperFaceChemicalWashFlowrateAct, _stRollerCollectData.RollerA1WFUpperFaceChemicalWashFlowrateAct);
                SetSv(Keys.RollerA1QuartzChemicalWashFlowrateAct, _stRollerCollectData.RollerA1QuartzChemicalWashFlowrateAct);
                SetSv(Keys.RollerA1WFLowerFaceDIWWashPressureAct, _stRollerCollectData.RollerA1WFLowerFaceDIWWashPressureAct);
                SetSv(Keys.RollerA1UpperRollerDIWWashFlowrateAct, _stRollerCollectData.RollerA1UpperRollerDIWWashFlowrateAct);
                SetSv(Keys.RollerA1QuartzDIWWashFlowrateAct, _stRollerCollectData.RollerA1QuartzDIWWashFlowrateAct);
                SetSv(Keys.RollerA1UpperRollerInsideDIWWashFlowrateAct, _stRollerCollectData.RollerA1UpperRollerInsideDIWWashFlowrateAct);
                SetSv(Keys.RollerA1LowerRollerInsideDIWWashFlowrateAct, _stRollerCollectData.RollerA1LowerRollerInsideDIWWashFlowrateAct);
                SetSv(Keys.RollerA1MageSonicDIWWashFlowrateAct, _stRollerCollectData.RollerA1MageSonicDIWWashFlowrateAct);
                SetSv(Keys.RollerA1ChamberDIWWashFlowrateAct, _stRollerCollectData.RollerA1ChamberDIWWashFlowrateAct);
                SetSv(Keys.RollerA1PillarWashFlowrateAct, _stRollerCollectData.RollerA1PillarWashFlowrateAct);
                SetSv(Keys.RollerA1CoolingWaterCirculationFlowrateAct, _stRollerCollectData.RollerA1CoolingWaterCirculationFlowrateAct);
                SetSv(Keys.RollerA1LowerRollerDownPressureAct, _stRollerCollectData.RollerA1LowerRollerDownPressureAct);
                SetSv(Keys.RollerA1ClampCylinderPressureAct, _stRollerCollectData.RollerA1ClampCylinderPressureAct);
                SetSv(Keys.RollerA1UpperRollerPressureAct1, _stRollerCollectData.RollerA1UpperRollerPressureAct1);
                SetSv(Keys.RollerA1ClampPressureAct, _stRollerCollectData.RollerA1ClampPressureAct);
                SetSv(Keys.RollerA1ClampPressureSet, _stRollerCollectData.RollerA1ClampPressureSet);

                SetSv(Keys.RollerA2ProcessStepAct, _stRollerCollectData.RollerA2ProcessStepAct);
                SetSv(Keys.RollerA2ProcessStepSet, _stRollerCollectData.RollerA2ProcessStepSet);
                SetSv(Keys.RollerA2ProcessTypeAct, _stRollerCollectData.RollerA2ProcessTypeAct);
                SetSv(Keys.RollerA2ProcessTypeSet, _stRollerCollectData.RollerA2ProcessTypeSet);
                SetSv(Keys.RollerA2RecipeTimeAct, _stRollerCollectData.RollerA2RecipeTimeAct);
                SetSv(Keys.RollerA2RecipeTimeSet, _stRollerCollectData.RollerA2RecipeTimeSet);
                SetSv(Keys.RollerA2UpperRollerRotateVelocityAct, _stRollerCollectData.RollerA2UpperRollerRotateVelocityAct);
                SetSv(Keys.RollerA2UpperRollerRotateVelocitySet, _stRollerCollectData.RollerA2UpperRollerRotateVelocitySet);
                SetSv(Keys.RollerA2LowerRollerRotateVelocityAct, _stRollerCollectData.RollerA2LowerRollerRotateVelocityAct);
                SetSv(Keys.RollerA2LowerRollerRotateVelocitySet, _stRollerCollectData.RollerA2LowerRollerRotateVelocitySet);
                SetSv(Keys.RollerA2WFLeftPillarRotateMotorVelocityAct, _stRollerCollectData.RollerA2WFLeftPillarRotateMotorVelocityAct);
                SetSv(Keys.RollerA2WFLeftPillarRotateMotorVelocitySet, _stRollerCollectData.RollerA2WFLeftPillarRotateMotorVelocitySet);
                SetSv(Keys.RollerA2WFRightPillarRotateMotorVelocityAct, _stRollerCollectData.RollerA2WFRightPillarRotateMotorVelocityAct);
                SetSv(Keys.RollerA2WFRightPillarRotateMotorVelocitySet, _stRollerCollectData.RollerA2WFRightPillarRotateMotorVelocitySet);
                SetSv(Keys.RollerA2UpperRollerPressureAct, _stRollerCollectData.RollerA2UpperRollerPressureAct);
                SetSv(Keys.RollerA2UpperRollerPressureSet, _stRollerCollectData.RollerA2UpperRollerPressureSet);
                SetSv(Keys.RollerA2LowerRollerPressureAct, _stRollerCollectData.RollerA2LowerRollerPressureAct);
                SetSv(Keys.RollerA2LowerRollerPressureSet, _stRollerCollectData.RollerA2LowerRollerPressureSet);
                SetSv(Keys.RollerA2WFUpperFaceDIWFlowrateAct, _stRollerCollectData.RollerA2WFUpperFaceDIWFlowrateAct);
                SetSv(Keys.RollerA2WFUpperFaceDIWFlowrateSet, _stRollerCollectData.RollerA2WFUpperFaceDIWFlowrateSet);
                SetSv(Keys.RollerA2WFLowerFaceDIWFlowrateAct, _stRollerCollectData.RollerA2WFLowerFaceDIWFlowrateAct);
                SetSv(Keys.RollerA2WFLowerFaceDIWFlowrateSet, _stRollerCollectData.RollerA2WFLowerFaceDIWFlowrateSet);
                SetSv(Keys.RollerA2WFUpperFaceChemicalSupplyAct, _stRollerCollectData.RollerA2WFUpperFaceChemicalSupplyAct);
                SetSv(Keys.RollerA2WFUpperFaceChemicalSupplySet, _stRollerCollectData.RollerA2WFUpperFaceChemicalSupplySet);
                SetSv(Keys.RollerA2WFLowerFaceChemicalSupplyAct, _stRollerCollectData.RollerA2WFLowerFaceChemicalSupplyAct);
                SetSv(Keys.RollerA2WFLowerFaceChemicalSupplySet, _stRollerCollectData.RollerA2WFLowerFaceChemicalSupplySet);
                SetSv(Keys.RollerA2MegaSonicSupplyAct, _stRollerCollectData.RollerA2MegaSonicSupplyAct);
                SetSv(Keys.RollerA2MegaSonicSupplySet, _stRollerCollectData.RollerA2MegaSonicSupplySet);
                SetSv(Keys.RollerA2ChemicalEnterFlowratemeteAct, _stRollerCollectData.RollerA2ChemicalEnterFlowratemeteAct);
                SetSv(Keys.RollerA2ChemicalEnterFlowratemeteSet, _stRollerCollectData.RollerA2ChemicalEnterFlowratemeteSet);
                SetSv(Keys.RollerA2DIWEnterFlowratemeteAct, _stRollerCollectData.RollerA2DIWEnterFlowratemeteAct);
                SetSv(Keys.RollerA2DIWEnterFlowratemeteSet, _stRollerCollectData.RollerA2DIWEnterFlowratemeteSet);
                SetSv(Keys.RollerA2WFLeftPillarRotateMotorAngleAct, _stRollerCollectData.RollerA2WFLeftPillarRotateMotorAngleAct);
                SetSv(Keys.RollerA2WFRightPillarRotateMotorAngleAct, _stRollerCollectData.RollerA2WFRightPillarRotateMotorAngleAct);
                SetSv(Keys.RollerA2UpperRollerRotateMotorAngleAct, _stRollerCollectData.RollerA2UpperRollerRotateMotorAngleAct);
                SetSv(Keys.RollerA2LowerRollerRotateMotorAngleAct, _stRollerCollectData.RollerA2LowerRollerRotateMotorAngleAct);
                SetSv(Keys.RollerA2ChemicalEntrancePressureAct, _stRollerCollectData.RollerA2ChemicalEntrancePressureAct);
                SetSv(Keys.RollerA2WFLowerFaceChemicalWashPressureAct, _stRollerCollectData.RollerA2WFLowerFaceChemicalWashPressureAct);
                SetSv(Keys.RollerA2WFUpperFaceChemicalWashFlowrateAct, _stRollerCollectData.RollerA2WFUpperFaceChemicalWashFlowrateAct);
                SetSv(Keys.RollerA2QuartzChemicalWashFlowrateAct, _stRollerCollectData.RollerA2QuartzChemicalWashFlowrateAct);
                SetSv(Keys.RollerA2WFLowerFaceDIWWashPressureAct, _stRollerCollectData.RollerA2WFLowerFaceDIWWashPressureAct);
                SetSv(Keys.RollerA2UpperRollerDIWWashFlowrateAct, _stRollerCollectData.RollerA2UpperRollerDIWWashFlowrateAct);
                SetSv(Keys.RollerA2QuartzDIWWashFlowrateAct, _stRollerCollectData.RollerA2QuartzDIWWashFlowrateAct);
                SetSv(Keys.RollerA2UpperRollerInsideDIWWashFlowrateAct, _stRollerCollectData.RollerA2UpperRollerInsideDIWWashFlowrateAct);
                SetSv(Keys.RollerA2LowerRollerInsideDIWWashFlowrateAct, _stRollerCollectData.RollerA2LowerRollerInsideDIWWashFlowrateAct);
                SetSv(Keys.RollerA2MageSonicDIWWashFlowrateAct, _stRollerCollectData.RollerA2MageSonicDIWWashFlowrateAct);
                SetSv(Keys.RollerA2ChamberDIWWashFlowrateAct, _stRollerCollectData.RollerA2ChamberDIWWashFlowrateAct);
                SetSv(Keys.RollerA2PillarWashFlowrateAct, _stRollerCollectData.RollerA2PillarWashFlowrateAct);
                SetSv(Keys.RollerA2CoolingWaterCirculationFlowrateAct, _stRollerCollectData.RollerA2CoolingWaterCirculationFlowrateAct);
                SetSv(Keys.RollerA2LowerRollerDownPressureAct, _stRollerCollectData.RollerA2LowerRollerDownPressureAct);
                SetSv(Keys.RollerA2ClampCylinderPressureAct, _stRollerCollectData.RollerA2ClampCylinderPressureAct);
                SetSv(Keys.RollerA2UpperRollerPressureAct1, _stRollerCollectData.RollerA2UpperRollerPressureAct1);
                SetSv(Keys.RollerA2ClampPressureAct, _stRollerCollectData.RollerA2ClampPressureAct);
                SetSv(Keys.RollerA2ClampPressureSet, _stRollerCollectData.RollerA2ClampPressureSet);

                SetSv(Keys.RollerA3ProcessStepAct, _stRollerCollectData.RollerA3ProcessStepAct);
                SetSv(Keys.RollerA3ProcessStepSet, _stRollerCollectData.RollerA3ProcessStepSet);
                SetSv(Keys.RollerA3ProcessTypeAct, _stRollerCollectData.RollerA3ProcessTypeAct);
                SetSv(Keys.RollerA3ProcessTypeSet, _stRollerCollectData.RollerA3ProcessTypeSet);
                SetSv(Keys.RollerA3RecipeTimeAct, _stRollerCollectData.RollerA3RecipeTimeAct);
                SetSv(Keys.RollerA3RecipeTimeSet, _stRollerCollectData.RollerA3RecipeTimeSet);
                SetSv(Keys.RollerA3UpperRollerRotateVelocityAct, _stRollerCollectData.RollerA3UpperRollerRotateVelocityAct);
                SetSv(Keys.RollerA3UpperRollerRotateVelocitySet, _stRollerCollectData.RollerA3UpperRollerRotateVelocitySet);
                SetSv(Keys.RollerA3LowerRollerRotateVelocityAct, _stRollerCollectData.RollerA3LowerRollerRotateVelocityAct);
                SetSv(Keys.RollerA3LowerRollerRotateVelocitySet, _stRollerCollectData.RollerA3LowerRollerRotateVelocitySet);
                SetSv(Keys.RollerA3WFLeftPillarRotateMotorVelocityAct, _stRollerCollectData.RollerA3WFLeftPillarRotateMotorVelocityAct);
                SetSv(Keys.RollerA3WFLeftPillarRotateMotorVelocitySet, _stRollerCollectData.RollerA3WFLeftPillarRotateMotorVelocitySet);
                SetSv(Keys.RollerA3WFRightPillarRotateMotorVelocityAct, _stRollerCollectData.RollerA3WFRightPillarRotateMotorVelocityAct);
                SetSv(Keys.RollerA3WFRightPillarRotateMotorVelocitySet, _stRollerCollectData.RollerA3WFRightPillarRotateMotorVelocitySet);
                SetSv(Keys.RollerA3UpperRollerPressureAct, _stRollerCollectData.RollerA3UpperRollerPressureAct);
                SetSv(Keys.RollerA3UpperRollerPressureSet, _stRollerCollectData.RollerA3UpperRollerPressureSet);
                SetSv(Keys.RollerA3LowerRollerPressureAct, _stRollerCollectData.RollerA3LowerRollerPressureAct);
                SetSv(Keys.RollerA3LowerRollerPressureSet, _stRollerCollectData.RollerA3LowerRollerPressureSet);
                SetSv(Keys.RollerA3WFUpperFaceDIWFlowrateAct, _stRollerCollectData.RollerA3WFUpperFaceDIWFlowrateAct);
                SetSv(Keys.RollerA3WFUpperFaceDIWFlowrateSet, _stRollerCollectData.RollerA3WFUpperFaceDIWFlowrateSet);
                SetSv(Keys.RollerA3WFLowerFaceDIWFlowrateAct, _stRollerCollectData.RollerA3WFLowerFaceDIWFlowrateAct);
                SetSv(Keys.RollerA3WFLowerFaceDIWFlowrateSet, _stRollerCollectData.RollerA3WFLowerFaceDIWFlowrateSet);
                SetSv(Keys.RollerA3WFUpperFaceChemicalSupplyAct, _stRollerCollectData.RollerA3WFUpperFaceChemicalSupplyAct);
                SetSv(Keys.RollerA3WFUpperFaceChemicalSupplySet, _stRollerCollectData.RollerA3WFUpperFaceChemicalSupplySet);
                SetSv(Keys.RollerA3WFLowerFaceChemicalSupplyAct, _stRollerCollectData.RollerA3WFLowerFaceChemicalSupplyAct);
                SetSv(Keys.RollerA3WFLowerFaceChemicalSupplySet, _stRollerCollectData.RollerA3WFLowerFaceChemicalSupplySet);
                SetSv(Keys.RollerA3MegaSonicSupplyAct, _stRollerCollectData.RollerA3MegaSonicSupplyAct);
                SetSv(Keys.RollerA3MegaSonicSupplySet, _stRollerCollectData.RollerA3MegaSonicSupplySet);
                SetSv(Keys.RollerA3ChemicalEnterFlowratemeteAct, _stRollerCollectData.RollerA3ChemicalEnterFlowratemeteAct);
                SetSv(Keys.RollerA3ChemicalEnterFlowratemeteSet, _stRollerCollectData.RollerA3ChemicalEnterFlowratemeteSet);
                SetSv(Keys.RollerA3DIWEnterFlowratemeteAct, _stRollerCollectData.RollerA3DIWEnterFlowratemeteAct);
                SetSv(Keys.RollerA3DIWEnterFlowratemeteSet, _stRollerCollectData.RollerA3DIWEnterFlowratemeteSet);
                SetSv(Keys.RollerA3WFLeftPillarRotateMotorAngleAct, _stRollerCollectData.RollerA3WFLeftPillarRotateMotorAngleAct);
                SetSv(Keys.RollerA3WFRightPillarRotateMotorAngleAct, _stRollerCollectData.RollerA3WFRightPillarRotateMotorAngleAct);
                SetSv(Keys.RollerA3UpperRollerRotateMotorAngleAct, _stRollerCollectData.RollerA3UpperRollerRotateMotorAngleAct);
                SetSv(Keys.RollerA3LowerRollerRotateMotorAngleAct, _stRollerCollectData.RollerA3LowerRollerRotateMotorAngleAct);
                SetSv(Keys.RollerA3ChemicalEntrancePressureAct, _stRollerCollectData.RollerA3ChemicalEntrancePressureAct);
                SetSv(Keys.RollerA3WFLowerFaceChemicalWashPressureAct, _stRollerCollectData.RollerA3WFLowerFaceChemicalWashPressureAct);
                SetSv(Keys.RollerA3WFUpperFaceChemicalWashFlowrateAct, _stRollerCollectData.RollerA3WFUpperFaceChemicalWashFlowrateAct);
                SetSv(Keys.RollerA3QuartzChemicalWashFlowrateAct, _stRollerCollectData.RollerA3QuartzChemicalWashFlowrateAct);
                SetSv(Keys.RollerA3WFLowerFaceDIWWashPressureAct, _stRollerCollectData.RollerA3WFLowerFaceDIWWashPressureAct);
                SetSv(Keys.RollerA3UpperRollerDIWWashFlowrateAct, _stRollerCollectData.RollerA3UpperRollerDIWWashFlowrateAct);
                SetSv(Keys.RollerA3QuartzDIWWashFlowrateAct, _stRollerCollectData.RollerA3QuartzDIWWashFlowrateAct);
                SetSv(Keys.RollerA3UpperRollerInsideDIWWashFlowrateAct, _stRollerCollectData.RollerA3UpperRollerInsideDIWWashFlowrateAct);
                SetSv(Keys.RollerA3LowerRollerInsideDIWWashFlowrateAct, _stRollerCollectData.RollerA3LowerRollerInsideDIWWashFlowrateAct);
                SetSv(Keys.RollerA3MageSonicDIWWashFlowrateAct, _stRollerCollectData.RollerA3MageSonicDIWWashFlowrateAct);
                SetSv(Keys.RollerA3ChamberDIWWashFlowrateAct, _stRollerCollectData.RollerA3ChamberDIWWashFlowrateAct);
                SetSv(Keys.RollerA3PillarWashFlowrateAct, _stRollerCollectData.RollerA3PillarWashFlowrateAct);
                SetSv(Keys.RollerA3CoolingWaterCirculationFlowrateAct, _stRollerCollectData.RollerA3CoolingWaterCirculationFlowrateAct);
                SetSv(Keys.RollerA3LowerRollerDownPressureAct, _stRollerCollectData.RollerA3LowerRollerDownPressureAct);
                SetSv(Keys.RollerA3ClampCylinderPressureAct, _stRollerCollectData.RollerA3ClampCylinderPressureAct);
                SetSv(Keys.RollerA3UpperRollerPressureAct1, _stRollerCollectData.RollerA3UpperRollerPressureAct1);
                SetSv(Keys.RollerA3ClampPressureAct, _stRollerCollectData.RollerA3ClampPressureAct);
                SetSv(Keys.RollerA3ClampPressureSet, _stRollerCollectData.RollerA3ClampPressureSet);
            }
        }

        private void CollectIOData1()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "IOData", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTIO.stRollerIOData _stRollerIOData = new KHSTIO.stRollerIOData();
                KHSTIO.ByteTo_stRollerIOData(ref _stRollerIOData, _readBuf);
                if (_stRollerIOData != null)
                {
                    CompiledFieldAccessor<stRollerIOData>.ProcessAllFields(_stRollerIOData, (name, value) =>
                    {
                        SetSv(this.rollerName + name, value);
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
        KHSTCLEAN.stPenCollectData _stPenCollectData;
        public FieldInfo[] collectDataFields;
        public string penName = string.Empty;
        public FieldInfo[] ioDataFields;
        public Pen(string name, KVLink link) : base(name, link)
        {
            _stPenCollectData = new KHSTCLEAN.stPenCollectData();
            collectDataFields = typeof(KHSTSV.stPenCollectData).GetFields();
            penName = name;
            ioDataFields = typeof(KHSTIO.stPenIOData).GetFields();
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
                KHSTCLEAN.ByteTo_stPenCollectData(ref _stPenCollectData, readBuf);
                SetSv(Keys.PenA1ProcessStepAct, _stPenCollectData.PenA1ProcessStepAct);
                SetSv(Keys.PenA1ProcessStepSet, _stPenCollectData.PenA1ProcessStepSet);
                SetSv(Keys.PenA1ProcessTypeAct, _stPenCollectData.PenA1ProcessTypeAct);
                SetSv(Keys.PenA1ProcessTypeSet, _stPenCollectData.PenA1ProcessTypeSet);
                SetSv(Keys.PenA1RecipeTimeAct, _stPenCollectData.PenA1RecipeTimeAct);
                SetSv(Keys.PenA1RecipeTimeSet, _stPenCollectData.PenA1RecipeTimeSet);
                SetSv(Keys.PenA1ChuckRotationMotorVelocityAct, _stPenCollectData.PenA1ChuckRotationMotorVelocityAct);
                SetSv(Keys.PenA1ChuckRotationMotorVelocitySet, _stPenCollectData.PenA1ChuckRotationMotorVelocitySet);
                SetSv(Keys.PenA1PenArmSweepMotorVelocityAct, _stPenCollectData.PenA1PenArmSweepMotorVelocityAct);
                SetSv(Keys.PenA1PenArmSweepMotorVelocitySet, _stPenCollectData.PenA1PenArmSweepMotorVelocitySet);
                SetSv(Keys.PenA1PenArmSweepMotorPositionAct, _stPenCollectData.PenA1PenArmSweepMotorPositionAct);
                SetSv(Keys.PenA1PenArmSweepMotorPositionSet, _stPenCollectData.PenA1PenArmSweepMotorPositionSet);
                SetSv(Keys.PenA1PenArmSweepMotorCycleAct, _stPenCollectData.PenA1PenArmSweepMotorCycleAct);
                SetSv(Keys.PenA1PenArmSweepMotorCycleSet, _stPenCollectData.PenA1PenArmSweepMotorCycleSet);
                SetSv(Keys.PenA1PenRotationMotorVelocityAct, _stPenCollectData.PenA1PenRotationMotorVelocityAct);
                SetSv(Keys.PenA1PenRotationMotorVelocitySet, _stPenCollectData.PenA1PenRotationMotorVelocitySet);
                SetSv(Keys.PenA1PenPressureSetValueAct, _stPenCollectData.PenA1PenPressureSetValueAct);
                SetSv(Keys.PenA1PenPressureSetValueSet, _stPenCollectData.PenA1PenPressureSetValueSet);
                SetSv(Keys.PenA1WFUpperFaceN2OutletFlowrateAct, _stPenCollectData.PenA1WFUpperFaceN2OutletFlowrateAct);
                SetSv(Keys.PenA1WFUpperFaceN2OutletFlowrateSet, _stPenCollectData.PenA1WFUpperFaceN2OutletFlowrateSet);
                SetSv(Keys.PenA1WFUpperFaceDIWFlowrateAct, _stPenCollectData.PenA1WFUpperFaceDIWFlowrateAct);
                SetSv(Keys.PenA1WFUpperFaceDIWFlowrateSet, _stPenCollectData.PenA1WFUpperFaceDIWFlowrateSet);
                SetSv(Keys.PenA1WFLowerFaceDIWFlowrateAct, _stPenCollectData.PenA1WFLowerFaceDIWFlowrateAct);
                SetSv(Keys.PenA1WFLowerFaceDIWFlowrateSet, _stPenCollectData.PenA1WFLowerFaceDIWFlowrateSet);
                SetSv(Keys.PenA1WFLowerFaceN2SupplyAct, _stPenCollectData.PenA1WFLowerFaceN2SupplyAct);
                SetSv(Keys.PenA1WFLowerFaceN2SupplySet, _stPenCollectData.PenA1WFLowerFaceN2SupplySet);
                SetSv(Keys.PenA1WFUpperLowerFaceChemicalSupplyAct, _stPenCollectData.PenA1WFUpperLowerFaceChemicalSupplyAct);
                SetSv(Keys.PenA1WFUpperLowerFaceChemicalSupplySet, _stPenCollectData.PenA1WFUpperLowerFaceChemicalSupplySet);
                SetSv(Keys.PenA1WFUpperFaceChemicalSupplyAct, _stPenCollectData.PenA1WFUpperFaceChemicalSupplyAct);
                SetSv(Keys.PenA1WFUpperFaceChemicalSupplySet, _stPenCollectData.PenA1WFUpperFaceChemicalSupplySet);
                SetSv(Keys.PenA1WFUpperFaceDICO2SupplyAct, _stPenCollectData.PenA1WFUpperFaceDICO2SupplyAct);
                SetSv(Keys.PenA1WFUpperFaceDICO2SupplySet, _stPenCollectData.PenA1WFUpperFaceDICO2SupplySet);
                SetSv(Keys.PenA1DIWInletFlowrateAct, _stPenCollectData.PenA1DIWInletFlowrateAct);
                SetSv(Keys.PenA1DIWInletFlowrateSet, _stPenCollectData.PenA1DIWInletFlowrateSet);
                SetSv(Keys.PenA1DIWSupplyPressureAct, _stPenCollectData.PenA1DIWSupplyPressureAct);
                SetSv(Keys.PenA1DIWSupplyPressureSet, _stPenCollectData.PenA1DIWSupplyPressureSet);
                SetSv(Keys.PenA1ChemicalFlowrateAct, _stPenCollectData.PenA1ChemicalFlowrateAct);
                SetSv(Keys.PenA1ChemicalFlowrateSet, _stPenCollectData.PenA1ChemicalFlowrateSet);
                SetSv(Keys.PenA1CO2SupplyPressureAct, _stPenCollectData.PenA1CO2SupplyPressureAct);
                SetSv(Keys.PenA1CO2SupplyPressureSet, _stPenCollectData.PenA1CO2SupplyPressureSet);
                SetSv(Keys.PenA1ChuckRotationMotorPositionAct, _stPenCollectData.PenA1ChuckRotationMotorPositionAct1);
                SetSv(Keys.PenA1PenRotationMotorPositionAct, _stPenCollectData.PenA1PenRotationMotorPositionAct);
                SetSv(Keys.PenA1DIWSupplyPressureAct1, _stPenCollectData.PenA1DIWSupplyPressureAct1);
                SetSv(Keys.PenA1CarbonatedDIWResistanceAct, _stPenCollectData.PenA1CarbonatedDIWResistanceAct);
                SetSv(Keys.PenA1CarbonatedDIWFlowrateAct, _stPenCollectData.PenA1CarbonatedDIWFlowrateAct);
                SetSv(Keys.PenA1DIWChamberFlowrateAct, _stPenCollectData.PenA1DIWChamberFlowrateAct);
                SetSv(Keys.PenA1PenDIWWashFlowrateAct, _stPenCollectData.PenA1PenDIWWashFlowrateAct);
                SetSv(Keys.PenA1QuartzDIWWashFlowrateAct, _stPenCollectData.PenA1QuartzDIWWashFlowrateAct);
                SetSv(Keys.PenA1ChemicalSupplyPressureAct, _stPenCollectData.PenA1ChemicalSupplyPressureAct);
                SetSv(Keys.PenA1QuartzChemicalWashFlowrateAct, _stPenCollectData.PenA1QuartzChemicalWashFlowrateAct);
                SetSv(Keys.PenA1WFUpperFaceWashChemicalFlowrateAct, _stPenCollectData.PenA1WFUpperFaceWashChemicalFlowrateAct);
                SetSv(Keys.PenA1WFLowerFaceWashChemicalFlowrateAct, _stPenCollectData.PenA1WFLowerFaceWashChemicalFlowrateAct);
                SetSv(Keys.PenA1WFUpperFaceN2PressureAct, _stPenCollectData.PenA1WFUpperFaceN2PressureAct);
                SetSv(Keys.PenA1WFLowerFaceN2PressureAct, _stPenCollectData.PenA1WFLowerFaceN2PressureAct);
                SetSv(Keys.PenA1WFLowerFaceN2OutletFlowAct, _stPenCollectData.PenA1WFLowerFaceN2OutletFlowAct1);
                SetSv(Keys.PenA1CUPPurgeFlowAct, _stPenCollectData.PenA1CUPPurgeFlowAct);
                SetSv(Keys.PenA1CO2SupplyPressureAct1, _stPenCollectData.PenA1CO2SupplyPressureAct1);
                SetSv(Keys.PenA2ProcessStepAct, _stPenCollectData.PenA2ProcessStepAct);
                SetSv(Keys.PenA2ProcessStepSet, _stPenCollectData.PenA2ProcessStepSet);
                SetSv(Keys.PenA2ProcessTypeAct, _stPenCollectData.PenA2ProcessTypeAct);
                SetSv(Keys.PenA2ProcessTypeSet, _stPenCollectData.PenA2ProcessTypeSet);
                SetSv(Keys.PenA2RecipeTimeAct, _stPenCollectData.PenA2RecipeTimeAct);
                SetSv(Keys.PenA2RecipeTimeSet, _stPenCollectData.PenA2RecipeTimeSet);
                SetSv(Keys.PenA2ChuckRotationMotorVelocityAct, _stPenCollectData.PenA2ChuckRotationMotorVelocityAct);
                SetSv(Keys.PenA2ChuckRotationMotorVelocitySet, _stPenCollectData.PenA2ChuckRotationMotorVelocitySet);
                SetSv(Keys.PenA2PenArmSweepMotorVelocityAct, _stPenCollectData.PenA2PenArmSweepMotorVelocityAct);
                SetSv(Keys.PenA2PenArmSweepMotorVelocitySet, _stPenCollectData.PenA2PenArmSweepMotorVelocitySet);
                SetSv(Keys.PenA2PenArmSweepMotorPositionAct, _stPenCollectData.PenA2PenArmSweepMotorPositionAct);
                SetSv(Keys.PenA2PenArmSweepMotorPositionSet, _stPenCollectData.PenA2PenArmSweepMotorPositionSet);
                SetSv(Keys.PenA2PenArmSweepMotorCycleAct, _stPenCollectData.PenA2PenArmSweepMotorCycleAct);
                SetSv(Keys.PenA2PenArmSweepMotorCycleSet, _stPenCollectData.PenA2PenArmSweepMotorCycleSet);
                SetSv(Keys.PenA2PenRotationMotorVelocityAct, _stPenCollectData.PenA2PenRotationMotorVelocityAct);
                SetSv(Keys.PenA2PenRotationMotorVelocitySet, _stPenCollectData.PenA2PenRotationMotorVelocitySet);
                SetSv(Keys.PenA2PenPressureSetValueAct, _stPenCollectData.PenA2PenPressureSetValueAct);
                SetSv(Keys.PenA2PenPressureSetValueSet, _stPenCollectData.PenA2PenPressureSetValueSet);
                SetSv(Keys.PenA2WFUpperFaceN2OutletFlowrateAct, _stPenCollectData.PenA2WFUpperFaceN2OutletFlowrateAct);
                SetSv(Keys.PenA2WFUpperFaceN2OutletFlowrateSet, _stPenCollectData.PenA2WFUpperFaceN2OutletFlowrateSet);
                SetSv(Keys.PenA2WFUpperFaceDIWFlowrateAct, _stPenCollectData.PenA2WFUpperFaceDIWFlowrateAct);
                SetSv(Keys.PenA2WFUpperFaceDIWFlowrateSet, _stPenCollectData.PenA2WFUpperFaceDIWFlowrateSet);
                SetSv(Keys.PenA2WFLowerFaceDIWFlowrateAct, _stPenCollectData.PenA2WFLowerFaceDIWFlowrateAct);
                SetSv(Keys.PenA2WFLowerFaceDIWFlowrateSet, _stPenCollectData.PenA2WFLowerFaceDIWFlowrateSet);
                SetSv(Keys.PenA2WFLowerFaceN2SupplyAct, _stPenCollectData.PenA2WFLowerFaceN2SupplyAct);
                SetSv(Keys.PenA2WFLowerFaceN2SupplySet, _stPenCollectData.PenA2WFLowerFaceN2SupplySet);
                SetSv(Keys.PenA2WFUpperLowerFaceChemicalSupplyAct, _stPenCollectData.PenA2WFUpperLowerFaceChemicalSupplyAct);
                SetSv(Keys.PenA2WFUpperLowerFaceChemicalSupplySet, _stPenCollectData.PenA2WFUpperLowerFaceChemicalSupplySet);
                SetSv(Keys.PenA2WFUpperFaceChemicalSupplyAct, _stPenCollectData.PenA2WFUpperFaceChemicalSupplyAct);
                SetSv(Keys.PenA2WFUpperFaceChemicalSupplySet, _stPenCollectData.PenA2WFUpperFaceChemicalSupplySet);
                SetSv(Keys.PenA2WFUpperFaceDICO2SupplyAct, _stPenCollectData.PenA2WFUpperFaceDICO2SupplyAct);
                SetSv(Keys.PenA2WFUpperFaceDICO2SupplySet, _stPenCollectData.PenA2WFUpperFaceDICO2SupplySet);
                SetSv(Keys.PenA2DIWInletFlowrateAct, _stPenCollectData.PenA2DIWInletFlowrateAct);
                SetSv(Keys.PenA2DIWInletFlowrateSet, _stPenCollectData.PenA2DIWInletFlowrateSet);
                SetSv(Keys.PenA2DIWSupplyPressureAct, _stPenCollectData.PenA2DIWSupplyPressureAct);
                SetSv(Keys.PenA2DIWSupplyPressureSet, _stPenCollectData.PenA2DIWSupplyPressureSet);
                SetSv(Keys.PenA2ChemicalFlowrateAct, _stPenCollectData.PenA2ChemicalFlowrateAct);
                SetSv(Keys.PenA2ChemicalFlowrateSet, _stPenCollectData.PenA2ChemicalFlowrateSet);
                SetSv(Keys.PenA2CO2SupplyPressure1Act, _stPenCollectData.PenA2CO2SupplyPressure1Act);
                SetSv(Keys.PenA2CO2SupplyPressure1Set, _stPenCollectData.PenA2CO2SupplyPressure1Set);
                SetSv(Keys.PenA2ChuckRotationMotorPositionAct, _stPenCollectData.PenA2ChuckRotationMotorPositionAct);
                SetSv(Keys.PenA2PenRotationMotorPositionAct, _stPenCollectData.PenA2PenRotationMotorPositionAct);
                SetSv(Keys.PenA2DIWSupplyPressureAct1, _stPenCollectData.PenA2DIWSupplyPressureAct1);
                SetSv(Keys.PenA2CarbonatedDIWResistanceAct, _stPenCollectData.PenA2CarbonatedDIWResistanceAct);
                SetSv(Keys.PenA2CarbonatedDIWFlowrateAct, _stPenCollectData.PenA2CarbonatedDIWFlowrateAct);
                SetSv(Keys.PenA2DIWChamberFlowrateAct, _stPenCollectData.PenA2DIWChamberFlowrateAct);
                SetSv(Keys.PenA2PenDIWWashFlowrateAct, _stPenCollectData.PenA2PenDIWWashFlowrateAct);
                SetSv(Keys.PenA2QuartzDIWWashFlowrateAct, _stPenCollectData.PenA2QuartzDIWWashFlowrateAct);
                SetSv(Keys.PenA2ChemicalSupplyPressureAct, _stPenCollectData.PenA2ChemicalSupplyPressureAct);
                SetSv(Keys.PenA2QuartzChemicalWashFlowrateAct, _stPenCollectData.PenA2QuartzChemicalWashFlowrateAct);
                SetSv(Keys.PenA2WFUpperFaceWashChemicalFlowrateAct, _stPenCollectData.PenA2WFUpperFaceWashChemicalFlowrateAct);
                SetSv(Keys.PenA2WFLowerFaceWashChemicalFlowrateAct, _stPenCollectData.PenA2WFLowerFaceWashChemicalFlowrateAct);
                SetSv(Keys.PenA2WFUpperFaceN2PressureAct, _stPenCollectData.PenA2WFUpperFaceN2PressureAct);
                SetSv(Keys.PenA2WFLowerFaceN2PressureAct, _stPenCollectData.PenA2WFLowerFaceN2PressureAct);
                SetSv(Keys.PenA2WFLowerFaceN2OutletFlowAct, _stPenCollectData.PenA2WFLowerFaceN2OutletFlowAct);
                SetSv(Keys.PenA2CUPPurgeFlowAct, _stPenCollectData.PenA2CUPPurgeFlowAct);
                SetSv(Keys.PenA2CO2SupplyPressure2Act, _stPenCollectData.PenA2CO2SupplyPressure2Act);

            }
        }

        private void CollectProcessData1()
        {
            byte[] _readBuf = new byte[2048];
            err = KHT.KHLReadVariable(kvLink.sock, "dataCollection", _readBuf, true);
            if (err == (int)KHLUserError.KHL_ERR_NONE)
            {
                KHSTSV.stPenCollectData _stPenCollectData = new KHSTSV.stPenCollectData();
                KHSTSV.ByteTo_stPenCollectData(ref _stPenCollectData, _readBuf);
                if (_stPenCollectData != null)
                {
                    CompiledFieldAccessor<stPenCollectData>.ProcessAllFields(_stPenCollectData, (name, value) =>
                    {
                        SetSv(this.penName + name, value);
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
                KHSTIO.stPenIOData _stPenIOData = new KHSTIO.stPenIOData();
                KHSTIO.ByteTo_stPenIOData(ref _stPenIOData, _readBuf);
                if (_stPenIOData != null)
                {
                    CompiledFieldAccessor<stPenIOData>.ProcessAllFields(_stPenIOData, (name, value) =>
                    {
                        SetSv(this.penName + name, value);
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
