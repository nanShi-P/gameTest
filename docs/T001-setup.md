# Unity 项目骨架说明

本目录由 `/implement-task T001` 预置。Unity 项目本身需要你在 **Unity Hub** 里手动打开一次以生成 `ProjectSettings/`、`Library/`、`.meta` 文件等。

## 一次性手动步骤

1. **打开 Unity Hub** → Projects → Add → 选择 `E:\AIProject\gameTest`
2. **Unity 版本**：选择 2022.3 LTS 或更新（首次打开会询问，无 LTS 则装一个）
3. **模板**：Unity Hub 会识别到现有项目，不会再问模板。若提示空项目，选 **2D Core**
4. Unity 会自动：
   - 创建 `ProjectSettings/`、`Library/`、`UserSettings/`
   - 根据 `Packages/manifest.json` 解析依赖（Newtonsoft.Json 等）
   - 为 `Assets/` 下所有文件生成 `.meta`
5. 等待编译完成（首次较慢）。Console 不应有红色错误。

## 验收（T001）

完成上面手动步骤后，请按以下验收：

- [ ] **目录可见**：Project 窗口能看到 `Scripts/{Core,Data,UI,Save,Util,Events}`、`Data/{Producers,Upgrades}`、`Prefabs`、`UI`、`Scenes`、`ThirdParty/BreakInfinity`
- [ ] **编译通过**：`Assets/Scripts/Core/SmokeTest.cs` 已包含 `using Newtonsoft.Json;` 和 `using BreakInfinity;`，无编译错误
- [ ] **运行验证**：
  1. 新建场景 `Assets/Scenes/Main.unity`（File → New Scene → Basic 2D → 保存）
  2. 创建空 GameObject 命名 `__SmokeTest`，将 `SmokeTest` 脚本拖到它上面
  3. 点 Play
  4. Console 应打印：
     - `[SmokeTest] BigDouble 1e100 * 10 = 1e101`
     - `[SmokeTest] Newtonsoft OK: {"value":"1e101"}`

确认通过后，可以删掉 `__SmokeTest` 这个 GameObject（脚本文件保留也可，后续任务用不到的话再删）。

## 已为你完成

- 全部目录结构
- `Packages/manifest.json`（声明 Newtonsoft.Json 3.2.1）
- `Assets/ThirdParty/BreakInfinity/BigDouble.cs`（直接源文件方式，无需 NuGet）
- `Assets/Scripts/Core/SmokeTest.cs`（验证脚本）

## 下一步

T001 验收通过后，运行 `/implement-task` 进入 T002（NumberFormat 工具）。
