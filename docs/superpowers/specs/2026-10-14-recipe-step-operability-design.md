# Recipe 参数可操作性（说明列）设计规格

- 日期：2026-10-14
- 分支：`feat/RecipeEdit-StepOperability`
- 规格来源：`Dry A_For_Recipe_Rev01.xls` / `Pen A1_For_Recipe_Rev01.xls` / `Roll A1_For_Recipe_Rev01.xls` 的「说明」列（AG 列）
- 涉及模块：`CMP/Models/Recipe/RecipeEdit/**`、`CMP/ViewModels/Tabs/RecipeTab/RecipeEditorViewModel.cs`、`CMP.Tests`

---

## 1. 背景

三份 Recipe 规格 Excel 新增了「说明」列（AG 列），用于描述**当某个参数（主要是 `Process Type`）取不同值时，本表中哪些参数可以操作、哪些不可以**。

现状：该列只以文字形式存在于 Excel，代码中没有任何对应机制，编辑器对所有参数一律可编辑。后果是用户可以在 `Process Type=3 (Dry)` 的步骤里修改只对 `Process Type=1 (Dry With Arm)` 有意义的参数（如 `IPA Arm Sweep Cycle`），产生无效甚至危险的配置。

## 2. 目标

1. 把「说明」列从自然语言转成**可执行的声明式规则元数据**，规则真源与 Excel 保持一一对应。
2. 在 Recipe 编辑器中，当 `Process Type` 变化时，**逐单元格**实时反映可操作性：不可操作的单元格灰显、只读，并以 Tooltip 显示原因原文。
3. 机制通用（能承载 Dry / Pen A1 / Roll 三份规格的说明形态），但**一期只落 Dry 的 13 条规则**。
4. 对现场存量 Recipe 文件**零改动**、零风险。

## 3. 非目标（本期明确不做）

| 不做 | 原因 |
|---|---|
| 修改 PLC 下发路径（`DryTabContentViewModel.AssignDryRecipe`） | 一期只做 UI 门控；灰显**不等于**不下发，此边界在本文档中显式记录 |
| 把不可操作步骤的值归零或回默认值 | 会丢历史配置并产生大量审计条目 |
| 隐藏不可操作的单元格 | 会与"参数是否存在于配方"混淆，排查困难 |
| 改写现场 Recipe JSON 的参数名（legacy 归一化迁移） | 单独立项，本期走内存映射 |
| 实现 Pen A1 / Roll 的具体规则 | 一期只做机制 + Dry；`AllOf` 仅预留结构 |
| 通用表达式引擎 | YAGNI，当前形态用集合匹配即可覆盖 |

## 4. 术语

| 术语 | 含义 |
|---|---|
| 步骤（Step） | Recipe 表中的一个列（`Step 1..N`），每列是一个工艺步骤 |
| 参数（Data） | Recipe 表中的一行（`Recipe.Data`） |
| 单元格 | (参数, 步骤) 组合，落点 = `Recipe.Data.Steps[i]` |
| 可操作（Operable） | 该单元格允许用户编辑 |
| 规则 | `当 SourceParameter 的该步取值 ∈ AllowValues 时，本参数该步可操作` |

## 5. 需求：Dry 规则真值表

`Process Type` 选项：`1: Dry With Arm`、`3: Dry`、`0: Disabled`、`-1: Recipe End`。

「说明」列解析后得到 13 条生效规则（参数名以模板为准）：

| 模板参数名 | 说明原文 | AllowValues |
|---|---|---|
| Process Time | `Process Type=3时允许` | `{3}` |
| Wafer Rotation Speed | `Process Type=1或3时允许` | `{1,3}` |
| IPA Arm Sweep Start Position | `Process Type=1时允许` | `{1}` |
| IPA Arm Sweep End Position | `Process Type=1时允许` | `{1}` |
| IPA Arm Sweep Speed | `Process Type=1时允许` | `{1}` |
| IPA Arm Cycle | `Process Type=1时允许` | `{1}` |
| Dilution N2 Gas Flow | `Process Type=1或3时允许` | `{1,3}` |
| N2 For WF Backside Flow | `Process Type=1或3时允许` | `{1,3}` |
| Outer Rinse Flow | `Process Type=1或3时允许` | `{1,3}` |
| Bubbling N2 Gas Flow | `Process Type=1或3时允许` | `{1,3}` |
| Upper Rinse Supply | `Process Type=1或3时允许` | `{1,3}` |
| Lower Rinse Supply | `Process Type=1或3时允许` | `{1,3}` |
| Wafer Rotation Acceleration | `Process Type=1或3时允许` | `{1,3}` |

