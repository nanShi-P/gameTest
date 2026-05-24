# 任务清单 — 炼金术士

> 最后更新：2026-05-24
> 来源：docs/requirements.md
> 目标平台：Windows PC，Unity（推荐 2022.3 LTS 或更新）

## 进度
- 待办：1
- 进行中：0
- 已完成：24

## 任务列表

### T001 — Unity 项目骨架与依赖
- **状态**：☑ completed（代码侧完成；待用户在 Unity Hub 中打开项目并按 `docs/T001-setup.md` 完成 Play 验证）
- **依赖**：无
- **估时**：1h
- **描述**：
  - 在 `E:\AIProject\gameTest` 创建 Unity 项目（2D 模板，Unity 2022.3 LTS+）。
  - 建立目录：`Assets/{Scripts/{Core,Data,UI,Save,Util,Events},Data/{Producers,Upgrades},Prefabs,UI,Scenes,ThirdParty}`。
  - 通过 Package Manager 安装 `com.unity.nuget.newtonsoft-json`。
  - 将 `BreakInfinity.cs`（https://github.com/Razenpok/BreakInfinity.cs）放到 `Assets/ThirdParty/BreakInfinity/`。
  - 创建主场景 `Assets/Scenes/Main.unity`。
- **验收**：
  - [ ] 上述目录在 Project 窗口可见
  - [ ] 新建一个测试脚本，能 `using Newtonsoft.Json;` 和 `using BreakInfinity;` 编译通过
  - [ ] `new BigDouble(1e100) * 10` 在 Console 打印正确
- **产物**：可运行的 Unity 空项目

### T002 — 数字格式化工具 NumberFormat
- **状态**：☑ completed（验收并入 SmokeTest，Play 时观察 Console）
- **依赖**：T001
- **估时**：1h
- **描述**：在 `Scripts/Util/NumberFormat.cs` 实现 `Format(BigDouble v)`，输出规则：
  - <1000：保留 1 位小数（"123.4"）
  - <1e15：K/M/B/T 后缀
  - ≥1e15：aa/ab/ac… 双字母后缀（每 3 个数量级一进）
- **验收**：
  - [ ] EditMode 测试：`Format(1234)=="1.23K"`、`Format(1e18)=="1.00aa"`、`Format(1e21)=="1.00ab"`
  - [ ] 负数与 0 正确处理
- **产物**：`NumberFormat.cs` + 单元测试

### T003 — 生产者数据 ScriptableObject
- **状态**：☑ completed（脚本+Seeder 完成；待用户在 Unity 中执行菜单 `Alchemist → Seed Producers` 生成 6 个资产）
- **依赖**：T001
- **估时**：1h
- **描述**：
  - `Scripts/Data/ProducerDef.cs`：`[CreateAssetMenu]` ScriptableObject，字段：`id, displayName, baseCost(double), baseProduction(double), costMultiplier=1.12, unlockProducerId, unlockProducerCount`。
  - 在 `Assets/Data/Producers/` 创建 6 个资产，数值来自 requirements.md §4。
- **验收**：
  - [ ] 6 个 ProducerDef 资产存在并按表格填好
  - [ ] Inspector 可直接编辑数值
- **产物**：6 个 ProducerDef 资产

### T004 — 升级数据 ScriptableObject
- **状态**：☑ completed（脚本+Seeder 完成；待用户在 Unity 执行 `Alchemist → Seed Upgrades`）

> **设计备注**：加速符文的 1/2/3 级解锁阶梯（10/25/50）在运行期 GameManager 中按"拥有该生产者数量 ≥ threshold * level"判断，避免 SO 字段过度复杂。
- **依赖**：T001
- **估时**：0.75h
- **描述**：
  - `Scripts/Data/UpgradeDef.cs`：字段 `id, displayName, type(WandClick|ProducerBoost|AutoClicker), targetProducerId, baseCost, costMultiplier, multiplier, maxLevel, unlockRule`。
  - 创建：魔杖 5 级、6 个生产者各 3 级加速符文、1 个自动点击符（数值来自 §5）。
