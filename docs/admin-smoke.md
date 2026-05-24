# A018 — 后台第一版端到端冒烟说明

> 本文档由 A001–A017 自动化执行后生成。包含交付状态、运行方式、与已知人工介入点。

## 已实现状态

| 模块 | 状态 |
|---|---|
| 后端 .NET 9 API + EF + SQLite | ✅ 代码完整，本地 dotnet run 已跑通；DB 自动 seed 6 producers / 8 upgrades / 3 events |
| 5 个资源 CRUD API | ✅ 全部 Swagger 可调用 |
| JsonExporter / GitPublisher / Publish API | ✅ 代码编译通过；**真实 git push 需 GITHUB_PAT** |
| 前端 React + TS + Vite | ✅ 6 个页面全部实现，CRUD + 发布 + 回滚 |
| Dockerfile × 2 + docker-compose | ✅ 编排就绪 |
| 端到端冒烟 | ⚠ 需用户提供 GITHUB_PAT 并启动 docker-compose 验证 |

## 运行方式

### 方式 A：本地直跑（开发期，最快验证）

```bash
# 终端 1：后端
cd E:/AIProject/gameTest-admin/backend/AlchemistAdmin
dotnet run    # → http://127.0.0.1:5000/swagger

# 终端 2：前端
cd E:/AIProject/gameTest-admin/frontend
npm run dev   # → http://127.0.0.1:5173
```

不需要 GITHUB_PAT 即可使用所有 CRUD 功能；只是「发布」按钮会报错（缺凭证）。

### 方式 B：Docker 一键起（生产级）

1. 在 `E:/AIProject/gameTest-admin/.env` 填入：
   ```
   GITHUB_PAT=ghp_你的真实token
   ```
   （`.env.example` 已是模板）

2. 启动：
   ```bash
   cd E:/AIProject/gameTest-admin
   docker-compose up -d --build
   ```

3. 浏览器打开 http://127.0.0.1:5173

## 你必须做的事（人工介入点）

### 1. 推两个仓库到 GitHub（A001 后未做）
```bash
cd E:/AIProject/gameTest-admin && git push -u origin main
cd E:/AIProject/gameTest-config && git push -u origin main
```

### 2. 在 gameTest-config 仓库启用 GitHub Pages
- Settings → Pages → Source: `Deploy from a branch`，Branch: `main`，folder: `/ (root)`
- 等 30 秒，访问 https://nanshi-p.github.io/gameTest-config/latest.json 应有 placeholder JSON

### 3. 生成 GitHub PAT
- https://github.com/settings/tokens → Generate new token (classic)
- scope 勾 `repo`（写权限）
- 复制 token，写入 `gameTest-admin/.env` 中的 `GITHUB_PAT=`

### 4. 端到端冒烟
1. 启动方式 A 或 B
2. 浏览器打开 5173 / Producers
3. 编辑「魔法学徒」的 baseProduction 改为 0.5，保存
4. 切到「发布」页 → 点 "Publish to GitHub Pages"
5. 顺利的话提示「发布成功 v1」
6. 30 秒后访问 https://nanshi-p.github.io/gameTest-config/latest.json，能看到 baseProduction = 0.5

## 已知简化

- 未引入 EF Migrations，用 `EnsureCreated()` 自动建表（若以后改 schema 需要手动删 admin.db 重启）
- 未引入 shadcn/ui，使用 vanilla CSS（满足功能，样式朴素）
- Producers 排序通过编辑 `order` 字段，没做拖拽排序（拖拽工程量大于功能价值）
- 未配 EF Migrations 故 schema 变更需 `rm admin.db && 重启`
- 无登录/鉴权（按需求约定 v1 不做）

## 后续

按 `docs/admin-tasks.md` 的 v2 占位项：
- A019 局外技能树
- A020 数值模拟器
- A021 简单密码登录（外网部署前必做）

游戏端联动（拉远程 JSON）待 admin 真实发布跑通后再做（T026–T029）。
