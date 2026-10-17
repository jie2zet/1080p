using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate
{
    public static class InsituConditionRecipeTemplate
    {
        public static ObservableCollection<Recipe.Data> Template =
        [
            new()
          {
            Id = 1,
            Name = "Process Type",
            IsUsed = false,
            Unit = "",
            Type = "option",
            Min = 1,
            Max = 2,
            Default = "1",
            Options = ["1:Condition", "2:None"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 2,
            Name = "Process Time",
            IsUsed = true,
            Unit = "s",
            Type = "float",
            Min = 0.00,
            Max = 9999.00,
            Default = "5.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "InSitu",
            IsUsed = false,
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
            IsUsed = false,
            Unit = "r/min",
            Type = "float",
            Min = 0.00,
            Max = 30.00,
            Default = "30.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Conditioner Head Rotation",
            IsUsed = true,
            Unit = "r/min",
            Type = "float",
            Min = 0.00,
            Max = 100.00,
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
            IsUsed = true,
            Unit = "Kpa",
            Type = "float",
            Min = 3.00,
            Max = 78.00,
            Default = "10.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 7,
            Name = "Condition Sweep Recipe",
            IsUsed = true,
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
            IsUsed = false,
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 1,
            Default = "0",
            Options = ["0:Close", "1:Open"],
            CompatibleName = [
              "PlantWash",
              "PlatenWash",
              "PlatenWashSelection"
            ],
            Steps = []
          },
            new()
          {
            Id = 9,
            Name = "Slurry Line 1 Type",
            IsUsed = false,
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
            IsUsed = false,
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
            IsUsed = false,
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
            IsUsed = false,
            Unit = "mL/min",
            Type = "float",
            Min = 0.00,
            Max = 500.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 13,
            Name = "Slurry Line 2 Flow",
            IsUsed = false,
            Unit = "mL/min",
            Type = "float",
            Min = 0.00,
            Max = 500.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 14,
            Name = "Slurry Line 3 Flow",
            IsUsed = false,
            Unit = "mL/min",
            Type = "float",
            Min = 0.00,
            Max = 500.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 15,
            Name = "Sweep Stop Time Type",
            IsUsed = true,
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
            IsUsed = true,
            Unit = "",
            Type = "float",
            Min = 0.00,
            Max = 100.00,
            Default = "30.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
        ];
    }
}