- **验收**：
  - [ ] 全部升级资产存在且类型分类正确
  - [ ] 自动点击符的解锁规则字段填写完整
- **产物**：升级 ScriptableObject 资产集

### T005 — BigDouble 的 Newtonsoft JsonConverter
- **状态**：☑ completed（验收并入 SmokeTest 4 条 RoundTrip 用例）
- **依赖**：T001
- **估时**：0.75h
- **描述**：`Scripts/Save/BigDoubleConverter.cs`，序列化为 `{"m":1.23,"e":45}` 形式，反序列化复原。
- **验收**：
  - [ ] EditMode 测试：`BigDouble(1e123)` 序列化-反序列化值不变
  - [ ] 0 与负数往返正确
- **产物**：Converter + 测试

### T006 — GameState 与 GameManager 单例
- **状态**：☑ completed（脚本完成；待用户在 Unity 中挂载到场景并拖引用）
- **依赖**：T003, T004
- **估时**：1.5h
- **描述**：
  - `Scripts/Core/GameState.cs`：纯数据类，含 `manaDust:BigDouble, totalManaEarned:BigDouble, philosopherStones:int, producerCounts:Dictionary<string,int>, upgradeLevels:Dictionary<string,int>, totalClicks:int, autoClickerOwned:bool, version:int`。
  - `Scripts/Core/GameManager.cs`：MonoBehaviour 单例（`DontDestroyOnLoad`），持有 `GameState` 与 ProducerDef/UpgradeDef 引用列表（Inspector 拖入）。提供 `Click()`、`TryBuyProducer(id)`、`TryBuyUpgrade(id)`、`TryPrestige()`。
  - 暂不写 Tick，下任务做。
- **验收**：
  - [ ] 主场景挂一个空 GameObject 名 `GameManager` 持有该脚本，Play 后 Console 打印初始状态
  - [ ] `Click()` 累加 manaDust 1，totalClicks+1
- **产物**：GameState + GameManager

### T007 — Tick 系统与产出公式
- **状态**：☑ completed（Tick + 产出公式 + 临时事件加成接口）
- **依赖**：T006
- **估时**：1h
- **描述**：
  - 在 GameManager 内启 `InvokeRepeating` 或协程，间隔 0.1s 触发 `Tick(0.1f)`。
  - `Tick(dt)`：遍历生产者，计算 `prod = baseProd * count * globalMult * dt`，累加 manaDust 与 totalManaEarned；globalMult = `1 + 0.02 * philosopherStones`，再乘所有 ProducerBoost 升级的 multiplier。
  - 处理事件加成（占位接口，由 T011 实现）。
- **验收**：
  - [ ] Inspector 手动把"魔法学徒"数量设为 5，Play 后 manaDust 每秒约 +1
  - [ ] Console 调试日志可关闭
- **产物**：Tick 循环

### T008 — 存档系统（Save / Load）
- **状态**：☑ completed（启动 Load + 每 10s 自动 Save + Quit/Pause 时 Save；菜单 `Alchemist/Save/*` 可手动管理）
- **依赖**：T005, T006
- **估时**：1.25h
- **描述**：
  - `Scripts/Save/SaveSystem.cs`：`Save(GameState)` 写入 `Application.persistentDataPath/save.json`；`Load()` 反序列化（若版本不符则按字段尝试兼容，否则忽略）。
  - GameManager：启动时 Load；每 10 秒自动 Save；OnApplicationQuit 时 Save。
- **验收**：
  - [ ] Play、点击 10 次、关闭，再次 Play 时 manaDust=10
  - [ ] 删除存档文件后启动恢复为初始状态
  - [ ] save.json 中能看到 BigDouble 的 `{m,e}` 结构
- **产物**：SaveSystem + 集成

### T009 — 主 UI 布局与点击炼金炉
- **状态**：☑ completed（代码构建 UI；后续标签页扩展到 ContentArea）
- **依赖**：T007
- **估时**：1.5h
- **描述**：
  - 在 Main 场景 Canvas 中按 §7 搭主布局：左 30% 炼金炉 + 数值，右 70% 标签页骨架（先做生产者页）。
  - 炼金炉用占位像素图（白色方块亦可），点击触发 `GameManager.Click()`。
  - 顶部数值 `ManaDustText` 与 `ProductionPerSecText` 通过脏标记每 0.1s 刷新一次（使用 NumberFormat）。
