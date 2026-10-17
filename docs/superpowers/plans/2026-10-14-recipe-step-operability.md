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

---

## Rev02 增补：Pen / Roll 落地（本次未提交 Git，仅工作区变更）

按工艺要求扩展 Pen A1 与 Roll A1。**两处对 Rev01 的纠正**（详见 spec §0 修订记录）：

1. **`OperableRule.AllOf` → `AnyOf`**：Pen A1 的跨参数条件是 Excel 原文的 **`或`**（`WF Upper Face Chemical Supply=1或 WF Lower Face Chemical Supply=1时允许`），Rev01 误读为「且」。因 `AllOf` 无调用点，改名 + 实现或分支为纯增量。
2. **源值无法解析时改为 fail-open**：Rev01 文字与伪代码矛盾，现已统一（`""` / 空白 / `"abc"` → 视为可操作）。

| 变更项 | 内容 |
|---|---|
| 模型 | `RecipeOperability.cs`：`AllOf` → `AnyOf`（或语义） |
| 求值器 | `RecipeOperabilityResolver.cs`：重写为 `IsSatisfied` 递归（自身条件 OR AnyOf 子条件）+ 条件级 fail-open；`GetSourceParameterNames` 递归收集 AnyOf 来源（否则改化学供给不会触发实时刷新） |
| 注册表 | `RecipeTemplateRegistry.cs`：新增 `PenRecipeFolder`（`Pen Recipe` → `PenA1RecipeTemplate`）、`RollRecipeFolder`（`Roll Recipe` → `RollRecipeTemplate`） |
| 模板 | `PenA1RecipeTemplate.cs` +15 条规则（13 单条件 + 2 AnyOf）；`RollRecipeTemplate.cs` +13 条规则（按字面，含冲突项） |
| 夹具 | 新增 `pen-operability-fixture.json`（28 行）、`roll-operability-fixture.json`（19 行）；三份夹具统一新增 `conditions`（来源 + 允许值）字段 |
| 测试 | 新增 AnyOf / fail-open / AnyOf 来源 / 单条件×可选取值 / Roll 冲突固化 / Pen·Roll legacy 映射；5 个测试改为 `[DataTestMethod]` 跨三个模板 |

**Roll 规格冲突（已知且已固化）**：Roll 说明列引用了同表 `Process Type` 行未定义的取值 `2/3/4`。按工艺决定「先按字面实现」，后果是 `Process Time`（`{3,4}`）与两个 `WF * Face DIW Flowrate`（`{2}`）在可选值 `{1,5,0,-1}` 下**永久不可操作**。已用 `RollProcessTimeAndDiwFlowrateAreNeverOperableWithCurrentOptions` 固化现状 —— 表格修订后该测试会失败，即提示同步更新模板与夹具。

**验证结果（Rev02）**：

| 项 | 结果 |
|---|---|
| `RecipeOperabilityTests` | **52/52 全绿**（Rev01 为 31） |
| `CMP.Tests.Models` | **59/59 全绿**（7 原有 + 52） |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线**（Pen/Roll 未引入新警告） |
| Git | **未提交**（按用户要求，全部变更留在工作区） |

---

## Rev03 增补：Roll 表格修订版落地（依据 `Roll A1_For_Recipe_Rev01 (2).xls`，仍未提交 Git）

逐单元格对比新旧两份 Roll Excel，差异**只有 `Recipe_Roll A1` 表 AG 列 13 处**（参数名、顺序、Min/Max、`不使用` 标记、`Process Type` 选项集均未变）。修订后说明列只用 `1`/`5`（顿号分隔枚举值），Rev02 记录的规格冲突**已消除**。

| Excel 行 | 参数 | Rev02（旧） | Rev03（新） |
|---|---|---|---|
| 11 (idx3) | Process Time | `Process Type=3或4` | **`Process Type=1、5`** |
| 12 (idx4) | Upper Roller Rotate Speed | *(空)* | **`Process Type=1`** ← 新增规则 |
| 14 (idx6) | Wafer Rotation | `=1` | **`=1、5`** |
| 15 (idx7) | Reserved（不使用） | `=1` | *(空)* |
| 18 (idx10) | WF Upper Face DIW Flowrate | `=2` | **`=1、5`** |
| 19 (idx11) | WF Lower Face DIW Flowrate | `=2` | **`=1、5`** |
| 20 (idx12) | WF Upper Face Chemical Supply | `=1或2或3` | **`=1、5`** |
| 21 (idx13) | WF Lower Face Chemical Supply | `=1或2或3` | **`=1、5`** |
| 22 (idx14) | Mega Sonic Supply（不使用） | `=1或2或3` | *(空)* |
| 24 (idx16) | Total Diluted Chemical Flow Rate | `=1` | **`WF Upper Face Chemical Supply=1`** ← 改跨参数 |
| 25 (idx17) | Chemical Dilution Ratio | `=1` | **`WF Upper Face Chemical Supply=1`** |
| 26 (idx18) | Upper Roll Inner Rinse Supply | `=1` | **`=1、5`** |
| 27 (idx19) | Down Roll Inner Rinse Supply | `=1` | **`=1、5`** |

| 变更项 | 内容 |
|---|---|
| 模板 | `RollRecipeTemplate.cs`：清掉旧规则后按修订版重挂 **14 条**（8×`{1,5}` + 4×`{1}` + 2×跨参数） |
| 夹具 | `roll-operability-fixture.json` 按修订版重写，`source` 指向 `(2).xls`，`note` 记录冲突已修正 |
| 测试 | 删除 `RollProcessTimeAndDiwFlowrateAreNeverOperableWithCurrentOptions`（冲突消除后不再成立）→ 替换为 `RollRevisedSpecRulesFollowProcessType`（4 组正反断言）；新增 `RollChemicalFlowRateFollowsUpperFaceChemicalSupply`；修正 `RollLegacyParameterNamesReceiveRules`（`Upper Roller Rotate Velocity` 现在**有**规则） |

**待确认（按字面实现，已在 spec §5.2 备注）**：`Total Diluted Chemical Flow Rate` / `Chemical Dilution Ratio` 的条件**只依赖** `WF Upper Face Chemical Supply`，未附加 `Process Type` 限定。因此 `Process Type = 0/-1` 且化学供给为 `1` 时，这两行仍可编辑。

