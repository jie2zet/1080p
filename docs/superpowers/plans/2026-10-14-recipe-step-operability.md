# Recipe 参数可操作性（说明列）Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 把 `Dry A_For_Recipe_Rev01.xls`「说明」列转成声明式规则，在 Recipe 编辑器中按 `Process Type` 逐单元格实时灰显/只读不可操作的参数。

**Architecture:** 规则真源放在模板（`DryRecipeTemplate`）→ 载入时 `RecipeTemplateRegistry.Decorate` 按归一化参数名把规则贴回 JSON 行 → `RecipeOperabilityResolver` 求值写入每个 `Step` 的派生态 `IsOperable`/`OperableReason` → 动态列模板与 `CellStyle` 绑定该状态实现 `IsEnabled`/灰底/ToolTip。派生字段全部 `[JsonIgnore]`，不落盘、不改数据、不影响 PLC 下发。

**Tech Stack:** C# 12 / .NET 8 (`net8.0-windows`) / WPF / Prism.Mvvm (`BindableBase`) / Newtonsoft.Json / MSTest 3.8

**Spec:** `docs/superpowers/specs/2026-10-14-recipe-step-operability-design.md`

## Global Constraints

- **构建命令（必须带这些参数，否则 MSBuild 跨工程解析会因环境问题静默失败）：**
  ```
  dotnet build CMP.Tests\CMP.Tests.csproj --no-restore -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false
  ```
- **测试命令：**
  ```
  dotnet test CMP.Tests\CMP.Tests.csproj -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~RecipeOperability" --nologo
  ```
  说明：`--no-restore` 必须保留（本机 NuGet 还原不可用，包已在本地缓存）；`-m:1 -nodeReuse:false` 必须保留（MSBuild 多进程节点通信在当前环境会失败）。
- 基线：`CMP.Tests` 的 `CMP.Tests.Models` 命名空间现有 7 个测试全绿；`CMP` 工程现有约 1008 条警告、0 错误。**不得新增编译错误**。
- 测试框架是 **MSTest**（`[TestClass]` / `[TestMethod]` / `Assert.*` / `CollectionAssert.*`），**不是 xunit**。
- `CMP.Tests.csproj` **未启用** `ImplicitUsings`，测试文件必须写全 `using`。
- `CMP.Tests.csproj` **未启用 `<Nullable>`**。测试文件若要使用 `string?` 等可空注解，**首行必须写 `#nullable enable`**（仓库既有约定，见 `DataLoggingTestSupport.cs:1`），否则会产生 CS8632 警告。`RecipeOperabilityTests.cs` 已加。
- `CMP` 工程启用了 `ImplicitUsings`，且是 **global using** 引入 `Prism.Mvvm`（`BindableBase`）、`Prism.Commands`、`System.Linq` 等。新文件仍建议显式写 `using`，不影响编译。
- 注释用中文，与该仓库既有风格一致。
- **`DryRecipeTemplate.Template` 是 `static` 共享实例**：任何代码（含测试）**不得修改其内容**，测试必须克隆行再改。
- **不改动**：`Recipe` JSON 落盘结构、`DryTabContentViewModel.AssignDryRecipe` 等 PLC 下发路径、`CheckRecipeDefualtValue`、审计逻辑、`RecipeEditorView.xaml`。
- 规则匹配**只按参数名**（归一化：删除 `空格 / _ / - / &`）或 `CompatibleName`，**禁止用 `Id` 或下标**。

---

## File Structure

| 文件 | 职责 |
|---|---|
| `CMP/Models/Recipe/RecipeEdit/RecipeOperability.cs`（新建） | `OperableRule` 记录 + `RecipeOptionValue` 选项前缀解析 |
| `CMP/Models/Recipe/RecipeEdit/RecipeOperabilityResolver.cs`（新建） | 索引建立、规则求值、来源参数收集与匹配 |
| `CMP/Models/Recipe/RecipeEdit/RecipeTemplateRegistry.cs`（新建） | 文件夹名 → 模板；`Decorate` 把规则贴回已载入的 Recipe |
| `CMP/Models/Recipe/RecipeEdit/Recipe.cs`（修改） | `Step.IsOperable` / `Step.OperableReason` / `Data.OperableRule`（均 `[JsonIgnore]`） |
| `CMP/Models/Recipe/RecipeEdit/RecipeParameterTemplate/DryRecipeTemplate.cs`（修改） | 为 13 个参数挂 `OperableRule` |
| `CMP/ViewModels/Tabs/RecipeTab/RecipeEditorViewModel.cs`（修改） | 载入时 `Decorate`；`RefreshOperability` 触发漏斗；来源 Step 订阅；列的 `CellStyle` 灰显触发器；单元格控件 `IsEnabled`+`ToolTip` 绑定 |
| `CMP.Tests/Models/RecipeOperabilityTests.cs`（新建） | 全部 14 项测试 |
| `CMP.Tests/Resources/dry-operability-fixture.json`（新建） | Excel「说明」列对账夹具（24 行） |
| `CMP.Tests/CMP.Tests.csproj`（修改） | 把夹具复制到输出目录 |

---

## Task 1: 选项前缀解析与规则类型

**Files:**
- Create: `CMP/Models/Recipe/RecipeEdit/RecipeOperability.cs`
- Test: `CMP.Tests/Models/RecipeOperabilityTests.cs`

**Interfaces:**
- Consumes: 无
- Produces:
  - `CMP.Models.Recipe.RecipeEdit.OperableRule` — `sealed record OperableRule(string SourceParameter, IReadOnlyList<string> AllowValues, string? Reason = null, IReadOnlyList<OperableRule>? AllOf = null)`
  - `CMP.Models.Recipe.RecipeEdit.RecipeOptionValue.NumericPrefix(string? value) -> string?`

