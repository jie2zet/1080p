using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate
{
    public static class DechuckRecipeTemplate
    {
        public static ObservableCollection<Recipe.Data> Template =
        [
            new()
          {
            Id = 1,
            Name = "Process Type",
            IsUsed = true,
            Unit = "",
            Type = "list",
            Min = -1,
            Max = 20,
            Default = "20",
            Options = ["20:Process Dechuck", "-1:Recipe End"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 2,
            Name = "Step Control",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 10,
            Default = "5",
            Options = [
              "0:Time",
              "2:Z1",
              "3:Z2",
              "4:Z3",
              "5:Z4",
              "6:Z5",
              "7:Z6",
              "8:Z7",
              "9:Z8",
              "10:Retainer Ring"
            ],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "Zone Min Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = -100.00,
            Max = 50.00,
            Default = "-50.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "Zone Max Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = -100.00,
            Max = 50.00,
            Default = "-50.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Max Time",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "s",
            Type = "float",
            Min = 0.00,
            Max = 99.00,
            Default = "5.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 6,
            Name = "Platen Rotation",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "r/min",
            Type = "float",
            Min = 0.00,
            Max = 180.00,
            Default = "180.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 7,
            Name = "Head Rotation",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "r/min",
            Type = "float",
            Min = 0.00,
            Max = 150.00,
            Default = "150.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 8,
            Name = "Zone 1 Type",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "2",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 9,
            Name = "Zone 2 Type",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "2",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 10,
            Name = "Zone 3 Type",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "2",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 11,
            Name = "Zone 4 Type",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "2",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 12,
            Name = "Zone 5 Type",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "2",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 13,
            Name = "Zone 6 Type",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "2",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 14,
            Name = "Zone 7 Type",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "1",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 15,
            Name = "Zone 8 Type",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "1",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 16,
            Name = "Retainer Ring Type",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "3",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [
              "RRType",
              "RetainerRingType"
            ],
            Steps = []
          },
            new()
          {
            Id = 17,
            Name = "Zone 1 Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 18,
            Name = "Zone 2 Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 19,
            Name = "Zone 3 Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 20,
            Name = "Zone 4 Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 21,
            Name = "Zone 5 Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 22,
            Name = "Zone 6 Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 23,
            Name = "Zone 7 Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
             new()
          {
            Id = 24,
            Name = "Zone 8 Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 25,
            Name = "Retainer Ring Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "15.00",
            Options = [],
            CompatibleName = [
              "ZoneRRPressure",
              "RetainerRingPressure"
            ],
            Steps = []
          },
            new()
          {
            Id = 26,
            Name = "Plant Wash Selection",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = ["0:Close", "1:Open"],
            CompatibleName = ["PlatenWashSelection"],
            Steps = []
          },
            new()
          {
            Id = 27,
            Name = "Membrane Height",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["20"], "Process Type=20时允许"),
            Unit = "mm",
            Type = "float",
            Min = 0.00,
            Max = 3.00,
            Default = "0.70",
            Options = [],
            CompatibleName = [
              "DechuckHeight",
              "MembraneHeight"
            ],
            Steps = []
          },
        ];
    }
}