**验证结果（Rev03）**：

| 项 | 结果 |
|---|---|
| `RecipeOperabilityTests` | **53/53 全绿** |
| `CMP.Tests.Models` | **60/60 全绿** |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线** |
| Git | **未提交** |

---

## Rev04 增补：Polish 工作簿落地（`Polish A_For_Recipe_Rev15.xls`，仍未提交 Git）

**12 个 recipe sheet → 11 个模板**，共注入 **275 条规则**。求值引擎**零改动**（`SourceParameter` 本就是任意参数名，支持 `Head Sweep Type` / `Condition Sweep Type`）。

### 侦查阶段的关键发现（决定了实现方式）

| # | 发现 | 处理 |
|---|---|---|
| 1 | 规则**不在 `说明` 列**，而在 **`显示条件说明`** 列（修订履历也印证） | 按用户决策用 `显示条件说明` |
| 2 | `说明` 列在 `Load Retry` / `UnLoad` / `UnLoad Retry` / `Dechuck` / `Dechuck Retry` / `ExsitCondition` 6 个 sheet 里是**遗留脏数据**（引用 `Process Type=1/2/3或4`，而这些表只有 `10/15/20/25` 与 `-1`） | 弃用该列。若照它实现会让这些行**永久灰显**（Roll 修表前同款陷阱） |
| 3 | 各 sheet 的模式参数不同：`Process Type` / `Head Sweep Type` / `Condition Sweep Type` | 引擎已支持；测试的 `BuildContent` 泛化为「第 1 行 = 模式参数」 |
| 4 | 现场旧参数名（`TopRingType` / `ZoneRRPressure` / `PlantWash` / `DIW` / `PolishHeight` / `SlurryStartPosition` / `SlurrySweepVelocity` / `BlowWFSelect` / `RRType` / `ConditionHeadRotation` / `DechuckHeight`） | 模板的 `CompatibleName` **已全部覆盖** → 存量配方能继承规则，无需改动 |
| 5 | 唯一断点：Excel `Platen Wash Selection` ↔ 模板 `Plant Wash Selection` | 给 3 个模板（Polish / Dechuck / DechuckRetry）补别名 `PlatenWashSelection`；**行名不动**（PLC 侧 `GetRealVal` 依赖 `PlantWashSelection`） |
| 6 | `ConditionRecipeTemplate` 同时服务 `Condition Recipe`（说明全空）与 `ExsitCondition Recipe`（`=25`） | 允许值取并集 **`{1,25}`**；`Reason` 保留 Excel 原文；夹具的 `conditions` 记录生效值 `["1","25"]` 并由顶层 `note` 说明偏差 |
| 7 | 部分 `不使用=1` 的行**模板里存在、界面会显示**（`Head Middle Wash` / `Head External Wash` / `Head Rotation` / `InSitu` / `Sweep Stop Time Type` / `Ratio`） | 按决策挂规则；对账测试断言 3 从「不使用行不得存在于模板」放宽为「若模板存在则必须挂规则」 |
| 8 | 现场 `Condition Sweep Recipe` 文件夹里同时有 in-situ / ex-situ 形态（实测 `Recipe_ExsituCondition Sweep.json` 就在其中） | 两个 Sweep 模板挂同一套规则，并加 `SweepTemplatesStayInSync` 断言 |
| 9 | `选择项次数条件` 列也有数据（样例 `1:0;`） | 工艺确认**不纳入**本次 |

### 变更文件

| 变更项 | 内容 |
|---|---|
| 模板（11 个） | `PolishRecipeTemplate` +36、`HeadSweepRecipeTemplate` +24、`LoadRecipeTemplate` +24、`LoadRetryRecipeTemplate` +24、`UnloadRecipeTemplate` +26、`UnloadRetryRecipeTemplate` +26、`DechuckRecipeTemplate` +26、`DechuckRetryRecipeTemplate` +26、`ConditionRecipeTemplate` +15、`InsituContidionSweepRecipeTemplate` +24、`ExsituContidionSweepRecipeTemplate` +24 |
| 别名 | Polish / Dechuck / DechuckRetry 的 `Plant Wash Selection` 行补 `PlatenWashSelection` |
| 注册表 | `RecipeTemplateRegistry`：新增 12 个文件夹常量与映射（Rev04 时共 15 个，**Rev11 移除冗余的 `Head Recipe` 后为 14 个**）；`Condition Recipe` 与 `ExsituCondition Recipe` 同指 `ConditionRecipeTemplate` |
| 夹具（12 份新增） | 由脚本从 Excel 直接生成（含 `excelName` 便于追溯）；`CMP.Tests.csproj` 的 3 条独立 `Content` 改为 `Resources\*.json` 通配 |
| 测试 | `BuildContent` 泛化为「第 1 行 = 模式参数」；`TemplateIdsAndNormalizedNamesAreUnique` / `EveryOperableRuleSourceExistsInTheSameTemplate` 改为遍历 `AllTemplates`（14 模板）；`TemplateRulesMatchExcelFixture` 改为按模板名 + 14 组 DataRow；`SingleConditionRulesFollowProcessTypeOptions` → `SingleConditionRulesFollowModeParameter`（15 文件夹）；新增 6 个 Polish 专项测试 |

### 验证结果（Rev04）

| 项 | 结果 |
|---|---|
| `RecipeOperabilityTests` | **93/93 全绿**（Rev03 为 53） |
| `CMP.Tests.Models` | **100/100 全绿** |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线** |
| 夹具 | 15 份，覆盖 14 个模板 |
| Git | **未提交** |

### 遗留（已固化，非阻塞）

1. **`Polish Recipe` 的 `64: Pad Search`**：规则全是 `=1或2` → 选 Pad Search 时除 `Process Type` 外整表灰显。已用 `PolishPadSearchGreysEveryGatedParameter` 固化，工艺若另有要求需修订表格。
2. **两处已知缺口沿用**：未注册文件夹缺 `Logger.Warn`；Dry 的回归测试未覆盖真实 `111.json` 全 15 行。
3. 手工 UI 验收（spec §10 共 12 条）需在现场机器执行。

---

## Rev05 增补：Condition 模板拆分（仍未提交 Git）

