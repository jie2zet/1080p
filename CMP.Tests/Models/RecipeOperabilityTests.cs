#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace CMP.Tests.Models;

[TestClass]
public class RecipeOperabilityTests
{
    private const string DryFolder = "Dry Recipe";

    // ---------- Task 1: RecipeOptionValue ----------

    [DataTestMethod]
    [DataRow("1: Dry With Arm", "1")]
    [DataRow("3: Dry", "3")]
    [DataRow("-1: Recipe End", "-1")]
    [DataRow("1", "1")]
    [DataRow("10", "10")]
    [DataRow("  3: Dry  ", "3")]
    [DataRow("", null)]
    [DataRow("   ", null)]
    [DataRow(null, null)]
    [DataRow("abc", null)]
    [DataRow("1abc", null)]
    [DataRow(": 1", null)]
    public void NumericPrefixParsesOptionStrings(string? input, string? expected)
    {
        Assert.AreEqual(expected, RecipeOptionValue.NumericPrefix(input));
    }

    // ---------- 测试夹具 ----------

    /// <summary>克隆模板行：**绝不共享** static 模板的 Steps 集合。</summary>
    private static Recipe.Data CloneTemplateRow(Recipe.Data spec) => new()
    {
        Id = spec.Id,
        Name = spec.Name,
        Unit = spec.Unit,
        Type = spec.Type,
        Min = spec.Min,
        Max = spec.Max,
        Default = spec.Default,
        Options = spec.Options == null ? null : new List<string>(spec.Options),
        CompatibleName = spec.CompatibleName == null ? null : new List<string>(spec.CompatibleName),
        Steps = new ObservableCollection<Recipe.Step>()
    };

    private static Recipe.Data Row(string name, string value, int stepCount = 1)
    {
        var row = new Recipe.Data { Name = name, Type = "float", Default = value, Steps = [] };
        for (int i = 0; i < stepCount; i++)
        {
            row.Steps.Add(new Recipe.Step { Id = i + 1, Value = value });
        }
        return row;
    }

    /// <summary>构建 Dry 配方：Process Type 的每一步都取 processTypeValue，其余参数取模板默认值。</summary>
    private static Recipe.Content BuildDryContent(string processTypeValue, int steps = 1)
    {
        var content = new Recipe.Content { Name = "test", Uuid = "test", Data = [] };

        foreach (var spec in DryRecipeTemplate.Template)
        {
            var row = CloneTemplateRow(spec);
            var isProcessType = RecipeOperabilityResolver.Normalize(spec.Name) == "ProcessType";
            for (int i = 0; i < steps; i++)
            {
                row.Steps.Add(new Recipe.Step
                {
                    Id = i + 1,
                    Value = isProcessType ? processTypeValue : row.Default
                });
            }
            content.Data!.Add(row);
        }

        RecipeTemplateRegistry.Decorate(content, DryFolder);
        RecipeOperabilityResolver.Recalculate(content);
        return content;
    }

    private static Recipe.Data Find(Recipe.Content content, string parameter) =>
        content.Data!.First(d =>
            RecipeOperabilityResolver.Normalize(d.Name) == RecipeOperabilityResolver.Normalize(parameter));

    private static bool IsOperable(Recipe.Content content, string parameter, int stepIndex = 0) =>
        Find(content, parameter).Steps[stepIndex].IsOperable;

    // ---------- Task 2: 求值引擎 ----------

    private static readonly int[] AllProcessTypes = [1, 3, 2, 0, -1];

    [TestMethod]
    public void ProcessTimeIsOperableOnlyForDry()
    {
        foreach (var pt in AllProcessTypes)
        {
            var content = BuildDryContent($"{pt}: x");
            Assert.AreEqual(pt == 3, IsOperable(content, "Process Time"), $"Process Time @ ProcessType={pt}");
        }
    }

    [TestMethod]
    public void IpaArmParametersAreOperableOnlyForDryWithArm()
    {
        string[] parameters =
        [
            "IPA Arm  Sweep Start Position",
            "IPA Arm  Sweep End Position",
            "IPA Arm Sweep Speed",
            "IPA Arm Cycle"
        ];

        foreach (var pt in AllProcessTypes)
        {
            var content = BuildDryContent($"{pt}: x");
            foreach (var parameter in parameters)
            {
                Assert.AreEqual(pt == 1, IsOperable(content, parameter), $"{parameter} @ ProcessType={pt}");
            }
        }
    }

