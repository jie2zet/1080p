using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate
{
    public static class PenA1RecipeTemplate
    {
        public static ObservableCollection<Recipe.Data> Template =
        [
            new()
          {
            Id = 1,
            Name = "Process Type",
            IsUsed = true,
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 5,
            Default = "1",
            Options = ["1:Pen", "5:Rinse", "0:Disabled", "-1:Recipe End"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 2,
            Name = "Process Time",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["5"], "Process Type=5时允许"),
            Unit = "s",
            Type = "float",
            Min = 0.00,
            Max = 3000.00,
            Default = "10.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "Wafer Rotation Speed",
            IsUsed = true,
            Unit = "r/min",
            Type = "float",
            Min = 50.00,
            Max = 1500.00,
            Default = "50.00",
            Options = [],
            CompatibleName = ["ChuckRotateVelocity","WaferRotationSpeed"],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "Pen Head Rotation Speed",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "r/min",
            Type = "float",
            Min = 10.00,
            Max = 180.00,
            Default = "50.00",
            Options = [],
            CompatibleName = ["PenRotateVelocity","PenHeadRotationSpeed"],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Pen Arm  Sweep Start Position",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "deg",
            Type = "option",
            Min = 1,
            Max = 3,
            Default = "2",
            Options = ["1:Start Edge", "2:Center", "3:End Edge"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 6,
            Name = "Pen Arm  Sweep End Position",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "deg",
            Type = "option",
            Min = 1,
            Max = 3,
            Default = "3",
            Options = ["1:Start Edge", "2:Center", "3:End Edge"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 7,
            Name = "Pen Arm  Sweep Center Speed",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "deg/s",
            Type = "float",
            Min = 0.00,
            Max = 100.00,
            Default = "20.00",
            Options = [],
            CompatibleName = ["PenArmSweepCenterVelocity"],
            Steps = []
          },
            new()
          {
            Id = 8,
            Name = "Pen Arm  Sweep Edge Speed",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "deg/s",
            Type = "float",
            Min = 0.00,
            Max = 100.00,
            Default = "20.00",
            Options = [],
            CompatibleName = ["PenArmSweepEdgeVelocity"],
            Steps = []
          },
            new()
          {
            Id = 9,
            Name = "Pen Arm  Cycle",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "Times",
            Type = "int",
            Min = 0,
            Max = 200,
            Default = "0",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 10,
            Name = "Pen Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "N",
            Type = "float",
            Min = 1.00,
            Max = 2.50,
            Default = "1.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 11,
            Name = "WF Upper Face N2 Flow",
            IsUsed = false,
            Unit = "L/min",
            Type = "float",
            Min = 0.00,
            Max = 100.00,
            Default = "30.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 12,
            Name = "WF Upper Face DIW Flow",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1或5时允许"),
            Unit = "mL/min",
            Type = "float",
            Min = 0.00,
            Max = 1500.00,
            Default = "1000.00",
            Options = [],
            CompatibleName = ["WFUpperFaceDIWFlow","WFUpperDIWFlow"],
            Steps = []
          },
             new()
          {
            Id = 13,
            Name = "WF Lower Face DIW Flow",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1或5时允许"),
            Unit = "mL/min",
            Type = "float",
            Min = 0.00,
            Max = 1500.00,
            Default = "1000.00",
            Options = [],
            CompatibleName = ["WFLowerFaceDIWFlow","WFLowerDIWFlow"],
            Steps = []
          },
             new()
          {
            Id = 14,
            Name = "WF Lower Face N2 Supply",
            IsUsed = false,
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = ["0:Disabled", "1:Enabled"],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 15,
            Name = "WF Upper Face Chemical Supply",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1或5时允许"),
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = ["0:Disabled", "1:Enabled"],
            CompatibleName = ["WFUpperLowerFaceChemicalSupply"],
            Steps = []
          },
             new()
          {
            Id = 16,
            Name = "WF Lower Face Chemical Supply",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1或5时允许"),
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = ["0:Disabled", "1:Enabled"],
            CompatibleName = ["WFUpperFaceChemicalSupply"],
            Steps = []
          },
             new()
          {
            Id = 17,
            Name = "WF Upper Face DI-CO2 Supply",
            IsUsed = false,
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = ["0:Disabled", "1:Enabled"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 18,
            Name = "WF Chemical Mixed Total Flow",
            IsUsed = true,
            OperableRule = new OperableRule(
                            "WF Upper Face Chemical Supply", ["1"],
                            "WF Upper Face Chemical Supply=1或\nWF Lower Face Chemical Supply=1\n时允许",
                            [new OperableRule("WF Lower Face Chemical Supply", ["1"])]),
            Unit = "",
            Type = "float",
            Min = 0.00,
            Max = 3500.00,
            Default = "1000.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 19,
            Name = "WF Chemical Ratio",
            IsUsed = true,
            OperableRule = new OperableRule(
                            "WF Upper Face Chemical Supply", ["1"],
                            "WF Upper Face Chemical Supply=1或\nWF Lower Face Chemical Supply=1\n时允许",
                            [new OperableRule("WF Lower Face Chemical Supply", ["1"])]),
            Unit = "",
            Type = "float",
            Min = 0.00,
            Max = 99.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 20,
            Name = "Pen Arm Type",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "0",
            Options = ["0:2Way", "1:1Way"],
            CompatibleName = [],
            Steps = []
          },
        ];
    }
}
