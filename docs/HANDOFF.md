# 手动介入清单（autonomous run 终止后续做）

> 本次 `/karpathy-guidelines` 自动化推进了 T015–T025（动画/音效/场景重构）。代码侧全部完成，下面是你需要在 Unity 中做的所有事，按顺序操作。

## 准备

1. **删档**：菜单 `Alchemist → Save → Delete Save`（场景结构大改，旧存档可能不兼容）

## A. 一次性资源生成

2. **生成占位精灵**：菜单 `Alchemist → Art → Generate Placeholders`
   - 完成后 `Assets/Resources/Art/Sprites/` 下应有 8 张 PNG
   - Console 会打印 `[PlaceholderSpriteGen] Generated...`

3. **重新生成升级资产**（如果你之前没跑过新的命名）：菜单 `Alchemist → Seed Upgrades (overwrite)`

## B. 场景里挂载新组件

打开 `Assets/Scenes/Main.unity`。

4. **新建 `AudioManager` 对象**（T019/T020）
   - Hierarchy 右键 → Create Empty → 命名 `AudioManager`
   - 拖 `Assets/Scripts/Audio/AudioManager.cs` 上去
   - 调 Master/Sfx/Bgm Volume（默认即可）

5. **新建 `WorkshopRoot` 对象**（T022/T023/T024）
   - Hierarchy 右键 → Create Empty → 命名 `WorkshopRoot`
   - 拖 `Assets/Scripts/Scene/WorkshopSceneBuilder.cs` 上去
   - 这一个脚本会自动建造地板/熔炉/6 个工作站位

6. **确认旧对象保留**
   - `GameManager`（含 Producer/Upgrade 列表）— 必须保留
   - `UIRoot`（含 MainUIBuilder / TabController / ProducersTab / UpgradesTab / PrestigeTab / EventBannerUI）— 必须保留
   - `EventManager`（RandomEventSystem）— 必须保留
   - `__SmokeTest` — **删除**（已无用且会执行旧测试）

7. **删档再 Play** 一次（GameState 默认值变了，必须清掉旧存档）

## C. 音效资源（可选，没有也不会报错）

8. 按 `docs/T020-audio-setup.md` 准备音频文件放入 `Assets/Resources/Audio/SFX/` 与 `Assets/Resources/Audio/BGM/`
   - 不放：游戏照常运行，只是没声音
   - 真实美术资源同样在 `Assets/Resources/Art/Sprites/...` **同名覆盖** PNG 即可（详见 `docs/T021-art.md`）

## D. 验收预期

Play 后应看到：

- **场景区**（无 UI 覆盖处）：俯视地板 + 中央紫色熔炉 + 6 个角色站位
  - 点熔炉 → 魔力尘 +N（音效 click，若有）
  - 站位上有小人原地微动（缩放/翻转）
  - 站位每 2-3 秒冒出 "+N" 飘字飞向熔炉
- **顶部 HUD**：魔力尘 / 产出/秒 / 点击+N，数字平滑滚动
- **右上角 ☰ 按钮**：点击 → 侧栏从右侧滑入，包含「生产者 / 升级 / 转生」3 个标签页（行为同旧版）
- **事件**：横幅从底部滑入/滑出
- **转生**：触发后全屏白→紫淡出闪光

## E. 已知限制

- 占位精灵的小人区分度低（仅颜色不同），建议尽快替换真实素材
- 数字滚动可能在大数（1e15+）时略卡，目前用 double 计算（已在 NumberFormat 范围内）
- 音效缺资源时 Console 第一次会打印 `not found — silent`（每个 key 只警告一次，符合设计）

## F. 完成后继续

E 与 F 都验证通过、且玩起来感觉对了之后，可以让 Claude 继续做 **T014（Windows 打包）**——这是最后一个待办任务。