- **验收**：
  - [ ] Play 后点击炼金炉，左侧数字 +1
  - [ ] 通过 Inspector 设产出后右上角产出/秒数值正确显示
  - [ ] UI 不每帧刷新（用 Profiler 或日志确认）
- **产物**：Main 场景 UI 骨架

### T010 — 生产者列表 UI 与购买交互
- **状态**：☑ completed（滚动列表 + 6 行；按钮按余额置灰；未解锁的行整行隐藏）
- **依赖**：T009
- **估时**：1.5h
- **描述**：
  - Prefab `ProducerRow`：图标占位、名称、拥有数、产出/秒、成本、购买按钮。
  - 右侧"生产者"标签页根据 ProducerDef 列表实例化 6 行。
  - 未达解锁条件的整行隐藏（或显示"???"）。
  - 余额不足时按钮置灰；购买成功后扣费 + 计数 +1 + 刷新本行。
- **验收**：
  - [ ] 余额 < 15 时第一行学徒按钮置灰
  - [ ] 购买学徒后成本上升至 `15 * 1.12`，向上取整或显示原值（按代码实现一致）
  - [ ] 第二行"炼金工坊"在拥有 0 学徒时不可见，购买 1 学徒后出现
- **产物**：ProducerRow Prefab + 控制脚本

### T011 — 升级标签页与自动点击符
- **状态**：☑ completed（TabController 切换；UpgradesTab 列表；魔杖/加速符文/自动点击符全部接通）

> **备注**：加速符文 1/2/3 级解锁阶梯按"拥有该生产者数量 ≥ 10/25/50"判断（requirements §5.1）。
- **依赖**：T010
- **估时**：1.25h
- **描述**：
  - "升级"标签页列出可见升级（按解锁规则过滤）：魔杖、各生产者加速符文、自动点击符。
  - 购买后立刻生效：魔杖影响 `Click()` 产出；加速符文加入 globalMult/对应生产者乘区；自动点击符触发后启动一个每 0.5s 调 `Click()` 的协程。
  - 自动点击符的解锁条件：`totalClicks ≥ 500` 或玩家有 50,000 MD（按 §5.2）。
- **验收**：
  - [ ] 买魔杖 1 级后点击产出从 1 变成 2
  - [ ] 买学徒加速符文后产出/秒翻倍
  - [ ] 买自动点击符后点击数自动增加
- **产物**：升级 UI + 行为接入

### T012 — 随机事件系统
- **状态**：☑ completed（3 种事件 + 权重 + 横幅 UI；调试可在 Inspector 临时把 min/max interval 调到 5/10 秒冒烟）
- **依赖**：T011
- **估时**：1h
- **描述**：
  - `Scripts/Events/RandomEventSystem.cs`：每 55–125 秒按权重抽事件。
  - 三种事件按 §6 实现；GameManager 暴露 `globalProductionMult` 与 `clickMult` 临时加成接口。
  - UI：屏幕顶部横幅 + 倒计时文本。
- **验收**：
  - [ ] 调低间隔到 2-5 秒做冒烟测试，能看到三种事件按比例出现
  - [ ] "学徒灵感"立即给玩家可见的 MD 增量
  - [ ] "魔法暴走"期间产出/秒数字 ×7
- **产物**：事件系统 + 横幅 UI

### T013 — 转生（贤者之石）
- **状态**：☑ completed（GetPendingStones + TryPrestige + 转生标签页 + 确认弹窗）
- **依赖**：T011
- **估时**：1h
- **描述**：
  - "转生"标签页：显示当前可获得 PS 数量 `floor(sqrt(totalManaEarned/1e10))`。
  - `totalManaEarned ≥ 1e10` 时按钮可点；点击后弹确认对话框；确认后：
    - PS += 计算值
    - 重置：manaDust=0、totalManaEarned=0、producerCounts 清零、魔杖与加速符文升级清零、autoClickerOwned=false、totalClicks=0
    - 保留：PS 数、未来加入的成就
  - globalMult 计入 `+0.02 * PS`（T007 已预留）。
