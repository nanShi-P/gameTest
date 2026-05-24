# 后台管理系统需求文档 — Alchemist Admin

> 最后更新：2026-05-24
> 类型：独立 Web 后台 + 配置发布管道
> **重要**：本文档与 `docs/requirements.md`（游戏需求）平行，互不覆盖。

## 1. 定位

游戏 Alchemist 的配置编辑后台。**不在游戏运行时使用**，是策划/开发者用来编辑游戏内数值的工具，配置发布后玩家客户端启动时自动拉取生效。

## 2. 技术栈

| 层 | 选型 |
|---|---|
| 后端 | **.NET 8 Web API**（C#，类型可与 Unity 共享，避免漂移） |
| 前端 | **React + Vite + TypeScript + shadcn/ui** |
| DB | **SQLite**（文件式，零运维） |
| 部署 | **本地 docker-compose**（仅自己用，不对外） |
| 发布目标 | **GitHub Pages**（静态 JSON，公网可访问，CDN 加速） |
| 发布机制 | **后端直接执行 `git commit + push`** 到指定仓库的 `gh-pages` 分支 |

仓库结构：游戏与后台共一个仓库或拆两个仓库都可，**建议拆**：
- `gameTest`（游戏，Unity 项目）
- `gameTest-admin`（后台 .NET + React）
- `gameTest-config`（配置发布仓库，启用 GitHub Pages，仅含 JSON）

## 3. 数据流

```
策划在后台编辑配置
  ↓ Save（落入 SQLite）
策划点 "Publish"
  ↓ 后端：导出 JSON → git commit → git push origin gh-pages
GitHub Pages 自动更新（约 30 秒）
  ↓
游戏客户端启动
  ↓ HTTP GET https://<user>.github.io/gameTest-config/latest.json（3s 超时）
  ├─ 成功 → 写 Application.persistentDataPath/config.json，启动游戏
  └─ 失败 → L2: 本地缓存；L3: Resources 内置 default_config.json
```

## 4. 后台功能（第一版）

### 4.1 资源管理 (Resources)
- 列表查看、新增、编辑、删除
- 字段：`id, displayName, initialValue, iconName, maxValue?, precision?`
- 当前游戏中只有「魔力尘」「贤者之石」，第一版可直接配死，但表结构和 API 要预留

### 4.2 生产者管理 (Producers)
- CRUD + 排序（拖拽改 order）
- 字段：`id, displayName, baseCost, baseProduction, costMultiplier(默认1.12), unlockProducerId, unlockProducerCount, unlockManaTotal, order, enabled`
- 字段语义与 `Assets/Scripts/Data/ProducerDef.cs` 一一对齐

### 4.3 升级管理 (Upgrades)
- CRUD
- 字段：`id, displayName, type(WandClick|ProducerBoost|AutoClicker), targetProducerId, baseCost, costMultiplier, multiplier, maxLevel, unlockCountThreshold, unlockManaTotal`
- `type` 用枚举下拉；`targetProducerId` 只在 ProducerBoost 时显示，下拉从 Producers 表来

### 4.4 随机事件管理 (Events)
- CRUD
- 字段：`id, kind(ManaSurge|ApprenticeInsight|MeteorShower|自定义), displayName, weight, duration, effectType, effectMagnitude`
- 全局参数：`minInterval, maxInterval, firstDelay`
- 第一版固定 3 种 kind，但 schema 预留自定义扩展

### 4.5 全局参数 (Globals)
- `prestigeThreshold`（默认 1e9）
- `prestigePerStoneBonus`（默认 0.02）
- `tickInterval`（默认 0.1）
- `autoClickerInterval`（默认 0.5）

### 4.6 发布管理 (Publish)
- 「Publish」按钮 → 后端：
  1. 从 DB 全量导出 → 生成 `latest.json`（包含所有上述 4.1–4.5）
  2. 同时生成带版本号文件 `v{N}.json`，N 自增
  3. 写入本地 `config-repo/` 目录
  4. 执行 `git add . && git commit -m "publish v{N}" && git push`
- 发布历史列表：版本号、发布时间、操作人、变更摘要
- 「Rollback」按钮：选择历史版本 → 写回 `latest.json` → push

### 4.7 v2 不做（明确排除）
- ❌ 局外技能树（v2 模块，schema 预留 `skills` 节点表）
- ❌ 数值模拟器（v3 想要再加）
- ❌ 玩家存档管理
- ❌ 灰度发布 / A/B 测试
- ❌ 多语言文案

## 5. 权限与登录

