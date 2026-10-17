#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CMP.Models.Recipe.RecipeEdit;
using CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate;
using CMP.Tests.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using CMP.ViewModels.Tabs.RecipeTab;

namespace CMP.Tests.Models;

[TestClass]
public class RecipeOperabilityTests
{
    private const string DryFolder = "Dry Recipe";
    private const string PenFolder = "Pen Recipe";
    private const string RollFolder = "Roll Recipe";
    private const string PolishFolder = RecipeTemplateRegistry.PolishRecipeFolder;
    private const string HeadSweepFolder = RecipeTemplateRegistry.HeadSweepRecipeFolder;
    private const string LoadFolder = RecipeTemplateRegistry.LoadRecipeFolder;
    private const string LoadRetryFolder = RecipeTemplateRegistry.LoadRetryRecipeFolder;
    private const string UnloadFolder = RecipeTemplateRegistry.UnloadRecipeFolder;
    private const string UnloadRetryFolder = RecipeTemplateRegistry.UnloadRetryRecipeFolder;
    private const string DechuckFolder = RecipeTemplateRegistry.DechuckRecipeFolder;
    private const string DechuckRetryFolder = RecipeTemplateRegistry.DechuckRetryRecipeFolder;
    private const string ConditionFolder = RecipeTemplateRegistry.InsituConditionRecipeFolder;
    private const string ExsituConditionFolder = RecipeTemplateRegistry.ExsituConditionRecipeFolder;
    private const string InsituConditionSweepFolder = RecipeTemplateRegistry.InsituConditionSweepRecipeFolder;
    private const string ExsituConditionSweepFolder = RecipeTemplateRegistry.ExsituConditionSweepRecipeFolder;

    /// <summary>
    /// 全部参数模板（名称 -> 模板）。新增模板时在此登记，一致性测试与夹具对账自动覆盖。
    /// 名称与 <see cref="RecipeTemplateRegistry"/> 的文件夹名一一对应。
    /// </summary>
    private static readonly (string Name, ObservableCollection<Recipe.Data> Template)[] AllTemplates =
    [
        ("Dry Recipe", DryRecipeTemplate.Template),
        ("Pen Recipe", PenA1RecipeTemplate.Template),
        ("Roll Recipe", RollRecipeTemplate.Template),
        ("Polish Recipe", PolishRecipeTemplate.Template),
        ("Head Sweep Recipe", HeadSweepRecipeTemplate.Template),
        ("Load Recipe", LoadRecipeTemplate.Template),
        ("Load Retry Recipe", LoadRetryRecipeTemplate.Template),
        ("UnLoad Recipe", UnloadRecipeTemplate.Template),
        ("UnLoad Retry Recipe", UnloadRetryRecipeTemplate.Template),
        ("Dechuck Recipe", DechuckRecipeTemplate.Template),
        ("Dechuck Retry Recipe", DechuckRetryRecipeTemplate.Template),
        ("InsituCondition Recipe", InsituConditionRecipeTemplate.Template),
        ("ExsituCondition Recipe", ExsituConditionRecipeTemplate.Template),
        ("InsituCondition Sweep Recipe", InsituContidionSweepRecipeTemplate.Template),
        ("ExsituCondition Sweep Recipe", ExsituContidionSweepRecipeTemplate.Template),
    ];

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
    // 全角 / 变体冒号：Excel 人工维护时极易混入，必须与半角等价
    [DataRow("1：Dry With Arm", "1")]
    [DataRow("3：Dry", "3")]
    [DataRow("-1：Recipe End", "-1")]
    [DataRow("  7：x  ", "7")]
    [DataRow("1﹕x", "1")]
    [DataRow("1∶x", "1")]
    public void NumericPrefixParsesOptionStrings(string? input, string? expected)
    {
        Assert.AreEqual(expected, RecipeOptionValue.NumericPrefix(input));
    }

    [DataTestMethod]
    [DataRow("1:HeadSweep1", "HeadSweep1")]
    [DataRow("1：HeadSweep1", "HeadSweep1")]
    [DataRow("1﹕HeadSweep1", "HeadSweep1")]
    [DataRow("HeadSweep1", "HeadSweep1")]
    [DataRow("", "")]
    [DataRow(null, "")]
    public void PartAfterSeparatorStripsNumericPrefix(string? input, string expected)
    {
        Assert.AreEqual(expected, RecipeOptionValue.PartAfterSeparator(input));
    }

    [DataTestMethod]
    // 与各 Operation Tab 的 GetRealVal 语义一致：数值目标取数字部分，字符串目标取标签部分
    [DataRow("1: Dry With Arm", "1")]
    [DataRow("1：Dry With Arm", "1")]
    [DataRow("-1:Recipe End", "-1")]
    [DataRow("10:Process Load", "10")]
    [DataRow("1", "1")]
    [DataRow("abc", "abc")]
    [DataRow("", "")]
    [DataRow(null, "")]
    public void NumericPartMatchesGetRealValSemantics(string? input, string expected)
    {
        Assert.AreEqual(expected, RecipeOptionValue.NumericPart(input));
    }

    [DataTestMethod]
    [DataRow("1: HeadSweep1", "HeadSweep1")]
    [DataRow("1：HeadSweep1", "HeadSweep1")]
    [DataRow("HeadSweep1", "HeadSweep1")]
    [DataRow("0:DIW", "DIW")]
    [DataRow("", "")]
    [DataRow(null, "")]
    public void LabelPartMatchesGetRealValSemantics(string? input, string expected)
    {
        Assert.AreEqual(expected, RecipeOptionValue.LabelPart(input));
    }

    [TestMethod]
    public void FullwidthColonInStepValueStillMatchesTheRule()
    {
        // 步骤值写成全角冒号时，Process Time({3}) 的门控必须照常生效（否则会静默退化成 fail-open）
        var content = new Recipe.Content { Data = [] };
        content.Data!.Add(Row("Process Type", "3：Dry"));
        content.Data.Add(Row("Process Time", "10"));

        RecipeTemplateRegistry.Decorate(content, DryFolder);
        RecipeOperabilityResolver.Recalculate(content);

        Assert.IsTrue(Find(content, "Process Time").Steps[0].IsOperable, "全角冒号应能解析出 3");

        var off = new Recipe.Content { Data = [] };
        off.Data!.Add(Row("Process Type", "1：Dry With Arm"));
        off.Data.Add(Row("Process Time", "10"));

        RecipeTemplateRegistry.Decorate(off, DryFolder);
        RecipeOperabilityResolver.Recalculate(off);

        Assert.IsFalse(Find(off, "Process Time").Steps[0].IsOperable, "1 不在 {3} 内，应灰显");
    }