- **验收**：
  - [ ] 累计 1e10 MD 后按钮亮起且显示正确 PS 数
  - [ ] 转生后所有相关数据重置，PS 保留
  - [ ] 转生后产出/秒受 PS 加成（手动配置 PS=10 时全局产出 +20%）
- **产物**：转生 UI + 逻辑

### T014 — Windows 打包与冒烟通关 ⚠️ 最后执行
- **状态**：☐ pending（**必须在 T015–T025 全部完成后才做**）
- **依赖**：T013, T020, T025（即所有动画/音效/场景任务）
- **估时**：1h
- **描述**：见文件末尾「附录：T014 打包任务（最后做）」

## 下一步
运行 `/implement-task T015` 开始动画/音效增补。**不要直接做 T014**——打包是收尾任务，要等动画与场景都做完。

---

## 增补模块：动画与音效（来源 requirements §10）

### T015 — UITween 工具类
- **状态**：☑ completed
- **依赖**：T009
- **估时**：0.75h
- **描述**：`Scripts/Util/UITween.cs`：静态方法集，封装常用 Coroutine 缓动：
  - `ScalePunch(Transform t, float low=0.92f, float total=0.12f)`
  - `MoveAndFade(RectTransform rt, Vector2 deltaPx, float duration, System.Action onDone)`
  - `ColorFlash(Graphic g, Color flash, float duration)`
  - `LerpNumber(System.Action<double> setter, double from, double to, float duration)`
- **验收**：
  - [ ] 在临时测试脚本上手动调用每个方法，UI 表现符合预期
  - [ ] 同一 transform 上多次 ScalePunch 不互相打架（同一 key 取消旧协程）
- **产物**：`UITween.cs`

### T016 — 点击反馈（炼金炉缩放 + 飘字 +N）
- **状态**：☑ completed
- **依赖**：T015
- **估时**：0.75h
- **描述**：
  - 修改 `MainUIBuilder` 的炼金炉按钮 OnClick：除 `GameManager.Click()` 外调用 `UITween.ScalePunch(cauldron)`
  - 生成 "+N" 飘字 GameObject（Text）放在炼金炉上方，随机水平偏移 ±20px，向上飘 60px 并淡出 700ms
- **验收**：
  - [ ] 点击炼金炉时按钮明显缩放回弹
  - [ ] 屏幕上能看到"+1"(或被加成后的"+20") 向上飘出并消失
  - [ ] 连击 10 次不卡顿，没有 GameObject 泄漏（Profiler 看 Hierarchy 数量回落）
- **产物**：MainUIBuilder 修改 + 新 `Scripts/UI/FloatingNumber.cs`

### T017 — 购买成功反馈 & 数值滚动
- **状态**：☑ completed（数值滚动在 T016 一并完成）
- **依赖**：T015, T010, T011
- **估时**：1h
- **描述**：
  - `TryBuyProducer/TryBuyUpgrade` 成功时，对应行调用 `UITween.ColorFlash`（背景闪暖色）+ 成本数字 `ScalePunch(1.2 → 1.0)`
  - 左侧"魔力尘"主数字、产出/秒、点击+N 改用 `UITween.LerpNumber` 平滑过渡（保留一个 double 缓存值）
- **验收**：
  - [ ] 成功购买学徒时该行有可见暖色一闪
  - [ ] 余额数字从 100 跳到 85 时是 150ms 内平滑滚动而非突变
- **产物**：MainUIBuilder/ProducerRow/UpgradeRow 修改

### T018 — 事件横幅滑入/滑出 & 转生闪光
- **状态**：☑ completed
- **依赖**：T015, T012, T013
- **估时**：0.75h
- **描述**：
  - 修改 `EventBannerUI`：显示/隐藏走 `UITween.MoveAndFade`，从 y=-40 滑入到 y=0，反向滑出
  - 修改 `PrestigeTab.TryPrestige` 成功路径：在最上层 Canvas 上插一个全屏白色 Image，alpha 1→0 over 1.2s（紫色辉光 lerp）
