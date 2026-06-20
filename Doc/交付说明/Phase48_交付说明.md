# Phase48 交付说明 - 天雷阁最小门派接入

## 结果摘要

本阶段把设计文档中的“天雷阁”先接入为可运行的最小门派数据包，并在原型 Run 界面补上“天雷阁开局”按钮。当前版本不提前实现概率暴击、雷击连锁、蓄力等尚未进入战斗引擎的机制，而是用现有稳定机制表达天雷阁的第一层特征：

- `雷击符`：基础雷系伤害牌。
- `天雷咒`：确定性伤害 + 眩晕，先验证控制玩法链路。
- `雷遁术`：抽牌循环牌，支撑天雷阁高频找牌的雏形。

因此 Phase48 的目标是“门派入口、初始牌组、奖励池、眩晕效果和 UI 切换可用”，不是完整天雷阁概率体系。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationCardEnums.cs`
  - `CultivationSect` 新增 `Thunder`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 新增天雷阁三张最小卡牌：`ThunderTalisman`、`HeavenlyThunderSpell`、`ThunderEscape`。
  - 新增 `CreateThunderSectStarterDeck()`，提供 12 张天雷阁初始牌组。
  - 新增 `CreateThunderSectRewardPool()`，提供天雷阁奖励池。
  - `CreateStarterDeck(CultivationSect sect)` 与 `CreateRewardPool(CultivationSect sect)` 接入 `CultivationSect.Thunder`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - `FormatSectName(...)` 增加“天雷阁”显示。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypeUI.cs`
  - 底部 `ActionBar` 新增 `ThunderSectButton`。
  - 点击按钮会调用 `ResetRun(CultivationSect.Thunder)` 并重启为天雷阁开局。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 新增 `ThunderSectStarterDeckUsesStunAndCyclingCards`，验证天雷阁初始牌组数量、关键卡数量、眩晕与抽牌效果。
  - 新增 `ThunderSectCardsCanStunInBattle`，验证 `天雷咒` 在战斗中施加眩晕并让敌人跳过行动。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 新增 `StartRunCanUseThunderSectStarterDeck`。
  - 新增 `ThunderSectBranchingRouteUsesThunderRewardPool`。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 扩展门派文本与快照测试，覆盖“天雷阁”。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypeUITests.cs`
  - 布局断言增加 `ThunderSectButton`。
  - 门派按钮切换测试增加天雷阁按钮、标题与牌组文本验证。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `USG0001`、`CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突。
- `git diff --check`
  - 结果：通过，退出码 0。
  - 备注：仅有 Git 提示 LF 将在下次触碰时转换为 CRLF。
- Unity MCP
  - 结果：未通过正式验收。
  - `mcpforunity://editor/state` 返回 `ready_for_tools=false`，阻塞原因为 `stale_status`。
  - `read_console` 超时。
  - `manage_scene(action="get_active")` 超时。
  - 定向 `run_tests(EditMode)` 超时。
  - 结论：本阶段新增代码和测试已通过 C# 编译，但 Unity Test Framework 实跑与控制台无 Error 验收仍等待 Unity MCP 恢复。

## 假设与风险

- 本阶段只做天雷阁“最小接入”。设计文档中的概率暴击、雷击连锁、蓄力、概率强化仍未实现。
- 现阶段把“天雷咒”的眩晕做成确定性效果，是为了先验证控制链路；后续接入概率系统后需要重新平衡数值。
- 原型 UI 的三个门派按钮是开发期快速切换入口，不是正式主菜单或门派选择流程。
- Unity MCP 不可用导致本阶段缺少 Unity Editor 内定向 EditMode 测试和控制台验收。

## 可选下一步

- Phase49：恢复 Unity MCP 后，补跑 Phase48 定向 EditMode 测试与完整 `GameLogic.EditModeTests`。
- Phase50：为天雷阁补第一版概率/暴击基础模型，再把 `雷击符` 和 `天雷咒` 从确定性表达改为概率表达。
- Phase51：接入第四个门派或开始把门派选择从原型按钮迁移到正式开局配置模型。
