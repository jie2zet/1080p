using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.kvsPlc.IO
{
    class KvStruct
    {
        const int stDryIOData_WSIZE = 3;
        const int stPenIOData_WSIZE = 7;
        const int stPolishIOData_WSIZE = 11;

        public class stDryIOData
        {
            public bool WFClampLimitSensor = default(bool);
            public bool WFReleaseLimitSensor = default(bool);
            public bool WFInletDoorOpenLimitSensor = default(bool);
            public bool WFInletDoorCloseLimitSensor = default(bool);
            public bool WFOutletDoorOpenLimitSensor = default(bool);
            public bool WFOutletDoorCloseLimitSensor = default(bool);
            public bool WafersearchSensor = default(bool);
            public bool LeakDetectionAlarm = default(bool);
            public bool IPAConcentrationAlarm = default(bool);
            public bool FlameDetectionAlarm = default(bool);
            public bool MicroDifferentialPressureMeter1HighAlarm = default(bool);
            public bool MicroDifferentialPressureMeter2HighAlarm = default(bool);
            public bool MicroDifferentialPressureMeter3HighAlarm = default(bool);
            public bool MicroDifferentialPressureMeter4HighAlarm = default(bool);
            public bool MicroDifferentialPressureMeter1LowAlarm = default(bool);
            public bool MicroDifferentialPressureMeter2LowAlarm = default(bool);
            public bool MicroDifferentialPressureMeter3LowAlarm = default(bool);
            public bool MicroDifferentialPressureMeter4LowAlarm = default(bool);
            public bool WFClampCylinderClampControl = default(bool);
            public bool WFClampCylinderReleaseControl = default(bool);
            public bool WaferInletDoorOpenControl = default(bool);
            public bool WaferInletDoorCloseControl = default(bool);
            public bool WaferOutletDoorOpenControl = default(bool);
            public bool WaferOutletDoorOpenControl_Copy1 = default(bool);
            public bool WFUpperFaceDIWWashInletValve2Control = default(bool);
            public bool WFUpperFaceDIWWashOutnletValve2Control = default(bool);
            public bool IPASupplyValveControl = default(bool);
            public bool WFUpperFaceDIWWashInletValve1Control = default(bool);
            public bool WFUpperFaceDIWWashOutnletValve1Control = default(bool);
            public bool WFLowerFaceDIWWashValveControl = default(bool);
            public bool N2MixOutletValveControl = default(bool);
            public bool WFUpperFaceN2OutletValveControl = default(bool);
            public bool WFLowerFaceN2OutletValve1Control = default(bool);
            public bool N2ReleaseValvesControl = default(bool);
            public bool WFUpperFaceIPAOutletValveControl = default(bool);
            public bool WFLowerFaceN2OutletValve2Control = default(bool);
        }

        public class stPenIOData
        {
            public bool _1PenUpperLimitSensor = default(bool);
            public bool _1PenLowerLimitSensor = default(bool);
            public bool _1ArmUpperLimitSensor = default(bool);
            public bool _1ArmLowerLimitSensor = default(bool);
            public bool _1WFClampLimitSensor = default(bool);
            public bool _1WFReleaseLimitSensor = default(bool);
            public bool _1NozzleUpperLimitSensor = default(bool);
            public bool _1NozzleLowerLimitSensor = default(bool);
            public bool _1WFInletDoorOpenLimitSensor = default(bool);
            public bool _1WFInletDoorCloseLimitSensor = default(bool);
            public bool _1WFOutletDoorOpenLimitSensor = default(bool);
            public bool _1WFOutletDoorCloseLimitSensor = default(bool);
            public bool _1WaferPresentSensor = default(bool);
            public bool _1PenPresentSensor = default(bool);
            public bool _1PenUpDownCylinderUpperControl = default(bool);
            public bool _1PenUpDownCylinderLowerControl = default(bool);
            public bool _1PenArmCylinderUpperControl = default(bool);
            public bool _1PenArmCylinderLowerControl = default(bool);
            public bool _1WFClampCylinderClampControl = default(bool);
            public bool _1WFClampCylinderReleaseControl = default(bool);
            public bool _1NozzleCylinderUpperControl = default(bool);
            public bool _1NozzleCylinderLowerControl = default(bool);
            public bool _1WaferInletDoorOpenControl = default(bool);
            public bool _1WaferInletDoorCloseControl = default(bool);
            public bool _1WaferOutletDoorOpenControl = default(bool);
            public bool _1WaferOutletDoorCloseControl = default(bool);
            public bool _1ChemicalInletValveControl = default(bool);
            public bool _1ChemicalOutletValveControl = default(bool);
            public bool _1DIWInletValveControl = default(bool);
            public bool _1DIWOutletValveControl = default(bool);
            public bool _1WFUpperFaceDIWWashValveControl = default(bool);
            public bool _1WFLowerfaceDIWWashValveControl = default(bool);
            public bool _1DIWSupplyValceControl = default(bool);
            public bool _1PenDIWWashValveControl = default(bool);
            public bool _1QuartzDIWWashValveControl = default(bool);
            public bool _1ChemicalPipelineWashValveControl = default(bool);
            public bool _1QuartzChemicalWashValveControl = default(bool);
            public bool _1WFUpperLowerFacesChemicalWashValveControl = default(bool);
            public bool _1WFUpperFacesChemicalWashValveControl = default(bool);
            public bool _1DIWChamberValveControl = default(bool);
            public bool _1WFUpperFaceCarbonatedWashValve = default(bool);
            public bool _1WFUpperN2OutletValveControl = default(bool);
            public bool _1CO2SupplyValveControl = default(bool);
            public bool _1CUPPurgeValveControl = default(bool);
            public bool _1WFLowerFaceN2OutletValveControl = default(bool);
            public bool _1DIWFullDetection = default(bool);
            public bool _1CavityDoorDetection = default(bool);
            public bool _1DIWCO2PressureDetection = default(bool);
            public bool _1WFLowerfaceN2PressureDetection = default(bool);
            public bool _1LiquidLeakageDetection = default(bool);
            public bool _1N2PressureDetection = default(bool);
            public bool _1DIWPressureDetection = default(bool);
            public bool _1BottomDripPanLeak = default(bool);
            public bool _2PenUpperLimitSensor = default(bool);
            public bool _2PenLowerLimitSensor = default(bool);
            public bool _2ArmUpperLimitSensor = default(bool);
            public bool _2ArmLowerLimitSensor = default(bool);
            public bool _2WFClampLimitSensor = default(bool);
            public bool _2WFReleaseLimitSensor = default(bool);
            public bool _2NozzleUpperLimitSensor = default(bool);
            public bool _2NozzleLowerLimitSensor = default(bool);
            public bool _2WFInletDoorOpenLimitSensor = default(bool);
            public bool _2WFInletDoorCloseLimitSensor = default(bool);
            public bool _2WFOutletDoorOpenLimitSensor = default(bool);
            public bool _2WFOutletDoorCloseLimitSensor = default(bool);
            public bool _2WaferPresentSensor = default(bool);
            public bool _2PenPresentSensor = default(bool);
            public bool _2PenUpDownCylinderUpperControl = default(bool);
            public bool _2PenUpDownCylinderLowerControl = default(bool);
            public bool _2PenArmCylinderUpperControl = default(bool);
            public bool _2PenArmCylinderLowerControl = default(bool);
            public bool _2WFClampCylinderClampControl = default(bool);
            public bool _2WFClampCylinderReleaseControl = default(bool);
            public bool _2NozzleCylinderUpperControl = default(bool);
            public bool _2NozzleCylinderLowerControl = default(bool);
            public bool _2WaferInletDoorOpenControl = default(bool);
            public bool _2WaferInletDoorCloseControl = default(bool);
            public bool _2WaferOutletDoorOpenControl = default(bool);
            public bool _2WaferOutletDoorCloseControl = default(bool);
            public bool _2ChemicalInletValveControl = default(bool);
            public bool _2ChemicalOutletValveControl = default(bool);
            public bool _2DIWInletValveControl = default(bool);
            public bool _2DIWOutletValveControl = default(bool);
            public bool _2WFUpperFaceDIWWashValveControl = default(bool);
            public bool _2WFLowerfaceDIWWashValveControl = default(bool);
            public bool _2DIWSupplyValceControl = default(bool);
            public bool _2PenDIWWashValveControl = default(bool);
            public bool _2QuartzDIWWashValveControl = default(bool);
            public bool _2ChemicalPipelineWashValveControl = default(bool);
            public bool _2QuartzChemicalWashValveControl = default(bool);
            public bool _2WFUpperLowerFacesChemicalWashValveControl = default(bool);
            public bool _2WFUpperFacesChemicalWashValveControl = default(bool);
            public bool _2DIWChamberValveControl = default(bool);
            public bool _2WFUpperFaceCarbonatedWashValve = default(bool);
            public bool _2WFUpperN2OutletValveControl = default(bool);
            public bool _2CO2SupplyValveControl = default(bool);
            public bool _2CUPPurgeValveControl = default(bool);
            public bool _2WFLowerFaceN2OutletValveControl = default(bool);
            public bool _2DIWFullDetection = default(bool);
            public bool _2CavityDoorDetection = default(bool);
            public bool _2DIWCO2PressureDetection = default(bool);
            public bool _2WFLowerfaceN2PressureDetection = default(bool);
            public bool _2LiquidLeakageDetection = default(bool);
            public bool _2N2PressureDetection = default(bool);
            public bool _2DIWPressureDetection = default(bool);
            public bool _2BottomDripPanLeak = default(bool);
        }

        public class stPolishIOData
        {
            public bool PlatenBreakwaterOutsideUpLimitSensor = default(bool);
            public bool PlatenBreakwaterOutsideDownLimitSensor = default(bool);
            public bool WaterSeparatorLiquidLevelSensor = default(bool);
            public bool WaferOutSensor = default(bool);
            public bool PlatenBreakwaterInsideUpLimitSensor = default(bool);
            public bool PlatenBreakwaterInsideDownLimitSensor = default(bool);
            public bool[] Void = new bool[14];
            public bool PlatenBreakwaterUpControl = default(bool);
            public bool PlatenBreakwaterDownControl = default(bool);
            public bool ConditionerHeadUpControl = default(bool);
            public bool ConditionerHeadDownControl = default(bool);
            public bool[] Void1 = new bool[16];
            public bool Slurry1ThreeWayTotalInletValveControl = default(bool);
            public bool Slurry1ThreeWayReturnValveControl = default(bool);
            public bool Slurry1ThreeWayIntletValveControl = default(bool);
            public bool WaterThreeWayIntletValve1Control = default(bool);
            public bool Slurry1OrDIWOutletValveControl = default(bool);
            public bool Slurry2ThreeWayTotalInletValve = default(bool);
            public bool Slurry2ThreeWayReturnValveControl = default(bool);
            public bool Slurry2ThreeWayIntletValveControl = default(bool);
            public bool WaterThreeWayIntletValve2Control = default(bool);
            public bool Slurry2OrDIWOutletValveControl = default(bool);
            public bool Slurry3ThreeWayTotalInletValveControl = default(bool);
            public bool Slurry3ThreeWayReturnValveControl = default(bool);
            public bool Slurry3ThreeWayIntletValveControl = default(bool);
            public bool WaterThreeWayIntletValve3Control = default(bool);
            public bool Slurry3OrDIWOutletValveControl = default(bool);
            public bool[] Void2 = new bool[3];
            public bool DIWOutletValveControl = default(bool);
            public bool PlatenWashValveControl = default(bool);
            public bool MovingHeadDIWWashValveControl = default(bool);
            public bool MovingHeadExternalDIWWashValveControl = default(bool);
            public bool HeadChemicalWashValveControl = default(bool);
            public bool ConditionerDIWWashValveControl = default(bool);
            public bool ConditionerArmDIWWashValveControl = default(bool);
            public bool ChemicalArmDIWWashValveControl = default(bool);
            public bool HeadMiddleWashValveControl = default(bool);
            public bool HeadExternalWashValveControl = default(bool);
            public bool WFUpperFaceChemicalWashValveControl = default(bool);
            public bool WFUpperFaceDIWWashValveControl = default(bool);
            public bool WFLowerFaceChemicalWashValveControl = default(bool);
            public bool WFLowerFaceDIWWashValveControl = default(bool);
            public bool PlatenCoolWaterValveControl = default(bool);
            public bool PlatenMotorCoolWaterValveControl = default(bool);
            public bool Void3 = default(bool);
            public bool Void4 = default(bool);
            public bool Void5 = default(bool);
            public bool Void6 = default(bool);
            public bool Void7 = default(bool);
            public bool Void8 = default(bool);
            public bool Void9 = default(bool);
            public bool Void10 = default(bool);
            public bool Void11 = default(bool);
            public bool Void12 = default(bool);
            public bool Void13 = default(bool);
            public bool Void14 = default(bool);
            public bool Void15 = default(bool);
            public bool Zone1PressValve1Control = default(bool);
            public bool Zone1PressValve2Control = default(bool);
            public bool Zone1VacuumValveControl = default(bool);
            public bool Zone2PressValve1Control = default(bool);
            public bool Zone2PressValve2Control = default(bool);
            public bool Zone2VacuumValveControl = default(bool);
            public bool Zone3PressValve1Control = default(bool);
            public bool Zone3PressValve2Control = default(bool);
            public bool Zone3VacuumValveControl = default(bool);
            public bool Zone4PressValve1Control = default(bool);
            public bool Zone4PressValve2Control = default(bool);
            public bool Zone4VacuumValveControl = default(bool);
            public bool Zone5PressValve1Control = default(bool);
            public bool Zone5PressValve2Control = default(bool);
            public bool Zone5VacuumValveControl = default(bool);
            public bool Zone6PressValve1Control = default(bool);
            public bool Zone6PressValve2Control = default(bool);
            public bool Zone6VacuumValveControl = default(bool);
            public bool Zone7PressValve1Control = default(bool);
            public bool Zone7PressValve2Control = default(bool);
            public bool Zone7VacuumValveControl = default(bool);
            public bool Zone8PressValve1Control = default(bool);
            public bool Zone8PressValve2Control = default(bool);
            public bool Zone8VacuumValveControl = default(bool);
            public bool TopRingPressValve1Control = default(bool);
            public bool TopRingPressValve2Control = default(bool);
            public bool TopRingVacuumValveControl = default(bool);
            public bool Void16 = default(bool);
            public bool Void17 = default(bool);
            public bool Void18 = default(bool);
            public bool Zone4BlowValve1Control = default(bool);
            public bool Zone4BlowValve2Control = default(bool);
            public bool Zone4AtmosphericValveControl = default(bool);
            public bool AirBagVacuumGeneratorIntletValveControl = default(bool);
            public bool AirBagVacuumGeneratorOutletValveControl = default(bool);
            public bool VacuumGeneratorIntletValveControl = default(bool);
            public bool VacuumGeneratorOutletValveControl = default(bool);
            public bool CatchWaterBlowValveControl = default(bool);
            public bool CatchWaterDrainValveControl = default(bool);
            public bool HeadExternalBlowN2ValveControl = default(bool);
            public bool Void19 = default(bool);
            public bool Void20 = default(bool);
            public bool Void21 = default(bool);
            public bool Void22 = default(bool);
            public bool Void23 = default(bool);
            public bool Void24 = default(bool);
            public bool Void25 = default(bool);
            public bool Void26 = default(bool);
            public bool Zone1VentValveControl = default(bool);
            public bool Zone2VentValveControl = default(bool);
            public bool Zone3VentValveControl = default(bool);
            public bool Zone5VentValveControl = default(bool);
            public bool Zone6VentValveControl = default(bool);
            public bool Zone7VentValveControl = default(bool);
            public bool Zone8VentValveControl = default(bool);
            public bool TopRingVentValveControl = default(bool);
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

        public static void ByteTo_stDryIOData(ref stDryIOData dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBool(ref dst.WFClampLimitSensor, src, 1, 0 + wordOffset, 0);
            ByteToBool(ref dst.WFReleaseLimitSensor, src, 1, 0 + wordOffset, 1);
            ByteToBool(ref dst.WFInletDoorOpenLimitSensor, src, 1, 0 + wordOffset, 2);
            ByteToBool(ref dst.WFInletDoorCloseLimitSensor, src, 1, 0 + wordOffset, 3);
            ByteToBool(ref dst.WFOutletDoorOpenLimitSensor, src, 1, 0 + wordOffset, 4);
            ByteToBool(ref dst.WFOutletDoorCloseLimitSensor, src, 1, 0 + wordOffset, 5);
            ByteToBool(ref dst.WafersearchSensor, src, 1, 0 + wordOffset, 6);
            ByteToBool(ref dst.LeakDetectionAlarm, src, 1, 0 + wordOffset, 7);
            ByteToBool(ref dst.IPAConcentrationAlarm, src, 1, 0 + wordOffset, 8);
            ByteToBool(ref dst.FlameDetectionAlarm, src, 1, 0 + wordOffset, 9);
            ByteToBool(ref dst.MicroDifferentialPressureMeter1HighAlarm, src, 1, 0 + wordOffset, 10);
            ByteToBool(ref dst.MicroDifferentialPressureMeter2HighAlarm, src, 1, 0 + wordOffset, 11);
            ByteToBool(ref dst.MicroDifferentialPressureMeter3HighAlarm, src, 1, 0 + wordOffset, 12);
            ByteToBool(ref dst.MicroDifferentialPressureMeter4HighAlarm, src, 1, 0 + wordOffset, 13);
            ByteToBool(ref dst.MicroDifferentialPressureMeter1LowAlarm, src, 1, 0 + wordOffset, 14);
            ByteToBool(ref dst.MicroDifferentialPressureMeter2LowAlarm, src, 1, 0 + wordOffset, 15);
            ByteToBool(ref dst.MicroDifferentialPressureMeter3LowAlarm, src, 1, 1 + wordOffset, 0);
            ByteToBool(ref dst.MicroDifferentialPressureMeter4LowAlarm, src, 1, 1 + wordOffset, 1);
            ByteToBool(ref dst.WFClampCylinderClampControl, src, 1, 1 + wordOffset, 2);
            ByteToBool(ref dst.WFClampCylinderReleaseControl, src, 1, 1 + wordOffset, 3);
            ByteToBool(ref dst.WaferInletDoorOpenControl, src, 1, 1 + wordOffset, 4);
            ByteToBool(ref dst.WaferInletDoorCloseControl, src, 1, 1 + wordOffset, 5);
            ByteToBool(ref dst.WaferOutletDoorOpenControl, src, 1, 1 + wordOffset, 6);
            ByteToBool(ref dst.WaferOutletDoorOpenControl_Copy1, src, 1, 1 + wordOffset, 7);
            ByteToBool(ref dst.WFUpperFaceDIWWashInletValve2Control, src, 1, 1 + wordOffset, 8);
            ByteToBool(ref dst.WFUpperFaceDIWWashOutnletValve2Control, src, 1, 1 + wordOffset, 9);
            ByteToBool(ref dst.IPASupplyValveControl, src, 1, 1 + wordOffset, 10);
            ByteToBool(ref dst.WFUpperFaceDIWWashInletValve1Control, src, 1, 1 + wordOffset, 11);
            ByteToBool(ref dst.WFUpperFaceDIWWashOutnletValve1Control, src, 1, 1 + wordOffset, 12);
            ByteToBool(ref dst.WFLowerFaceDIWWashValveControl, src, 1, 1 + wordOffset, 13);
            ByteToBool(ref dst.N2MixOutletValveControl, src, 1, 1 + wordOffset, 14);
            ByteToBool(ref dst.WFUpperFaceN2OutletValveControl, src, 1, 1 + wordOffset, 15);
            ByteToBool(ref dst.WFLowerFaceN2OutletValve1Control, src, 1, 2 + wordOffset, 0);
            ByteToBool(ref dst.N2ReleaseValvesControl, src, 1, 2 + wordOffset, 1);
            ByteToBool(ref dst.WFUpperFaceIPAOutletValveControl, src, 1, 2 + wordOffset, 2);
            ByteToBool(ref dst.WFLowerFaceN2OutletValve2Control, src, 1, 2 + wordOffset, 3);
        }
        private static void ByteToBase_stDryIOData(Array dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(dst);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                stDryIOData elemTmp = new stDryIOData();
                NextIndices(dst, indices);
                ByteTo_stDryIOData(ref elemTmp, src, len, wordOffset);
                dst.SetValue(elemTmp, indices);
                wordOffset += stDryIOData_WSIZE;
            }
        }
        public static void ByteTo_stDryIOData(ref stDryIOData[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryIOData(ref stDryIOData[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryIOData(ref stDryIOData[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryIOData(ref stDryIOData[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryIOData(ref stDryIOData[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryIOData(ref stDryIOData[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryIOData(ref stDryIOData[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stDryIOData(ref stDryIOData[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stDryIOData(dst, src, len, wordOffset);
        }

        public static void ByteTo_stPenIOData(ref stPenIOData dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBool(ref dst._1PenUpperLimitSensor, src, 1, 0 + wordOffset, 0);
            ByteToBool(ref dst._1PenLowerLimitSensor, src, 1, 0 + wordOffset, 1);
            ByteToBool(ref dst._1ArmUpperLimitSensor, src, 1, 0 + wordOffset, 2);
            ByteToBool(ref dst._1ArmLowerLimitSensor, src, 1, 0 + wordOffset, 3);
            ByteToBool(ref dst._1WFClampLimitSensor, src, 1, 0 + wordOffset, 4);
            ByteToBool(ref dst._1WFReleaseLimitSensor, src, 1, 0 + wordOffset, 5);
            ByteToBool(ref dst._1NozzleUpperLimitSensor, src, 1, 0 + wordOffset, 6);
            ByteToBool(ref dst._1NozzleLowerLimitSensor, src, 1, 0 + wordOffset, 7);
            ByteToBool(ref dst._1WFInletDoorOpenLimitSensor, src, 1, 0 + wordOffset, 8);
            ByteToBool(ref dst._1WFInletDoorCloseLimitSensor, src, 1, 0 + wordOffset, 9);
            ByteToBool(ref dst._1WFOutletDoorOpenLimitSensor, src, 1, 0 + wordOffset, 10);
            ByteToBool(ref dst._1WFOutletDoorCloseLimitSensor, src, 1, 0 + wordOffset, 11);
            ByteToBool(ref dst._1WaferPresentSensor, src, 1, 0 + wordOffset, 12);
            ByteToBool(ref dst._1PenPresentSensor, src, 1, 0 + wordOffset, 13);
            ByteToBool(ref dst._1PenUpDownCylinderUpperControl, src, 1, 0 + wordOffset, 14);
            ByteToBool(ref dst._1PenUpDownCylinderLowerControl, src, 1, 0 + wordOffset, 15);
            ByteToBool(ref dst._1PenArmCylinderUpperControl, src, 1, 1 + wordOffset, 0);
            ByteToBool(ref dst._1PenArmCylinderLowerControl, src, 1, 1 + wordOffset, 1);
            ByteToBool(ref dst._1WFClampCylinderClampControl, src, 1, 1 + wordOffset, 2);
            ByteToBool(ref dst._1WFClampCylinderReleaseControl, src, 1, 1 + wordOffset, 3);
            ByteToBool(ref dst._1NozzleCylinderUpperControl, src, 1, 1 + wordOffset, 4);
            ByteToBool(ref dst._1NozzleCylinderLowerControl, src, 1, 1 + wordOffset, 5);
            ByteToBool(ref dst._1WaferInletDoorOpenControl, src, 1, 1 + wordOffset, 6);
            ByteToBool(ref dst._1WaferInletDoorCloseControl, src, 1, 1 + wordOffset, 7);
            ByteToBool(ref dst._1WaferOutletDoorOpenControl, src, 1, 1 + wordOffset, 8);
            ByteToBool(ref dst._1WaferOutletDoorCloseControl, src, 1, 1 + wordOffset, 9);
            ByteToBool(ref dst._1ChemicalInletValveControl, src, 1, 1 + wordOffset, 10);
            ByteToBool(ref dst._1ChemicalOutletValveControl, src, 1, 1 + wordOffset, 11);
            ByteToBool(ref dst._1DIWInletValveControl, src, 1, 1 + wordOffset, 12);
            ByteToBool(ref dst._1DIWOutletValveControl, src, 1, 1 + wordOffset, 13);
            ByteToBool(ref dst._1WFUpperFaceDIWWashValveControl, src, 1, 1 + wordOffset, 14);
            ByteToBool(ref dst._1WFLowerfaceDIWWashValveControl, src, 1, 1 + wordOffset, 15);
            ByteToBool(ref dst._1DIWSupplyValceControl, src, 1, 2 + wordOffset, 0);
            ByteToBool(ref dst._1PenDIWWashValveControl, src, 1, 2 + wordOffset, 1);
            ByteToBool(ref dst._1QuartzDIWWashValveControl, src, 1, 2 + wordOffset, 2);
            ByteToBool(ref dst._1ChemicalPipelineWashValveControl, src, 1, 2 + wordOffset, 3);
            ByteToBool(ref dst._1QuartzChemicalWashValveControl, src, 1, 2 + wordOffset, 4);
            ByteToBool(ref dst._1WFUpperLowerFacesChemicalWashValveControl, src, 1, 2 + wordOffset, 5);
            ByteToBool(ref dst._1WFUpperFacesChemicalWashValveControl, src, 1, 2 + wordOffset, 6);
            ByteToBool(ref dst._1DIWChamberValveControl, src, 1, 2 + wordOffset, 7);
            ByteToBool(ref dst._1WFUpperFaceCarbonatedWashValve, src, 1, 2 + wordOffset, 8);
            ByteToBool(ref dst._1WFUpperN2OutletValveControl, src, 1, 2 + wordOffset, 9);
            ByteToBool(ref dst._1CO2SupplyValveControl, src, 1, 2 + wordOffset, 10);
            ByteToBool(ref dst._1CUPPurgeValveControl, src, 1, 2 + wordOffset, 11);
            ByteToBool(ref dst._1WFLowerFaceN2OutletValveControl, src, 1, 2 + wordOffset, 12);
            ByteToBool(ref dst._1DIWFullDetection, src, 1, 2 + wordOffset, 13);
            ByteToBool(ref dst._1CavityDoorDetection, src, 1, 2 + wordOffset, 14);
            ByteToBool(ref dst._1DIWCO2PressureDetection, src, 1, 2 + wordOffset, 15);
            ByteToBool(ref dst._1WFLowerfaceN2PressureDetection, src, 1, 3 + wordOffset, 0);
            ByteToBool(ref dst._1LiquidLeakageDetection, src, 1, 3 + wordOffset, 1);
            ByteToBool(ref dst._1N2PressureDetection, src, 1, 3 + wordOffset, 2);
            ByteToBool(ref dst._1DIWPressureDetection, src, 1, 3 + wordOffset, 3);
            ByteToBool(ref dst._1BottomDripPanLeak, src, 1, 3 + wordOffset, 4);
            ByteToBool(ref dst._2PenUpperLimitSensor, src, 1, 3 + wordOffset, 5);
            ByteToBool(ref dst._2PenLowerLimitSensor, src, 1, 3 + wordOffset, 6);
            ByteToBool(ref dst._2ArmUpperLimitSensor, src, 1, 3 + wordOffset, 7);
            ByteToBool(ref dst._2ArmLowerLimitSensor, src, 1, 3 + wordOffset, 8);
            ByteToBool(ref dst._2WFClampLimitSensor, src, 1, 3 + wordOffset, 9);
            ByteToBool(ref dst._2WFReleaseLimitSensor, src, 1, 3 + wordOffset, 10);
            ByteToBool(ref dst._2NozzleUpperLimitSensor, src, 1, 3 + wordOffset, 11);
            ByteToBool(ref dst._2NozzleLowerLimitSensor, src, 1, 3 + wordOffset, 12);
            ByteToBool(ref dst._2WFInletDoorOpenLimitSensor, src, 1, 3 + wordOffset, 13);
            ByteToBool(ref dst._2WFInletDoorCloseLimitSensor, src, 1, 3 + wordOffset, 14);
            ByteToBool(ref dst._2WFOutletDoorOpenLimitSensor, src, 1, 3 + wordOffset, 15);
            ByteToBool(ref dst._2WFOutletDoorCloseLimitSensor, src, 1, 4 + wordOffset, 0);
            ByteToBool(ref dst._2WaferPresentSensor, src, 1, 4 + wordOffset, 1);
            ByteToBool(ref dst._2PenPresentSensor, src, 1, 4 + wordOffset, 2);
            ByteToBool(ref dst._2PenUpDownCylinderUpperControl, src, 1, 4 + wordOffset, 3);
            ByteToBool(ref dst._2PenUpDownCylinderLowerControl, src, 1, 4 + wordOffset, 4);
            ByteToBool(ref dst._2PenArmCylinderUpperControl, src, 1, 4 + wordOffset, 5);
            ByteToBool(ref dst._2PenArmCylinderLowerControl, src, 1, 4 + wordOffset, 6);
            ByteToBool(ref dst._2WFClampCylinderClampControl, src, 1, 4 + wordOffset, 7);
            ByteToBool(ref dst._2WFClampCylinderReleaseControl, src, 1, 4 + wordOffset, 8);
            ByteToBool(ref dst._2NozzleCylinderUpperControl, src, 1, 4 + wordOffset, 9);
            ByteToBool(ref dst._2NozzleCylinderLowerControl, src, 1, 4 + wordOffset, 10);
            ByteToBool(ref dst._2WaferInletDoorOpenControl, src, 1, 4 + wordOffset, 11);
            ByteToBool(ref dst._2WaferInletDoorCloseControl, src, 1, 4 + wordOffset, 12);
            ByteToBool(ref dst._2WaferOutletDoorOpenControl, src, 1, 4 + wordOffset, 13);
            ByteToBool(ref dst._2WaferOutletDoorCloseControl, src, 1, 4 + wordOffset, 14);
            ByteToBool(ref dst._2ChemicalInletValveControl, src, 1, 4 + wordOffset, 15);
            ByteToBool(ref dst._2ChemicalOutletValveControl, src, 1, 5 + wordOffset, 0);
            ByteToBool(ref dst._2DIWInletValveControl, src, 1, 5 + wordOffset, 1);
            ByteToBool(ref dst._2DIWOutletValveControl, src, 1, 5 + wordOffset, 2);
            ByteToBool(ref dst._2WFUpperFaceDIWWashValveControl, src, 1, 5 + wordOffset, 3);
            ByteToBool(ref dst._2WFLowerfaceDIWWashValveControl, src, 1, 5 + wordOffset, 4);
            ByteToBool(ref dst._2DIWSupplyValceControl, src, 1, 5 + wordOffset, 5);
            ByteToBool(ref dst._2PenDIWWashValveControl, src, 1, 5 + wordOffset, 6);
            ByteToBool(ref dst._2QuartzDIWWashValveControl, src, 1, 5 + wordOffset, 7);
            ByteToBool(ref dst._2ChemicalPipelineWashValveControl, src, 1, 5 + wordOffset, 8);
            ByteToBool(ref dst._2QuartzChemicalWashValveControl, src, 1, 5 + wordOffset, 9);
            ByteToBool(ref dst._2WFUpperLowerFacesChemicalWashValveControl, src, 1, 5 + wordOffset, 10);
            ByteToBool(ref dst._2WFUpperFacesChemicalWashValveControl, src, 1, 5 + wordOffset, 11);
            ByteToBool(ref dst._2DIWChamberValveControl, src, 1, 5 + wordOffset, 12);
            ByteToBool(ref dst._2WFUpperFaceCarbonatedWashValve, src, 1, 5 + wordOffset, 13);
            ByteToBool(ref dst._2WFUpperN2OutletValveControl, src, 1, 5 + wordOffset, 14);
            ByteToBool(ref dst._2CO2SupplyValveControl, src, 1, 5 + wordOffset, 15);
            ByteToBool(ref dst._2CUPPurgeValveControl, src, 1, 6 + wordOffset, 0);
            ByteToBool(ref dst._2WFLowerFaceN2OutletValveControl, src, 1, 6 + wordOffset, 1);
            ByteToBool(ref dst._2DIWFullDetection, src, 1, 6 + wordOffset, 2);
            ByteToBool(ref dst._2CavityDoorDetection, src, 1, 6 + wordOffset, 3);
            ByteToBool(ref dst._2DIWCO2PressureDetection, src, 1, 6 + wordOffset, 4);
            ByteToBool(ref dst._2WFLowerfaceN2PressureDetection, src, 1, 6 + wordOffset, 5);
            ByteToBool(ref dst._2LiquidLeakageDetection, src, 1, 6 + wordOffset, 6);
            ByteToBool(ref dst._2N2PressureDetection, src, 1, 6 + wordOffset, 7);
            ByteToBool(ref dst._2DIWPressureDetection, src, 1, 6 + wordOffset, 8);
            ByteToBool(ref dst._2BottomDripPanLeak, src, 1, 6 + wordOffset, 9);
        }
        private static void ByteToBase_stPenIOData(Array dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(dst);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                stPenIOData elemTmp = new stPenIOData();
                NextIndices(dst, indices);
                ByteTo_stPenIOData(ref elemTmp, src, len, wordOffset);
                dst.SetValue(elemTmp, indices);
                wordOffset += stPenIOData_WSIZE;
            }
        }
        public static void ByteTo_stPenIOData(ref stPenIOData[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenIOData(ref stPenIOData[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenIOData(ref stPenIOData[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenIOData(ref stPenIOData[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenIOData(ref stPenIOData[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenIOData(ref stPenIOData[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenIOData(ref stPenIOData[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPenIOData(ref stPenIOData[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPenIOData(dst, src, len, wordOffset);
        }

        public static void ByteTo_stPolishIOData(ref stPolishIOData dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBool(ref dst.PlatenBreakwaterOutsideUpLimitSensor, src, 1, 0 + wordOffset, 0);
            ByteToBool(ref dst.PlatenBreakwaterOutsideDownLimitSensor, src, 1, 0 + wordOffset, 1);
            ByteToBool(ref dst.WaterSeparatorLiquidLevelSensor, src, 1, 0 + wordOffset, 2);
            ByteToBool(ref dst.WaferOutSensor, src, 1, 0 + wordOffset, 3);
            ByteToBool(ref dst.PlatenBreakwaterInsideUpLimitSensor, src, 1, 0 + wordOffset, 4);
            ByteToBool(ref dst.PlatenBreakwaterInsideDownLimitSensor, src, 1, 0 + wordOffset, 5);
            ByteToBool(ref dst.Void, src, 14, 1 + wordOffset, 0);
            ByteToBool(ref dst.PlatenBreakwaterUpControl, src, 1, 1 + wordOffset, 14);
            ByteToBool(ref dst.PlatenBreakwaterDownControl, src, 1, 1 + wordOffset, 15);
            ByteToBool(ref dst.ConditionerHeadUpControl, src, 1, 2 + wordOffset, 0);
            ByteToBool(ref dst.ConditionerHeadDownControl, src, 1, 2 + wordOffset, 1);
            ByteToBool(ref dst.Void1, src, 16, 3 + wordOffset, 0);
            ByteToBool(ref dst.Slurry1ThreeWayTotalInletValveControl, src, 1, 4 + wordOffset, 0);
            ByteToBool(ref dst.Slurry1ThreeWayReturnValveControl, src, 1, 4 + wordOffset, 1);
            ByteToBool(ref dst.Slurry1ThreeWayIntletValveControl, src, 1, 4 + wordOffset, 2);
            ByteToBool(ref dst.WaterThreeWayIntletValve1Control, src, 1, 4 + wordOffset, 3);
            ByteToBool(ref dst.Slurry1OrDIWOutletValveControl, src, 1, 4 + wordOffset, 4);
            ByteToBool(ref dst.Slurry2ThreeWayTotalInletValve, src, 1, 4 + wordOffset, 5);
            ByteToBool(ref dst.Slurry2ThreeWayReturnValveControl, src, 1, 4 + wordOffset, 6);
            ByteToBool(ref dst.Slurry2ThreeWayIntletValveControl, src, 1, 4 + wordOffset, 7);
            ByteToBool(ref dst.WaterThreeWayIntletValve2Control, src, 1, 4 + wordOffset, 8);
            ByteToBool(ref dst.Slurry2OrDIWOutletValveControl, src, 1, 4 + wordOffset, 9);
            ByteToBool(ref dst.Slurry3ThreeWayTotalInletValveControl, src, 1, 4 + wordOffset, 10);
            ByteToBool(ref dst.Slurry3ThreeWayReturnValveControl, src, 1, 4 + wordOffset, 11);
            ByteToBool(ref dst.Slurry3ThreeWayIntletValveControl, src, 1, 4 + wordOffset, 12);
            ByteToBool(ref dst.WaterThreeWayIntletValve3Control, src, 1, 4 + wordOffset, 13);
            ByteToBool(ref dst.Slurry3OrDIWOutletValveControl, src, 1, 4 + wordOffset, 14);
            ByteToBool(ref dst.Void2, src, 3, 5 + wordOffset, 0);
            ByteToBool(ref dst.DIWOutletValveControl, src, 1, 5 + wordOffset, 3);
            ByteToBool(ref dst.PlatenWashValveControl, src, 1, 5 + wordOffset, 4);
            ByteToBool(ref dst.MovingHeadDIWWashValveControl, src, 1, 5 + wordOffset, 5);
            ByteToBool(ref dst.MovingHeadExternalDIWWashValveControl, src, 1, 5 + wordOffset, 6);
            ByteToBool(ref dst.HeadChemicalWashValveControl, src, 1, 5 + wordOffset, 7);
            ByteToBool(ref dst.ConditionerDIWWashValveControl, src, 1, 5 + wordOffset, 8);
            ByteToBool(ref dst.ConditionerArmDIWWashValveControl, src, 1, 5 + wordOffset, 9);
            ByteToBool(ref dst.ChemicalArmDIWWashValveControl, src, 1, 5 + wordOffset, 10);
            ByteToBool(ref dst.HeadMiddleWashValveControl, src, 1, 5 + wordOffset, 11);
            ByteToBool(ref dst.HeadExternalWashValveControl, src, 1, 5 + wordOffset, 12);
            ByteToBool(ref dst.WFUpperFaceChemicalWashValveControl, src, 1, 5 + wordOffset, 13);
            ByteToBool(ref dst.WFUpperFaceDIWWashValveControl, src, 1, 5 + wordOffset, 14);
            ByteToBool(ref dst.WFLowerFaceChemicalWashValveControl, src, 1, 5 + wordOffset, 15);
            ByteToBool(ref dst.WFLowerFaceDIWWashValveControl, src, 1, 6 + wordOffset, 0);
            ByteToBool(ref dst.PlatenCoolWaterValveControl, src, 1, 6 + wordOffset, 1);
            ByteToBool(ref dst.PlatenMotorCoolWaterValveControl, src, 1, 6 + wordOffset, 2);
            ByteToBool(ref dst.Void3, src, 1, 6 + wordOffset, 3);
            ByteToBool(ref dst.Void4, src, 1, 6 + wordOffset, 4);
            ByteToBool(ref dst.Void5, src, 1, 6 + wordOffset, 5);
            ByteToBool(ref dst.Void6, src, 1, 6 + wordOffset, 6);
            ByteToBool(ref dst.Void7, src, 1, 6 + wordOffset, 7);
            ByteToBool(ref dst.Void8, src, 1, 6 + wordOffset, 8);
            ByteToBool(ref dst.Void9, src, 1, 6 + wordOffset, 9);
            ByteToBool(ref dst.Void10, src, 1, 6 + wordOffset, 10);
            ByteToBool(ref dst.Void11, src, 1, 6 + wordOffset, 11);
            ByteToBool(ref dst.Void12, src, 1, 6 + wordOffset, 12);
            ByteToBool(ref dst.Void13, src, 1, 6 + wordOffset, 13);
            ByteToBool(ref dst.Void14, src, 1, 6 + wordOffset, 14);
            ByteToBool(ref dst.Void15, src, 1, 6 + wordOffset, 15);
            ByteToBool(ref dst.Zone1PressValve1Control, src, 1, 7 + wordOffset, 0);
            ByteToBool(ref dst.Zone1PressValve2Control, src, 1, 7 + wordOffset, 1);
            ByteToBool(ref dst.Zone1VacuumValveControl, src, 1, 7 + wordOffset, 2);
            ByteToBool(ref dst.Zone2PressValve1Control, src, 1, 7 + wordOffset, 3);
            ByteToBool(ref dst.Zone2PressValve2Control, src, 1, 7 + wordOffset, 4);
            ByteToBool(ref dst.Zone2VacuumValveControl, src, 1, 7 + wordOffset, 5);
            ByteToBool(ref dst.Zone3PressValve1Control, src, 1, 7 + wordOffset, 6);
            ByteToBool(ref dst.Zone3PressValve2Control, src, 1, 7 + wordOffset, 7);
            ByteToBool(ref dst.Zone3VacuumValveControl, src, 1, 7 + wordOffset, 8);
            ByteToBool(ref dst.Zone4PressValve1Control, src, 1, 7 + wordOffset, 9);
            ByteToBool(ref dst.Zone4PressValve2Control, src, 1, 7 + wordOffset, 10);
            ByteToBool(ref dst.Zone4VacuumValveControl, src, 1, 7 + wordOffset, 11);
            ByteToBool(ref dst.Zone5PressValve1Control, src, 1, 7 + wordOffset, 12);
            ByteToBool(ref dst.Zone5PressValve2Control, src, 1, 7 + wordOffset, 13);
            ByteToBool(ref dst.Zone5VacuumValveControl, src, 1, 7 + wordOffset, 14);
            ByteToBool(ref dst.Zone6PressValve1Control, src, 1, 7 + wordOffset, 15);
            ByteToBool(ref dst.Zone6PressValve2Control, src, 1, 8 + wordOffset, 0);
            ByteToBool(ref dst.Zone6VacuumValveControl, src, 1, 8 + wordOffset, 1);
            ByteToBool(ref dst.Zone7PressValve1Control, src, 1, 8 + wordOffset, 2);
            ByteToBool(ref dst.Zone7PressValve2Control, src, 1, 8 + wordOffset, 3);
            ByteToBool(ref dst.Zone7VacuumValveControl, src, 1, 8 + wordOffset, 4);
            ByteToBool(ref dst.Zone8PressValve1Control, src, 1, 8 + wordOffset, 5);
            ByteToBool(ref dst.Zone8PressValve2Control, src, 1, 8 + wordOffset, 6);
            ByteToBool(ref dst.Zone8VacuumValveControl, src, 1, 8 + wordOffset, 7);
            ByteToBool(ref dst.TopRingPressValve1Control, src, 1, 8 + wordOffset, 8);
            ByteToBool(ref dst.TopRingPressValve2Control, src, 1, 8 + wordOffset, 9);
            ByteToBool(ref dst.TopRingVacuumValveControl, src, 1, 8 + wordOffset, 10);
            ByteToBool(ref dst.Void16, src, 1, 8 + wordOffset, 11);
            ByteToBool(ref dst.Void17, src, 1, 8 + wordOffset, 12);
            ByteToBool(ref dst.Void18, src, 1, 8 + wordOffset, 13);
            ByteToBool(ref dst.Zone4BlowValve1Control, src, 1, 8 + wordOffset, 14);
            ByteToBool(ref dst.Zone4BlowValve2Control, src, 1, 8 + wordOffset, 15);
            ByteToBool(ref dst.Zone4AtmosphericValveControl, src, 1, 9 + wordOffset, 0);
            ByteToBool(ref dst.AirBagVacuumGeneratorIntletValveControl, src, 1, 9 + wordOffset, 1);
            ByteToBool(ref dst.AirBagVacuumGeneratorOutletValveControl, src, 1, 9 + wordOffset, 2);
            ByteToBool(ref dst.VacuumGeneratorIntletValveControl, src, 1, 9 + wordOffset, 3);
            ByteToBool(ref dst.VacuumGeneratorOutletValveControl, src, 1, 9 + wordOffset, 4);
            ByteToBool(ref dst.CatchWaterBlowValveControl, src, 1, 9 + wordOffset, 5);
            ByteToBool(ref dst.CatchWaterDrainValveControl, src, 1, 9 + wordOffset, 6);
            ByteToBool(ref dst.HeadExternalBlowN2ValveControl, src, 1, 9 + wordOffset, 7);
            ByteToBool(ref dst.Void19, src, 1, 9 + wordOffset, 8);
            ByteToBool(ref dst.Void20, src, 1, 9 + wordOffset, 9);
            ByteToBool(ref dst.Void21, src, 1, 9 + wordOffset, 10);
            ByteToBool(ref dst.Void22, src, 1, 9 + wordOffset, 11);
            ByteToBool(ref dst.Void23, src, 1, 9 + wordOffset, 12);
            ByteToBool(ref dst.Void24, src, 1, 9 + wordOffset, 13);
            ByteToBool(ref dst.Void25, src, 1, 9 + wordOffset, 14);
            ByteToBool(ref dst.Void26, src, 1, 9 + wordOffset, 15);
            ByteToBool(ref dst.Zone1VentValveControl, src, 1, 10 + wordOffset, 0);
            ByteToBool(ref dst.Zone2VentValveControl, src, 1, 10 + wordOffset, 1);
            ByteToBool(ref dst.Zone3VentValveControl, src, 1, 10 + wordOffset, 2);
            ByteToBool(ref dst.Zone5VentValveControl, src, 1, 10 + wordOffset, 3);
            ByteToBool(ref dst.Zone6VentValveControl, src, 1, 10 + wordOffset, 4);
            ByteToBool(ref dst.Zone7VentValveControl, src, 1, 10 + wordOffset, 5);
            ByteToBool(ref dst.Zone8VentValveControl, src, 1, 10 + wordOffset, 6);
            ByteToBool(ref dst.TopRingVentValveControl, src, 1, 10 + wordOffset, 7);
        }
        private static void ByteToBase_stPolishIOData(Array dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(dst);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                stPolishIOData elemTmp = new stPolishIOData();
                NextIndices(dst, indices);
                ByteTo_stPolishIOData(ref elemTmp, src, len, wordOffset);
                dst.SetValue(elemTmp, indices);
                wordOffset += stPolishIOData_WSIZE;
            }
        }
        public static void ByteTo_stPolishIOData(ref stPolishIOData[] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishIOData(ref stPolishIOData[,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishIOData(ref stPolishIOData[,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishIOData(ref stPolishIOData[,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishIOData(ref stPolishIOData[,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishIOData(ref stPolishIOData[,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishIOData(ref stPolishIOData[,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishIOData(dst, src, len, wordOffset);
        }
        public static void ByteTo_stPolishIOData(ref stPolishIOData[,,,,,,,] dst, byte[] src, int len = 1, int wordOffset = 0)
        {
            ByteToBase_stPolishIOData(dst, src, len, wordOffset);
        }

        public static void stDryIOData_ToByte(ref byte[] dst, stDryIOData src, int len = 1, int wordOffset = 0)
        {
            BoolToByte(ref dst, src.WFClampLimitSensor, 1, 0 + wordOffset, 0);
            BoolToByte(ref dst, src.WFReleaseLimitSensor, 1, 0 + wordOffset, 1);
            BoolToByte(ref dst, src.WFInletDoorOpenLimitSensor, 1, 0 + wordOffset, 2);
            BoolToByte(ref dst, src.WFInletDoorCloseLimitSensor, 1, 0 + wordOffset, 3);
            BoolToByte(ref dst, src.WFOutletDoorOpenLimitSensor, 1, 0 + wordOffset, 4);
            BoolToByte(ref dst, src.WFOutletDoorCloseLimitSensor, 1, 0 + wordOffset, 5);
            BoolToByte(ref dst, src.WafersearchSensor, 1, 0 + wordOffset, 6);
            BoolToByte(ref dst, src.LeakDetectionAlarm, 1, 0 + wordOffset, 7);
            BoolToByte(ref dst, src.IPAConcentrationAlarm, 1, 0 + wordOffset, 8);
            BoolToByte(ref dst, src.FlameDetectionAlarm, 1, 0 + wordOffset, 9);
            BoolToByte(ref dst, src.MicroDifferentialPressureMeter1HighAlarm, 1, 0 + wordOffset, 10);
            BoolToByte(ref dst, src.MicroDifferentialPressureMeter2HighAlarm, 1, 0 + wordOffset, 11);
            BoolToByte(ref dst, src.MicroDifferentialPressureMeter3HighAlarm, 1, 0 + wordOffset, 12);
            BoolToByte(ref dst, src.MicroDifferentialPressureMeter4HighAlarm, 1, 0 + wordOffset, 13);
            BoolToByte(ref dst, src.MicroDifferentialPressureMeter1LowAlarm, 1, 0 + wordOffset, 14);
            BoolToByte(ref dst, src.MicroDifferentialPressureMeter2LowAlarm, 1, 0 + wordOffset, 15);
            BoolToByte(ref dst, src.MicroDifferentialPressureMeter3LowAlarm, 1, 1 + wordOffset, 0);
            BoolToByte(ref dst, src.MicroDifferentialPressureMeter4LowAlarm, 1, 1 + wordOffset, 1);
            BoolToByte(ref dst, src.WFClampCylinderClampControl, 1, 1 + wordOffset, 2);
            BoolToByte(ref dst, src.WFClampCylinderReleaseControl, 1, 1 + wordOffset, 3);
            BoolToByte(ref dst, src.WaferInletDoorOpenControl, 1, 1 + wordOffset, 4);
            BoolToByte(ref dst, src.WaferInletDoorCloseControl, 1, 1 + wordOffset, 5);
            BoolToByte(ref dst, src.WaferOutletDoorOpenControl, 1, 1 + wordOffset, 6);
            BoolToByte(ref dst, src.WaferOutletDoorOpenControl_Copy1, 1, 1 + wordOffset, 7);
            BoolToByte(ref dst, src.WFUpperFaceDIWWashInletValve2Control, 1, 1 + wordOffset, 8);
            BoolToByte(ref dst, src.WFUpperFaceDIWWashOutnletValve2Control, 1, 1 + wordOffset, 9);
            BoolToByte(ref dst, src.IPASupplyValveControl, 1, 1 + wordOffset, 10);
            BoolToByte(ref dst, src.WFUpperFaceDIWWashInletValve1Control, 1, 1 + wordOffset, 11);
            BoolToByte(ref dst, src.WFUpperFaceDIWWashOutnletValve1Control, 1, 1 + wordOffset, 12);
            BoolToByte(ref dst, src.WFLowerFaceDIWWashValveControl, 1, 1 + wordOffset, 13);
            BoolToByte(ref dst, src.N2MixOutletValveControl, 1, 1 + wordOffset, 14);
            BoolToByte(ref dst, src.WFUpperFaceN2OutletValveControl, 1, 1 + wordOffset, 15);
            BoolToByte(ref dst, src.WFLowerFaceN2OutletValve1Control, 1, 2 + wordOffset, 0);
            BoolToByte(ref dst, src.N2ReleaseValvesControl, 1, 2 + wordOffset, 1);
            BoolToByte(ref dst, src.WFUpperFaceIPAOutletValveControl, 1, 2 + wordOffset, 2);
            BoolToByte(ref dst, src.WFLowerFaceN2OutletValve2Control, 1, 2 + wordOffset, 3);
        }
        private static void stDryIOData_ToByteBase(ref byte[] dst, Array src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(src);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                NextIndices(src, indices);
                stDryIOData_ToByte(ref dst, (stDryIOData)src.GetValue(indices), len, wordOffset);
                wordOffset += stDryIOData_WSIZE;
            }
        }
        public static void stDryIOData_ToByte(ref byte[] dst, stDryIOData[] src, int len = 1, int wordOffset = 0)
        {
            stDryIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryIOData_ToByte(ref byte[] dst, stDryIOData[,] src, int len = 1, int wordOffset = 0)
        {
            stDryIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryIOData_ToByte(ref byte[] dst, stDryIOData[,,] src, int len = 1, int wordOffset = 0)
        {
            stDryIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryIOData_ToByte(ref byte[] dst, stDryIOData[,,,] src, int len = 1, int wordOffset = 0)
        {
            stDryIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryIOData_ToByte(ref byte[] dst, stDryIOData[,,,,] src, int len = 1, int wordOffset = 0)
        {
            stDryIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryIOData_ToByte(ref byte[] dst, stDryIOData[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stDryIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryIOData_ToByte(ref byte[] dst, stDryIOData[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stDryIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stDryIOData_ToByte(ref byte[] dst, stDryIOData[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stDryIOData_ToByteBase(ref dst, src, len, wordOffset);
        }

        public static void stPenIOData_ToByte(ref byte[] dst, stPenIOData src, int len = 1, int wordOffset = 0)
        {
            BoolToByte(ref dst, src._1PenUpperLimitSensor, 1, 0 + wordOffset, 0);
            BoolToByte(ref dst, src._1PenLowerLimitSensor, 1, 0 + wordOffset, 1);
            BoolToByte(ref dst, src._1ArmUpperLimitSensor, 1, 0 + wordOffset, 2);
            BoolToByte(ref dst, src._1ArmLowerLimitSensor, 1, 0 + wordOffset, 3);
            BoolToByte(ref dst, src._1WFClampLimitSensor, 1, 0 + wordOffset, 4);
            BoolToByte(ref dst, src._1WFReleaseLimitSensor, 1, 0 + wordOffset, 5);
            BoolToByte(ref dst, src._1NozzleUpperLimitSensor, 1, 0 + wordOffset, 6);
            BoolToByte(ref dst, src._1NozzleLowerLimitSensor, 1, 0 + wordOffset, 7);
            BoolToByte(ref dst, src._1WFInletDoorOpenLimitSensor, 1, 0 + wordOffset, 8);
            BoolToByte(ref dst, src._1WFInletDoorCloseLimitSensor, 1, 0 + wordOffset, 9);
            BoolToByte(ref dst, src._1WFOutletDoorOpenLimitSensor, 1, 0 + wordOffset, 10);
            BoolToByte(ref dst, src._1WFOutletDoorCloseLimitSensor, 1, 0 + wordOffset, 11);
            BoolToByte(ref dst, src._1WaferPresentSensor, 1, 0 + wordOffset, 12);
            BoolToByte(ref dst, src._1PenPresentSensor, 1, 0 + wordOffset, 13);
            BoolToByte(ref dst, src._1PenUpDownCylinderUpperControl, 1, 0 + wordOffset, 14);
            BoolToByte(ref dst, src._1PenUpDownCylinderLowerControl, 1, 0 + wordOffset, 15);
            BoolToByte(ref dst, src._1PenArmCylinderUpperControl, 1, 1 + wordOffset, 0);
            BoolToByte(ref dst, src._1PenArmCylinderLowerControl, 1, 1 + wordOffset, 1);
            BoolToByte(ref dst, src._1WFClampCylinderClampControl, 1, 1 + wordOffset, 2);
            BoolToByte(ref dst, src._1WFClampCylinderReleaseControl, 1, 1 + wordOffset, 3);
            BoolToByte(ref dst, src._1NozzleCylinderUpperControl, 1, 1 + wordOffset, 4);
            BoolToByte(ref dst, src._1NozzleCylinderLowerControl, 1, 1 + wordOffset, 5);
            BoolToByte(ref dst, src._1WaferInletDoorOpenControl, 1, 1 + wordOffset, 6);
            BoolToByte(ref dst, src._1WaferInletDoorCloseControl, 1, 1 + wordOffset, 7);
            BoolToByte(ref dst, src._1WaferOutletDoorOpenControl, 1, 1 + wordOffset, 8);
            BoolToByte(ref dst, src._1WaferOutletDoorCloseControl, 1, 1 + wordOffset, 9);
            BoolToByte(ref dst, src._1ChemicalInletValveControl, 1, 1 + wordOffset, 10);
            BoolToByte(ref dst, src._1ChemicalOutletValveControl, 1, 1 + wordOffset, 11);
            BoolToByte(ref dst, src._1DIWInletValveControl, 1, 1 + wordOffset, 12);
            BoolToByte(ref dst, src._1DIWOutletValveControl, 1, 1 + wordOffset, 13);
            BoolToByte(ref dst, src._1WFUpperFaceDIWWashValveControl, 1, 1 + wordOffset, 14);
            BoolToByte(ref dst, src._1WFLowerfaceDIWWashValveControl, 1, 1 + wordOffset, 15);
            BoolToByte(ref dst, src._1DIWSupplyValceControl, 1, 2 + wordOffset, 0);
            BoolToByte(ref dst, src._1PenDIWWashValveControl, 1, 2 + wordOffset, 1);
            BoolToByte(ref dst, src._1QuartzDIWWashValveControl, 1, 2 + wordOffset, 2);
            BoolToByte(ref dst, src._1ChemicalPipelineWashValveControl, 1, 2 + wordOffset, 3);
            BoolToByte(ref dst, src._1QuartzChemicalWashValveControl, 1, 2 + wordOffset, 4);
            BoolToByte(ref dst, src._1WFUpperLowerFacesChemicalWashValveControl, 1, 2 + wordOffset, 5);
            BoolToByte(ref dst, src._1WFUpperFacesChemicalWashValveControl, 1, 2 + wordOffset, 6);
            BoolToByte(ref dst, src._1DIWChamberValveControl, 1, 2 + wordOffset, 7);
            BoolToByte(ref dst, src._1WFUpperFaceCarbonatedWashValve, 1, 2 + wordOffset, 8);
            BoolToByte(ref dst, src._1WFUpperN2OutletValveControl, 1, 2 + wordOffset, 9);
            BoolToByte(ref dst, src._1CO2SupplyValveControl, 1, 2 + wordOffset, 10);
            BoolToByte(ref dst, src._1CUPPurgeValveControl, 1, 2 + wordOffset, 11);
            BoolToByte(ref dst, src._1WFLowerFaceN2OutletValveControl, 1, 2 + wordOffset, 12);
            BoolToByte(ref dst, src._1DIWFullDetection, 1, 2 + wordOffset, 13);
            BoolToByte(ref dst, src._1CavityDoorDetection, 1, 2 + wordOffset, 14);
            BoolToByte(ref dst, src._1DIWCO2PressureDetection, 1, 2 + wordOffset, 15);
            BoolToByte(ref dst, src._1WFLowerfaceN2PressureDetection, 1, 3 + wordOffset, 0);
            BoolToByte(ref dst, src._1LiquidLeakageDetection, 1, 3 + wordOffset, 1);
            BoolToByte(ref dst, src._1N2PressureDetection, 1, 3 + wordOffset, 2);
            BoolToByte(ref dst, src._1DIWPressureDetection, 1, 3 + wordOffset, 3);
            BoolToByte(ref dst, src._1BottomDripPanLeak, 1, 3 + wordOffset, 4);
            BoolToByte(ref dst, src._2PenUpperLimitSensor, 1, 3 + wordOffset, 5);
            BoolToByte(ref dst, src._2PenLowerLimitSensor, 1, 3 + wordOffset, 6);
            BoolToByte(ref dst, src._2ArmUpperLimitSensor, 1, 3 + wordOffset, 7);
            BoolToByte(ref dst, src._2ArmLowerLimitSensor, 1, 3 + wordOffset, 8);
            BoolToByte(ref dst, src._2WFClampLimitSensor, 1, 3 + wordOffset, 9);
            BoolToByte(ref dst, src._2WFReleaseLimitSensor, 1, 3 + wordOffset, 10);
            BoolToByte(ref dst, src._2NozzleUpperLimitSensor, 1, 3 + wordOffset, 11);
            BoolToByte(ref dst, src._2NozzleLowerLimitSensor, 1, 3 + wordOffset, 12);
            BoolToByte(ref dst, src._2WFInletDoorOpenLimitSensor, 1, 3 + wordOffset, 13);
            BoolToByte(ref dst, src._2WFInletDoorCloseLimitSensor, 1, 3 + wordOffset, 14);
            BoolToByte(ref dst, src._2WFOutletDoorOpenLimitSensor, 1, 3 + wordOffset, 15);
            BoolToByte(ref dst, src._2WFOutletDoorCloseLimitSensor, 1, 4 + wordOffset, 0);
            BoolToByte(ref dst, src._2WaferPresentSensor, 1, 4 + wordOffset, 1);
            BoolToByte(ref dst, src._2PenPresentSensor, 1, 4 + wordOffset, 2);
            BoolToByte(ref dst, src._2PenUpDownCylinderUpperControl, 1, 4 + wordOffset, 3);
            BoolToByte(ref dst, src._2PenUpDownCylinderLowerControl, 1, 4 + wordOffset, 4);
            BoolToByte(ref dst, src._2PenArmCylinderUpperControl, 1, 4 + wordOffset, 5);
            BoolToByte(ref dst, src._2PenArmCylinderLowerControl, 1, 4 + wordOffset, 6);
            BoolToByte(ref dst, src._2WFClampCylinderClampControl, 1, 4 + wordOffset, 7);
            BoolToByte(ref dst, src._2WFClampCylinderReleaseControl, 1, 4 + wordOffset, 8);
            BoolToByte(ref dst, src._2NozzleCylinderUpperControl, 1, 4 + wordOffset, 9);
            BoolToByte(ref dst, src._2NozzleCylinderLowerControl, 1, 4 + wordOffset, 10);
            BoolToByte(ref dst, src._2WaferInletDoorOpenControl, 1, 4 + wordOffset, 11);
            BoolToByte(ref dst, src._2WaferInletDoorCloseControl, 1, 4 + wordOffset, 12);
            BoolToByte(ref dst, src._2WaferOutletDoorOpenControl, 1, 4 + wordOffset, 13);
            BoolToByte(ref dst, src._2WaferOutletDoorCloseControl, 1, 4 + wordOffset, 14);
            BoolToByte(ref dst, src._2ChemicalInletValveControl, 1, 4 + wordOffset, 15);
            BoolToByte(ref dst, src._2ChemicalOutletValveControl, 1, 5 + wordOffset, 0);
            BoolToByte(ref dst, src._2DIWInletValveControl, 1, 5 + wordOffset, 1);
            BoolToByte(ref dst, src._2DIWOutletValveControl, 1, 5 + wordOffset, 2);
            BoolToByte(ref dst, src._2WFUpperFaceDIWWashValveControl, 1, 5 + wordOffset, 3);
            BoolToByte(ref dst, src._2WFLowerfaceDIWWashValveControl, 1, 5 + wordOffset, 4);
            BoolToByte(ref dst, src._2DIWSupplyValceControl, 1, 5 + wordOffset, 5);
            BoolToByte(ref dst, src._2PenDIWWashValveControl, 1, 5 + wordOffset, 6);
            BoolToByte(ref dst, src._2QuartzDIWWashValveControl, 1, 5 + wordOffset, 7);
            BoolToByte(ref dst, src._2ChemicalPipelineWashValveControl, 1, 5 + wordOffset, 8);
            BoolToByte(ref dst, src._2QuartzChemicalWashValveControl, 1, 5 + wordOffset, 9);
            BoolToByte(ref dst, src._2WFUpperLowerFacesChemicalWashValveControl, 1, 5 + wordOffset, 10);
            BoolToByte(ref dst, src._2WFUpperFacesChemicalWashValveControl, 1, 5 + wordOffset, 11);
            BoolToByte(ref dst, src._2DIWChamberValveControl, 1, 5 + wordOffset, 12);
            BoolToByte(ref dst, src._2WFUpperFaceCarbonatedWashValve, 1, 5 + wordOffset, 13);
            BoolToByte(ref dst, src._2WFUpperN2OutletValveControl, 1, 5 + wordOffset, 14);
            BoolToByte(ref dst, src._2CO2SupplyValveControl, 1, 5 + wordOffset, 15);
            BoolToByte(ref dst, src._2CUPPurgeValveControl, 1, 6 + wordOffset, 0);
            BoolToByte(ref dst, src._2WFLowerFaceN2OutletValveControl, 1, 6 + wordOffset, 1);
            BoolToByte(ref dst, src._2DIWFullDetection, 1, 6 + wordOffset, 2);
            BoolToByte(ref dst, src._2CavityDoorDetection, 1, 6 + wordOffset, 3);
            BoolToByte(ref dst, src._2DIWCO2PressureDetection, 1, 6 + wordOffset, 4);
            BoolToByte(ref dst, src._2WFLowerfaceN2PressureDetection, 1, 6 + wordOffset, 5);
            BoolToByte(ref dst, src._2LiquidLeakageDetection, 1, 6 + wordOffset, 6);
            BoolToByte(ref dst, src._2N2PressureDetection, 1, 6 + wordOffset, 7);
            BoolToByte(ref dst, src._2DIWPressureDetection, 1, 6 + wordOffset, 8);
            BoolToByte(ref dst, src._2BottomDripPanLeak, 1, 6 + wordOffset, 9);
        }
        private static void stPenIOData_ToByteBase(ref byte[] dst, Array src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(src);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                NextIndices(src, indices);
                stPenIOData_ToByte(ref dst, (stPenIOData)src.GetValue(indices), len, wordOffset);
                wordOffset += stPenIOData_WSIZE;
            }
        }
        public static void stPenIOData_ToByte(ref byte[] dst, stPenIOData[] src, int len = 1, int wordOffset = 0)
        {
            stPenIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenIOData_ToByte(ref byte[] dst, stPenIOData[,] src, int len = 1, int wordOffset = 0)
        {
            stPenIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenIOData_ToByte(ref byte[] dst, stPenIOData[,,] src, int len = 1, int wordOffset = 0)
        {
            stPenIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenIOData_ToByte(ref byte[] dst, stPenIOData[,,,] src, int len = 1, int wordOffset = 0)
        {
            stPenIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenIOData_ToByte(ref byte[] dst, stPenIOData[,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPenIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenIOData_ToByte(ref byte[] dst, stPenIOData[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPenIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenIOData_ToByte(ref byte[] dst, stPenIOData[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPenIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPenIOData_ToByte(ref byte[] dst, stPenIOData[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPenIOData_ToByteBase(ref dst, src, len, wordOffset);
        }

        public static void stPolishIOData_ToByte(ref byte[] dst, stPolishIOData src, int len = 1, int wordOffset = 0)
        {
            BoolToByte(ref dst, src.PlatenBreakwaterOutsideUpLimitSensor, 1, 0 + wordOffset, 0);
            BoolToByte(ref dst, src.PlatenBreakwaterOutsideDownLimitSensor, 1, 0 + wordOffset, 1);
            BoolToByte(ref dst, src.WaterSeparatorLiquidLevelSensor, 1, 0 + wordOffset, 2);
            BoolToByte(ref dst, src.WaferOutSensor, 1, 0 + wordOffset, 3);
            BoolToByte(ref dst, src.PlatenBreakwaterInsideUpLimitSensor, 1, 0 + wordOffset, 4);
            BoolToByte(ref dst, src.PlatenBreakwaterInsideDownLimitSensor, 1, 0 + wordOffset, 5);
            BoolToByte(ref dst, src.Void, 14, 1 + wordOffset, 0);
            BoolToByte(ref dst, src.PlatenBreakwaterUpControl, 1, 1 + wordOffset, 14);
            BoolToByte(ref dst, src.PlatenBreakwaterDownControl, 1, 1 + wordOffset, 15);
            BoolToByte(ref dst, src.ConditionerHeadUpControl, 1, 2 + wordOffset, 0);
            BoolToByte(ref dst, src.ConditionerHeadDownControl, 1, 2 + wordOffset, 1);
            BoolToByte(ref dst, src.Void1, 16, 3 + wordOffset, 0);
            BoolToByte(ref dst, src.Slurry1ThreeWayTotalInletValveControl, 1, 4 + wordOffset, 0);
            BoolToByte(ref dst, src.Slurry1ThreeWayReturnValveControl, 1, 4 + wordOffset, 1);
            BoolToByte(ref dst, src.Slurry1ThreeWayIntletValveControl, 1, 4 + wordOffset, 2);
            BoolToByte(ref dst, src.WaterThreeWayIntletValve1Control, 1, 4 + wordOffset, 3);
            BoolToByte(ref dst, src.Slurry1OrDIWOutletValveControl, 1, 4 + wordOffset, 4);
            BoolToByte(ref dst, src.Slurry2ThreeWayTotalInletValve, 1, 4 + wordOffset, 5);
            BoolToByte(ref dst, src.Slurry2ThreeWayReturnValveControl, 1, 4 + wordOffset, 6);
            BoolToByte(ref dst, src.Slurry2ThreeWayIntletValveControl, 1, 4 + wordOffset, 7);
            BoolToByte(ref dst, src.WaterThreeWayIntletValve2Control, 1, 4 + wordOffset, 8);
            BoolToByte(ref dst, src.Slurry2OrDIWOutletValveControl, 1, 4 + wordOffset, 9);
            BoolToByte(ref dst, src.Slurry3ThreeWayTotalInletValveControl, 1, 4 + wordOffset, 10);
            BoolToByte(ref dst, src.Slurry3ThreeWayReturnValveControl, 1, 4 + wordOffset, 11);
            BoolToByte(ref dst, src.Slurry3ThreeWayIntletValveControl, 1, 4 + wordOffset, 12);
            BoolToByte(ref dst, src.WaterThreeWayIntletValve3Control, 1, 4 + wordOffset, 13);
            BoolToByte(ref dst, src.Slurry3OrDIWOutletValveControl, 1, 4 + wordOffset, 14);
            BoolToByte(ref dst, src.Void2, 3, 5 + wordOffset, 0);
            BoolToByte(ref dst, src.DIWOutletValveControl, 1, 5 + wordOffset, 3);
            BoolToByte(ref dst, src.PlatenWashValveControl, 1, 5 + wordOffset, 4);
            BoolToByte(ref dst, src.MovingHeadDIWWashValveControl, 1, 5 + wordOffset, 5);
            BoolToByte(ref dst, src.MovingHeadExternalDIWWashValveControl, 1, 5 + wordOffset, 6);
            BoolToByte(ref dst, src.HeadChemicalWashValveControl, 1, 5 + wordOffset, 7);
            BoolToByte(ref dst, src.ConditionerDIWWashValveControl, 1, 5 + wordOffset, 8);
            BoolToByte(ref dst, src.ConditionerArmDIWWashValveControl, 1, 5 + wordOffset, 9);
            BoolToByte(ref dst, src.ChemicalArmDIWWashValveControl, 1, 5 + wordOffset, 10);
            BoolToByte(ref dst, src.HeadMiddleWashValveControl, 1, 5 + wordOffset, 11);
            BoolToByte(ref dst, src.HeadExternalWashValveControl, 1, 5 + wordOffset, 12);
            BoolToByte(ref dst, src.WFUpperFaceChemicalWashValveControl, 1, 5 + wordOffset, 13);
            BoolToByte(ref dst, src.WFUpperFaceDIWWashValveControl, 1, 5 + wordOffset, 14);
            BoolToByte(ref dst, src.WFLowerFaceChemicalWashValveControl, 1, 5 + wordOffset, 15);
            BoolToByte(ref dst, src.WFLowerFaceDIWWashValveControl, 1, 6 + wordOffset, 0);
            BoolToByte(ref dst, src.PlatenCoolWaterValveControl, 1, 6 + wordOffset, 1);
            BoolToByte(ref dst, src.PlatenMotorCoolWaterValveControl, 1, 6 + wordOffset, 2);
            BoolToByte(ref dst, src.Void3, 1, 6 + wordOffset, 3);
            BoolToByte(ref dst, src.Void4, 1, 6 + wordOffset, 4);
            BoolToByte(ref dst, src.Void5, 1, 6 + wordOffset, 5);
            BoolToByte(ref dst, src.Void6, 1, 6 + wordOffset, 6);
            BoolToByte(ref dst, src.Void7, 1, 6 + wordOffset, 7);
            BoolToByte(ref dst, src.Void8, 1, 6 + wordOffset, 8);
            BoolToByte(ref dst, src.Void9, 1, 6 + wordOffset, 9);
            BoolToByte(ref dst, src.Void10, 1, 6 + wordOffset, 10);
            BoolToByte(ref dst, src.Void11, 1, 6 + wordOffset, 11);
            BoolToByte(ref dst, src.Void12, 1, 6 + wordOffset, 12);
            BoolToByte(ref dst, src.Void13, 1, 6 + wordOffset, 13);
            BoolToByte(ref dst, src.Void14, 1, 6 + wordOffset, 14);
            BoolToByte(ref dst, src.Void15, 1, 6 + wordOffset, 15);
            BoolToByte(ref dst, src.Zone1PressValve1Control, 1, 7 + wordOffset, 0);
            BoolToByte(ref dst, src.Zone1PressValve2Control, 1, 7 + wordOffset, 1);
            BoolToByte(ref dst, src.Zone1VacuumValveControl, 1, 7 + wordOffset, 2);
            BoolToByte(ref dst, src.Zone2PressValve1Control, 1, 7 + wordOffset, 3);
            BoolToByte(ref dst, src.Zone2PressValve2Control, 1, 7 + wordOffset, 4);
            BoolToByte(ref dst, src.Zone2VacuumValveControl, 1, 7 + wordOffset, 5);
            BoolToByte(ref dst, src.Zone3PressValve1Control, 1, 7 + wordOffset, 6);
            BoolToByte(ref dst, src.Zone3PressValve2Control, 1, 7 + wordOffset, 7);
            BoolToByte(ref dst, src.Zone3VacuumValveControl, 1, 7 + wordOffset, 8);
            BoolToByte(ref dst, src.Zone4PressValve1Control, 1, 7 + wordOffset, 9);
            BoolToByte(ref dst, src.Zone4PressValve2Control, 1, 7 + wordOffset, 10);
            BoolToByte(ref dst, src.Zone4VacuumValveControl, 1, 7 + wordOffset, 11);
            BoolToByte(ref dst, src.Zone5PressValve1Control, 1, 7 + wordOffset, 12);
            BoolToByte(ref dst, src.Zone5PressValve2Control, 1, 7 + wordOffset, 13);
            BoolToByte(ref dst, src.Zone5VacuumValveControl, 1, 7 + wordOffset, 14);
            BoolToByte(ref dst, src.Zone6PressValve1Control, 1, 7 + wordOffset, 15);
            BoolToByte(ref dst, src.Zone6PressValve2Control, 1, 8 + wordOffset, 0);
            BoolToByte(ref dst, src.Zone6VacuumValveControl, 1, 8 + wordOffset, 1);
            BoolToByte(ref dst, src.Zone7PressValve1Control, 1, 8 + wordOffset, 2);
            BoolToByte(ref dst, src.Zone7PressValve2Control, 1, 8 + wordOffset, 3);
            BoolToByte(ref dst, src.Zone7VacuumValveControl, 1, 8 + wordOffset, 4);
            BoolToByte(ref dst, src.Zone8PressValve1Control, 1, 8 + wordOffset, 5);
            BoolToByte(ref dst, src.Zone8PressValve2Control, 1, 8 + wordOffset, 6);
            BoolToByte(ref dst, src.Zone8VacuumValveControl, 1, 8 + wordOffset, 7);
            BoolToByte(ref dst, src.TopRingPressValve1Control, 1, 8 + wordOffset, 8);
            BoolToByte(ref dst, src.TopRingPressValve2Control, 1, 8 + wordOffset, 9);
            BoolToByte(ref dst, src.TopRingVacuumValveControl, 1, 8 + wordOffset, 10);
            BoolToByte(ref dst, src.Void16, 1, 8 + wordOffset, 11);
            BoolToByte(ref dst, src.Void17, 1, 8 + wordOffset, 12);
            BoolToByte(ref dst, src.Void18, 1, 8 + wordOffset, 13);
            BoolToByte(ref dst, src.Zone4BlowValve1Control, 1, 8 + wordOffset, 14);
            BoolToByte(ref dst, src.Zone4BlowValve2Control, 1, 8 + wordOffset, 15);
            BoolToByte(ref dst, src.Zone4AtmosphericValveControl, 1, 9 + wordOffset, 0);
            BoolToByte(ref dst, src.AirBagVacuumGeneratorIntletValveControl, 1, 9 + wordOffset, 1);
            BoolToByte(ref dst, src.AirBagVacuumGeneratorOutletValveControl, 1, 9 + wordOffset, 2);
            BoolToByte(ref dst, src.VacuumGeneratorIntletValveControl, 1, 9 + wordOffset, 3);
            BoolToByte(ref dst, src.VacuumGeneratorOutletValveControl, 1, 9 + wordOffset, 4);
            BoolToByte(ref dst, src.CatchWaterBlowValveControl, 1, 9 + wordOffset, 5);
            BoolToByte(ref dst, src.CatchWaterDrainValveControl, 1, 9 + wordOffset, 6);
            BoolToByte(ref dst, src.HeadExternalBlowN2ValveControl, 1, 9 + wordOffset, 7);
            BoolToByte(ref dst, src.Void19, 1, 9 + wordOffset, 8);
            BoolToByte(ref dst, src.Void20, 1, 9 + wordOffset, 9);
            BoolToByte(ref dst, src.Void21, 1, 9 + wordOffset, 10);
            BoolToByte(ref dst, src.Void22, 1, 9 + wordOffset, 11);
            BoolToByte(ref dst, src.Void23, 1, 9 + wordOffset, 12);
            BoolToByte(ref dst, src.Void24, 1, 9 + wordOffset, 13);
            BoolToByte(ref dst, src.Void25, 1, 9 + wordOffset, 14);
            BoolToByte(ref dst, src.Void26, 1, 9 + wordOffset, 15);
            BoolToByte(ref dst, src.Zone1VentValveControl, 1, 10 + wordOffset, 0);
            BoolToByte(ref dst, src.Zone2VentValveControl, 1, 10 + wordOffset, 1);
            BoolToByte(ref dst, src.Zone3VentValveControl, 1, 10 + wordOffset, 2);
            BoolToByte(ref dst, src.Zone5VentValveControl, 1, 10 + wordOffset, 3);
            BoolToByte(ref dst, src.Zone6VentValveControl, 1, 10 + wordOffset, 4);
            BoolToByte(ref dst, src.Zone7VentValveControl, 1, 10 + wordOffset, 5);
            BoolToByte(ref dst, src.Zone8VentValveControl, 1, 10 + wordOffset, 6);
            BoolToByte(ref dst, src.TopRingVentValveControl, 1, 10 + wordOffset, 7);
        }
        private static void stPolishIOData_ToByteBase(ref byte[] dst, Array src, int len = 1, int wordOffset = 0)
        {
            int[] indices = NewIndices(src);
            int offset = wordOffset;
            for (int i = 0; i < len; i++)
            {
                NextIndices(src, indices);
                stPolishIOData_ToByte(ref dst, (stPolishIOData)src.GetValue(indices), len, wordOffset);
                wordOffset += stPolishIOData_WSIZE;
            }
        }
        public static void stPolishIOData_ToByte(ref byte[] dst, stPolishIOData[] src, int len = 1, int wordOffset = 0)
        {
            stPolishIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishIOData_ToByte(ref byte[] dst, stPolishIOData[,] src, int len = 1, int wordOffset = 0)
        {
            stPolishIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishIOData_ToByte(ref byte[] dst, stPolishIOData[,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishIOData_ToByte(ref byte[] dst, stPolishIOData[,,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishIOData_ToByte(ref byte[] dst, stPolishIOData[,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishIOData_ToByte(ref byte[] dst, stPolishIOData[,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishIOData_ToByte(ref byte[] dst, stPolishIOData[,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishIOData_ToByteBase(ref dst, src, len, wordOffset);
        }
        public static void stPolishIOData_ToByte(ref byte[] dst, stPolishIOData[,,,,,,,] src, int len = 1, int wordOffset = 0)
        {
            stPolishIOData_ToByteBase(ref dst, src, len, wordOffset);
        }

    }
}