- [ ] **Step 1: 写失败测试**

创建 `CMP.Tests/Models/RecipeOperabilityTests.cs`：

```csharp
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
}
```

- [ ] **Step 2: 运行测试确认失败**

Run:
```
dotnet test CMP.Tests\CMP.Tests.csproj -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~RecipeOperability" --nologo
```
Expected: 编译失败 — `RecipeOptionValue` 不存在（错误 CS0103）。

- [ ] **Step 3: 写最小实现**

创建 `CMP/Models/Recipe/RecipeEdit/RecipeOperability.cs`：

```csharp
using System.Collections.Generic;

namespace CMP.Models.Recipe.RecipeEdit;

/// <summary>
/// 参数可操作性规则：当 <see cref="SourceParameter"/> 在同一个步骤上的取值属于
/// <see cref="AllowValues"/> 时，本参数在该步骤上可操作。
/// 规则真源是 Recipe 规格 Excel 的「说明」列，由模板承载，不落盘。
/// </summary>
/// <param name="SourceParameter">规则来源参数名，例："Process Type"。</param>
/// <param name="AllowValues">允许的选项数字前缀集合，例：["1", "3"]。</param>
/// <param name="Reason">「说明」列原文，仅在不可操作时作为 ToolTip 显示。</param>
/// <param name="AllOf">预留：全部子规则都满足时才可操作（Pen A1 的 AND 组合条件）。本期不参与求值。</param>
public sealed record OperableRule(
    string SourceParameter,
    IReadOnlyList<string> AllowValues,
    string? Reason = null,
    IReadOnlyList<OperableRule>? AllOf = null);

/// <summary>
/// Recipe 选项取值的统一解析工具。
/// 选项在配方中以 "1: Dry With Arm" 或 "1" 两种形态存在，本类只负责取出数字前缀。
/// </summary>
public static class RecipeOptionValue
{
    /// <summary>
    /// 取出选项的数字前缀；无法解析时返回 null。
    /// "1: Dry With Arm" -> "1"；"1" -> "1"；"-1: Recipe End" -> "-1"；"" -> null；"abc" -> null。
    /// </summary>
    public static string? NumericPrefix(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var span = value.AsSpan().Trim();

        int i = 0;
        if (span[0] == '-') i = 1;

        int start = i;
        while (i < span.Length && char.IsDigit(span[i])) i++;

        if (i == start) return null;                          // 没有数字
        if (i < span.Length && span[i] != ':') return null;   // 数字后面既不是 ':' 也不是结尾

        return span[..i].ToString();
    }
}
```

- [ ] **Step 4: 运行测试确认通过**

Run:
```
dotnet test CMP.Tests\CMP.Tests.csproj -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~RecipeOperability" --nologo
```
Expected: PASS，12 个 `NumericPrefixParsesOptionStrings` 用例通过。

- [ ] **Step 5: 提交**

```bash
git add CMP/Models/Recipe/RecipeEdit/RecipeOperability.cs CMP.Tests/Models/RecipeOperabilityTests.cs
git commit -m "feat(recipe): 新增 OperableRule 与选项前缀解析 RecipeOptionValue"
```

---

## Task 2: 派生态与规则求值引擎

**Files:**
- Modify: `CMP/Models/Recipe/RecipeEdit/Recipe.cs:42-78`
- Create: `CMP/Models/Recipe/RecipeEdit/RecipeOperabilityResolver.cs`
- Test: `CMP.Tests/Models/RecipeOperabilityTests.cs`（追加）

**Interfaces:**
- Consumes: `OperableRule`、`RecipeOptionValue`（Task 1）
- Produces:
  - `Recipe.Step.IsOperable` (bool, 默认 true)、`Recipe.Step.OperableReason` (string?)
  - `Recipe.Data.OperableRule` (`OperableRule?`)
  - `RecipeOperabilityResolver.Normalize(string? name) -> string`
  - `RecipeOperabilityResolver.Recalculate(Recipe.Content? content) -> void`
  - `RecipeOperabilityResolver.GetSourceParameterNames(Recipe.Content? content) -> HashSet<string>`
  - `RecipeOperabilityResolver.MatchesAnyName(Recipe.Data? data, IReadOnlySet<string> names) -> bool`

- [ ] **Step 1: 写失败测试（追加到 `RecipeOperabilityTests` 类内）**

先把测试夹具辅助方法加进类里：

```csharp
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
        Options = spec.Options == null ? null : [.. spec.Options],
        CompatibleName = spec.CompatibleName == null ? null : [.. spec.CompatibleName],
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
```

再加测试：

```csharp
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
```

- [ ] **Step 2: 运行测试确认失败**

Run:
```
dotnet test CMP.Tests\CMP.Tests.csproj -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~RecipeOperability" --nologo
```
Expected: 编译失败 — `RecipeOperabilityResolver` / `Recipe.Step.IsOperable` / `Recipe.Data.OperableRule` 不存在。

- [ ] **Step 3: 修改模型**

`CMP/Models/Recipe/RecipeEdit/Recipe.cs`：在 `Data` 类的 `CompatibleName` 之后加入 `OperableRule`：

```csharp
        public List<string>? CompatibleName { get; set; }

        /// <summary>
        /// 参数可操作性规则（规格元数据，来自模板 / Excel「说明」列）。null 表示无限制。
        /// 派生于规格而非数据，不落盘。
        /// </summary>
        [JsonIgnore]
        public OperableRule? OperableRule { get; set; }
```

`CMP/Models/Recipe/RecipeEdit/Recipe.cs`：把 `Step` 类替换为：

