using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate
{
    public static class DryRecipeTemplate
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
            Max = 3,
            Default = "1",
            Options = ["1: Dry With Arm", "3: Dry", "0: Disabled", "-1: Recipe End"],
            Steps = []
          },
            new()
          {
            Id = 2,
            Name = "Process Time",
            Unit = "s",
            Type = "float",
            Min = 0.00,
            Max = 0.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "Chuck Rotate Velocity",
            Unit = "r/min",
            Type = "float",
            Min = 0.00,
            Max = 0.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "IPA Arm  Sweep Start Position",
            Unit = "deg",
            Type = "float",
            Min = 0.00,
            Max = 0.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "IPA Arm  Sweep End Position",
            Unit = "deg",
            Type = "float",
            Min = 0.00,
            Max = 0.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 6,
            Name = "IPA Arm  Sweep Center Velocity",
            Unit = "deg/s",
            Type = "float",
            Min = 0.00,
            Max = 0.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 7,
            Name = "IPA Arm  Sweep Edge Velocity",
            Unit = "deg/s",
            Type = "float",
            Min = 0.00,
            Max = 0.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 8,
            Name = "IPA Arm  Cycle",
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
            Id = 9,
            Name = "WF Upper Face N2 Flow",
            Unit = "L/min",
            Type = "float",
            Min = 0.00,
            Max = 0.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 10,
            Name = "WF Lower Face N2 Flow 1",
            Unit = "L/min",
            Type = "float",
            Min = 0.00,
            Max = 0.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 11,
            Name = "WF Upper Face DIW Flow 2",
            Unit = "L/min",
            Type = "float",
            Min = 0.00,
            Max = 0.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 12,
            Name = "WF Upper Face IPA Supply",
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
            Name = "WF Lower Face N2 Supply 2",
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
            Id = 14,
            Name = "WF Upper Face DIW Supply 1",
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
            Name = "WF Lower Face DIW Supply",
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
