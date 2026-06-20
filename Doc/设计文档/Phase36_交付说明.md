# Phase36 交付说明 - 金丹被动机制与完整 UI 闭环

## 结果摘要

本阶段在 Phase35 的完整跑团 UI 外壳之上，补齐金丹期入口的关键交互闭环：玩家从筑基短路线突破到金丹后，不再直接进入金丹战斗，而是先进入“金丹被动三选一”状态。UI 会展示 3 个金丹被动按钮，选择后立即进入“金丹魔修”战斗，并把被动效果带入战斗状态。

同时补齐了金丹战斗所需的基础状态能力：眩晕、敌人治疗、敌人攻击强化、每回合额外抽牌、功法灵力消耗降低，并让 Presenter 与完整 UI 都能显示这些新增状态。

## 实际改动清单

- `BattleEngine`
  - 支持创建战斗时传入 `GoldenCorePassiveDefinition`。
  - 新增三类金丹被动战斗效果：
    - 剑心成丹：战斗开始获得锋锐。
    - 流水之势：每回合额外抽牌。
    - 雷种入体：功法灵力消耗降低。
  - 新增卡牌眩晕效果与敌人眩晕跳过行动。
  - 新增敌人意图：攻击并眩晕、治疗、攻击强化。
  - 金丹魔修意图改为治疗、强化与连击组合。
- `CultivationRunEngine` / `CultivationRunState`
  - 突破到金丹时进入 `GoldenCorePassiveChoice`。
  - 选择金丹被动后开始当前节点战斗。
  - Run 状态保存当前可选被动与已选被动。
- `CultivationRunPrototypePresenter`
  - Run 总览、资源条、操作列表、战斗详情显示金丹被动、眩晕、额外抽牌和灵力消耗降低。
  - 等待操作文案统一改为中文状态名。
- `CultivationRunPrototypeUI`
  - 新增金丹被动选择按钮区。
  - 点击被动后刷新为金丹战斗 UI。
- EditMode 测试
  - 覆盖金丹被动选择态、选择后进入战斗、三类被动效果、眩晕跳过行动、UI 按钮生成与点击闭环。
  - 修正旧测试中英文 enum 状态文案断言，统一到中文 UI 文案。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍有项目既有 warning，主要是 `System.Net.Http` / `System.IO.Compression` 引用冲突、`USG0001`、`CS8632`。
- `git diff --check`
  - 结果：通过。
- Unity MCP `refresh_unity`
  - 结果：通过。
  - 备注：刷新期间出现可恢复 domain reload 断连，最终 Editor ready。
- Unity MCP 定向 EditMode 测试
  - 10/10 Passed。
- Unity MCP 完整 `GameLogic.EditModeTests`
  - 128/128 Passed。
- Unity MCP `execute_code` 操作探针
  - 结果：通过。
  - 观测值：`Before=GoldenCorePassiveChoice/GoldenCore/3; Buttons=3; After=InBattle/雷种入体; CostReduction=1`。
- Unity MCP `read_console`
  - 结果：0 Error / 0 Warning。

## 假设与风险

- 本阶段仍是代码生成的 uGUI 原型 UI，不是最终商业化 Prefab 或 TEngine UI Window。
- 金丹被动目前是硬编码种子数据，后续仍应迁移到配置表或数据资产。
- 额外抽牌会受当前牌库可抽数量限制；测试验证的是被动参数和抽牌容量，而不是强制生成额外卡牌。
- 金丹期目前只有入口战一场，尚未扩展到完整金丹层地图、敌人池和奖励池。

## 可选下一步

- Phase37：把完整跑团 UI 拆成可复用 Prefab 或 TEngine UI Window。
- Phase38：扩展金丹层地图、敌人池和金丹奖励池。
- Phase39：做 UI 视觉黄金样板和截图验收，替换当前代码生成临时控件。
