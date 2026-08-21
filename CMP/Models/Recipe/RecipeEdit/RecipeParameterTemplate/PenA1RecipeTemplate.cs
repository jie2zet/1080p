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
            Unit = "s",
            Type = "float",
            Min = 0,
            Max = 3000,
            Default = "10",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "Wafer Rotation Speed",
            Unit = "r/min",
            Type = "float",
            Min = 50,
            Max = 1500,
            Default = "50",
            Options = [],
            CompatibleName = ["ChuckRotateVelocity"],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "Pen Head Rotation Speed",
            Unit = "r/min",
            Type = "float",
            Min = 50,
            Max = 180,
            Default = "50",
            Options = [],
            CompatibleName = ["PenRotateVelocity"],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Pen Arm  Sweep Start Position",
            Unit = "deg",
            Type = "int",
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
            Unit = "deg",
            Type = "int",
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
            Unit = "deg/s",
            Type = "float",
            Min = 5,
            Max = 100,
            Default = "20",
            Options = [],
            CompatibleName = ["PenArmSweepCenterVelocity"],
            Steps = []
          },
            new()
          {
            Id = 8,
            Name = "Pen Arm  Sweep Edge Speed",
            Unit = "deg/s",
            Type = "float",
            Min = 5,
            Max = 100,
            Default = "20",
            Options = [],
            CompatibleName = ["PenArmSweepEdgeVelocity"],
            Steps = []
          },
            new()
          {
            Id = 9,
            Name = "Pen Arm  Cycle",
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
            Unit = "N",
            Type = "float",
            Min = 1,
            Max = 2.5,
            Default = "1",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 11,
            Name = "WF Upper Face N2 Flow",
            Unit = "L/min",
            Type = "float",
            Min = 0,
            Max = 100,
            Default = "30",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 12,
            Name = "WF Upper Face DIW Flow",
            Unit = "mL/min",
            Type = "float",
            Min = 500,
            Max = 1500,
            Default = "1000",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 13,
            Name = "WF Lower Face DIW Flow",
            Unit = "mL/min",
            Type = "float",
            Min = 500,
            Max = 1500,
            Default = "1000",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 14,
            Name = "WF Lower Face N2 Supply",
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
            Unit = "",
            Type = "float",
            Min = 0,
            Max = 3500,
            Default = "1000",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 19,
            Name = "WF Chemical Ratio",
            Unit = "",
            Type = "int",
            Min = 0,
            Max = 10,
            Default = "0",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 20,
            Name = "Pen Arm Type",
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
