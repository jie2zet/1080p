using System;
using System.Collections.Generic;
using System.Text;

namespace CMP.Constants
{
    public static class ChamberExponentMap
    {
        /// <summary>
        /// 与PLC工程师约定的Chamber所对应的指数
        /// </summary>
        public static readonly Dictionary<string, int> ChamberExponentMapping = new ()
        {
            { "PolishA", 0 },
            { "PolishB", 1 },
            { "PolishC", 2 },
            { "PolishD", 3 },
            { "RollerA1", 4 },
            { "RollerA2", 5 },
            { "RollerA3", 6 },
            { "RollerB1", 7 },
            { "RollerB2", 8 },
            { "RollerB3", 9 },
            { "PenA", 10 },
            { "PenB", 12 },
            { "DryA", 14 },
            { "DryB", 15 }         
        };
    }
}