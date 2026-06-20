# Phase64 交付说明 - 药王谷中毒循环最小可玩切片

## 结果摘要

本阶段补齐六门派目标中的第五个门派：药王谷。当前按“可选门派开局、可运行中毒/毒爆/吸灵/持续恢复核心循环、可进入奖励池、可被固定种子自动走局跑通”的最小范围落地。

药王谷当前战斗定位为中毒堆积、持续回复和持久消耗战。实现重点是先让药王谷拥有区别于剑宗、火云宗、天雷阁和玄黄宗的状态结算闭环，而不是一次性补完整设计文档中的全部 27 张卡、法宝、免疫、毒王领域或完整净化体系。

## 实际改动清单

- 战斗状态与结算
  - `CardEffectType` 新增 `Poison`、`PoisonBurst`、`Leech`、`Regeneration`、`PoisonAttackCounter`。
  - `CombatantState` 新增中毒层数、直接伤害、中毒回合开始结算和负面状态清理能力。
  - `BattleState` 新增毒瘴反伤中毒、生生不息持续回复和本回合中毒伤害触发记录。
  - `BattleEngine` 接入中毒 DOT、毒爆消耗中毒并造成直接伤害、吸灵按实际伤害回复、生生不息回合开始回复、毒瘴护体在敌人攻击命中后反加中毒。
- 药王谷门派入口
  - `CultivationSect` 新增 `Medicine`。
  - 原型 Run UI 新增“药王谷开局”按钮。
  - Presenter、战斗 UI 和快照文本补充“药王谷”“中毒”“生生不息”“毒瘴”等展示。
  - 固定种子自动走局支持指定药王谷门派。
- 药王谷卡牌池
  - 新增起始与奖励卡：毒藤术、腐毒掌、吸灵术、回春术、毒瘴护体、草木遁、瘴气弥漫、毒爆术、寄生种子、生生不息。
  - 新增 `CreateMedicineSectStarterDeck()` 与 `CreateMedicineSectRewardPool()`。
  - `CreateStarterDeck`、`CreateRewardPool` 和分支路线奖励池接入药王谷。
- 自动走局策略
  - `CultivationRunAutoPlayer` 增加对中毒、毒爆、吸灵、持续恢复和毒瘴反击的基础评分，使药王谷固定种子路线能稳定推进。
- 测试
  - EditMode 覆盖中毒 DOT、毒爆直接伤害、持续回复、吸灵、毒瘴反中毒、药王谷起始牌组和药王谷奖励池分发。
  - PlayMode 新增药王谷固定种子自动走局，验证原型 UI 可以从药王谷开局推进到当前路线完成。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 通过，0 Error。
  - 本轮输出 2 个项目既有 `MSB3277` warning，来自 `System.Net.Http` / `System.IO.Compression` 版本冲突。
- `git diff --check`
  - 通过；仅有 Git 工作副本 LF/CRLF 转换提示。
- Unity MCP 验证
  - `refresh_unity(scope="scripts", mode="force", compile="request", wait_for_ready=true)`：通过；刷新期间发生一次域重载断连，MCP 自动恢复并返回 ready。
  - `run_tests(EditMode, assembly_names=["GameLogic.EditModeTests"])`：通过，200/200。
  - `run_tests(PlayMode, test_names=[三条固定种子走局用例])`：通过，3/3。
  - PlayMode 覆盖剑宗默认路线、玄黄宗路线和药王谷路线。
  - `read_console(types=["error","warning"])`：最终 0 条 Error/Warning。

## 验收过程备注

- 首次 PlayMode 全量任务在 Unity Test Framework 进入 PlayMode 过渡时卡住，随后确认 Editor 不在播放模式、控制台无业务错误。
- 清理 Unity Test Framework 残留测试作业后，用正确完整测试名 `GameLogic.PlayModeTests.CultivationRunFixedSeedAutoPlayTests.*` 重新执行，3 条 PlayMode 用例全部真实执行并通过。
- 恢复过程产生的临时 `InitTestScene*.unity` 和无效 0-test 报告已清理，最终工作树只保留本阶段业务、测试和文档改动。

## 假设与风险

- 本阶段没有实现药王谷设计文档中的完整 27 张卡、完整净化体系、免疫、毒王领域、药王谷专属法宝或完整构筑平衡。
- 当前“净化”只复用负面状态清理能力，为后续扩展留下结算入口；尚未作为完整卡牌与 UI 选择体系交付。
- 自动走局证明的是固定种子、固定策略和当前原型路线的可通行性，不代表真实玩家策略、多路线平衡、失败局体验或长期数值曲线已经完成。

## 可选下一步

- Phase65 可继续补齐魔道或另一个未完成门派的最小可玩切片。
- 也可以先回到药王谷，扩展完整 27 张卡、净化/免疫/毒王领域和专属法宝，再做一轮平衡与 UI 文案细化。