**背景**：用户把 `ConditionRecipeTemplate` 重命名为 `InsituConditionRecipeTemplate`，并新建了 `ExsituConditionRecipeTemplate`，要求按两个 sheet 分别设计 `OperableRule`，不再共用模板。

### OperableRule 重新设计（按 sheet 字面）

| sheet | 模板 | 显示条件说明 | 规则 |
|---|---|---|---|
| `Condition Recipe_Polish A` | `InsituConditionRecipeTemplate` | **全为空** | **0 条**（移除 Rev04 遗留的 15 条） |
| `ExsitCondition Recipe_Polish A` | `ExsituConditionRecipeTemplate` | `Process Type=25时允许` ×15 | **15 条**，允许值由 Rev04 的折中 `{1,25}` 收回为**字面 `{25}`** |

> Rev04 的 `{1,25}` 并集纯粹是为「两 sheet 共用一个模板」做的妥协；拆分后不再需要。

### 拆分时点发现的 4 处缺陷（已修）

| 位置 | 问题 | 修法 |
|---|---|---|
| `RecipeEditorViewModel.OnTreeAdd` | 缺 `ExsituConditionRecipeTemplate` 的 `case` → 新建该类型配方会落到 `default: return;` **静默失败** | 补 `case` 指向 `ExsituConditionRecipeTemplate.Template` |
| `RecipeTemplateRegistry` | `ExsituCondition Recipe` 仍指 `InsituConditionRecipeTemplate` → 该文件夹规则永不生效 | 改指 `ExsituConditionRecipeTemplate.Template` |
| `ExsituConditionRecipeTemplate.cs` | 声明为 `public class`（其余模板均 `public static class`） | 改为 `public static class` |
| `AddRecipeEditorViewModel` | case 写作 `"Exsitu Condition Recipe"`（多一个空格），实际文件夹名是 `ExsituCondition Recipe` | 标签改为实际文件夹名 |

### 测试调整

- `AllTemplates` 14 → **15** 个模板（新增 `ExsituCondition Recipe`）。
- `TemplateRulesMatchExcelFixture`：`Condition Recipe` 改用 `condition-operability-fixture.json`（0 规则 → 断言 1/2 空转通过），新增 `ExsituCondition Recipe` 一行。
- **对账断言 3 收紧**：`NotUsed == 1` **且 `Reason != null`** 且模板存在 → 必须挂规则。原先只判 `NotUsed == 1`，对 `Condition Recipe` 那些「不使用=1 但没有任何条件」的行会误报。
- `SingleConditionRulesFollowModeParameter` 移除 `Condition Recipe` 文件夹（该模板按设计 0 规则，`Count > 0` 断言不适用）。
- 新增 `ConditionTemplatesAreIndependent`（两模板互不相同、Insitu 无规则、Exsitu 每行 `{25}`）、`ExsituConditionParametersFollowProcessType`、`InsituConditionParametersAreAlwaysOperable`。

### 验证结果（Rev05）

| 项 | 结果 |
|---|---|
| `RecipeOperabilityTests` | **96/96 全绿** |
| `CMP.Tests.Models` | **103/103 全绿** |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线** |
| 规则分布 | `InsituConditionRecipeTemplate` = 0 条；`ExsituConditionRecipeTemplate` = 15 条 |
| Git | **未提交** |

### 待你确认（未改，属 Option 集而非 OperableRule）

1. ~~**`ExsituConditionRecipeTemplate` 的 Process Type 选项仍是 `1:InsituCondition / 25:ExsituCondition / -1:Recipe End`**~~ **已按表格修正** → `["25:Condition", "-1:Recipe End"]`，`Default = "25"`。
2. ~~**`InsituConditionRecipeTemplate` 的 Process Type 选项**~~ **已按表格修正** → `["1:Condition", "2:None"]`（现场 `Condition Recipe_Polish A.json` 本就是这个集合）。
3. 两个模板的 Process Type `Min = 1 / Max = 2` 保持 Excel 原值未动；因 `Type = "option"` 走 ComboBox，`Min/Max` 不参与校验，暂无功能影响（Exsitu 的 `Min/Max` 与选项 `25/-1` 在 Excel 里本就不一致）。

### 选项集变更的风险评估（已核实）

| 关注点 | 结论 |
|---|---|
| 存量配方会被改写吗？ | **不会**。`Decorate` 只写 `OperableRule`；`CheckRecipeDefualtValue` 校验的是配方自带的 `Options`，与模板无关。`4545tg.json` / `ykuk.json` 里仍是旧集合 `["1:InsituCondition","25:ExsituCondition","-1:Recipe End"]`，不受影响 |
| 现场已有配方与新选项集一致吗？ | `Condition Recipe_Polish A.json` 本就是 `["1:Condition","2:None"]` → 变更属对齐 |
| `ExsituCondition Recipe` 文件夹有数据吗？ | 空（0 个 JSON）→ 无风险 |
| `Default` 会指向不存在的选项吗？ | 已同步：Insitu `"1"` → 命中 `1:Condition`；Exsitu `"25"` → 命中 `25:Condition`。新增测试 `ConditionProcessTypeOptionsMatchTheExcelSheets` 断言 `Default` 能 `StartsWith` 命中某个选项（`OnAddStep` 的匹配方式） |

### 验证结果（Rev05 二次）

| 项 | 结果 |
|---|---|
| `CMP.Tests.Models` | **104/104 全绿** |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线** |
| Git | **未提交** |

### Rev05 三次：逐行核对 Min / Max / Default

用脚本把两张 sheet 的 `Min.` / `Max.` / `初期值` 与模板的 `Min` / `Max` / `Default` 逐行对齐（按归一化参数名 + 别名匹配）。

**已修正（InsituConditionRecipeTemplate，6 处）**

| 参数 | 项目 | 原值 | 改为 | 依据 |
|---|---|---|---|---|
| Process Time | `Default` | `"100.00"` | `"5.00"` | 初期值 5 |
| Platen Rotation | `Max` | `180.00` | `30.00` | Max. 30 |
| Platen Rotation | `Default` | `"10.00"` | `"30.00"` | 初期值 30 |
| Slurry Line 1/2/3 Flow | `Default` | `"100.00"` | `"0.00"` | 初期值 0 |

**ExsituConditionRecipeTemplate**：除下表的异常行外，Min/Max/Default 全部与 `ExsitCondition Recipe_Polish A` 一致，**无改动**。

