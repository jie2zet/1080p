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
            Max = 5,
            Default = "1",
            Options = ["1: Roll", "5: Rinse", "0: Disabled", "-1: Recipe End"],
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
            Name = "Upper Roller Rotate Speed",
            Unit = "r/min",
            Type = "float",
            Min = 50,
            Max = 600,
            Default = "50",
            Options = [],
            CompatibleName = ["UpperRollerRotateVelocity", "UpperRollerRotateSpeed"],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "Lower Roller Rotate Speed",
            Unit = "r/min",
            Type = "float",
            Min = 50,
            Max = 300,
            Default = "50",
            Options = [],
            CompatibleName = ["LowerRollerRotateVelocity", "LowerRollerRotateSpeed"],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Wafer Rotation",
            Unit = "r/min",
            Type = "float",
            Min = 50,
            Max = 250,
            Default = "50",
            Options = [],
            CompatibleName = ["PillarRotateVelocity", "WaferRotation"],
            Steps = []
          },
            new()
          {
            Id = 6,
            Name = "Upper Roller Pressure",
            Unit = "N",
            Type = "float",
            Min = 2,
            Max = 12,
            Default = "6",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 7,
            Name = "Lower Roller Pressure",
            Unit = "N",
            Type = "float",
            Min = 2,
            Max = 12,
            Default = "6",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 8,
            Name = "WF Upper Face DIW Flowrate",
            Unit = "mL/min",
            Type = "float",
            Min = 0,
            Max = 2000,
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
            Min = 0,
            Max = 2000,
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
            Name = "Total Diluted Chemical Flow Rate",
            Unit = "",
            Type = "float",
            Min = 10,
            Max = 3500,
            Default = "800.00",
            Options = [],
            CompatibleName = ["ChemicalMixedTotalFlowrate","TotalDilutedChemicalFlowRate"],
            Steps = []
          },
            new()
          {
            Id = 14,
            Name = "Chemical Dilution Ratio",
            Unit = "",
            Type = "float",
            Min = 1,
            Max = 99,
            Default = "8.00",
            Options = [],
            CompatibleName = ["ChemicalRatio","ChemicalDilutionRatio"],
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