```csharp
    public class Step:BindableBase
    {
        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _value = string.Empty;
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private bool _isOperable = true;

        /// <summary>
        /// 当前步骤上该参数是否可操作（派生态，由 RecipeOperabilityResolver 计算）。不落盘。
        /// </summary>
        [JsonIgnore]
        public bool IsOperable
        {
            get => _isOperable;
            set => SetProperty(ref _isOperable, value);
        }

        private string? _operableReason;

        /// <summary>
        /// 不可操作的原因原文（Excel「说明」列），仅 IsOperable 为 false 时有值。不落盘。
        /// </summary>
        [JsonIgnore]
        public string? OperableReason
        {
            get => _operableReason;
            set => SetProperty(ref _operableReason, value);
        }
    }
```

`Newtonsoft.Json` 已在 `Recipe.cs` 顶部导入，`[JsonIgnore]` 可直接使用。

- [ ] **Step 4: 实现求值引擎**

创建 `CMP/Models/Recipe/RecipeEdit/RecipeOperabilityResolver.cs`：

```csharp
using System;
using System.Collections.Generic;

namespace CMP.Models.Recipe.RecipeEdit;

/// <summary>
/// 依据参数上的 <see cref="OperableRule"/> 计算每个步骤的可操作性。
/// 设计约束：只按参数名匹配（归一化 + CompatibleName）；任何异常情况一律 fail-open（视为可操作），
/// 避免一条脏数据把整张表锁成只读。
/// </summary>
public static class RecipeOperabilityResolver
{
    /// <summary>
    /// 重算 Recipe 中所有带规则参数的每一步可操作性。
    /// 结构性变化（载入 / 增删步 / 粘贴 / 列重排）后必须调用。
    /// </summary>
    public static void Recalculate(Recipe.Content? content)
    {
        var dataList = content?.Data;
        if (dataList == null || dataList.Count == 0) return;

        var index = BuildIndex(dataList);

        foreach (var target in dataList)
        {
            var rule = target?.OperableRule;
            if (rule == null) continue;

            if (!index.TryGetValue(Normalize(rule.SourceParameter), out var source) || source.Steps == null)
            {
                SetAll(target!, operable: true, reason: null);
                continue;
            }

            if (target!.Steps == null) continue;

            for (int i = 0; i < target.Steps.Count; i++)
            {
                var step = target.Steps[i];

                if (i >= source.Steps.Count)          // 步数不齐 -> fail-open
                {
                    step.IsOperable = true;
                    step.OperableReason = null;
                    continue;
                }

                var value = RecipeOptionValue.NumericPrefix(source.Steps[i].Value);
                var operable = value != null && rule.AllowValues.Contains(value);

                step.IsOperable = operable;
                step.OperableReason = operable ? null : rule.Reason;
            }
        }
    }

    /// <summary>
    /// 归一化参数名：删除 空格 / _ / - / &amp;，与 PLC 侧 GetRealVal 的规则保持一致。
    /// </summary>
    public static string Normalize(string? name) =>
        (name ?? string.Empty).Replace(" ", "").Replace("_", "").Replace("-", "").Replace("&", "");

    /// <summary>收集当前 Recipe 中所有规则的来源参数名（归一化后），用于订阅变更。</summary>
    public static HashSet<string> GetSourceParameterNames(Recipe.Content? content)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (content?.Data == null) return names;

        foreach (var data in content.Data)
        {
            var rule = data?.OperableRule;
            if (rule == null) continue;

            var key = Normalize(rule.SourceParameter);
            if (key.Length > 0) names.Add(key);
        }
        return names;
    }

    /// <summary>参数名或任一 CompatibleName 命中给定名称集合。</summary>
    public static bool MatchesAnyName(Recipe.Data? data, IReadOnlySet<string> names)
    {
        if (data == null || names == null || names.Count == 0) return false;
        if (names.Contains(Normalize(data.Name))) return true;
        if (data.CompatibleName == null) return false;

        foreach (var alias in data.CompatibleName)
        {
            if (names.Contains(Normalize(alias))) return true;
        }
        return false;
    }

    private static Dictionary<string, Recipe.Data> BuildIndex(IEnumerable<Recipe.Data> dataList)
    {
        var index = new Dictionary<string, Recipe.Data>(StringComparer.Ordinal);

        foreach (var data in dataList)
        {
            if (data == null) continue;

            var key = Normalize(data.Name);
            if (key.Length > 0) index.TryAdd(key, data);

            if (data.CompatibleName == null) continue;
            foreach (var alias in data.CompatibleName)
            {
                var aliasKey = Normalize(alias);
                if (aliasKey.Length > 0) index.TryAdd(aliasKey, data);
            }
        }
        return index;
    }

    private static void SetAll(Recipe.Data target, bool operable, string? reason)
    {
        if (target.Steps == null) return;
        foreach (var step in target.Steps)
        {
            step.IsOperable = operable;
            step.OperableReason = reason;
        }
    }
}
```

**注意：** 上面测试用到 `RecipeTemplateRegistry.Decorate`，它属于 Task 3。为了在本任务内步骤可编译，**先在 Task 2 内创建能通过的占位实现，Task 3 再补全** —— 实际执行时请按此顺序：本 Step 先创建 `RecipeTemplateRegistry.cs` 的最小版本（只有 `Resolve` 返回 `null`、`Decorate` 返回 `false`），使 Task 2 的编译通过；`MissingSourceRow...` / `OutOfRangeSteps...` / `BuildDryContent` 依赖 `Decorate` 的测试在 Task 2 中**允许失败**，Task 3 完成后必须全绿。

- [ ] **Step 5: 运行测试确认通过（Task 3 完成后复跑）**

Run:
```
dotnet test CMP.Tests\CMP.Tests.csproj -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~RecipeOperability" --nologo
```
Expected（Task 3 完成后）：全部 PASS。