    [TestMethod]
    public void TemplateOptionsAndDefaultsUseHalfwidthColonsOnly()
    {
        // PLC 侧 GetRealVal 用半角 ':' 拆分选项取值与标签；模板里混入全角冒号会导致下发取不到值。
        // 解析层已兼容全角，但作为「规格源头」的模板仍应保持半角。
        char[] variants = ['\uFF1A', '\uFE55', '\u2236'];   // 全角冒号 / 小冒号 / 比例号
        foreach (var (name, template) in AllTemplates)
        {
            foreach (var row in template)
            {
                foreach (var option in row.Options ?? [])
                {
                    Assert.AreEqual(-1, option.IndexOfAny(variants),
                        $"{name}/{row.Name} 的选项混入了全角冒号：{option}");
                }
                Assert.AreEqual(-1, (row.Default ?? string.Empty).IndexOfAny(variants),
                    $"{name}/{row.Name} 的 Default 混入了全角冒号");
            }
        }
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

    /// <summary>
    /// 构建配方：第 1 行（模式参数：Process Type / Head Sweep Type / Condition Sweep Type）的每一步都取
    /// modeValue，其余参数取模板默认值。
    /// </summary>
    private static Recipe.Content BuildContent(
        ObservableCollection<Recipe.Data> template, string folder, string modeValue, int steps = 1)
    {
        var content = new Recipe.Content { Name = "test", Uuid = "test", Data = [] };

        for (int r = 0; r < template.Count; r++)
        {
            var row = CloneTemplateRow(template[r]);
            for (int i = 0; i < steps; i++)
            {
                row.Steps.Add(new Recipe.Step
                {
                    Id = i + 1,
                    Value = r == 0 ? modeValue : row.Default
                });
            }
            content.Data!.Add(row);
        }

        RecipeTemplateRegistry.Decorate(content, folder);
        RecipeOperabilityResolver.Recalculate(content);
        return content;
    }

    private static Recipe.Content BuildDryContent(string processTypeValue, int steps = 1) =>
        BuildContent(DryRecipeTemplate.Template, DryFolder, processTypeValue, steps);

    private static Recipe.Content BuildPenContent(string processTypeValue, int steps = 1) =>
        BuildContent(PenA1RecipeTemplate.Template, PenFolder, processTypeValue, steps);

    private static Recipe.Content BuildRollContent(string processTypeValue, int steps = 1) =>
        BuildContent(RollRecipeTemplate.Template, RollFolder, processTypeValue, steps);

    private static Recipe.Data Find(Recipe.Content content, string parameter) =>
        content.Data!.First(d =>
            RecipeOperabilityResolver.Normalize(d.Name) == RecipeOperabilityResolver.Normalize(parameter));

    private static bool IsOperable(Recipe.Content content, string parameter, int stepIndex = 0) =>
        Find(content, parameter).Steps[stepIndex].IsOperable;

    private static void SetStepValue(Recipe.Content content, string parameter, string value, int stepIndex = 0)
    {
        Find(content, parameter).Steps[stepIndex].Value = value;
    }

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
        Assert.IsFalse(json.Contains("IsUsed"), json);

        var roundTrip = JsonConvert.DeserializeObject<Recipe.Content>(json);
        Assert.IsNotNull(roundTrip);
        Assert.AreEqual(content.Data!.Count, roundTrip!.Data!.Count);
        Assert.IsTrue(roundTrip.Data[0].Steps[0].IsOperable);   // 反序列化后派生态回到默认 true
    }

    [TestMethod]
    public void UnparsableSourceValueKeepsStepsOperable()
    {
        // 规格：任何"判不出来"的情况一律 fail-open（宁可放开，不可锁死）
        foreach (var raw in new[] { "", "   ", "abc" })
        {
            var content = new Recipe.Content { Data = [] };
            content.Data!.Add(Row("Process Type", raw));
            content.Data.Add(Row("Process Time", "10"));

            RecipeTemplateRegistry.Decorate(content, DryFolder);
            RecipeOperabilityResolver.Recalculate(content);

            var processTime = Find(content, "Process Time");
            Assert.IsTrue(processTime.Steps[0].IsOperable, $"源值 [{raw}] 应 fail-open");
            Assert.IsNull(processTime.Steps[0].OperableReason);
        }
    }

    [TestMethod]
    public void AnyOfRuleIsSatisfiedByAnySingleSubCondition()
    {
        // Pen：WF Chemical Mixed Total Flow / WF Chemical Ratio 的允许条件
        // = "WF Upper Face Chemical Supply=1 或 WF Lower Face Chemical Supply=1"（**或**，不是且）
        var content = BuildPenContent("1:Pen");

        SetStepValue(content, "WF Upper Face Chemical Supply", "0:Disabled");
        SetStepValue(content, "WF Lower Face Chemical Supply", "0:Disabled");
        RecipeOperabilityResolver.Recalculate(content);
        Assert.IsFalse(IsOperable(content, "WF Chemical Mixed Total Flow"), "两边都关");
        Assert.IsFalse(IsOperable(content, "WF Chemical Ratio"), "两边都关");

        SetStepValue(content, "WF Lower Face Chemical Supply", "1:Enabled");
        RecipeOperabilityResolver.Recalculate(content);
        Assert.IsTrue(IsOperable(content, "WF Chemical Mixed Total Flow"), "仅下表面开 -> 应允许");
        Assert.IsTrue(IsOperable(content, "WF Chemical Ratio"), "仅下表面开 -> 应允许");

        SetStepValue(content, "WF Upper Face Chemical Supply", "1:Enabled");
        SetStepValue(content, "WF Lower Face Chemical Supply", "0:Disabled");
        RecipeOperabilityResolver.Recalculate(content);
        Assert.IsTrue(IsOperable(content, "WF Chemical Mixed Total Flow"), "仅上表面开 -> 应允许");
    }