**刻意未照抄的异常（Excel 数据本身有误）**

| 行 | Excel 初期值 | 保留 | 原因 |
|---|---|---|---|
| Platen Wash Selection、Slurry Line 1/2/3 Type（两 sheet 共 7 行） | `0.1` | `"0"` | `option/list` 参数的合法值是 `0:Close` / `0:Slurry` 等，`0.1` 非法；`CheckRecipeDefualtValue` 会把它归一到 `Options[0]`，与现值**等效**。已核实单元格格式为 `0.00_ `（不是百分比显示），值确实是 0.1 |
| Sweep Stop Time Type（仅 Exsitu） | `0.1` | `"0"` | 同上 |
| Process Type（仅 Exsitu） | `1` | `"25"` | Excel 初期值 `1` 与该表自己的选项集 `{25:Condition, -1:Recipe End}` 冲突；取唯一有效过程值 `25` |

**新增护栏**：两份夹具登记 Excel 的 `excelMin`/`excelMax`/`excelDefault`，异常行带 `valueNote`；测试 `ConditionTemplateNumericConfigMatchesExcel` 逐行断言一致（带 `valueNote` 的行跳过）。两个模板的 `Min`/`Max`/`Default` 数值现已与表格对齐 —— 唯一保留的 6 行差异均有书面理由。

### 验证结果（Rev05 三次）

| 项 | 结果 |
|---|---|
| `CMP.Tests.Models` | **106/106 全绿** |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线** |
| Git | **未提交** |

---

## Rev06 增补：「不使用」行隐藏（仍未提交 Git）

### 需求与语义

Excel 的「不使用」列 = 1 的参数行，**界面上不显示**。

### 实施

| 层 | 变更 |
|---|---|
| 模型 | `Recipe.Data.IsUsed`（bool，默认 `true`，`[JsonIgnore]`）—— 与 `OperableRule` 同为**规格元数据**，真源 = Excel「不使用」列，不落盘 |
| 挂载 | `RecipeTemplateRegistry.Decorate` 在贴 `OperableRule` 的同时把模板的 `IsUsed` 贴到已载入的行上（存量 JSON 同样靠归一化名 + `CompatibleName` 命中） |
| 模板 | **28 个模板行**加 `IsUsed = false`（9 个模板，见 spec §0 Rev06 的逐模板清单） |
| 视图 | 新增 `CMP/Views/Tabs/RecipeTab/RecipeRowVisibility.cs`（过滤谓词）；`RecipeEditorViewModel.ApplyRowVisibility()` 给 DataGrid 绑定集合的**默认 CollectionView** 设 `Filter`，在 `RefreshEditor` 中调用 |
| XAML | **未改** |

**为什么用 CollectionView 过滤而不是 `RowStyle` + `DataTrigger`**：
1. 不改 XAML，也不用 `BasedOn="{StaticResource {x:Type DataGridRow}}"`（后者在主题样式缺失时会 `StaticResource` 抛异常，我无法在本环境启动 WPF 实测，风险不可接受）；
2. 不覆盖 HandyControl 的行样式，无视觉回归风险；
3. `RecipeData.Data` 本身不变 → **隐藏行的数据仍保留、照常落盘与下发**（这是关键正确性要求）。

### 唯一例外

~~`InsituConditionRecipeTemplate` 的 `Process Type`：保留显示。~~ **已取消**：按工艺要求**严格照表**，`InsituConditionRecipeTemplate.Process Type` 现为 `IsUsed = false`（隐藏）。夹具的 `isUsedNote` 例外机制已删除，`TemplateIsUsedMatchesExcelNotUsed` 现在是无例外的严格校验。

**代价（需现场确认）**：Insitu Condition 配方编辑器只显示 6 行，`Process Type` 不可见 → 无法在编辑器内切换工艺模式。若不符合工艺意图，应修订 `Condition Recipe_Polish A` 表格的 `不使用` 列，而不是在代码里加特例。

### 全部模板行显式声明 IsUsed

按工艺要求，**356 个模板行全部显式写出 `IsUsed`**（`true` 327 / `false` 29），不再依赖「默认 true」的隐式行为：

| 模板 | 行数 | true | false |
|---|---|---|---|
| `DryRecipeTemplate` | 16 | 14 | 2 |
| `PenA1RecipeTemplate` | 20 | 17 | 3 |
| `RollRecipeTemplate` | 16 | 15 | 1 |
| `PolishRecipeTemplate` | 39 | 39 | 0 |
| `HeadSweepRecipeTemplate` | 25 | 25 | 0 |
| `LoadRecipeTemplate` / `LoadRetryRecipeTemplate` | 各 25 | 各 23 | 各 2 |
| `UnloadRecipeTemplate` / `UnloadRetryRecipeTemplate` | 各 27 | 各 24 | 各 3 |
| `DechuckRecipeTemplate` / `DechuckRetryRecipeTemplate` | 各 27 | 各 27 | 各 0 |
| `InsituConditionRecipeTemplate` | 16 | 6 | 10 |
| `ExsituConditionRecipeTemplate` | 16 | 13 | 3 |
| `InsituContidionSweepRecipeTemplate` / `ExsituContidionSweepRecipeTemplate` | 各 25 | 各 25 | 各 0 |
| **合计** | **356** | **327** | **29** |

配合用户已放开的 Dry/Pen/Roll 注释行，**表格成为唯一真源**：以后某参数在「使用/不使用」之间切换，只需翻转 `IsUsed` 的布尔值，不必增删代码行。

### 连带发现：Dry / Pen / Roll 的注释行已被放开

实施前检查发现，Dry / Pen / Roll 三个模板里**原先被注释掉**的 `不使用=1` 行已由用户放开成正式行（Dry +2：`IPA Arm Sweep Center/Edge Velocity`；Pen +3：`WF Upper Face N2 Flow`、`WF Lower Face N2 Supply`、`WF Upper Face DI-CO2 Supply`；Roll +1：`Mega Sonic Supply`）—— 正是本特性要隐藏的对象。放开后 `TemplateRulesMatchExcelFixture` 的断言 3 立刻报了 2 处失败（这些行没有规则），确认了断言语义需要随新特性调整。

### 断言调整

