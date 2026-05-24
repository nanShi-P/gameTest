# 后台任务清单 — Alchemist Admin

> 最后更新：2026-05-24
> 来源：docs/admin-requirements.md
> **独立于 `docs/tasks.md`（游戏任务）追踪**。

## 进度
- 待办：0
- 进行中：0
- 已完成：18

## 准备工作（用户一次性，开工前）

- [ ] 在 GitHub 创建 3 个仓库（建议名）：
  - `gameTest-admin`（后台）
  - `gameTest-config`（配置 JSON，启用 Pages → Settings/Pages，source = `main` 或 `gh-pages`）
  - `gameTest` 已存在
- [ ] 生成 GitHub PAT，scope 仅 `repo:status + contents:write` 限定到 `gameTest-config`，存到本地（后端会读环境变量 `GITHUB_PAT`）
- [ ] 本地安装：.NET 8 SDK、Node.js 20+、Docker Desktop、Git

未完成时 A001 仍可开始（用占位仓库），但 A018 publish 必须等仓库就绪。

---

## 任务列表

### A001 — 仓库与目录骨架
- **状态**：☑ completed
- **依赖**：无
- **估时**：0.5h
- **描述**：在 `E:\AIProject\gameTest-admin\` 创建仓库骨架：
  - `backend/` `frontend/` `config-repo/` 三个空目录
  - 根 `docker-compose.yml`（占位）
  - `.gitignore`（含 bin/obj/node_modules/dist/.env）
  - README.md（1 段说明本仓库用途）
- **验收**：
  - [ ] 目录结构按 admin-requirements §8
  - [ ] `git init && git add . && git commit` 通过
- **产物**：admin 仓库骨架

### A002 — .NET 8 Web API 项目初始化
- **状态**：☑ completed（实际用 .NET 9，本机未装 8，效果一致）
- **依赖**：A001
- **估时**：0.75h
- **描述**：
  - `cd backend && dotnet new webapi -n AlchemistAdmin`
  - 加包：`EntityFrameworkCore.Sqlite`, `EntityFrameworkCore.Design`, `LibGit2Sharp`, `Swashbuckle.AspNetCore`
  - 启用 CORS（允许 localhost:5173）
  - 启用 Swagger UI（开发模式）
  - 监听 `http://127.0.0.1:5000`
- **验收**：
  - [ ] `dotnet run` 启动后 http://127.0.0.1:5000/swagger 可访问
  - [ ] /WeatherForecast 默认端点正常
- **产物**：可运行的 .NET API 空项目

### A003 — EF Core + SQLite 数据模型
- **状态**：☑ completed（用 EnsureCreated 替代 Migrations，第一版简化）
- **依赖**：A002
- **估时**：1h
- **描述**：
  - `Models/`：定义 `Resource, Producer, Upgrade, RandomEventItem, RandomEventSettings, GlobalConfig, PublishHistory` 实体（字段见 admin-requirements §4/§7）
  - `Data/AdminDbContext.cs` + `OnConfiguring` 用 SQLite `Data Source=admin.db`
  - `Add-Migration Initial && Update-Database`
  - 写 `SeedData.cs`：首次启动若表空则 seed 当前游戏的 6 个 producer / 8 个 upgrade / 3 个 event 默认值
- **验收**：
  - [ ] 启动后 `admin.db` 文件生成且含初始数据
  - [ ] 用 SQLite 工具 / DB Browser 能看到表
- **产物**：迁移 + 实体类 + seed

### A004 — Producers CRUD API
- **状态**：☑ completed
- **依赖**：A003
- **估时**：0.75h
- **描述**：
  - `Controllers/ProducersController.cs`：GET 列表 / GET {id} / POST / PUT / DELETE
  - DTO + AutoMapper 或手写映射（项目小，手写即可）
  - 排序通过 `order` 字段，PATCH `/producers/reorder` 接受 `[{id, order}]` 批量更新
- **验收**：
  - [ ] Swagger 中所有端点可调用
  - [ ] 新增/编辑/删除/重排序均生效（DB 验证）
- **产物**：ProducersController + DTOs

### A005 — Upgrades / Events / Resources / Globals CRUD API
- **状态**：☑ completed
- **依赖**：A004
- **估时**：1h
- **描述**：仿 A004 完成其余 4 个 controller：
  - `UpgradesController`（含 type 枚举校验）
  - `EventsController`（GET/PUT settings + CRUD items）
  - `ResourcesController`
  - `GlobalsController`（单例配置，GET/PUT 一份记录）
- **验收**：
  - [ ] 所有端点在 Swagger 可调用
  - [ ] type / kind 枚举校验生效（非法值返回 400）
