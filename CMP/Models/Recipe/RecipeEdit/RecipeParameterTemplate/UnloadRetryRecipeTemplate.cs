using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate
{
    public static class UnloadRetryRecipeTemplate
    {
        public static ObservableCollection<Recipe.Data> Template =
        [
            new()
          {
            Id = 1,
            Name = "Process Type",
            Unit = "",
            Type = "list",
            Min = -1,
            Max = 15,
            Default = "15",
            Options = ["15:Process Unload", "-1:Recipe End"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 2,
            Name = "Step Control",
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 10,
            Default = "0",
            Options = [
              "0:Time",
              "1:Wafer In RB",
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
            Unit = "Kpa",
            Type = "float",
            Min = -100.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "Zone Max Pressure",
            Unit = "Kpa",
            Type = "float",
            Min = -100.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Max Time",
            Unit = "s",
            Type = "float",
            Min = 0.00,
            Max = 1000.00,
            Default = "10.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 6,
            Name = "Zone 1 Type",
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
            Id = 7,
            Name = "Zone 2 Type",
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "3",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 8,
            Name = "Zone 3 Type",
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "3",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 9,
            Name = "Zone 4 Type",
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "3",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 10,
            Name = "Zone 5 Type",
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "3",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 11,
            Name = "Zone 6 Type",
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "3",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 12,
            Name = "Zone 7 Type",
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
            Id = 13,
            Name = "Zone 8 Type",
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
            Id = 14,
            Name = "Retainer Ring Type",
            Unit = "",
            Type = "list",
            Min = 1,
            Max = 3,
            Default = "2",
            Options = ["1:Vent", "2:Vac", "3:Press"],
            CompatibleName = [
              "TopRingType",
              "RetainerRingType"
            ],
            Steps = []
          },
            new()
          {
            Id = 15,
            Name = "Zone 1 Pressure",
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
            Id = 16,
            Name = "Zone 2 Pressure",
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "7.50",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 17,
            Name = "Zone 3 Pressure",
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "7.50",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 18,
            Name = "Zone 4 Pressure",
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "5.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 19,
            Name = "Zone 5 Pressure",
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "7.50",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 20,
            Name = "Zone 6 Pressure",
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "7.50",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 21,
            Name = "Zone 7 Pressure",
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
            Name = "Zone 8 Pressure",
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
            Name = "Retainer Ring Pressure",
            Unit = "Kpa",
            Type = "float",
            Min = 0.00,
            Max = 50.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [
              "TopRingPressure",
              "RetainerRingPressure"
            ],
            Steps = []
          },
            new()
          {
            Id = 24,
            Name = "Head Middle Wash",
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 1,
            Default = "",
            Options = ["0:Close", "1:Open"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 25,
            Name = "Head External Wash",
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 150,
            Default = "20",
            Options = ["0:Close", "1:Open"],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 26,
            Name = "Head Rotation",
            Unit = "r/min",
            Type = "float",
            Min = 0.00,
            Max = 200.00,
            Default = "0.00",
            Options = [],
            CompatibleName = [],
            Steps = []
          },
            new()
          {
            Id = 27,
            Name = "Wafer Release Select",
            Unit = "",
            Type = "list",
            Min = 0,
            Max = 3,
            Default = "0",
            Options = ["0:N2", "1:DIW", "2:All", "3:Disable"],
            CompatibleName = [
              "BlowWFSelect",
              "WaferReleaseSelect"
            ],
            Steps = []
          },
        ];
    }
}
