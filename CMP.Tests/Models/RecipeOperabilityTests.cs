#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate;
using CMP.Tests.Services;
using CMP.Views.Tabs.RecipeTab;
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

    // ---------- Task 3: 模板注册表 ----------

    [TestMethod]
    public void ResolveReturnsDryTemplateForDryFolder()
    {
        var template = RecipeTemplateRegistry.Resolve(DryFolder);
        Assert.IsNotNull(template);
        Assert.IsTrue(template!.Count > 0);
        Assert.IsNull(RecipeTemplateRegistry.Resolve("No Such Folder"));
        Assert.IsNull(RecipeTemplateRegistry.Resolve(null));
    }

    [TestMethod]
    public void DecorateWithUnknownFolderIsNoOp()
    {
        var content = new Recipe.Content { Data = [] };
        content.Data!.Add(Row("Process Time", "10"));

        var decorated = RecipeTemplateRegistry.Decorate(content, "No Such Folder");

        Assert.IsFalse(decorated);
        Assert.IsNull(content.Data[0].OperableRule);
    }

    [TestMethod]
    public void LegacyParameterNamesStillReceiveRulesAfterDecorate()
    {
        var content = new Recipe.Content { Data = [] };
        // 现场 111.json 形态：只有 legacy 名
        content.Data!.Add(Row("Process Type", "2: Sweep"));
        content.Data.Add(Row("Chuck Rotate Velocity", "300"));      // -> Wafer Rotation Speed
        content.Data.Add(Row("WF Upper Face N2 Flow", "4"));        // -> Dilution N2 Gas Flow
        content.Data.Add(Row("Pen Arm  Cycle", "3"));               // 未知参数

        RecipeTemplateRegistry.Decorate(content, DryFolder);
        RecipeOperabilityResolver.Recalculate(content);

        var legacy = Find(content, "Chuck Rotate Velocity");
        Assert.IsNotNull(legacy.OperableRule);
        Assert.IsFalse(legacy.Steps[0].IsOperable);                                  // {1,3} 不含 2
        Assert.AreEqual("Process Type=1或3时允许", legacy.Steps[0].OperableReason);

        Assert.IsFalse(Find(content, "WF Upper Face N2 Flow").Steps[0].IsOperable);  // {1,3} 不含 2
        Assert.IsTrue(Find(content, "Pen Arm  Cycle").Steps[0].IsOperable);          // 无规则 -> 可操作
        Assert.IsTrue(Find(content, "Process Type").Steps[0].IsOperable);

        // 反向：Process Type=3 时两行都允许
        var dry = new Recipe.Content { Data = [] };
        dry.Data!.Add(Row("Process Type", "3: Dry"));
        dry.Data.Add(Row("Chuck Rotate Velocity", "300"));
        RecipeTemplateRegistry.Decorate(dry, DryFolder);
        RecipeOperabilityResolver.Recalculate(dry);
        Assert.IsTrue(Find(dry, "Chuck Rotate Velocity").Steps[0].IsOperable);
    }

    [TestMethod]
    public void DecorateDoesNotThrowOnEmptyContent()
    {
        Assert.IsFalse(RecipeTemplateRegistry.Decorate(null, DryFolder));
        Assert.IsFalse(RecipeTemplateRegistry.Decorate(new Recipe.Content(), DryFolder));
    }

    // ---------- Task 4: 模板规则与 Excel 对账 ----------

    private sealed class Fixture
    {
        public List<FixtureRow> Rows { get; set; } = [];
    }

    private sealed class FixtureRow
    {
        public int Index { get; set; }
        public string Parameter { get; set; } = string.Empty;
        public int NotUsed { get; set; }
        public string? Reason { get; set; }
    }

    private static Fixture LoadFixture()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "dry-operability-fixture.json");
        Assert.IsTrue(File.Exists(path), $"缺少夹具文件: {path}");
        return JsonConvert.DeserializeObject<Fixture>(File.ReadAllText(path))!;
    }

    [TestMethod]
    public void DryTemplateIdsAndNormalizedNamesAreUnique()
    {
        var ids = DryRecipeTemplate.Template.Select(d => d.Id).ToList();
        CollectionAssert.AllItemsAreUnique(ids);

        var names = DryRecipeTemplate.Template
            .Select(d => RecipeOperabilityResolver.Normalize(d.Name))
            .ToList();
        CollectionAssert.AllItemsAreUnique(names);
    }

    [TestMethod]
    public void EveryOperableRuleSourceExistsInTheSameTemplate()
    {
        var index = DryRecipeTemplate.Template
            .SelectMany(d => new[] { RecipeOperabilityResolver.Normalize(d.Name) }
                .Concat((d.CompatibleName ?? []).Select(RecipeOperabilityResolver.Normalize)))
            .ToHashSet(StringComparer.Ordinal);

        foreach (var row in DryRecipeTemplate.Template.Where(d => d.OperableRule != null))
        {
            var source = RecipeOperabilityResolver.Normalize(row.OperableRule!.SourceParameter);
            Assert.IsTrue(index.Contains(source),
                $"{row.Name} 的规则来源 {row.OperableRule.SourceParameter} 不在模板中");
        }
    }

    [TestMethod]
    public void DryTemplateRulesMatchExcelFixture()
    {
        var fixture = LoadFixture();
        var template = DryRecipeTemplate.Template;
        var templateByKey = template.ToDictionary(
            d => RecipeOperabilityResolver.Normalize(d.Name), d => d);

        // 1) 模板里有规则的每条，必须在夹具中找到同名条目，且 reason 一致
        foreach (var row in template.Where(d => d.OperableRule != null))
        {
            var key = RecipeOperabilityResolver.Normalize(row.Name);
            var spec = fixture.Rows.FirstOrDefault(r =>
                RecipeOperabilityResolver.Normalize(r.Parameter) == key);

            Assert.IsNotNull(spec, $"夹具缺少参数 {row.Name}");
            Assert.AreEqual(0, spec!.NotUsed, $"{row.Name} 在 Excel 中标为不使用");
            Assert.AreEqual(spec.Reason, row.OperableRule!.Reason, $"{row.Name} 的说明原文不一致");
        }

        // 2) 夹具中所有「启用且带条件」的行，模板必须已实现
        foreach (var spec in fixture.Rows.Where(r => r.NotUsed == 0 && r.Reason != null))
        {
            var key = RecipeOperabilityResolver.Normalize(spec.Parameter);
            Assert.IsTrue(templateByKey.ContainsKey(key), $"模板缺少参数 {spec.Parameter}");
            Assert.IsNotNull(templateByKey[key].OperableRule, $"{spec.Parameter} 未挂规则");
        }

        // 3) 夹具中「不使用=1」的行，模板不得存在
        foreach (var spec in fixture.Rows.Where(r => r.NotUsed == 1))
        {
            var key = RecipeOperabilityResolver.Normalize(spec.Parameter);
            Assert.IsFalse(templateByKey.ContainsKey(key), $"{spec.Parameter} 标为不使用但模板中存在");
        }
    }

    // ---------- Task 5: 视图接线 ----------

    [WpfTestMethod]
    public void StepCellStyleGreysOutOnlyWhenNotOperable()
    {
        var style = RecipeStepCellStyle.CreateCellStyle(2);

        Assert.AreEqual(typeof(DataGridCell), style.TargetType);

        var trigger = style.Triggers.OfType<DataTrigger>().Single();
        Assert.AreEqual("Steps[2].IsOperable", ((Binding)trigger.Binding!).Path.Path);
        Assert.AreEqual(false, trigger.Value);

        var background = trigger.Setters.OfType<Setter>()
            .Single(s => s.Property == Control.BackgroundProperty);
        Assert.AreSame(RecipeStepCellStyle.DisabledBackground, background.Value);

        var foreground = trigger.Setters.OfType<Setter>()
            .Single(s => s.Property == Control.ForegroundProperty);
        Assert.AreSame(RecipeStepCellStyle.DisabledForeground, foreground.Value);
    }

    [WpfTestMethod]
    public void OperabilityBindingsPointAtTheSameStepIndex()
    {
        Assert.AreEqual("Steps[1].IsOperable", RecipeStepCellStyle.CreateIsOperableBinding(1).Path.Path);
        Assert.AreEqual("Steps[1].OperableReason", RecipeStepCellStyle.CreateOperableReasonBinding(1).Path.Path);
    }
}