`TemplateRulesMatchExcelFixture` 的第 3 条断言（`不使用=1` 且模板存在且带条件 → 必须挂规则）**已删除**：隐藏行谈不上可操作性。其职责由新增的 `TemplateIsUsedMatchesExcelNotUsed` 承接 —— 15 份夹具逐行断言 `IsUsed == (不使用 == 0)`。

### 测试

| 测试 | 作用 |
|---|---|
| `TemplateIsUsedMatchesExcelNotUsed`（15 组） | 逐行核对模型 `IsUsed` 与 Excel「不使用」列 |
| `UnusedRowsAreHiddenFromTheEditorView` | 过滤后可见集合不含 `IsUsed=false` 的行；源集合条数不变 |
| `DerivedOperabilityFieldsAreNeverSerialized`（扩展） | 断言 JSON 不含 `IsUsed` |

### 验证结果（Rev06）

| 项 | 结果 |
|---|---|
| `RecipeOperabilityTests` | **115/115 全绿** |
| `CMP.Tests.Models` | **122/122 全绿**（Rev05 为 106） |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线** |
| 标记行数 | 356 行全部显式 `IsUsed`（true 327 / false 29） |
| Git | **未提交** |

### 待现场确认

`InsituConditionRecipeTemplate` 隐藏 10 行（含 `Process Type`、`Platen Rotation`、`Platen Wash Selection`、3 个 Slurry Type、3 个 Slurry Flow），而 `ExsituConditionRecipeTemplate` 只隐藏 3 行 —— 两张 sheet 对 `InSitu` / `Platen Rotation` 等参数的「不使用」标记并不一致（Insitu sheet 标 1、Exsitu sheet 标 0）。这是 Excel 里的真实差异，现已严格照表执行；但 Insitu 表那一列看着像整列误标（连 `Process Type` 都被标为不使用），建议工艺复核。

---

## Rev07 增补：全角冒号加固（仍未提交 Git）

### 背景

工艺提醒：Excel 人工维护，中文输入法下 `:` 容易变成全角 `：`(U+FF1A)，导致选项解析**静默失败** —— `NumericPrefix` 返回 null → fail-open → 该灰显的格子不灰显（门控形同失效）；若流入模板还会让 PLC 侧 `GetRealVal` 取不到值。

### 扫描结论

| 位置 | 全角冒号 | 结论 |
|---|---|---|
| 四份表格全部单元格 | 仅 2 处，均在**中文说明文字**中（`一 区域说明：`、修订履历 `选项2：Close`） | 「选择项」单元格全部使用半角冒号 |
| 现场 44 个配方 JSON 的 `Options`/`Default`/`Steps` | **0 处** | 数据未污染 |

问题当前未发生，但风险真实（那 2 处正是中文输入法的产物）。

### 加固

| 层 | 变更 |
|---|---|
| 解析层 | `RecipeOptionValue.NumericPrefix` 分隔符放开为 `:` `：`(U+FF1A) `﹕`(U+FE55) `∶`(U+2236)；新增 `PartAfterSeparator()` |
| 调用点 | `RecipeEditorViewModel.OnTreeViewNodeChanged` 的 `item.Value.Split(":")` → `RecipeOptionValue.PartAfterSeparator(item.Value)`（同一 bug 类的第二处） |
| 护栏 | `TemplateOptionsAndDefaultsUseHalfwidthColonsOnly` 断言所有模板 `Options`/`Default` 只用半角 —— 把问题挡在「表格 → 模板」边界，因为 PLC 侧只按半角拆分 |
| 测试 | `NumericPrefixParsesOptionStrings` +6 组全角/变体 DataRow；`PartAfterSeparatorStripsNumericPrefix` 6 组；`FullwidthColonInStepValueStillMatchesTheRule` |

### 顺带查出的录入隐患（当前转写均正确，无需改）

| 现象 | 位置 | 现状 |
|---|---|---|
| 英文选择项用**全角分号 `；`** 分隔 | `Pen A1`：`Pen Arm Sweep Start/End Position`、`WF Chemical Ratio`（3 处） | 模板 `Options` 已正确拆开；**后续机械转写必须同时按 `;` 与 `；` 拆** |
| 选项列表用省略号缩略 | `Polish Recipe` 的 `Jump Step No.`（`……25:Step25`） | 模板已展开为完整 26 项 |
| 非纯数字前缀 `>1:MIX` | `Pen A1` 的 `WF Chemical Ratio` | 模板中该参数为 `float` 且无 `Options`，不受影响 |
| 「初期值」列 | 全部表格 | 无非空非数值内容，数值对账可靠 |

### 残留暴露（未改）

PLC 下发路径的 `GetRealVal`（5 个 Operation VM）仍只按半角 `:` 拆分。当前数据无全角冒号，且护栏测试已守住「表格混入全角」的入口；若要加固属独立任务（会动下发路径，与本期"下发路径不动"的决策冲突）。

### 验证结果（Rev07）

| 项 | 结果 |
|---|---|
| `CMP.Tests.Models` | **136/136 全绿**（Rev06 为 122） |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线** |
| Git | **未提交** |

---

## Rev08 增补：把 IsUsed 推广到 PLC 下发路径（仍未提交 Git）

### 需求

工艺在 `PolishUnitTabContentViewModel.GetRealVal` 中加了 `if (model == null || !model.IsUsed) return default;` —— 不使用的参数下发 0。要求推广到其他处。

### 实施

| 项 | 变更 |
|---|---|
| 守卫 | 5 处 `GetRealVal` 全部加 `!model.IsUsed`（Dry / Roller / Pen / Polish / LoadPort） |
| **Decorate** | **4 处 `LoadRecipeContent` + LoadPort 加载点补调 `RecipeTemplateRegistry.Decorate(content, folderName)`** |
| 选项解析 | 5 处 `value.Split(':')` → `RecipeOptionValue.NumericPart()` / `LabelPart()` |
| 新方法 | `RecipeOptionValue.NumericPart` / `LabelPart` |

### ⚠️ 最重要的一点：守卫原本是死代码

`Recipe.Data.IsUsed` 是 `[JsonIgnore]` 的，**只有 `Decorate` 会填充它**；而全部 Operation Tab 都从不调用 `Decorate`（全仓库此前只有 `RecipeEditorViewModel` 调过一次）。因此工艺加的守卫在补 `Decorate` 之前**永远不会触发**。