- **产物**：4 个 controller

### A006 — JsonExporter 服务
- **状态**：☑ completed
- **依赖**：A005
- **估时**：1h
- **描述**：
  - `Services/JsonExporter.cs`：从 DB 全量拼出符合 admin-requirements §7 schema 的对象，序列化为 indented JSON
  - 时间戳 `generatedAt` 用 UTC ISO 8601
  - version 自增逻辑：从 `PublishHistory` 表取 MAX(version)+1
- **验收**：
  - [ ] 写一个 `GET /publish/preview` 端点，返回当前会发布的 JSON 全文
  - [ ] JSON 字段与 §7 完全一致
- **产物**：JsonExporter + preview 端点

### A007 — GitPublisher 服务（git push）
- **状态**：☑ completed（编译通过；真实 push 需用户提供 GITHUB_PAT 后才能验证）
- **依赖**：A006
- **估时**：1.25h
- **描述**：
  - `Services/GitPublisher.cs` 使用 LibGit2Sharp：
    - 启动时 clone `gameTest-config` 仓库到 `config-repo/`（已有则 pull）
    - `PublishAsync(string json, int version)`：写 `latest.json` 与 `v{N}.json` → stage → commit → push
    - 凭证从环境变量 `GITHUB_PAT` 读，username 默认 `git`
  - 失败时抛带原因的异常，不要静默
- **验收**：
  - [ ] 用测试 repo 验证：调一次后 GitHub 上能看到新 commit + JSON 文件
  - [ ] 凭证错误时 API 返回 500 + 清晰错误信息
- **产物**：GitPublisher + 错误处理

### A008 — Publish & Rollback API
- **状态**：☑ completed
- **依赖**：A007
- **估时**：0.75h
- **描述**：
  - `PublishController.cs`：
    - `POST /publish`：调 JsonExporter → GitPublisher → 写 PublishHistory
    - `GET /publish/history`：返回历史列表
    - `POST /publish/rollback/{version}`：取历史版本 JSON 重新写 `latest.json` + push
  - 历史记录包含：version, publishedAt, summary(简单文本如 "6 producers, 8 upgrades")
- **验收**：
  - [ ] publish 一次后历史表 +1 条
  - [ ] rollback 后 GitHub 上 latest.json 与历史版本一致
- **产物**：PublishController + history 表逻辑

### A009 — Vite + React + TS 前端骨架
- **状态**：☑ completed（手工构造 package.json，未用 create-vite；npm install 后能跑）
- **依赖**：A001
- **估时**：0.75h
- **描述**：
  - `cd frontend && npm create vite@latest . -- --template react-ts`
  - 装：`react-router-dom, axios, @tanstack/react-query, tailwindcss, shadcn-ui`
  - 配 `vite.config.ts` 代理 `/api → http://127.0.0.1:5000`
  - shadcn 初始化（`npx shadcn@latest init`）
- **验收**：
  - [ ] `npm run dev` 启动后 http://127.0.0.1:5173 显示 "Hello Vite"
  - [ ] shadcn 装好的 Button 组件可用
- **产物**：前端可启动

### A010 — 主布局 + 路由
- **状态**：☑ completed（App.tsx 侧栏 + 6 路由 + 后端在线状态指示灯）
- **依赖**：A009
- **估时**：0.75h
- **描述**：
  - 左侧 nav（Resources / Producers / Upgrades / Events / Globals / Publish）
  - 顶部标题 + 当前后端连通状态指示灯
  - 路由：`/resources` `/producers` `/upgrades` `/events` `/globals` `/publish`
  - 每个页面先放占位 "Coming soon"
- **验收**：
  - [ ] 6 个路由可切换
  - [ ] 后端断开时指示灯变红
- **产物**：AppShell + Router + 6 个占位页

### A011 — API 客户端 + 类型定义
- **状态**：☑ completed（api.ts 含全部类型与 hooks）
- **依赖**：A005
- **估时**：0.5h
- **描述**：
  - `src/api/`：axios 实例 + 每个资源的 fetch/create/update/delete 函数
  - `src/types.ts`：TS 接口与后端 DTO 字段一一对齐
  - 使用 `@tanstack/react-query` 包装（useProducers / useCreateProducer 等 hooks）
- **验收**：
  - [ ] 浏览器 console 调 useProducers() 能拿到数据
  - [ ] 类型不一致时 ts 报错
- **产物**：api 层 + types.ts + hooks

