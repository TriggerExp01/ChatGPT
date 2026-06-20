# Phase46 交付说明 - 门派启动数据包接入

## 结果摘要

本阶段把 Phase45 已落地的火云宗最小卡池接入跑团启动链路。现在跑团可以通过 `CultivationSect` 明确选择剑宗或火云宗；默认仍保持剑宗，避免破坏现有测试与原型 UI 行为。火云宗启动时会使用火云宗初始牌组，并让原型路线中的战斗/精英节点使用火云宗奖励池。

本阶段没有新增正式门派选择界面、解锁系统、存档字段、视觉资源或新战斗机制；目标是先补齐“第二门派可被 Run 入口消费”的纯逻辑层。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationCardEnums.cs`
  - 新增 `CultivationSect` 枚举：`Sword`、`FireCloud`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunState.cs`
  - 新增只读 `Sect` 字段，记录本轮跑团所属门派。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 新增 `CreateStarterDeck(CultivationSect sect)`。
  - 新增 `CreateRewardPool(CultivationSect sect)`。
  - `CreateFirstPrototypeRoute(...)` 与 `CreateFirstPrototypeBranchingRoute(...)` 增加门派参数，并按门派切换奖励池。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunEngine.cs`
  - `StartRun(...)` 增加 `sect` 参数。
  - 未显式传入牌组或路线时，按门派生成默认牌组和默认路线。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypeUI.cs`
  - 新增 `ResetRun(CultivationSect sect)` 重载。
  - 默认 `ResetRun()` 仍使用剑宗；后续门派选择 UI 可直接调用重载入口。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 新增 `StartRunCanUseFireCloudSectStarterDeck`。
  - 新增 `FireCloudSectBranchingRouteUsesFireCloudRewardPool`。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `USG0001`、`CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突。
- `git diff --check`
  - 结果：通过，退出码 0。
  - 备注：仅有 Git 提示 LF 将在下次触碰时转换为 CRLF。
- Unity MCP
  - 结果：未通过正式验收。
  - 现象：`mcpforunity://editor/state` 返回 `ready_for_tools=false`、`stale_status`；`read_console` 超时；`manage_scene` 超时；`run_tests` 返回未发现可用 Unity Editor 实例。
  - 结论：本阶段不能宣称 Unity Test Framework 或 Unity 控制台验收通过。

## 假设与风险

- 火云宗目前仍复用原型路线里的敌人与节点结构，只替换起始牌组和奖励池；这适合逻辑接入，但还不是完整的火云宗专属路线。
- 火云宗奖励池仍是最小池，不代表 `卡牌池设计_火云宗.md` 的完整卡池已经实现。
- 由于 Unity MCP 桥接不可用，新增 EditMode 测试尚未通过 Unity Test Framework 实跑确认；当前只能确认 C# 工程构建通过。

## 可选下一步

- Phase47：恢复 Unity MCP 后，补跑 Phase44 到 Phase46 的定向 EditMode 测试和完整 `GameLogic.EditModeTests`。
- Phase48：实现原型内门派选择入口，让 UI 可在剑宗和火云宗之间切换开局。
- Phase49：扩展火云宗中期卡牌与专属机制，例如烈焰印记、引爆、灼烧转化。