- [ ] **Step 6: 提交**

```bash
git add CMP/Models/Recipe/RecipeEdit/Recipe.cs CMP/Models/Recipe/RecipeEdit/RecipeOperabilityResolver.cs CMP/Models/Recipe/RecipeEdit/RecipeTemplateRegistry.cs CMP.Tests/Models/RecipeOperabilityTests.cs
git commit -m "feat(recipe): 新增 Step 可操作派生态与 RecipeOperabilityResolver 求值引擎"
```

---

## Task 3: 模板注册表与元数据挂载

**Files:**
- Modify: `CMP/Models/Recipe/RecipeEdit/RecipeTemplateRegistry.cs`
- Test: `CMP.Tests/Models/RecipeOperabilityTests.cs`（追加）

**Interfaces:**
- Consumes: `RecipeOperabilityResolver.Normalize`、`DryRecipeTemplate.Template`、`Recipe.Data.OperableRule`
- Produces:
  - `RecipeTemplateRegistry.DryRecipeFolder` (`const string` = `"Dry Recipe"`)
  - `RecipeTemplateRegistry.Resolve(string? folderName) -> ObservableCollection<Recipe.Data>?`
  - `RecipeTemplateRegistry.Decorate(Recipe.Content? content, string? folderName) -> bool`

- [ ] **Step 1: 写失败测试（追加到类内）**

```csharp
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
```

- [ ] **Step 2: 运行测试确认失败**

Run:
```
dotnet test CMP.Tests\CMP.Tests.csproj -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~RecipeOperability" --nologo
```
Expected: `ResolveReturnsDryTemplateForDryFolder` FAIL（`Resolve` 返回 null）；`LegacyParameterNamesStillReceiveRulesAfterDecorate` FAIL（`OperableRule` 为 null）。

- [ ] **Step 3: 实现注册表**

用完整实现替换 `CMP/Models/Recipe/RecipeEdit/RecipeTemplateRegistry.cs`：

```csharp
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate;

namespace CMP.Models.Recipe.RecipeEdit;

/// <summary>
/// Recipe 文件夹名 -> 参数模板 的注册表。
/// 本工程的既有约定是「文件夹名 = Recipe 类型」，见 Chamber.ResolveUnitRecipePath
/// 与 SequenceRecipeEditorViewModel 的目录常量。
/// </summary>
public static class RecipeTemplateRegistry
{
    /// <summary>Dry Recipe 文件夹名。</summary>
    public const string DryRecipeFolder = "Dry Recipe";

    private static readonly Dictionary<string, Func<ObservableCollection<Recipe.Data>>> Factories =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [DryRecipeFolder] = () => DryRecipeTemplate.Template,
        };

    /// <summary>取文件夹对应的模板；未注册返回 null（调用方据此 fail-open）。</summary>
    public static ObservableCollection<Recipe.Data>? Resolve(string? folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName)) return null;
        return Factories.TryGetValue(folderName.Trim(), out var factory) ? factory() : null;
    }

    /// <summary>
    /// 把模板上的规格元数据（当前只有 <see cref="Recipe.Data.OperableRule"/>）按归一化参数名 +
    /// CompatibleName 贴到已载入的 Recipe 行上。存量 JSON 使用 legacy 参数名，靠 CompatibleName 命中。
    /// 只读模板，不修改模板内容；未注册的文件夹不做任何事。
    /// </summary>
    /// <returns>是否至少贴中一行。</returns>
    public static bool Decorate(Recipe.Content? content, string? folderName)
    {
        var template = Resolve(folderName);
        if (template == null || content?.Data == null || content.Data.Count == 0) return false;

        var templateIndex = new Dictionary<string, Recipe.Data>(StringComparer.Ordinal);
        foreach (var item in template)
        {
            if (item == null) continue;

            var key = RecipeOperabilityResolver.Normalize(item.Name);
            if (key.Length > 0) templateIndex.TryAdd(key, item);

            if (item.CompatibleName == null) continue;
            foreach (var alias in item.CompatibleName)
            {
                var aliasKey = RecipeOperabilityResolver.Normalize(alias);
                if (aliasKey.Length > 0) templateIndex.TryAdd(aliasKey, item);
            }
        }

        var decorated = false;
        foreach (var row in content.Data)
        {
            if (row == null) continue;
            if (!templateIndex.TryGetValue(RecipeOperabilityResolver.Normalize(row.Name), out var spec)) continue;

            row.OperableRule = spec.OperableRule;
            decorated = true;
        }
        return decorated;
    }
}
```

- [ ] **Step 4: 运行测试确认通过**

Run:
```
dotnet test CMP.Tests\CMP.Tests.csproj -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~RecipeOperability" --nologo
```
Expected: `ResolveReturnsDryTemplateForDryFolder` 转绿。**真值表测试与 `LegacyParameterNames...` 此时仍 FAIL** —— 因为 `DryRecipeTemplate` 还没挂规则（Task 4 才有），Task 4 完成后必须全绿。

- [ ] **Step 5: 提交**

```bash
git add CMP/Models/Recipe/RecipeEdit/RecipeTemplateRegistry.cs CMP.Tests/Models/RecipeOperabilityTests.cs
git commit -m "feat(recipe): 新增 RecipeTemplateRegistry 与 Decorate 元数据挂载"
```

---

## Task 4: Dry 规则表与 Excel 对账夹具

**Files:**
- Modify: `CMP/Models/Recipe/RecipeEdit/RecipeParameterTemplate/DryRecipeTemplate.cs`
- Create: `CMP.Tests/Resources/dry-operability-fixture.json`
- Modify: `CMP.Tests/CMP.Tests.csproj`
- Test: `CMP.Tests/Models/RecipeOperabilityTests.cs`（追加）

