# T021 — 美术资源说明

## 占位资源（自动生成）

代码侧已提供 Editor 工具，**菜单**：

`Alchemist → Art → Generate Placeholders`

执行后会在 `Assets/Resources/Art/Sprites/` 生成：

- `Floor/floor.png` — 32×32 棕色地板块
- `Cauldron/cauldron.png` — 64×64 紫色熔炉（带圆形高亮）
- `Characters/char_apprentice.png` 等 6 张 — 32×32 不同颜色的小人占位

> 必须放在 `Resources/` 下，运行时通过 `Resources.Load<Sprite>("Art/Sprites/...")` 加载。

导入设置已自动配置为 Sprite + Point filter + 32 PPU，像素风可直接用。

## 替换为真实美术

去 [itch.io 免费像素包](https://itch.io/game-assets/free/tag-pixel-art) 或 OpenGameArt 下载素材，**同名覆盖** PNG 即可，不需要改代码：

- 地板/墙体：搜 `pixel dungeon floor`、`fantasy floor tiles`
- 熔炉：搜 `cauldron sprite`、`magic forge`
- 小人：搜 `pixel character`、`tiny wizard`，建议每个尺寸不超过 32×32

如果新资源尺寸与占位不同（如熔炉 128×128），WorkshopSceneBuilder 会根据 Sprite 实际像素自动缩放；但**像素密度（PPU）保持 32** 以维持风格统一。
