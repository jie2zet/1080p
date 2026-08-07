using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.kvsPlc.SV
{
    class KvStruct
    {
        const int stDryCollectData_WSIZE = 64;
        const int stPenCollectData_WSIZE = 216;
        const int stPolishCollectData_WSIZE = 304;
        const int stRollerCollectData_WSIZE = 324;


        public class stDryCollectData
        {
            public short ProcessStepAct = default(short);
            public short ProcessStepSet = default(short);
            public short ProcessTypeAct = default(short);
            public short ProcessTypeSet = default(short);
            public float RecipeTimeAct = default(float);
            public float RecipeTimeSet = default(float);
            public float ChuckRotationMotorVelocityValuAct = default(float);
            public float ChuckRotationMotorVelocityValuSet = default(float);
            public float ArmSweepMotorVelocityAct = default(float);
            public float ArmSweepMotorVelocitySet = default(float);
            public float ArmSweepMotorPositionAct = default(float);
            public float ArmSweepMotorPositionSet = default(float);
            public short ArmSweepMotorCycleAct = default(short);
            public short ArmSweepMotorCycleSet = default(short);
            public float WFUpperFaceN2PuegeFlowrateAct = default(float);
            public float WFUpperFaceN2PuegeFlowrateSet = default(float);
            public float WFLowerFaceN2Puege1FlowrateAct = default(float);
            public float WFLowerFaceN2Puege1FlowrateSet = default(float);
            public float WFUpperFaceDIWWashInlet2FlowrateAct = default(float);
            public float WFUpperFaceDIWWashInlet2FlowrateSet = default(float);
            public short WFUpperFaceIPASupplyAct = default(short);
            public short WFUpperFaceIPASupplySet = default(short);
            public short WFLowerFaceN2Supply2Act = default(short);
            public short WFLowerFaceN2Supply2Set = default(short);
            public short WFUpperFaceDIWSupply1Act = default(short);
            public short WFUpperFaceDIWSupply1Set = default(short);
            public short WFLowerFaceDIWSupplyAct = default(short);
            public short WFLowerFaceDIWSupplySet = default(short);
            public float N2MixOutletFlowrateAct = default(float);
            public float N2MixOutletFlowrateSet = default(float);
            public float ChuckRotationMotorPositionAct = default(float);
            public float ChuckRotationMotorTorqueActualValueAct = default(float);
            public float ArmSweepMotorTorqueActualValueAct = default(float);
            public float WFUpperFaceDIWWash1FlowrateAct = default(float);
            public float WFLowerFaceDIWWashFlowrateAct = default(float);
            public float IPAWeighAct = default(float);
            public float IPAPressureAct = default(float);
            public float IPALiquidLevelAct = default(float);
            public bool ChuckMotorServoOn = default(bool);
            public bool ArmSweepMotorServoOn = default(bool);
        }

        public class stPenCollectData
        {
            public short _1ProcessStepAct = default(short);
            public short _1ProcessStepSet = default(short);
            public short _1ProcessTypeAct = default(short);
            public short _1ProcessTypeSet = default(short);
            public float _1RecipeTimeAct = default(float);
            public float _1RecipeTimeSet = default(float);
            public float _1ChuckRotationMotorVelocityAct = default(float);
            public float _1ChuckRotationMotorVelocitySet = default(float);
            public float _1PenArmSweepMotorVelocityAct = default(float);
            public float _1PenArmSweepMotorVelocitySet = default(float);
            public float _1PenArmSweepMotorPositionAct = default(float);
            public float _1PenArmSweepMotorPositionSet = default(float);
            public short _1PenArmSweepMotorCycleAct = default(short);
            public short _1PenArmSweepMotorCycleSet = default(short);
            public float _1PenRotationMotorVelocityAct = default(float);
            public float _1PenRotationMotorVelocitySet = default(float);
            public float _1PenPressureSetValueAct = default(float);
            public float _1PenPressureSetValueSet = default(float);
            public float _1WFUpperFaceN2OutletFlowrateAct = default(float);
            public float _1WFUpperFaceN2OutletFlowrateSet = default(float);
            public float _1WFUpperFaceDIWFlowrateAct = default(float);
            public float _1WFUpperFaceDIWFlowrateSet = default(float);
            public float _1WFLowerFaceDIWFlowrateAct = default(float);
            public float _1WFLowerFaceDIWFlowrateSet = default(float);
            public short _1WFLowerFaceN2SupplyAct = default(short);
            public short _1WFLowerFaceN2SupplySet = default(short);
            public short _1WFUpperLowerFaceChemicalSupplyAct = default(short);
            public short _1WFUpperLowerFaceChemicalSupplySet = default(short);
            public short _1WFUpperFaceChemicalSupplyAct = default(short);
            public short _1WFUpperFaceChemicalSupplySet = default(short);
            public short _1WFUpperFaceDICO2SupplyAct = default(short);
            public short _1WFUpperFaceDICO2SupplySet = default(short);
            public float _1DIWInletFlowrateAct = default(float);
            public float _1DIWInletFlowrateSet = default(float);
            public float _1DIWSupplyPressureAct = default(float);
            public float _1DIWSupplyPressureSet = default(float);
            public float _1ChemicalFlowrateAct = default(float);
            public float _1ChemicalFlowrateSet = default(float);
            public float _1CO2SupplyPressureAct = default(float);
            public float _1CO2SupplyPressureSet = default(float);
            public float _1ChuckRotationMotorPositionAct = default(float);
            public float _1PenRotationMotorPositionAct = default(float);
            public float _1DIWSupplyPressure1Act = default(float);
            public float _1CarbonatedDIWResistanceAct = default(float);
            public float _1CarbonatedDIWFlowrateAct = default(float);
            public float _1DIWChamberFlowrateAct = default(float);
            public float _1PenDIWWashFlowrateAct = default(float);
            public float _1QuartzDIWWashFlowrateAct = default(float);
            public float _1ChemicalSupplyPressureAct = default(float);
            public float _1QuartzChemicalWashFlowrateAct = default(float);
            public float _1WFUpperFaceWashChemicalFlowrateAct = default(float);
            public float _1WFLowerFaceWashChemicalFlowrateAct = default(float);
            public float _1WFUpperFaceN2PressureAct = default(float);
            public float _1WFLowerFaceN2PressureAct = default(float);
            public float _1WFLowerFaceN2OutletFlowAct = default(float);
            public float _1CUPPurgeFlowAct = default(float);
            public float _1CO2SupplyPressure1Act = default(float);
            public float _1ChuckRotationMotorTorqueActualValueAct = default(float);
            public float _1ArmMotorTorqueActualValueAct = default(float);
            public float _1PenRotationMotorTorqueActualValueAct = default(float);
            public bool _1ChuckMotorServoOn = default(bool);
            public bool _1PenArmSweepMotorServoOn = default(bool);
            public bool _1PenRotationMotorServoOn = default(bool);
            public short _2ProcessStepAct = default(short);
            public short _2ProcessStepSet = default(short);
            public short _2ProcessTypeAct = default(short);
            public short _2ProcessTypeSet = default(short);
            public float _2RecipeTimeAct = default(float);
            public float _2RecipeTimeSet = default(float);
            public float _2ChuckRotationMotorVelocityAct = default(float);
            public float _2ChuckRotationMotorVelocitySet = default(float);
            public float _2PenArmSweepMotorVelocityAct = default(float);
            public float _2PenArmSweepMotorVelocitySet = default(float);
            public float _2PenArmSweepMotorPositionAct = default(float);
            public float _2PenArmSweepMotorPositionSet = default(float);
            public short _2PenArmSweepMotorCycleAct = default(short);
            public short _2PenArmSweepMotorCycleSet = default(short);
            public float _2PenRotationMotorVelocityAct = default(float);
            public float _2PenRotationMotorVelocitySet = default(float);
            public float _2PenPressureSetValueAct = default(float);
            public float _2PenPressureSetValueSet = default(float);
            public float _2WFUpperFaceN2OutletFlowrateAct = default(float);
            public float _2WFUpperFaceN2OutletFlowrateSet = default(float);
            public float _2WFUpperFaceDIWFlowrateAct = default(float);
            public float _2WFUpperFaceDIWFlowrateSet = default(float);
            public float _2WFLowerFaceDIWFlowrateAct = default(float);
            public float _2WFLowerFaceDIWFlowrateSet = default(float);
            public short _2WFLowerFaceN2SupplyAct = default(short);
            public short _2WFLowerFaceN2SupplySet = default(short);
            public short _2WFUpperLowerFaceChemicalSupplyAct = default(short);
            public short _2WFUpperLowerFaceChemicalSupplySet = default(short);
            public short _2WFUpperFaceChemicalSupplyAct = default(short);
            public short _2WFUpperFaceChemicalSupplySet = default(short);
            public short _2WFUpperFaceDICO2SupplyAct = default(short);
            public short _2WFUpperFaceDICO2SupplySet = default(short);
            public float _2DIWInletFlowrateAct = default(float);
            public float _2DIWInletFlowrateSet = default(float);
            public float _2DIWSupplyPressureAct = default(float);
            public float _2DIWSupplyPressureSet = default(float);
            public float _2ChemicalFlowrateAct = default(float);
            public float _2ChemicalFlowrateSet = default(float);
            public float _2CO2SupplyPressureAct = default(float);
            public float _2CO2SupplyPressureSet = default(float);
            public float _2ChuckRotationMotorPositionAct = default(float);
            public float _2PenRotationMotorPositionAct = default(float);
            public float _2DIWSupplyPressure1Act = default(float);
            public float _2CarbonatedDIWResistanceAct = default(float);
            public float _2CarbonatedDIWFlowrateAct = default(float);
            public float _2DIWChamberFlowrateAct = default(float);
            public float _2PenDIWWashFlowrateAct = default(float);
            public float _2QuartzDIWWashFlowrateAct = default(float);
            public float _2ChemicalSupplyPressureAct = default(float);
            public float _2QuartzChemicalWashFlowrateAct = default(float);
            public float _2WFUpperFaceWashChemicalFlowrateAct = default(float);
            public float _2WFLowerFaceWashChemicalFlowrateAct = default(float);
            public float _2WFUpperFaceN2PressureAct = default(float);
            public float _2WFLowerFaceN2PressureAct = default(float);
            public float _2WFLowerFaceN2OutletFlowAct = default(float);
            public float _2CUPPurgeFlowAct = default(float);
            public float _2CO2SupplyPressure1Act = default(float);
            public float _2ChuckRotationMotorTorqueActualValueAct = default(float);
            public float _2ArmMotorTorqueActualValueAct = default(float);
            public float _2PenRotationMotorTorqueActualValueAct = default(float);
            public bool _2ChuckMotorServoOn = default(bool);
            public bool _2PenArmSweepMotorServoOn = default(bool);
            public bool _2PenRotationMotorServoOn = default(bool);
        }

        public class stPolishCollectData
        {
            public short ProcessStepAct = default(short);
            public short ProcessStepSet = default(short);
            public short ProcessTypeAct = default(short);
            public short ProcessTypeSet = default(short);
            public float RecipeTimeAct = default(float);
            public float RecipeTimeSet = default(float);
            public short ExsituConditionProcessStepAct = default(short);
            public short ExsituConditionProcessStepSet = default(short);
            public float ExsituConditionRecipeTimeAct = default(float);
            public float ExsituConditionRecipeTimeSet = default(float);
            public float MainPolishRecipeTimeAct = default(float);
            public float MainPolishRecipeTimeSet = default(float);
            public float MainPolishCumulativeTimeAct = default(float);
            public float MainPolishCumulativeTimeSet = default(float);
            public float PlatenRotationSpeedAct = default(float);
            public float PlatenRotationSpeedSet = default(float);
            public float HeadRotationSpeedAct = default(float);
            public float HeadRotationSpeedSet = default(float);
            public float HeadSweepSpeedAct = default(float);
            public float HeadSweepSpeedSet = default(float);
            public float HeadSweepPositionAct = default(float);
            public float HeadSweepPositionSet = default(float);
            public short HeadSweepFrequencyAct = default(short);
            public short HeadSweepFrequencySet = default(short);
            public float HeadZ_AxisSpeedAct = default(float);
            public float HeadZ_AxisSpeedSet = default(float);
            public float HeadZ_AxisPositionAct = default(float);
            public float HeadZ_AxisPositionSet = default(float);
            public float ConditionRotationSpeedAct = default(float);
            public float ConditionRotationSpeedSet = default(float);
            public float ConditionArmSweepSpeedAct = default(float);
            public float ConditionArmSweepSpeedSet = default(float);
            public float ConditionArmSweepPositionAct = default(float);
            public float ConditionArmSweepPositionSet = default(float);
            public short ConditionArmSweepFrequencyAct = default(short);
            public short ConditionArmSweepFrequencySet = default(short);
            public short SlurryLine1TypeAct = default(short);
            public short SlurryLine2TypeAct = default(short);
            public short SlurryLine3TypeAct = default(short);
            public float Slurry1OrDIWFlowrateAct = default(float);
            public float Slurry1OrDIWFlowrateSet = default(float);
            public float Slurry2OrDIWFlowrateAct = default(float);
            public float Slurry2OrDIWFlowrateSet = default(float);
            public float Slurry3OrDIWFlowrateAct = default(float);
            public float Slurry3OrDIWFlowrateSet = default(float);
            public short Zone1TypeAct = default(short);
            public short Zone2TypeAct = default(short);
            public short Zone3TypeAct = default(short);
            public short Zone4TypeAct = default(short);
            public short Zone5TypeAct = default(short);
            public short Zone6TypeAct = default(short);
            public short Zone7TypeAct = default(short);
            public short Zone8TypeAct = default(short);
            public short RetainerRingTypeAct = default(short);
            public float Zone1PressureAct = default(float);
            public float Zone1PressureSet = default(float);
            public float Zone2PressureAct = default(float);
            public float Zone2PressureSet = default(float);
            public float Zone3PressureAct = default(float);
            public float Zone3PressureSet = default(float);
            public float Zone4PressureAct = default(float);
            public float Zone4PressureSet = default(float);
            public float Zone5PressureAct = default(float);
            public float Zone5PressureSet = default(float);
            public float Zone6PressureAct = default(float);
            public float Zone6PressureSet = default(float);
            public float Zone7PressureAct = default(float);
            public float Zone7PressureSet = default(float);
            public float Zone8PressureAct = default(float);
            public float Zone8PressureSet = default(float);
            public float RetainerRingAirBagPressureAct = default(float);
            public float RetainerRingAirBagPressureSet = default(float);
            public float ConditionDownForcePressureAct = default(float);
            public float ConditionDownForcePressureSet = default(float);
            public float SlurryNozzleSpeedAct = default(float);
            public float SlurryNozzleSpeedSet = default(float);
            public float SlurryNozzlePositionAct = default(float);
            public float SlurryNozzlePositionSet = default(float);
            public float PlatenRotationAngleAct = default(float);
            public float HeadRotationAngleAct = default(float);
            public float ConditionRotationAngleAct = default(float);
            public float Slurry1Press1Act = default(float);
            public float Slurry1Press2Act = default(float);
            public float Slurry2Press1Act = default(float);
            public float Slurry2Press2Act = default(float);
            public float Slurry3Press1Act = default(float);
            public float Slurry3Press2Act = default(float);
            public float DressingWaterFlowrateAct = default(float);
            public float PlatenWashTubePressAct = default(float);
            public float PlatenWashTubeFlowrateAct = default(float);
            public float ConditionDIWWashTubeFlowrateAct = default(float);
            public float ConditionArmDIWWashTubeFlowrateAct = default(float);
            public float RR_GapCleaningTubeFlowrateAct = default(float);
            public float ReleaseDIWTubeRinseAct = default(float);
            public float LTPWaferRinseFlowrateAct = default(float);
            public float TRCleaningTubeFlowrateAct = default(float);
            public float PlatenCoolingWaterFlowrateAct = default(float);
            public float PlatenMotorCoolingWaterFlowrateAct = default(float);
            public float Zone1FlowrateAct = default(float);
            public float Zone2FlowrateAct = default(float);
            public float Zone3FlowrateAct = default(float);
            public float Zone4FlowrateAct = default(float);
            public float Zone5FlowrateAct = default(float);
            public float Zone6FlowrateAct = default(float);
            public float Zone7FlowrateAct = default(float);
            public float Zone8FlowrateAct = default(float);
            public float Zone4N2PressAct = default(float);
            public float RetainingRingAirBagFlowrateAct = default(float);
            public float VacuumPressAct = default(float);
            public float ReleaseN2PurgeFlowRateAct = default(float);
            public float ConditionHeadHeightPositionAct = default(float);
            public float RetainerRingHeightAct = default(float);
            public float Zone1PressurePVAct = default(float);
            public float Zone1PressurePVSet = default(float);
            public float Zone2PressurePVAct = default(float);
            public float Zone2PressurePVSet = default(float);
            public float Zone3PressurePVAct = default(float);
            public float Zone3PressurePVSet = default(float);
            public float Zone4PressurePVAct = default(float);
            public float Zone4PressurePVSet = default(float);
            public float Zone5PressurePVAct = default(float);
            public float Zone5PressurePVSet = default(float);
            public float Zone6PressurePVAct = default(float);
            public float Zone6PressurePVSet = default(float);
            public float Zone7PressurePVAct = default(float);
            public float Zone7PressurePVSet = default(float);
            public float Zone8PressurePVAct = default(float);
            public float Zone8PressurePVSet = default(float);
            public float RetainerRingAirBagPressurePVAct = default(float);
            public float RetainerRingAirBagPressurePVSet = default(float);
            public float ConditionDownForcePressurePVAct = default(float);
            public float ConditionDownForcePressurePVSet = default(float);
            public float ConditionDownForceInlineAct = default(float);
            public float PadTemperatureAct = default(float);
            public float ConditionCenterRadiusPositionAct = default(float);
            public float PlatenRotationMotorTorqueAct = default(float);
            public float HeadRotationMotorTorqueAct = default(float);
            public float HeadSweepMotorTorqueAct = default(float);
            public float HeadZ_AxisMotorTorqueAct = default(float);
            public float ConditionRotationMotorTorqueAct = default(float);
            public float ConditionSweepMotorTorqueAct = default(float);
            public float SlurryNozzleMotorTorqueAct = default(float);
            public bool PlatenRotationMotorServoOn = default(bool);
            public bool HeadRotationMotorServoOn = default(bool);
            public bool HeadSweepMotorServoOn = default(bool);
            public bool HeadUpDownMotorServoOn = default(bool);
            public bool ConditionHeadRotationMotorServoOn = default(bool);
            public bool ConditionSweepMotorServoOn = default(bool);
            public bool SlurrySweepMotorServoOn = default(bool);
            public float Zone1PressureInlineAct = default(float);
            public float Zone2PressureInlineAct = default(float);
            public float Zone3PressureInlineAct = default(float);
            public float Zone4PressureInlineAct = default(float);
            public float Zone5PressureInlineAct = default(float);
            public float Zone6PressureInlineAct = default(float);
            public float Zone7PressureInlineAct = default(float);
            public float Zone8PressureInlineAct = default(float);
            public float RetainerRingAirBagPressureInlineAct = default(float);
            public float ConditionDownForcePressureInlineAct = default(float);
            public float RetainerRingPressureAct = default(float);
            public float RetainerRingPressureSet = default(float);
            public short UnitWaferCount = default(short);
            public short AirBagWaferCount = default(short);
            public short AirBagUesTime = default(short);
            public short MembranceWaferCount = default(short);
            public short MembranceUesTime = default(short);
            public short RatainerRingWaferCount = default(short);
            public short RatainerRingUesTime = default(short);
            public short PadWaferCount = default(short);
            public short PadUesTime = default(short);
            public short ConditionInsiUesTime = default(short);
            public short ConditionExsiUesTime = default(short);
            public short UnitIdleTimeh = default(short);
            public short UnitIdleTimemin = default(short);
            public short UnitIdleTimes = default(short);
        }

        public class stRollerCollectData
        {
            public short _1ProcessStepAct = default(short);
            public short _1ProcessStepSet = default(short);
            public short _1ProcessTypeAct = default(short);
            public short _1ProcessTypeSet = default(short);
            public float _1RecipeTimeAct = default(float);
            public float _1RecipeTimeSet = default(float);
            public float _1UpperRollerRotateVelocityAct = default(float);
            public float _1UpperRollerRotateVelocitySet = default(float);
            public float _1LowerRollerRotateVelocityAct = default(float);
            public float _1LowerRollerRotateVelocitySet = default(float);
            public float _1WFLeftPillarRotateMotorVelocityAct = default(float);
            public float _1WFLeftPillarRotateMotorVelocitySet = default(float);
            public float _1WFRightPillarRotateMotorVelocityAct = default(float);
            public float _1WFRightPillarRotateMotorVelocitySet = default(float);
            public float _1UpperRollerPressureAct = default(float);
            public float _1UpperRollerPressureSet = default(float);
            public float _1LowerRollerPressureAct = default(float);
            public float _1LowerRollerPressureSet = default(float);
            public float _1WFUpperFaceDIWFlowrateAct = default(float);
            public float _1WFUpperFaceDIWFlowrateSet = default(float);
            public float _1WFLowerFaceDIWFlowrateAct = default(float);
            public float _1WFLowerFaceDIWFlowrateSet = default(float);
            public short _1WFUpperFaceChemicalSupplyAct = default(short);
            public short _1WFUpperFaceChemicalSupplySet = default(short);
            public short _1WFLowerFaceChemicalSupplyAct = default(short);
            public short _1WFLowerFaceChemicalSupplySet = default(short);
            public short _1MegaSonicSupplyAct = default(short);
            public short _1MegaSonicSupplySet = default(short);
            public float _1ChemicalEnterFlowratemeteAct = default(float);
            public float _1ChemicalEnterFlowratemeteSet = default(float);
            public float _1DIWEnterFlowratemeteAct = default(float);
            public float _1DIWEnterFlowratemeteSet = default(float);
            public float _1WFLeftPillarRotateMotorAngleAct = default(float);
            public float _1WFRightPillarRotateMotorAngleAct = default(float);
            public float _1UpperRollerRotateMotorAngleAct = default(float);
            public float _1LowerRollerRotateMotorAngleAct = default(float);
            public float _1ChemicalEntrancePressureAct = default(float);
            public float _1WFLowerFaceChemicalWashPressureAct = default(float);
            public float _1WFUpperFaceChemicalWashFlowrateAct = default(float);
            public float _1QuartzChemicalWashFlowrateAct = default(float);
            public float _1WFLowerFaceDIWWashPressureAct = default(float);
            public float _1UpperRollerDIWWashFlowrateAct = default(float);
            public float _1QuartzDIWWashFlowrateAct = default(float);
            public float _1UpperRollerInsideDIWWashFlowrateAct = default(float);
            public float _1LowerRollerInsideDIWWashFlowrateAct = default(float);
            public float _1MageSonicDIWWashFlowrateAct = default(float);
            public float _1ChamberDIWWashFlowrateAct = default(float);
            public float _1PillarWashFlowrateAct = default(float);
            public float _1CoolingWaterCirculationFlowrateAct = default(float);
            public float _1LowerRollerDownPressureAct = default(float);
            public float _1ClampCylinderPressureAct = default(float);
            public float _1UpperRollerPressure1Act = default(float);
            public float _1ClampPressureAct = default(float);
            public float _1ClampPressureSet = default(float);
            public float _1WFLeftPillarRotateMotorTorqueActualValueAct = default(float);
            public float _1WFRightPillarRotateMotorTorqueActualValueAct = default(float);
            public float _1UpperRollerRotateMotorTorqueActualValueAct = default(float);
            public float _1LowerRollerRotateMotorTorqueActualValueAct = default(float);
            public bool _1WFLeftPillarMotorServoOn = default(bool);
            public bool _1WFRightPillarMotorServoOn = default(bool);
            public bool _1UpperRollerMotorServoOn = default(bool);
            public bool _1LowerRollerMotorServoOn = default(bool);
            public short _2ProcessStepAct = default(short);
            public short _2ProcessStepSet = default(short);
            public short _2ProcessTypeAct = default(short);
            public short _2ProcessTypeSet = default(short);
            public float _2RecipeTimeAct = default(float);
            public float _2RecipeTimeSet = default(float);
            public float _2UpperRollerRotateVelocityAct = default(float);
            public float _2UpperRollerRotateVelocitySet = default(float);
            public float _2LowerRollerRotateVelocityAct = default(float);
            public float _2LowerRollerRotateVelocitySet = default(float);
            public float _2WFLeftPillarRotateMotorVelocityAct = default(float);
            public float _2WFLeftPillarRotateMotorVelocitySet = default(float);
            public float _2WFRightPillarRotateMotorVelocityAct = default(float);
            public float _2WFRightPillarRotateMotorVelocitySet = default(float);
            public float _2UpperRollerPressureAct = default(float);
            public float _2UpperRollerPressureSet = default(float);
            public float _2LowerRollerPressureAct = default(float);
            public float _2LowerRollerPressureSet = default(float);
            public float _2WFUpperFaceDIWFlowrateAct = default(float);
            public float _2WFUpperFaceDIWFlowrateSet = default(float);
            public float _2WFLowerFaceDIWFlowrateAct = default(float);
            public float _2WFLowerFaceDIWFlowrateSet = default(float);
            public short _2WFUpperFaceChemicalSupplyAct = default(short);
            public short _2WFUpperFaceChemicalSupplySet = default(short);
            public short _2WFLowerFaceChemicalSupplyAct = default(short);
            public short _2WFLowerFaceChemicalSupplySet = default(short);
            public short _2MegaSonicSupplyAct = default(short);
            public short _2MegaSonicSupplySet = default(short);
            public float _2ChemicalEnterFlowratemeteAct = default(float);
            public float _2ChemicalEnterFlowratemeteSet = default(float);
            public float _2DIWEnterFlowratemeteAct = default(float);
            public float _2DIWEnterFlowratemeteSet = default(float);
            public float _2WFLeftPillarRotateMotorAngleAct = default(float);
            public float _2WFRightPillarRotateMotorAngleAct = default(float);
            public float _2UpperRollerRotateMotorAngleAct = default(float);
            public float _2LowerRollerRotateMotorAngleAct = default(float);
            public float _2ChemicalEntrancePressureAct = default(float);
            public float _2WFLowerFaceChemicalWashPressureAct = default(float);
            public float _2WFUpperFaceChemicalWashFlowrateAct = default(float);
            public float _2QuartzChemicalWashFlowrateAct = default(float);
            public float _2WFLowerFaceDIWWashPressureAct = default(float);
            public float _2UpperRollerDIWWashFlowrateAct = default(float);
            public float _2QuartzDIWWashFlowrateAct = default(float);
            public float _2UpperRollerInsideDIWWashFlowrateAct = default(float);
            public float _2LowerRollerInsideDIWWashFlowrateAct = default(float);
            public float _2MageSonicDIWWashFlowrateAct = default(float);
            public float _2ChamberDIWWashFlowrateAct = default(float);
            public float _2PillarWashFlowrateAct = default(float);
            public float _2CoolingWaterCirculationFlowrateAct = default(float);
            public float _2LowerRollerDownPressureAct = default(float);
            public float _2ClampCylinderPressureAct = default(float);
            public float _2UpperRollerPressure1Act = default(float);
            public float _2ClampPressureAct = default(float);
            public float _2ClampPressureSet = default(float);
            public float _2WFLeftPillarRotateMotorTorqueActualValueAct = default(float);
            public float _2WFRightPillarRotateMotorTorqueActualValueAct = default(float);
            public float _2UpperRollerRotateMotorTorqueActualValueAct = default(float);
            public float _2LowerRollerRotateMotorTorqueActualValueAct = default(float);
            public bool _2WFLeftPillarMotorServoOn = default(bool);
            public bool _2WFRightPillarMotorServoOn = default(bool);
            public bool _2UpperRollerMotorServoOn = default(bool);
            public bool _2LowerRollerMotorServoOn = default(bool);
            public short _3ProcessStepAct = default(short);
            public short _3ProcessStepSet = default(short);
            public short _3ProcessTypeAct = default(short);
            public short _3ProcessTypeSet = default(short);
            public float _3RecipeTimeAct = default(float);
            public float _3RecipeTimeSet = default(float);
            public float _3UpperRollerRotateVelocityAct = default(float);
            public float _3UpperRollerRotateVelocitySet = default(float);
            public float _3LowerRollerRotateVelocityAct = default(float);
            public float _3LowerRollerRotateVelocitySet = default(float);
            public float _3WFLeftPillarRotateMotorVelocityAct = default(float);
            public float _3WFLeftPillarRotateMotorVelocitySet = default(float);
            public float _3WFRightPillarRotateMotorVelocityAct = default(float);
            public float _3WFRightPillarRotateMotorVelocitySet = default(float);
            public float _3UpperRollerPressureAct = default(float);
            public float _3UpperRollerPressureSet = default(float);
            public float _3LowerRollerPressureAct = default(float);
            public float _3LowerRollerPressureSet = default(float);
            public float _3WFUpperFaceDIWFlowrateAct = default(float);
            public float _3WFUpperFaceDIWFlowrateSet = default(float);
            public float _3WFLowerFaceDIWFlowrateAct = default(float);
            public float _3WFLowerFaceDIWFlowrateSet = default(float);
            public short _3WFUpperFaceChemicalSupplyAct = default(short);
            public short _3WFUpperFaceChemicalSupplySet = default(short);
            public short _3WFLowerFaceChemicalSupplyAct = default(short);
            public short _3WFLowerFaceChemicalSupplySet = default(short);
            public short _3MegaSonicSupplyAct = default(short);
            public short _3MegaSonicSupplySet = default(short);
            public float _3ChemicalEnterFlowratemeteAct = default(float);
            public float _3ChemicalEnterFlowratemeteSet = default(float);
            public float _3DIWEnterFlowratemeteAct = default(float);
            public float _3DIWEnterFlowratemeteSet = default(float);
            public float _3WFLeftPillarRotateMotorAngleAct = default(float);
            public float _3WFRightPillarRotateMotorAngleAct = default(float);
            public float _3UpperRollerRotateMotorAngleAct = default(float);
            public float _3LowerRollerRotateMotorAngleAct = default(float);
            public float _3ChemicalEntrancePressureAct = default(float);
            public float _3WFLowerFaceChemicalWashPressureAct = default(float);
            public float _3WFUpperFaceChemicalWashFlowrateAct = default(float);
            public float _3QuartzChemicalWashFlowrateAct = default(float);
            public float _3WFLowerFaceDIWWashPressureAct = default(float);
            public float _3UpperRollerDIWWashFlowrateAct = default(float);
            public float _3QuartzDIWWashFlowrateAct = default(float);
            public float _3UpperRollerInsideDIWWashFlowrateAct = default(float);
            public float _3LowerRollerInsideDIWWashFlowrateAct = default(float);
            public float _3MageSonicDIWWashFlowrateAct = default(float);
            public float _3ChamberDIWWashFlowrateAct = default(float);
            public float _3PillarWashFlowrateAct = default(float);
            public float _3CoolingWaterCirculationFlowrateAct = default(float);
            public float _3LowerRollerDownPressureAct = default(float);
            public float _3ClampCylinderPressureAct = default(float);
            public float _3UpperRollerPressure1Act = default(float);
            public float _3ClampPressureAct = default(float);
            public float _3ClampPressureSet = default(float);
            public float _3WFLeftPillarRotateMotorTorqueActualValueAct = default(float);
            public float _3WFRightPillarRotateMotorTorqueActualValueAct = default(float);
            public float _3UpperRollerRotateMotorTorqueActualValueAct = default(float);
            public float _3LowerRollerRotateMotorTorqueActualValueAct = default(float);
            public bool _3WFLeftPillarMotorServoOn = default(bool);
            public bool _3WFRightPillarMotorServoOn = default(bool);
            public bool _3UpperRollerMotorServoOn = default(bool);
            public bool _3LowerRollerMotorServoOn = default(bool);
        }

        public static int[] NewIndices(Array target)
        {
            int rank = target.Rank;
            int[] indices = new int[rank];
            Array.Clear(indices, 0, rank);
            indices[rank - 1]--;
            return indices;
        }

        public static void NextIndices(Array target, int[] indices)
        {
            int rank = target.Rank;
            indices[rank - 1]++;
            for (int dim = rank - 1; dim >= 0; dim--)
            {
                if (indices[dim] > target.GetUpperBound(dim))
                {
                    if (dim == 0) break;
                    for (int j = dim; j < rank; j++) indices[j] = target.GetLowerBound(j);
                    indices[dim - 1]++;
                }
            }
        }

        public static Array CreateClassInstanceArray<T>(int[] sizes)
        {
            Array entity = Array.CreateInstance(typeof(T), sizes);
            int[] indices = NewIndices(entity);
            for (int i = 0; i < entity.Length; i++)
            {
                NextIndices(entity, indices);
                entity.SetValue(Activator.CreateInstance(typeof(T)), indices);
            }
            return entity;
        }

        private static void SwapBytes(byte[] tgt, int byteSize, int byteOffset = 0)
        {
            byte tmpByte;
            for (int i = 0; i < byteSize / 2; i++)
            {
                tmpByte = tgt[i * 2 + byteOffset];
                tgt[i * 2 + byteOffset] = tgt[i * 2 + 1 + byteOffset];
                tgt[i * 2 + 1 + byteOffset] = tmpByte;
            }
        }

        public static void ByteToBoolBase(Array dst, byte[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            int byteOffset = (wordOffset * 2) + (bitOffset / 8);
            int byteBitOffset = bitOffset % 8;
            int[] indices = NewIndices(src);
            for (int i = 0; i < len; i++)
            {
                NextIndices(src, indices);
                dst.SetValue((src[((i + byteBitOffset) / 8) + byteOffset] & (1 << (i + byteBitOffset) % 8)) != 0, indices);
            }
        }

        public static void ByteToBool(ref bool dst, byte[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            int byteOffset = (wordOffset * 2) + (bitOffset / 8);
            int byteBitOffset = bitOffset % 8;
            dst = (src[byteOffset] & 1 << byteBitOffset) != 0;
        }

        public static void ByteToBool(ref bool[] dst, byte[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            ByteToBoolBase(dst, src, len, wordOffset, bitOffset);
        }
        public static void ByteToBool(ref bool[,] dst, byte[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            ByteToBoolBase(dst, src, len, wordOffset, bitOffset);
        }
        public static void ByteToBool(ref bool[,,] dst, byte[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            ByteToBoolBase(dst, src, len, wordOffset, bitOffset);
        }
        public static void ByteToBool(ref bool[,,,] dst, byte[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            ByteToBoolBase(dst, src, len, wordOffset, bitOffset);
        }
        public static void ByteToBool(ref bool[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            ByteToBoolBase(dst, src, len, wordOffset, bitOffset);
        }
        public static void ByteToBool(ref bool[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            ByteToBoolBase(dst, src, len, wordOffset, bitOffset);
        }
        public static void ByteToBool(ref bool[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            ByteToBoolBase(dst, src, len, wordOffset, bitOffset);
        }
        public static void ByteToBool(ref bool[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            ByteToBoolBase(dst, src, len, wordOffset, bitOffset);
        }

        public static void ByteToShort(ref short dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            dst = BitConverter.ToInt16(src, wordOffset * 2);
        }

        public static void ByteToShort(ref short[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(short));
        }
        public static void ByteToShort(ref short[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(short));
        }
        public static void ByteToShort(ref short[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(short));
        }
        public static void ByteToShort(ref short[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(short));
        }
        public static void ByteToShort(ref short[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(short));
        }
        public static void ByteToShort(ref short[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(short));
        }
        public static void ByteToShort(ref short[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(short));
        }
        public static void ByteToShort(ref short[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(short));
        }

        public static void ByteToUshort(ref ushort dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            dst = BitConverter.ToUInt16(src, wordOffset * 2);
        }

        public static void ByteToUshort(ref ushort[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(ushort));
        }
        public static void ByteToUshort(ref ushort[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(ushort));
        }
        public static void ByteToUshort(ref ushort[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(ushort));
        }
        public static void ByteToUshort(ref ushort[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(ushort));
        }
        public static void ByteToUshort(ref ushort[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(ushort));
        }
        public static void ByteToUshort(ref ushort[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(ushort));
        }
        public static void ByteToUshort(ref ushort[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(ushort));
        }
        public static void ByteToUshort(ref ushort[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(ushort));
        }

        public static void ByteToInt(ref int dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            dst = BitConverter.ToInt32(src, wordOffset * 2);
        }

        public static void ByteToInt(ref int[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(int));
        }
        public static void ByteToInt(ref int[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(int));
        }
        public static void ByteToInt(ref int[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(int));
        }
        public static void ByteToInt(ref int[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(int));
        }
        public static void ByteToInt(ref int[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(int));
        }
        public static void ByteToInt(ref int[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(int));
        }
        public static void ByteToInt(ref int[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(int));
        }
        public static void ByteToInt(ref int[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(int));
        }

        public static void ByteToUint(ref uint dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            dst = BitConverter.ToUInt32(src, wordOffset * 2);
        }

        public static void ByteToUint(ref uint[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(uint));
        }
        public static void ByteToUint(ref uint[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(uint));
        }
        public static void ByteToUint(ref uint[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(uint));
        }
        public static void ByteToUint(ref uint[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(uint));
        }
        public static void ByteToUint(ref uint[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(uint));
        }
        public static void ByteToUint(ref uint[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(uint));
        }
        public static void ByteToUint(ref uint[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(uint));
        }
        public static void ByteToUint(ref uint[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(uint));
        }

        public static void ByteToFloat(ref float dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            dst = BitConverter.ToSingle(src, wordOffset * 2);
        }

        public static void ByteToFloat(ref float[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(float));
        }
        public static void ByteToFloat(ref float[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(float));
        }
        public static void ByteToFloat(ref float[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(float));
        }
        public static void ByteToFloat(ref float[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(float));
        }
        public static void ByteToFloat(ref float[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(float));
        }
        public static void ByteToFloat(ref float[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(float));
        }
        public static void ByteToFloat(ref float[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(float));
        }
        public static void ByteToFloat(ref float[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(float));
        }

        public static void ByteToDouble(ref double dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            dst = BitConverter.ToDouble(src, wordOffset * 2);
        }

        public static void ByteToDouble(ref double[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(double));
        }
        public static void ByteToDouble(ref double[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(double));
        }
        public static void ByteToDouble(ref double[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(double));
        }
        public static void ByteToDouble(ref double[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(double));
        }
        public static void ByteToDouble(ref double[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(double));
        }
        public static void ByteToDouble(ref double[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(double));
        }
        public static void ByteToDouble(ref double[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(double));
        }
        public static void ByteToDouble(ref double[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(double));
        }

        public static void ByteToTime(ref long dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            dst = BitConverter.ToInt64(src, wordOffset * 2);
        }

        public static void ByteToTime(ref long[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(long));
        }
        public static void ByteToTime(ref long[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(long));
        }
        public static void ByteToTime(ref long[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(long));
        }
        public static void ByteToTime(ref long[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(long));
        }
        public static void ByteToTime(ref long[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(long));
        }
        public static void ByteToTime(ref long[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(long));
        }
        public static void ByteToTime(ref long[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(long));
        }
        public static void ByteToTime(ref long[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, wordOffset * 2, dst, 0, len * sizeof(long));
        }

        public static void ByteToString(ref byte[] dst, byte[] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            byte[] strTmp = new byte[len * stringSize];
            Array.Copy(src, wordOffset * 2, strTmp, 0, len * stringSize);
            SwapBytes(strTmp, len * stringSize);
            Buffer.BlockCopy(strTmp, 0, dst, 0, len * stringSize);
        }
        public static void ByteToString(ref byte[,] dst, byte[] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            byte[] strTmp = new byte[len * stringSize];
            Array.Copy(src, wordOffset * 2, strTmp, 0, len * stringSize);
            SwapBytes(strTmp, len * stringSize);
            Buffer.BlockCopy(strTmp, 0, dst, 0, len * stringSize);
        }
        public static void ByteToString(ref byte[,,] dst, byte[] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            byte[] strTmp = new byte[len * stringSize];
            Array.Copy(src, wordOffset * 2, strTmp, 0, len * stringSize);
            SwapBytes(strTmp, len * stringSize);
            Buffer.BlockCopy(strTmp, 0, dst, 0, len * stringSize);
        }
        public static void ByteToString(ref byte[,,,] dst, byte[] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            byte[] strTmp = new byte[len * stringSize];
            Array.Copy(src, wordOffset * 2, strTmp, 0, len * stringSize);
            SwapBytes(strTmp, len * stringSize);
            Buffer.BlockCopy(strTmp, 0, dst, 0, len * stringSize);
        }
        public static void ByteToString(ref byte[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            byte[] strTmp = new byte[len * stringSize];
            Array.Copy(src, wordOffset * 2, strTmp, 0, len * stringSize);
            SwapBytes(strTmp, len * stringSize);
            Buffer.BlockCopy(strTmp, 0, dst, 0, len * stringSize);
        }
        public static void ByteToString(ref byte[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            byte[] strTmp = new byte[len * stringSize];
            Array.Copy(src, wordOffset * 2, strTmp, 0, len * stringSize);
            SwapBytes(strTmp, len * stringSize);
            Buffer.BlockCopy(strTmp, 0, dst, 0, len * stringSize);
        }
        public static void ByteToString(ref byte[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            byte[] strTmp = new byte[len * stringSize];
            Array.Copy(src, wordOffset * 2, strTmp, 0, len * stringSize);
            SwapBytes(strTmp, len * stringSize);
            Buffer.BlockCopy(strTmp, 0, dst, 0, len * stringSize);
        }
        public static void ByteToString(ref byte[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            byte[] strTmp = new byte[len * stringSize];
            Array.Copy(src, wordOffset * 2, strTmp, 0, len * stringSize);
            SwapBytes(strTmp, len * stringSize);
            Buffer.BlockCopy(strTmp, 0, dst, 0, len * stringSize);
        }

        public static void BoolToByteBase(ref byte[] dst, Array src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            int[] indices = NewIndices(dst);
            for (int i = 0; i < len; i++)
            {
                NextIndices(dst, indices);
                BoolToByte(ref dst, (bool)src.GetValue(indices), 1, wordOffset, bitOffset);
                bitOffset++;
                if (bitOffset >= 16)
                {
                    bitOffset = 0;
                    wordOffset++;
                }
            }
        }

        public static void BoolToByte(ref byte[] dst, bool src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            int byteOffset = (wordOffset * 2) + (bitOffset / 8);
            int byteBitOffset = bitOffset % 8;
            if (src)
            {
                dst[byteOffset] |= (byte)(1 << byteBitOffset);
            }
            else
            {
                dst[byteOffset] &= (byte)~(1 << byteBitOffset);
            }
        }

        public static void BoolToByte(ref byte[] dst, bool[] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            BoolToByteBase(ref dst, src, len, wordOffset, bitOffset);
        }
        public static void BoolToByte(ref byte[] dst, bool[,] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            BoolToByteBase(ref dst, src, len, wordOffset, bitOffset);
        }
        public static void BoolToByte(ref byte[] dst, bool[,,] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            BoolToByteBase(ref dst, src, len, wordOffset, bitOffset);
        }
        public static void BoolToByte(ref byte[] dst, bool[,,,] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            BoolToByteBase(ref dst, src, len, wordOffset, bitOffset);
        }
        public static void BoolToByte(ref byte[] dst, bool[,,,,] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            BoolToByteBase(ref dst, src, len, wordOffset, bitOffset);
        }
        public static void BoolToByte(ref byte[] dst, bool[,,,,,] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            BoolToByteBase(ref dst, src, len, wordOffset, bitOffset);
        }
        public static void BoolToByte(ref byte[] dst, bool[,,,,,,] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            BoolToByteBase(ref dst, src, len, wordOffset, bitOffset);
        }
        public static void BoolToByte(ref byte[] dst, bool[,,,,,,,] src, int len = 1, int wordOffset = 0, int bitOffset = 0)
        {
            BoolToByteBase(ref dst, src, len, wordOffset, bitOffset);
        }

        public static void ShortToByte(ref byte[] dst, short src, int len = 1, int wordOffset = 0)
        {
            Array.Copy(BitConverter.GetBytes(src), 0, dst, wordOffset * 2, len * sizeof(short));
        }

        public static void ShortToByte(ref byte[] dst, short[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(short));
        }
        public static void ShortToByte(ref byte[] dst, short[,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(short));
        }
        public static void ShortToByte(ref byte[] dst, short[,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(short));
        }
        public static void ShortToByte(ref byte[] dst, short[,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(short));
        }
        public static void ShortToByte(ref byte[] dst, short[,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(short));
        }
        public static void ShortToByte(ref byte[] dst, short[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(short));
        }
        public static void ShortToByte(ref byte[] dst, short[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(short));
        }
        public static void ShortToByte(ref byte[] dst, short[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(short));
        }

        public static void UshortToByte(ref byte[] dst, ushort src, int len = 1, int wordOffset = 0)
        {
            Array.Copy(BitConverter.GetBytes(src), 0, dst, wordOffset * 2, len * sizeof(ushort));
        }

        public static void UshortToByte(ref byte[] dst, ushort[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(ushort));
        }
        public static void UshortToByte(ref byte[] dst, ushort[,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(ushort));
        }
        public static void UshortToByte(ref byte[] dst, ushort[,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(ushort));
        }
        public static void UshortToByte(ref byte[] dst, ushort[,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(ushort));
        }
        public static void UshortToByte(ref byte[] dst, ushort[,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(ushort));
        }
        public static void UshortToByte(ref byte[] dst, ushort[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(ushort));
        }
        public static void UshortToByte(ref byte[] dst, ushort[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(ushort));
        }
        public static void UshortToByte(ref byte[] dst, ushort[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(ushort));
        }

        public static void IntToByte(ref byte[] dst, int src, int len = 1, int wordOffset = 0)
        {
            Array.Copy(BitConverter.GetBytes(src), 0, dst, wordOffset * 2, len * sizeof(int));
        }

        public static void IntToByte(ref byte[] dst, int[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(int));
        }
        public static void IntToByte(ref byte[] dst, int[,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(int));
        }
        public static void IntToByte(ref byte[] dst, int[,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(int));
        }
        public static void IntToByte(ref byte[] dst, int[,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(int));
        }
        public static void IntToByte(ref byte[] dst, int[,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(int));
        }
        public static void IntToByte(ref byte[] dst, int[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(int));
        }
        public static void IntToByte(ref byte[] dst, int[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(int));
        }
        public static void IntToByte(ref byte[] dst, int[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(int));
        }

        public static void UintToByte(ref byte[] dst, uint src, int len = 1, int wordOffset = 0)
        {
            Array.Copy(BitConverter.GetBytes(src), 0, dst, wordOffset * 2, len * sizeof(uint));
        }

        public static void UintToByte(ref byte[] dst, uint[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(uint));
        }
        public static void UintToByte(ref byte[] dst, uint[,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(uint));
        }
        public static void UintToByte(ref byte[] dst, uint[,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(uint));
        }
        public static void UintToByte(ref byte[] dst, uint[,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(uint));
        }
        public static void UintToByte(ref byte[] dst, uint[,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(uint));
        }
        public static void UintToByte(ref byte[] dst, uint[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(uint));
        }
        public static void UintToByte(ref byte[] dst, uint[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(uint));
        }
        public static void UintToByte(ref byte[] dst, uint[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(uint));
        }

        public static void FloatToByte(ref byte[] dst, float src, int len = 1, int wordOffset = 0)
        {
            Array.Copy(BitConverter.GetBytes(src), 0, dst, wordOffset * 2, len * sizeof(float));
        }

        public static void FloatToByte(ref byte[] dst, float[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(float));
        }
        public static void FloatToByte(ref byte[] dst, float[,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(float));
        }
        public static void FloatToByte(ref byte[] dst, float[,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(float));
        }
        public static void FloatToByte(ref byte[] dst, float[,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(float));
        }
        public static void FloatToByte(ref byte[] dst, float[,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(float));
        }
        public static void FloatToByte(ref byte[] dst, float[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(float));
        }
        public static void FloatToByte(ref byte[] dst, float[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(float));
        }
        public static void FloatToByte(ref byte[] dst, float[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(float));
        }

        public static void DoubleToByte(ref byte[] dst, double src, int len = 1, int wordOffset = 0)
        {
            Array.Copy(BitConverter.GetBytes(src), 0, dst, wordOffset * 2, len * sizeof(double));
        }

        public static void DoubleToByte(ref byte[] dst, double[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(double));
        }
        public static void DoubleToByte(ref byte[] dst, double[,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(double));
        }
        public static void DoubleToByte(ref byte[] dst, double[,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(double));
        }
        public static void DoubleToByte(ref byte[] dst, double[,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(double));
        }
        public static void DoubleToByte(ref byte[] dst, double[,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(double));
        }
        public static void DoubleToByte(ref byte[] dst, double[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(double));
        }
        public static void DoubleToByte(ref byte[] dst, double[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(double));
        }
        public static void DoubleToByte(ref byte[] dst, double[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(double));
        }

        public static void TimeToByte(ref byte[] dst, long src, int len = 1, int wordOffset = 0)
        {
            Array.Copy(BitConverter.GetBytes(src), 0, dst, wordOffset * 2, len * sizeof(long));
        }

        public static void TimeToByte(ref byte[] dst, long[] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(long));
        }
        public static void TimeToByte(ref byte[] dst, long[,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(long));
        }
        public static void TimeToByte(ref byte[] dst, long[,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(long));
        }
        public static void TimeToByte(ref byte[] dst, long[,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(long));
        }
        public static void TimeToByte(ref byte[] dst, long[,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(long));
        }
        public static void TimeToByte(ref byte[] dst, long[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(long));
        }
        public static void TimeToByte(ref byte[] dst, long[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(long));
        }
        public static void TimeToByte(ref byte[] dst, long[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * sizeof(long));
        }

        public static void StringToByte(ref byte[] dst, byte[] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * stringSize);
            SwapBytes(dst, len * stringSize, wordOffset * 2);
        }
        public static void StringToByte(ref byte[] dst, byte[,] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * stringSize);
            SwapBytes(dst, len * stringSize, wordOffset * 2);
        }
        public static void StringToByte(ref byte[] dst, byte[,,] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * stringSize);
            SwapBytes(dst, len * stringSize, wordOffset * 2);
        }
        public static void StringToByte(ref byte[] dst, byte[,,,] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * stringSize);
            SwapBytes(dst, len * stringSize, wordOffset * 2);
        }
        public static void StringToByte(ref byte[] dst, byte[,,,,] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * stringSize);
            SwapBytes(dst, len * stringSize, wordOffset * 2);
        }
        public static void StringToByte(ref byte[] dst, byte[,,,,,] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * stringSize);
            SwapBytes(dst, len * stringSize, wordOffset * 2);
        }
        public static void StringToByte(ref byte[] dst, byte[,,,,,,] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * stringSize);
            SwapBytes(dst, len * stringSize, wordOffset * 2);
        }
        public static void StringToByte(ref byte[] dst, byte[,,,,,,,] src, int len = 1, int wordOffset = 0, int stringSize = 0)
        {
            Buffer.BlockCopy(src, 0, dst, wordOffset * 2, len * stringSize);
            SwapBytes(dst, len * stringSize, wordOffset * 2);
        }

        public static void ByteTo_stDryCollectData(ref stDryCollectData dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToShort(ref dst.ProcessStepAct, src, 1, 0 + wordOffset);
            ByteToShort(ref dst.ProcessStepSet, src, 1, 1 + wordOffset);
            ByteToShort(ref dst.ProcessTypeAct, src, 1, 2 + wordOffset);
            ByteToShort(ref dst.ProcessTypeSet, src, 1, 3 + wordOffset);
            ByteToFloat(ref dst.RecipeTimeAct, src, 1, 4 + wordOffset);
            ByteToFloat(ref dst.RecipeTimeSet, src, 1, 6 + wordOffset);
            ByteToFloat(ref dst.ChuckRotationMotorVelocityValuAct, src, 1, 8 + wordOffset);
            ByteToFloat(ref dst.ChuckRotationMotorVelocityValuSet, src, 1, 10 + wordOffset);
            ByteToFloat(ref dst.ArmSweepMotorVelocityAct, src, 1, 12 + wordOffset);
            ByteToFloat(ref dst.ArmSweepMotorVelocitySet, src, 1, 14 + wordOffset);
            ByteToFloat(ref dst.ArmSweepMotorPositionAct, src, 1, 16 + wordOffset);
            ByteToFloat(ref dst.ArmSweepMotorPositionSet, src, 1, 18 + wordOffset);
            ByteToShort(ref dst.ArmSweepMotorCycleAct, src, 1, 20 + wordOffset);
            ByteToShort(ref dst.ArmSweepMotorCycleSet, src, 1, 21 + wordOffset);
            ByteToFloat(ref dst.WFUpperFaceN2PuegeFlowrateAct, src, 1, 22 + wordOffset);
            ByteToFloat(ref dst.WFUpperFaceN2PuegeFlowrateSet, src, 1, 24 + wordOffset);
            ByteToFloat(ref dst.WFLowerFaceN2Puege1FlowrateAct, src, 1, 26 + wordOffset);
            ByteToFloat(ref dst.WFLowerFaceN2Puege1FlowrateSet, src, 1, 28 + wordOffset);
            ByteToFloat(ref dst.WFUpperFaceDIWWashInlet2FlowrateAct, src, 1, 30 + wordOffset);
            ByteToFloat(ref dst.WFUpperFaceDIWWashInlet2FlowrateSet, src, 1, 32 + wordOffset);
            ByteToShort(ref dst.WFUpperFaceIPASupplyAct, src, 1, 34 + wordOffset);
            ByteToShort(ref dst.WFUpperFaceIPASupplySet, src, 1, 35 + wordOffset);
            ByteToShort(ref dst.WFLowerFaceN2Supply2Act, src, 1, 36 + wordOffset);
            ByteToShort(ref dst.WFLowerFaceN2Supply2Set, src, 1, 37 + wordOffset);
            ByteToShort(ref dst.WFUpperFaceDIWSupply1Act, src, 1, 38 + wordOffset);
            ByteToShort(ref dst.WFUpperFaceDIWSupply1Set, src, 1, 39 + wordOffset);
            ByteToShort(ref dst.WFLowerFaceDIWSupplyAct, src, 1, 40 + wordOffset);
            ByteToShort(ref dst.WFLowerFaceDIWSupplySet, src, 1, 41 + wordOffset);
            ByteToFloat(ref dst.N2MixOutletFlowrateAct, src, 1, 42 + wordOffset);
            ByteToFloat(ref dst.N2MixOutletFlowrateSet, src, 1, 44 + wordOffset);
            ByteToFloat(ref dst.ChuckRotationMotorPositionAct, src, 1, 46 + wordOffset);
            ByteToFloat(ref dst.ChuckRotationMotorTorqueActualValueAct, src, 1, 48 + wordOffset);
            ByteToFloat(ref dst.ArmSweepMotorTorqueActualValueAct, src, 1, 50 + wordOffset);
            ByteToFloat(ref dst.WFUpperFaceDIWWash1FlowrateAct, src, 1, 52 + wordOffset);
            ByteToFloat(ref dst.WFLowerFaceDIWWashFlowrateAct, src, 1, 54 + wordOffset);
            ByteToFloat(ref dst.IPAWeighAct, src, 1, 56 + wordOffset);
            ByteToFloat(ref dst.IPAPressureAct, src, 1, 58 + wordOffset);
            ByteToFloat(ref dst.IPALiquidLevelAct, src, 1, 60 + wordOffset);
            ByteToBool(ref dst.ChuckMotorServoOn, src, 1, 62 + wordOffset, 0);
            ByteToBool(ref dst.ArmSweepMotorServoOn, src, 1, 62 + wordOffset, 1);
        }
        private static void ByteToBase_stDryCollectData(Array dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(dst);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                stDryCollectData elemTmp = new stDryCollectData();
                NextIndices(dst, indices);
                ByteTo_stDryCollectData(ref elemTmp, src, len, wordOffset);
                dst.SetValue(elemTmp, indices);
                wordOffset += stDryCollectData_WSIZE;
            }
        }
        public static void ByteTo_stDryCollectData(ref stDryCollectData[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryCollectData(ref stDryCollectData[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryCollectData(ref stDryCollectData[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryCollectData(ref stDryCollectData[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryCollectData(ref stDryCollectData[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryCollectData(ref stDryCollectData[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryCollectData(ref stDryCollectData[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryCollectData(ref stDryCollectData[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryCollectData(dst, src, len, wordOffset);
        }

        public static void ByteTo_stPenCollectData(ref stPenCollectData dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToShort(ref dst._1ProcessStepAct, src, 1, 0 + wordOffset);
            ByteToShort(ref dst._1ProcessStepSet, src, 1, 1 + wordOffset);
            ByteToShort(ref dst._1ProcessTypeAct, src, 1, 2 + wordOffset);
            ByteToShort(ref dst._1ProcessTypeSet, src, 1, 3 + wordOffset);
            ByteToFloat(ref dst._1RecipeTimeAct, src, 1, 4 + wordOffset);
            ByteToFloat(ref dst._1RecipeTimeSet, src, 1, 6 + wordOffset);
            ByteToFloat(ref dst._1ChuckRotationMotorVelocityAct, src, 1, 8 + wordOffset);
            ByteToFloat(ref dst._1ChuckRotationMotorVelocitySet, src, 1, 10 + wordOffset);
            ByteToFloat(ref dst._1PenArmSweepMotorVelocityAct, src, 1, 12 + wordOffset);
            ByteToFloat(ref dst._1PenArmSweepMotorVelocitySet, src, 1, 14 + wordOffset);
            ByteToFloat(ref dst._1PenArmSweepMotorPositionAct, src, 1, 16 + wordOffset);
            ByteToFloat(ref dst._1PenArmSweepMotorPositionSet, src, 1, 18 + wordOffset);
            ByteToShort(ref dst._1PenArmSweepMotorCycleAct, src, 1, 20 + wordOffset);
            ByteToShort(ref dst._1PenArmSweepMotorCycleSet, src, 1, 21 + wordOffset);
            ByteToFloat(ref dst._1PenRotationMotorVelocityAct, src, 1, 22 + wordOffset);
            ByteToFloat(ref dst._1PenRotationMotorVelocitySet, src, 1, 24 + wordOffset);
            ByteToFloat(ref dst._1PenPressureSetValueAct, src, 1, 26 + wordOffset);
            ByteToFloat(ref dst._1PenPressureSetValueSet, src, 1, 28 + wordOffset);
            ByteToFloat(ref dst._1WFUpperFaceN2OutletFlowrateAct, src, 1, 30 + wordOffset);
            ByteToFloat(ref dst._1WFUpperFaceN2OutletFlowrateSet, src, 1, 32 + wordOffset);
            ByteToFloat(ref dst._1WFUpperFaceDIWFlowrateAct, src, 1, 34 + wordOffset);
            ByteToFloat(ref dst._1WFUpperFaceDIWFlowrateSet, src, 1, 36 + wordOffset);
            ByteToFloat(ref dst._1WFLowerFaceDIWFlowrateAct, src, 1, 38 + wordOffset);
            ByteToFloat(ref dst._1WFLowerFaceDIWFlowrateSet, src, 1, 40 + wordOffset);
            ByteToShort(ref dst._1WFLowerFaceN2SupplyAct, src, 1, 42 + wordOffset);
            ByteToShort(ref dst._1WFLowerFaceN2SupplySet, src, 1, 43 + wordOffset);
            ByteToShort(ref dst._1WFUpperLowerFaceChemicalSupplyAct, src, 1, 44 + wordOffset);
            ByteToShort(ref dst._1WFUpperLowerFaceChemicalSupplySet, src, 1, 45 + wordOffset);
            ByteToShort(ref dst._1WFUpperFaceChemicalSupplyAct, src, 1, 46 + wordOffset);
            ByteToShort(ref dst._1WFUpperFaceChemicalSupplySet, src, 1, 47 + wordOffset);
            ByteToShort(ref dst._1WFUpperFaceDICO2SupplyAct, src, 1, 48 + wordOffset);
            ByteToShort(ref dst._1WFUpperFaceDICO2SupplySet, src, 1, 49 + wordOffset);
            ByteToFloat(ref dst._1DIWInletFlowrateAct, src, 1, 50 + wordOffset);
            ByteToFloat(ref dst._1DIWInletFlowrateSet, src, 1, 52 + wordOffset);
            ByteToFloat(ref dst._1DIWSupplyPressureAct, src, 1, 54 + wordOffset);
            ByteToFloat(ref dst._1DIWSupplyPressureSet, src, 1, 56 + wordOffset);
            ByteToFloat(ref dst._1ChemicalFlowrateAct, src, 1, 58 + wordOffset);
            ByteToFloat(ref dst._1ChemicalFlowrateSet, src, 1, 60 + wordOffset);
            ByteToFloat(ref dst._1CO2SupplyPressureAct, src, 1, 62 + wordOffset);
            ByteToFloat(ref dst._1CO2SupplyPressureSet, src, 1, 64 + wordOffset);
            ByteToFloat(ref dst._1ChuckRotationMotorPositionAct, src, 1, 66 + wordOffset);
            ByteToFloat(ref dst._1PenRotationMotorPositionAct, src, 1, 68 + wordOffset);
            ByteToFloat(ref dst._1DIWSupplyPressure1Act, src, 1, 70 + wordOffset);
            ByteToFloat(ref dst._1CarbonatedDIWResistanceAct, src, 1, 72 + wordOffset);
            ByteToFloat(ref dst._1CarbonatedDIWFlowrateAct, src, 1, 74 + wordOffset);
            ByteToFloat(ref dst._1DIWChamberFlowrateAct, src, 1, 76 + wordOffset);
            ByteToFloat(ref dst._1PenDIWWashFlowrateAct, src, 1, 78 + wordOffset);
            ByteToFloat(ref dst._1QuartzDIWWashFlowrateAct, src, 1, 80 + wordOffset);
            ByteToFloat(ref dst._1ChemicalSupplyPressureAct, src, 1, 82 + wordOffset);
            ByteToFloat(ref dst._1QuartzChemicalWashFlowrateAct, src, 1, 84 + wordOffset);
            ByteToFloat(ref dst._1WFUpperFaceWashChemicalFlowrateAct, src, 1, 86 + wordOffset);
            ByteToFloat(ref dst._1WFLowerFaceWashChemicalFlowrateAct, src, 1, 88 + wordOffset);
            ByteToFloat(ref dst._1WFUpperFaceN2PressureAct, src, 1, 90 + wordOffset);
            ByteToFloat(ref dst._1WFLowerFaceN2PressureAct, src, 1, 92 + wordOffset);
            ByteToFloat(ref dst._1WFLowerFaceN2OutletFlowAct, src, 1, 94 + wordOffset);
            ByteToFloat(ref dst._1CUPPurgeFlowAct, src, 1, 96 + wordOffset);
            ByteToFloat(ref dst._1CO2SupplyPressure1Act, src, 1, 98 + wordOffset);
            ByteToFloat(ref dst._1ChuckRotationMotorTorqueActualValueAct, src, 1, 100 + wordOffset);
            ByteToFloat(ref dst._1ArmMotorTorqueActualValueAct, src, 1, 102 + wordOffset);
            ByteToFloat(ref dst._1PenRotationMotorTorqueActualValueAct, src, 1, 104 + wordOffset);
            ByteToBool(ref dst._1ChuckMotorServoOn, src, 1, 106 + wordOffset, 0);
            ByteToBool(ref dst._1PenArmSweepMotorServoOn, src, 1, 106 + wordOffset, 1);
            ByteToBool(ref dst._1PenRotationMotorServoOn, src, 1, 106 + wordOffset, 2);
            ByteToShort(ref dst._2ProcessStepAct, src, 1, 107 + wordOffset);
            ByteToShort(ref dst._2ProcessStepSet, src, 1, 108 + wordOffset);
            ByteToShort(ref dst._2ProcessTypeAct, src, 1, 109 + wordOffset);
            ByteToShort(ref dst._2ProcessTypeSet, src, 1, 110 + wordOffset);
            ByteToFloat(ref dst._2RecipeTimeAct, src, 1, 112 + wordOffset);
            ByteToFloat(ref dst._2RecipeTimeSet, src, 1, 114 + wordOffset);
            ByteToFloat(ref dst._2ChuckRotationMotorVelocityAct, src, 1, 116 + wordOffset);
            ByteToFloat(ref dst._2ChuckRotationMotorVelocitySet, src, 1, 118 + wordOffset);
            ByteToFloat(ref dst._2PenArmSweepMotorVelocityAct, src, 1, 120 + wordOffset);
            ByteToFloat(ref dst._2PenArmSweepMotorVelocitySet, src, 1, 122 + wordOffset);
            ByteToFloat(ref dst._2PenArmSweepMotorPositionAct, src, 1, 124 + wordOffset);
            ByteToFloat(ref dst._2PenArmSweepMotorPositionSet, src, 1, 126 + wordOffset);
            ByteToShort(ref dst._2PenArmSweepMotorCycleAct, src, 1, 128 + wordOffset);
            ByteToShort(ref dst._2PenArmSweepMotorCycleSet, src, 1, 129 + wordOffset);
            ByteToFloat(ref dst._2PenRotationMotorVelocityAct, src, 1, 130 + wordOffset);
            ByteToFloat(ref dst._2PenRotationMotorVelocitySet, src, 1, 132 + wordOffset);
            ByteToFloat(ref dst._2PenPressureSetValueAct, src, 1, 134 + wordOffset);
            ByteToFloat(ref dst._2PenPressureSetValueSet, src, 1, 136 + wordOffset);
            ByteToFloat(ref dst._2WFUpperFaceN2OutletFlowrateAct, src, 1, 138 + wordOffset);
            ByteToFloat(ref dst._2WFUpperFaceN2OutletFlowrateSet, src, 1, 140 + wordOffset);
            ByteToFloat(ref dst._2WFUpperFaceDIWFlowrateAct, src, 1, 142 + wordOffset);
            ByteToFloat(ref dst._2WFUpperFaceDIWFlowrateSet, src, 1, 144 + wordOffset);
            ByteToFloat(ref dst._2WFLowerFaceDIWFlowrateAct, src, 1, 146 + wordOffset);
            ByteToFloat(ref dst._2WFLowerFaceDIWFlowrateSet, src, 1, 148 + wordOffset);
            ByteToShort(ref dst._2WFLowerFaceN2SupplyAct, src, 1, 150 + wordOffset);
            ByteToShort(ref dst._2WFLowerFaceN2SupplySet, src, 1, 151 + wordOffset);
            ByteToShort(ref dst._2WFUpperLowerFaceChemicalSupplyAct, src, 1, 152 + wordOffset);
            ByteToShort(ref dst._2WFUpperLowerFaceChemicalSupplySet, src, 1, 153 + wordOffset);
            ByteToShort(ref dst._2WFUpperFaceChemicalSupplyAct, src, 1, 154 + wordOffset);
            ByteToShort(ref dst._2WFUpperFaceChemicalSupplySet, src, 1, 155 + wordOffset);
            ByteToShort(ref dst._2WFUpperFaceDICO2SupplyAct, src, 1, 156 + wordOffset);
            ByteToShort(ref dst._2WFUpperFaceDICO2SupplySet, src, 1, 157 + wordOffset);
            ByteToFloat(ref dst._2DIWInletFlowrateAct, src, 1, 158 + wordOffset);
            ByteToFloat(ref dst._2DIWInletFlowrateSet, src, 1, 160 + wordOffset);
            ByteToFloat(ref dst._2DIWSupplyPressureAct, src, 1, 162 + wordOffset);
            ByteToFloat(ref dst._2DIWSupplyPressureSet, src, 1, 164 + wordOffset);
            ByteToFloat(ref dst._2ChemicalFlowrateAct, src, 1, 166 + wordOffset);
            ByteToFloat(ref dst._2ChemicalFlowrateSet, src, 1, 168 + wordOffset);
            ByteToFloat(ref dst._2CO2SupplyPressureAct, src, 1, 170 + wordOffset);
            ByteToFloat(ref dst._2CO2SupplyPressureSet, src, 1, 172 + wordOffset);
            ByteToFloat(ref dst._2ChuckRotationMotorPositionAct, src, 1, 174 + wordOffset);
            ByteToFloat(ref dst._2PenRotationMotorPositionAct, src, 1, 176 + wordOffset);
            ByteToFloat(ref dst._2DIWSupplyPressure1Act, src, 1, 178 + wordOffset);
            ByteToFloat(ref dst._2CarbonatedDIWResistanceAct, src, 1, 180 + wordOffset);
            ByteToFloat(ref dst._2CarbonatedDIWFlowrateAct, src, 1, 182 + wordOffset);
            ByteToFloat(ref dst._2DIWChamberFlowrateAct, src, 1, 184 + wordOffset);
            ByteToFloat(ref dst._2PenDIWWashFlowrateAct, src, 1, 186 + wordOffset);
            ByteToFloat(ref dst._2QuartzDIWWashFlowrateAct, src, 1, 188 + wordOffset);
            ByteToFloat(ref dst._2ChemicalSupplyPressureAct, src, 1, 190 + wordOffset);
            ByteToFloat(ref dst._2QuartzChemicalWashFlowrateAct, src, 1, 192 + wordOffset);
            ByteToFloat(ref dst._2WFUpperFaceWashChemicalFlowrateAct, src, 1, 194 + wordOffset);
            ByteToFloat(ref dst._2WFLowerFaceWashChemicalFlowrateAct, src, 1, 196 + wordOffset);
            ByteToFloat(ref dst._2WFUpperFaceN2PressureAct, src, 1, 198 + wordOffset);
            ByteToFloat(ref dst._2WFLowerFaceN2PressureAct, src, 1, 200 + wordOffset);
            ByteToFloat(ref dst._2WFLowerFaceN2OutletFlowAct, src, 1, 202 + wordOffset);
            ByteToFloat(ref dst._2CUPPurgeFlowAct, src, 1, 204 + wordOffset);
            ByteToFloat(ref dst._2CO2SupplyPressure1Act, src, 1, 206 + wordOffset);
            ByteToFloat(ref dst._2ChuckRotationMotorTorqueActualValueAct, src, 1, 208 + wordOffset);
            ByteToFloat(ref dst._2ArmMotorTorqueActualValueAct, src, 1, 210 + wordOffset);
            ByteToFloat(ref dst._2PenRotationMotorTorqueActualValueAct, src, 1, 212 + wordOffset);
            ByteToBool(ref dst._2ChuckMotorServoOn, src, 1, 214 + wordOffset, 0);
            ByteToBool(ref dst._2PenArmSweepMotorServoOn, src, 1, 214 + wordOffset, 1);
            ByteToBool(ref dst._2PenRotationMotorServoOn, src, 1, 214 + wordOffset, 2);
        }
        private static void ByteToBase_stPenCollectData(Array dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(dst);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                stPenCollectData elemTmp = new stPenCollectData();
                NextIndices(dst, indices);
                ByteTo_stPenCollectData(ref elemTmp, src, len, wordOffset);
                dst.SetValue(elemTmp, indices);
                wordOffset += stPenCollectData_WSIZE;
            }
        }
        public static void ByteTo_stPenCollectData(ref stPenCollectData[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenCollectData(ref stPenCollectData[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenCollectData(ref stPenCollectData[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenCollectData(ref stPenCollectData[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenCollectData(ref stPenCollectData[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenCollectData(ref stPenCollectData[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenCollectData(ref stPenCollectData[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenCollectData(ref stPenCollectData[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenCollectData(dst, src, len, wordOffset);
        }

        public static void ByteTo_stPolishCollectData(ref stPolishCollectData dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToShort(ref dst.ProcessStepAct, src, 1, 0 + wordOffset);
            ByteToShort(ref dst.ProcessStepSet, src, 1, 1 + wordOffset);
            ByteToShort(ref dst.ProcessTypeAct, src, 1, 2 + wordOffset);
            ByteToShort(ref dst.ProcessTypeSet, src, 1, 3 + wordOffset);
            ByteToFloat(ref dst.RecipeTimeAct, src, 1, 4 + wordOffset);
            ByteToFloat(ref dst.RecipeTimeSet, src, 1, 6 + wordOffset);
            ByteToShort(ref dst.ExsituConditionProcessStepAct, src, 1, 8 + wordOffset);
            ByteToShort(ref dst.ExsituConditionProcessStepSet, src, 1, 9 + wordOffset);
            ByteToFloat(ref dst.ExsituConditionRecipeTimeAct, src, 1, 10 + wordOffset);
            ByteToFloat(ref dst.ExsituConditionRecipeTimeSet, src, 1, 12 + wordOffset);
            ByteToFloat(ref dst.MainPolishRecipeTimeAct, src, 1, 14 + wordOffset);
            ByteToFloat(ref dst.MainPolishRecipeTimeSet, src, 1, 16 + wordOffset);
            ByteToFloat(ref dst.MainPolishCumulativeTimeAct, src, 1, 18 + wordOffset);
            ByteToFloat(ref dst.MainPolishCumulativeTimeSet, src, 1, 20 + wordOffset);
            ByteToFloat(ref dst.PlatenRotationSpeedAct, src, 1, 22 + wordOffset);
            ByteToFloat(ref dst.PlatenRotationSpeedSet, src, 1, 24 + wordOffset);
            ByteToFloat(ref dst.HeadRotationSpeedAct, src, 1, 26 + wordOffset);
            ByteToFloat(ref dst.HeadRotationSpeedSet, src, 1, 28 + wordOffset);
            ByteToFloat(ref dst.HeadSweepSpeedAct, src, 1, 30 + wordOffset);
            ByteToFloat(ref dst.HeadSweepSpeedSet, src, 1, 32 + wordOffset);
            ByteToFloat(ref dst.HeadSweepPositionAct, src, 1, 34 + wordOffset);
            ByteToFloat(ref dst.HeadSweepPositionSet, src, 1, 36 + wordOffset);
            ByteToShort(ref dst.HeadSweepFrequencyAct, src, 1, 38 + wordOffset);
            ByteToShort(ref dst.HeadSweepFrequencySet, src, 1, 39 + wordOffset);
            ByteToFloat(ref dst.HeadZ_AxisSpeedAct, src, 1, 40 + wordOffset);
            ByteToFloat(ref dst.HeadZ_AxisSpeedSet, src, 1, 42 + wordOffset);
            ByteToFloat(ref dst.HeadZ_AxisPositionAct, src, 1, 44 + wordOffset);
            ByteToFloat(ref dst.HeadZ_AxisPositionSet, src, 1, 46 + wordOffset);
            ByteToFloat(ref dst.ConditionRotationSpeedAct, src, 1, 48 + wordOffset);
            ByteToFloat(ref dst.ConditionRotationSpeedSet, src, 1, 50 + wordOffset);
            ByteToFloat(ref dst.ConditionArmSweepSpeedAct, src, 1, 52 + wordOffset);
            ByteToFloat(ref dst.ConditionArmSweepSpeedSet, src, 1, 54 + wordOffset);
            ByteToFloat(ref dst.ConditionArmSweepPositionAct, src, 1, 56 + wordOffset);
            ByteToFloat(ref dst.ConditionArmSweepPositionSet, src, 1, 58 + wordOffset);
            ByteToShort(ref dst.ConditionArmSweepFrequencyAct, src, 1, 60 + wordOffset);
            ByteToShort(ref dst.ConditionArmSweepFrequencySet, src, 1, 61 + wordOffset);
            ByteToShort(ref dst.SlurryLine1TypeAct, src, 1, 62 + wordOffset);
            ByteToShort(ref dst.SlurryLine2TypeAct, src, 1, 63 + wordOffset);
            ByteToShort(ref dst.SlurryLine3TypeAct, src, 1, 64 + wordOffset);
            ByteToFloat(ref dst.Slurry1OrDIWFlowrateAct, src, 1, 66 + wordOffset);
            ByteToFloat(ref dst.Slurry1OrDIWFlowrateSet, src, 1, 68 + wordOffset);
            ByteToFloat(ref dst.Slurry2OrDIWFlowrateAct, src, 1, 70 + wordOffset);
            ByteToFloat(ref dst.Slurry2OrDIWFlowrateSet, src, 1, 72 + wordOffset);
            ByteToFloat(ref dst.Slurry3OrDIWFlowrateAct, src, 1, 74 + wordOffset);
            ByteToFloat(ref dst.Slurry3OrDIWFlowrateSet, src, 1, 76 + wordOffset);
            ByteToShort(ref dst.Zone1TypeAct, src, 1, 78 + wordOffset);
            ByteToShort(ref dst.Zone2TypeAct, src, 1, 79 + wordOffset);
            ByteToShort(ref dst.Zone3TypeAct, src, 1, 80 + wordOffset);
            ByteToShort(ref dst.Zone4TypeAct, src, 1, 81 + wordOffset);
            ByteToShort(ref dst.Zone5TypeAct, src, 1, 82 + wordOffset);
            ByteToShort(ref dst.Zone6TypeAct, src, 1, 83 + wordOffset);
            ByteToShort(ref dst.Zone7TypeAct, src, 1, 84 + wordOffset);
            ByteToShort(ref dst.Zone8TypeAct, src, 1, 85 + wordOffset);
            ByteToShort(ref dst.RetainerRingTypeAct, src, 1, 86 + wordOffset);
            ByteToFloat(ref dst.Zone1PressureAct, src, 1, 88 + wordOffset);
            ByteToFloat(ref dst.Zone1PressureSet, src, 1, 90 + wordOffset);
            ByteToFloat(ref dst.Zone2PressureAct, src, 1, 92 + wordOffset);
            ByteToFloat(ref dst.Zone2PressureSet, src, 1, 94 + wordOffset);
            ByteToFloat(ref dst.Zone3PressureAct, src, 1, 96 + wordOffset);
            ByteToFloat(ref dst.Zone3PressureSet, src, 1, 98 + wordOffset);
            ByteToFloat(ref dst.Zone4PressureAct, src, 1, 100 + wordOffset);
            ByteToFloat(ref dst.Zone4PressureSet, src, 1, 102 + wordOffset);
            ByteToFloat(ref dst.Zone5PressureAct, src, 1, 104 + wordOffset);
            ByteToFloat(ref dst.Zone5PressureSet, src, 1, 106 + wordOffset);
            ByteToFloat(ref dst.Zone6PressureAct, src, 1, 108 + wordOffset);
            ByteToFloat(ref dst.Zone6PressureSet, src, 1, 110 + wordOffset);
            ByteToFloat(ref dst.Zone7PressureAct, src, 1, 112 + wordOffset);
            ByteToFloat(ref dst.Zone7PressureSet, src, 1, 114 + wordOffset);
            ByteToFloat(ref dst.Zone8PressureAct, src, 1, 116 + wordOffset);
            ByteToFloat(ref dst.Zone8PressureSet, src, 1, 118 + wordOffset);
            ByteToFloat(ref dst.RetainerRingAirBagPressureAct, src, 1, 120 + wordOffset);
            ByteToFloat(ref dst.RetainerRingAirBagPressureSet, src, 1, 122 + wordOffset);
            ByteToFloat(ref dst.ConditionDownForcePressureAct, src, 1, 124 + wordOffset);
            ByteToFloat(ref dst.ConditionDownForcePressureSet, src, 1, 126 + wordOffset);
            ByteToFloat(ref dst.SlurryNozzleSpeedAct, src, 1, 128 + wordOffset);
            ByteToFloat(ref dst.SlurryNozzleSpeedSet, src, 1, 130 + wordOffset);
            ByteToFloat(ref dst.SlurryNozzlePositionAct, src, 1, 132 + wordOffset);
            ByteToFloat(ref dst.SlurryNozzlePositionSet, src, 1, 134 + wordOffset);
            ByteToFloat(ref dst.PlatenRotationAngleAct, src, 1, 136 + wordOffset);
            ByteToFloat(ref dst.HeadRotationAngleAct, src, 1, 138 + wordOffset);
            ByteToFloat(ref dst.ConditionRotationAngleAct, src, 1, 140 + wordOffset);
            ByteToFloat(ref dst.Slurry1Press1Act, src, 1, 142 + wordOffset);
            ByteToFloat(ref dst.Slurry1Press2Act, src, 1, 144 + wordOffset);
            ByteToFloat(ref dst.Slurry2Press1Act, src, 1, 146 + wordOffset);
            ByteToFloat(ref dst.Slurry2Press2Act, src, 1, 148 + wordOffset);
            ByteToFloat(ref dst.Slurry3Press1Act, src, 1, 150 + wordOffset);
            ByteToFloat(ref dst.Slurry3Press2Act, src, 1, 152 + wordOffset);
            ByteToFloat(ref dst.DressingWaterFlowrateAct, src, 1, 154 + wordOffset);
            ByteToFloat(ref dst.PlatenWashTubePressAct, src, 1, 156 + wordOffset);
            ByteToFloat(ref dst.PlatenWashTubeFlowrateAct, src, 1, 158 + wordOffset);
            ByteToFloat(ref dst.ConditionDIWWashTubeFlowrateAct, src, 1, 160 + wordOffset);
            ByteToFloat(ref dst.ConditionArmDIWWashTubeFlowrateAct, src, 1, 162 + wordOffset);
            ByteToFloat(ref dst.RR_GapCleaningTubeFlowrateAct, src, 1, 164 + wordOffset);
            ByteToFloat(ref dst.ReleaseDIWTubeRinseAct, src, 1, 166 + wordOffset);
            ByteToFloat(ref dst.LTPWaferRinseFlowrateAct, src, 1, 168 + wordOffset);
            ByteToFloat(ref dst.TRCleaningTubeFlowrateAct, src, 1, 170 + wordOffset);
            ByteToFloat(ref dst.PlatenCoolingWaterFlowrateAct, src, 1, 172 + wordOffset);
            ByteToFloat(ref dst.PlatenMotorCoolingWaterFlowrateAct, src, 1, 174 + wordOffset);
            ByteToFloat(ref dst.Zone1FlowrateAct, src, 1, 176 + wordOffset);
            ByteToFloat(ref dst.Zone2FlowrateAct, src, 1, 178 + wordOffset);
            ByteToFloat(ref dst.Zone3FlowrateAct, src, 1, 180 + wordOffset);
            ByteToFloat(ref dst.Zone4FlowrateAct, src, 1, 182 + wordOffset);
            ByteToFloat(ref dst.Zone5FlowrateAct, src, 1, 184 + wordOffset);
            ByteToFloat(ref dst.Zone6FlowrateAct, src, 1, 186 + wordOffset);
            ByteToFloat(ref dst.Zone7FlowrateAct, src, 1, 188 + wordOffset);
            ByteToFloat(ref dst.Zone8FlowrateAct, src, 1, 190 + wordOffset);
            ByteToFloat(ref dst.Zone4N2PressAct, src, 1, 192 + wordOffset);
            ByteToFloat(ref dst.RetainingRingAirBagFlowrateAct, src, 1, 194 + wordOffset);
            ByteToFloat(ref dst.VacuumPressAct, src, 1, 196 + wordOffset);
            ByteToFloat(ref dst.ReleaseN2PurgeFlowRateAct, src, 1, 198 + wordOffset);
            ByteToFloat(ref dst.ConditionHeadHeightPositionAct, src, 1, 200 + wordOffset);
            ByteToFloat(ref dst.RetainerRingHeightAct, src, 1, 202 + wordOffset);
            ByteToFloat(ref dst.Zone1PressurePVAct, src, 1, 204 + wordOffset);
            ByteToFloat(ref dst.Zone1PressurePVSet, src, 1, 206 + wordOffset);
            ByteToFloat(ref dst.Zone2PressurePVAct, src, 1, 208 + wordOffset);
            ByteToFloat(ref dst.Zone2PressurePVSet, src, 1, 210 + wordOffset);
            ByteToFloat(ref dst.Zone3PressurePVAct, src, 1, 212 + wordOffset);
            ByteToFloat(ref dst.Zone3PressurePVSet, src, 1, 214 + wordOffset);
            ByteToFloat(ref dst.Zone4PressurePVAct, src, 1, 216 + wordOffset);
            ByteToFloat(ref dst.Zone4PressurePVSet, src, 1, 218 + wordOffset);
            ByteToFloat(ref dst.Zone5PressurePVAct, src, 1, 220 + wordOffset);
            ByteToFloat(ref dst.Zone5PressurePVSet, src, 1, 222 + wordOffset);
            ByteToFloat(ref dst.Zone6PressurePVAct, src, 1, 224 + wordOffset);
            ByteToFloat(ref dst.Zone6PressurePVSet, src, 1, 226 + wordOffset);
            ByteToFloat(ref dst.Zone7PressurePVAct, src, 1, 228 + wordOffset);
            ByteToFloat(ref dst.Zone7PressurePVSet, src, 1, 230 + wordOffset);
            ByteToFloat(ref dst.Zone8PressurePVAct, src, 1, 232 + wordOffset);
            ByteToFloat(ref dst.Zone8PressurePVSet, src, 1, 234 + wordOffset);
            ByteToFloat(ref dst.RetainerRingAirBagPressurePVAct, src, 1, 236 + wordOffset);
            ByteToFloat(ref dst.RetainerRingAirBagPressurePVSet, src, 1, 238 + wordOffset);
            ByteToFloat(ref dst.ConditionDownForcePressurePVAct, src, 1, 240 + wordOffset);
            ByteToFloat(ref dst.ConditionDownForcePressurePVSet, src, 1, 242 + wordOffset);
            ByteToFloat(ref dst.ConditionDownForceInlineAct, src, 1, 244 + wordOffset);
            ByteToFloat(ref dst.PadTemperatureAct, src, 1, 246 + wordOffset);
            ByteToFloat(ref dst.ConditionCenterRadiusPositionAct, src, 1, 248 + wordOffset);
            ByteToFloat(ref dst.PlatenRotationMotorTorqueAct, src, 1, 250 + wordOffset);
            ByteToFloat(ref dst.HeadRotationMotorTorqueAct, src, 1, 252 + wordOffset);
            ByteToFloat(ref dst.HeadSweepMotorTorqueAct, src, 1, 254 + wordOffset);
            ByteToFloat(ref dst.HeadZ_AxisMotorTorqueAct, src, 1, 256 + wordOffset);
            ByteToFloat(ref dst.ConditionRotationMotorTorqueAct, src, 1, 258 + wordOffset);
            ByteToFloat(ref dst.ConditionSweepMotorTorqueAct, src, 1, 260 + wordOffset);
            ByteToFloat(ref dst.SlurryNozzleMotorTorqueAct, src, 1, 262 + wordOffset);
            ByteToBool(ref dst.PlatenRotationMotorServoOn, src, 1, 264 + wordOffset, 0);
            ByteToBool(ref dst.HeadRotationMotorServoOn, src, 1, 264 + wordOffset, 1);
            ByteToBool(ref dst.HeadSweepMotorServoOn, src, 1, 264 + wordOffset, 2);
            ByteToBool(ref dst.HeadUpDownMotorServoOn, src, 1, 264 + wordOffset, 3);
            ByteToBool(ref dst.ConditionHeadRotationMotorServoOn, src, 1, 264 + wordOffset, 4);
            ByteToBool(ref dst.ConditionSweepMotorServoOn, src, 1, 264 + wordOffset, 5);
            ByteToBool(ref dst.SlurrySweepMotorServoOn, src, 1, 264 + wordOffset, 6);
            ByteToFloat(ref dst.Zone1PressureInlineAct, src, 1, 266 + wordOffset);
            ByteToFloat(ref dst.Zone2PressureInlineAct, src, 1, 268 + wordOffset);
            ByteToFloat(ref dst.Zone3PressureInlineAct, src, 1, 270 + wordOffset);
            ByteToFloat(ref dst.Zone4PressureInlineAct, src, 1, 272 + wordOffset);
            ByteToFloat(ref dst.Zone5PressureInlineAct, src, 1, 274 + wordOffset);
            ByteToFloat(ref dst.Zone6PressureInlineAct, src, 1, 276 + wordOffset);
            ByteToFloat(ref dst.Zone7PressureInlineAct, src, 1, 278 + wordOffset);
            ByteToFloat(ref dst.Zone8PressureInlineAct, src, 1, 280 + wordOffset);
            ByteToFloat(ref dst.RetainerRingAirBagPressureInlineAct, src, 1, 282 + wordOffset);
            ByteToFloat(ref dst.ConditionDownForcePressureInlineAct, src, 1, 284 + wordOffset);
            ByteToFloat(ref dst.RetainerRingPressureAct, src, 1, 286 + wordOffset);
            ByteToFloat(ref dst.RetainerRingPressureSet, src, 1, 288 + wordOffset);
            ByteToShort(ref dst.UnitWaferCount, src, 1, 290 + wordOffset);
            ByteToShort(ref dst.AirBagWaferCount, src, 1, 291 + wordOffset);
            ByteToShort(ref dst.AirBagUesTime, src, 1, 292 + wordOffset);
            ByteToShort(ref dst.MembranceWaferCount, src, 1, 293 + wordOffset);
            ByteToShort(ref dst.MembranceUesTime, src, 1, 294 + wordOffset);
            ByteToShort(ref dst.RatainerRingWaferCount, src, 1, 295 + wordOffset);
            ByteToShort(ref dst.RatainerRingUesTime, src, 1, 296 + wordOffset);
            ByteToShort(ref dst.PadWaferCount, src, 1, 297 + wordOffset);
            ByteToShort(ref dst.PadUesTime, src, 1, 298 + wordOffset);
            ByteToShort(ref dst.ConditionInsiUesTime, src, 1, 299 + wordOffset);
            ByteToShort(ref dst.ConditionExsiUesTime, src, 1, 300 + wordOffset);
            ByteToShort(ref dst.UnitIdleTimeh, src, 1, 301 + wordOffset);
            ByteToShort(ref dst.UnitIdleTimemin, src, 1, 302 + wordOffset);
            ByteToShort(ref dst.UnitIdleTimes, src, 1, 303 + wordOffset);
}
        private static void ByteToBase_stPolishCollectData(Array dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(dst);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                stPolishCollectData elemTmp = new stPolishCollectData();
                NextIndices(dst, indices);
                ByteTo_stPolishCollectData(ref elemTmp, src, len, wordOffset);
                dst.SetValue(elemTmp, indices);
                wordOffset += stPolishCollectData_WSIZE;
            }
        }
        public static void ByteTo_stPolishCollectData(ref stPolishCollectData[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishCollectData(ref stPolishCollectData[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishCollectData(ref stPolishCollectData[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishCollectData(ref stPolishCollectData[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishCollectData(ref stPolishCollectData[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishCollectData(ref stPolishCollectData[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishCollectData(ref stPolishCollectData[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishCollectData(ref stPolishCollectData[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishCollectData(dst, src, len, wordOffset);
        }

        public static void ByteTo_stRollerCollectData(ref stRollerCollectData dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToShort(ref dst._1ProcessStepAct, src, 1, 0 + wordOffset);
            ByteToShort(ref dst._1ProcessStepSet, src, 1, 1 + wordOffset);
            ByteToShort(ref dst._1ProcessTypeAct, src, 1, 2 + wordOffset);
            ByteToShort(ref dst._1ProcessTypeSet, src, 1, 3 + wordOffset);
            ByteToFloat(ref dst._1RecipeTimeAct, src, 1, 4 + wordOffset);
            ByteToFloat(ref dst._1RecipeTimeSet, src, 1, 6 + wordOffset);
            ByteToFloat(ref dst._1UpperRollerRotateVelocityAct, src, 1, 8 + wordOffset);
            ByteToFloat(ref dst._1UpperRollerRotateVelocitySet, src, 1, 10 + wordOffset);
            ByteToFloat(ref dst._1LowerRollerRotateVelocityAct, src, 1, 12 + wordOffset);
            ByteToFloat(ref dst._1LowerRollerRotateVelocitySet, src, 1, 14 + wordOffset);
            ByteToFloat(ref dst._1WFLeftPillarRotateMotorVelocityAct, src, 1, 16 + wordOffset);
            ByteToFloat(ref dst._1WFLeftPillarRotateMotorVelocitySet, src, 1, 18 + wordOffset);
            ByteToFloat(ref dst._1WFRightPillarRotateMotorVelocityAct, src, 1, 20 + wordOffset);
            ByteToFloat(ref dst._1WFRightPillarRotateMotorVelocitySet, src, 1, 22 + wordOffset);
            ByteToFloat(ref dst._1UpperRollerPressureAct, src, 1, 24 + wordOffset);
            ByteToFloat(ref dst._1UpperRollerPressureSet, src, 1, 26 + wordOffset);
            ByteToFloat(ref dst._1LowerRollerPressureAct, src, 1, 28 + wordOffset);
            ByteToFloat(ref dst._1LowerRollerPressureSet, src, 1, 30 + wordOffset);
            ByteToFloat(ref dst._1WFUpperFaceDIWFlowrateAct, src, 1, 32 + wordOffset);
            ByteToFloat(ref dst._1WFUpperFaceDIWFlowrateSet, src, 1, 34 + wordOffset);
            ByteToFloat(ref dst._1WFLowerFaceDIWFlowrateAct, src, 1, 36 + wordOffset);
            ByteToFloat(ref dst._1WFLowerFaceDIWFlowrateSet, src, 1, 38 + wordOffset);
            ByteToShort(ref dst._1WFUpperFaceChemicalSupplyAct, src, 1, 40 + wordOffset);
            ByteToShort(ref dst._1WFUpperFaceChemicalSupplySet, src, 1, 41 + wordOffset);
            ByteToShort(ref dst._1WFLowerFaceChemicalSupplyAct, src, 1, 42 + wordOffset);
            ByteToShort(ref dst._1WFLowerFaceChemicalSupplySet, src, 1, 43 + wordOffset);
            ByteToShort(ref dst._1MegaSonicSupplyAct, src, 1, 44 + wordOffset);
            ByteToShort(ref dst._1MegaSonicSupplySet, src, 1, 45 + wordOffset);
            ByteToFloat(ref dst._1ChemicalEnterFlowratemeteAct, src, 1, 46 + wordOffset);
            ByteToFloat(ref dst._1ChemicalEnterFlowratemeteSet, src, 1, 48 + wordOffset);
            ByteToFloat(ref dst._1DIWEnterFlowratemeteAct, src, 1, 50 + wordOffset);
            ByteToFloat(ref dst._1DIWEnterFlowratemeteSet, src, 1, 52 + wordOffset);
            ByteToFloat(ref dst._1WFLeftPillarRotateMotorAngleAct, src, 1, 54 + wordOffset);
            ByteToFloat(ref dst._1WFRightPillarRotateMotorAngleAct, src, 1, 56 + wordOffset);
            ByteToFloat(ref dst._1UpperRollerRotateMotorAngleAct, src, 1, 58 + wordOffset);
            ByteToFloat(ref dst._1LowerRollerRotateMotorAngleAct, src, 1, 60 + wordOffset);
            ByteToFloat(ref dst._1ChemicalEntrancePressureAct, src, 1, 62 + wordOffset);
            ByteToFloat(ref dst._1WFLowerFaceChemicalWashPressureAct, src, 1, 64 + wordOffset);
            ByteToFloat(ref dst._1WFUpperFaceChemicalWashFlowrateAct, src, 1, 66 + wordOffset);
            ByteToFloat(ref dst._1QuartzChemicalWashFlowrateAct, src, 1, 68 + wordOffset);
            ByteToFloat(ref dst._1WFLowerFaceDIWWashPressureAct, src, 1, 70 + wordOffset);
            ByteToFloat(ref dst._1UpperRollerDIWWashFlowrateAct, src, 1, 72 + wordOffset);
            ByteToFloat(ref dst._1QuartzDIWWashFlowrateAct, src, 1, 74 + wordOffset);
            ByteToFloat(ref dst._1UpperRollerInsideDIWWashFlowrateAct, src, 1, 76 + wordOffset);
            ByteToFloat(ref dst._1LowerRollerInsideDIWWashFlowrateAct, src, 1, 78 + wordOffset);
            ByteToFloat(ref dst._1MageSonicDIWWashFlowrateAct, src, 1, 80 + wordOffset);
            ByteToFloat(ref dst._1ChamberDIWWashFlowrateAct, src, 1, 82 + wordOffset);
            ByteToFloat(ref dst._1PillarWashFlowrateAct, src, 1, 84 + wordOffset);
            ByteToFloat(ref dst._1CoolingWaterCirculationFlowrateAct, src, 1, 86 + wordOffset);
            ByteToFloat(ref dst._1LowerRollerDownPressureAct, src, 1, 88 + wordOffset);
            ByteToFloat(ref dst._1ClampCylinderPressureAct, src, 1, 90 + wordOffset);
            ByteToFloat(ref dst._1UpperRollerPressure1Act, src, 1, 92 + wordOffset);
            ByteToFloat(ref dst._1ClampPressureAct, src, 1, 94 + wordOffset);
            ByteToFloat(ref dst._1ClampPressureSet, src, 1, 96 + wordOffset);
            ByteToFloat(ref dst._1WFLeftPillarRotateMotorTorqueActualValueAct, src, 1, 98 + wordOffset);
            ByteToFloat(ref dst._1WFRightPillarRotateMotorTorqueActualValueAct, src, 1, 100 + wordOffset);
            ByteToFloat(ref dst._1UpperRollerRotateMotorTorqueActualValueAct, src, 1, 102 + wordOffset);
            ByteToFloat(ref dst._1LowerRollerRotateMotorTorqueActualValueAct, src, 1, 104 + wordOffset);
            ByteToBool(ref dst._1WFLeftPillarMotorServoOn, src, 1, 106 + wordOffset, 0);
            ByteToBool(ref dst._1WFRightPillarMotorServoOn, src, 1, 106 + wordOffset, 1);
            ByteToBool(ref dst._1UpperRollerMotorServoOn, src, 1, 106 + wordOffset, 2);
            ByteToBool(ref dst._1LowerRollerMotorServoOn, src, 1, 106 + wordOffset, 3);
            ByteToShort(ref dst._2ProcessStepAct, src, 1, 107 + wordOffset);
            ByteToShort(ref dst._2ProcessStepSet, src, 1, 108 + wordOffset);
            ByteToShort(ref dst._2ProcessTypeAct, src, 1, 109 + wordOffset);
            ByteToShort(ref dst._2ProcessTypeSet, src, 1, 110 + wordOffset);
            ByteToFloat(ref dst._2RecipeTimeAct, src, 1, 112 + wordOffset);
            ByteToFloat(ref dst._2RecipeTimeSet, src, 1, 114 + wordOffset);
            ByteToFloat(ref dst._2UpperRollerRotateVelocityAct, src, 1, 116 + wordOffset);
            ByteToFloat(ref dst._2UpperRollerRotateVelocitySet, src, 1, 118 + wordOffset);
            ByteToFloat(ref dst._2LowerRollerRotateVelocityAct, src, 1, 120 + wordOffset);
            ByteToFloat(ref dst._2LowerRollerRotateVelocitySet, src, 1, 122 + wordOffset);
            ByteToFloat(ref dst._2WFLeftPillarRotateMotorVelocityAct, src, 1, 124 + wordOffset);
            ByteToFloat(ref dst._2WFLeftPillarRotateMotorVelocitySet, src, 1, 126 + wordOffset);
            ByteToFloat(ref dst._2WFRightPillarRotateMotorVelocityAct, src, 1, 128 + wordOffset);
            ByteToFloat(ref dst._2WFRightPillarRotateMotorVelocitySet, src, 1, 130 + wordOffset);
            ByteToFloat(ref dst._2UpperRollerPressureAct, src, 1, 132 + wordOffset);
            ByteToFloat(ref dst._2UpperRollerPressureSet, src, 1, 134 + wordOffset);
            ByteToFloat(ref dst._2LowerRollerPressureAct, src, 1, 136 + wordOffset);
            ByteToFloat(ref dst._2LowerRollerPressureSet, src, 1, 138 + wordOffset);
            ByteToFloat(ref dst._2WFUpperFaceDIWFlowrateAct, src, 1, 140 + wordOffset);
            ByteToFloat(ref dst._2WFUpperFaceDIWFlowrateSet, src, 1, 142 + wordOffset);
            ByteToFloat(ref dst._2WFLowerFaceDIWFlowrateAct, src, 1, 144 + wordOffset);
            ByteToFloat(ref dst._2WFLowerFaceDIWFlowrateSet, src, 1, 146 + wordOffset);
            ByteToShort(ref dst._2WFUpperFaceChemicalSupplyAct, src, 1, 148 + wordOffset);
            ByteToShort(ref dst._2WFUpperFaceChemicalSupplySet, src, 1, 149 + wordOffset);
            ByteToShort(ref dst._2WFLowerFaceChemicalSupplyAct, src, 1, 150 + wordOffset);
            ByteToShort(ref dst._2WFLowerFaceChemicalSupplySet, src, 1, 151 + wordOffset);
            ByteToShort(ref dst._2MegaSonicSupplyAct, src, 1, 152 + wordOffset);
            ByteToShort(ref dst._2MegaSonicSupplySet, src, 1, 153 + wordOffset);
            ByteToFloat(ref dst._2ChemicalEnterFlowratemeteAct, src, 1, 154 + wordOffset);
            ByteToFloat(ref dst._2ChemicalEnterFlowratemeteSet, src, 1, 156 + wordOffset);
            ByteToFloat(ref dst._2DIWEnterFlowratemeteAct, src, 1, 158 + wordOffset);
            ByteToFloat(ref dst._2DIWEnterFlowratemeteSet, src, 1, 160 + wordOffset);
            ByteToFloat(ref dst._2WFLeftPillarRotateMotorAngleAct, src, 1, 162 + wordOffset);
            ByteToFloat(ref dst._2WFRightPillarRotateMotorAngleAct, src, 1, 164 + wordOffset);
            ByteToFloat(ref dst._2UpperRollerRotateMotorAngleAct, src, 1, 166 + wordOffset);
            ByteToFloat(ref dst._2LowerRollerRotateMotorAngleAct, src, 1, 168 + wordOffset);
            ByteToFloat(ref dst._2ChemicalEntrancePressureAct, src, 1, 170 + wordOffset);
            ByteToFloat(ref dst._2WFLowerFaceChemicalWashPressureAct, src, 1, 172 + wordOffset);
            ByteToFloat(ref dst._2WFUpperFaceChemicalWashFlowrateAct, src, 1, 174 + wordOffset);
            ByteToFloat(ref dst._2QuartzChemicalWashFlowrateAct, src, 1, 176 + wordOffset);
            ByteToFloat(ref dst._2WFLowerFaceDIWWashPressureAct, src, 1, 178 + wordOffset);
            ByteToFloat(ref dst._2UpperRollerDIWWashFlowrateAct, src, 1, 180 + wordOffset);
            ByteToFloat(ref dst._2QuartzDIWWashFlowrateAct, src, 1, 182 + wordOffset);
            ByteToFloat(ref dst._2UpperRollerInsideDIWWashFlowrateAct, src, 1, 184 + wordOffset);
            ByteToFloat(ref dst._2LowerRollerInsideDIWWashFlowrateAct, src, 1, 186 + wordOffset);
            ByteToFloat(ref dst._2MageSonicDIWWashFlowrateAct, src, 1, 188 + wordOffset);
            ByteToFloat(ref dst._2ChamberDIWWashFlowrateAct, src, 1, 190 + wordOffset);
            ByteToFloat(ref dst._2PillarWashFlowrateAct, src, 1, 192 + wordOffset);
            ByteToFloat(ref dst._2CoolingWaterCirculationFlowrateAct, src, 1, 194 + wordOffset);
            ByteToFloat(ref dst._2LowerRollerDownPressureAct, src, 1, 196 + wordOffset);
            ByteToFloat(ref dst._2ClampCylinderPressureAct, src, 1, 198 + wordOffset);
            ByteToFloat(ref dst._2UpperRollerPressure1Act, src, 1, 200 + wordOffset);
            ByteToFloat(ref dst._2ClampPressureAct, src, 1, 202 + wordOffset);
            ByteToFloat(ref dst._2ClampPressureSet, src, 1, 204 + wordOffset);
            ByteToFloat(ref dst._2WFLeftPillarRotateMotorTorqueActualValueAct, src, 1, 206 + wordOffset);
            ByteToFloat(ref dst._2WFRightPillarRotateMotorTorqueActualValueAct, src, 1, 208 + wordOffset);
            ByteToFloat(ref dst._2UpperRollerRotateMotorTorqueActualValueAct, src, 1, 210 + wordOffset);
            ByteToFloat(ref dst._2LowerRollerRotateMotorTorqueActualValueAct, src, 1, 212 + wordOffset);
            ByteToBool(ref dst._2WFLeftPillarMotorServoOn, src, 1, 214 + wordOffset, 0);
            ByteToBool(ref dst._2WFRightPillarMotorServoOn, src, 1, 214 + wordOffset, 1);
            ByteToBool(ref dst._2UpperRollerMotorServoOn, src, 1, 214 + wordOffset, 2);
            ByteToBool(ref dst._2LowerRollerMotorServoOn, src, 1, 214 + wordOffset, 3);
            ByteToShort(ref dst._3ProcessStepAct, src, 1, 215 + wordOffset);
            ByteToShort(ref dst._3ProcessStepSet, src, 1, 216 + wordOffset);
            ByteToShort(ref dst._3ProcessTypeAct, src, 1, 217 + wordOffset);
            ByteToShort(ref dst._3ProcessTypeSet, src, 1, 218 + wordOffset);
            ByteToFloat(ref dst._3RecipeTimeAct, src, 1, 220 + wordOffset);
            ByteToFloat(ref dst._3RecipeTimeSet, src, 1, 222 + wordOffset);
            ByteToFloat(ref dst._3UpperRollerRotateVelocityAct, src, 1, 224 + wordOffset);
            ByteToFloat(ref dst._3UpperRollerRotateVelocitySet, src, 1, 226 + wordOffset);
            ByteToFloat(ref dst._3LowerRollerRotateVelocityAct, src, 1, 228 + wordOffset);
            ByteToFloat(ref dst._3LowerRollerRotateVelocitySet, src, 1, 230 + wordOffset);
            ByteToFloat(ref dst._3WFLeftPillarRotateMotorVelocityAct, src, 1, 232 + wordOffset);
            ByteToFloat(ref dst._3WFLeftPillarRotateMotorVelocitySet, src, 1, 234 + wordOffset);
            ByteToFloat(ref dst._3WFRightPillarRotateMotorVelocityAct, src, 1, 236 + wordOffset);
            ByteToFloat(ref dst._3WFRightPillarRotateMotorVelocitySet, src, 1, 238 + wordOffset);
            ByteToFloat(ref dst._3UpperRollerPressureAct, src, 1, 240 + wordOffset);
            ByteToFloat(ref dst._3UpperRollerPressureSet, src, 1, 242 + wordOffset);
            ByteToFloat(ref dst._3LowerRollerPressureAct, src, 1, 244 + wordOffset);
            ByteToFloat(ref dst._3LowerRollerPressureSet, src, 1, 246 + wordOffset);
            ByteToFloat(ref dst._3WFUpperFaceDIWFlowrateAct, src, 1, 248 + wordOffset);
            ByteToFloat(ref dst._3WFUpperFaceDIWFlowrateSet, src, 1, 250 + wordOffset);
            ByteToFloat(ref dst._3WFLowerFaceDIWFlowrateAct, src, 1, 252 + wordOffset);
            ByteToFloat(ref dst._3WFLowerFaceDIWFlowrateSet, src, 1, 254 + wordOffset);
            ByteToShort(ref dst._3WFUpperFaceChemicalSupplyAct, src, 1, 256 + wordOffset);
            ByteToShort(ref dst._3WFUpperFaceChemicalSupplySet, src, 1, 257 + wordOffset);
            ByteToShort(ref dst._3WFLowerFaceChemicalSupplyAct, src, 1, 258 + wordOffset);
            ByteToShort(ref dst._3WFLowerFaceChemicalSupplySet, src, 1, 259 + wordOffset);
            ByteToShort(ref dst._3MegaSonicSupplyAct, src, 1, 260 + wordOffset);
            ByteToShort(ref dst._3MegaSonicSupplySet, src, 1, 261 + wordOffset);
            ByteToFloat(ref dst._3ChemicalEnterFlowratemeteAct, src, 1, 262 + wordOffset);
            ByteToFloat(ref dst._3ChemicalEnterFlowratemeteSet, src, 1, 264 + wordOffset);
            ByteToFloat(ref dst._3DIWEnterFlowratemeteAct, src, 1, 266 + wordOffset);
            ByteToFloat(ref dst._3DIWEnterFlowratemeteSet, src, 1, 268 + wordOffset);
            ByteToFloat(ref dst._3WFLeftPillarRotateMotorAngleAct, src, 1, 270 + wordOffset);
            ByteToFloat(ref dst._3WFRightPillarRotateMotorAngleAct, src, 1, 272 + wordOffset);
            ByteToFloat(ref dst._3UpperRollerRotateMotorAngleAct, src, 1, 274 + wordOffset);
            ByteToFloat(ref dst._3LowerRollerRotateMotorAngleAct, src, 1, 276 + wordOffset);
            ByteToFloat(ref dst._3ChemicalEntrancePressureAct, src, 1, 278 + wordOffset);
            ByteToFloat(ref dst._3WFLowerFaceChemicalWashPressureAct, src, 1, 280 + wordOffset);
            ByteToFloat(ref dst._3WFUpperFaceChemicalWashFlowrateAct, src, 1, 282 + wordOffset);
            ByteToFloat(ref dst._3QuartzChemicalWashFlowrateAct, src, 1, 284 + wordOffset);
            ByteToFloat(ref dst._3WFLowerFaceDIWWashPressureAct, src, 1, 286 + wordOffset);
            ByteToFloat(ref dst._3UpperRollerDIWWashFlowrateAct, src, 1, 288 + wordOffset);
            ByteToFloat(ref dst._3QuartzDIWWashFlowrateAct, src, 1, 290 + wordOffset);
            ByteToFloat(ref dst._3UpperRollerInsideDIWWashFlowrateAct, src, 1, 292 + wordOffset);
            ByteToFloat(ref dst._3LowerRollerInsideDIWWashFlowrateAct, src, 1, 294 + wordOffset);
            ByteToFloat(ref dst._3MageSonicDIWWashFlowrateAct, src, 1, 296 + wordOffset);
            ByteToFloat(ref dst._3ChamberDIWWashFlowrateAct, src, 1, 298 + wordOffset);
            ByteToFloat(ref dst._3PillarWashFlowrateAct, src, 1, 300 + wordOffset);
            ByteToFloat(ref dst._3CoolingWaterCirculationFlowrateAct, src, 1, 302 + wordOffset);
            ByteToFloat(ref dst._3LowerRollerDownPressureAct, src, 1, 304 + wordOffset);
            ByteToFloat(ref dst._3ClampCylinderPressureAct, src, 1, 306 + wordOffset);
            ByteToFloat(ref dst._3UpperRollerPressure1Act, src, 1, 308 + wordOffset);
            ByteToFloat(ref dst._3ClampPressureAct, src, 1, 310 + wordOffset);
            ByteToFloat(ref dst._3ClampPressureSet, src, 1, 312 + wordOffset);
            ByteToFloat(ref dst._3WFLeftPillarRotateMotorTorqueActualValueAct, src, 1, 314 + wordOffset);
            ByteToFloat(ref dst._3WFRightPillarRotateMotorTorqueActualValueAct, src, 1, 316 + wordOffset);
            ByteToFloat(ref dst._3UpperRollerRotateMotorTorqueActualValueAct, src, 1, 318 + wordOffset);
            ByteToFloat(ref dst._3LowerRollerRotateMotorTorqueActualValueAct, src, 1, 320 + wordOffset);
            ByteToBool(ref dst._3WFLeftPillarMotorServoOn, src, 1, 322 + wordOffset, 0);
            ByteToBool(ref dst._3WFRightPillarMotorServoOn, src, 1, 322 + wordOffset, 1);
            ByteToBool(ref dst._3UpperRollerMotorServoOn, src, 1, 322 + wordOffset, 2);
            ByteToBool(ref dst._3LowerRollerMotorServoOn, src, 1, 322 + wordOffset, 3);
        }
        private static void ByteToBase_stRollerCollectData(Array dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(dst);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                stRollerCollectData elemTmp = new stRollerCollectData();
                NextIndices(dst, indices);
                ByteTo_stRollerCollectData(ref elemTmp, src, len, wordOffset);
                dst.SetValue(elemTmp, indices);
                wordOffset += stRollerCollectData_WSIZE;
            }
        }
        public static void ByteTo_stRollerCollectData(ref stRollerCollectData[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stRollerCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stRollerCollectData(ref stRollerCollectData[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stRollerCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stRollerCollectData(ref stRollerCollectData[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stRollerCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stRollerCollectData(ref stRollerCollectData[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stRollerCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stRollerCollectData(ref stRollerCollectData[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stRollerCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stRollerCollectData(ref stRollerCollectData[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stRollerCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stRollerCollectData(ref stRollerCollectData[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stRollerCollectData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stRollerCollectData(ref stRollerCollectData[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stRollerCollectData(dst, src, len, wordOffset);
        }

        public static void stDryCollectData_ToByte(ref byte[] dst, stDryCollectData src, int len = 1, int wordOffset = 0)
        {
            ShortToByte(ref dst, src.ProcessStepAct, 1, 0 + wordOffset);
            ShortToByte(ref dst, src.ProcessStepSet, 1, 1 + wordOffset);
            ShortToByte(ref dst, src.ProcessTypeAct, 1, 2 + wordOffset);
            ShortToByte(ref dst, src.ProcessTypeSet, 1, 3 + wordOffset);
            FloatToByte(ref dst, src.RecipeTimeAct, 1, 4 + wordOffset);
            FloatToByte(ref dst, src.RecipeTimeSet, 1, 6 + wordOffset);
            FloatToByte(ref dst, src.ChuckRotationMotorVelocityValuAct, 1, 8 + wordOffset);
            FloatToByte(ref dst, src.ChuckRotationMotorVelocityValuSet, 1, 10 + wordOffset);
            FloatToByte(ref dst, src.ArmSweepMotorVelocityAct, 1, 12 + wordOffset);
            FloatToByte(ref dst, src.ArmSweepMotorVelocitySet, 1, 14 + wordOffset);
            FloatToByte(ref dst, src.ArmSweepMotorPositionAct, 1, 16 + wordOffset);
            FloatToByte(ref dst, src.ArmSweepMotorPositionSet, 1, 18 + wordOffset);
            ShortToByte(ref dst, src.ArmSweepMotorCycleAct, 1, 20 + wordOffset);
            ShortToByte(ref dst, src.ArmSweepMotorCycleSet, 1, 21 + wordOffset);
            FloatToByte(ref dst, src.WFUpperFaceN2PuegeFlowrateAct, 1, 22 + wordOffset);
            FloatToByte(ref dst, src.WFUpperFaceN2PuegeFlowrateSet, 1, 24 + wordOffset);
            FloatToByte(ref dst, src.WFLowerFaceN2Puege1FlowrateAct, 1, 26 + wordOffset);
            FloatToByte(ref dst, src.WFLowerFaceN2Puege1FlowrateSet, 1, 28 + wordOffset);
            FloatToByte(ref dst, src.WFUpperFaceDIWWashInlet2FlowrateAct, 1, 30 + wordOffset);
            FloatToByte(ref dst, src.WFUpperFaceDIWWashInlet2FlowrateSet, 1, 32 + wordOffset);
            ShortToByte(ref dst, src.WFUpperFaceIPASupplyAct, 1, 34 + wordOffset);
            ShortToByte(ref dst, src.WFUpperFaceIPASupplySet, 1, 35 + wordOffset);
            ShortToByte(ref dst, src.WFLowerFaceN2Supply2Act, 1, 36 + wordOffset);
            ShortToByte(ref dst, src.WFLowerFaceN2Supply2Set, 1, 37 + wordOffset);
            ShortToByte(ref dst, src.WFUpperFaceDIWSupply1Act, 1, 38 + wordOffset);
            ShortToByte(ref dst, src.WFUpperFaceDIWSupply1Set, 1, 39 + wordOffset);
            ShortToByte(ref dst, src.WFLowerFaceDIWSupplyAct, 1, 40 + wordOffset);
            ShortToByte(ref dst, src.WFLowerFaceDIWSupplySet, 1, 41 + wordOffset);
            FloatToByte(ref dst, src.N2MixOutletFlowrateAct, 1, 42 + wordOffset);
            FloatToByte(ref dst, src.N2MixOutletFlowrateSet, 1, 44 + wordOffset);
            FloatToByte(ref dst, src.ChuckRotationMotorPositionAct, 1, 46 + wordOffset);
            FloatToByte(ref dst, src.ChuckRotationMotorTorqueActualValueAct, 1, 48 + wordOffset);
            FloatToByte(ref dst, src.ArmSweepMotorTorqueActualValueAct, 1, 50 + wordOffset);
            FloatToByte(ref dst, src.WFUpperFaceDIWWash1FlowrateAct, 1, 52 + wordOffset);
            FloatToByte(ref dst, src.WFLowerFaceDIWWashFlowrateAct, 1, 54 + wordOffset);
            FloatToByte(ref dst, src.IPAWeighAct, 1, 56 + wordOffset);
            FloatToByte(ref dst, src.IPAPressureAct, 1, 58 + wordOffset);
            FloatToByte(ref dst, src.IPALiquidLevelAct, 1, 60 + wordOffset);
            BoolToByte(ref dst, src.ChuckMotorServoOn, 1, 62 + wordOffset, 0);
            BoolToByte(ref dst, src.ArmSweepMotorServoOn, 1, 62 + wordOffset, 1);
        }
        private static void stDryCollectData_ToByteBase(ref byte[] dst, Array src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(src);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                NextIndices(src, indices);
                stDryCollectData_ToByte(ref dst, (stDryCollectData)src.GetValue(indices), len, wordOffset);
                wordOffset += stDryCollectData_WSIZE;
            }
        }
        public static void stDryCollectData_ToByte(ref byte[] dst, stDryCollectData[] src, int len = 1, int wordOffset = 0)
        {
            stDryCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryCollectData_ToByte(ref byte[] dst, stDryCollectData[,] src, int len = 1, int wordOffset = 0)
        {
            stDryCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryCollectData_ToByte(ref byte[] dst, stDryCollectData[,,] src, int len = 1, int wordOffset = 0)
        {
            stDryCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryCollectData_ToByte(ref byte[] dst, stDryCollectData[,,,] src, int len = 1, int wordOffset = 0)
        {
            stDryCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryCollectData_ToByte(ref byte[] dst, stDryCollectData[,,,,] src, int len = 1, int wordOffset = 0)
        {
            stDryCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryCollectData_ToByte(ref byte[] dst, stDryCollectData[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stDryCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryCollectData_ToByte(ref byte[] dst, stDryCollectData[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stDryCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryCollectData_ToByte(ref byte[] dst, stDryCollectData[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stDryCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }

        public static void stPenCollectData_ToByte(ref byte[] dst, stPenCollectData src, int len = 1, int wordOffset = 0)
        {
            ShortToByte(ref dst, src._1ProcessStepAct, 1, 0 + wordOffset);
            ShortToByte(ref dst, src._1ProcessStepSet, 1, 1 + wordOffset);
            ShortToByte(ref dst, src._1ProcessTypeAct, 1, 2 + wordOffset);
            ShortToByte(ref dst, src._1ProcessTypeSet, 1, 3 + wordOffset);
            FloatToByte(ref dst, src._1RecipeTimeAct, 1, 4 + wordOffset);
            FloatToByte(ref dst, src._1RecipeTimeSet, 1, 6 + wordOffset);
            FloatToByte(ref dst, src._1ChuckRotationMotorVelocityAct, 1, 8 + wordOffset);
            FloatToByte(ref dst, src._1ChuckRotationMotorVelocitySet, 1, 10 + wordOffset);
            FloatToByte(ref dst, src._1PenArmSweepMotorVelocityAct, 1, 12 + wordOffset);
            FloatToByte(ref dst, src._1PenArmSweepMotorVelocitySet, 1, 14 + wordOffset);
            FloatToByte(ref dst, src._1PenArmSweepMotorPositionAct, 1, 16 + wordOffset);
            FloatToByte(ref dst, src._1PenArmSweepMotorPositionSet, 1, 18 + wordOffset);
            ShortToByte(ref dst, src._1PenArmSweepMotorCycleAct, 1, 20 + wordOffset);
            ShortToByte(ref dst, src._1PenArmSweepMotorCycleSet, 1, 21 + wordOffset);
            FloatToByte(ref dst, src._1PenRotationMotorVelocityAct, 1, 22 + wordOffset);
            FloatToByte(ref dst, src._1PenRotationMotorVelocitySet, 1, 24 + wordOffset);
            FloatToByte(ref dst, src._1PenPressureSetValueAct, 1, 26 + wordOffset);
            FloatToByte(ref dst, src._1PenPressureSetValueSet, 1, 28 + wordOffset);
            FloatToByte(ref dst, src._1WFUpperFaceN2OutletFlowrateAct, 1, 30 + wordOffset);
            FloatToByte(ref dst, src._1WFUpperFaceN2OutletFlowrateSet, 1, 32 + wordOffset);
            FloatToByte(ref dst, src._1WFUpperFaceDIWFlowrateAct, 1, 34 + wordOffset);
            FloatToByte(ref dst, src._1WFUpperFaceDIWFlowrateSet, 1, 36 + wordOffset);
            FloatToByte(ref dst, src._1WFLowerFaceDIWFlowrateAct, 1, 38 + wordOffset);
            FloatToByte(ref dst, src._1WFLowerFaceDIWFlowrateSet, 1, 40 + wordOffset);
            ShortToByte(ref dst, src._1WFLowerFaceN2SupplyAct, 1, 42 + wordOffset);
            ShortToByte(ref dst, src._1WFLowerFaceN2SupplySet, 1, 43 + wordOffset);
            ShortToByte(ref dst, src._1WFUpperLowerFaceChemicalSupplyAct, 1, 44 + wordOffset);
            ShortToByte(ref dst, src._1WFUpperLowerFaceChemicalSupplySet, 1, 45 + wordOffset);
            ShortToByte(ref dst, src._1WFUpperFaceChemicalSupplyAct, 1, 46 + wordOffset);
            ShortToByte(ref dst, src._1WFUpperFaceChemicalSupplySet, 1, 47 + wordOffset);
            ShortToByte(ref dst, src._1WFUpperFaceDICO2SupplyAct, 1, 48 + wordOffset);
            ShortToByte(ref dst, src._1WFUpperFaceDICO2SupplySet, 1, 49 + wordOffset);
            FloatToByte(ref dst, src._1DIWInletFlowrateAct, 1, 50 + wordOffset);
            FloatToByte(ref dst, src._1DIWInletFlowrateSet, 1, 52 + wordOffset);
            FloatToByte(ref dst, src._1DIWSupplyPressureAct, 1, 54 + wordOffset);
            FloatToByte(ref dst, src._1DIWSupplyPressureSet, 1, 56 + wordOffset);
            FloatToByte(ref dst, src._1ChemicalFlowrateAct, 1, 58 + wordOffset);
            FloatToByte(ref dst, src._1ChemicalFlowrateSet, 1, 60 + wordOffset);
            FloatToByte(ref dst, src._1CO2SupplyPressureAct, 1, 62 + wordOffset);
            FloatToByte(ref dst, src._1CO2SupplyPressureSet, 1, 64 + wordOffset);
            FloatToByte(ref dst, src._1ChuckRotationMotorPositionAct, 1, 66 + wordOffset);
            FloatToByte(ref dst, src._1PenRotationMotorPositionAct, 1, 68 + wordOffset);
            FloatToByte(ref dst, src._1DIWSupplyPressure1Act, 1, 70 + wordOffset);
            FloatToByte(ref dst, src._1CarbonatedDIWResistanceAct, 1, 72 + wordOffset);
            FloatToByte(ref dst, src._1CarbonatedDIWFlowrateAct, 1, 74 + wordOffset);
            FloatToByte(ref dst, src._1DIWChamberFlowrateAct, 1, 76 + wordOffset);
            FloatToByte(ref dst, src._1PenDIWWashFlowrateAct, 1, 78 + wordOffset);
            FloatToByte(ref dst, src._1QuartzDIWWashFlowrateAct, 1, 80 + wordOffset);
            FloatToByte(ref dst, src._1ChemicalSupplyPressureAct, 1, 82 + wordOffset);
            FloatToByte(ref dst, src._1QuartzChemicalWashFlowrateAct, 1, 84 + wordOffset);
            FloatToByte(ref dst, src._1WFUpperFaceWashChemicalFlowrateAct, 1, 86 + wordOffset);
            FloatToByte(ref dst, src._1WFLowerFaceWashChemicalFlowrateAct, 1, 88 + wordOffset);
            FloatToByte(ref dst, src._1WFUpperFaceN2PressureAct, 1, 90 + wordOffset);
            FloatToByte(ref dst, src._1WFLowerFaceN2PressureAct, 1, 92 + wordOffset);
            FloatToByte(ref dst, src._1WFLowerFaceN2OutletFlowAct, 1, 94 + wordOffset);
            FloatToByte(ref dst, src._1CUPPurgeFlowAct, 1, 96 + wordOffset);
            FloatToByte(ref dst, src._1CO2SupplyPressure1Act, 1, 98 + wordOffset);
            FloatToByte(ref dst, src._1ChuckRotationMotorTorqueActualValueAct, 1, 100 + wordOffset);
            FloatToByte(ref dst, src._1ArmMotorTorqueActualValueAct, 1, 102 + wordOffset);
            FloatToByte(ref dst, src._1PenRotationMotorTorqueActualValueAct, 1, 104 + wordOffset);
            BoolToByte(ref dst, src._1ChuckMotorServoOn, 1, 106 + wordOffset, 0);
            BoolToByte(ref dst, src._1PenArmSweepMotorServoOn, 1, 106 + wordOffset, 1);
            BoolToByte(ref dst, src._1PenRotationMotorServoOn, 1, 106 + wordOffset, 2);
            ShortToByte(ref dst, src._2ProcessStepAct, 1, 107 + wordOffset);
            ShortToByte(ref dst, src._2ProcessStepSet, 1, 108 + wordOffset);
            ShortToByte(ref dst, src._2ProcessTypeAct, 1, 109 + wordOffset);
            ShortToByte(ref dst, src._2ProcessTypeSet, 1, 110 + wordOffset);
            FloatToByte(ref dst, src._2RecipeTimeAct, 1, 112 + wordOffset);
            FloatToByte(ref dst, src._2RecipeTimeSet, 1, 114 + wordOffset);
            FloatToByte(ref dst, src._2ChuckRotationMotorVelocityAct, 1, 116 + wordOffset);
            FloatToByte(ref dst, src._2ChuckRotationMotorVelocitySet, 1, 118 + wordOffset);
            FloatToByte(ref dst, src._2PenArmSweepMotorVelocityAct, 1, 120 + wordOffset);
            FloatToByte(ref dst, src._2PenArmSweepMotorVelocitySet, 1, 122 + wordOffset);
            FloatToByte(ref dst, src._2PenArmSweepMotorPositionAct, 1, 124 + wordOffset);
            FloatToByte(ref dst, src._2PenArmSweepMotorPositionSet, 1, 126 + wordOffset);
            ShortToByte(ref dst, src._2PenArmSweepMotorCycleAct, 1, 128 + wordOffset);
            ShortToByte(ref dst, src._2PenArmSweepMotorCycleSet, 1, 129 + wordOffset);
            FloatToByte(ref dst, src._2PenRotationMotorVelocityAct, 1, 130 + wordOffset);
            FloatToByte(ref dst, src._2PenRotationMotorVelocitySet, 1, 132 + wordOffset);
            FloatToByte(ref dst, src._2PenPressureSetValueAct, 1, 134 + wordOffset);
            FloatToByte(ref dst, src._2PenPressureSetValueSet, 1, 136 + wordOffset);
            FloatToByte(ref dst, src._2WFUpperFaceN2OutletFlowrateAct, 1, 138 + wordOffset);
            FloatToByte(ref dst, src._2WFUpperFaceN2OutletFlowrateSet, 1, 140 + wordOffset);
            FloatToByte(ref dst, src._2WFUpperFaceDIWFlowrateAct, 1, 142 + wordOffset);
            FloatToByte(ref dst, src._2WFUpperFaceDIWFlowrateSet, 1, 144 + wordOffset);
            FloatToByte(ref dst, src._2WFLowerFaceDIWFlowrateAct, 1, 146 + wordOffset);
            FloatToByte(ref dst, src._2WFLowerFaceDIWFlowrateSet, 1, 148 + wordOffset);
            ShortToByte(ref dst, src._2WFLowerFaceN2SupplyAct, 1, 150 + wordOffset);
            ShortToByte(ref dst, src._2WFLowerFaceN2SupplySet, 1, 151 + wordOffset);
            ShortToByte(ref dst, src._2WFUpperLowerFaceChemicalSupplyAct, 1, 152 + wordOffset);
            ShortToByte(ref dst, src._2WFUpperLowerFaceChemicalSupplySet, 1, 153 + wordOffset);
            ShortToByte(ref dst, src._2WFUpperFaceChemicalSupplyAct, 1, 154 + wordOffset);
            ShortToByte(ref dst, src._2WFUpperFaceChemicalSupplySet, 1, 155 + wordOffset);
            ShortToByte(ref dst, src._2WFUpperFaceDICO2SupplyAct, 1, 156 + wordOffset);
            ShortToByte(ref dst, src._2WFUpperFaceDICO2SupplySet, 1, 157 + wordOffset);
            FloatToByte(ref dst, src._2DIWInletFlowrateAct, 1, 158 + wordOffset);
            FloatToByte(ref dst, src._2DIWInletFlowrateSet, 1, 160 + wordOffset);
            FloatToByte(ref dst, src._2DIWSupplyPressureAct, 1, 162 + wordOffset);
            FloatToByte(ref dst, src._2DIWSupplyPressureSet, 1, 164 + wordOffset);
            FloatToByte(ref dst, src._2ChemicalFlowrateAct, 1, 166 + wordOffset);
            FloatToByte(ref dst, src._2ChemicalFlowrateSet, 1, 168 + wordOffset);
            FloatToByte(ref dst, src._2CO2SupplyPressureAct, 1, 170 + wordOffset);
            FloatToByte(ref dst, src._2CO2SupplyPressureSet, 1, 172 + wordOffset);
            FloatToByte(ref dst, src._2ChuckRotationMotorPositionAct, 1, 174 + wordOffset);
            FloatToByte(ref dst, src._2PenRotationMotorPositionAct, 1, 176 + wordOffset);
            FloatToByte(ref dst, src._2DIWSupplyPressure1Act, 1, 178 + wordOffset);
            FloatToByte(ref dst, src._2CarbonatedDIWResistanceAct, 1, 180 + wordOffset);
            FloatToByte(ref dst, src._2CarbonatedDIWFlowrateAct, 1, 182 + wordOffset);
            FloatToByte(ref dst, src._2DIWChamberFlowrateAct, 1, 184 + wordOffset);
            FloatToByte(ref dst, src._2PenDIWWashFlowrateAct, 1, 186 + wordOffset);
            FloatToByte(ref dst, src._2QuartzDIWWashFlowrateAct, 1, 188 + wordOffset);
            FloatToByte(ref dst, src._2ChemicalSupplyPressureAct, 1, 190 + wordOffset);
            FloatToByte(ref dst, src._2QuartzChemicalWashFlowrateAct, 1, 192 + wordOffset);
            FloatToByte(ref dst, src._2WFUpperFaceWashChemicalFlowrateAct, 1, 194 + wordOffset);
            FloatToByte(ref dst, src._2WFLowerFaceWashChemicalFlowrateAct, 1, 196 + wordOffset);
            FloatToByte(ref dst, src._2WFUpperFaceN2PressureAct, 1, 198 + wordOffset);
            FloatToByte(ref dst, src._2WFLowerFaceN2PressureAct, 1, 200 + wordOffset);
            FloatToByte(ref dst, src._2WFLowerFaceN2OutletFlowAct, 1, 202 + wordOffset);
            FloatToByte(ref dst, src._2CUPPurgeFlowAct, 1, 204 + wordOffset);
            FloatToByte(ref dst, src._2CO2SupplyPressure1Act, 1, 206 + wordOffset);
            FloatToByte(ref dst, src._2ChuckRotationMotorTorqueActualValueAct, 1, 208 + wordOffset);
            FloatToByte(ref dst, src._2ArmMotorTorqueActualValueAct, 1, 210 + wordOffset);
            FloatToByte(ref dst, src._2PenRotationMotorTorqueActualValueAct, 1, 212 + wordOffset);
            BoolToByte(ref dst, src._2ChuckMotorServoOn, 1, 214 + wordOffset, 0);
            BoolToByte(ref dst, src._2PenArmSweepMotorServoOn, 1, 214 + wordOffset, 1);
            BoolToByte(ref dst, src._2PenRotationMotorServoOn, 1, 214 + wordOffset, 2);
        }
        private static void stPenCollectData_ToByteBase(ref byte[] dst, Array src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(src);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                NextIndices(src, indices);
                stPenCollectData_ToByte(ref dst, (stPenCollectData)src.GetValue(indices), len, wordOffset);
                wordOffset += stPenCollectData_WSIZE;
            }
        }
        public static void stPenCollectData_ToByte(ref byte[] dst, stPenCollectData[] src, int len = 1, int wordOffset = 0)
        {
            stPenCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenCollectData_ToByte(ref byte[] dst, stPenCollectData[,] src, int len = 1, int wordOffset = 0)
        {
            stPenCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenCollectData_ToByte(ref byte[] dst, stPenCollectData[,,] src, int len = 1, int wordOffset = 0)
        {
            stPenCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenCollectData_ToByte(ref byte[] dst, stPenCollectData[,,,] src, int len = 1, int wordOffset = 0)
        {
            stPenCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenCollectData_ToByte(ref byte[] dst, stPenCollectData[,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPenCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenCollectData_ToByte(ref byte[] dst, stPenCollectData[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPenCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenCollectData_ToByte(ref byte[] dst, stPenCollectData[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPenCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenCollectData_ToByte(ref byte[] dst, stPenCollectData[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPenCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }

        public static void stPolishCollectData_ToByte(ref byte[] dst, stPolishCollectData src, int len = 1, int wordOffset = 0)
        {
            ShortToByte(ref dst, src.ProcessStepAct, 1, 0 + wordOffset);
            ShortToByte(ref dst, src.ProcessStepSet, 1, 1 + wordOffset);
            ShortToByte(ref dst, src.ProcessTypeAct, 1, 2 + wordOffset);
            ShortToByte(ref dst, src.ProcessTypeSet, 1, 3 + wordOffset);
            FloatToByte(ref dst, src.RecipeTimeAct, 1, 4 + wordOffset);
            FloatToByte(ref dst, src.RecipeTimeSet, 1, 6 + wordOffset);
            ShortToByte(ref dst, src.ExsituConditionProcessStepAct, 1, 8 + wordOffset);
            ShortToByte(ref dst, src.ExsituConditionProcessStepSet, 1, 9 + wordOffset);
            FloatToByte(ref dst, src.ExsituConditionRecipeTimeAct, 1, 10 + wordOffset);
            FloatToByte(ref dst, src.ExsituConditionRecipeTimeSet, 1, 12 + wordOffset);
            FloatToByte(ref dst, src.MainPolishRecipeTimeAct, 1, 14 + wordOffset);
            FloatToByte(ref dst, src.MainPolishRecipeTimeSet, 1, 16 + wordOffset);
            FloatToByte(ref dst, src.MainPolishCumulativeTimeAct, 1, 18 + wordOffset);
            FloatToByte(ref dst, src.MainPolishCumulativeTimeSet, 1, 20 + wordOffset);
            FloatToByte(ref dst, src.PlatenRotationSpeedAct, 1, 22 + wordOffset);
            FloatToByte(ref dst, src.PlatenRotationSpeedSet, 1, 24 + wordOffset);
            FloatToByte(ref dst, src.HeadRotationSpeedAct, 1, 26 + wordOffset);
            FloatToByte(ref dst, src.HeadRotationSpeedSet, 1, 28 + wordOffset);
            FloatToByte(ref dst, src.HeadSweepSpeedAct, 1, 30 + wordOffset);
            FloatToByte(ref dst, src.HeadSweepSpeedSet, 1, 32 + wordOffset);
            FloatToByte(ref dst, src.HeadSweepPositionAct, 1, 34 + wordOffset);
            FloatToByte(ref dst, src.HeadSweepPositionSet, 1, 36 + wordOffset);
            ShortToByte(ref dst, src.HeadSweepFrequencyAct, 1, 38 + wordOffset);
            ShortToByte(ref dst, src.HeadSweepFrequencySet, 1, 39 + wordOffset);
            FloatToByte(ref dst, src.HeadZ_AxisSpeedAct, 1, 40 + wordOffset);
            FloatToByte(ref dst, src.HeadZ_AxisSpeedSet, 1, 42 + wordOffset);
            FloatToByte(ref dst, src.HeadZ_AxisPositionAct, 1, 44 + wordOffset);
            FloatToByte(ref dst, src.HeadZ_AxisPositionSet, 1, 46 + wordOffset);
            FloatToByte(ref dst, src.ConditionRotationSpeedAct, 1, 48 + wordOffset);
            FloatToByte(ref dst, src.ConditionRotationSpeedSet, 1, 50 + wordOffset);
            FloatToByte(ref dst, src.ConditionArmSweepSpeedAct, 1, 52 + wordOffset);
            FloatToByte(ref dst, src.ConditionArmSweepSpeedSet, 1, 54 + wordOffset);
            FloatToByte(ref dst, src.ConditionArmSweepPositionAct, 1, 56 + wordOffset);
            FloatToByte(ref dst, src.ConditionArmSweepPositionSet, 1, 58 + wordOffset);
            ShortToByte(ref dst, src.ConditionArmSweepFrequencyAct, 1, 60 + wordOffset);
            ShortToByte(ref dst, src.ConditionArmSweepFrequencySet, 1, 61 + wordOffset);
            ShortToByte(ref dst, src.SlurryLine1TypeAct, 1, 62 + wordOffset);
            ShortToByte(ref dst, src.SlurryLine2TypeAct, 1, 63 + wordOffset);
            ShortToByte(ref dst, src.SlurryLine3TypeAct, 1, 64 + wordOffset);
            FloatToByte(ref dst, src.Slurry1OrDIWFlowrateAct, 1, 66 + wordOffset);
            FloatToByte(ref dst, src.Slurry1OrDIWFlowrateSet, 1, 68 + wordOffset);
            FloatToByte(ref dst, src.Slurry2OrDIWFlowrateAct, 1, 70 + wordOffset);
            FloatToByte(ref dst, src.Slurry2OrDIWFlowrateSet, 1, 72 + wordOffset);
            FloatToByte(ref dst, src.Slurry3OrDIWFlowrateAct, 1, 74 + wordOffset);
            FloatToByte(ref dst, src.Slurry3OrDIWFlowrateSet, 1, 76 + wordOffset);
            ShortToByte(ref dst, src.Zone1TypeAct, 1, 78 + wordOffset);
            ShortToByte(ref dst, src.Zone2TypeAct, 1, 79 + wordOffset);
            ShortToByte(ref dst, src.Zone3TypeAct, 1, 80 + wordOffset);
            ShortToByte(ref dst, src.Zone4TypeAct, 1, 81 + wordOffset);
            ShortToByte(ref dst, src.Zone5TypeAct, 1, 82 + wordOffset);
            ShortToByte(ref dst, src.Zone6TypeAct, 1, 83 + wordOffset);
            ShortToByte(ref dst, src.Zone7TypeAct, 1, 84 + wordOffset);
            ShortToByte(ref dst, src.Zone8TypeAct, 1, 85 + wordOffset);
            ShortToByte(ref dst, src.RetainerRingTypeAct, 1, 86 + wordOffset);
            FloatToByte(ref dst, src.Zone1PressureAct, 1, 88 + wordOffset);
            FloatToByte(ref dst, src.Zone1PressureSet, 1, 90 + wordOffset);
            FloatToByte(ref dst, src.Zone2PressureAct, 1, 92 + wordOffset);
            FloatToByte(ref dst, src.Zone2PressureSet, 1, 94 + wordOffset);
            FloatToByte(ref dst, src.Zone3PressureAct, 1, 96 + wordOffset);
            FloatToByte(ref dst, src.Zone3PressureSet, 1, 98 + wordOffset);
            FloatToByte(ref dst, src.Zone4PressureAct, 1, 100 + wordOffset);
            FloatToByte(ref dst, src.Zone4PressureSet, 1, 102 + wordOffset);
            FloatToByte(ref dst, src.Zone5PressureAct, 1, 104 + wordOffset);
            FloatToByte(ref dst, src.Zone5PressureSet, 1, 106 + wordOffset);
            FloatToByte(ref dst, src.Zone6PressureAct, 1, 108 + wordOffset);
            FloatToByte(ref dst, src.Zone6PressureSet, 1, 110 + wordOffset);
            FloatToByte(ref dst, src.Zone7PressureAct, 1, 112 + wordOffset);
            FloatToByte(ref dst, src.Zone7PressureSet, 1, 114 + wordOffset);
            FloatToByte(ref dst, src.Zone8PressureAct, 1, 116 + wordOffset);
            FloatToByte(ref dst, src.Zone8PressureSet, 1, 118 + wordOffset);
            FloatToByte(ref dst, src.RetainerRingAirBagPressureAct, 1, 120 + wordOffset);
            FloatToByte(ref dst, src.RetainerRingAirBagPressureSet, 1, 122 + wordOffset);
            FloatToByte(ref dst, src.ConditionDownForcePressureAct, 1, 124 + wordOffset);
            FloatToByte(ref dst, src.ConditionDownForcePressureSet, 1, 126 + wordOffset);
            FloatToByte(ref dst, src.SlurryNozzleSpeedAct, 1, 128 + wordOffset);
            FloatToByte(ref dst, src.SlurryNozzleSpeedSet, 1, 130 + wordOffset);
            FloatToByte(ref dst, src.SlurryNozzlePositionAct, 1, 132 + wordOffset);
            FloatToByte(ref dst, src.SlurryNozzlePositionSet, 1, 134 + wordOffset);
            FloatToByte(ref dst, src.PlatenRotationAngleAct, 1, 136 + wordOffset);
            FloatToByte(ref dst, src.HeadRotationAngleAct, 1, 138 + wordOffset);
            FloatToByte(ref dst, src.ConditionRotationAngleAct, 1, 140 + wordOffset);
            FloatToByte(ref dst, src.Slurry1Press1Act, 1, 142 + wordOffset);
            FloatToByte(ref dst, src.Slurry1Press2Act, 1, 144 + wordOffset);
            FloatToByte(ref dst, src.Slurry2Press1Act, 1, 146 + wordOffset);
            FloatToByte(ref dst, src.Slurry2Press2Act, 1, 148 + wordOffset);
            FloatToByte(ref dst, src.Slurry3Press1Act, 1, 150 + wordOffset);
            FloatToByte(ref dst, src.Slurry3Press2Act, 1, 152 + wordOffset);
            FloatToByte(ref dst, src.DressingWaterFlowrateAct, 1, 154 + wordOffset);
            FloatToByte(ref dst, src.PlatenWashTubePressAct, 1, 156 + wordOffset);
            FloatToByte(ref dst, src.PlatenWashTubeFlowrateAct, 1, 158 + wordOffset);
            FloatToByte(ref dst, src.ConditionDIWWashTubeFlowrateAct, 1, 160 + wordOffset);
            FloatToByte(ref dst, src.ConditionArmDIWWashTubeFlowrateAct, 1, 162 + wordOffset);
            FloatToByte(ref dst, src.RR_GapCleaningTubeFlowrateAct, 1, 164 + wordOffset);
            FloatToByte(ref dst, src.ReleaseDIWTubeRinseAct, 1, 166 + wordOffset);
            FloatToByte(ref dst, src.LTPWaferRinseFlowrateAct, 1, 168 + wordOffset);
            FloatToByte(ref dst, src.TRCleaningTubeFlowrateAct, 1, 170 + wordOffset);
            FloatToByte(ref dst, src.PlatenCoolingWaterFlowrateAct, 1, 172 + wordOffset);
            FloatToByte(ref dst, src.PlatenMotorCoolingWaterFlowrateAct, 1, 174 + wordOffset);
            FloatToByte(ref dst, src.Zone1FlowrateAct, 1, 176 + wordOffset);
            FloatToByte(ref dst, src.Zone2FlowrateAct, 1, 178 + wordOffset);
            FloatToByte(ref dst, src.Zone3FlowrateAct, 1, 180 + wordOffset);
            FloatToByte(ref dst, src.Zone4FlowrateAct, 1, 182 + wordOffset);
            FloatToByte(ref dst, src.Zone5FlowrateAct, 1, 184 + wordOffset);
            FloatToByte(ref dst, src.Zone6FlowrateAct, 1, 186 + wordOffset);
            FloatToByte(ref dst, src.Zone7FlowrateAct, 1, 188 + wordOffset);
            FloatToByte(ref dst, src.Zone8FlowrateAct, 1, 190 + wordOffset);
            FloatToByte(ref dst, src.Zone4N2PressAct, 1, 192 + wordOffset);
            FloatToByte(ref dst, src.RetainingRingAirBagFlowrateAct, 1, 194 + wordOffset);
            FloatToByte(ref dst, src.VacuumPressAct, 1, 196 + wordOffset);
            FloatToByte(ref dst, src.ReleaseN2PurgeFlowRateAct, 1, 198 + wordOffset);
            FloatToByte(ref dst, src.ConditionHeadHeightPositionAct, 1, 200 + wordOffset);
            FloatToByte(ref dst, src.RetainerRingHeightAct, 1, 202 + wordOffset);
            FloatToByte(ref dst, src.Zone1PressurePVAct, 1, 204 + wordOffset);
            FloatToByte(ref dst, src.Zone1PressurePVSet, 1, 206 + wordOffset);
            FloatToByte(ref dst, src.Zone2PressurePVAct, 1, 208 + wordOffset);
            FloatToByte(ref dst, src.Zone2PressurePVSet, 1, 210 + wordOffset);
            FloatToByte(ref dst, src.Zone3PressurePVAct, 1, 212 + wordOffset);
            FloatToByte(ref dst, src.Zone3PressurePVSet, 1, 214 + wordOffset);
            FloatToByte(ref dst, src.Zone4PressurePVAct, 1, 216 + wordOffset);
            FloatToByte(ref dst, src.Zone4PressurePVSet, 1, 218 + wordOffset);
            FloatToByte(ref dst, src.Zone5PressurePVAct, 1, 220 + wordOffset);
            FloatToByte(ref dst, src.Zone5PressurePVSet, 1, 222 + wordOffset);
            FloatToByte(ref dst, src.Zone6PressurePVAct, 1, 224 + wordOffset);
            FloatToByte(ref dst, src.Zone6PressurePVSet, 1, 226 + wordOffset);
            FloatToByte(ref dst, src.Zone7PressurePVAct, 1, 228 + wordOffset);
            FloatToByte(ref dst, src.Zone7PressurePVSet, 1, 230 + wordOffset);
            FloatToByte(ref dst, src.Zone8PressurePVAct, 1, 232 + wordOffset);
            FloatToByte(ref dst, src.Zone8PressurePVSet, 1, 234 + wordOffset);
            FloatToByte(ref dst, src.RetainerRingAirBagPressurePVAct, 1, 236 + wordOffset);
            FloatToByte(ref dst, src.RetainerRingAirBagPressurePVSet, 1, 238 + wordOffset);
            FloatToByte(ref dst, src.ConditionDownForcePressurePVAct, 1, 240 + wordOffset);
            FloatToByte(ref dst, src.ConditionDownForcePressurePVSet, 1, 242 + wordOffset);
            FloatToByte(ref dst, src.ConditionDownForceInlineAct, 1, 244 + wordOffset);
            FloatToByte(ref dst, src.PadTemperatureAct, 1, 246 + wordOffset);
            FloatToByte(ref dst, src.ConditionCenterRadiusPositionAct, 1, 248 + wordOffset);
            FloatToByte(ref dst, src.PlatenRotationMotorTorqueAct, 1, 250 + wordOffset);
            FloatToByte(ref dst, src.HeadRotationMotorTorqueAct, 1, 252 + wordOffset);
            FloatToByte(ref dst, src.HeadSweepMotorTorqueAct, 1, 254 + wordOffset);
            FloatToByte(ref dst, src.HeadZ_AxisMotorTorqueAct, 1, 256 + wordOffset);
            FloatToByte(ref dst, src.ConditionRotationMotorTorqueAct, 1, 258 + wordOffset);
            FloatToByte(ref dst, src.ConditionSweepMotorTorqueAct, 1, 260 + wordOffset);
            FloatToByte(ref dst, src.SlurryNozzleMotorTorqueAct, 1, 262 + wordOffset);
            BoolToByte(ref dst, src.PlatenRotationMotorServoOn, 1, 264 + wordOffset, 0);
            BoolToByte(ref dst, src.HeadRotationMotorServoOn, 1, 264 + wordOffset, 1);
            BoolToByte(ref dst, src.HeadSweepMotorServoOn, 1, 264 + wordOffset, 2);
            BoolToByte(ref dst, src.HeadUpDownMotorServoOn, 1, 264 + wordOffset, 3);
            BoolToByte(ref dst, src.ConditionHeadRotationMotorServoOn, 1, 264 + wordOffset, 4);
            BoolToByte(ref dst, src.ConditionSweepMotorServoOn, 1, 264 + wordOffset, 5);
            BoolToByte(ref dst, src.SlurrySweepMotorServoOn, 1, 264 + wordOffset, 6);
            FloatToByte(ref dst, src.Zone1PressureInlineAct, 1, 266 + wordOffset);
            FloatToByte(ref dst, src.Zone2PressureInlineAct, 1, 268 + wordOffset);
            FloatToByte(ref dst, src.Zone3PressureInlineAct, 1, 270 + wordOffset);
            FloatToByte(ref dst, src.Zone4PressureInlineAct, 1, 272 + wordOffset);
            FloatToByte(ref dst, src.Zone5PressureInlineAct, 1, 274 + wordOffset);
            FloatToByte(ref dst, src.Zone6PressureInlineAct, 1, 276 + wordOffset);
            FloatToByte(ref dst, src.Zone7PressureInlineAct, 1, 278 + wordOffset);
            FloatToByte(ref dst, src.Zone8PressureInlineAct, 1, 280 + wordOffset);
            FloatToByte(ref dst, src.RetainerRingAirBagPressureInlineAct, 1, 282 + wordOffset);
            FloatToByte(ref dst, src.ConditionDownForcePressureInlineAct, 1, 284 + wordOffset);
            FloatToByte(ref dst, src.RetainerRingPressureAct, 1, 286 + wordOffset);
            FloatToByte(ref dst, src.RetainerRingPressureSet, 1, 288 + wordOffset);
            ShortToByte(ref dst, src.UnitWaferCount, 1, 290 + wordOffset);
            ShortToByte(ref dst, src.AirBagWaferCount, 1, 291 + wordOffset);
            ShortToByte(ref dst, src.AirBagUesTime, 1, 292 + wordOffset);
            ShortToByte(ref dst, src.MembranceWaferCount, 1, 293 + wordOffset);
            ShortToByte(ref dst, src.MembranceUesTime, 1, 294 + wordOffset);
            ShortToByte(ref dst, src.RatainerRingWaferCount, 1, 295 + wordOffset);
            ShortToByte(ref dst, src.RatainerRingUesTime, 1, 296 + wordOffset);
            ShortToByte(ref dst, src.PadWaferCount, 1, 297 + wordOffset);
            ShortToByte(ref dst, src.PadUesTime, 1, 298 + wordOffset);
            ShortToByte(ref dst, src.ConditionInsiUesTime, 1, 299 + wordOffset);
            ShortToByte(ref dst, src.ConditionExsiUesTime, 1, 300 + wordOffset);
            ShortToByte(ref dst, src.UnitIdleTimeh, 1, 301 + wordOffset);
            ShortToByte(ref dst, src.UnitIdleTimemin, 1, 302 + wordOffset);
            ShortToByte(ref dst, src.UnitIdleTimes, 1, 303 + wordOffset);
}
        private static void stPolishCollectData_ToByteBase(ref byte[] dst, Array src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(src);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                NextIndices(src, indices);
                stPolishCollectData_ToByte(ref dst, (stPolishCollectData)src.GetValue(indices), len, wordOffset);
                wordOffset += stPolishCollectData_WSIZE;
            }
        }
        public static void stPolishCollectData_ToByte(ref byte[] dst, stPolishCollectData[] src, int len = 1, int wordOffset = 0)
        {
            stPolishCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishCollectData_ToByte(ref byte[] dst, stPolishCollectData[,] src, int len = 1, int wordOffset = 0)
        {
            stPolishCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishCollectData_ToByte(ref byte[] dst, stPolishCollectData[,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishCollectData_ToByte(ref byte[] dst, stPolishCollectData[,,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishCollectData_ToByte(ref byte[] dst, stPolishCollectData[,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishCollectData_ToByte(ref byte[] dst, stPolishCollectData[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishCollectData_ToByte(ref byte[] dst, stPolishCollectData[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishCollectData_ToByte(ref byte[] dst, stPolishCollectData[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }

        public static void stRollerCollectData_ToByte(ref byte[] dst, stRollerCollectData src, int len = 1, int wordOffset = 0)
        {
            ShortToByte(ref dst, src._1ProcessStepAct, 1, 0 + wordOffset);
            ShortToByte(ref dst, src._1ProcessStepSet, 1, 1 + wordOffset);
            ShortToByte(ref dst, src._1ProcessTypeAct, 1, 2 + wordOffset);
            ShortToByte(ref dst, src._1ProcessTypeSet, 1, 3 + wordOffset);
            FloatToByte(ref dst, src._1RecipeTimeAct, 1, 4 + wordOffset);
            FloatToByte(ref dst, src._1RecipeTimeSet, 1, 6 + wordOffset);
            FloatToByte(ref dst, src._1UpperRollerRotateVelocityAct, 1, 8 + wordOffset);
            FloatToByte(ref dst, src._1UpperRollerRotateVelocitySet, 1, 10 + wordOffset);
            FloatToByte(ref dst, src._1LowerRollerRotateVelocityAct, 1, 12 + wordOffset);
            FloatToByte(ref dst, src._1LowerRollerRotateVelocitySet, 1, 14 + wordOffset);
            FloatToByte(ref dst, src._1WFLeftPillarRotateMotorVelocityAct, 1, 16 + wordOffset);
            FloatToByte(ref dst, src._1WFLeftPillarRotateMotorVelocitySet, 1, 18 + wordOffset);
            FloatToByte(ref dst, src._1WFRightPillarRotateMotorVelocityAct, 1, 20 + wordOffset);
            FloatToByte(ref dst, src._1WFRightPillarRotateMotorVelocitySet, 1, 22 + wordOffset);
            FloatToByte(ref dst, src._1UpperRollerPressureAct, 1, 24 + wordOffset);
            FloatToByte(ref dst, src._1UpperRollerPressureSet, 1, 26 + wordOffset);
            FloatToByte(ref dst, src._1LowerRollerPressureAct, 1, 28 + wordOffset);
            FloatToByte(ref dst, src._1LowerRollerPressureSet, 1, 30 + wordOffset);
            FloatToByte(ref dst, src._1WFUpperFaceDIWFlowrateAct, 1, 32 + wordOffset);
            FloatToByte(ref dst, src._1WFUpperFaceDIWFlowrateSet, 1, 34 + wordOffset);
            FloatToByte(ref dst, src._1WFLowerFaceDIWFlowrateAct, 1, 36 + wordOffset);
            FloatToByte(ref dst, src._1WFLowerFaceDIWFlowrateSet, 1, 38 + wordOffset);
            ShortToByte(ref dst, src._1WFUpperFaceChemicalSupplyAct, 1, 40 + wordOffset);
            ShortToByte(ref dst, src._1WFUpperFaceChemicalSupplySet, 1, 41 + wordOffset);
            ShortToByte(ref dst, src._1WFLowerFaceChemicalSupplyAct, 1, 42 + wordOffset);
            ShortToByte(ref dst, src._1WFLowerFaceChemicalSupplySet, 1, 43 + wordOffset);
            ShortToByte(ref dst, src._1MegaSonicSupplyAct, 1, 44 + wordOffset);
            ShortToByte(ref dst, src._1MegaSonicSupplySet, 1, 45 + wordOffset);
            FloatToByte(ref dst, src._1ChemicalEnterFlowratemeteAct, 1, 46 + wordOffset);
            FloatToByte(ref dst, src._1ChemicalEnterFlowratemeteSet, 1, 48 + wordOffset);
            FloatToByte(ref dst, src._1DIWEnterFlowratemeteAct, 1, 50 + wordOffset);
            FloatToByte(ref dst, src._1DIWEnterFlowratemeteSet, 1, 52 + wordOffset);
            FloatToByte(ref dst, src._1WFLeftPillarRotateMotorAngleAct, 1, 54 + wordOffset);
            FloatToByte(ref dst, src._1WFRightPillarRotateMotorAngleAct, 1, 56 + wordOffset);
            FloatToByte(ref dst, src._1UpperRollerRotateMotorAngleAct, 1, 58 + wordOffset);
            FloatToByte(ref dst, src._1LowerRollerRotateMotorAngleAct, 1, 60 + wordOffset);
            FloatToByte(ref dst, src._1ChemicalEntrancePressureAct, 1, 62 + wordOffset);
            FloatToByte(ref dst, src._1WFLowerFaceChemicalWashPressureAct, 1, 64 + wordOffset);
            FloatToByte(ref dst, src._1WFUpperFaceChemicalWashFlowrateAct, 1, 66 + wordOffset);
            FloatToByte(ref dst, src._1QuartzChemicalWashFlowrateAct, 1, 68 + wordOffset);
            FloatToByte(ref dst, src._1WFLowerFaceDIWWashPressureAct, 1, 70 + wordOffset);
            FloatToByte(ref dst, src._1UpperRollerDIWWashFlowrateAct, 1, 72 + wordOffset);
            FloatToByte(ref dst, src._1QuartzDIWWashFlowrateAct, 1, 74 + wordOffset);
            FloatToByte(ref dst, src._1UpperRollerInsideDIWWashFlowrateAct, 1, 76 + wordOffset);
            FloatToByte(ref dst, src._1LowerRollerInsideDIWWashFlowrateAct, 1, 78 + wordOffset);
            FloatToByte(ref dst, src._1MageSonicDIWWashFlowrateAct, 1, 80 + wordOffset);
            FloatToByte(ref dst, src._1ChamberDIWWashFlowrateAct, 1, 82 + wordOffset);
            FloatToByte(ref dst, src._1PillarWashFlowrateAct, 1, 84 + wordOffset);
            FloatToByte(ref dst, src._1CoolingWaterCirculationFlowrateAct, 1, 86 + wordOffset);
            FloatToByte(ref dst, src._1LowerRollerDownPressureAct, 1, 88 + wordOffset);
            FloatToByte(ref dst, src._1ClampCylinderPressureAct, 1, 90 + wordOffset);
            FloatToByte(ref dst, src._1UpperRollerPressure1Act, 1, 92 + wordOffset);
            FloatToByte(ref dst, src._1ClampPressureAct, 1, 94 + wordOffset);
            FloatToByte(ref dst, src._1ClampPressureSet, 1, 96 + wordOffset);
            FloatToByte(ref dst, src._1WFLeftPillarRotateMotorTorqueActualValueAct, 1, 98 + wordOffset);
            FloatToByte(ref dst, src._1WFRightPillarRotateMotorTorqueActualValueAct, 1, 100 + wordOffset);
            FloatToByte(ref dst, src._1UpperRollerRotateMotorTorqueActualValueAct, 1, 102 + wordOffset);
            FloatToByte(ref dst, src._1LowerRollerRotateMotorTorqueActualValueAct, 1, 104 + wordOffset);
            BoolToByte(ref dst, src._1WFLeftPillarMotorServoOn, 1, 106 + wordOffset, 0);
            BoolToByte(ref dst, src._1WFRightPillarMotorServoOn, 1, 106 + wordOffset, 1);
            BoolToByte(ref dst, src._1UpperRollerMotorServoOn, 1, 106 + wordOffset, 2);
            BoolToByte(ref dst, src._1LowerRollerMotorServoOn, 1, 106 + wordOffset, 3);
            ShortToByte(ref dst, src._2ProcessStepAct, 1, 107 + wordOffset);
            ShortToByte(ref dst, src._2ProcessStepSet, 1, 108 + wordOffset);
            ShortToByte(ref dst, src._2ProcessTypeAct, 1, 109 + wordOffset);
            ShortToByte(ref dst, src._2ProcessTypeSet, 1, 110 + wordOffset);
            FloatToByte(ref dst, src._2RecipeTimeAct, 1, 112 + wordOffset);
            FloatToByte(ref dst, src._2RecipeTimeSet, 1, 114 + wordOffset);
            FloatToByte(ref dst, src._2UpperRollerRotateVelocityAct, 1, 116 + wordOffset);
            FloatToByte(ref dst, src._2UpperRollerRotateVelocitySet, 1, 118 + wordOffset);
            FloatToByte(ref dst, src._2LowerRollerRotateVelocityAct, 1, 120 + wordOffset);
            FloatToByte(ref dst, src._2LowerRollerRotateVelocitySet, 1, 122 + wordOffset);
            FloatToByte(ref dst, src._2WFLeftPillarRotateMotorVelocityAct, 1, 124 + wordOffset);
            FloatToByte(ref dst, src._2WFLeftPillarRotateMotorVelocitySet, 1, 126 + wordOffset);
            FloatToByte(ref dst, src._2WFRightPillarRotateMotorVelocityAct, 1, 128 + wordOffset);
            FloatToByte(ref dst, src._2WFRightPillarRotateMotorVelocitySet, 1, 130 + wordOffset);
            FloatToByte(ref dst, src._2UpperRollerPressureAct, 1, 132 + wordOffset);
            FloatToByte(ref dst, src._2UpperRollerPressureSet, 1, 134 + wordOffset);
            FloatToByte(ref dst, src._2LowerRollerPressureAct, 1, 136 + wordOffset);
            FloatToByte(ref dst, src._2LowerRollerPressureSet, 1, 138 + wordOffset);
            FloatToByte(ref dst, src._2WFUpperFaceDIWFlowrateAct, 1, 140 + wordOffset);
            FloatToByte(ref dst, src._2WFUpperFaceDIWFlowrateSet, 1, 142 + wordOffset);
            FloatToByte(ref dst, src._2WFLowerFaceDIWFlowrateAct, 1, 144 + wordOffset);
            FloatToByte(ref dst, src._2WFLowerFaceDIWFlowrateSet, 1, 146 + wordOffset);
            ShortToByte(ref dst, src._2WFUpperFaceChemicalSupplyAct, 1, 148 + wordOffset);
            ShortToByte(ref dst, src._2WFUpperFaceChemicalSupplySet, 1, 149 + wordOffset);
            ShortToByte(ref dst, src._2WFLowerFaceChemicalSupplyAct, 1, 150 + wordOffset);
            ShortToByte(ref dst, src._2WFLowerFaceChemicalSupplySet, 1, 151 + wordOffset);
            ShortToByte(ref dst, src._2MegaSonicSupplyAct, 1, 152 + wordOffset);
            ShortToByte(ref dst, src._2MegaSonicSupplySet, 1, 153 + wordOffset);
            FloatToByte(ref dst, src._2ChemicalEnterFlowratemeteAct, 1, 154 + wordOffset);
            FloatToByte(ref dst, src._2ChemicalEnterFlowratemeteSet, 1, 156 + wordOffset);
            FloatToByte(ref dst, src._2DIWEnterFlowratemeteAct, 1, 158 + wordOffset);
            FloatToByte(ref dst, src._2DIWEnterFlowratemeteSet, 1, 160 + wordOffset);
            FloatToByte(ref dst, src._2WFLeftPillarRotateMotorAngleAct, 1, 162 + wordOffset);
            FloatToByte(ref dst, src._2WFRightPillarRotateMotorAngleAct, 1, 164 + wordOffset);
            FloatToByte(ref dst, src._2UpperRollerRotateMotorAngleAct, 1, 166 + wordOffset);
            FloatToByte(ref dst, src._2LowerRollerRotateMotorAngleAct, 1, 168 + wordOffset);
            FloatToByte(ref dst, src._2ChemicalEntrancePressureAct, 1, 170 + wordOffset);
            FloatToByte(ref dst, src._2WFLowerFaceChemicalWashPressureAct, 1, 172 + wordOffset);
            FloatToByte(ref dst, src._2WFUpperFaceChemicalWashFlowrateAct, 1, 174 + wordOffset);
            FloatToByte(ref dst, src._2QuartzChemicalWashFlowrateAct, 1, 176 + wordOffset);
            FloatToByte(ref dst, src._2WFLowerFaceDIWWashPressureAct, 1, 178 + wordOffset);
            FloatToByte(ref dst, src._2UpperRollerDIWWashFlowrateAct, 1, 180 + wordOffset);
            FloatToByte(ref dst, src._2QuartzDIWWashFlowrateAct, 1, 182 + wordOffset);
            FloatToByte(ref dst, src._2UpperRollerInsideDIWWashFlowrateAct, 1, 184 + wordOffset);
            FloatToByte(ref dst, src._2LowerRollerInsideDIWWashFlowrateAct, 1, 186 + wordOffset);
            FloatToByte(ref dst, src._2MageSonicDIWWashFlowrateAct, 1, 188 + wordOffset);
            FloatToByte(ref dst, src._2ChamberDIWWashFlowrateAct, 1, 190 + wordOffset);
            FloatToByte(ref dst, src._2PillarWashFlowrateAct, 1, 192 + wordOffset);
            FloatToByte(ref dst, src._2CoolingWaterCirculationFlowrateAct, 1, 194 + wordOffset);
            FloatToByte(ref dst, src._2LowerRollerDownPressureAct, 1, 196 + wordOffset);
            FloatToByte(ref dst, src._2ClampCylinderPressureAct, 1, 198 + wordOffset);
            FloatToByte(ref dst, src._2UpperRollerPressure1Act, 1, 200 + wordOffset);
            FloatToByte(ref dst, src._2ClampPressureAct, 1, 202 + wordOffset);
            FloatToByte(ref dst, src._2ClampPressureSet, 1, 204 + wordOffset);
            FloatToByte(ref dst, src._2WFLeftPillarRotateMotorTorqueActualValueAct, 1, 206 + wordOffset);
            FloatToByte(ref dst, src._2WFRightPillarRotateMotorTorqueActualValueAct, 1, 208 + wordOffset);
            FloatToByte(ref dst, src._2UpperRollerRotateMotorTorqueActualValueAct, 1, 210 + wordOffset);
            FloatToByte(ref dst, src._2LowerRollerRotateMotorTorqueActualValueAct, 1, 212 + wordOffset);
            BoolToByte(ref dst, src._2WFLeftPillarMotorServoOn, 1, 214 + wordOffset, 0);
            BoolToByte(ref dst, src._2WFRightPillarMotorServoOn, 1, 214 + wordOffset, 1);
            BoolToByte(ref dst, src._2UpperRollerMotorServoOn, 1, 214 + wordOffset, 2);
            BoolToByte(ref dst, src._2LowerRollerMotorServoOn, 1, 214 + wordOffset, 3);
            ShortToByte(ref dst, src._3ProcessStepAct, 1, 215 + wordOffset);
            ShortToByte(ref dst, src._3ProcessStepSet, 1, 216 + wordOffset);
            ShortToByte(ref dst, src._3ProcessTypeAct, 1, 217 + wordOffset);
            ShortToByte(ref dst, src._3ProcessTypeSet, 1, 218 + wordOffset);
            FloatToByte(ref dst, src._3RecipeTimeAct, 1, 220 + wordOffset);
            FloatToByte(ref dst, src._3RecipeTimeSet, 1, 222 + wordOffset);
            FloatToByte(ref dst, src._3UpperRollerRotateVelocityAct, 1, 224 + wordOffset);
            FloatToByte(ref dst, src._3UpperRollerRotateVelocitySet, 1, 226 + wordOffset);
            FloatToByte(ref dst, src._3LowerRollerRotateVelocityAct, 1, 228 + wordOffset);
            FloatToByte(ref dst, src._3LowerRollerRotateVelocitySet, 1, 230 + wordOffset);
            FloatToByte(ref dst, src._3WFLeftPillarRotateMotorVelocityAct, 1, 232 + wordOffset);
            FloatToByte(ref dst, src._3WFLeftPillarRotateMotorVelocitySet, 1, 234 + wordOffset);
            FloatToByte(ref dst, src._3WFRightPillarRotateMotorVelocityAct, 1, 236 + wordOffset);
            FloatToByte(ref dst, src._3WFRightPillarRotateMotorVelocitySet, 1, 238 + wordOffset);
            FloatToByte(ref dst, src._3UpperRollerPressureAct, 1, 240 + wordOffset);
            FloatToByte(ref dst, src._3UpperRollerPressureSet, 1, 242 + wordOffset);
            FloatToByte(ref dst, src._3LowerRollerPressureAct, 1, 244 + wordOffset);
            FloatToByte(ref dst, src._3LowerRollerPressureSet, 1, 246 + wordOffset);
            FloatToByte(ref dst, src._3WFUpperFaceDIWFlowrateAct, 1, 248 + wordOffset);
            FloatToByte(ref dst, src._3WFUpperFaceDIWFlowrateSet, 1, 250 + wordOffset);
            FloatToByte(ref dst, src._3WFLowerFaceDIWFlowrateAct, 1, 252 + wordOffset);
            FloatToByte(ref dst, src._3WFLowerFaceDIWFlowrateSet, 1, 254 + wordOffset);
            ShortToByte(ref dst, src._3WFUpperFaceChemicalSupplyAct, 1, 256 + wordOffset);
            ShortToByte(ref dst, src._3WFUpperFaceChemicalSupplySet, 1, 257 + wordOffset);
            ShortToByte(ref dst, src._3WFLowerFaceChemicalSupplyAct, 1, 258 + wordOffset);
            ShortToByte(ref dst, src._3WFLowerFaceChemicalSupplySet, 1, 259 + wordOffset);
            ShortToByte(ref dst, src._3MegaSonicSupplyAct, 1, 260 + wordOffset);
            ShortToByte(ref dst, src._3MegaSonicSupplySet, 1, 261 + wordOffset);
            FloatToByte(ref dst, src._3ChemicalEnterFlowratemeteAct, 1, 262 + wordOffset);
            FloatToByte(ref dst, src._3ChemicalEnterFlowratemeteSet, 1, 264 + wordOffset);
            FloatToByte(ref dst, src._3DIWEnterFlowratemeteAct, 1, 266 + wordOffset);
            FloatToByte(ref dst, src._3DIWEnterFlowratemeteSet, 1, 268 + wordOffset);
            FloatToByte(ref dst, src._3WFLeftPillarRotateMotorAngleAct, 1, 270 + wordOffset);
            FloatToByte(ref dst, src._3WFRightPillarRotateMotorAngleAct, 1, 272 + wordOffset);
            FloatToByte(ref dst, src._3UpperRollerRotateMotorAngleAct, 1, 274 + wordOffset);
            FloatToByte(ref dst, src._3LowerRollerRotateMotorAngleAct, 1, 276 + wordOffset);
            FloatToByte(ref dst, src._3ChemicalEntrancePressureAct, 1, 278 + wordOffset);
            FloatToByte(ref dst, src._3WFLowerFaceChemicalWashPressureAct, 1, 280 + wordOffset);
            FloatToByte(ref dst, src._3WFUpperFaceChemicalWashFlowrateAct, 1, 282 + wordOffset);
            FloatToByte(ref dst, src._3QuartzChemicalWashFlowrateAct, 1, 284 + wordOffset);
            FloatToByte(ref dst, src._3WFLowerFaceDIWWashPressureAct, 1, 286 + wordOffset);
            FloatToByte(ref dst, src._3UpperRollerDIWWashFlowrateAct, 1, 288 + wordOffset);
            FloatToByte(ref dst, src._3QuartzDIWWashFlowrateAct, 1, 290 + wordOffset);
            FloatToByte(ref dst, src._3UpperRollerInsideDIWWashFlowrateAct, 1, 292 + wordOffset);
            FloatToByte(ref dst, src._3LowerRollerInsideDIWWashFlowrateAct, 1, 294 + wordOffset);
            FloatToByte(ref dst, src._3MageSonicDIWWashFlowrateAct, 1, 296 + wordOffset);
            FloatToByte(ref dst, src._3ChamberDIWWashFlowrateAct, 1, 298 + wordOffset);
            FloatToByte(ref dst, src._3PillarWashFlowrateAct, 1, 300 + wordOffset);
            FloatToByte(ref dst, src._3CoolingWaterCirculationFlowrateAct, 1, 302 + wordOffset);
            FloatToByte(ref dst, src._3LowerRollerDownPressureAct, 1, 304 + wordOffset);
            FloatToByte(ref dst, src._3ClampCylinderPressureAct, 1, 306 + wordOffset);
            FloatToByte(ref dst, src._3UpperRollerPressure1Act, 1, 308 + wordOffset);
            FloatToByte(ref dst, src._3ClampPressureAct, 1, 310 + wordOffset);
            FloatToByte(ref dst, src._3ClampPressureSet, 1, 312 + wordOffset);
            FloatToByte(ref dst, src._3WFLeftPillarRotateMotorTorqueActualValueAct, 1, 314 + wordOffset);
            FloatToByte(ref dst, src._3WFRightPillarRotateMotorTorqueActualValueAct, 1, 316 + wordOffset);
            FloatToByte(ref dst, src._3UpperRollerRotateMotorTorqueActualValueAct, 1, 318 + wordOffset);
            FloatToByte(ref dst, src._3LowerRollerRotateMotorTorqueActualValueAct, 1, 320 + wordOffset);
            BoolToByte(ref dst, src._3WFLeftPillarMotorServoOn, 1, 322 + wordOffset, 0);
            BoolToByte(ref dst, src._3WFRightPillarMotorServoOn, 1, 322 + wordOffset, 1);
            BoolToByte(ref dst, src._3UpperRollerMotorServoOn, 1, 322 + wordOffset, 2);
            BoolToByte(ref dst, src._3LowerRollerMotorServoOn, 1, 322 + wordOffset, 3);
        }
        private static void stRollerCollectData_ToByteBase(ref byte[] dst, Array src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(src);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                NextIndices(src, indices);
                stRollerCollectData_ToByte(ref dst, (stRollerCollectData)src.GetValue(indices), len, wordOffset);
                wordOffset += stRollerCollectData_WSIZE;
            }
        }
        public static void stRollerCollectData_ToByte(ref byte[] dst, stRollerCollectData[] src, int len = 1, int wordOffset = 0)
        {
            stRollerCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stRollerCollectData_ToByte(ref byte[] dst, stRollerCollectData[,] src, int len = 1, int wordOffset = 0)
        {
            stRollerCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stRollerCollectData_ToByte(ref byte[] dst, stRollerCollectData[,,] src, int len = 1, int wordOffset = 0)
        {
            stRollerCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stRollerCollectData_ToByte(ref byte[] dst, stRollerCollectData[,,,] src, int len = 1, int wordOffset = 0)
        {
            stRollerCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stRollerCollectData_ToByte(ref byte[] dst, stRollerCollectData[,,,,] src, int len = 1, int wordOffset = 0)
        {
            stRollerCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stRollerCollectData_ToByte(ref byte[] dst, stRollerCollectData[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stRollerCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stRollerCollectData_ToByte(ref byte[] dst, stRollerCollectData[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stRollerCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stRollerCollectData_ToByte(ref byte[] dst, stRollerCollectData[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stRollerCollectData_ToByteBase(ref dst, src, len, wordOffset);
        }

    }
}