**Interfaces:**
- Consumes: `OperableRule`、`RecipeOperabilityResolver.Normalize`、`DryRecipeTemplate.Template`
- Produces: `DryRecipeTemplate.Template` 中 13 个参数挂上 `OperableRule`

- [ ] **Step 1: 创建 Excel 对账夹具**

创建 `CMP.Tests/Resources/dry-operability-fixture.json`（数据来自 `Dry A_For_Recipe_Rev01.xls` / sheet `Recipe_Dry A` / AG 列「说明」）：

```json
{
  "recipeType": "Dry Recipe",
  "source": "Dry A_For_Recipe_Rev01.xls / Recipe_Dry A / AG 列(说明)",
  "rows": [
    { "index": 1, "parameter": "Process Type", "notUsed": 0, "reason": null },
    { "index": 2, "parameter": "Reserved", "notUsed": 1, "reason": null },
    { "index": 3, "parameter": "Process Time", "notUsed": 0, "reason": "Process Type=3时允许" },
    { "index": 4, "parameter": "Wafer Rotation Speed", "notUsed": 0, "reason": "Process Type=1或3时允许" },
    { "index": 5, "parameter": "IPA Arm  Sweep Start Position", "notUsed": 0, "reason": "Process Type=1时允许" },
    { "index": 6, "parameter": "Reserved", "notUsed": 1, "reason": null },
    { "index": 7, "parameter": "IPA Arm  Sweep End Position", "notUsed": 0, "reason": "Process Type=1时允许" },
    { "index": 8, "parameter": "Reserved", "notUsed": 1, "reason": null },
    { "index": 9, "parameter": "IPA Arm  Sweep Center Velocity", "notUsed": 1, "reason": "Process Type=2时允许" },
    { "index": 10, "parameter": "IPA Arm  Sweep Edge Velocity", "notUsed": 1, "reason": "Process Type=2时允许" },
    { "index": 11, "parameter": "IPA Arm  Sweep Speed", "notUsed": 0, "reason": "Process Type=1时允许" },
    { "index": 12, "parameter": "IPA Arm  Cycle", "notUsed": 0, "reason": "Process Type=1时允许" },
    { "index": 13, "parameter": "Reserved", "notUsed": 1, "reason": "Process Type=2时允许" },
    { "index": 14, "parameter": "Reserved", "notUsed": 1, "reason": "Process Type=2时允许" },
    { "index": 15, "parameter": "Reserved", "notUsed": 1, "reason": "Process Type=2时允许" },
    { "index": 16, "parameter": "Reserved", "notUsed": 1, "reason": "Process Type=2时允许" },
    { "index": 17, "parameter": "Reserved", "notUsed": 1, "reason": "Process Type=2时允许" },
    { "index": 18, "parameter": "Dilution N2 Gas Flow", "notUsed": 0, "reason": "Process Type=1或3时允许" },
    { "index": 19, "parameter": "N2 For WF Backside Flow", "notUsed": 0, "reason": "Process Type=1或3时允许" },
    { "index": 20, "parameter": "Outer Rinse Flow", "notUsed": 0, "reason": "Process Type=1或3时允许" },
    { "index": 21, "parameter": "Bubbling N2 Gas Flow", "notUsed": 0, "reason": "Process Type=1或3时允许" },
    { "index": 22, "parameter": "Upper Rinse Supply", "notUsed": 0, "reason": "Process Type=1或3时允许" },
    { "index": 23, "parameter": "Lower Rinse Supply", "notUsed": 0, "reason": "Process Type=1或3时允许" },
    { "index": 24, "parameter": "Wafer Rotation  Acceleration", "notUsed": 0, "reason": "Process Type=1或3时允许" }
  ]
}
```

在 `CMP.Tests/CMP.Tests.csproj` 的 `<ItemGroup>`（`PackageReference` 那一组）之后加入：

```xml
  <ItemGroup>
    <Content Include="Resources\dry-operability-fixture.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
```

- [ ] **Step 2: 写失败测试（追加到类内）**

```csharp
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
```

- [ ] **Step 3: 运行测试确认失败**

Run:
```
dotnet test CMP.Tests\CMP.Tests.csproj -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~RecipeOperability" --nologo
```
Expected: `DryTemplateRulesMatchExcelFixture` FAIL（模板尚未挂规则，第 2 段断言报"未挂规则"）；真值表测试仍 FAIL。

- [ ] **Step 4: 给模板挂规则**

编辑 `CMP/Models/Recipe/RecipeEdit/RecipeParameterTemplate/DryRecipeTemplate.cs`：在顶部 `using` 区加入

```csharp
using CMP.Models.Recipe.RecipeEdit;
```

（该文件命名空间是 `CMP.Models.Recipe.RecipeEdit.RecipeParameterTemplate`，父命名空间成员默认可见，此 using 可省；写上更明确。）

然后为下列 13 个 `new()` 初始化器各加一行 `OperableRule = ...`（放在 `Steps = []` 之前，逗号结尾）：

| 参数（`Name = ` 的值） | 追加的成员 |
|---|---|
| `"Process Time"` | `OperableRule = new OperableRule("Process Type", ["3"], "Process Type=3时允许"),` |
| `"Wafer Rotation Speed"` | `OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),` |
| `"IPA Arm  Sweep Start Position"` | `OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),` |
| `"IPA Arm  Sweep End Position"` | `OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),` |
| `"IPA Arm Sweep Speed"` | `OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),` |
| `"IPA Arm Cycle"` | `OperableRule = new OperableRule("Process Type", ["1"], "Process Type=1时允许"),` |
| `"Dilution N2 Gas Flow"` | `OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),` |
| `"N2 For WF Backside Flow"` | `OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),` |
| `"Outer Rinse Flow"` | `OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),` |
| `"Bubbling N2 Gas Flow"` | `OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),` |
| `"Upper Rinse Supply"` | `OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),` |
| `"Lower Rinse Supply"` | `OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),` |
| `"Wafer Rotation Acceleration"` | `OperableRule = new OperableRule("Process Type", ["1", "3"], "Process Type=1或3时允许"),` |

