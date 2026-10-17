using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate
{
    public static class ExsituConditionRecipeTemplate
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
            Min = 1,
            Max = 2,
            Default = "25",
            Options = ["25:Condition", "-1:Recipe End"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 2,
            Name = "Process Time",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
            Unit = "s",
            Type = "float",
            Min = 0.00,
            Max = 9999.00,
            Default = "100.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "InSitu",
            IsUsed = false,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
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
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
            Unit = "r/min",
            Type = "float",
            Min = 0.00,
            Max = 180.00,
            Default = "10.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Conditioner Head Rotation",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
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
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
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
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
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
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
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
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
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
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
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
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
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
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
            Unit = "mL/min",
            Type = "float",
            Min = 0.00,
            Max = 500.00,
            Default = "100.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 13,
            Name = "Slurry Line 2 Flow",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
            Unit = "mL/min",
            Type = "float",
            Min = 0.00,
            Max = 500.00,
            Default = "100.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 14,
            Name = "Slurry Line 3 Flow",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
            Unit = "mL/min",
            Type = "float",
            Min = 0.00,
            Max = 500.00,
            Default = "100.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 15,
            Name = "Sweep Stop Time Type",
            IsUsed = false,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
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
            IsUsed = false,
            OperableRule = new OperableRule("Process Type", ["25"], "Process Type=25时允许"),
            Unit = "",
            Type = "int",
            Min = 0,
            Max = 100,
            Default = "30",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
        ];
    }
}