- **验收**：
  - [ ] 事件触发时横幅从下方明显滑入
  - [ ] 事件结束时横幅滑出再隐藏
  - [ ] 转生时屏幕一闪白光，1.2s 内淡出
- **产物**：EventBannerUI、PrestigeTab 修改；新 `Scripts/UI/PrestigeFlash.cs`

### T019 — AudioManager 音效框架
- **状态**：☑ completed（待用户在场景挂载 AudioManager 脚本）
- **依赖**：T009
- **估时**：1h
- **描述**：
  - `Scripts/Audio/AudioManager.cs`：单例 MonoBehaviour，启动时扫描 `Resources/Audio/SFX/` 和 `Resources/Audio/BGM/`，按文件名 key 缓存 AudioClip
  - API：`Play(string key)`、`PlayBGM(string key)`、`SetMasterVolume/SetSfxVolume/SetBgmVolume`
  - SFX 用 AudioSource 池（4 路），BGM 单 AudioSource 循环
  - 找不到 key 时静默不报错，仅 Debug.Log 一次警告
- **验收**：
  - [ ] 放入测试 wav 后 `AudioManager.Play("test")` 能听到声音
  - [ ] 连续触发 10 次 click 音不爆音
  - [ ] 缺资源时不抛异常
- **产物**：`AudioManager.cs` + `Assets/Resources/Audio/{SFX,BGM}/` 目录

### T020 — 音效接入 & BGM
- **状态**：☑ completed（代码侧；用户需按 `docs/T020-audio-setup.md` 准备资源）
- **依赖**：T019, T011, T012, T013
- **估时**：0.5h
- **描述**：
  - 点击炼金炉 → Play("click")
  - 购买成功 → Play("buy")；转生成功 → Play("prestige")
  - 三种事件触发 → Play("event_surge"/"event_insight"/"event_meteor")
  - 启动时 PlayBGM("bgm_main")
  - 用户需自备资源放入 `Assets/Resources/Audio/SFX/click.wav` 等；说明文档放 `docs/T020-audio-setup.md`
- **验收**：
  - [ ] 接入点全部正确触发（即使没资源也不报错）
  - [ ] 放入资源后能听到对应音效
- **产物**：GameManager、PrestigeTab、RandomEventSystem 接入；音频资源占位说明

## 下一步
运行 `/implement-task T015` 开始第一个动画任务。打包任务 T014 待动画音效完成后再做。

---

## 增补模块：游戏场景（来源 requirements §11）

> **顺序**：先做完 T015–T020 动画/音效，再做下面场景任务。

### T021 — 美术资源占位与目录
- **状态**：☑ completed（待用户在 Unity 菜单 `Alchemist/Art/Generate Placeholders`）
- **依赖**：T009
- **估时**：0.5h
- **描述**：
  - 建立目录 `Assets/Art/Sprites/{Floor,Cauldron,Characters}/`
  - 编写 Editor 工具 `PlaceholderSpriteGen.cs`：菜单 `Alchemist/Art/Generate Placeholders` 程序生成纯色方块占位精灵（floor.png 32×32 棕、cauldron.png 64×64 紫、char_apprentice.png 32×32 + 文字"学"…共 6 种角色），保存为 PNG 并 AssetDatabase.ImportAsset
  - 写 `docs/T021-art.md` 说明：如何把真正的 CC0 素材替换掉占位（同名覆盖即可）
- **验收**：
  - [ ] 菜单执行后 `Assets/Art/Sprites/...` 出现 8 张占位 PNG
  - [ ] 在 Scene 中拖一张到 SpriteRenderer 能看到颜色与文字
- **产物**：占位生成工具 + 文档

### T022 — Scene 视图构建（场景相机 + 地板 + 熔炉）
- **状态**：☑ completed（待用户挂 WorkshopRoot）
- **依赖**：T021
- **估时**：1h
- **描述**：
  - 新脚本 `Scripts/Scene/WorkshopSceneBuilder.cs`，Awake 时程序构建 2D 顶下视角场景：
    - 设置 `Camera.main` 正交、size=4
    - 平铺地板（10×7 个 floor sprite）
    - 中央放魔法熔炉 SpriteRenderer，绑定 `BoxCollider2D` + 子对象 `CauldronClickHandler`（OnMouseDown → GameManager.Click()）
  - 沿用 §11.2 的固定坐标
