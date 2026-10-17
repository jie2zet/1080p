# Recipe 参数可操作性（说明列）设计规格

- 日期：2026-10-14
- 分支：`feat/RecipeEdit-StepOperability`
- 规格来源：`Dry A_For_Recipe_Rev01.xls` / `Pen A1_For_Recipe_Rev01.xls` / `Roll A1_For_Recipe_Rev01.xls` 的「说明」列（AG 列）
- 涉及模块：`CMP/Models/Recipe/RecipeEdit/**`、`CMP/ViewModels/Tabs/RecipeTab/RecipeEditorViewModel.cs`、`CMP.Tests`

## 0. 修订记录

### Rev13（Sweep 文件夹拆分 / Retry 选项与文件夹解耦 / 参数名→文件夹显式映射）

| # | 变更 | 依据 |
|---|---|---|
| 1 | `ConfigViewModel` **删除** `string folderName = item.Name.Replace("Retry ", "");`，改为直接 `LoadRecipesFromFolder(item.Name)` | Retry 已各有文件夹，「与非 Retry 文件夹共用列表」的前提不再成立；启动路径（同文件 L888）本来就是 `item.Name`，两处入口现已一致。**Rev12 的遗留项闭环** |
| 2 | `RecipeTemplateRegistry.ConditionSweepRecipeFolder`（`"Condition Sweep Recipe"`）拆为 **`InsituConditionSweepRecipeFolder`（`"InsituCondition Sweep Recipe"`）** 与 **`ExsituConditionSweepRecipeFolder`（`"ExsituCondition Sweep Recipe"`）** → 已注册文件夹 **14 → 15** | 两个 sweep 已各有独立模板（Rev05 拆分）。拆分前同一文件夹被两处指向**不同**模板：注册表 → `InsituContidionSweepRecipeTemplate`，`AddRecipeEditorViewModel` → `ExsituContidionSweepRecipeTemplate` —— 装饰与新建必然打架 |
| 3 | 现场目录同步：新建上述两个目录；`Condition Sweep Recipe\Recipe_ExsituCondition Sweep.json` 移入 `ExsituCondition Sweep Recipe`；删除原 `Condition Sweep Recipe` 目录 | 该文件是 ex-situ 形态的 sweep 配方，归属 Exsitu；insitu 目录暂空（现场尚无 insitu sweep 配方） |
| 4 | `AssignConditionSweepRecipe(ref list, name, "Insitu"/"Exsitu")` 拆为 **`AssignInsituConditionSweepRecipe`** 与 **`AssignExsituConditionSweepRecipe`**（均无 `type` 参数），各自硬编码自己的文件夹常量与 KST 结构体；Growl 文案带上形态前缀 | 不再用字符串参数辨别形态 —— 形态、文件夹、结构体三者一一对应，由编译器保证 |
| 5 | `RecipeEditorViewModel` 的参数名→文件夹判断改为显式映射 `ResolveRecipeFolderForParameter(parameterName, ownerFolder)` | Rev12 把常量值从 `"Condition Recipe"` 改为 `"InsituCondition Recipe"` 后，`folderName == 常量` 对参数名 `Condition Recipe` **永不成立**（参数名是 Excel 字面，不能改）→ 编译错误 + 该行下拉列表取不到文件。显式映射同时解决 `Condition Sweep Recipe` 在两个 Condition 模板里同名、需按所属文件夹分流的问题 |
| 6 | 测试同步：`AllTemplates` / 夹具 DataRow 的模板标签改用真实文件夹名（`InsituCondition Recipe`、`InsituCondition Sweep Recipe`、`ExsituCondition Sweep Recipe`）；`RegisteredFoldersMatchTheDeployedRecipeFolders` 期望集合 14 → 15；`ResolveReturnsTemplateForKnownFolder`、`SingleConditionRulesFollowModeParameter` 各增 1 个 sweep 文件夹 | 模板标签与注册表一一对应；新增文件夹必须被解析测试覆盖 |

**行为差异（需知晓）**：
- Config 页 `Load/UnLoad/Dechuck Retry` 三个 Enum 项的选项列表改为来自**各自的** Retry 文件夹（现场各 1/1/2 个配方），不再借用非 Retry 文件夹 —— 这是本次的**意图**（Rev12 曾把它标为「待工艺确认」）。
- `InsituCondition Recipe\4545tg.json` 的 `Condition Sweep Recipe` 行 `Options` 里残留历史值 `Recipe_ExsituCondition Sweep`，其选中值为 `None` → 不影响下发；编辑器重新载入时该行选项会按 insitu sweep 文件夹重写。
- Insitu 侧暂无可选 sweep 配方：insitu 下发时若引用了不存在的名字，仍走既有 fail-open（Growl `Cannot Find Insitu Condition Sweep Recipe`，不写该子结构、返回下标）。

### Rev12（Recipe 文件夹名统一走注册表常量）

| # | 变更 | 依据 |
|---|---|---|
| 1 | **代码中所有 Recipe 文件夹名一律引用 `RecipeTemplateRegistry` 常量**（替换 99 处）：`PolishUnitTabContentViewModel`(42+3) / `ConfigViewModel`(18) / `AddRecipeEditorViewModel`(14) / `Chamber.cs`(4) / `DryTabContentViewModel`(5) / `GenericRollerTabContentViewModel`(5) / `PenCleanTabContentViewModel`(5) / `RecipeEditorViewModel`(3) | 单一真源；避免各处字面量漂移 |
| 2 | **排除项**：三个模板文件里的 `Name = "Head Sweep Recipe"` / `"Condition Recipe"` / `"Condition Sweep Recipe"` 是**参数名**而非文件夹名；`MessageEvents.cs` 的 1 处为注释 | 误改会破坏模板 |
| 3 | 顺带修复：**3 个 Retry 加载点指向非 Retry 文件夹**（列表侧已改为 Retry 文件夹，加载侧仍是旧的 → 下拉列出 `H6H` 却去 `Load Recipe\H6H.json` 找不到，**下发不生效**） | 改用 `LoadRetryRecipeFolder` / `UnloadRetryRecipeFolder` / `DechuckRetryRecipeFolder`；已核对三份 Retry 配方参数结构与目标 KST 结构体完全匹配 |
| 4 | 顺带修复：第二个事件 switch 的 `case "Unload Recipe"`（小写 l）与实际文件夹名 `"UnLoad Recipe"` 不匹配 → 该分支永不命中、保存后列表不刷新 | 改用常量后自然对齐 |

**仍未统一（需工艺确认）**：`ConfigViewModel` 的 `item.Name.Replace("Retry ", "")` **有意**让 Config 页 Retry 选项列出非 Retry 文件夹的配方 —— 与 Polish 页现在列真实 Retry 配方的做法不一致，属功能语义选择，未擅自改动。**（Rev13 已按工艺要求删除该行：Retry 各用各自文件夹。）**

### Rev11（移除冗余的 `Head Recipe` 文件夹）

| # | 变更 | 依据 |
|---|---|---|
| 1 | `RecipeTemplateRegistry` 删除 `HeadRecipeFolder` 常量与对应的 `Factories` 映射 → 已注册文件夹 **15 → 14** | `Head Recipe` 与 `Head Sweep Recipe` 重复：两处指向同一 `HeadSweepRecipeTemplate`，且现场配方（`Recipe_Head Sweep.json`）已并入 `Head Sweep Recipe`，`Head Recipe` 目录已不存在 |
| 2 | 测试同步：`ResolveReturnsTemplateForKnownFolder` 移除该 DataRow；`RegisteredFoldersMatchTheDeployedRecipeFolders` 的期望集合移除 `"Head Recipe"` | 同上 |