特殊行：

| 参数 | 处理 |
|---|---|
| `Process Type` | 说明为空 → **无规则，永远可操作**（它是规则来源，必须可编辑） |
| IPA Arm Sweep Center Velocity / Edge Velocity / 5 个 Reserved | 说明为 `Process Type=2时允许`，但 Excel `不使用=1` → **不渲染**；规则仍登记入表以备后续 Sweep Recipe 区使用 |

**Process Type = 0 (Disabled) / -1 (Recipe End)**：按规格直译 —— 所有带条件的参数不可操作，Tooltip 显示 `Process Type=0` / `Process Type=-1`。不做"整列停用"特判，保持规则可预测。

> 备注：`Process Type=2` 相关的参数在当前表中全部 `不使用=1`，对应 `说明` sheet 中偏移 3000 的 `Sweep Recipe (Reserved)` 区域。因此 Dry 表中实际生效的只有 `{1}`、`{3}`、`{1,3}` 三组。

## 6. 设计

### 6.1 数据模型（`Recipe.cs`）

```csharp
public class Step : BindableBase
{
    // 已有：Id / Value（落盘）
    [JsonIgnore] public bool IsOperable { get; set; } = true;   // 派生态
    [JsonIgnore] public string? OperableReason { get; set; }    // 说明原文，用于 Tooltip
}

public class Data : BindableBase
{
    // 已有：Id / Name / Unit / Type / Min / Max / Default / Options / CompatibleName / Steps
    [JsonIgnore] public OperableRule? OperableRule { get; set; } // 规格元数据
}
```

`IsOperable` 为派生值、`OperableRule` 为规格——两者都**不落盘**，落盘内容与现状 100% 一致。

### 6.2 规则表达（新增 `RecipeOperability.cs`）

```csharp
public sealed record OperableRule(
    string SourceParameter,                 // 例："Process Type"
    IReadOnlyList<string> AllowValues,      // 例：["1","3"]；存选项的数字前缀
    string? Reason = null,                  // 说明原文，供 Tooltip
    IReadOnlyList<OperableRule>? AllOf = null); // 预留：Pen A1 "A=1 且 B=1 时允许"
```

`AllOf` 一期不实现求值分支（模板中无数据），但结构先留，避免 Pen A1 上线时推倒接口。

新增 `RecipeOptionValue` 工具，统一选项取值解析：

- `NumericPrefix("1: Dry With Arm")` → `"1"`
- `NumericPrefix("1")` → `"1"`
- `NumericPrefix("")` / 无法解析 → `null`

该工具同时替换现有散落实现（`GetRealVal` 拆 `:`、`OnTreeViewNodeChanged` 拆 `:`、`OnAddStep` 用 `StartsWith`）。

### 6.3 求值引擎（新增 `RecipeOperabilityResolver.cs`）

```
Recalculate(Recipe.Content content) -> void
  1. 建索引：Normalize(name) -> Data
       Normalize = 去除 空格 / _ / - / &  （与 GetRealVal 的归一化完全一致）
       同时把 Data.CompatibleName 中的每个名字也指向同一 Data
  2. 对每个带 OperableRule 的 Data：
       若 SourceParameter 不存在 → 该参数所有步 IsOperable = true（fail-open）
       对每个 step i：
         target = data.Steps[i]
         若 i >= source.Steps.Count → fail-open（true）
         否则 v = NumericPrefix(source.Steps[i].Value)
              IsOperable = (v != null && AllowValues.Contains(v))
              OperableReason = IsOperable ? null : Reason
```

`OperableReason` **仅在不可操作时写入**（`IsOperable == true` 时为 `null`），避免每个可编辑单元格都弹出无意义的 ToolTip。

**fail-open 原则**：任何解析失败、参数缺失、步数不齐都视为「可操作」。一条脏数据不得把整表锁成只读。

### 6.4 元数据挂载（新增 `RecipeTemplateRegistry.cs`）

问题：`OnTreeViewNodeChanged`（`RecipeEditorViewModel.cs:671`）直接从 JSON 反序列化，**不经过模板**。而现存 Recipe 全部使用 legacy 参数名（见附录 B），规则若只写在模板里，存量文件将拿不到规则。