- **验收**：
  - [ ] Play 后看到地板 + 中央熔炉
  - [ ] 点熔炉魔力尘 +N（沿用原 Click 流程，含飘字 T016）
- **产物**：`WorkshopSceneBuilder.cs` + `CauldronClickHandler.cs`

### T023 — 工作站位与小人渲染
- **状态**：☑ completed
- **依赖**：T022
- **估时**：1.5h
- **描述**：
  - `Scripts/Scene/WorkStation.cs`：一个工作站位 MonoBehaviour，配置 `producerId` + 位置；根据 `GameManager.State.GetProducerCount(producerId)` 实时（每 0.5s）同步小人数量，最多 8 个，按 2×4 网格摆放
  - `Scripts/Scene/Worker.cs`：单个小人 SpriteRenderer + 微动协程（1-2s 随机一次轻微 scale 0.95→1.05 或左右翻转）
  - WorkshopSceneBuilder 在 §11.2 6 个坐标处实例化 6 个 WorkStation
  - 超过 8 个时在站位下方显示"x N"小文本
- **验收**：
  - [ ] 拥有 3 个学徒 → 学徒站位出现 3 个小人
  - [ ] 拥有 10 个 → 显示 8 个小人 + "x10" 文本
  - [ ] 小人在原地有可见微动，节奏不同步
- **产物**：WorkStation/Worker 脚本

### T024 — 站位产出飘字
- **状态**：☑ completed
- **依赖**：T023, T015
- **估时**：0.5h
- **描述**：
  - WorkStation 内部每 2-3 秒（拥有数 ≥1 时）触发一次 "+N" 飘字（N = 该站位每秒产出，显示用 NumberFormat），从站位飞向熔炉位置并淡出 800ms
  - 复用 T015 的 UITween / T016 的 FloatingNumber 实现
- **验收**：
  - [ ] 多个站位同时有飘字向熔炉飞，不阻塞
  - [ ] 产出为 0 的站位不出飘字
- **产物**：WorkStation 增量修改

### T025 — UI 抽屉化（侧栏折叠 + HUD 常驻）
- **状态**：☑ completed
- **依赖**：T022
- **估时**：1.25h
- **描述**：
  - 重构 `MainUIBuilder`：
    - 删除左侧 30% 面板（炼金炉移到场景）
    - 顶部保留窄 HUD 条（魔力尘 / 产出/秒 / 点击 +N）
    - 右侧加一个「☰」开关按钮，点击展开/收起原 TabController 内容（用 UITween 滑动）
  - 收起时场景全显；展开时占右侧约 40% 宽度
- **验收**：
  - [ ] 默认侧栏收起，仅顶部 HUD + 中央场景
  - [ ] 点 ☰ 侧栏平滑滑入，包含生产者/升级/转生三个标签页
  - [ ] 再点 ☰ 平滑滑出
- **产物**：MainUIBuilder / TabController 修改

## 下一步（增补后）
按序运行：`/implement-task T015` → T016 → ... → T020 → T021 → ... → T025 → T014（打包）。

---

## 附录：T014 打包任务（最后做）

> 把它放在文档末尾以避免被误选为"下一个任务"。`implement-task` 默认按 ID 顺序取，但 T014 显式声明依赖 T020/T025，未完成时不能取它。

### T014 详细描述
- Build Settings 选 Windows x64，输出到 `Build/Alchemist/`
- 调整图标与窗口名为「炼金术士」
- 通关一次 MVP（约 1-2 小时或调速验证）：从 0 到完成 1 次转生，记录任何阻塞点为新任务
- **验收**：
  - [ ] `Alchemist.exe` 双击可启动并通关
  - [ ] 存档在重启后保留
  - [ ] 无 Console Error / 红色异常
- **产物**：Windows 构建产物

