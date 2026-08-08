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
            Steps = []
          },
            new()
          {
            Id = 2,
            Name = "Process Time",
            Unit = "s",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "Chuck Rotate Velocity",
            Unit = "r/min",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "Pen Rotate Velocity",
            Unit = "r/min",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Pen Arm  Sweep Start Position",
            Unit = "deg",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 6,
            Name = "Pen Arm  Sweep End Position",
            Unit = "deg",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
          },
            new()
          {
            Id = 7,
            Name = "Pen Arm  Sweep Center Velocity",
            Unit = "deg/s",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 8,
            Name = "Pen Arm  Sweep Edge Velocity",
            Unit = "deg/s",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 9,
            Name = "Pen Arm  Cycle",
            Unit = "Times",
            Type = "int",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 10,
            Name = "Pen Pressure",
            Unit = "N",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 11,
            Name = "WF Upper Face N2 Flow",
            Unit = "L/min",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 12,
            Name = "WF Upper Face DIW Flow",
            Unit = "mL/min",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
             new()
          {
            Id = 13,
            Name = "WF Lower Face DIW Flow",
            Unit = "mL/min",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
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
            Steps = []
          },
             new()
          {
            Id = 15,
            Name = "WF UpperLower Face Chemical Supply",
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = ["0:Disabled", "1:Enabled"],
            Steps = []
          },
             new()
          {
            Id = 16,
            Name = "WF Upper Face Chemical Supply",
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = ["0:Disabled", "1:Enabled"],
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
            Steps = []
          },
            new()
          {
            Id = 18,
            Name = "WF Chemical Mixed Total Flow",
            Unit = "",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 19,
            Name = "WF Chemical Ratio",
            Unit = "",
            Type = "float",
            Min = 0,
            Max = 0,
            Default = "0",
            Options = [],
            Steps = []
          },
        ];
    }
}