    [TestMethod]
    public void FlowParametersAreOperableForDryWithArmAndDry()
    {
        string[] parameters =
        [
            "Wafer Rotation Speed",
            "Dilution N2 Gas Flow",
            "N2 For WF Backside Flow",
            "Outer Rinse Flow",
            "Bubbling N2 Gas Flow",
            "Upper Rinse Supply",
            "Lower Rinse Supply",
            "Wafer Rotation Acceleration"
        ];

        foreach (var pt in AllProcessTypes)
        {
            var content = BuildDryContent($"{pt}: x");
            foreach (var parameter in parameters)
            {
                Assert.AreEqual(pt == 1 || pt == 3, IsOperable(content, parameter), $"{parameter} @ ProcessType={pt}");
            }
        }
    }

    [TestMethod]
    public void ProcessTypeItselfIsAlwaysOperable()
    {
        foreach (var pt in AllProcessTypes)
        {
            var content = BuildDryContent($"{pt}: x");
            Assert.IsTrue(IsOperable(content, "Process Type"), $"Process Type @ ProcessType={pt}");
            Assert.IsNull(Find(content, "Process Type").Steps[0].OperableReason);
        }
    }

    [TestMethod]
    public void ReasonIsOnlySetWhenNotOperable()
    {
        var content = BuildDryContent("3: Dry");
        var processTime = Find(content, "Process Time");
        Assert.IsTrue(processTime.Steps[0].IsOperable);
        Assert.IsNull(processTime.Steps[0].OperableReason);

        var arm = BuildDryContent("3: Dry");
        var cycle = Find(arm, "IPA Arm Cycle");
        Assert.IsFalse(cycle.Steps[0].IsOperable);
        Assert.AreEqual("Process Type=1时允许", cycle.Steps[0].OperableReason);
    }

    [TestMethod]
    public void MissingSourceRowKeepsEverythingOperable()
    {
        var content = new Recipe.Content { Data = [] };
        content.Data!.Add(Row("Process Time", "10"));

        RecipeTemplateRegistry.Decorate(content, DryFolder);
        RecipeOperabilityResolver.Recalculate(content);

        Assert.IsTrue(content.Data[0].Steps[0].IsOperable);
        Assert.IsNull(content.Data[0].Steps[0].OperableReason);
    }

    [TestMethod]
    public void OutOfRangeStepsStayOperableWhenSourceHasFewerSteps()
    {
        var content = new Recipe.Content { Data = [] };
        content.Data!.Add(Row("Process Type", "1: Dry With Arm"));   // 只有 1 步
        content.Data.Add(Row("Process Time", "10", stepCount: 2));    // 目标有 2 步

        RecipeTemplateRegistry.Decorate(content, DryFolder);
        RecipeOperabilityResolver.Recalculate(content);

        var processTime = Find(content, "Process Time");
        Assert.IsFalse(processTime.Steps[0].IsOperable);   // PT=1 -> Process Time 不允许
        Assert.IsTrue(processTime.Steps[1].IsOperable);    // 越界 -> fail-open
    }

    [TestMethod]
    public void EmptyContentDoesNotThrow()
    {
        RecipeOperabilityResolver.Recalculate(null);
        RecipeOperabilityResolver.Recalculate(new Recipe.Content());
    }

    [TestMethod]
    public void SourceParameterNamesAreCollectedFromRules()
    {
        var content = BuildDryContent("3: Dry");
        var names = RecipeOperabilityResolver.GetSourceParameterNames(content);

        CollectionAssert.Contains(names.ToList(), "ProcessType");
        Assert.IsTrue(RecipeOperabilityResolver.MatchesAnyName(Find(content, "Process Type"), names));
        Assert.IsFalse(RecipeOperabilityResolver.MatchesAnyName(Find(content, "Process Time"), names));
    }

    [TestMethod]
    public void DerivedOperabilityFieldsAreNeverSerialized()
    {
        var content = BuildDryContent("3: Dry");
        var json = JsonConvert.SerializeObject(content);

        Assert.IsFalse(json.Contains("IsOperable"), json);
        Assert.IsFalse(json.Contains("OperableReason"), json);
        Assert.IsFalse(json.Contains("OperableRule"), json);
        Assert.IsFalse(json.Contains("SourceParameter"), json);
        Assert.IsFalse(json.Contains("AllowValues"), json);

        var roundTrip = JsonConvert.DeserializeObject<Recipe.Content>(json);
        Assert.IsNotNull(roundTrip);
        Assert.AreEqual(content.Data!.Count, roundTrip!.Data!.Count);
        Assert.IsTrue(roundTrip.Data[0].Steps[0].IsOperable);   // 反序列化后派生态回到默认 true
    }
}
