# Phase75 交付说明：低风险秘境事件池收口

## 结果摘要

本阶段只收口当前已经开始的低风险秘境事件池逻辑，没有继续扩大玩法范围。

这批改动让原型路线中的低风险秘境节点可以从事件池中抽取事件，并补入三个新的低风险秘境事件：仙府遗迹、游商议价、修炼石台。事件效果覆盖随机升级卡牌、从当前节点奖励池获得随机卡、下一次坊市一次性折扣等最小可测逻辑。

需要明确：这不是完整可玩的游戏交付，也没有证明游戏手感、UI 体验、地图表现或美术质量已经成立。本阶段只是把已经写下的逻辑和测试闭环收住，避免留下半截工作。

## 实际改动清单

- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunNode.cs`
  - 为节点新增 `MysticEventPool`，允许一个秘境节点挂接多个候选事件。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunState.cs`
  - 新增 `CurrentMysticEvent`，用于记录本次进入秘境时实际抽到的事件。
  - 新增 `NextMarketDiscountPercent`、设置与消费接口，用于下一次坊市一次性折扣。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunReward.cs`
  - 新增秘境效果类型：`UpgradeRandomCard`、`GainRandomRewardCard`、`NextMarketDiscount`。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunEngine.cs`
  - 进入秘境时从事件池按奖励随机源抽取当前事件。
  - 离开节点时清理当前秘境事件状态。
  - 进入坊市时应用并消费下一次坊市折扣。
  - 补入随机升级牌、随机获得奖励牌和一次性折扣的效果处理。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 新增 `ImmortalAbodeMysticEvent`、`WanderingMerchantMysticEvent`、`TrainingStonePlatformMysticEvent`。
  - 新增 `CreateLowRiskMysticEventPool()`，事件池包含 4 个事件：仙府遗迹、灵泉、游商、修炼石台。
  - 将原型路线中的低风险秘境节点改为使用事件池。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - 秘境展示优先显示本次抽到的 `CurrentMysticEvent`，兼容旧的单事件节点。
- `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 覆盖事件池内容、固定种子抽取、仙府探索失败、读壁经升级、游商折扣一次性消费、石台随机奖励牌。
- `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 覆盖事件池抽到当前秘境后，原型文本能展示实际事件与选项，并验证选择后的状态变化。

## 验证与结果

- Unity MCP 目标 EditMode 测试已通过：
  - `GameLogic.Tests.CultivationRunEngineTests`
  - `GameLogic.Tests.CultivationRunPrototypePresenterTests`
  - 结果：144/144 passed。
- Unity MCP 单项回归测试已通过：
  - `GameLogic.Tests.CultivationRunPrototypeUITests.PrototypeUiCanResolveMysticEventAndGainPill`
  - 结果：1/1 passed。
- Unity MCP 全量 EditMode 测试已通过：
  - 结果：265/265 passed。
- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过。
  - 说明：仍有项目既有 warning，包括 `System.Net.Http` / `System.IO.Compression` 版本冲突与 `USG0001`，本阶段没有新增编译错误。
- `git diff --check`
  - 结果：通过。
  - 说明：仅有 Git 的 CRLF 转换提示，无空白错误。
- Unity MCP 控制台 Error 查询
  - 初次查询存在 1 条 MCP 插件连接流已释放错误，来源为 MCP refresh 断线重连，不是游戏代码错误。
  - 清空控制台后复查：0 条 Error。

## 假设与风险

- 当前随机升级卡牌默认使用第一条升级分支，没有做 UI 选择弹窗，也没有做多分支选择策略。
- 当前随机奖励卡只从当前节点 `RewardPool` 抽取，没有接入正式配置表、稀有度权重或事件专属奖励池。
- 当前事件描述仍主要是英文原型文本，没有做正式中文文案、排版和本地化。
- 本阶段没有改 UI Prefab、场景、美术、地图视觉和玩家输入链路，因此不能用它证明游戏已经可玩或好玩。
- 自动化测试只能证明逻辑分支和状态转换成立，不能替代真实试玩对节奏、反馈、点击路径和视觉完成度的判断。

## 可选下一步

- 停止继续堆逻辑，先回到产品合同和垂直切片边界，重新确认“第一屏实际可玩体验”要长什么样。
- 若继续做原型，应优先做一个固定种子、固定路线、固定卡组的可复现试玩局，并导出截图、状态、操作序列和战斗日志。
