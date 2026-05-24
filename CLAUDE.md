# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目全景

这是一个 Unity 增量游戏的多仓库项目，分布在 `E:\AIProject\` 下：

| 仓库 | 路径 | 内容 |
|---|---|---|
| **gameTest** | `E:\AIProject\gameTest`（本仓库） | Unity 像素风魔法主题增量游戏；目标 Windows PC；Unity 2022.3 LTS+ |
| **gameTest-admin** | `E:\AIProject\gameTest-admin` | 配置后台：.NET 9 Web API + React/TS + SQLite，本地 docker-compose 运行 |
| **gameTest-config** | `E:\AIProject\gameTest-config` | 纯 JSON 配置仓库，启用 GitHub Pages；admin 后端通过 `git push` 自动发布 |

游戏端启动时拉取 `https://nanshi-p.github.io/gameTest-config/latest.json`，三层兜底（远程 → 本地缓存 → 内置默认）。

注意 CLAUDE.md 在游戏仓库根，但你可能被指派去改 admin 子项目。涉及到 admin 时务必 `cd E:/AIProject/gameTest-admin` 操作，不要在游戏仓库下凭空找文件。

## 文档与任务追踪（必读）

游戏与 admin 是独立模块，文档分开追踪：

| 范围 | 需求 | 任务 | 进度留痕 / handoff |
|---|---|---|---|
| 游戏 | `docs/requirements.md` | `docs/tasks.md` | `docs/HANDOFF.md`, `docs/progress-log.md`, `docs/T020-audio-setup.md`, `docs/T021-art.md` |
| Admin | `docs/admin-requirements.md` | `docs/admin-tasks.md` | `docs/admin-smoke.md` |

**铁律**：
- requirements 是数值与行为的**唯一真相源**；任务清单只是路径。冲突以 requirements 为准；如 requirements 缺失，反提示用户补充，不要凭空造数值。
- `docs/tasks.md` / `docs/admin-tasks.md` 是跨会话追踪。完成一个 → 状态 `pending → in_progress → completed`，更新顶部进度统计。
- **一次只做一个任务**。即使顺手能做下一个也不要做，破坏可追溯性。
- 任务实现完后若需用户在 Unity Editor / 浏览器 / shell 中手动操作，**必须显式列步骤**，写到对应 handoff 文档。

## 三件套 Skill 工作流

`/refine-requirements` → `/plan-tasks` → `/implement-task` 串联开发。skill 描述在 `.claude/skills/`，调用约定见各 skill SKILL.md。

## 当前进度概览（2026-05-24）

| 模块 | 进度 |
|---|---|
| 游戏 T001–T013 核心循环 | ✅ 完成 |
| 游戏 T015–T025 动画/音效/场景重构 | ✅ 代码完成；待用户在 Unity 中挂 AudioManager + WorkshopRoot、跑两个 Editor 菜单（详 `docs/HANDOFF.md`） |
| 游戏 T014 Windows 打包 | ☐ 待全部内容完工后做 |
| 游戏 T026–T029 ConfigLoader（联动 admin） | ☐ 待规划 |
| Admin A001–A018 整套 | ✅ 代码完成；本地 dotnet run + npm 验证；docker-compose 编排就绪；真实 git push 需用户提供 `GITHUB_PAT`（详 `docs/admin-smoke.md`） |
| Admin A019–A021（技能树/模拟器/登录） | ☐ v2 占位 |

## 游戏侧核心运行模型

读这几个文件能掌握整体走向：

