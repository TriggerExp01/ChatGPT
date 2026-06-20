# Phase34 交付说明 - 筑基层短路线与冰冻机制

## 结果摘要

本阶段把筑基期从单场验证战扩展为一段可走完的短路线。玩家击败“筑基剑修”后，会进入筑基层路线选择，可选择“冰霜蛇妖”普通战或“筑基闭关”，随后进入“风灵鸟”普通战，最后挑战“双头冰火蟒”精英战并完成当前原型路线。

同时接入筑基期第一个教学机制“冰冻”：敌人可在攻击时施加冰冻，玩家下一回合灵力会被压低。运行 UI 的战斗文本也会显示玩家和敌人的冰冻状态。

## 实际改动清单

- 新增敌人意图 `AttackAndFreeze`。
- `CombatantState` 增加冰冻层数、持续回合、清除负面状态和回合开始结算。
- `BattleEngine` 在玩家回合开始时结算冰冻灵力惩罚，并支持敌人攻击后施加冰冻。
- 新增筑基层敌人：
  - `冰霜蛇妖`：普通战，教学冰冻。
  - `风灵鸟`：普通战，快节奏多段压力。
  - `双头冰火蟒`：精英战，冰冻与灼烧组合压力。
- 默认分支原型路线扩展为筑基层短路线：
  - `筑基剑修` -> `冰霜蛇妖` / `筑基闭关` -> `风灵鸟` -> `双头冰火蟒`。
- `CultivationRunPrototypePresenter` 战斗文本新增冰冻状态展示。
- EditMode 测试新增冰冻结算、筑基层路线选择、筑基层短路线完成和 UI 路径覆盖。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `System.Net.Http` / `System.IO.Compression` 引用冲突、`USG0001`、`CS8632` 等。
- `git diff --check`
  - 结果：通过。
- Unity MCP `refresh_unity`
  - 结果：通过。过程中出现可恢复 domain reload 断连，最终 Editor ready。
- Unity MCP `GameLogic.EditModeTests`
  - 结果：117/117 Passed。
- Unity MCP `execute_code` 筑基层路线探针
  - 结果：`Choices=node_frost_serpent_demon,node_foundation_meditation; Route=node_frost_serpent_demon>node_wind_spirit_bird>node_dual_head_ice_fire_serpent; Final=Completed; Realm=Foundation; Breakthroughs=1; Artifacts=2; SpiritStones=150;`
- Unity MCP `execute_code` 冰冻探针
  - 结果：`Spirit=3/4; Freeze=1/1;`，确认冰冻把本回合灵力从 4 压到 3。
- Unity MCP `read_console`
  - 结果：0 Error / 0 Warning。

## 假设与风险

- 冰冻当前采用最小实现：每层冰冻使玩家下一回合灵力 -1，持续回合按状态结算；暂不做图标、特效、抵抗、免疫或高级叠层规则。
- `风灵鸟` 的闪避/抽牌等设计文档机制还未接入，本阶段用现有可结算的攻击、防御、多段压力表达节奏差异。
- `双头冰火蟒` 当前是筑基层短路线精英，不代表正式筑基期 Boss；正式 Boss、金丹突破和更完整筑基层地图留给后续阶段。
- 路线数据仍在 `CultivationSeedData` 中硬编码，后续内容量继续扩大时应迁移到配置表或独立数据层。

## 可选下一步

- Phase35：接入筑基层正式 Boss 或金丹突破入口，让当前路线从筑基完成推进到金丹骨架。
- Phase36：补齐筑基层更多专属机制，如中毒、闪避、镜像/牌库检查等，并在 UI 中提供更清晰的状态标识。