### 顺带修掉的无效代码

Polish 的 `GetRealVal` 里有一行 `value.Replace("：",":")` —— `string.Replace` 返回新字符串，结果被丢弃，**该语句没有任何效果**。已由 `NumericPart`/`LabelPart` 正确覆盖（两者都认全角冒号）。

### 行为等价性核对

| 原实现 | 新实现 | 差异 |
|---|---|---|
| 数值目标 `arr[0]` | `NumericPart` = 数字前缀，失败则去空白原文 | 仅「全角冒号可解析」改进 |
| 字符串目标 `arr.Length >= 2 ? arr[1] : arr[0]` | `LabelPart` = 分隔符后部分并去空白 | 仅「全角冒号可解析」+「去首尾空白」改进 |

### 附带发现（既存问题，未改，留待工艺确认）

**Retry 的下拉列表与加载器都指向非 Retry 文件夹**：
- `case "Load Retry Recipe": LoadRecipesFromFolder("Load Recipe", LoadRetryRecipes)`，且 `AssignLoadRetryRecipe()` 里 `LoadRecipeContent("Load Recipe", ...)`
- `case "Dechuck Retry Recipe": LoadRecipesFromFolder("Dechuck Recipe", ...)`，同理
- 现场实际文件：`Load Retry Recipe/H6H.json`、`UnLoad Retry Recipe/UTUTU.json`、`Dechuck Retry Recipe/111.json,112.json`

列表与加载器**自洽**（不会报"文件未找到"），但真实 Retry 配方**永远选不到**，而常规配方会被写进 Retry 的 PLC 结构体。修改会改变「下发哪份配方」，故未擅自改动。

（另：代码里的 `"Unload Recipe"` 与注册表常量 `"UnLoad Recipe"` 大小写不同，但 `Resolve` 用 `OrdinalIgnoreCase` → 可正常解析。）

### 验证结果（Rev08）

| 项 | 结果 |
|---|---|
| `CMP.Tests.Models` | **150/150 全绿**（Rev07 为 136） |
| `CMP` 构建 | 0 错误，警告 **1006 ≤ 基线 1008**（重构后比基线少 2，均为既存警告） |
| Git | **未提交** |

---

## Rev09 增补：Polish 页 ExsituCondition 下拉框从 ConditionRecipes 分离（仍未提交 Git）

### 问题

`PolishUnitTabContentView.xaml:102` 那一行（TextBlock `Condition`、按钮 `BreakIn` → `ExsituConditionCommand`）的 ComboBox 绑在 `ConditionRecipes` 上，而：

| 环节 | 实际文件夹 |
|---|---|
| 下拉列表 `ConditionRecipes` | `Condition Recipe`（**In-situ**） |
| `BreakInCommandExecute` 加载（`GetRealVal` 之前那一步） | `ExsituCondition Recipe`（**Exsitu**） |
| 自动流程 `GetRecipe` 取的 Sequence 键 | `RecipePolishAExsitu` / `RecipePolishBExsitu` / `RecipePolishCExsitu`（**Exsitu**） |

即**列表与加载指向不同文件夹**：界面上列出的是 in-situ 配方，执行时却按同名去 Exsitu 文件夹找 → 找不到就只弹一句 "Recipe file not found"。

### 修复

| 层 | 变更 |
|---|---|
| VM | 新增 `ExsituConditionRecipes` 集合（从 `ExsituCondition Recipe` 加载） |
| VM | `SelectedConditionRecipe` → **改名** `SelectedExsituConditionRecipe`（该属性的真实消费者一直是 Exsitu/BreakIn 路径） |
| VM | `BreakInCommandExecute` 内 5 处引用改指向新成员；`AssignWaferToPLC` 也写入新成员 |
| VM | 初始化：`LoadRecipesFromFolder("ExsituCondition Recipe", ExsituConditionRecipes)` + 默认选中 |
| VM | Recipe 保存事件新增 `case "ExsituCondition Recipe":` 分支（重载该列表并保持选中）；`case "Condition Recipe"` 分支只保留 `SelectedPreCondRecipe` |
| XAML | `PolishUnitTabContentView.xaml:102-103` → `ItemsSource="{Binding ExsituConditionRecipes}"` / `SelectedItem="{Binding SelectedExsituConditionRecipe}"` |

**保留未动**：`ConditionRecipes` / `SelectedPreCondRecipe`（仍服务于 XAML 中 `Visibility="Hidden"` 的 `Pre_Cond` 行）；`AssignConditionRecipe()`（并非 UI 按钮路径，而是解析 Polish/Exsitu 配方里 `Condition Recipe` 子配方引用用的辅助方法，从 `Condition Recipe` 文件夹取——符合其语义）。

**未改但值得注意**：该行 TextBlock 的文案仍是 `{lex:Loc Condition}`（"Condition"），而它现在列的是 Exsitu 配方、按钮写 BreakIn。若要改成 "Exsitu Condition" 需新增本地化键，本次未动。

### 验证结果（Rev09）

| 项 | 结果 |
|---|---|
| `CMP.Tests.Models` | **150/150 全绿** |
| `CMP` 构建 | 0 错误，警告 **1006 ≤ 基线** |
| `SelectedConditionRecipe` 残留 | 0（全仓库已无引用） |
| Git | **未提交** |

---

## Rev10 增补：新设备首启补齐 Recipe 文件夹（仍未提交 Git）

### 需求

