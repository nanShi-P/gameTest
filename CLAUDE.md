# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目性质

Unity 像素风魔法主题 **增量游戏（Incremental / Idle Game）** 的 MVP，目标平台 Windows PC。
Unity 版本：2022.3 LTS+（实际打开的版本以 `ProjectSettings/ProjectVersion.txt` 为准）。

## 三件套工作流（必须遵循）

本项目通过三个项目级 skill 串联开发，对应三份持久化文档：

| Skill | 输入 | 输出 |
|---|---|---|
| `refine-requirements` | 用户想法 | `docs/requirements.md` |
| `plan-tasks` | `docs/requirements.md` | `docs/tasks.md` |
| `implement-task [TID]` | `docs/tasks.md` + requirements | Unity 代码/资产 + 更新 tasks.md 状态 |

**关键约定**：
- `docs/requirements.md` 是数值与行为的**唯一真相源**。任务清单只是路径，遇到冲突以 requirements 为准；若 requirements 缺失，反过来提示用户回 `/refine-requirements` 补充，**不要自己编数值**。
- `docs/tasks.md` 是跨会话的任务追踪（不是 Claude Code 的 TaskCreate；那是会话内的，不适合本流程）。每完成一个任务，状态从 `pending → in_progress → completed`，并更新顶部进度统计。
- **一次只做一个任务**。即使顺手能做下一个也不要做，会破坏可追溯性。
- 若任务粒度超过 2 小时，拆成子任务（如 `T003a/T003b`）再做第一个。
- 任务实现完成后若需要用户在 Unity Editor 中手动操作（建场景、拖引用、设 Layer 等），**必须显式列出步骤**——我无法操控 Unity Editor。

## 当前进度概览

- **已完成**：T001–T013（数据/Tick/存档/UI/转生/事件全部跑通），共 13 个任务
- **未完成**：T014（Windows 打包）；T015–T020（动画+音效增补）；T021–T025（2D 顶下视角场景重构）
- 详细状态读 `docs/tasks.md`

## 核心运行模型（关键类）

读这几个文件就能掌握整体走向：

- `Scripts/Core/GameManager.cs` — 单例。承载 `State`（GameState）、ProducerDef/UpgradeDef 列表、Tick 循环（`InvokeRepeating(Tick, 0.1)`）、点击/购买/转生公共接口、`TempGlobalMult`/`TempClickMult`（事件加成）、`AutoClickerLoop` 协程。**所有玩法操作都从这里调用**。
- `Scripts/Core/GameState.cs` — 纯数据类（无 MonoBehaviour），可直接被 Newtonsoft 序列化。**默认 `producerCounts` 含 `apprentice: 1`**（初始赠送）。
- `Scripts/Events/RandomEventSystem.cs` — 自己的协程循环，按权重抽 3 种事件，通过 `GameManager.TempGlobalMult/TempClickMult` 注入加成。提供 `ClearActiveEvent()` 给转生清理。
- `Scripts/Save/SaveSystem.cs` — 静态类。GameManager 启动 Load、每 10s + Quit/Pause 时 Save。BigDouble 走 `BigDoubleConverter`（序列化为 `{m,e}`）。
- `Scripts/Util/NumberFormat.cs` — `Format(BigDouble)`：<1000 一位小数；K/M/B/T/Qa；之后 aa/ab/ac…

## UI 是"代码构建"模式（不要手拖 Canvas）

整个 UI 由 MonoBehaviour 在 `Awake/Start` 中用 `new GameObject(..., typeof(Image), ...)` 程序化构建，**不依赖 Prefab、不依赖手动拖拽**。这是项目的核心约定：

- `MainUIBuilder` 挂在 `UIRoot` 上 → 自己 new Canvas + 左右面板 + 底部横幅
- `TabController` 也挂 `UIRoot` 上 → 自己建标签栏 + PanelRoot；提供 `AddTab(id, label)` 与 `RegisterPanel(id, panel)` 给各 Tab 用
- `ProducersTab`/`UpgradesTab`/`PrestigeTab` 各自 new 自己的 panel，调 `tc.RegisterPanel(...)` 注册
- 行 UI（`ProducerRow`/`UpgradeRow`）是纯 C# 类（非 MonoBehaviour），由 Tab 创建并管理刷新

### UI 代码构建的踩坑（写过坏过的坑，别再踩）

