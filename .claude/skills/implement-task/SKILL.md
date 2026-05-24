---
name: implement-task
description: 从 docs/tasks.md 中取一个任务，在 Unity 项目中实现对应的 C# 代码、ScriptableObject、Prefab 或 UI，完成后更新任务状态并报告。用于按计划推进开发。可传入任务 ID（如 T003），不传则自动取下一个待办。
---

# Implement Task — Unity 增量游戏任务实现

## 前置条件
- 存在 `docs/tasks.md`（否则提示运行 `/plan-tasks`）
- 存在 `docs/requirements.md`（作为数值/行为的权威来源）

## 工作流

### 步骤 1：选定任务
- 若用户传入任务 ID（如 `T003`），定位该任务
- 否则按 ID 顺序取第一个 `pending` 且依赖已 `completed` 的任务
- 在 `docs/tasks.md` 中将该任务状态改为 `in_progress`

### 步骤 2：理解任务上下文
- 读取该任务的描述、依赖任务的产物、`docs/requirements.md` 的相关章节
- 若任务描述与需求文档冲突，**停下来问用户**，不要自己猜

### 步骤 3：实现
按 Unity 增量游戏约定：

- **目录约定**
  - C# 脚本：`Assets/Scripts/{Core,Data,UI,Save,Util}/`
  - ScriptableObject 资产：`Assets/Data/`
  - Prefab：`Assets/Prefabs/`
  - 场景：`Assets/Scenes/`

- **代码风格**
  - 命名空间：`Game.{Core,Data,UI,Save}`
  - 公共字段加 `[SerializeField] private`，避免裸 `public`
  - 大数一律 `BreakInfinity.BigDouble`，不要混用 double
  - tick 用固定时间步（默认 0.1s），不在 Update 里跑业务

- **UI**
  - 优先 UGUI（成熟稳定），除非需求指定 UI Toolkit
  - 数字显示走统一格式化工具 `NumberFormat.Format(BigDouble)`
  - 高频刷新用脏标记 + Coroutine，不要每帧 SetText

- **存档**
  - JSON via Newtonsoft；BigDouble 用自定义 Converter
  - 存档结构含 `version` 字段

### 步骤 4：自检
对照任务的"验收"逐条核对：
- [ ] 是否每条都能通过？不能则继续修
- [ ] 是否影响了已完成任务的行为？若是，记录到任务备注

### 步骤 5：更新任务状态
- 在 `docs/tasks.md` 中：
  - 当前任务 `in_progress` → `completed`
  - 更新顶部"进度"统计
  - 若实现过程发现遗漏任务，追加到清单末尾（编号顺延）

### 步骤 6：报告
向用户给出简短报告（不超过 8 行）：
- 完成的任务 ID 和标题
- 新增/修改的关键文件路径
- 验收条目的通过情况
- 下一个建议任务的 ID 和标题
- 若有阻塞或需用户在 Unity Editor 中手动配置的步骤，**明确列出**（如"请在 Unity 中将 X Prefab 拖入 GameManager 的 Y 字段"）

## 重要原则
- **一次只做一个任务**，不要顺手把后面的也做了，会破坏可追溯性
- **Unity 编辑器侧的操作**（如建场景、拖引用、设 Layer）我无法直接执行 — 把这些清晰列给用户去点
- **数值来源**永远是 `requirements.md`；任务清单是路径，需求文档是真相
- 完成后**必须更新** `docs/tasks.md`，否则下次取任务会重复
- 若任务粒度其实太大（超过 2 小时），先拆成子任务（T003a/T003b），再实现第一个