不加规则的保持原样：`Process Type`（说明为空 → 永远可操作）。

示例（`Process Time` 那一段改造后）：

```csharp
            new()
          {
            Id = 2,
            Name = "Process Time",
            Unit = "s",
            Type = "float",
            Min = 0,
            Max = 9999,
            Default = "0",
            Options = [],
            OperableRule = new OperableRule("Process Type", ["3"], "Process Type=3时允许"),
            Steps = []
          },
```

- [ ] **Step 5: 运行测试确认通过**

Run:
```
dotnet test CMP.Tests\CMP.Tests.csproj -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~RecipeOperability" --nologo
```
Expected: 全部 PASS（含 Task 2、Task 3 遗留用例）。

- [ ] **Step 6: 提交**

```bash
git add CMP/Models/Recipe/RecipeEdit/RecipeParameterTemplate/DryRecipeTemplate.cs CMP.Tests/Resources/dry-operability-fixture.json CMP.Tests/CMP.Tests.csproj CMP.Tests/Models/RecipeOperabilityTests.cs
git commit -m "feat(recipe): Dry 模板挂载 13 条可操作性规则并加入 Excel 对账夹具"
```

---

## Task 5: 编辑器接线（Decorate / 重算 / 灰显 / 只读 / ToolTip）

**Files:**
- Modify: `CMP/ViewModels/Tabs/RecipeTab/RecipeEditorViewModel.cs`

**Interfaces:**
- Consumes: `RecipeTemplateRegistry.Decorate`、`RecipeOperabilityResolver.Recalculate`、`RecipeOperabilityResolver.GetSourceParameterNames`、`RecipeOperabilityResolver.MatchesAnyName`、`Recipe.Step.IsOperable/OperableReason`
- Produces: 无对外新接口（纯 UI 行为）

- [ ] **Step 1: 加 using 与静态画刷**

在 `RecipeEditorViewModel.cs` 顶部 using 区加入：

```csharp
using System.ComponentModel;
```

在类内字段区（`private bool newItemVisible;` 之前）加入：

```csharp
    /// <summary>不可操作单元格的背景色。</summary>
    private static readonly System.Windows.Media.Brush DisabledCellBackground =
        new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF0, 0xF0, 0xF0));

    /// <summary>不可操作单元格的前景色。</summary>
    private static readonly System.Windows.Media.Brush DisabledCellForeground =
        System.Windows.Media.Brushes.Gray;

    /// <summary>已订阅 Value 变更的规则来源 Step（Process Type 等），用于退订与状态重算。</summary>
    private readonly List<Recipe.Step> _operabilitySourceSteps = [];
```

> 说明：本文件已 `using System.Drawing;`，`Color` 有歧义，故画刷全部用完全限定名。

- [ ] **Step 2: 加可操作性方法**

在 `RefreshEditor` 方法之前插入：

```csharp
    /// <summary>
    /// 重算每一步参数的可操作性，并重建规则来源 Step 的变更订阅。
    /// 所有结构性变化（载入 / 增删步 / 粘贴 / 列重排）都必须经过这里。
    /// </summary>
    private void RefreshOperability()
    {
        RecipeOperabilityResolver.Recalculate(RecipeData);
        AttachOperabilityTracking();
    }

    /// <summary>
    /// 订阅规则来源参数（如 Process Type）每个 Step 的 Value 变更，实现"改 Process Type 立即灰显"。
    /// 每次调用先退订，避免重复订阅与旧对象滞留（本方法是唯一订阅入口）。
    /// </summary>
    private void AttachOperabilityTracking()
    {
        DetachOperabilityTracking();

        var sourceNames = RecipeOperabilityResolver.GetSourceParameterNames(RecipeData);
        if (sourceNames.Count == 0 || RecipeData?.Data == null) return;

        foreach (var data in RecipeData.Data)
        {
            if (data?.Steps == null) continue;
            if (!RecipeOperabilityResolver.MatchesAnyName(data, sourceNames)) continue;

            foreach (var step in data.Steps)
            {
                step.PropertyChanged += OnOperabilitySourceStepChanged;
                _operabilitySourceSteps.Add(step);
            }
        }
    }

    private void DetachOperabilityTracking()
    {
        foreach (var step in _operabilitySourceSteps)
        {
            step.PropertyChanged -= OnOperabilitySourceStepChanged;
        }
        _operabilitySourceSteps.Clear();
    }

    private void OnOperabilitySourceStepChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Recipe.Step.Value)) return;
        RecipeOperabilityResolver.Recalculate(RecipeData);
    }
```

- [ ] **Step 3: 把 RefreshOperability 接进 RefreshEditor 漏斗**

在 `RefreshEditor()` 的空值守卫之后、`var steps = ...` 之前插入一行：

```csharp
        RefreshOperability();
```

改造后的开头应为：

```csharp
    public void RefreshEditor()
    {
        if (RecipeData == null || RecipeData!.Data!.Count == 0)
            return;

        // 列结构即将重建，先按当前数据重算可操作性并重建订阅
        RefreshOperability();

        var steps = RecipeData!.Data![0].Steps;
```

- [ ] **Step 4: 载入 Recipe 时贴规则**

在 `OnTreeViewNodeChanged` 中，`foreach (var data in RecipeData.Data) { ... }` 修正选项的循环结束之后、`RefreshEditor();` 之前插入：