    [TestMethod]
    public void SourceParameterNamesIncludeAnyOfSubRules()
    {
        var content = BuildPenContent("1:Pen");
        var names = RecipeOperabilityResolver.GetSourceParameterNames(content).ToList();

        CollectionAssert.Contains(names, "ProcessType");
        CollectionAssert.Contains(names, "WFUpperFaceChemicalSupply");
        CollectionAssert.Contains(names, "WFLowerFaceChemicalSupply");
    }

    // ---------- Task 3: 模板注册表 ----------

    [DataTestMethod]
    [DataRow(RecipeTemplateRegistry.DryRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.PenRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.RollRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.PolishRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.HeadSweepRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.LoadRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.LoadRetryRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.UnloadRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.UnloadRetryRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.DechuckRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.DechuckRetryRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.InsituConditionRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.ExsituConditionRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.InsituConditionSweepRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.ExsituConditionSweepRecipeFolder)]
    public void ResolveReturnsTemplateForKnownFolder(string folder)
    {
        var template = RecipeTemplateRegistry.Resolve(folder);
        Assert.IsNotNull(template, folder);
        Assert.IsTrue(template!.Count > 0, folder);
    }

    [TestMethod]
    public void ResolveReturnsNullForUnknownFolder()
    {
        Assert.IsNull(RecipeTemplateRegistry.Resolve("No Such Folder"));
        Assert.IsNull(RecipeTemplateRegistry.Resolve(null));
    }

    [TestMethod]
    public void RegisteredFoldersMatchTheDeployedRecipeFolders()
    {
        // 这份清单同时用于「新设备首次运行时创建缺失目录」，必须与现场
        // D:\CMP\Configuration\Recipes\UnitRecipe 下的目录一一对应，且每个都能解析到模板。
        string[] expected =
        [
            "Dry Recipe", "Pen Recipe", "Roll Recipe", "Polish Recipe",
            "Head Sweep Recipe",
            "Load Recipe", "Load Retry Recipe",
            "UnLoad Recipe", "UnLoad Retry Recipe",
            "Dechuck Recipe", "Dechuck Retry Recipe",
            "InsituCondition Recipe", "ExsituCondition Recipe",
            "InsituCondition Sweep Recipe", "ExsituCondition Sweep Recipe",
        ];

        CollectionAssert.AreEquivalent(expected, RecipeTemplateRegistry.RegisteredFolders.ToList());

        foreach (var folder in RecipeTemplateRegistry.RegisteredFolders)
        {
            Assert.IsNotNull(RecipeTemplateRegistry.Resolve(folder), $"未注册模板: {folder}");
        }
    }

