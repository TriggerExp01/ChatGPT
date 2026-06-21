# Phase71 交付说明：接入高阶通用法宝规则

## 结果摘要

本阶段继续补齐通用法宝池中可通过纯战斗逻辑验证的高阶法宝，已接入 `青冥剑`、`五行法阵`、`万魂幡`、`天道石` 四个规则。

本阶段不涉及 UI Prefab、场景视觉、地图可视范围和秘境概率事件。

## 实际改动清单

- `青冥剑`
  - 每场战斗首次攻击伤害翻倍。
  - 与既有蓄力、固定加伤、攻击百分比加成共用伤害结算路径。
- `五行法阵`
  - 每个玩家回合开始时，对所有未阵亡敌人造成 2 点直接伤害。
- `万魂幡`
  - 每个敌人首次被击杀时恢复 5 HP。
  - 同一敌人重复检查不会重复回血。
- `天道石`
  - 每回合额外抽 1 张牌。
  - 作为战斗状态增益挂载，实际从后续抽牌流程生效。
- 通用法宝池
  - 四个新法宝加入通用法宝奖励池。
  - `青冥剑` 与 `五行法阵` 加入原型坊市条目。
- 展示数据
  - 补充四个新法宝的效果摘要、图标键和战斗状态摘要。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 通过。
  - 仅保留既有警告：`USG0001`、`UIBase.cs` nullable 注释、`System.Net.Http/System.IO.Compression` 版本冲突。
- `git diff --check`
  - 通过。
- Unity MCP
  - `refresh_unity(mode=if_dirty, scope=scripts, compile=request, wait_for_ready=true)` 通过。
  - 刷新过程中出现一次 Unity MCP 断连恢复，工具返回 editor ready。
  - 目标 EditMode 测试通过：`205/205`。
    - `GameLogic.Tests.CultivationBattleEngineTests`
    - `GameLogic.Tests.CultivationRunEngineTests`
    - `GameLogic.Tests.CultivationRunPrototypePresenterTests`
  - Unity Console Error：`0`。

## 自动化已证明的内容

- 四个新法宝能正确挂载到战斗状态。
- `青冥剑` 只消耗一次首次攻击翻倍。
- `五行法阵` 在回合开始对全体敌人生效。
- `万魂幡` 对同一敌人只结算一次击杀回血。
- `天道石` 能提升后续回合抽牌量。
- 法宝池、坊市数量和展示摘要可被测试覆盖。

## 假设与风险

- 本阶段仍是逻辑与展示数据层验证，没有进行真实玩家试玩。
- 新增日志中部分新法宝触发文案暂用英文稳定字符串，避免触碰既有乱码显示问题；后续可集中做战斗日志中文文案整理。
- `灵兽袋`、`破障珠`、`天机盘`、`不灭金丹`、`造化玉碟` 尚未接入，因为它们分别涉及丹药发放、秘境概率、地图可视范围、致死保护和升级分支 UI，适合拆分到后续独立阶段。

## 可选下一步

- Phase72 可优先接入 `不灭金丹` 的致死保护，需统一玩家受伤入口和死亡结算。
- 或优先接入 `灵兽袋`，需明确随机丹药来源、丹药槽满时的处理规则，以及战斗开始发放时机。