解决：

```csharp
RecipeTemplateRegistry.Resolve(string folderName)                    // 文件夹名 -> 模板，未注册返回 null
RecipeTemplateRegistry.Decorate(Recipe.Content content, string folderName)  // 按归一化名把规则贴回行上
```

- 依据：本工程已有的「文件夹名 = Recipe 类型」约定（`Chamber.ResolveUnitRecipePath`、`SequenceRecipeEditorViewModel` 的目录常量）。
- 匹配：`Normalize(Name)` 或 `CompatibleName` 命中，**不使用 Id / 下标**（模板 Id 与 Excel Index 不一致，见附录 B）。
- 未注册的文件夹：`Decorate` 不做任何事（fail-open）并写一条 `Logger.Warn`。
- 附带收益（可选、可延后）：`OnTreeAdd`（`RecipeEditorViewModel.cs:825-873`）的 15 分支 switch 可收敛为 `Registry.Resolve` + 模板查询。

### 6.5 触发时机（必须全覆盖）

| # | 时机 | 位置 |
|---|---|---|
| 1 | Recipe 载入 | `OnTreeViewNodeChanged` 反序列化 + `Decorate` 之后 |
| 2 | `Process Type` 单元格被编辑 | 订阅 `Process Type` 行每个 `Step` 的 `PropertyChanged`（`WeakEventManager`；载入/重建列时重建订阅） |
| 3 | 加步 / 删步 / 粘贴步 / 新步 | `OnAddStep`、`OnDeleteStep`、`OnPasteStep`、`OnNewStep` |
| 4 | 步骤重编号 | `ReFillStepData` |
| 5 | **列重排** | `OnColumnReordered:174`（Step 的 Value 随列交换，必须重算） |
| 6 | 保存前 | `OnSaveRecipe`（保证落盘与界面一致；本期不改数据，仅重算状态） |

重算只写 `Step.IsOperable` / `OperableReason`，**不重建列**（绑定自动刷新）；只有列结构性变化才调用 `RefreshEditor`。

### 6.6 视图层

1. `RecipeCellTemplateSelector.SelectTemplate`（`RecipeEditorViewModel.cs:1438`）已持有 `_stepIndex`，三种模板（ComboBox / NumericUpDown / TextBox）统一追加：
   - `factory.SetBinding(UIElement.IsEnabledProperty, new Binding($"Steps[{_stepIndex}].IsOperable"))`
   - `factory.SetBinding(FrameworkElement.ToolTipProperty, new Binding($"Steps[{_stepIndex}].OperableReason"))`
2. `RefreshEditor`（`RecipeEditorViewModel.cs:1291`，唯一知道 `stepIndex` 的建列点）为每列构造 `CellStyle` 并加 `DataTrigger`：绑定 `Steps[stepIndex].IsOperable == False` → `Background = #F0F0F0`、`Foreground = Gray`。
   - 现有模板已把控件 `Background` 绑到 `DataGridCell.Background`，因此只改 Cell 背景即可，无需重复写控件样式。
3. 键盘路径验证：`IsEnabled=False` 后 Tab / 方向键 / 滚轮不得改变值；如发现可绕过，在 `PreviewKeyDown` 拦截。
4. Tooltip 文案直接使用 Excel「说明」原文（如 `Process Type=1时允许`），**不做本地化**——它是规格引文，必须与 Excel 逐字一致，以便对账测试守住。

### 6.7 不改动的部分（显式声明）

- `Steps[i].Value` 任何情况下不被本功能修改；
- `CheckRecipeDefualtValue`、`WriteUnitRecipeParameterChangeAudit`、`SaveUnitRecipes` 行为不变；
- `DryTabContentViewModel.AssignDryRecipe:319-338` 全量下发逻辑不变 → **灰显不代表不下发**，不可操作单元格的历史值仍会写入 PLC。

## 7. 不变式（测试须守住）

1. `IsOperable` / `OperableReason` / `OperableRule` **永不落盘**（`[JsonIgnore]`）。
2. 规则**只按参数名**匹配（归一化 + `CompatibleName`），**永不使用 Id 或下标**。
3. 任何解析失败 / 参数缺失 / 步数不齐 → **fail-open**。
4. `IsOperable` 只驱动 UI，不参与下发、不参与审计、不修改业务数据。
5. 重算不重建列；只有列结构性变化才 `RefreshEditor`。
6. 规则真源 = 模板 = Excel，由对账测试守住一致性。