其它设备首次运行时 `D:\CMP\Configuration\Recipes\UnitRecipe\` 下没有任何子目录 → 各类型的配方无法新建/读取。要求在**初始化 Recipe 数据之前**检查并创建「每个 RecipeTemplate 对应的文件夹」。

### 实施

| 层 | 变更 |
|---|---|
| `RecipeTemplateRegistry` | 新增 `RegisteredFolders`（**Rev11 后 14 个**文件夹名，来自 `Factories` 键）+ `EnsureFoldersExist(basePath)`（幂等，返回本次新建的名字） |
| `RecipeEditorViewModel` | 新增私有 `EnsureRecipeFolders()`，在 **`InitRecipesData()` 第一行**调用（`InitRecipesData` 由构造函数调用） |

**为什么创建逻辑放在注册表**（而不是全写在 VM 里）：
1. 文件夹清单必须以注册表为单一真源 —— 新增模板时自动带上目录，不会漏建；
2. 便于**自动测试**：`EnsureFoldersExist` 可传入临时目录验证，VM 里则无法（`recipeBasePath` 是硬编码私有字段）。

**失败处理**：`EnsureRecipeFolders()` 捕获异常并 `Logger.Error` —— 建目录失败不应阻断编辑器启动；界面表现为列表为空，日志可定位原因。

### 测试

| 测试 | 作用 |
|---|---|
| `RegisteredFoldersMatchTheDeployedRecipeFolders` | 清单与现场 15 个目录**集合相等**，且每个都能 `Resolve` 到模板 |
| `EnsureFoldersExistCreatesEveryRegisteredFolderIdempotently` | 临时目录：全部建出 → 二次调用返回 0 → 空/空白路径安全返回；用后清理 |

### 验证结果（Rev10）

| 项 | 结果 |
|---|---|
| `CMP.Tests.Models` | **152/152 全绿**（Rev09 为 150） |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线**（新代码无警告） |
| Git | **未提交** |

---

## Rev11 增补：移除冗余的 `Head Recipe` 文件夹（仍未提交 Git）

### 发现

`Head Recipe` 与 `Head Sweep Recipe` 重复 —— 两者在注册表里同指 `HeadSweepRecipeTemplate`，且现场配方 `Recipe_Head Sweep.json` **已在 `Head Sweep Recipe` 目录中**，`Head Recipe` 目录已不存在。

**必须改代码的理由**：Rev10 的 `EnsureFoldersExist` 会按 `RegisteredFolders` 在启动时补齐缺失目录 —— 不移除这一项，**每次启动都会把已删掉的空 `Head Recipe` 目录重新建出来**。

### 变更

| 项 | 内容 |
|---|---|
| `RecipeTemplateRegistry` | 删除 `HeadRecipeFolder` 常量与 `Factories` 映射 → 已注册文件夹 **15 → 14** |
| 测试 | `ResolveReturnsTemplateForKnownFolder` 移除该 DataRow；`RegisteredFoldersMatchTheDeployedRecipeFolders` 期望集合移除 `"Head Recipe"` |

### 验证结果（Rev11）

| 项 | 结果 |
|---|---|
| `CMP.Tests.Models` | **151/151 全绿**（移除 1 个 DataRow） |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线** |
| 注册表文件夹数 | **14** |
| 代码中 `HeadRecipeFolder` / `"Head Recipe"` 残留 | 0 |
| Git | **未提交** |

---

## Rev12 增补：Recipe 文件夹名统一走 `RecipeTemplateRegistry` 常量（仍未提交 Git）

### 需求

`PolishUnitTabContentViewModel` / `ConfigViewModel` / `AddRecipeEditorViewModel` 里硬编码的 Recipe 文件夹名，统一改用 `RecipeTemplateRegistry` 的常量。

### 实施范围（先全仓库扫描，再逐处替换）

| 文件 | 替换数 | 性质 |
|---|---|---|
| `PolishUnitTabContentViewModel.cs` | 42 + 3 Retry 加载点 | ✅ 文件夹名 |
| `ConfigViewModel.cs` | 18 | ✅ 文件夹名（`item.Name` 即文件夹名） |
| `AddRecipeEditorViewModel.cs` | 14 | ✅ 文件夹名（`GetDefualtTemplate` 的 switch） |
| `Chamber.cs` | 4 | ✅ `ResolveUnitRecipePath` 的 工位→文件夹 映射（顺带补 `using`） |
| `DryTabContentViewModel.cs` | 5 | ✅ 加载/列表/事件判断 |
| `GenericRollerTabContentViewModel.cs` | 5 | ✅ 同上 |
| `PenCleanTabContentViewModel.cs` | 5 | ✅ 同上 |
| `RecipeEditorViewModel.cs` | 3 | ⚠️ 参数名兼作文件夹路径（见下） |
| **合计** | **99** | |

**明确不改**（扫描中已识别并排除）：
- `PolishRecipeTemplate` / `InsituConditionRecipeTemplate` / `ExsituConditionRecipeTemplate` 里的 `Name = "Head Sweep Recipe"` / `"Condition Recipe"` / `"Condition Sweep Recipe"` —— 这些是**模板参数名**，不是文件夹名；
- `MessageEvents.cs` 中的一处注释。

`RecipeEditorViewModel` L729-731 的比较对象是 `data.Name`（参数名），同时又把该名字当文件夹路径用 —— 因模板就是按文件夹名命名该选择项，故直接引用常量并**加注释说明双重身份**，避免两处字面量各自漂移。

### 顺带修掉的两处真实缺陷

| 缺陷 | 证据 | 影响 |
|---|---|---|
| **3 个 Retry 加载点指向非 Retry 文件夹** | `AssignLoadRetryRecipe` 等：列表侧已由工艺改为 `Load Retry Recipe`，加载侧仍写 `"Load Recipe"` → `Load Recipe\H6H.json` 不存在 | 下拉列出真实 Retry 配方（`H6H` 等）却加载不到 → 只弹一句 "file not found"，**下发不生效**。改用 `LoadRetryRecipeFolder` / `UnloadRetryRecipeFolder` / `DechuckRetryRecipeFolder` 后与列表一致；已核对三份 Retry 配方的参数结构与目标 KST 结构体完全匹配（Load 25 / UnLoad 27 / Dechuck 27 项） |
| **第二个事件 switch 的 Unload 分支大小写不匹配** | `case "Unload Recipe"`（小写 l）vs 事件发布的实际文件夹名 `"UnLoad Recipe"` | 该分支永不命中 → 保存 Unload 配方后列表不刷新。改用常量后自然对齐 |

### 仍未统一（需工艺确认）

`ConfigViewModel` L487 `string folderName = item.Name.Replace("Retry ", "");` —— **有意**让 Config 页的 Retry 选项列出**非 Retry** 文件夹的配方（"Load Retry Recipe" → "Load Recipe"）。这与 Polish 页现已改为列真实 Retry 配方的做法不一致。属功能语义选择，本次未动。**（Rev13 已按工艺要求删除该行。）**

### 验证结果（Rev12）

| 项 | 结果 |
|---|---|
| `CMP.Tests.Models` | **151/151 全绿** |
| `CMP` 构建 | 0 错误，警告 **1008 = 基线** |
| 代码中游离的文件夹字面量 | 0（仅剩常量定义、模板参数名、1 处注释） |
| Git | **未提交** |

## Rev13 增补：Sweep 文件夹拆分 / Retry 解耦 / 参数名→文件夹显式映射（仍未提交 Git）

### 需求（三条，按用户给定顺序执行）

1. `ConfigViewModel` L487 的 `string folderName = item.Name.Replace("Retry ", "");` —— 以前是「同一个文件夹混用」，现在分开管控了，直接删掉这行。
2. `RecipeTemplateRegistry.ConditionSweepRecipeFolder` 拆成 `InsituConditionSweepRecipeFolder` 与 `ExsituConditionSweepRecipeFolder`（两个 sweep 现在各有自己的模板）。
3. `AssignConditionSweepRecipe` 改成两个方法分别下发，不再用 `type` 参数辨别。

### 拆分前勘察到的不一致（说明第 2 条为什么必须做）

| 位置 | 指向的模板 |
|---|---|
| `RecipeTemplateRegistry.Factories["Condition Sweep Recipe"]` | `InsituContidionSweepRecipeTemplate` |
| `AddRecipeEditorViewModel.GetDefualtTemplate()` 的 `case ConditionSweepRecipeFolder` | `ExsituContidionSweepRecipeTemplate` |

同一文件夹、两个模板：编辑器按 Exsitu 模板新建，载入时却按 Insitu 模板贴 `OperableRule`/`IsUsed`。

### 面向用户的确认（按「不确定先问」）

| 问题 | 用户选择 |
|---|---|
| 两个文件夹的磁盘名 | `InsituCondition Sweep Recipe` / `ExsituCondition Sweep Recipe`（与 `InsituCondition Recipe` / `ExsituCondition Recipe` 命名对称） |
| 现场唯一的 sweep 配方文件 | 移入 Exsitu 目录，并补建 Insitu 空目录 |

### 实施

| # | 文件 | 改动 |
|---|---|---|
| 1 | `ConfigViewModel.cs` | 删除 `Replace("Retry ", "")` 行，改 `LoadRecipesFromFolder(item.Name)`（该文件启动路径 L888 本就是 `item.Name`，两处现已一致） |
| 2 | `RecipeTemplateRegistry.cs` | 1 个常量 → 2 个常量；`Factories` 各接自己的模板；`RegisteredFolders` 14 → 15 |
| 3 | `AddRecipeEditorViewModel.cs` | 原 1 个 `case` → 2 个 `case`，分别指向 `InsituContidionSweepRecipeTemplate` / `ExsituContidionSweepRecipeTemplate` |
| 4 | `PolishUnitTabContentViewModel.cs` | `AssignConditionSweepRecipe(..., type)` → `AssignInsituConditionSweepRecipe(...)` + `AssignExsituConditionSweepRecipe(...)`；两个调用点（Exsitu 主流程 L1974、Insitu 主流程 L2441）各自改名 |
| 5 | `RecipeEditorViewModel.cs` | 参数名判断改为 `ResolveRecipeFolderForParameter(data.Name, unit!.Name)`（**顺带修掉一个编译错误**，见下） |
| 6 | `CMP.Tests` | 模板标签、`RegisteredFolders` 期望集合、DataRow 同步；两个夹具的 `recipeType` 改为新文件夹名（仅文档字段，无测试消费） |
| 7 | 现场目录 | 建 `InsituCondition Sweep Recipe` / `ExsituCondition Sweep Recipe`；搬迁 `Recipe_ExsituCondition Sweep.json`；删除 `Condition Sweep Recipe` |

### 顺带修掉的编译错误（Rev12 的尾巴）

Rev12 把常量 `ConditionRecipeFolder = "Condition Recipe"` 改名为 `InsituConditionRecipeFolder = "InsituCondition Recipe"`（因现场目录已改名），但有 4 处引用没跟上：

- `RecipeEditorViewModel.cs:733`（编译错误）
- `RecipeOperabilityTests.cs:34 / 465 / 806(注释)`

**为什么不能简单换成新常量**：`RecipeEditorViewModel` 那一处的比较对象是 `data.Name` —— Polish 模板里该行参数名是 Excel 字面 `Condition Recipe`（见 `polish-operability-fixture.json:455`），换成 `"InsituCondition Recipe"` 后条件永不成立，该行下拉列表会静默取不到文件。故改为**显式映射**：

```csharp
private static string? ResolveRecipeFolderForParameter(string parameterName, string? ownerFolder) =>
    parameterName switch
    {
        "Head Sweep Recipe" => RecipeTemplateRegistry.HeadSweepRecipeFolder,
        "Condition Recipe" => RecipeTemplateRegistry.InsituConditionRecipeFolder,
        "Condition Sweep Recipe" => ownerFolder == RecipeTemplateRegistry.ExsituConditionRecipeFolder
            ? RecipeTemplateRegistry.ExsituConditionSweepRecipeFolder
            : RecipeTemplateRegistry.InsituConditionSweepRecipeFolder,
        _ => null,
    };
```

`Condition Sweep Recipe` 在两个 Condition 模板里同名，必须靠 `ownerFolder`（当前编辑的配方文件夹）分流 —— 这正是不再依赖「参数名 == 文件夹名」隐含约定的原因。

### 验证结果（Rev13）

| 项 | 结果 |
|---|---|
| `CMP.Tests.Models` | **153/153 全绿**（151 + 新增 2 个 sweep 文件夹 DataRow） |
| `CMP` 构建 | 0 错误（增量构建 1 个 NU1902 警告；全量构建警告数仍为基线 1008） |
| 残留旧标识符 | `ConditionSweepRecipeFolder` / `ConditionRecipeFolder` / `AssignConditionSweepRecipe` 均为 0 |
| 现场目录 | 15 个目录，与 `RegisteredFolders` 一一对应；`ExsituCondition Sweep Recipe` 内有唯一的 sweep 配方 |
| Git | **未提交** |

### 已知残留（未处理，供工艺判断）

- Insitu 侧暂无可选 sweep 配方；`InsituCondition Recipe\4545tg.json` 的该行 `Options` 残留 exsitu 配方名（选中值 `None`，无功能影响）。
