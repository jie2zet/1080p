using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate
{
    public static class ConditionRecipeTemplate
    {
        public static ObservableCollection<Recipe.Data> Template =
        [
            new()
          {
            Id = 1,
            Name = "Process Type",
            Unit = "",
            Type = "option",
            Min = 1,
            Max = 2,
            Default = "1",
            Options = ["1:InsituCondition", "25:Condition", "-1:Recipe End"],
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
            Max = 9999,
            Default = "100.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "InSitu",
            Unit = "",
            Type = "list",
            Min = -1,
            Max = 26,
            Default = "5",
            Options = ["1:On","2:Exsit"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "Platen Rotation",
            Unit = "r/min",
            Type = "float",
            Min = 0,
            Max = 180,
            Default = "10.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Conditioner Head Rotation",
            Unit = "r/min",
            Type = "float",
            Min = 0,
            Max = 100,
            Default = "10.00",
            Options = [],
            CompatibleName = [
              "ConditionHeadRotation",
              "ConditionerHeadRotation"
            ],
            Steps = []
          },
            new()
          {
            Id = 6,
            Name = "Condition Pressure",
            Unit = "kPa",
            Type = "float",
            Min = 3,
            Max = 78,
            Default = "10.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 7,
            Name = "Condition Recipe",
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 5,
            Default = "0",
            Options = [
              "0:None",
              "1:Condition Recipe1",
              "2:Condition Recipe2",
              "3:Condition Recipe3",
              "4:Condition Recipe4",
              "5:Condition Recipe5"
            ],
            CompatibleName = [
              "ConditionSweepRecipe",
              "ConditionRecipe"
            ],
            Steps = []
          },
            new()
          {
            Id = 8,
            Name = "Platen Wash Selection",
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 1,
            Default = "0",
            Options = ["0:Close", "1:Open"],
            CompatibleName = [
              "PlantWash",
              "PlatenWashSelection"
            ],
            Steps = []
          },
            new()
          {
            Id = 9,
            Name = "Slurry Line 1 Type",
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 1,
            Default = "0",
            Options = [
                "0:Slurry", 
                "1:DIW",
                "2:Close"
                ],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 10,
            Name = "Slurry Line 2 Type",
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 1,
            Default = "0",
            Options = [
                "0:Slurry",
                "1:DIW",
                "2:Close"
                ],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 11,
            Name = "Slurry Line 3 Type",
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 1,
            Default = "0",
            Options = [
                "0:Slurry",
                "1:DIW",
                "2:Close"
                ],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 12,
            Name = "Slurry Line 1 Flow",
            Unit = "mL/min",
            Type = "float",
            Min = 0,
            Max = 500,
            Default = "100.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 13,
            Name = "Slurry Line 2 Flow",
            Unit = "mL/min",
            Type = "float",
            Min = 0,
            Max = 500,
            Default = "100.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 14,
            Name = "Slurry Line 3 Flow",
            Unit = "mL/min",
            Type = "float",
            Min = 0,
            Max = 500,
            Default = "100.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 15,
            Name = "Sweep Stop Time Type",
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 1,
            Default = "0",
            Options = ["0:Time", "1:Ratio"],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 16,
            Name = "Ratio",
            Unit = "",
            Type = "float",
            Min = 0,
            Max = 100,
            Default = "30.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
        ];
    }
}