```csharp
                // 按文件夹名把模板上的可操作性规则贴回（含 legacy 参数名经 CompatibleName 命中）
                RecipeTemplateRegistry.Decorate(RecipeData, unit.Name);
```

改造后应为：

```csharp
                foreach (var data in RecipeData.Data)
                {
                    ...（原有 Algorithm / Model / 子配方选项处理）...
                }

                // 按文件夹名把模板上的可操作性规则贴回（含 legacy 参数名经 CompatibleName 命中）
                RecipeTemplateRegistry.Decorate(RecipeData, unit.Name);

                RefreshEditor();
                return;
```

- [ ] **Step 5: 保存前重算**

在 `OnSaveRecipe` 中 `CheckRecipeDefualtValue();` 之前插入：

```csharp
        // 保证界面状态与待落盘数据一致
        RefreshOperability();
```

- [ ] **Step 6: 给步骤列加灰显触发器**

把 `RefreshEditor` 中的建列循环替换为：

```csharp
        for (int stepIndex = 0; stepIndex < steps.Count; stepIndex++)
        {
            var step = steps[stepIndex];

            var cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));

            // 该行该步不可操作时灰显（步骤下标只有此处可知，因此在列上构造触发器）
            var notOperableTrigger = new DataTrigger
            {
                Binding = new Binding($"Steps[{stepIndex}].IsOperable"),
                Value = false
            };
            notOperableTrigger.Setters.Add(new Setter(Control.BackgroundProperty, DisabledCellBackground));
            notOperableTrigger.Setters.Add(new Setter(Control.ForegroundProperty, DisabledCellForeground));
            cellStyle.Triggers.Add(notOperableTrigger);

            // Create a template column for each step
            var templateColumn = new DataGridTemplateColumn
            {
                Header = $"Step {step.Id}",
                CellTemplateSelector = new RecipeCellTemplateSelector(stepIndex),
                CellEditingTemplateSelector = new RecipeCellTemplateSelector(stepIndex),
                CellStyle = cellStyle
            };
            _dataGrid.Columns.Add(templateColumn);
        }
```

- [ ] **Step 7: 单元格控件绑定 IsEnabled 与 ToolTip**

在 `RecipeCellTemplateSelector` 类中，`SelectTemplate` 之前加入辅助方法：

```csharp
    /// <summary>
    /// 统一的编辑控件装饰：可操作性绑定。
    /// IsEnabled=false 让灰显单元格无法编辑；ToolTip 显示 Excel「说明」原文作为原因。
    /// </summary>
    private static void ApplyOperabilityBindings(FrameworkElementFactory factory, int stepIndex)
    {
        factory.SetBinding(UIElement.IsEnabledProperty,
            new Binding($"Steps[{stepIndex}].IsOperable"));
        factory.SetBinding(FrameworkElement.ToolTipProperty,
            new Binding($"Steps[{stepIndex}].OperableReason"));
    }
```

在 `SelectTemplate` 的三个分支中，各自 `var factory = new FrameworkElementFactory(...)` 之后、`return template;` 之前，插入同一行调用：

```csharp
            ApplyOperabilityBindings(factory, _stepIndex);
```

三个分支分别是：`option/list` 的 `ComboBox`、`int/float` 的 `NumericUpDown`、`else` 的 `TextBox`。共 **3 处**。

- [ ] **Step 8: 构建验证**

Run:
```
dotnet build CMP.Tests\CMP.Tests.csproj --no-restore -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false
```
Expected: `0 个错误`，且 `CMP.csproj` 的警告数量不高于基线（1008 条）。

- [ ] **Step 9: 回归测试**

Run:
```
dotnet test CMP.Tests\CMP.Tests.csproj -m:1 -nodeReuse:false -p:MSBuildEnableWorkloadResolver=false --filter "FullyQualifiedName~CMP.Tests.Models" --nologo
```
Expected: 原有 7 个测试 + 新增全部 RecipeOperability 测试，**0 失败**。

- [ ] **Step 10: 手工 UI 验收（需在装有现场数据的机器上执行）**

按 spec §10 逐条勾选：

1. 打开 `Dry Recipe` 下任一配方，把某一步 `Process Type` 改成 `3: Dry` → 该步 `Process Time`/流量的单元格可编辑，`IPA Arm Sweep Start Position`/`IPA Arm Sweep Speed`/`IPA Arm Cycle` 变灰；
2. 改成 `1: Dry With Arm` → 上述 IPA Arm 参数恢复可编辑，`Process Time` 变灰；
3. 灰显单元格鼠标悬停显示 `Process Type=1时允许`；
4. 灰显单元格无法通过鼠标、Tab、方向键、滚轮修改；
5. 列重排、加步、删步、粘贴、另存之后状态依然正确；
6. 保存后 JSON diff 只有步骤值，不含 `IsOperable` / `OperableReason` / `OperableRule`；
7. 载入 `Dry Recipe\111.json`（legacy 名）后，`Chuck Rotate Velocity`、`WF Upper Face N2 Flow` 等行按规则灰显。

- [ ] **Step 11: 提交**

```bash
git add CMP/ViewModels/Tabs/RecipeTab/RecipeEditorViewModel.cs
git commit -m "feat(recipe): 编辑器按 Process Type 实时灰显/只读不可操作参数

- 载入时 Decorate 贴回规则，RefreshEditor 作为重算漏斗
- 订阅规则来源 Step 的 Value 变更实现实时刷新
- 步骤列加 CellStyle 灰显触发器，单元格控件绑定 IsEnabled 与 ToolTip
- 不改动任何步骤值、JSON 结构与 PLC 下发路径"
```

---

## Self-Review

**Spec 覆盖核对**