    [TestMethod]
    public void EnsureFoldersExistCreatesEveryRegisteredFolderIdempotently()
    {
        var root = Path.Combine(Path.GetTempPath(), "cmp-recipe-folders-" + Guid.NewGuid().ToString("N"));
        try
        {
            var created = RecipeTemplateRegistry.EnsureFoldersExist(root);

            // 首次调用应把全部已注册文件夹建出来
            CollectionAssert.AreEquivalent(RecipeTemplateRegistry.RegisteredFolders.ToList(), created.ToList());
            foreach (var folder in RecipeTemplateRegistry.RegisteredFolders)
            {
                Assert.IsTrue(Directory.Exists(Path.Combine(root, folder)), $"未创建目录: {folder}");
            }

            // 幂等：第二次不应再创建任何目录
            var again = RecipeTemplateRegistry.EnsureFoldersExist(root);
            Assert.AreEqual(0, again.Count, "重复调用不应再创建目录");

            // 空路径安全返回
            Assert.AreEqual(0, RecipeTemplateRegistry.EnsureFoldersExist(null).Count);
            Assert.AreEqual(0, RecipeTemplateRegistry.EnsureFoldersExist("   ").Count);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
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

    [TestMethod]
    public void PenLegacyParameterNamesReceiveRules()
    {
        var content = new Recipe.Content { Data = [] };
        // 现场 Pen A1_For_Recipe_Rev01.json 形态：legacy 名
        content.Data!.Add(Row("Process Type", "5:Rinse"));
        content.Data.Add(Row("Pen Rotate Velocity", "50"));                  // -> Pen Head Rotation Speed {1}
        content.Data.Add(Row("Pen Arm  Sweep Center Velocity", "20"));       // -> Pen Arm  Sweep Center Speed {1}
        content.Data.Add(Row("WF Upper&Lower Face Chemical Supply", "1"));   // -> WF Upper Face Chemical Supply {1,5}
        content.Data.Add(Row("WF Chemical Mixed Total Flow", "1000"));       // AnyOf 两条条件

        RecipeTemplateRegistry.Decorate(content, PenFolder);
        RecipeOperabilityResolver.Recalculate(content);

        var headSpeed = Find(content, "Pen Rotate Velocity");
        Assert.IsNotNull(headSpeed.OperableRule);
        Assert.IsFalse(headSpeed.Steps[0].IsOperable);                       // {1} 不含 5
        Assert.AreEqual("Process Type=1时允许", headSpeed.Steps[0].OperableReason);

        var centerSpeed = Find(content, "Pen Arm  Sweep Center Velocity");
        Assert.IsNotNull(centerSpeed.OperableRule);
        Assert.IsFalse(centerSpeed.Steps[0].IsOperable);

        var chemical = Find(content, "WF Upper&Lower Face Chemical Supply");
        Assert.IsNotNull(chemical.OperableRule);
        Assert.IsTrue(chemical.Steps[0].IsOperable);                         // {1,5} 含 5

        Assert.AreEqual(2, Flatten(Find(content, "WF Chemical Mixed Total Flow").OperableRule!).Count());
    }

    [TestMethod]
    public void RollLegacyParameterNamesReceiveRules()
    {
        var content = new Recipe.Content { Data = [] };
        // 现场 Roll Recipe/*.json 形态：legacy 名
        content.Data!.Add(Row("Process Type", "1: Roll"));
        content.Data.Add(Row("Upper Roller Rotate Velocity", "50"));      // -> Upper Roller Rotate Speed {1}
        content.Data.Add(Row("Lower Roller Rotate Velocity", "50"));      // -> Lower Roller Rotate Speed {1}
        content.Data.Add(Row("Pillar  Rotate Velocity", "50"));           // -> Wafer Rotation {1,5}
        content.Data.Add(Row("Chemical Mixed Total Flowrate", "800"));    // -> Total Diluted Chemical Flow Rate（跨参数）
        content.Data.Add(Row("Chemical Ratio", "8"));                     // -> Chemical Dilution Ratio（跨参数）
        content.Data.Add(Row("Mega Sonic Supply", "1"));                  // 不使用=1，模板中无此行
        content.Data.Add(Row("WF Upper Face Chemical Supply", "1"));      // {1,5}，且是上面两条跨参数规则的来源

        RecipeTemplateRegistry.Decorate(content, RollFolder);
        RecipeOperabilityResolver.Recalculate(content);

        var upperSpeed = Find(content, "Upper Roller Rotate Velocity");
        Assert.IsNotNull(upperSpeed.OperableRule);                         // 修订版新增 {1} 规则
        Assert.IsTrue(upperSpeed.Steps[0].IsOperable);                     // PT=1 允许

        Assert.IsNotNull(Find(content, "Lower Roller Rotate Velocity").OperableRule);
        Assert.IsTrue(Find(content, "Lower Roller Rotate Velocity").Steps[0].IsOperable);   // {1} 含 1
        Assert.IsNotNull(Find(content, "Pillar  Rotate Velocity").OperableRule);
        Assert.IsTrue(Find(content, "Chemical Mixed Total Flowrate").Steps[0].IsOperable);
        Assert.IsTrue(Find(content, "Chemical Ratio").Steps[0].IsOperable);

        Assert.IsNull(Find(content, "Mega Sonic Supply").OperableRule);    // 模板无此行 -> 无规则
        Assert.IsTrue(Find(content, "Mega Sonic Supply").Steps[0].IsOperable);

        // 反向：Process Type=5(Rinse) 时 {1} 组变灰
        var rinse = new Recipe.Content { Data = [] };
        rinse.Data!.Add(Row("Process Type", "5: Rinse"));
        rinse.Data.Add(Row("Lower Roller Rotate Velocity", "50"));
        RecipeTemplateRegistry.Decorate(rinse, RollFolder);
        RecipeOperabilityResolver.Recalculate(rinse);
        Assert.IsFalse(Find(rinse, "Lower Roller Rotate Velocity").Steps[0].IsOperable);
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
        public string? ExcelName { get; set; }
        public int NotUsed { get; set; }
        public string? Reason { get; set; }
        public List<FixtureCondition> Conditions { get; set; } = [];

        // Excel 的 Min. / Max. / 初期值（以字符串承载，兼容空单元格）；仅部分夹具登记
        public string? ExcelMin { get; set; }
        public string? ExcelMax { get; set; }
        public string? ExcelDefault { get; set; }

        /// <summary>该行的 Excel 数值本身有误、未照抄时记录原因；有此标注则跳过数值对账。</summary>
        public string? ValueNote { get; set; }
    }

    private sealed class FixtureCondition
    {
        public string Source { get; set; } = string.Empty;
        public List<string> Allow { get; set; } = [];
    }

    private static Fixture LoadFixture(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);
        Assert.IsTrue(File.Exists(path), $"缺少夹具文件: {path}");
        return JsonConvert.DeserializeObject<Fixture>(File.ReadAllText(path))!;
    }

    /// <summary>把规则展开为「条件列表」：自身条件 + 递归展开的 AnyOf 子条件。</summary>
    private static IEnumerable<(string Source, List<string> Allow)> Flatten(OperableRule rule)
    {
        yield return (rule.SourceParameter, rule.AllowValues.ToList());
        if (rule.AnyOf == null) yield break;

        foreach (var sub in rule.AnyOf)
        {
            if (sub == null) continue;
            foreach (var item in Flatten(sub)) yield return item;
        }
    }

    [TestMethod]
    public void TemplateIdsAndNormalizedNamesAreUnique()
    {
        foreach (var (name, template) in AllTemplates)
        {
            CollectionAssert.AllItemsAreUnique(template.Select(d => d.Id).ToList(), $"{name}: Id 重复");
            CollectionAssert.AllItemsAreUnique(
                template.Select(d => RecipeOperabilityResolver.Normalize(d.Name)).ToList(),
                $"{name}: 归一化参数名重复");
        }
    }

    [TestMethod]
    public void EveryOperableRuleSourceExistsInTheSameTemplate()
    {
        foreach (var (name, template) in AllTemplates)
        {
            var index = template
                .SelectMany(d => new[] { RecipeOperabilityResolver.Normalize(d.Name) }
                    .Concat((d.CompatibleName ?? []).Select(RecipeOperabilityResolver.Normalize)))
                .ToHashSet(StringComparer.Ordinal);

            foreach (var row in template.Where(d => d.OperableRule != null))
            {
                foreach (var (source, _) in Flatten(row.OperableRule!))
                {
                    Assert.IsTrue(index.Contains(RecipeOperabilityResolver.Normalize(source)),
                        $"{name}/{row.Name} 的规则来源 {source} 不在模板中");
                }
            }
        }
    }

    [DataTestMethod]
    [DataRow("Dry Recipe", "dry-operability-fixture.json")]
    [DataRow("Pen Recipe", "pen-operability-fixture.json")]
    [DataRow("Roll Recipe", "roll-operability-fixture.json")]
    [DataRow("Polish Recipe", "polish-operability-fixture.json")]
    [DataRow("Head Sweep Recipe", "head-sweep-operability-fixture.json")]
    [DataRow("Load Recipe", "load-operability-fixture.json")]
    [DataRow("Load Retry Recipe", "load-retry-operability-fixture.json")]
    [DataRow("UnLoad Recipe", "unload-operability-fixture.json")]
    [DataRow("UnLoad Retry Recipe", "unload-retry-operability-fixture.json")]
    [DataRow("Dechuck Recipe", "dechuck-operability-fixture.json")]
    [DataRow("Dechuck Retry Recipe", "dechuck-retry-operability-fixture.json")]
    [DataRow("InsituCondition Recipe", "condition-operability-fixture.json")]
    [DataRow("ExsituCondition Recipe", "exsitu-condition-operability-fixture.json")]
    [DataRow("InsituCondition Sweep Recipe", "condition-sweep-operability-fixture.json")]
    [DataRow("ExsituCondition Sweep Recipe", "exsitu-condition-sweep-operability-fixture.json")]
    public void TemplateRulesMatchExcelFixture(string templateName, string fixtureFile)
    {
        var fixture = LoadFixture(fixtureFile);
        var template = AllTemplates.First(t => t.Name == templateName).Template;
        var templateByKey = template.ToDictionary(
            d => RecipeOperabilityResolver.Normalize(d.Name), d => d);

        // 1) 模板里有规则的每条，必须在夹具中找到同名条目，且 reason 一致
        foreach (var row in template.Where(d => d.OperableRule != null))
        {
            var key = RecipeOperabilityResolver.Normalize(row.Name);
            var spec = fixture.Rows.FirstOrDefault(r =>
                RecipeOperabilityResolver.Normalize(r.Parameter) == key);

            Assert.IsNotNull(spec, $"{fixtureFile}: 夹具缺少参数 {row.Name}");
            Assert.AreEqual(spec.Reason, row.OperableRule!.Reason, $"{templateName}/{row.Name} 的说明原文不一致");

            // 条件列表逐项对账（含 AnyOf 展开）：来源参数 + 允许值
            var actual = Flatten(row.OperableRule).ToList();
            Assert.AreEqual(spec.Conditions.Count, actual.Count, $"{templateName}/{row.Name} 的条件数量不一致");
            for (int i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(spec.Conditions[i].Source, actual[i].Source,
                    $"{templateName}/{row.Name} 第 {i + 1} 个条件的来源不一致");
                CollectionAssert.AreEqual(spec.Conditions[i].Allow, actual[i].Allow,
                    $"{templateName}/{row.Name} 第 {i + 1} 个条件的允许值不一致");
            }
        }

        // 2) 夹具中「启用且带条件」的行，模板必须已实现
        foreach (var spec in fixture.Rows.Where(r => r.NotUsed == 0 && r.Reason != null))
        {
            var key = RecipeOperabilityResolver.Normalize(spec.Parameter);
            Assert.IsTrue(templateByKey.ContainsKey(key), $"{fixtureFile}: 模板缺少参数 {spec.Parameter}");
            Assert.IsNotNull(templateByKey[key].OperableRule, $"{fixtureFile}: {spec.Parameter} 未挂规则");
        }

        // 说明：`不使用=1` 的行由 IsUsed 隐藏（见 TemplateIsUsedMatchesExcelNotUsed），
        // 隐藏行谈不上可操作性，因此这里不再要求它们挂规则。
    }

    [DataTestMethod]
    [DataRow(RecipeTemplateRegistry.DryRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.PenRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.RollRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.PolishRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.HeadSweepRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.LoadRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.LoadRetryRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.UnloadRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.UnloadRetryRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.DechuckRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.DechuckRetryRecipeFolder)]
    // 注意：不含 InsituConditionRecipeFolder —— `Condition Recipe_Polish A` 的显示条件说明全为空，
    // InsituConditionRecipeTemplate 按设计不挂任何规则，由 ConditionTemplatesAreIndependent 覆盖。
    [DataRow(RecipeTemplateRegistry.ExsituConditionRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.InsituConditionSweepRecipeFolder)]
    [DataRow(RecipeTemplateRegistry.ExsituConditionSweepRecipeFolder)]
    public void SingleConditionRulesFollowModeParameter(string folder)
    {
        var template = RecipeTemplateRegistry.Resolve(folder)!;
        var modeRow = template.First();                        // 第 1 行恒为模式参数
        var modeKey = RecipeOperabilityResolver.Normalize(modeRow.Name);
        var selectable = modeRow.Options!.Select(o => RecipeOptionValue.NumericPrefix(o)!).ToArray();

        var modeRules = template.Where(d =>
            d.OperableRule != null &&
            d.OperableRule!.AnyOf == null &&
            RecipeOperabilityResolver.Normalize(d.OperableRule.SourceParameter) == modeKey).ToList();

        Assert.IsTrue(modeRules.Count > 0, $"{folder}: 没有以模式参数 {modeRow.Name} 为来源的单条件规则");

        foreach (var mode in selectable)
        {
            var content = BuildContent(template, folder, $"{mode}: x");
            foreach (var spec in modeRules)
            {
                Assert.AreEqual(spec.OperableRule!.AllowValues.Contains(mode), IsOperable(content, spec.Name),
                    $"{folder}/{spec.Name} @ {modeRow.Name}={mode}");
            }
        }
    }

    [DataTestMethod]
    [DataRow("1: Roll", true)]
    [DataRow("5: Rinse", true)]
    [DataRow("0: Disabled", false)]
    [DataRow("-1: Recipe End", false)]
    public void RollRevisedSpecRulesFollowProcessType(string processType, bool inOneOrFive)
    {
        // Roll A1_For_Recipe_Rev01 (2).xls 修订后：说明列只用 Process Type 的 1/5，
        // 与选项集 {1,5,0,-1} 自洽（原版引用未定义的 2/3/4 的问题已修正）。
        var content = BuildRollContent(processType);

        // {1,5} 组
        Assert.AreEqual(inOneOrFive, IsOperable(content, "Process Time"), processType);
        Assert.AreEqual(inOneOrFive, IsOperable(content, "Wafer Rotation"), processType);
        Assert.AreEqual(inOneOrFive, IsOperable(content, "WF Upper Face DIW Flowrate"), processType);
        Assert.AreEqual(inOneOrFive, IsOperable(content, "WF Lower Face DIW Flowrate"), processType);
        Assert.AreEqual(inOneOrFive, IsOperable(content, "WF Upper Face Chemical Supply"), processType);
        Assert.AreEqual(inOneOrFive, IsOperable(content, "WF Lower Face Chemical Supply"), processType);
        Assert.AreEqual(inOneOrFive, IsOperable(content, "Upper Roll Inner Rinse Supply"), processType);
        Assert.AreEqual(inOneOrFive, IsOperable(content, "Down Roll Inner Rinse Supply"), processType);

        // {1} 组：只有 1: Roll 可编辑
        var isRoll = processType == "1: Roll";
        Assert.AreEqual(isRoll, IsOperable(content, "Upper Roller Rotate Speed"), processType);
        Assert.AreEqual(isRoll, IsOperable(content, "Lower Roller Rotate Speed"), processType);
        Assert.AreEqual(isRoll, IsOperable(content, "Upper Roller Pressure"), processType);
        Assert.AreEqual(isRoll, IsOperable(content, "Lower Roller Pressure"), processType);
    }

    [TestMethod]
    public void RollChemicalFlowRateFollowsUpperFaceChemicalSupply()
    {
        // 修订版：Total Diluted Chemical Flow Rate / Chemical Dilution Ratio
        // 的允许条件由 "Process Type=1" 改为跨参数条件 "WF Upper Face Chemical Supply=1时允许"
        var content = BuildRollContent("1: Roll");

        SetStepValue(content, "WF Upper Face Chemical Supply", "0:Disabled");
        RecipeOperabilityResolver.Recalculate(content);
        Assert.IsFalse(IsOperable(content, "Total Diluted Chemical Flow Rate"), "上表面化学供给关闭");
        Assert.IsFalse(IsOperable(content, "Chemical Dilution Ratio"), "上表面化学供给关闭");
        Assert.AreEqual("WF Upper Face Chemical Supply=1时允许",
            Find(content, "Total Diluted Chemical Flow Rate").Steps[0].OperableReason);

        SetStepValue(content, "WF Upper Face Chemical Supply", "1:Enabled");
        RecipeOperabilityResolver.Recalculate(content);
        Assert.IsTrue(IsOperable(content, "Total Diluted Chemical Flow Rate"), "上表面化学供给开启");
        Assert.IsTrue(IsOperable(content, "Chemical Dilution Ratio"), "上表面化学供给开启");
    }

    // ---------- Polish 工作簿（Rev15 / 显示条件说明列） ----------

    [DataTestMethod]
    [DataRow("1: Fix", false)]
    [DataRow("2: Traverse", true)]
    public void HeadSweepTypeControlsGatedParameters(string mode, bool traverse)
    {
        var content = BuildContent(HeadSweepRecipeTemplate.Template, HeadSweepFolder, mode);

        // 模式参数自身永远可操作
        Assert.IsTrue(IsOperable(content, "Head Sweep Type"), mode);

        // 规则 =1或2 -> 两种模式都可编辑
        Assert.IsTrue(IsOperable(content, "Start Position"), mode);
        Assert.IsTrue(IsOperable(content, "End Position"), mode);

        // 规则 =2 -> 仅 Traverse 可编辑
        Assert.AreEqual(traverse, IsOperable(content, "Zone Count"), mode);
        Assert.AreEqual(traverse, IsOperable(content, "Zone 1 Ratio"), mode);
        Assert.AreEqual(traverse, IsOperable(content, "Zone 20 Ratio"), mode);
        Assert.AreEqual(traverse, IsOperable(content, "Head Sweep Count"), mode);
    }

    [TestMethod]
    public void ConditionSweepParametersFollowTraverse()
    {
        // Condition Sweep Type 只有一个可选取值 2:Traverse，规则全是 =2
        var content = BuildContent(InsituContidionSweepRecipeTemplate.Template, InsituConditionSweepFolder, "2:Traverse");

        Assert.IsTrue(IsOperable(content, "Condition Sweep Type"));
        Assert.IsTrue(IsOperable(content, "Zone Count"));
        Assert.IsTrue(IsOperable(content, "Zone 1 Ratio"));
        Assert.IsTrue(IsOperable(content, "Condition Sweep Count"));
    }

    [DataTestMethod]
    [DataRow("10:Process Load", true)]
    [DataRow("-1:Recipe End", false)]
    public void LoadRecipeParametersFollowProcessType(string mode, bool expected)
    {
        var content = BuildContent(LoadRecipeTemplate.Template, LoadFolder, mode);

        Assert.IsTrue(IsOperable(content, "Process Type"), mode);          // 模式参数自身永远可操作
        Assert.AreEqual(expected, IsOperable(content, "Step Control"), mode);
        Assert.AreEqual(expected, IsOperable(content, "Zone 1 Type"), mode);
        Assert.AreEqual(expected, IsOperable(content, "Retainer Ring Type"), mode);

        // 这两行 Excel 标 不使用=1，但模板存在、界面会显示 -> 按决策也门控
        Assert.AreEqual(expected, IsOperable(content, "Head Middle Wash"), mode);
        Assert.AreEqual(expected, IsOperable(content, "Head External Wash"), mode);
    }

    [TestMethod]
    public void PlatenWashAliasResolvesInAllThreeTemplates()
    {
        // Excel 写 `Platen Wash Selection`，三个模板的行名是 `Plant Wash Selection`，靠新增别名对齐
        foreach (var (template, folder) in new[]
                 {
                     (PolishRecipeTemplate.Template, PolishFolder),
                     (DechuckRecipeTemplate.Template, DechuckFolder),
                     (DechuckRetryRecipeTemplate.Template, DechuckRetryFolder),
                 })
        {
            var content = new Recipe.Content { Data = [] };
            content.Data!.Add(Row("Process Type", "-1: Recipe End"));
            content.Data.Add(Row("Platen Wash Selection", "1"));

            RecipeTemplateRegistry.Decorate(content, folder);
            RecipeOperabilityResolver.Recalculate(content);

            var row = Find(content, "Platen Wash Selection");
            Assert.IsNotNull(row.OperableRule, $"{folder}: Platen Wash Selection 未命中别名");
            Assert.IsFalse(row.Steps[0].IsOperable, $"{folder}: 在 Recipe End 应不可操作");
        }
    }

    [TestMethod]
    public void SweepTemplatesStayInSync()
    {
        // Insitu / Exsitu 两个 Sweep 模板已各自独立（各自文件夹、各自 sheet），
        // 但参数名与规则必须保持一致，否则两边行为会漂移。
        // 若某个 sheet 单独变更，此测试会失败 —— 此时应确认是否真的允许两者分叉。
        var insitu = InsituContidionSweepRecipeTemplate.Template;
        var exsitu = ExsituContidionSweepRecipeTemplate.Template;

        CollectionAssert.AreEqual(
            insitu.Select(d => RecipeOperabilityResolver.Normalize(d.Name)).ToList(),
            exsitu.Select(d => RecipeOperabilityResolver.Normalize(d.Name)).ToList());

        for (int i = 0; i < insitu.Count; i++)
        {
            var a = insitu[i].OperableRule;
            var b = exsitu[i].OperableRule;

            Assert.AreEqual(a == null, b == null, $"第 {i + 1} 行规则存在性不一致");
            if (a == null || b == null) continue;

            Assert.AreEqual(a.SourceParameter, b.SourceParameter, $"第 {i + 1} 行来源不一致");
            Assert.AreEqual(a.Reason, b.Reason, $"第 {i + 1} 行说明不一致");
            CollectionAssert.AreEqual(a.AllowValues.ToList(), b.AllowValues.ToList(), $"第 {i + 1} 行允许值不一致");
        }
    }

    /// <summary>全部夹具（模板名 -> 夹具文件名），供 IsUsed 等逐行对账复用。</summary>
    private static readonly (string Template, string Fixture)[] AllFixtures =
    [
        ("Dry Recipe", "dry-operability-fixture.json"),
        ("Pen Recipe", "pen-operability-fixture.json"),
        ("Roll Recipe", "roll-operability-fixture.json"),
        ("Polish Recipe", "polish-operability-fixture.json"),
        ("Head Sweep Recipe", "head-sweep-operability-fixture.json"),
        ("Load Recipe", "load-operability-fixture.json"),
        ("Load Retry Recipe", "load-retry-operability-fixture.json"),
        ("UnLoad Recipe", "unload-operability-fixture.json"),
        ("UnLoad Retry Recipe", "unload-retry-operability-fixture.json"),
        ("Dechuck Recipe", "dechuck-operability-fixture.json"),
        ("Dechuck Retry Recipe", "dechuck-retry-operability-fixture.json"),
        ("InsituCondition Recipe", "condition-operability-fixture.json"),
        ("ExsituCondition Recipe", "exsitu-condition-operability-fixture.json"),
        ("InsituCondition Sweep Recipe", "condition-sweep-operability-fixture.json"),
        ("ExsituCondition Sweep Recipe", "exsitu-condition-sweep-operability-fixture.json"),
    ];

    /// <summary>
    /// Excel「不使用」列 = 1 的参数行在模板中必须标 <c>IsUsed = false</c>（界面隐藏），
    /// = 0 的必须标 <c>IsUsed = true</c>。**无例外** —— 表格是唯一真源，表格有误应改表格。
    /// </summary>
    [DataTestMethod]
    [DataRow("Dry Recipe", "dry-operability-fixture.json")]
    [DataRow("Pen Recipe", "pen-operability-fixture.json")]
    [DataRow("Roll Recipe", "roll-operability-fixture.json")]
    [DataRow("Polish Recipe", "polish-operability-fixture.json")]
    [DataRow("Head Sweep Recipe", "head-sweep-operability-fixture.json")]
    [DataRow("Load Recipe", "load-operability-fixture.json")]
    [DataRow("Load Retry Recipe", "load-retry-operability-fixture.json")]
    [DataRow("UnLoad Recipe", "unload-operability-fixture.json")]
    [DataRow("UnLoad Retry Recipe", "unload-retry-operability-fixture.json")]
    [DataRow("Dechuck Recipe", "dechuck-operability-fixture.json")]
    [DataRow("Dechuck Retry Recipe", "dechuck-retry-operability-fixture.json")]
    [DataRow("InsituCondition Recipe", "condition-operability-fixture.json")]
    [DataRow("ExsituCondition Recipe", "exsitu-condition-operability-fixture.json")]
    [DataRow("InsituCondition Sweep Recipe", "condition-sweep-operability-fixture.json")]
    [DataRow("ExsituCondition Sweep Recipe", "exsitu-condition-sweep-operability-fixture.json")]
    public void TemplateIsUsedMatchesExcelNotUsed(string templateName, string fixtureFile)
    {
        var fixture = LoadFixture(fixtureFile);
        var template = AllTemplates.First(t => t.Name == templateName).Template;
        var byKey = template.ToDictionary(d => RecipeOperabilityResolver.Normalize(d.Name), d => d);

        var checkedRows = 0;
        foreach (var spec in fixture.Rows)
        {
            if (!byKey.TryGetValue(RecipeOperabilityResolver.Normalize(spec.Parameter), out var row)) continue;

            Assert.AreEqual(spec.NotUsed == 0, row.IsUsed,
                $"{fixtureFile}: {row.Name} 的 IsUsed 与 Excel 不使用={spec.NotUsed} 不一致");
            checkedRows++;
        }

        Assert.IsTrue(checkedRows > 0, $"{templateName}: 未校验到任何参数行");
    }

    [WpfTestMethod]
    public void UnusedRowsAreHiddenFromTheEditorView()
    {
        // `不使用=1` 的行不出现在编辑器的可见集合中；行数据仍保留在 Data 里（照常落盘/下发）
        var content = BuildContent(DryRecipeTemplate.Template, DryFolder, "1: Dry With Arm");
        var hidden = content.Data!.Where(d => !d.IsUsed).ToList();
        Assert.IsTrue(hidden.Count > 0, "Dry 模板应有 不使用=1 的行");

        var view = CollectionViewSource.GetDefaultView(content.Data);
        view.Filter = RecipeRowVisibility.Filter;

        var visible = view.Cast<Recipe.Data>().ToList();
        Assert.AreEqual(content.Data.Count - hidden.Count, visible.Count);
        Assert.IsTrue(visible.All(d => d.IsUsed));
        Assert.IsTrue(visible.Any(d => d.Name == "Process Type"));          // 模式参数可见
        Assert.IsFalse(visible.Any(d => d.Name == "IPA Arm Sweep Center Velocity"));
        Assert.AreEqual(content.Data.Count, content.Data.Count);            // 源集合未被改动
    }

    [TestMethod]
    public void ConditionTemplatesAreIndependent()
    {
        // 两个 sheet 各有独立模板，不再共用：
        //   `Condition Recipe_Polish A`      -> InsituConditionRecipeTemplate（显示条件说明全空 -> 无规则）
        //   `ExsitCondition Recipe_Polish A` -> ExsituConditionRecipeTemplate（15 条，字面允许值 {25}）
        var insitu = InsituConditionRecipeTemplate.Template;
        var exsitu = ExsituConditionRecipeTemplate.Template;

        Assert.AreNotSame(insitu, exsitu);

        var fixture = LoadFixture("condition-operability-fixture.json");
        Assert.IsTrue(fixture.Rows.All(r => r.Reason == null),
            "Condition Recipe sheet 不应有显示条件说明");
        Assert.IsTrue(insitu.All(d => d.OperableRule == null),
            "InsituConditionRecipeTemplate 不应挂任何规则");

        // Exsitu：除模式参数外每一行都必须挂 =25 的规则
        var exsituRules = exsitu.Where(d => d.OperableRule != null).ToList();
        Assert.AreEqual(exsitu.Count - 1, exsituRules.Count, "ExsituConditionRecipeTemplate 规则条数");
        Assert.IsTrue(exsituRules.All(d => d.OperableRule!.SourceParameter == "Process Type"
                                           && d.OperableRule.AllowValues.SequenceEqual(new[] { "25" })
                                           && d.OperableRule.Reason == "Process Type=25时允许"),
            "ExsituConditionRecipeTemplate 的规则应为字面 {25}");
    }

    [DataTestMethod]
    [DataRow("25:Condition", true)]
    [DataRow("-1:Recipe End", false)]
    public void ExsituConditionParametersFollowProcessType(string mode, bool expected)
    {
        var content = BuildContent(ExsituConditionRecipeTemplate.Template, ExsituConditionFolder, mode);

        Assert.IsTrue(IsOperable(content, "Process Type"), mode);       // 模式参数自身永远可操作
        Assert.AreEqual(expected, IsOperable(content, "Process Time"), mode);
        Assert.AreEqual(expected, IsOperable(content, "Conditioner Head Rotation"), mode);
        Assert.AreEqual(expected, IsOperable(content, "Sweep Stop Time Type"), mode);
        Assert.AreEqual(expected, IsOperable(content, "Ratio"), mode);
    }

    [TestMethod]
    public void InsituConditionParametersAreAlwaysOperable()
    {
        // InsituConditionRecipeTemplate 无规则 -> 全部行恒可操作（fail-open 的极端情形）
        foreach (var mode in new[] { "1:Condition", "2:None" })
        {
            var content = BuildContent(InsituConditionRecipeTemplate.Template, ConditionFolder, mode);
            Assert.IsTrue(content.Data!.All(d => d.Steps.All(s => s.IsOperable)), mode);
        }
    }

    [DataTestMethod]
    [DataRow("InsituCondition Recipe", "condition-operability-fixture.json")]
    [DataRow("ExsituCondition Recipe", "exsitu-condition-operability-fixture.json")]
    public void ConditionTemplateNumericConfigMatchesExcel(string templateName, string fixtureFile)
    {
        // 逐行核对 Min / Max / Default 与表格的 Min. / Max. / 初期值。
        // 夹具中带 ValueNote 的行 = Excel 数值本身有误、刻意未照抄，跳过。
        var fixture = LoadFixture(fixtureFile);
        var template = AllTemplates.First(t => t.Name == templateName).Template;
        var byKey = template.ToDictionary(d => RecipeOperabilityResolver.Normalize(d.Name), d => d);

        var checkedRows = 0;
        foreach (var spec in fixture.Rows)
        {
            if (spec.ValueNote != null) continue;
            if (!byKey.TryGetValue(RecipeOperabilityResolver.Normalize(spec.Parameter), out var row)) continue;

            var excelMin = ParseNumber(spec.ExcelMin);
            var excelMax = ParseNumber(spec.ExcelMax);
            var excelDefault = ParseNumber(spec.ExcelDefault);
            if (excelMin == null && excelMax == null && excelDefault == null) continue;

            if (excelMin != null)
                Assert.AreEqual(excelMin.Value, row.Min, 0.0001, $"{templateName}/{row.Name} Min");
            if (excelMax != null)
                Assert.AreEqual(excelMax.Value, row.Max, 0.0001, $"{templateName}/{row.Name} Max");
            if (excelDefault != null)
                Assert.AreEqual(excelDefault.Value, ParseNumber(row.Default) ?? double.NaN, 0.0001,
                    $"{templateName}/{row.Name} Default");

            checkedRows++;
        }

        Assert.IsTrue(checkedRows > 0, $"{templateName}: 未校验到任何参数行");
    }

    private static double? ParseNumber(string? text) =>
        double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : null;

    [TestMethod]
    public void ConditionProcessTypeOptionsMatchTheExcelSheets()
    {
        // 两个 Condition 模板的 Process Type 选项按各自 sheet 的「选择项（英文）」设置
        var insitu = InsituConditionRecipeTemplate.Template.First();
        var exsitu = ExsituConditionRecipeTemplate.Template.First();

        Assert.AreEqual("Process Type", insitu.Name);
        CollectionAssert.AreEqual(new[] { "1:Condition", "2:None" }, insitu.Options);
        Assert.AreEqual("1", insitu.Default);

        Assert.AreEqual("Process Type", exsitu.Name);
        CollectionAssert.AreEqual(new[] { "25:Condition", "-1:Recipe End" }, exsitu.Options);
        Assert.AreEqual("25", exsitu.Default);

        // Default 必须能命中某个选项（OnAddStep 用 StartsWith 匹配），否则新步骤会走异常兜底
        foreach (var row in new[] { insitu, exsitu })
        {
            Assert.IsTrue(row.Options!.Any(o => o.StartsWith(row.Default)),
                $"Default [{row.Default}] 无法匹配任何选项");
        }
    }

    [DataTestMethod]
    [DataRow("64:Pad Search")]
    [DataRow("-1:Recipe End")]
    public void PolishPadSearchGreysEveryGatedParameter(string mode)
    {
        // 固化现状：Polish Recipe 的规则全是 `=1或2`，而选项含 `64:Pad Search`。
        // 因此选 Pad Search（或 Recipe End）时除 Process Type 外整表不可操作。
        // 工艺若认为 Pad Search 下应有可编辑项，需同步修订表格、模板与夹具，本测试会失败以提示。
        var content = BuildContent(PolishRecipeTemplate.Template, PolishFolder, mode);

        Assert.IsTrue(IsOperable(content, "Process Type"), mode);
        Assert.IsFalse(IsOperable(content, "Process Time"), mode);
        Assert.IsFalse(IsOperable(content, "Platen Rotation"), mode);
        Assert.IsFalse(IsOperable(content, "Membrane Height"), mode);
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