第一版**单用户、无登录**。后端只在本地 docker-compose 跑，监听 `127.0.0.1:5000`，外网访问不到。

> 未来若多人协作再加 OAuth / 简单密码 token。

## 6. 游戏客户端改造（独立任务到 game tasks.md）

> 这部分实际落到游戏侧任务，不在 admin tasks 里。但为了完整性记一下：

- 新增 `Scripts/Config/ConfigLoader.cs`，启动时三层兜底加载 JSON
- 新增 `Scripts/Config/GameConfig.cs`（纯数据 DTO，对应后台 schema）
- `GameManager.Awake` 改为 await ConfigLoader → 用配置 new GameState/ProducerDef 列表
- 当前 ScriptableObject 资产保留作为 `default_config.json` 的源（提供一个 Editor 工具 SO→JSON 导出）
- 增加加载界面（覆盖 Canvas 一张「加载中…」直到 ConfigLoader 完成）
- 配置 URL 用 ScriptableObject 或 PlayerPrefs 存储，方便切换正式/测试环境

## 7. JSON Schema（核心）

```jsonc
{
  "version": 12,
  "generatedAt": "2026-05-24T10:30:00Z",
  "globals": {
    "prestigeThreshold": 1e9,
    "prestigePerStoneBonus": 0.02,
    "tickInterval": 0.1,
    "autoClickerInterval": 0.5
  },
  "resources": [
    { "id": "manaDust", "displayName": "魔力尘", "initialValue": 0 }
  ],
  "producers": [
    {
      "id": "apprentice", "displayName": "魔法学徒",
      "baseCost": 15, "baseProduction": 0.2, "costMultiplier": 1.12,
      "unlockProducerId": "", "unlockProducerCount": 0, "unlockManaTotal": 10,
      "order": 0, "enabled": true
    }
  ],
  "upgrades": [
    {
      "id": "wand", "displayName": "魔杖强化", "type": "WandClick",
      "targetProducerId": "",
      "baseCost": 50, "costMultiplier": 8, "multiplier": 2, "maxLevel": 5,
      "unlockCountThreshold": 0, "unlockManaTotal": 0
    }
  ],
  "events": {
    "settings": { "minInterval": 55, "maxInterval": 125, "firstDelay": 30 },
    "items": [
      { "id": "surge", "kind": "ManaSurge", "displayName": "魔法暴走 ×7",
        "weight": 40, "duration": 30, "effectType": "GlobalMult", "effectMagnitude": 7 }
    ]
  }
}
```

## 8. 后端项目结构

```
gameTest-admin/
├─ docker-compose.yml
├─ backend/                       # .NET 8 Web API
│  ├─ Program.cs
│  ├─ Models/                     # 实体 + DTO（C# 类，可与 Unity 共享文件）
│  ├─ Data/AdminDbContext.cs     # EF Core + SQLite
│  ├─ Controllers/
│  │  ├─ ResourcesController.cs
│  │  ├─ ProducersController.cs
│  │  ├─ UpgradesController.cs
│  │  ├─ EventsController.cs
│  │  ├─ GlobalsController.cs
│  │  └─ PublishController.cs    # 触发 git push
│  └─ Services/
│     ├─ JsonExporter.cs
│     └─ GitPublisher.cs          # LibGit2Sharp
├─ frontend/                      # React + Vite + TS
│  ├─ src/
│  │  ├─ api/                     # axios + 类型生成
│  │  ├─ pages/{Resources,Producers,Upgrades,Events,Globals,Publish}
│  │  ├─ components/              # shadcn/ui 组件
│  │  └─ App.tsx
└─ config-repo/                   # git 子模块，指向 gameTest-config
```

## 9. 非功能需求

- 后端启动 < 3s
- 前端开发热重载
- JSON 导出 < 100KB（当前规模下远低于）
- 单次 publish（含 git push） < 10s

## 10. 不做的事（本期 v1 显式排除）

- ❌ 局外技能树（v2）
- ❌ 数值模拟 / 平衡分析
- ❌ 玩家存档管理
- ❌ 灰度发布、A/B 测试
- ❌ 多语言
- ❌ 多人协作 / 登录鉴权
- ❌ 自动化 CI 测试（手动 Postman 验证 API）
- ❌ Docker 镜像推到镜像仓库（本地构建即用）

## 下一步
运行 `/plan-tasks`（或直接说"拆任务"）把本文档转为开发任务清单到 `docs/admin-tasks.md`（**不与 `docs/tasks.md` 混淆**）。