| Spec 章节 | 落地任务 |
|---|---|
| §5 Dry 13 条真值表 | Task 4（模板规则）+ Task 4 夹具对账测试 |
| §5 `Process Type` 永远可操作 | Task 2 `ProcessTypeItselfIsAlwaysOperable` |
| §5 PT=0/-1 直译 | Task 2 真值表测试（`AllProcessTypes` 含 0、-1） |
| §6.1 模型派生态 | Task 2 Step 3 |
| §6.2 `OperableRule` + `RecipeOptionValue` | Task 1 |
| §6.3 求值引擎 + fail-open | Task 2 Step 4 + `OutOfRangeSteps...` / `MissingSourceRow...` |
| §6.4 `RecipeTemplateRegistry` + `Decorate` | Task 3 |
| §6.5 触发时机 6 项 | Task 5：载入(Step 4)、改 PT(Step 2)、增删粘贴步(经 `RefreshEditor`/`ReFillStepData` 漏斗 Step 3)、列重排(`OnColumnReordered` 调 `RefreshEditor`)、保存前(Step 5) |
| §6.6 视图层 | Task 5 Step 6/7 |
| §6.7 不改动部分 | Global Constraints 明列；Task 5 Step 11 commit message 复述 |
| §7 不变式 1（不落盘） | Task 2 `DerivedOperabilityFieldsAreNeverSerialized` |
| §7 不变式 2（只按名匹配） | Task 2 `Normalize`/`BuildIndex`；Task 3 legacy 测试 |
| §7 不变式 3（fail-open） | Task 2 两个专项测试 |
| §7 不变式 6（模板=Excel） | Task 4 `DryTemplateRulesMatchExcelFixture` |
| §9 测试 1–14 | 全部落在 `RecipeOperabilityTests` |
| §10 手工验收 | Task 5 Step 10 |

**已知的规格收窄（有意为之，非遗漏）**

1. spec §6.2 提到 `RecipeOptionValue` 同时替换 Operation 侧 `GetRealVal` 的 `":"` 解析。本计划**不做**：`GetRealVal` 对 string 目标取 `arr[1]`（要标签）、对数值目标取 `arr[0]`，语义不同，且该路径属于 PLC 下发链路——与「一期不动下发路径」的决策冲突。统一留作独立任务。
2. spec §9 夹具名为 `CMP.Tests/Resources/dry-operability-fixture.json` ✅ 一致；夹具内含 Excel 全部 24 行（含 `不使用=1`），由此覆盖 spec §5「预留项仍登记入表」的意图，**不新增无人消费的生产代码**。
3. spec §6.5 建议用 `WeakEventManager`。本计划改用**显式强订阅 + 唯一入口先退订**：订阅只在 `AttachOperabilityTracking` 一处建立、进入即先 `Detach`，VM 是唯一监听者，不存在泄漏路径；行为比弱事件更可预测。

**类型一致性核对**

- `OperableRule(string, IReadOnlyList<string>, string?, IReadOnlyList<OperableRule>?)` — Task 1 定义，Task 4 使用 `new OperableRule("Process Type", ["3"], "...")` ✅（第三参为 `Reason`）
- `RecipeOperabilityResolver.Normalize(string?) -> string` — Task 2 定义，Task 3/4 使用 ✅
- `RecipeTemplateRegistry.Decorate(Recipe.Content?, string?) -> bool` — Task 3 定义，Task 2 测试与 Task 5 VM 使用 ✅
- `RecipeTemplateRegistry.DryRecipeFolder` — Task 3 定义，Task 4 fixture 的 `recipeType` 与之同值 `"Dry Recipe"` ✅
- `Recipe.Step.IsOperable` / `OperableReason` — Task 2 定义，Task 5 绑定路径 `Steps[{i}].IsOperable` / `.OperableReason` 一致 ✅
- `ApplyOperabilityBindings(FrameworkElementFactory, int)` — Task 5 Step 7 定义并在 3 个分支调用 ✅

---

## 执行记录（实际执行时的偏差）

| # | 偏差 | 原因 |
|---|---|---|
| 1 | 测试文件首行加 `#nullable enable` | `CMP.Tests.csproj` 未启用 `<Nullable>`，用 `string?` 会产生 CS8632；仓库既有约定见 `DataLoggingTestSupport.cs:1` |
| 2 | Task 5 Step 6/7 的灰显样式与绑定工厂**抽到新文件** `CMP/Views/Tabs/RecipeTab/RecipeStepCellStyle.cs`（`CreateCellStyle` / `CreateIsOperableBinding` / `CreateOperableReasonBinding`） | 计划把样式内联在 `RefreshEditor` 里，而 Task 5 的唯一验证是**手工 UI 验收**——绑定路径 `Steps[i].IsOperable` 是整个改动最易写错的一环，却无法自动验证。抽出后可用 2 个 `[WpfTestMethod]` 断言绑定路径、触发器条件与画刷，把"只能靠人眼"变成自动化证据 |
| 3 | 新增 2 个视图接线测试（`StepCellStyleGreysOutOnlyWhenNotOperable`、`OperabilityBindingsPointAtTheSameStepIndex`） | 同上 |
| 4 | `CloneTemplateRow` 用 `new List<string>(...)` 而非 `[.. spec.Options]` | 目标类型化条件表达式 + 集合表达式的组合写法有编译风险，显式构造更稳 |

**执行结果**：Task 1–5 全部完成，`RecipeOperabilityTests` **31 个测试全绿**；`CMP.Tests.Models` 命名空间 **38/38 全绿**（7 个原有 + 31 个新增）；`CMP` 工程 0 错误、警告数 **1008 = 基线**（无新增）。

**仍未完成**：Task 5 Step 10 的手工 UI 验收（需要在装有现场数据与 HMI 的机器上运行 WPF 程序逐条勾选 spec §10）。
