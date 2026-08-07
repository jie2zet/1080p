using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate
{
    public static class RollRecipeTemplate
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
            Max = 1,
            Default = "1",
            Options = ["1: Roll", "0: Disabled", "-1: Recipe End"],
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
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "Upper Roller Rotate Velocity",
            Unit = "r/min",
            Type = "float",
            Min = 50,
            Max = 600,
            Default = "50",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "Lower Roller Rotate Velocity",
            Unit = "r/min",
            Type = "float",
            Min = 50,
            Max = 600,
            Default = "50",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Pillar  Rotate Velocity",
            Unit = "r/min",
            Type = "float",
            Min = 50,
            Max = 600,
            Default = "50",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 6,
            Name = "Upper Roller Pressure",
            Unit = "N",
            Type = "float",
            Min = 0,
            Max = 500,
            Default = "30",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 7,
            Name = "Lower Roller Pressure",
            Unit = "N",
            Type = "float",
            Min = 0,
            Max = 500,
            Default = "30",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 8,
            Name = "WF Upper Face DIW Flowrate",
            Unit = "mL/min",
            Type = "float",
            Min = 100,
            Max = 1000,
            Default = "1000",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 9,
            Name = "WF Lower Face DIW Flowrate",
            Unit = "mL/min",
            Type = "float",
            Min = 100,
            Max = 1000,
            Default = "1000",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 10,
            Name = "WF Upper Face Chemical Supply",
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = [ "0:Disabled", "1:Enabled"],
            Steps = []
          },
            new()
          {
            Id = 11,
            Name = "WF Lower Face Chemical Supply",
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
            Id = 12,
            Name = "Mega Sonic Supply",
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
            Id = 13,
            Name = "Chemical Mixed Total Flowrate",
            Unit = "",
            Type = "float",
            Min = 10,
            Max = 3500,
            Default = "800.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 14,
            Name = "Chemical Ratio",
            Unit = "",
            Type = "float",
            Min = 1,
            Max = 99,
            Default = "8.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 15,
            Name = "Upper Roll Inner Rinse Supply",
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
            Name = "Down Roll Inner Rinse Supply",
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = ["0:Disabled", "1:Enabled"],
            Steps = []
          },
        ];
    }
}
