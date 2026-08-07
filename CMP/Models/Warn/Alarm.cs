using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Warn
{
    [SugarTable("Alarm")]
    public  class Alarm : BindableBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int AlarmId { get; set; } = 0;

        public string AlarmCode { get; set; } = "";
        public String AlarmName { get; set; } = "";

        public String Type { get; set; } = "";

        public Unit Unit { get; set; } = Unit.None;

        public string DefaultOperation { get; set; } = "";

        public String Description { get; set; } = "";

        [SugarColumn(ColumnDataType = "datetime")]
        public DateTime AlarmTime { get; set; } = DateTime.Now;

        public String Comment { get; set; } = "";

        public bool IsAlarmed { get; set; } = false;

        public bool Enabled { get; set; } = true;

        public string PropertyName { get; set; } = "";
    }

    public enum Unit
    {
        None = -1,
        PolishA = 0,
        PolishB = 1,
        PolishC = 2,
        PolishD = 3,
        RollerA1 = 4,
        RollerA2 = 5,
        RollerA3 = 6,
        RollerB1 = 7,
        RollerB2 = 8,
        RollerB3 = 9,
        PenA1 = 10,
        PenA2 = 11,
        PenB1 = 12,
        PenB2 = 13,
        DryA = 14,
        DryB = 15,
        EFEMRobot=16,
        TransBuffer = 17,
        TransRobot = 18,
        FixABuffer = 19,
        FixALTP1 = 20,
        FixALTP2 = 21,
        FixALTP3 = 22,
        FixBBuffer = 23,
        FixBLTP1 = 24,
        FixBLTP2 = 25,
        FixBLTP3 = 26,
        CleanARobot1 = 27,
        CleanARobot2 = 28,
        CleanBRobot1 = 29,
        CleanBRobot2 = 30,
        Robot = -2
    }
}
