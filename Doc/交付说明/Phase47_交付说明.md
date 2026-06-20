# Phase47 交付说明 - 原型门派选择入口

## 结果摘要

本阶段在 Phase46 的门派启动数据包基础上，补上原型界面可操作的门派切换入口。现在 `CultivationRunPrototypeUI` 的底部操作栏提供“剑宗开局”和“火云宗开局”两个按钮，点击后会重启本轮 Run 并切换对应门派的起始牌组与路线奖励池。

本阶段还让 Presenter 和 Snapshot 暴露当前门派，界面阶段标题与运行总览会显示门派名称，后续可以继续接存档、门派解锁、正式门派选择界面或主菜单入口。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - 新增 `FormatSectName(CultivationSect sect)`。
  - `BuildRunText(...)` 增加当前门派显示。
  - `BuildPhaseTitle(...)` 增加当前门派显示。
  - `RunPrototypeSnapshot` 增加 `Sect` 字段。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypeUI.cs`
  - 底部 `ActionBar` 新增 `SwordSectButton` 和 `FireCloudSectButton`。
  - 两个按钮分别调用 `ResetRun(CultivationSect.Sword)` 和 `ResetRun(CultivationSect.FireCloud)`。
  - 原有 `ResetButton` 保持剑宗默认行为。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 新增 `BuildTextAndSnapshotIncludeSelectedSect`，验证门派文本、阶段标题与快照字段。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypeUITests.cs`
  - 布局断言增加两个门派按钮。
  - 新增 `SectButtonsRestartRunWithSelectedSect`，验证点击火云宗/剑宗按钮后切换门派与牌组文本。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `USG0001`、`CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突。
- `git diff --check`
  - 结果：通过，退出码 0。
  - 备注：仅有 Git 提示 LF 将在下次触碰时转换为 CRLF。
- Unity MCP
  - 结果：未通过正式验收。
  - 现象：`mcpforunity://editor/state` 返回 `ready_for_tools=false`、`stale_status`；`read_console` 超时；定向 `run_tests` 超时。
  - 结论：本阶段新增测试已通过 C# 编译，但尚未通过 Unity Test Framework 实跑确认。

## 假设与风险

- 当前门派选择入口是原型界面的快速切换按钮，不是正式主菜单、门派创建界面或解锁流程。
- 火云宗仍复用原型路线敌人与节点结构，只切换初始牌组和奖励池。
- Unity MCP 桥接仍不可用，阶段验收缺少 Unity 控制台与 EditMode 实跑证据。

## 可选下一步

- Phase48：恢复 Unity MCP 后，补跑 Phase44 到 Phase47 的定向 EditMode 测试和完整 `GameLogic.EditModeTests`。
- Phase49：将门派选择抽象成正式开局配置模型，为存档、主菜单和门派解锁做准备。
- Phase50：继续扩展火云宗中期卡牌和专属机制。
