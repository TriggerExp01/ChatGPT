# Phase31 交付说明 - 秘境事件基础入口

## 结果摘要

本阶段完成“秘境事件基础入口”的最小可验收实现。原型路线在休息节点之后新增“灵泉秘境”分支，玩家进入秘境后可以从固定事件选项中选择恢复、获得丹药或离开，并在结算后推进到后续首领战。

## 实际改动清单

- 新增 `Mystic` 跑图节点类型与 `Mystic` 运行状态。
- 新增秘境事件数据结构：`MysticEventDefinition`、`MysticEventOption`、`MysticEventEffectType`。
- `CultivationRunNode` 支持挂载固定秘境事件。
- `CultivationRunState` 记录当前秘境选项与已结算秘境选项。
- `CultivationRunEngine` 新增秘境进入、选项结算和推进逻辑。
- 种子数据新增“灵泉秘境”事件：
  - 饮用灵泉：恢复 30 HP。
  - 采集小还丹：获得 1 个小还丹。
  - 离开灵泉：不获得额外奖励。
- 原型分支路线在休息后新增“灵泉秘境”，并保持宝箱、坊市、精英首领分支可达。
- 原型 Presenter 与 UI 增加秘境状态展示、秘境按钮和快照计数。
- EditMode 测试新增秘境恢复、秘境获丹、非法状态拒绝、Presenter 快照、UI 结算路径覆盖。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍有既有 `System.Net.Http`、`System.IO.Compression` 与 `USG0001` warning。
- `git diff --check`
  - 结果：通过。
- Unity MCP `refresh_unity`
  - 结果：通过。过程中出现一次可恢复 MCP 断连，最终 Editor ready。
- Unity MCP `GameLogic.EditModeTests`
  - 结果：109/109 Passed。
- Unity MCP `execute_code` 秘境路线探针
  - 结果：`StatusAtMystic=Mystic; ChoiceCount=3; Option=collect_small_restore_pill; Pills=1; Resolved=1; Status=InBattle; CurrentNodeId=node_stone_demon_leader;`
- Unity MCP `read_console`
  - 结果：清理可恢复 MCP transport 旧错误后，0 Error / 0 Warning。

## 假设与风险

- 本阶段只做基础入口和固定事件，不做正式事件池、权重、随机、战斗型事件、美术资源或完整 UI 重构。
- 秘境事件当前直接写在种子数据中，后续如果事件数量增加，应迁移到配置表或独立内容数据层。
- 当前 UI 仍是原型 UI，只提供功能入口；完整 UI 作为下一阶段单独推进。

## 可选下一步

- Phase32：接入完整可玩的运行 UI，将战斗、路线、休息、宝箱、坊市、秘境、奖励统一到一个可操作界面。