## 8. 兼容与迁移

| 场景 | 处理 |
|---|---|
| 存量 JSON 无新字段 | `[JsonIgnore]` 字段默认 `true` → 天然兼容 |
| 存量 JSON 使用 legacy 参数名 | `Decorate` 经 `CompatibleName` 内存映射，**不改文件** |
| 文件夹未注册模板 | fail-open + `Logger.Warn` |
| 规则引用的参数在某个 Recipe 中不存在 | 该规则不生效，其余规则正常 |
| 用户在 A 文件夹里放 B 模板的配方 | `Decorate` 按文件夹判定 → 规则不生效（fail-open）。本期接受，已在风险表登记 |

## 9. 测试计划（`CMP.Tests`）

| # | 用例 | 断言 |
|---|---|---|
| 1 | `Resolver` 真值表：13 规则 × PT ∈ {1,3,2,0,-1} | 逐格匹配规格（65 组） |
| 2 | `Process Type` 自身 | 恒可操作 |
| 3 | `Process Type` 行缺失 | 全部 fail-open |
| 4 | 步数不齐（PT 3 步 / 目标 1 步） | 越界步 fail-open，现有步正常求值 |
| 5 | legacy 名 `Chuck Rotate Velocity` | 命中 `Wafer Rotation Speed` 规则 |
| 6 | 未知参数名 `Pen Arm  Cycle` | 不参与、不抛异常 |
| 7 | `RecipeOptionValue.NumericPrefix` | `"1: Dry With Arm"→"1"`、`"1"→"1"`、`""→null`、`"abc"→null` |
| 8 | `Registry` 模板 | Id 唯一、Name 归一化后唯一 |
| 9 | `Registry` 规则引用 | `SourceParameter` 必须存在于同一模板 |
| 10 | `Registry.Decorate` legacy 名回贴 | 规则落到 legacy 行上 |
| 11 | `Registry.Decorate` 未知文件夹 | 不抛异常、不改数据 |
| 12 | **Excel 对账** | `DryRecipeTemplate` 规则集合 == `dry-operability-fixture.json`（含 `AllowValues` 与说明原文） |
| 13 | 序列化 | `Recipe.Content` 序列化结果不含 `IsOperable` / `OperableReason` / `OperableRule` |
| 14 | 回归 | 载入 `111.json` 形态（legacy 15 行）规则正确落位 |

夹具 `CMP.Tests/Resources/dry-operability-fixture.json` 由 Excel「说明」列导出，工艺改 Excel 而忘记改代码时测试变红。

## 10. 验收标准

**自动化**：第 9 节 14 项测试全绿；`dotnet build` 无新增警告。

**手工（UI）**：
1. 打开 Dry Recipe，改某步 `Process Type` → 同行其余单元格按真值表**立即**灰显/恢复，无需重开页面；
2. 灰显单元格鼠标悬停显示 `Process Type=N时允许`；
3. 灰显单元格无法通过鼠标/键盘修改；
4. 列重排、加步、删步、粘贴、另存之后状态依然正确；
5. 保存后 JSON diff **只有**步骤值（无新字段）；
6. 载入现场 legacy 命名的 Dry Recipe，规则正确生效。

## 11. 风险与对策

| 风险 | 对策 |
|---|---|
| `PropertyChanged` 订阅泄漏 | `WeakEventManager`；载入 / 重建列时统一退订 |
| 列重排后下标语义变化 | 只按当前 `Steps` 列表下标求值，不缓存下标 |
| legacy 名匹配不全 | 复用 `CompatibleName`；用真实 `111.json` 做回归夹具 |
| 脏数据把表锁死 | fail-open 不变式 + 专项单测 |
| 新字段污染 JSON | `[JsonIgnore]` + 序列化断言测试 |
| 模板 Id 与 Excel Index 错位 | 规则只按名匹配 + Registry 一致性测试 |
| 文件夹与模板类型不匹配 | fail-open + `Logger.Warn`；本期接受 |
| 灰显被误解为"不下发" | 规格 6.7 显式声明；后续若要过滤，单独立项 |

## 12. 实施顺序

