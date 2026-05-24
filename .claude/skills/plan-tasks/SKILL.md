---
name: plan-tasks
description: 读取 docs/requirements.md，输出一份有顺序、可执行的 Unity 开发任务清单到 docs/tasks.md。每个任务粒度控制在 0.5-2 小时可完成，附带验收标准。用于需求确定后开始排期、或需求重大变更后重排任务。
---

# Plan Tasks — Unity 增量游戏任务规划

## 前置条件
存在 `docs/requirements.md`。若不存在，提示用户先运行 `/refine-requirements`。

## 工作流

### 步骤 1：读取并理解需求
完整读取 `docs/requirements.md`。若有 `docs/tasks.md` 已存在，询问用户是"增量更新"还是"重新生成"。

### 步骤 2：按推荐顺序拆解任务

Unity 增量游戏典型分层（按依赖顺序）：

1. **项目骨架**：Unity 项目创建、目录结构（Scripts/Data/UI/Prefabs/Saves）、必备包（Newtonsoft.Json、BreakInfinity）
2. **核心数据层**：BigDouble 封装、资源类、生产者数据（ScriptableObject）
3. **游戏循环**：GameManager 单例、Tick 系统、时间管理
4. **存档系统**：Save/Load、JSON 序列化、版本号、离线时间记录
5. **离线收益**：闭式公式计算、欢迎回来弹窗
6. **UI 基础**：主面板布局、数字格式化（K/M/B/aa/ab…）、脏标记刷新
7. **购买/升级交互**：按钮、成本/余额联动、置灰逻辑
8. **转生系统**（若需求中有）
9. **成就/解锁**
10. **存档导入导出**
11. **打包与平台适配**

### 步骤 3：写入 `docs/tasks.md`

模板：

```markdown
# 任务清单 — {{游戏名}}

> 最后更新：{{YYYY-MM-DD}}
> 来源：docs/requirements.md

## 进度
- 待办：N
- 进行中：N
- 已完成：N

## 任务列表

### T001 — 项目骨架搭建
- **状态**：☐ pending
- **依赖**：无
- **估时**：1h
- **描述**：创建 Unity 项目（版本：……），建立目录结构 Assets/{Scripts,Data,UI,Prefabs,Saves}，导入 Newtonsoft.Json (com.unity.nuget.newtonsoft-json) 与 BreakInfinity.cs。
- **验收**：
  - [ ] 目录结构创建完毕
  - [ ] Newtonsoft 在 C# 中 `using Newtonsoft.Json;` 编译通过
  - [ ] BreakInfinity 的 BigDouble 能正常 new 与运算
- **产物**：Unity 项目骨架

### T002 — 资源与生产者数据结构
- **状态**：☐ pending
- **依赖**：T001
- **估时**：1.5h
- **描述**：……
- **验收**：……
- **产物**：……

…（依此类推）

## 下一步
运行 `/implement-task T001` 开始实现具体任务，或直接 `/implement-task` 自动取下一个待办任务。
```

### 步骤 4：交付
- 写入 `docs/tasks.md`
- 简短列出前 3 个任务标题，提示用户运行 `/implement-task`

## 重要原则
- **粒度**：单个任务 0.5-2 小时可完成，超过就拆
- **依赖明确**：每个任务列出 `依赖`，避免乱序
- **验收可检验**：每条验收必须能在 Unity 中点出来或 Console 看到，不要写"代码看起来正确"这种废话
- **不要在本 skill 里写实现代码**，只规划任务
- **数值与具体值**来源必须是 requirements.md，不要凭空发明；缺失则反过来提示用户回 `/refine-requirements` 补充
