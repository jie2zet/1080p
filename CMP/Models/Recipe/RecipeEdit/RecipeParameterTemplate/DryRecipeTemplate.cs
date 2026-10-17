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
            IsUsed = true,
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
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["3"], "Process Type=3时允许"),
            Unit = "s",
            Type = "float",
            Min = 0,
            Max = 9999,
            Default = "0",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 3,
            Name = "Wafer Rotation Speed",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),
            Unit = "r/min",
            Type = "float",
            Min = 0.00,
            Max = 1800.00,
            Default = "300.00",
            Options = [],
            CompatibleName = ["ChuckRotateVelocity","WaferRotationSpeed"],
            Steps = []
          },
            new()
          {
            Id = 4,
            Name = "IPA Arm  Sweep Start Position",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "deg",
            Type = "option",
            Min = 1.00,
            Max = 5.00,
            Default = "1.00",
            Options = ["1:Start Position", "2: Near Center Position", "3: Middle Position", "4: Near Edge", "5: Edge Position"],
            Steps = []
          },
            new()
          {
            Id = 5,
            Name = "IPA Arm  Sweep End Position",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "deg",
            Type = "option",
            Min = 1.00,
            Max = 5.00,
            Default = "1.00",
            Options = ["1:Start Position", "2: Near Center Position", "3: Middle Position", "4: Near Edge", "5: Edge Position"],
            Steps = []
          },
            new()
          {
            Id = 6,
            Name = "IPA Arm Sweep Center Velocity",
            IsUsed = false,
            Unit = "deg/s",
            Type = "float",
            Min = 1.00,
            Max = 60.00,
            Default = "3.00",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 7,
            Name = "IPA Arm Sweep Edge Velocity",
            IsUsed = false,
            Unit = "deg/s",
            Type = "float",
            Min = 1.00,
            Max = 60.00,
            Default = "3.00",
            Options = [],
            Steps = []
          },
             new()
          {
            Id = 8,
            Name = "IPA Arm Sweep Speed",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "",
            Type = "float",
            Min = 0.00,
            Max = 2.00,
            Default = "0.24",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 9,
            Name = "IPA Arm Cycle",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),
            Unit = "Times",
            Type = "int",
            Min = 0,
            Max = 2,
            Default = "0.237",
            Options = [],
            Steps = []
          },
            new()
          {
            Id = 10,
            Name = "Dilution N2 Gas Flow",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),
            Unit = "L/min",
            Type = "float",
            Min = 0.00,
            Max = 10.00,
            Default = "4.00",
            Options = [],
            CompatibleName = ["WFUpperFaceN2Flow","DilutionN2GasFlow"],
            Steps = []
          },
            new()
          {
            Id = 11,
            Name = "N2 For WF Backside Flow",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),
            Unit = "L/min",
            Type = "float",
            Min = 2.00,
            Max = 100.00,
            Default = "50.00",
            Options = [],
            CompatibleName = ["WFLowerFaceN2Flow1", "N2ForWFBacksideFlow"],
            Steps = []
          },
            new()
          {
            Id = 12,
            Name = "Outer Rinse Flow",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),
            Unit = "ml/min",
            Type = "float",
            Min = 0.00,
            Max = 1000.00,
            Default = "100.00",
            Options = [],
            CompatibleName = ["WFUpperFaceDIWFlow2", "OuterRinseFlow"],
            Steps = []
          },
            new()
          {
            Id = 13,
            Name = "Bubbling N2 Gas Flow",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),
            Unit = "L/min",
            Type = "float",
            Min = 0.00,
            Max = 10.00,
            Default = "0.5",
            Options = [],
            CompatibleName = ["WFUpperFaceIPASupply", "BubblingN2GasFlow"],
            Steps = []
          },
             new()
          {
            Id = 14,
            Name = "Upper Rinse Supply",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = ["0:Disabled", "1:Enabled"],
            CompatibleName = ["WFLowerFaceN2Supply2", "UpperRinseSupply"],
            Steps = []
          },
             new()
          {
            Id = 15,
            Name = "Lower Rinse Supply",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),
            Unit = "",
            Type = "option",
            Min = 0,
            Max = 1,
            Default = "1",
            Options = ["0:Disabled", "1:Enabled"],
            CompatibleName = ["WFUpperFaceDIWSupply1", "LowerRinseSupply"],
            Steps = []
          },
            new()
          {
            Id = 16,
            Name = "Wafer Rotation Acceleration",
            IsUsed = true,
            OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),
            Unit = "",
            Type = "float",
            Min = 0,
            Max = 2000,
            Default = "100",
            Options = [],
            CompatibleName = ["WFLowerFaceDIWSupply", "WaferRotationAcceleration"],
            Steps = []
          },
        ];
    }
}