**为什么必须改代码（不只是删目录）**：Rev10 的 `EnsureFoldersExist` 会按 `RegisteredFolders` 在启动时补齐缺失目录 —— 若不移除这一项，**每次启动都会把已删掉的空 `Head Recipe` 目录重新建出来**。

**影响**：不再为 `Head Recipe` 目录做模板装饰（`OperableRule` / `IsUsed`）。因该目录已空且不再创建，无实际影响。

### Rev10（新设备首启时补齐 Recipe 文件夹）

其它设备首次运行时 `D:\CMP\Configuration\Recipes\UnitRecipe\` 下**一个子目录都没有**，导致各 Recipe 类型的配方既无法新建也无法读取。

| # | 变更 | 依据 |
|---|---|---|
| 1 | `RecipeTemplateRegistry` 新增 **`RegisteredFolders`**（14 个文件夹名的只读快照，来自 `Factories` 的键）与 **`EnsureFoldersExist(basePath)`**（幂等补齐，返回本次新建的名字） | 文件夹清单必须以注册表为**单一真源**；这样以后新增模板会自动带上对应目录，不会漏建。**Rev11 从 15 减为 14**（移除冗余的 `Head Recipe`） |
| 2 | `RecipeEditorViewModel` 新增 `EnsureRecipeFolders()`，并在 **`InitRecipesData()` 的第一行**调用 | 满足「初始化 Recipe 数据之前」；失败只记日志、不阻断编辑器启动（界面表现为列表为空，日志可定位） |

**行为**：`EnsureFoldersExist` 先建根目录，再逐个 `Directory.CreateDirectory`（对已存在目录幂等）；返回集合只含**本次新建**的，VM 在有新建时写一条 `Info` 日志（含新建清单）。

**测试**：
- `RegisteredFoldersMatchTheDeployedRecipeFolders` —— 清单与现场 15 个目录**集合相等**，且每个都能 `Resolve` 到模板（防止漏注册导致该类型在新设备上不可用）。
- `EnsureFoldersExistCreatesEveryRegisteredFolderIdempotently` —— 用临时目录验证「全部建出 → 二次调用返回 0 → 空/空白路径安全返回」，用后清理。

### Rev09（Polish 页 ExsituCondition 下拉框分离）

Insitu 与 Exsitu 的 Condition 配方已分文件夹创建，界面上那一行的下拉框需跟着分开。

| # | 变更 | 依据 |
|---|---|---|
| 1 | `PolishUnitTabContentView.xaml:102-103` 的 ComboBox 由 `ConditionRecipes` / `SelectedConditionRecipe` 改为 **`ExsituConditionRecipes` / `SelectedExsituConditionRecipe`** | 该行按钮是 `ExsituConditionCommand`（BreakIn），且 `BreakInCommandExecute` 从 `ExsituCondition Recipe` 文件夹加载、自动流程取 Sequence 键 `RecipePolish?Exsitu` —— 列表原先却来自 `Condition Recipe`（in-situ），**列表与加载指向不同文件夹**（列出 in-situ 名字、执行时去 Exsitu 目录找 → 只能弹 "file not found"） |
| 2 | VM 新增 `ExsituConditionRecipes`（从 `ExsituCondition Recipe` 加载）；`SelectedConditionRecipe` **改名为** `SelectedExsituConditionRecipe` | 该属性的真实消费者一直是 Exsitu/BreakIn 路径 |
| 3 | `BreakInCommandExecute`（5 处）、`AssignWaferToPLC`、初始化加载、Recipe 保存事件的 `case "ExsituCondition Recipe"` 同步 | 见上 |

**保留未动**：`ConditionRecipes` / `SelectedPreCondRecipe`（服务于 XAML 中 `Visibility="Hidden"` 的 `Pre_Cond` 行）；`AssignConditionRecipe()`（解析 Polish/Exsitu 配方内 `Condition Recipe` 子配方引用，从 `Condition Recipe` 取，语义正确）。改后全仓库已无 `SelectedConditionRecipe` 引用。

**未改但需注意**：该行 TextBlock 文案仍为 `{lex:Loc Condition}`，而其内容是 Exsitu 配方、按钮为 BreakIn；若要改为 "Exsitu Condition" 需新增本地化键。

### Rev08（把 `IsUsed` 推广到 PLC 下发路径）

工艺在 `PolishUnitTabContentViewModel.GetRealVal` 中加了 `if (model == null || !model.IsUsed) return default;`，使**不使用的参数下发 0**。本次将其推广到全部下发路径。

| # | 变更 | 依据 |
|---|---|---|
| 1 | **5 处 `GetRealVal` 统一加 `!model.IsUsed` 守卫**：`DryTabContentViewModel` / `GenericRollerTabContentViewModel` / `PenCleanTabContentViewModel` / `PolishUnitTabContentViewModel` / `LoadPortRecipeContentViewModel` | `IsUsed=false` → 返回 `default(T)`（数值即 0） |
| 2 | **⚠️ 关键补充：4 处 `LoadRecipeContent` + LoadPort 的加载点补调 `RecipeTemplateRegistry.Decorate(content, folderName)`** | `IsUsed` 是 `[JsonIgnore]` 的，**只有 `Decorate` 会填充它**。此前这些 Operation Tab 从不调用 `Decorate` → `IsUsed` 恒为 `true` → **工艺加的那个守卫是死代码**。补上后守卫才真正生效。`Decorate` 同时会贴上 `OperableRule`（在下发路径无副作用） |
| 3 | **5 处脆弱的 `value.Split(':')` 统一改用 `RecipeOptionValue`**：数值目标用 `NumericPart()`、字符串目标用 `LabelPart()`；新增这两个方法 | 原实现各处重复且只认半角冒号；且工艺在 Polish 里写的 `value.Replace("：",":")` **是无效语句**（返回值未赋回），已由 `NumericPart`/`LabelPart` 正确覆盖。行为等价性：`arr[0]`→`NumericPart`、`arr[1]`→`LabelPart`，仅有「全角冒号可解析」与「标签去空白」两处改进 |
| 4 | `LoadPortRecipeContentViewModel` 的加载点用 `Path.GetFileName(Path.GetDirectoryName(路径))` 反推文件夹名 | 该处直接持有绝对路径，没有现成的文件夹名 |

**语义确认**：`default(T)` 对 `short`/`float`/`int` 都是 0 ✓。字符串目标的参数（`HeadSweepRecipe` / `ConditionRecipe` / `ConditionSweepRecipe`）当前**均为 `IsUsed = true`**，故不会返回 null。

**附带发现（既存问题，未改）**

| 问题 | 证据 | 影响 |
|---|---|---|
| **Retry 的下拉列表与加载器都指向非 Retry 文件夹** | `case "Load Retry Recipe": LoadRecipesFromFolder("Load Recipe", LoadRetryRecipes)`；`AssignLoadRetryRecipe()` 里 `LoadRecipeContent("Load Recipe", ...)`。`Dechuck Retry Recipe` 同理指向 `Dechuck Recipe` | 列表与加载器**自洽**（不会报"文件未找到"），但实际 Retry 配方（`Load Retry Recipe/H6H.json`、`UnLoad Retry Recipe/UTUTU.json`、`Dechuck Retry Recipe/111.json,112.json`）**永远选不到**，而常规配方会被写进 Retry 的 PLC 结构体。修改会影响「下发哪份配方」，故未擅自改动，留待工艺确认 |
| `Unload Recipe`（小写 L）与注册表的 `UnLoad Recipe` 大小写不一致 | `LoadRecipeContent("Unload Recipe", ...)` / 注册表常量 `"UnLoad Recipe"` | `RecipeTemplateRegistry.Resolve` 用 `OrdinalIgnoreCase` → **可正常解析**，无需改 |
| 各 Operation Tab 的 Retry 与非 Retry 模板在参数名/规则/IsUsed 上当前完全等价 | 对比 `LoadRecipeTemplate` 与 `LoadRetryRecipeTemplate` 等 | 上述文件夹错位目前不产生规则差异；若将来表格分化则会静默取错模板 |

### Rev07（全角冒号加固）

工艺提醒：Excel 由人工维护，中文输入法下容易把半角 `:` 打成全角 `：`(U+FF1A)，会导致选项解析静默失败（门控不生效 / 下发取值落空）。

**扫描结论（4 份表格 + 现场 44 个配方 JSON）**

| 位置 | 全角冒号 `：` | 说明 |
|---|---|---|
| `Dry` / `Pen` / `Roll` / `Polish` 四份表格全部单元格 | **仅 2 处，都在中文说明文字里**（`一 区域说明：`、修订履历 `选项2：Close`） | 所有「选择项」单元格用的都是**半角**冒号 |
| 现场 44 个 UnitRecipe JSON 的 `Options` / `Default` / `Steps` | **0 处** | 数据未被污染 |

问题当前**未发生**，但风险真实（那 2 处中文单元格正说明中文输入法确实会打出全角冒号）。已做两层防护：

| 层 | 措施 |
|---|---|
| **解析层** | `RecipeOptionValue.NumericPrefix` 的分隔符由「仅 `:`」放宽为 `:` `/ ：`(U+FF1A) `/ ﹕`(U+FE55) `/ ∶`(U+2236)；新增 `PartAfterSeparator()` 提供「取分隔符之后部分」的统一实现 |
| **规格源头** | `RecipeEditorViewModel.OnTreeViewNodeChanged` 里原有的 `item.Value.Split(":")` 改用 `RecipeOptionValue.PartAfterSeparator()`（同一 bug 类的第二处） |
| **护栏测试** | `TemplateOptionsAndDefaultsUseHalfwidthColonsOnly`：断言**所有模板的 `Options` 与 `Default` 只用半角冒号** —— 因为 PLC 侧 `GetRealVal` 只按半角拆分，模板一旦混入全角，下发会取不到值。该测试把问题挡在「表格 → 模板」这一道边界上 |

**顺带查出的数据录入隐患（均已确认当前转写正确，无需改）**

| 现象 | 位置 | 现状 |
|---|---|---|
| 英文选择项用**全角分号 `；`** 分隔 | `Pen A1` 的 `Pen Arm Sweep Start/End Position`（`1:Start Edge；2:Center；3:End Edge`）、`WF Chemical Ratio`（`0:DIW； 1:Chemical；>1:MIX；`） | 模板的 `Options` 数组已正确拆开 ✓。后续若做机械转写，**必须同时按 `;` 和 `；` 拆分** |
| 选项列表被省略号缩略 | `Polish Recipe` 的 `Jump Step No.`：`1:Step1;…;……25:Step25;26:Step End;-1:Not Set` | 模板已展开为完整的 26 项 ✓ |
| 非纯数字选项前缀（`>1:MIX`） | `Pen A1` 的 `WF Chemical Ratio` | 该参数在模板中是 `float` 且无 `Options` ✓；`NumericPrefix(">1:MIX")` 返回 null → fail-open（不影响） |
| 「初期值」列 | 全部表格 | 无非空非数值内容 ✓（数值对账可靠） |

**残留暴露（未改，已在 §3 非目标中登记）**：PLC 下发路径的 `GetRealVal`（`DryTabContentViewModel` 等 5 个 Operation VM）仍只按半角 `:` 拆分。当前数据无全角冒号，且护栏测试已挡住「表格混入全角」的入口；若将来需要加固，属独立任务（会动下发路径）。

### Rev06（「不使用」行隐藏）

新增行级可见性：Excel「不使用」列 = 1 的参数行**在界面上不显示**。

| # | 变更 | 依据 |
|---|---|---|
| 1 | **`Recipe.Data` 新增 `IsUsed`**（bool，默认 `true`，`[JsonIgnore]`）；`Decorate` 一并把模板的 `IsUsed` 贴到已载入的行上 | 与 `OperableRule` 同属**规格元数据**（真源 = Excel「不使用」列），不落盘 |
| 2 | **界面隐藏方式：给 DataGrid 绑定集合的默认 `CollectionView` 设过滤谓词**（`RecipeRowVisibility.Filter`），在 `RefreshEditor` 里调用 `ApplyRowVisibility()` | 不改 XAML、不覆盖 HandyControl 的 `DataGridRow` 样式；`RecipeData.Data` 本身不被改动 → **隐藏行的数据仍保留、照常落盘与下发**（PLC 侧对「不使用」参数的处理不变） |
| 3 | **29 个模板行标 `IsUsed = false`**（9 个模板） | 各 sheet 的「不使用」列 |
| 4 | **全部 356 个模板行都显式写出 `IsUsed`**（`true` 327 / `false` 29） | 代码显式镜像表格，避免"默认值隐式生效"；后续表格改动只需翻转该标记，不必再改结构 |

**无例外，严格照表**：`Condition Recipe_Polish A` 把 `Process Type` 也标了 `不使用=1`，早期实现曾以「模式参数隐藏后无法切换模式」为由保留显示 —— **现已按工艺要求严格执行表格，`InsituConditionRecipeTemplate.Process Type` 的 `IsUsed = false`**。代价是 Insitu Condition 配方编辑器只显示 6 行（`Process Type` 不可见）、无法在编辑器内切换模式；若这不符合工艺意图，应**修订表格**而非在代码里加特例。夹具已删除 `isUsedNote` 例外机制，`TemplateIsUsedMatchesExcelNotUsed` 现在是无例外的严格校验。

**逐模板隐藏清单**

| 模板 | 隐藏的参数 |
|---|---|
| `DryRecipeTemplate` | IPA Arm Sweep Center Velocity、IPA Arm Sweep Edge Velocity |
| `PenA1RecipeTemplate` | WF Upper Face N2 Flow、WF Lower Face N2 Supply、WF Upper Face DI-CO2 Supply |
| `RollRecipeTemplate` | Mega Sonic Supply |
| `LoadRecipeTemplate` / `LoadRetryRecipeTemplate` | Head Middle Wash、Head External Wash |
| `UnloadRecipeTemplate` / `UnloadRetryRecipeTemplate` | Head Middle Wash、Head External Wash、Head Rotation |
| `InsituConditionRecipeTemplate` | **Process Type**、InSitu、Platen Rotation、Platen Wash Selection、Slurry Line 1/2/3 Type、Slurry Line 1/2/3 Flow（10 条） |
| `ExsituConditionRecipeTemplate` | InSitu、Sweep Stop Time Type、Ratio |
| Polish / Head Sweep / Dechuck / DechuckRetry / Condition Sweep / ExsituCondition Sweep | 无（其 `不使用=1` 行都不在模板中：`Reserve`、`Zone 21-45 Ratio` 等） |

**连带调整**：Dry / Pen / Roll 三个模板原先被注释掉的 `不使用=1` 行已由用户放开成为正式行（Dry +2、Pen +3、Roll +1）—— 正是本特性要隐藏的对象。放开注释的用意是让**表格成为唯一真源**：以后表格里某个参数在「使用/不使用」之间切换，只需改 `IsUsed` 的值，不必反复增删代码行。

**对账断言调整**：`TemplateRulesMatchExcelFixture` 原先的第 3 条（「`不使用=1` 且模板存在且带条件 → 必须挂规则」）**已删除** —— 隐藏行谈不上可操作性，该要求在新语义下不成立；其职责由新增的 `TemplateIsUsedMatchesExcelNotUsed` 承接。

### Rev05（Condition 模板拆分）

`ConditionRecipeTemplate` 被拆成两个独立模板，不再共用：

| # | 变更 | 依据 |
|---|---|---|
| 1 | **`ConditionRecipeTemplate` → 重命名为 `InsituConditionRecipeTemplate`**（`Condition Recipe` 文件夹），**移除全部 15 条规则** | `Condition Recipe_Polish A` 的「显示条件说明」列**全为空** → 该 sheet 不产生任何规则 |
| 2 | **新建 `ExsituConditionRecipeTemplate`**（`ExsituCondition Recipe` 文件夹），**15 条规则允许值由 `{1,25}` 收回为字面 `{25}`** | Rev04 的 `{1,25}` 并集是为「两 sheet 共用一模板」做的折中；拆分后不再需要，直接按 `ExsitCondition Recipe_Polish A` 字面实现 |
| 3 | `RecipeModelTemplate` 枚举与 `AddRecipeEditorViewModel` / `RecipeEditorViewModel.OnTreeAdd` / `RecipeTemplateRegistry` 同步 | 见下 |
| 4 | **两个模板的 Process Type 选项按各自 sheet 的「选择项（英文）」设置**：`InsituConditionRecipeTemplate` = `["1:Condition", "2:None"]`；`ExsituConditionRecipeTemplate` = `["25:Condition", "-1:Recipe End"]`（`Default` 相应为 `"1"` / `"25"`） | `Condition Recipe_Polish A` 的选择项原文 `1: Condition; 2:  None`；`ExsitCondition Recipe_Polish A` 为 `25: Condition; -1:Recipe End`。现场 `Condition Recipe_Polish A.json` 本就使用 `["1:Condition","2:None"]`，属**对齐**而非偏离。**存量配方不受影响**：`Decorate` 只贴 `OperableRule`、不碰 `Options`，`CheckRecipeDefualtValue` 读的是配方自带的 `Options`，因此仅影响新建配方 |
| 5 | **逐行核对并修正 Min / Max / Default**（8 个字段） | 见下表 |

**Rev05 数值修正明细（InsituConditionRecipeTemplate，依据 `Condition Recipe_Polish A`）**

| 参数 | 项目 | 原值 | 改为 |
|---|---|---|---|
| Process Time | `Default` | `"100.00"` | `"5.00"` |
| Platen Rotation | `Max` | `180.00` | `30.00` |
| Platen Rotation | `Default` | `"10.00"` | `"30.00"` |
| Slurry Line 1 Flow | `Default` | `"100.00"` | `"0.00"` |
| Slurry Line 2 Flow | `Default` | `"100.00"` | `"0.00"` |
| Slurry Line 3 Flow | `Default` | `"100.00"` | `"0.00"` |

`ExsituConditionRecipeTemplate` 除下表标注的异常外，Min/Max/Default **全部与 `ExsitCondition Recipe_Polish A` 一致**，无需修改。

**刻意未照抄的 6 行（Excel 自身数据错误，已在夹具 `valueNote` 中记录）**

| 参数 | Excel 初期值 | 模板保留值 | 原因 |
|---|---|---|---|
| Platen Wash Selection、Slurry Line 1/2/3 Type（两份 sheet 共 7 行） | `0.1` | `"0"` | `0.1` 对 option/list 参数**非法**（选项为 `0:Close/1:Open`、`0:Slurry/1:DIW/2:Close`）；`CheckRecipeDefualtValue` 会将其归一到 `Options[0]`，与当前模板值**等效** |
| Sweep Stop Time Type（仅 Exsitu） | `0.1` | `"0"` | 同上（选项 `0:Time/1:Ratio`） |
| Process Type（仅 Exsitu） | `1` | `"25"` | Excel 的 初期值 `1` 与**该表自己的选项集** `{25:Condition, -1:Recipe End}` 冲突；取唯一有效过程值 `25` |

**新增护栏测试** `ConditionTemplateNumericConfigMatchesExcel`：夹具已登记 Excel 的 `Min./Max./初期值`，测试逐行断言模板的 `Min`/`Max`/`Default` 与之一致（带 `valueNote` 的异常行跳过）。以后表格改动而忘记改代码会直接变红。

**随之修复的 4 处（拆分时点上的实际缺陷）**

| 位置 | 问题 |
|---|---|
| `RecipeEditorViewModel.OnTreeAdd` | 缺 `ExsituConditionRecipeTemplate` 的 `case` → 新建 Exsitu Condition 配方会落到 `default: return;`，**静默失败** |
| `RecipeTemplateRegistry` | `ExsituCondition Recipe` 仍指向 `InsituConditionRecipeTemplate` → 该文件夹规则永不生效 |
| `ExsituConditionRecipeTemplate` | 声明为 `public class`，其余模板均为 `public static class` → 已统一 |
| `AddRecipeEditorViewModel` | case 写作 `"Exsitu Condition Recipe"`（多空格），实际文件夹名是 `ExsituCondition Recipe` → 默认模板选不中 |

**测试相应调整**

- `AllTemplates` 增加 `ExsituCondition Recipe`（14 → 15 个模板）。
- `TemplateRulesMatchExcelFixture`：`Condition Recipe` 改用 `condition-operability-fixture.json`（0 规则，断言 1/2 空转通过），新增 `ExsituCondition Recipe` 一行。
- 对账断言 3 收紧为 `NotUsed == 1` **且 `Reason != null`** 且模板存在 → 必须挂规则（`Condition Recipe` 的 `不使用=1` 行没有任何条件，不应被要求挂规则）。
- `SingleConditionRulesFollowModeParameter` 去掉 `Condition Recipe` 文件夹（该模板按设计 0 规则）。
- 新增 `ConditionTemplatesAreIndependent` / `ExsituConditionParametersFollowProcessType` / `InsituConditionParametersAreAlwaysOperable`。

### Rev04（Polish 工作簿落地：12 个 sheet / 11 个模板）

依据 `Polish A_For_Recipe_Rev15.xls`。**该工作簿的规则来源列与前三个工作簿不同。**

| # | 变更 | 依据 |
|---|---|---|
| 1 | **规则来源列 = `显示条件说明`，不是 `说明`** | 修订履历写明「在显示条件说明一列增加是否允许显示」。且 `说明` 列在 `Load Retry` / `UnLoad` / `UnLoad Retry` / `Dechuck` / `Dechuck Retry` / `ExsitCondition` 6 个 sheet 里是**遗留脏数据**（引用 `Process Type=1/2/3或4`，而这些表的 Process Type 只有 `10/15/20/25` 与 `-1`）——照它实现会让这些行永久灰显 |
| 2 | `显示条件说明` 的「是否允许**显示**」**沿用现有「灰显只读」语义** | 与 Dry/Pen/Roll 行为一致；且条件按「步」计算，整行隐藏在逐步条件下语义不明 |
| 3 | **`ConditionRecipeTemplate` 同时服务 `Condition Recipe` 与 `ExsituCondition Recipe` 两个 sheet**，允许值放宽为 **`{1,25}`** | 模板 Process Type 选项本就是 `1:InsituCondition / 25:ExsituCondition / -1`。`ExsitCondition` sheet 写 `=25时允许`，而 `Condition` sheet 的说明列**全空**；取并集 `{1,25}` 的实际效果是「只有 `-1: Recipe End` 时全部灰显」，两个变体都不误伤。`Reason` 保留 Excel 原文 `Process Type=25时允许` |
| 4 | **`不使用=1` 但模板中确实存在的行也挂规则**（`Head Middle Wash` / `Head External Wash` / `Head Rotation` / `InSitu` / `Sweep Stop Time Type` / `Ratio` 等） | 这些行在界面上**会显示**（模板里有、现场 JSON 里也有），且 Excel 给了它们条件。对账测试断言 3 相应从「不使用行不得存在于模板」放宽为「若模板存在则必须挂规则」 |
| 5 | **给 `Plant Wash Selection` 行补别名 `PlatenWashSelection`**（Polish / Dechuck / DechuckRetry 三个模板） | Excel 写 `Platen Wash Selection`，模板行名是 `Plant Wash Selection`。行名不动（PLC 侧 `GetRealVal` 仍靠 `PlantWashSelection` 匹配），仅补别名 |
| 6 | **新增 12 个文件夹 → 模板注册**，并把两个 Sweep 模板的规则同步 | 见 §6.4 |
| 7 | **`选择项次数条件` 列不纳入本次** | 该列在 38 列布局的 sheet 里有数据（样例 `1:0;`），属另一套机制；工艺确认暂不处理 |

### Rev03（Roll 表格修订版落地）

| # | 变更 | 依据 |
|---|---|---|
| 1 | **Roll 规则全部按 `Roll A1_For_Recipe_Rev01 (2).xls` 重写**：规则数 13 → **14**（`Upper Roller Rotate Speed` 新增 `{1}`）；`Process Time`、`Wafer Rotation`、两个 DIW Flowrate、两个 Chemical Supply、两个 Inner Rinse 由 `{3,4}`/`{2}`/`{1,2,3}`/`{1}` 统一改为 **`{1,5}`** | 修订版说明列只用 Process Type 的 `1`/`5`，与选项集 `{1,5,0,-1}` 自洽；**Rev02 记录的规格冲突已消除** |
| 2 | **新增 Roll 跨参数条件**：`Total Diluted Chemical Flow Rate` / `Chemical Dilution Ratio` 的允许条件由 `Process Type=1` 改为 **`WF Upper Face Chemical Supply=1`** | 修订版说明列原文 |
| 3 | Rev02 用于固化冲突的测试 `RollProcessTimeAndDiwFlowrateAreNeverOperableWithCurrentOptions` **已删除**，替换为 `RollRevisedSpecRulesFollowProcessType`（正反两向断言）+ 新增 `RollChemicalFlowRateFollowsUpperFaceChemicalSupply` | 冲突消除后原测试不再成立 |
| 4 | 夹具 `roll-operability-fixture.json` 按修订版重写（`source` 指向 `(2).xls`） | 同上 |

### Rev02（Pen / Roll 落地）— 相对 Rev01 的变更

| # | 变更 | 依据 |
|---|---|---|
| 1 | **`OperableRule.AllOf` → `AnyOf`，语义由「全部子条件满足」改为「任一子条件满足」** | Rev01 把 Pen A1 的跨参数条件误读为「且」。Excel 原文为 `WF Upper Face Chemical Supply=1**或**\nWF Lower Face Chemical Supply=1\n时允许`，是**或**。三份 Excel 中不存在真正的 AND 条件。`AllOf` 当时无任何调用点，改名为 `AnyOf` 并实现求值分支，13 条 Dry 规则零改动 |
| 2 | **源值无法解析时改为 fail-open** | Rev01 的文字（「解析失败 → fail-open」）与伪代码（`v != null && AllowValues.Contains(v)`，即 fail-closed）自相矛盾。现统一为 fail-open：源参数缺失 / 步数不齐 / 源值无数字前缀 / 允许值集合为空 → 一律视为可操作 |
| 3 | **注册 Pen Recipe 与 Roll Recipe，并为两个模板挂载规则** | Rev01 的非目标「一期只做 Dry」完成 |
| 4 | **记录 Roll 规格冲突**：Roll 的说明列引用了同表 `Process Type` 未定义的取值 `2/3/4`，字面实现会让 `Process Time`({3,4}) 与两个 `WF * Face DIW Flowrate`({2}) 在可选值 `{1,5,0,-1}` 下**永久不可操作** | 工艺决定「先按字面实现，待表格修订后再改」；用 `RollProcessTimeAndDiwFlowrateAreNeverOperableWithCurrentOptions` 固化现状，表格修订后该测试会失败以提示同步 |
| 5 | 夹具新增 `conditions` 字段（来源参数 + 允许值），对账测试逐条件比对 | 原夹具只登记 `reason`，允许值集合无独立校验（把 `{1,3}` 写成 `{1}` 不会被发现） |

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
| 实现 Pen A1 / Roll 的具体规则 | ~~一期只做机制 + Dry；`AllOf` 仅预留结构~~ **Rev02 已完成**（见 §0 修订记录） |
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

### 5.1 Pen A1 规则真值表（Rev02）

`Process Type` 选项：`1: Pen`、`5: Rinse`、`0: Disabled`、`-1: Recipe End`（与说明列自洽）。

| 模板参数 | 说明原文 | 允许条件 |
|---|---|---|
| Process Time | `Process Type=5时允许` | `Process Type ∈ {5}` |
| Pen Head Rotation Speed | `Process Type=1时允许` | `∈ {1}` |
| Pen Arm  Sweep Start Position / End Position / Center Speed / Edge Speed / Cycle | `Process Type=1时允许` | `∈ {1}` |
| Pen Pressure | `Process Type=1时允许` | `∈ {1}` |
| WF Upper Face DIW Flow / WF Lower Face DIW Flow | `Process Type=1或5时允许` | `∈ {1,5}` |
| WF Upper Face Chemical Supply / WF Lower Face Chemical Supply | `Process Type=1或5时允许` | `∈ {1,5}` |
| Pen Arm Type | `Process Type=1时允许` | `∈ {1}` |
| **WF Chemical Mixed Total Flow** | `WF Upper Face Chemical Supply=1或 WF Lower Face Chemical Supply=1时允许` | **`ChemUpper=1` 或 `ChemLower=1`**（AnyOf） |
| **WF Chemical Ratio** | 同上 | 同上 |
| Wafer Rotation Speed | 说明为空 | 无规则（永远可操作） |

共 15 条规则（13 条单条件 + 2 条 AnyOf）。`不使用=1` 的 3 个参数（`WF Upper Face N2 Flow`、`WF Lower Face N2 Supply`、`WF Upper Face DI-CO2 Supply`）在模板中已被注释掉，不参与。

### 5.2 Roll A1 规则真值表（Rev03，依据修订版 `Roll A1_For_Recipe_Rev01 (2).xls`）

`Process Type` 选项：`1: Roll`、`5: Rinse`、`0: Disabled`、`-1: Recipe End`。

> ✅ **原规格冲突已修正**：Rev02 时说明列引用了本表未定义的 `2/3/4`；修订版已全部改用 `1`/`5`（分隔符为**顿号 `、`**，表示枚举多个取值，与 `或` 等价），与选项集完全自洽。

| 模板参数 | 说明原文 | 允许条件 |
|---|---|---|
| Process Time | `Process Type=1、5时允许` | `Process Type ∈ {1,5}` |
| Wafer Rotation | `Process Type=1、5时允许` | `∈ {1,5}` |
| WF Upper Face DIW Flowrate | `Process Type=1、5时允许` | `∈ {1,5}` |
| WF Lower Face DIW Flowrate | `Process Type=1、5时允许` | `∈ {1,5}` |
| WF Upper Face Chemical Supply | `Process Type=1、5时允许` | `∈ {1,5}` |
| WF Lower Face Chemical Supply | `Process Type=1、5时允许` | `∈ {1,5}` |
| Upper Roll Inner Rinse Supply | `Process Type=1、5时允许` | `∈ {1,5}` |
| Down Roll Inner Rinse Supply | `Process Type=1、5时允许` | `∈ {1,5}` |
| **Upper Roller Rotate Speed** | `Process Type=1时允许` | `∈ {1}` ← 修订版**新增**（原说明为空） |
| Lower Roller Rotate Speed | `Process Type=1时允许` | `∈ {1}` |
| Upper Roller Pressure | `Process Type=1时允许` | `∈ {1}` |
| Lower Roller Pressure | `Process Type=1时允许` | `∈ {1}` |
| **Total Diluted Chemical Flow Rate** | `WF Upper Face Chemical Supply=1时允许` | **跨参数**：`WF Upper Face Chemical Supply ∈ {1}` ← 由 `Process Type=1` 改为跨参数 |
| **Chemical Dilution Ratio** | `WF Upper Face Chemical Supply=1时允许` | 同上 |
| Process Type | 说明为空 | 无规则（永远可操作） |
| Mega Sonic Supply / Reserved ×3 | `不使用=1` | 模板无此行 |

共 **14** 条规则（8 条 `{1,5}` + 4 条 `{1}` + 2 条跨参数）。

> 备注：`Total Diluted Chemical Flow Rate` / `Chemical Dilution Ratio` 的条件**只依赖** `WF Upper Face Chemical Supply`，未附加 `Process Type` 限定。因此当 `Process Type` 为 `0 (Disabled)` / `-1 (Recipe End)` 而化学供给值为 `1` 时，这两行仍可编辑 —— 这是 Excel 字面语义，若工艺意图并非如此需另行说明。

### 5.3 Polish 工作簿真值表（Rev04，依据 `Polish A_For_Recipe_Rev15.xls`）

**规则来源列 = `显示条件说明`**（前三个工作簿用的是 `说明` 列）。

**12 个 recipe sheet → 11 个模板**（`Condition Recipe` 与 `ExsitCondition Recipe` 共用 `ConditionRecipeTemplate`）：

| sheet | 模板 | 模式参数 | 模式选项 | 规则条数 |
|---|---|---|---|---|
| `Polish Recipe_Polish A` | `PolishRecipeTemplate` | Process Type | `1, 2, 64, -1` | 36 |
| `Recipe_Head Sweep` | `HeadSweepRecipeTemplate` | **Head Sweep Type** | `1:Fix, 2:Traverse` | 24 |
| `Load Recipe_Polish A` | `LoadRecipeTemplate` | Process Type | `10, -1` | 24 |
| `Load Retry Recipe_Polish A` | `LoadRetryRecipeTemplate` | Process Type | `10, -1` | 24 |
| `UnLoad Recipe_Polish A` | `UnloadRecipeTemplate` | Process Type | `15, -1` | 26 |
| `UnLoad Retry Recipe_Polish A` | `UnloadRetryRecipeTemplate` | Process Type | `15, -1` | 26 |
| `Dechuck Recipe_Polish A` | `DechuckRecipeTemplate` | Process Type | `20, -1` | 26 |
| `Dechuck Retry Recipe_Polish A` | `DechuckRetryRecipeTemplate` | Process Type | `20, -1` | 26 |
| `Condition Recipe_Polish A` | `InsituConditionRecipeTemplate` | Process Type | `1, 2` | **0**（说明列全空） |
| `ExsitCondition Recipe_Polish A` | `ExsituConditionRecipeTemplate` | Process Type | `25, -1` | 15（字面 `{25}`） |
| `Recipe_Condition Sweep` | `InsituContidionSweepRecipeTemplate` | **Condition Sweep Type** | `2:Traverse` | 24 |
| `Recipe_ExsituCondition Sweep` | `ExsituContidionSweepRecipeTemplate` | **Condition Sweep Type** | `2:Traverse` | 24 |

合计 **275 条**规则注入模板。

**要点**

- **模式参数不一定是 `Process Type`**：Sweep 类表格用 `Head Sweep Type` / `Condition Sweep Type`。求值引擎无需改动（`SourceParameter` 本就是任意参数名）。
- **`Process Type` 在这些表里是「配方身份」而非「工艺模式」**：`10:Process Load` / `15:Process Unload` / `20:Process Dechuck` / `25:ExsituCondition` 各只有一个有效值 + `-1: Recipe End`。因此 `=10` 之类的规则实际效果是「`Recipe End` 时除模式参数外全部灰显」。
- **现场旧参数名已被模板的 `CompatibleName` 覆盖**：`TopRingType`、`ZoneRRPressure`、`PlantWash`、`DIW`、`PolishHeight`、`SlurryStartPosition`、`SlurrySweepVelocity`、`BlowWFSelect`、`RRType`、`ConditionHeadRotation`、`DechuckHeight` 均有别名 → 存量配方能正确继承规则。
- **唯一的名称断点已修补**：Excel `Platen Wash Selection` ↔ 模板 `Plant Wash Selection`，补别名 `PlatenWashSelection`（Polish / Dechuck / DechuckRetry）。
- **两个 Sweep 模板必须保持一致**：现场「Condition Sweep Recipe」文件夹里同时存放 in-situ 与 ex-situ 形态（实测 `Recipe_ExsituCondition Sweep.json` 就在该文件夹），两者参数与规则完全一致 → 由 `SweepTemplatesStayInSync` 断言守住。
- **Polish Recipe 的 `64: Pad Search`**：规则全是 `=1或2`，而选项含 `64:Pad Search` → 选 Pad Search 时除 `Process Type` 外**整表不可操作**。按字面实现并用 `PolishPadSearchGreysEveryGatedParameter` 固化，工艺若另有要求需修订表格。

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
    [JsonIgnore] public OperableRule? OperableRule { get; set; } // 规格元数据：可操作性
    [JsonIgnore] public bool IsUsed { get; set; } = true;        // 规格元数据：不使用=1 → false（界面隐藏）
}
```

