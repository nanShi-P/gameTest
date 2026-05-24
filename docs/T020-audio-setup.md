# T020 — 音效资源准备说明

代码侧已完成所有音效触发点接入；要听到声音，你需要把音频文件放入对应目录。**不放也不会报错**，只是听不到声音。

## 必需的文件

放入 `Assets/Resources/Audio/SFX/`（文件名必须严格一致，格式 `.wav` / `.ogg` / `.mp3`，**不带扩展名作为 key**）：

| 文件名 | 触发场景 | 推荐音色 |
|---|---|---|
| `click.wav` | 点击炼金炉 | 短促"叮"或"咕"，<200ms |
| `buy.wav` | 购买生产者/升级成功 | 收银/咔哒，<300ms |
| `prestige.wav` | 转生 | 1-2 秒长音、辉煌感 |
| `event_surge.wav` | 魔法暴走 | 嗡鸣/能量充能 |
| `event_insight.wav` | 学徒灵感 | 灵光一闪 |
| `event_meteor.wav` | 流星雨 | 陨石坠落 |

放入 `Assets/Resources/Audio/BGM/`：

| 文件名 | 用途 |
|---|---|
| `bgm_main.ogg` | 主场景循环 BGM（推荐 ogg 节省体积） |

## 推荐来源

- https://freesound.org/  搜 "click pop"、"magic chime"、"meteor impact"，筛选 CC0
- https://opengameart.org/  分类 Sound Effects，过滤 CC0/Public Domain
- BGM 关键词：「magic ambient loop」「alchemy workshop」

## 验收

放入文件后回 Unity：
1. Console 应打印 `[AudioManager] SFX loaded: N, BGM loaded: M`，N/M 应大于 0
2. 进入 Play：
   - 听到 BGM 循环
   - 点炼金炉听到 click
   - 买生产者听到 buy
   - 转生听到 prestige
   - 等事件触发或调短间隔听到 3 种事件音

## 在 Unity 中挂载 AudioManager

代码已写好，但 `AudioManager` 是 MonoBehaviour 单例，需要场景里有一个对象挂着它：

1. Hierarchy → Create Empty → 命名 `AudioManager`
2. 拖 `Assets/Scripts/Audio/AudioManager.cs` 到它上面
3. （可选）在 Inspector 调 Master/Sfx/Bgm Volume

完成。