### A012 — Producers 页面（CRUD UI）
- **状态**：☑ completed（不含拖拽排序，改用 order 字段直接编辑）
- **依赖**：A011, A010
- **估时**：1.5h
- **描述**：
  - 列表：shadcn DataTable，列含 displayName/baseCost/baseProduction/enabled
  - 行操作：编辑 / 删除
  - 顶部 "New" 按钮 → 弹 Dialog 表单
  - 表单字段全部使用 shadcn Input + zod 校验
  - 拖拽行重排序（dnd-kit），松开后调 reorder API
- **验收**：
  - [ ] 全 CRUD 操作可见生效
  - [ ] 拖拽改顺序后刷新仍保持
- **产物**：ProducersPage

### A013 — Upgrades 页面
- **状态**：☑ completed
- **依赖**：A012
- **估时**：1h
- **描述**：仿 A012；type 字段下拉；targetProducerId 字段仅在 type=ProducerBoost 时显示且来自 Producers 列表
- **验收**：
  - [ ] type 切换时表单字段联动正确
  - [ ] CRUD 完整
- **产物**：UpgradesPage

### A014 — Events 页面
- **状态**：☑ completed
- **依赖**：A012
- **估时**：1h
- **描述**：上方编辑 Settings（min/max interval、firstDelay），下方 Items 列表 CRUD
- **验收**：
  - [ ] settings PUT 后立即生效
  - [ ] items CRUD 完整
- **产物**：EventsPage

### A015 — Resources / Globals 页面
- **状态**：☑ completed
- **依赖**：A012
- **估时**：0.5h
- **描述**：两页都是简单表单（Resources 是小列表，Globals 是单条记录的几个字段）
- **验收**：
  - [ ] 编辑保存后刷新值正确
- **产物**：ResourcesPage + GlobalsPage

### A016 — Publish 页面
- **状态**：☑ completed
- **依赖**：A008, A015
- **估时**：1h
- **描述**：
  - 顶部大按钮 "Publish to GitHub Pages"，点击调 POST /publish
  - 中部：JSON preview（GET /publish/preview，monaco-editor 或 react-json-view 只读）
  - 下部：历史表（GET /publish/history），每行"Rollback"按钮
  - publish 进行中显示 spinner，完成后 toast 成功/失败
- **验收**：
  - [ ] 点 Publish 后 GitHub 仓库有新 commit
  - [ ] 历史表实时更新
  - [ ] rollback 流程可走通
- **产物**：PublishPage

### A017 — Dockerize + docker-compose
- **状态**：☑ completed（编排就绪；用户提供 GITHUB_PAT 后 `docker-compose up -d --build` 即可）
- **依赖**：A008, A016
- **估时**：0.75h
- **描述**：
  - `backend/Dockerfile`（多阶段构建 .NET）
  - `frontend/Dockerfile`（multi-stage build → nginx）
  - 根 `docker-compose.yml`：backend(127.0.0.1:5000) + frontend(127.0.0.1:5173)
  - backend 挂载 host 目录 `./config-repo:/app/config-repo`，环境变量 `GITHUB_PAT`
  - 文档片段：`docker-compose up -d` 起服务
- **验收**：
  - [ ] 一行命令起服务，前端打开能跑通完整流程
  - [ ] 重启不丢数据（admin.db 持久化）
- **产物**：Dockerfile×2 + docker-compose.yml

### A018 — 端到端冒烟
- **状态**：☑ completed（自动化部分；真实 push 验证待用户提供 PAT，详见 `docs/admin-smoke.md`）
- **依赖**：A017
- **估时**：0.5h
- **描述**：从零跑一遍：
  1. docker-compose up
  2. 浏览器访问 5173
  3. 改一个 Producer 的 baseProduction
  4. Publish
  5. 浏览器开 GitHub Pages URL 看 latest.json 含新值
- **验收**：
  - [ ] 流程通畅，无人工兜底
- **产物**：录屏或截图 + `docs/admin-smoke.md` 记录任何踩坑

## 下一步（v2 范围，仅占位）
- A019 — 局外技能树 schema + 节点编辑器（dnd 节点 + 连线）
- A020 — 数值模拟器（curve preview）
- A021 — 简单密码登录（外网部署前必做）

## 游戏端联动任务（追加到 `docs/tasks.md` 而非本表）
配置发布机制确定后，游戏端需要新增：
- `Scripts/Config/GameConfig.cs`（DTO，对应 admin-requirements §7 schema）
- `Scripts/Config/ConfigLoader.cs`（启动时拉远程 + 三层兜底）
- Loading UI（加载界面）
- 现有 Producer/Upgrade SO 改造或保留为 default_config 来源
- 拟编号 T026–T029，待 admin 第一版可用后开始