1. **Start 顺序陷阱**：MonoBehaviour 之间的 `Start()` 执行顺序不固定。`TabController.tabs` 必须在 `RegisterPanel` 被调用时已存在 → TabController 用了 `EnsureBuilt()` 模式（按需触发构建，不依赖 Start 顺序）。新增 Tab 类一律先调 `tc.RegisterPanel(...)` **再** `SetParent(tc.PanelRoot)` 以保证 PanelRoot 已建好。
2. **Inactive 节点的 LayoutGroup 不重算**：标签切换后必须 `LayoutRebuilder.ForceRebuildLayoutImmediate` 整棵子树，否则 ScrollRect/VerticalLayoutGroup 不显示。`TabController.Activate` 已处理。
3. **字体糊**：永远走 `Scripts/UI/UIFont.Get()`，它会优先取 Microsoft YaHei UI。**禁止**直接 `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`。
4. **数字刷新**：脏标记 + 兜底每 0.2s 强刷一次（持续累加的 PPS 不会主动触发 StateChanged）。

## 代码与目录约定

- 脚本：`Assets/Scripts/{Core,Data,UI,Save,Util,Events}/`
- ScriptableObject 资产：`Assets/Data/{Producers,Upgrades}/`
- Editor 工具：`Assets/Editor/`
- Prefab：`Assets/Prefabs/`；场景：`Assets/Scenes/Main.unity`
- 第三方源码：`Assets/ThirdParty/BreakInfinity/BigDouble.cs`（直接源码，非 NuGet）
- 命名空间：`Game.{Core,Data,UI,Save,Util,Events}`，与目录对齐
- 字段：`[SerializeField] private`，避免裸 `public`
- **大数一律 `BreakInfinity.BigDouble`**。注意 BigDouble 操作符不全：`<` `>` 不可用，要走 `CompareTo()`；`Sign` 是静态方法 `BigDouble.Sign(v)` 不是属性；`Pow10(long)` 比 `Pow(BigDouble, long)` 更可靠。
- **Tick 走固定时间步（默认 0.1s）**，不要把业务放 `Update()`
- UI 数字显示统一走 `Game.Util.NumberFormat.Format(BigDouble)`
- 存档：Newtonsoft.Json（`com.unity.nuget.newtonsoft-json`），BigDouble 通过 `BigDoubleConverter`，存档结构含 `version` 字段，写入 `Application.persistentDataPath/save.json`

## Editor 菜单（已有工具）

- `Alchemist/Seed Producers (overwrite)` — 按 requirements §4 批量生成 6 个 ProducerDef 资产
- `Alchemist/Seed Upgrades (overwrite)` — 按 §5 批量生成 8 个 UpgradeDef 资产
- `Alchemist/Save/Open Save Folder` — 在文件浏览器中打开存档目录
- `Alchemist/Save/Delete Save` — 删档（调试常用）

数值变更后重跑对应 Seeder 即可同步资产，不要手动改 SO 字段去对齐 requirements。

## GameManager Inspector 调试旋钮

调试期常用（用完务必复位 + 删档）：

- `Debug Seed Producer Id` + `Debug Seed Count` — 进入 Play 时强制给某生产者一些初始数量
- `Debug Start Mana` — 进入 Play 时给初始魔力尘（仅在存档为空时生效）
- `Debug Speed Multiplier` — 全局产出倍速（100 表示 100×）
- `Load On Start` — 取消勾选可跳过 Load（调试 GameState 默认值时用）

## 我无法做的事（必须委托给用户）

- 启动 Unity Editor / 进入 Play 模式 / 观察 Console
- 创建场景、Prefab；Inspector 中拖引用、配 Layer/Tag
- Build & Run、平台切换；导入 Asset Store 包
- 通过 Unity Package Manager UI 安装包（可改 `Packages/manifest.json`，Unity 会自动解析）
- 准备真实美术/音效资源（可生成程序化占位）

实现任务时遇到以上需求，写到任务完成报告的"需要你手动做的步骤"里。

## 验证手段

EditMode/PlayMode Tests 框架未引入。验证以 **`Assets/Scripts/Core/SmokeTest.cs` 中的运行时断言** + Console 输出为主。新增工具类时把验证用例追加进 SmokeTest，要求用户 Play 后检查 Console 无 `[... FAIL]`。

## 仓库与运行

- Git 主分支：`main`
- README 内容仅占位
- `start-claude-mpu.cmd` 是用户启动 Claude Code 的本地包装脚本（设置自托管 API 路由），与游戏无关，不要修改