- `Scripts/Core/GameManager.cs` — 单例。Tick 循环 `InvokeRepeating(Tick, 0.1)`；持有 `State`、ProducerDef/UpgradeDef 列表；公共 API `Click / TryBuyProducer / TryBuyUpgrade / TryPrestige`；`TempGlobalMult/TempClickMult` 给事件加成注入；`AutoClickerLoop` 协程。**所有玩法操作从这里调用**。
- `Scripts/Core/GameState.cs` — 纯数据类，可直接 Newtonsoft 序列化。**默认 `producerCounts` 含 `apprentice: 1`**（初始赠送），转生后保留。
- `Scripts/Events/RandomEventSystem.cs` — 自己的协程循环，按权重抽 3 种事件，通过 `GameManager.TempGlobalMult/TempClickMult` 注入加成；`ClearActiveEvent()` 给转生用。
- `Scripts/Save/SaveSystem.cs` — 启动 Load、每 10s + Quit/Pause 时 Save；BigDouble 通过 `BigDoubleConverter` 序列化为 `{m, e}`。
- `Scripts/Util/NumberFormat.cs` — `Format(BigDouble)`：<1000 一位小数；K/M/B/T/Qa；之后 aa/ab/ac…
- `Scripts/Util/UITween.cs` — `ScalePunch / MoveAndFade / ColorFlash / LerpNumber`，无 DOTween 依赖。
- `Scripts/Audio/AudioManager.cs` — 自动扫描 `Resources/Audio/{SFX,BGM}`；缺资源静默。
- `Scripts/Scene/WorkshopSceneBuilder.cs` — 2D 顶下视角场景骨架（地板/熔炉/6 站位）。
- `Scripts/UI/MainUIBuilder.cs` — 顶部 HUD + 右侧可折叠侧栏。

## UI 是"代码构建"模式

UI 全部由 MonoBehaviour 在 `Awake/Start` 中用 `new GameObject(..., typeof(Image), ...)` 程序化构建。**不依赖 Prefab、不依赖手动拖拽**。这是项目核心约定。

### 踩坑实录（不要再踩）

1. **Start 顺序陷阱**：MonoBehaviour Start 顺序不固定。`TabController.tabs` 必须在 `RegisterPanel` 调用时已存在 → 用 `EnsureBuilt()` 模式，按需触发构建。新 Tab 类一律先 `tc.RegisterPanel(...)` 再 `SetParent(tc.PanelRoot)`。
2. **Inactive 节点的 LayoutGroup 不重算**：标签切换后必须 `LayoutRebuilder.ForceRebuildLayoutImmediate` 整棵子树。`TabController.Activate` 已处理。
3. **字体糊**：永远走 `Scripts/UI/UIFont.Get()`，优先取 Microsoft YaHei UI。**禁止**直接 `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`。
4. **数字刷新**：脏标记 + 兜底每 0.1–0.2s 强刷一次（持续累加的 PPS 不会主动触发 StateChanged）。
5. **Resources.Load 路径**：必须在 `Assets/Resources/` 下，否则运行时返回 null（占位精灵生成器已正确使用 `Resources/Art/Sprites/`）。

## 代码与目录约定（游戏）

- 脚本：`Assets/Scripts/{Core,Data,UI,Save,Util,Events,Scene,Audio}/`
- ScriptableObject 资产：`Assets/Data/{Producers,Upgrades}/`
- Editor 工具：`Assets/Editor/`
- Prefab：`Assets/Prefabs/`；场景：`Assets/Scenes/Main.unity`
- 第三方源码：`Assets/ThirdParty/BreakInfinity/BigDouble.cs`（直接源码，非 NuGet）
- 资源（音/图）：`Assets/Resources/{Audio,Art}/`
- 命名空间：`Game.{Core,Data,UI,Save,Util,Events,Scene,Audio}`，与目录对齐
- 字段：`[SerializeField] private`，避免裸 `public`
- **大数一律 `BreakInfinity.BigDouble`**。注意：
  - `<` `>` 操作符不可用 → 走 `CompareTo()`
  - `Sign` 是静态方法 `BigDouble.Sign(v)`，不是属性
  - `Pow10(long)` 比 `Pow(BigDouble, long)` 更可靠
- **Tick 走固定时间步（默认 0.1s）**，业务别放 `Update()`
- UI 数字显示统一 `Game.Util.NumberFormat.Format(BigDouble)`
- 存档：Newtonsoft.Json（`com.unity.nuget.newtonsoft-json`），BigDouble 走 `BigDoubleConverter`，含 `version` 字段，写入 `Application.persistentDataPath/save.json`

## Editor 菜单（已有工具）

- `Alchemist/Seed Producers (overwrite)` — 按 requirements §4 批量生成 6 个 ProducerDef
- `Alchemist/Seed Upgrades (overwrite)` — 按 §5 批量生成 8 个 UpgradeDef
- `Alchemist/Save/Open Save Folder` / `Delete Save` — 存档管理
- `Alchemist/Art/Generate Placeholders` — 程序化生成占位精灵到 `Resources/Art/Sprites/`