1. `RecipeOptionValue`（前缀解析）——纯函数，先写测试
2. `Recipe.cs` 加三个 `[JsonIgnore]` 属性
3. `RecipeTemplateRegistry` + `Decorate`
4. `DryRecipeTemplate` 补全 13 条规则（+ 7 条预留）
5. `RecipeOperabilityResolver`
6. `RecipeEditorViewModel` 接线（载入 / 订阅 / 6 个触发点 / `CellStyle`）
7. `RecipeCellTemplateSelector` 加 `IsEnabled` + `ToolTip`
8. 测试与 Excel 对账夹具

---

## 附录 A：Excel「说明」列原始数据（Dry A_For_Recipe_Rev01.xls / Recipe_Dry A / AG 列）

| Excel Index | 参数名 | 不使用 | 说明 |
|---|---|---|---|
| 1 | Process Type | 0 | （空） |
| 2 | Reserved | 1 | （空） |
| 3 | Process Time | 0 | Process Type=3时允许 |
| 4 | Wafer Rotation Speed | 0 | Process Type=1或3时允许 |
| 5 | IPA Arm Sweep Start Position | 0 | Process Type=1时允许 |
| 6 | Reserved | 1 | （空） |
| 7 | IPA Arm Sweep End Position | 0 | Process Type=1时允许 |
| 8 | Reserved | 1 | （空） |
| 9 | IPA Arm Sweep Center Velocity | 1 | Process Type=2时允许 |
| 10 | IPA Arm Sweep Edge Velocity | 1 | Process Type=2时允许 |
| 11 | IPA Arm Sweep Speed | 0 | Process Type=1时允许 |
| 12 | IPA Arm Cycle | 0 | Process Type=1时允许 |
| 13–17 | Reserved | 1 | Process Type=2时允许 |
| 18 | Dilution N2 Gas Flow | 0 | Process Type=1或3时允许 |
| 19 | N2 For WF Backside Flow | 0 | Process Type=1或3时允许 |
| 20 | Outer Rinse Flow | 0 | Process Type=1或3时允许 |
| 21 | Bubbling N2 Gas Flow | 0 | Process Type=1或3时允许 |
| 22 | Upper Rinse Supply | 0 | Process Type=1或3时允许 |
| 23 | Lower Rinse Supply | 0 | Process Type=1或3时允许 |
| 24 | Wafer Rotation Acceleration | 0 | Process Type=1或3时允许 |

## 附录 B：勘察发现（实现时必须尊重的既存事实）

1. **模板 Id ≠ Excel Index**：`DryRecipeTemplate.cs` 中 `Id=6` 是 `IPA Arm Sweep Speed`（Excel Index=11），因为 Excel 的 6/7 在模板中被注释掉。→ 规则键必须是**参数名**。
2. **现场 Dry Recipe 使用 legacy 参数名且只有 15 行**（实测 `D:\CMP\Configuration\Recipes\UnitRecipe\Dry Recipe\`）：`Chuck Rotate Velocity`、`WF Upper Face N2 Flow`、`WF Lower Face N2 Flow 1`、`WF Upper Face DIW Flow 2`、`WF Upper Face IPA Supply`、`WF Lower Face N2 Supply 2`、`WF Upper Face DIW Supply 1`、`WF Lower Face DIW Supply`。模板通过 `CompatibleName` 与它们对应，PLC 侧 `GetRealVal` 已使用该机制，编辑器侧**尚未**。
3. **既存数据问题**：`Dry A1_For_Recipe_Rev01.json` 第 7 行名为 `Pen Arm  Cycle`（应为 `IPA Arm  Cycle`），按名匹配不到任何模板参数，PLC 侧静默取 `default`=0。本期不修，登记为后续数据迁移任务。
4. **`OnAddStep` 的默认值匹配**用 `o.StartsWith(data.Default)`，`OnTreeViewNodeChanged` 拆 `":"` 取后半段，`GetRealVal` 拆 `":"` 取前半段——三处选项解析语义不一致，统一收敛到 `RecipeOptionValue`。
5. **`说明` 列不是 ProcessType 专用**：`Pen A1` 含 `Process Type=1或5时允许` 与 AND 组合条件（`WF Upper Face Chemical Supply=1 且 WF Lower Face Chemical Supply=1`），`Roll A1` 含 `Process Type=1或2或3时允许`。本期机制按通用形态设计，实现只落 Dry。
