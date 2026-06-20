# Phase33 交付说明 - 境界突破基础

## 结果摘要

本阶段完成“炼气到筑基”的最小可验收突破链路。默认分支原型路线在击败“石魔首领”并处理奖励后，不再直接结束运行，而是突破到筑基境界，进入新的“筑基剑修”战斗节点。

突破会带来一组基础成长：玩家生命上限提升并回满，灵力上限提升，手牌上限提升，并记录已突破次数。完整五层境界地图、突破动画、金丹被动、道基树和正式数值平衡不在本阶段范围内。

## 实际改动清单

- 新增 `CultivationRealm` 境界枚举，覆盖炼气、筑基、金丹、元婴、化神、渡劫。
- `CultivationRunNode` 支持声明节点所属境界层。
- `CultivationRunState` 新增当前境界、突破次数、灵力上限、手牌上限，并提供 `TryBreakthroughTo` 推进逻辑。
- `BattleEngine` 支持按运行状态创建战斗，传入灵力上限和手牌上限。
- 原型分支路线在“石魔首领”后新增“筑基剑修”战斗节点。
- `CultivationRunEngine` 在进入新节点时自动检查境界突破，并把成长后的战斗参数传入当前战斗。
- `CultivationRunPrototypePresenter` 和 `CultivationRunPrototypeUI` 增加境界、突破次数、灵力上限、手牌上限展示。
- EditMode 测试新增引擎路径和 UI 路径覆盖，验证首领胜利后进入筑基战斗。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `System.Net.Http` / `System.IO.Compression` 引用冲突、`USG0001`、`CS8632` 等。
- `git diff --check`
  - 结果：通过。
- Unity MCP `refresh_unity`
  - 结果：通过。过程中出现一次可恢复 domain reload 断连，最终 Editor ready。
- Unity MCP `GameLogic.EditModeTests`
  - 结果：112/112 Passed。
- Unity MCP `execute_code` 境界突破探针
  - 结果：`Status=InBattle; Realm=Foundation; Breakthroughs=1; Node=node_foundation_sword_cultivator; HP=110/110; SpiritMax=4; HandLimit=6; BattleSpirit=4; BattleHandLimit=6; Hand=6;`
- Unity MCP `read_console`
  - 结果：0 Error / 0 Warning。

## 假设与风险

- 本阶段只实现境界突破的基础骨架，数值采用最小成长规则：每突破一层，生命上限 +10 并回满，灵力上限 +1，手牌上限 +1。
- 当前只有默认分支原型路线接入筑基第一战，旧线性原型路线保持原有结束行为，用于兼容既有测试。
- “筑基剑修”仍为代码内种子数据，后续内容扩展时应迁移到配置表或独立内容数据层。
- UI 当前只做信息展示接入，不做突破动画、境界图标、正式 Prefab 化和美术资源替换。

## 可选下一步

- Phase34：扩展筑基层可玩路线，加入 2 到 3 个筑基普通/精英/事件节点，让第二层不只是单场验证战。
- Phase35：将境界突破表现做成正式 UI 样板，包括突破面板、境界徽标、成长条目和节点层级视觉区分。