数值改了 → 重跑对应 Seeder，不要手动改 SO 字段。

## GameManager Inspector 调试旋钮

调试期常用（用完务必复位 + 删档）：

- `Debug Seed Producer Id` + `Debug Seed Count` — 给某生产者强制初始数量
- `Debug Start Mana` — 给初始魔力尘（仅在存档为空时生效）
- `Debug Speed Multiplier` — 全局产出倍速（100 = 100×）
- `Load On Start` — 取消勾选可跳过 Load

## Admin 项目约定（在 gameTest-admin 下工作时）

- **后端 .NET 9**（用户机器没装 .NET 8，用 9 等价，target framework=`net9.0`）
- **EF Core 9.0 + SQLite**；用 `EnsureCreated()`（无 Migrations，schema 变更要 `rm admin.db && 重启`）
- **Swashbuckle 必须用 7.2.0**（10.x 与 Microsoft.OpenApi API 不兼容，编译报错）
- **`Microsoft.AspNetCore.OpenApi` 9.0.6** 包含必要的 `Microsoft.OpenApi.Models` 命名空间
- **LibGit2Sharp** 用 `CredentialsHandler` 时必须 `using LibGit2Sharp.Handlers;`
- 前端无 shadcn/ui，用 vanilla CSS（功能等价，避免 npm 配置复杂度）
- npm 在 git bash 下可能 PATH 异常（找不到 node）→ 直接调 `/c/Program\ Files/nodejs/npm.cmd` 或让用户用 cmd
- **配置数据流**：admin DB → JSON Export → LibGit2Sharp push 到 gameTest-config 仓库 → GitHub Pages → 游戏客户端 HTTP 拉取
- admin 需要 `GITHUB_PAT` 环境变量才能 publish；缺失时 `/api/publish` 返回 500
- 后台 schema 在 `admin-requirements.md §7`，游戏端 DTO 必须严格对齐
- 调试运行：`cd backend/AlchemistAdmin && dotnet run` + `cd frontend && npm run dev`（端口 5000 / 5173）

## 我无法做的事（委托给用户）

- 启动 Unity Editor / 进入 Play 模式 / 观察 Console
- 创建 Unity 场景、Prefab；Inspector 中拖引用、配 Layer/Tag
- Build & Run、平台切换；导入 Asset Store 包
- 通过 Unity Package Manager UI 安装包（可改 `Packages/manifest.json`，Unity 会自动解析）
- 准备真实美术/音效资源（可生成程序化占位）
- `git push` 到远程（涉及凭证，需用户确认）
- 在 GitHub 上设置 Pages / 生成 PAT / 配置仓库设置
- Docker Desktop GUI 操作（但 docker-compose CLI 可用）

实现任务时遇到以上需求，写到对应任务的完成报告/handoff 文档里。

## Git 卫生（重要）

游戏仓库 `.gitignore` 已规范化（不再跟踪 `Library/Temp/Logs/UserSettings/` 以及 `*.csproj/*.sln`）。如果未来 Unity 又生成了不该跟的文件，先扩 `.gitignore` 再 `git rm --cached`，不要让 Library 之类的污染仓库。

## 验证手段

- **游戏**：EditMode/PlayMode Tests 未引入。验证以 `Assets/Scripts/Core/SmokeTest.cs` 中的运行时断言 + Console 输出为主。新增工具类时把验证用例追加进 SmokeTest，要求用户 Play 后检查 Console 无 `[... FAIL]`。
- **Admin 后端**：Swagger UI（`http://127.0.0.1:5000/swagger`）+ `curl /ping`、`curl /api/producers`。
- **Admin 前端**：浏览器 `http://127.0.0.1:5173`，左上角后端在线状态指示灯。

## 仓库与运行

- Git 主分支：`main`（三仓库一致）
- README 内容简单，不要据此推断项目细节
- `start-claude-mpu.cmd` 是用户启动 Claude Code 的本地包装脚本（设置自托管 API 路由），与游戏无关，不要修改