`IsOperable` 为派生值、`OperableRule` 与 `IsUsed` 为规格——都**不落盘**，落盘内容与现状 100% 一致。

**行级可见性（Rev06）**：`IsUsed = false` 的行通过 **DataGrid 默认 `CollectionView` 的过滤谓词**隐藏（`RecipeRowVisibility.Filter`），在 `RefreshEditor` 中经 `ApplyRowVisibility()` 应用。只影响显示视图，`RecipeData.Data` 不变 → 隐藏行照常落盘与下发。

### 6.2 规则表达（新增 `RecipeOperability.cs`）

```csharp
public sealed record OperableRule(
    string SourceParameter,                 // 例："Process Type"
    IReadOnlyList<string> AllowValues,      // 例：["1","3"]；存选项的数字前缀
    string? Reason = null,                  // 说明原文，供 Tooltip
    IReadOnlyList<OperableRule>? AnyOf = null); // **或**语义：Pen A1 "A=1 或 B=1 时允许"
```

`AnyOf` 为 Rev02 启用的**或**分支：本规则满足 =（自身条件满足）或（任一子规则满足）。子规则的 `Reason` 不参与显示，ToolTip 始终取最外层规则的 `Reason`。

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
  2. 对每个带 OperableRule 的 Data，对每个 step i：
       IsSatisfied(rule, i) = 自身条件满足 或 任一 AnyOf 子规则满足
         IsConditionSatisfied(source, allow, i):
            allow 为空/缺失            -> true   （fail-open）
            source 不在索引中          -> true   （fail-open）
            i >= source.Steps.Count    -> true   （fail-open）
            NumericPrefix(值) == null  -> true   （fail-open，Rev02 统一）
            否则 -> allow.Contains(NumericPrefix(值))
       step.IsOperable = IsSatisfied
       step.OperableReason = IsSatisfied ? null : rule.Reason
