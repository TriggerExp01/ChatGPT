# Phase73 交付说明：灵兽袋丹药发放接入

## 结果摘要

本阶段按设计文档接入通用法宝 `灵兽袋` 的核心规则：每累计 3 场战斗胜利，获得 1 颗随机丹药。

本阶段只做纯逻辑和原型展示摘要，不修改 UI Prefab、场景、美术资源或地图视觉表现。

## 规则决策

- 触发计数按“战斗胜利次数”计算，不按进入战斗次数计算。
- 每累计 3 场胜利尝试发放 1 颗随机丹药。
- 随机丹药来源先使用现有基础丹药池：
  - 小还丹
  - 大还丹
  - 增元丹
  - 解毒丹
  - 破境丹
  - 筑基丹
- 丹药槽满时不发放丹药，也不重置计数；后续胜利会继续尝试发放，避免玩家永久损失一次触发。
- `灵兽袋` 进入通用法宝奖励池，并进入默认坊市，价格为 50 灵石。

## 实际改动清单

- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunReward.cs`
  - 新增 `ArtifactEffectType.PillEveryThirdVictory`。
  - 新增 `PillEveryThirdVictoryInterval` 派生属性。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunState.cs`
  - 新增 `SpiritBeastBagVictoryCounter`。
  - 新增胜利计数递增与重置方法。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunEngine.cs`
  - 在战斗胜利结算中接入 `灵兽袋` 发药逻辑。
  - 满 3 胜且丹药槽未满时，从基础丹药池随机获得 1 颗丹药并重置计数。
  - 满 3 胜但丹药槽已满时保留计数，并写入战斗日志。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 新增 `CultivationSeedData.SpiritBeastBagArtifact`。
  - 新增 `CreateBasicPillRewardPool()`。
  - 将 `灵兽袋` 加入通用法宝奖励池。
  - 将 `灵兽袋` 加入默认坊市条目，价格 50。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - 新增 `灵兽袋` 效果摘要、图标键和当前胜利计数展示。
- `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 覆盖法宝池包含 `灵兽袋`。
  - 覆盖坊市 50 灵石购买 `灵兽袋`。
  - 覆盖 3 场胜利后获得随机丹药。
  - 覆盖丹药槽满时保留触发进度，腾出槽位后继续发放。
- `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 覆盖 `灵兽袋` 图标键、胜利计数展示和效果摘要。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过。
  - 说明：仍存在项目既有 warning，包括 `USG0001`、`CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突 warning；本阶段未新增错误。
- `git diff --check`
  - 结果：通过。
  - 说明：仅有 Git 的 CRLF 转换提示，无空白错误。
- Unity MCP `refresh_unity(mode=if_dirty, scope=scripts, compile=request, wait_for_ready=true)`
  - 结果：通过。
  - 说明：期间 Unity 域重载导致一次断连，工具自动恢复后 Editor ready。
- Unity MCP 目标 EditMode 测试：
  - `GameLogic.Tests.CultivationRunEngineTests`
  - `GameLogic.Tests.CultivationRunPrototypePresenterTests`
  - 结果：132/132 passed。
- Unity MCP 全量 EditMode 测试：
  - 结果：253/253 passed。
- Unity Console Error 查询：
  - 结果：0 条 Error。

## 假设与风险

- 本阶段没有新增丹药类型，只复用现有基础丹药池。
- `灵兽袋` 的名称和描述在 `BattleEngine.cs` 中使用英文字符串，以避免继续扩大该文件历史中文编码显示问题；原型展示层已提供中文效果摘要。
- 丹药槽满时保留计数属于本阶段明确规则决策；如果后续希望“满槽也消耗触发次数”，需要单独调整。
- 本阶段未做 UI Prefab 或视觉资源接入，正式图标和美术表现仍需在 UI 样板确认后处理。

## 可选下一步

- Phase74 可继续接入 `破障珠`，建议先明确秘境概率提升的当前事件随机模型。
- 或接入 `天机盘`，但它涉及地图可视范围和路线预览，可能需要先确认 UI/地图展示边界。
