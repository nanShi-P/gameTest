# 进度留痕（autonomous run）

> 由 `/karpathy-guidelines` 模式触发的自动化推进。中断后可从最后一条续做。

## Run 1 — 2026-05-24

**目标**：从 T015 起完成所有未完成任务直到必须人工介入。
**结果**：T015–T025 代码侧全部完成（10 个任务）。T014（Windows 打包）必须手动，已终止。

### 完成清单

| 任务 | 关键产物 |
|---|---|
| T015 | `Scripts/Util/UITween.cs` — ScalePunch / MoveAndFade / ColorFlash / LerpNumber |
| T016 | `Scripts/UI/FloatingNumber.cs`；MainUIBuilder 接入炼金炉缩放+飘字 |
| T017 | ProducerRow/UpgradeRow 购买成功 ColorFlash + 成本数字 ScalePunch；T016 同步做了数值滚动 |
| T018 | EventBannerUI 改为底部滑入/滑出；`PrestigeFlash.cs` 全屏白→紫闪光 |
| T019 | `Scripts/Audio/AudioManager.cs` — Resources 自动扫描、SFX 池、BGM 单 source、缺资源静默 |
| T020 | GameManager Click/TryBuy/TryPrestige 接入；RandomEventSystem 三种事件音；AudioManager.Start 启动 bgm_main；`docs/T020-audio-setup.md` |
| T021 | `Assets/Editor/PlaceholderSpriteGen.cs` 菜单 `Alchemist/Art/Generate Placeholders` 输出到 `Resources/Art/Sprites/`；`docs/T021-art.md` |
| T022 | `Scripts/Scene/WorkshopSceneBuilder.cs`（相机/地板/熔炉/CauldronClickHandler） |
| T023 | `Scripts/Scene/WorkStation.cs` + Worker 微动协程；6 站位环形布局 |
| T024 | `Scripts/Scene/WorldFloatingNumber.cs`；WorkStation.EmitLoop 每 2-3s 飘字飞向熔炉 |
| T025 | MainUIBuilder 重构：顶部 HUD + 右侧可折叠侧栏；左侧旧炼金炉已删（场景里 Cauldron 接替） |

### 必须人工的事 → 见 `docs/HANDOFF.md`

总结：场景重构后必须重新挂 AudioManager / WorkshopRoot、跑两个 Editor 菜单（Seed 占位精灵）、删档；然后 Play 验证；之后才能做 T014 打包。
