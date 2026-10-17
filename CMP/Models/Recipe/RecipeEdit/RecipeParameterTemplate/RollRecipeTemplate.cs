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
            IsUsed = true,
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
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1、5时允许"),
            Unit = "s",
            Type = "float",
            Min = 0.00,
            Max = 3000.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "Upper Roller Rotate Speed",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "r/min",
            Type = "float",
            Min = 50.00,
            Max = 300.00,
            Default = "50.00",
            Options = [],
            CompatibleName = ["UpperRollerRotateVelocity", "UpperRollerRotateSpeed"],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "Lower Roller Rotate Speed",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "r/min",
            Type = "float",
            Min = 50.00,
            Max = 300.00,
            Default = "50.00",
            Options = [],
            CompatibleName = ["LowerRollerRotateVelocity", "LowerRollerRotateSpeed"],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "Wafer Rotation",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1、5时允许"),
            Unit = "r/min",
            Type = "float",
            Min = 50.00,
            Max = 250.00,
            Default = "50.00",
            Options = [],
            CompatibleName = ["PillarRotateVelocity", "WaferRotation"],
            Steps = []
          },
            new()
          {
            Id = 6,
            Name = "Upper Roller Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "N",
            Type = "float",
            Min = 2.00,
            Max = 12.00,
            Default = "6.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 7,
            Name = "Lower Roller Pressure",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "N",
            Type = "float",
            Min = 2.00,
            Max = 12.00,
            Default = "6.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 8,
            Name = "WF Upper Face DIW Flowrate",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1、5时允许"),
            Unit = "mL/min",
            Type = "float",
            Min = 0.00,
            Max = 2000.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 9,
            Name = "WF Lower Face DIW Flowrate",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1、5时允许"),
            Unit = "mL/min",
            Type = "float",
            Min = 0.00,
            Max = 2000.00,
            Default = "0.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 10,
            Name = "WF Upper Face Chemical Supply",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1、5时允许"),
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
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1、5时允许"),
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
            IsUsed = false,
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
            IsUsed = true,
            OperableRule = new OperableRule("WF Upper Face Chemical Supply", ["1"], "WF Upper Face Chemical Supply=1时允许"),
            Unit = "",
            Type = "float",
            Min = 10.00,
            Max = 3500.00,
            Default = "800.00",
            Options = [],
            CompatibleName = ["ChemicalMixedTotalFlowrate","TotalDilutedChemicalFlowRate"],
            Steps = []
          },
            new()
          {
            Id = 14,
            Name = "Chemical Dilution Ratio",
            IsUsed = true,
            OperableRule = new OperableRule("WF Upper Face Chemical Supply", ["1"], "WF Upper Face Chemical Supply=1时允许"),
            Unit = "",
            Type = "float",
            Min = 1.00,
            Max = 99.00,
            Default = "8.00",
            Options = [],
            CompatibleName = ["ChemicalRatio","ChemicalDilutionRatio"],
            Steps = []
          },
            new()
          {
            Id = 15,
            Name = "Upper Roll Inner Rinse Supply",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1、5时允许"),
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
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "5"], "Process Type=1、5时允许"),
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