```

`OperableReason` **仅在不可操作时写入**（`IsOperable == true` 时为 `null`），避免每个可编辑单元格都弹出无意义的 ToolTip。

**fail-open 原则（Rev02 统一）**：允许值集合为空/缺失、源参数缺失、步数不齐、源值解析不出数字前缀 —— 一律视为「可操作」。一条脏数据不得把整表锁成只读。等价地：只有「能确定地判出不允许」时才灰显。

### 6.4 元数据挂载（新增 `RecipeTemplateRegistry.cs`）

问题：`OnTreeViewNodeChanged`（`RecipeEditorViewModel.cs:671`）直接从 JSON 反序列化，**不经过模板**。而现存 Recipe 全部使用 legacy 参数名（见附录 B），规则若只写在模板里，存量文件将拿不到规则。

解决：

```csharp
RecipeTemplateRegistry.Resolve(string folderName)                    // 文件夹名 -> 模板，未注册返回 null
RecipeTemplateRegistry.Decorate(Recipe.Content content, string folderName)  // 按归一化名把规则贴回行上
```

- 依据：本工程已有的「文件夹名 = Recipe 类型」约定（`Chamber.ResolveUnitRecipePath`、`SequenceRecipeEditorViewModel` 的目录常量）。
- **已注册的 15 个文件夹**（Rev13 后）：

  | 文件夹 | 模板 |
  |---|---|
  | `Dry Recipe` | `DryRecipeTemplate` |
  | `Pen Recipe` | `PenA1RecipeTemplate` |
  | `Roll Recipe` | `RollRecipeTemplate` |
  | `Polish Recipe` | `PolishRecipeTemplate` |
  | `Head Sweep Recipe` | `HeadSweepRecipeTemplate` |
  | `Load Recipe` | `LoadRecipeTemplate` |
  | `Load Retry Recipe` | `LoadRetryRecipeTemplate` |
  | `UnLoad Recipe` | `UnloadRecipeTemplate` |
  | `UnLoad Retry Recipe` | `UnloadRetryRecipeTemplate` |
  | `Dechuck Recipe` | `DechuckRecipeTemplate` |
  | `Dechuck Retry Recipe` | `DechuckRetryRecipeTemplate` |
  | `InsituCondition Recipe` | `InsituConditionRecipeTemplate` |
  | `ExsituCondition Recipe` | `ExsituConditionRecipeTemplate` |
  | `InsituCondition Sweep Recipe` | `InsituContidionSweepRecipeTemplate` |
  | `ExsituCondition Sweep Recipe` | `ExsituContidionSweepRecipeTemplate` |

  > ~~`Head Recipe` 与 `Head Sweep Recipe` 内容相同~~ —— **Rev11 已移除 `Head Recipe`**（冗余，配方已并入 `Head Sweep Recipe`）。**Rev13 起两个 sweep 模板各有独立文件夹**（原先共用 `Condition Sweep Recipe`）；两者参数名与规则仍须一致，由 `SweepTemplatesStayInSync` 守住。
- 匹配：`Normalize(Name)` 或 `CompatibleName` 命中，**不使用 Id / 下标**（模板 Id 与 Excel Index 不一致，见附录 B）。
- 未注册的文件夹：`Decorate` 不做任何事（fail-open）。⚠️ Rev01 写了「并写一条 `Logger.Warn`」，**实现中未落实**（记为已知缺口，待补）。
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

1. `IsOperable` / `OperableReason` / `OperableRule` / `IsUsed` **永不落盘**（`[JsonIgnore]`）。
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
| 文件夹未注册模板 | fail-open（**未实现** `Logger.Warn`，已知缺口） |
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
| 12 | **Excel 对账**（Dry / Pen / Roll 三份夹具） | 模板规则集合 == 夹具（逐条比对 `Reason` + 条件来源 + 允许值集合） |
| 13 | 序列化 | `Recipe.Content` 序列化结果不含 `IsOperable` / `OperableReason` / `OperableRule` |
| 14 | 回归 | 载入 legacy 名形态规则正确落位（Dry / Pen / Roll） |
| 15 | **AnyOf（Rev02）** | 子条件任一方满足即可操作；两边都不满足时不可操作 |
| 16 | **源值不可解析 fail-open（Rev02）** | 源值为 `""` / 空白 / `"abc"` 时视为可操作 |
| 17 | **AnyOf 来源订阅（Rev02）** | `GetSourceParameterNames` 含子规则来源 |
| 18 | **单条件规则 × 全部可选取值（Rev02）** | 三个模板逐格比对 `AllowValues` |
| 19 | **Roll 修订版规则（Rev03）** | `{1,5}` 组在 `1: Roll` / `5: Rinse` 可操作、`0` / `-1` 不可操作；`{1}` 组仅 `1: Roll` 可操作（替换 Rev02 的「冲突固化」测试） |
| 20 | **Roll 跨参数条件（Rev03）** | `WF Upper Face Chemical Supply` 为 `0` 时 `Total Diluted Chemical Flow Rate` / `Chemical Dilution Ratio` 不可操作，为 `1` 时可操作 |
| 21 | **多模板覆盖（Rev04）** | `TemplateIdsAndNormalizedNamesAreUnique` / `EveryOperableRuleSourceExistsInTheSameTemplate` 改为遍历 `AllTemplates`（14 个模板） |
| 22 | **夹具对账扩到 14 个模板（Rev04）** | 每份夹具逐条件比对来源 + 允许值；`不使用=1` 但模板存在的行必须挂规则（断言 3 已放宽） |
| 23 | **模式参数不一定是 Process Type（Rev04）** | `SingleConditionRulesFollowModeParameter` 以模板第 1 行为模式参数，覆盖 14 个文件夹（含 Head Sweep Type / Condition Sweep Type） |
| 24 | **Head Sweep 模式分支（Rev04）** | `1:Fix` 时 Start/End Position 可操作、Zone Count/Ratio/Count 不可操作；`2:Traverse` 时全部可操作 |
| 25 | **Load 在 Recipe End 全灰（Rev04）** | `-1: Recipe End` 时除 Process Type 外不可操作；`不使用=1` 但在模板中的 `Head Middle Wash` / `Head External Wash` 同样被门控 |
| 26 | **Platen Wash 别名（Rev04）** | 用 `Platen Wash Selection` 命名的行能在 Polish / Dechuck / DechuckRetry 三个模板命中规则 |
| 27 | **两个 Sweep 模板保持一致（Rev04）** | 参数名序列、规则来源、说明、允许值逐行相同 |
| 28 | **Condition sheet 无规则（Rev04）** | `condition-operability-fixture.json` 全行 `reason == null` |
| 29 | **Polish Pad Search 固化（Rev04）** | `64:Pad Search` / `-1: Recipe End` 下除 Process Type 外整表不可操作 |
| 30 | **两个 Condition 模板独立（Rev05）** | `InsituConditionRecipeTemplate` 无任何规则（其 sheet 说明列全空）；`ExsituConditionRecipeTemplate` 除模式参数外每行挂字面 `{25}`，来源与说明原文一致 |
| 31 | **Exsitu 在 Recipe End 全灰（Rev05）** | `-1: Recipe End` 时除 Process Type 外不可操作；`25:ExsituCondition` 时可操作 |
| 32 | **Insitu 恒可操作（Rev05）** | 三种模式取值下全部行可操作（无规则的 fail-open 极端情形） |
| 33 | **不使用行隐藏（Rev06）** | 15 份夹具逐行断言：Excel `不使用=1` → 模板行 `IsUsed = false`；`=0` → `true`。**无例外** |
| 34 | **隐藏行仍保留数据（Rev06）** | 过滤后的可见集合不含 `IsUsed=false` 的行，但 `RecipeData.Data` 条数不变（隐藏行照常落盘/下发） |
| 35 | **IsUsed 不落盘（Rev06）** | `Recipe.Content` 序列化结果不含 `IsUsed` |
| 36 | **全角冒号兼容（Rev07）** | `NumericPrefix` 对 `1：x` / `1﹕x` / `1∶x` 与半角等价；`PartAfterSeparator` 兼容四种分隔符；全角冒号的步骤值仍能正确命中规则 |
| 37 | **模板只用半角冒号（Rev07）** | 所有模板的 `Options` 与 `Default` 不含 `：`/`﹕`/`∶`（PLC 侧 `GetRealVal` 只按半角拆分） |
| 38 | **下发路径的选项解析（Rev08）** | `NumericPart` 8 组 + `LabelPart` 6 组，语义与各 `GetRealVal` 一致（数值取前缀、字符串取标签，兼容全角） |

夹具：15 份，覆盖 15 个模板。Dry / Pen / Roll 从「说明」列导出；Polish 工作簿的 12 份从「显示条件说明」列导出。工艺改 Excel 而忘记改代码时对账测试变红。

**当前测试规模**：`RecipeOperabilityTests` **115 个用例全绿**；`CMP.Tests.Models` 命名空间 **122/122**（其中 4 个为 `[WpfTestMethod]`）。

## 10. 验收标准

**自动化**：第 9 节 38 项测试全绿；`dotnet build` 无新增警告（基线 1008）。

**手工（UI）**：
1. 打开 Dry / Pen / Roll Recipe，改某步 `Process Type` → 同行其余单元格按真值表**立即**灰显/恢复，无需重开页面；
2. 灰显单元格鼠标悬停显示说明原文（Pen 的 `WF Chemical Mixed Total Flow` 为两行文本）；
3. 灰显单元格无法通过鼠标/键盘修改；
4. 列重排、加步、删步、粘贴、另存之后状态依然正确；
5. 保存后 JSON diff **只有**步骤值（无新字段）；
6. 载入现场 legacy 命名的四个 Dry 配方、`Pen A1_For_Recipe_Rev01.json`、`Roll A1_For_Recipe_Rev01.json`，规则正确生效；
7. Pen：把 `WF Upper Face Chemical Supply` 或 `WF Lower Face Chemical Supply` 任一改为 `1:Enabled`，`WF Chemical Mixed Total Flow` / `WF Chemical Ratio` 应恢复可编辑（**或**语义）；
8. Roll（Rev03）：`Process Type` 在 `1: Roll` 与 `5: Rinse` 之间切换时，`Process Time`、两个 DIW Flowrate、两个 Chemical Supply、两个 Inner Rinse、`Wafer Rotation` 应保持可编辑；切到 `0: Disabled` 后它们变灰；把 `WF Upper Face Chemical Supply` 改为 `0:Disabled` 后 `Total Diluted Chemical Flow Rate` / `Chemical Dilution Ratio` 变灰。
9. Polish（Rev04）：打开 `Head Sweep Recipe` 下的配方，把 `Head Sweep Type` 在 `1: Fix` 与 `2: Traverse` 之间切换 → `Start/End Position` 始终可编辑，`Zone Count`、`Zone N Ratio`、`Head Sweep Count` 仅在 `2: Traverse` 可编辑。
10. Polish（Rev04）：打开 `Load Recipe` / `UnLoad Recipe` / `Dechuck Recipe`，把 `Process Type` 改为 `-1: Recipe End` → 除 `Process Type` 外整表变灰。
11. Polish（Rev04）：打开 `Polish Recipe`，把 `Process Type` 改为 `64: Pad Search` → 除 `Process Type` 外整表变灰（现状，待工艺确认）；改成 `1` / `2` 恢复可编辑。
12. Polish（Rev04）：存量 `Condition Recipe` 配方（模板行名 `Platen Wash Selection`）与 Polish / Dechuck 配方都能正确灰显，验证 `PlantWashSelection` / `PlatenWashSelection` 别名生效。
13. **不使用行隐藏（Rev06）**：打开 Dry Recipe → 表格中**看不到** `IPA Arm Sweep Center Velocity` / `IPA Arm Sweep Edge Velocity`；打开 Pen Recipe → 看不到 `WF Upper Face N2 Flow` / `WF Lower Face N2 Supply` / `WF Upper Face DI-CO2 Supply`；打开 Roll Recipe → 看不到 `Mega Sonic Supply`。
14. **不使用行隐藏（Rev06）**：打开 Load / UnLoad / Dechuck / Condition / ExsituCondition 配方，对照 spec §0 Rev06 的「逐模板隐藏清单」逐条确认。⚠️ 特别确认 **`InsituCondition` 配方的 `Process Type` 也不可见**（严格照表的结果）—— 若影响现场操作，说明该 sheet 的 `不使用` 列需要修订，而不是改代码。
15. **隐藏行数据不丢（Rev06）**：在含隐藏行的配方上新增/删除步骤并保存 → 保存后的 JSON 里隐藏行的 Step 数据仍存在（行数与保存前一致），diff 只有步骤值。

## 11. 风险与对策

| 风险 | 对策 |
|---|---|
| `PropertyChanged` 订阅泄漏 | `WeakEventManager`；载入 / 重建列时统一退订 |
| 列重排后下标语义变化 | 只按当前 `Steps` 列表下标求值，不缓存下标 |
| legacy 名匹配不全 | 复用 `CompatibleName`；用真实 `111.json` 做回归夹具 |
| 脏数据把表锁死 | fail-open 不变式 + 专项单测 |
| 新字段污染 JSON | `[JsonIgnore]` + 序列化断言测试 |
| 模板 Id 与 Excel Index 错位 | 规则只按名匹配 + Registry 一致性测试 |
| 文件夹与模板类型不匹配 | fail-open（**未实现** `Logger.Warn`）；本期接受 |
| **Roll 说明列曾引用未定义的 Process Type 取值（Rev02）** | Rev03 已由修订版表格 `(2).xls` 修正，规则全部重写为自洽的 `{1,5}`/`{1}`/跨参数 |
| Roll 跨参数条件未附加 Process Type 限定（Rev03） | 按 Excel 字面实现；已在 §5.2 备注中记录，若工艺意图不同需另行说明 |
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
5. **`说明` 列不是 ProcessType 专用**：`Pen A1` 除 `Process Type=1或5时允许` 外，还有**跨参数「或」条件**（`WF Upper Face Chemical Supply=1或 WF Lower Face Chemical Supply=1 时允许`，作用于 `WF Chemical Mixed Total Flow` / `WF Chemical Ratio`），`Roll A1` 含 `Process Type=1或2或3时允许`。机制按通用形态设计；Rev02 已为 Dry / Pen / Roll 三个模板落规则。
6. **Roll 规格曾自相矛盾（Rev02 发现，Rev03 已修正）**：`Roll A1_For_Recipe_Rev01.xls` 的 `Process Type` 行只定义了 `1/5/0/-1`，但同表说明列引用了 `2/3/4`。修订版 `Roll A1_For_Recipe_Rev01 (2).xls` 已改用 `1`/`5`（顿号分隔枚举值），冲突消除；`Upper Roller Rotate Speed` 新增 `{1}` 规则，`Total Diluted Chemical Flow Rate` / `Chemical Dilution Ratio` 改为跨参数条件。
7. **Polish 工作簿的规则在 `显示条件说明` 列，不是 `说明` 列（Rev04）**：`说明` 列在 6 个 sheet 里是引用不存在取值的遗留脏数据（`Process Type=1/2/3或4`，而这些表的 Process Type 只有 `10/15/20/25`）。
8. **`Process Type` 在 Polish 工作簿里是「配方身份」而非「工艺模式」（Rev04）**：取值 `10:Process Load` / `15:Process Unload` / `20:Process Dechuck` / `25:ExsituCondition`，各只有一个有效值 + `-1: Recipe End`。
9. **Sweep 类表格的模式参数是 `Head Sweep Type` / `Condition Sweep Type`（Rev04）**，不是 `Process Type`；求值引擎无需改动。
10. **`Plant Wash Selection` ↔ `Platen Wash Selection`（Rev04）**：三个模板行名是 `Plant...`（PLC 侧 `GetRealVal` 依赖该名），Excel 用 `Platen...`；已补别名 `PlatenWashSelection`。
11. **`ConditionRecipeTemplate` 曾服务两个 sheet（Rev04），Rev05 已拆开**：`Condition Recipe` → `InsituConditionRecipeTemplate`（说明全空，无规则）；`ExsituCondition Recipe` → `ExsituConditionRecipeTemplate`（`=25`，字面）。
