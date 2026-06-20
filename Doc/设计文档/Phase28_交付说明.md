# Phase 28 交付说明：回春玉佩法宝恢复
> 日期：2026-06-20  
> 范围：接入第二个法宝 `回春玉佩` 的胜利后恢复效果  
> 状态：已完成并通过 Unity MCP 验收

## 目标

本阶段继续沿用 Phase 27 的法宝最小闭环，接入设计文档中的续航型法宝 `回春玉佩`。

- 设计效果：每场战斗结束后恢复 3 HP。
- 获取方式：当前先接入原型坊市。
- 坊市价格：30 灵石。

本阶段不扩展完整法宝掉落、宝箱、精英战奖励或法宝唯一性规则。

## 实际改动

- 扩展法宝效果类型：
  - 新增 `ArtifactEffectType.HealAfterVictory`。
  - `ArtifactDefinition` 新增 `HealAfterVictoryAmount` 便捷属性。
- 接入 `CultivationSeedData.RejuvenationJadeArtifact`：
  - ID：`rejuvenation_jade`。
  - 名称：回春玉佩。
  - 效果：每场战斗结束后恢复 3 HP。
  - 原型坊市商品：`market_rejuvenation_jade`，价格 30 灵石。
- 扩展胜利结算：
  - 战斗胜利时先同步当前战斗 HP 到 Run 状态。
  - 持有回春玉佩时恢复 HP，恢复量不超过最大 HP。
  - 同步 `CurrentBattle.Player.CurrentHp` 与 `PlayerCurrentHp`，保证奖励界面和 Snapshot 显示一致。
  - 实际恢复大于 0 时写入战斗日志“法宝恢复 X HP。”。
- 扩展测试：
  - 引擎层覆盖购买回春玉佩、胜利后恢复、战斗 HP 与 Run HP 同步。
  - 引擎层覆盖满血附近恢复不溢出。
  - Presenter 覆盖坊市文本显示“回春玉佩（法宝） / 30 灵石”。
  - 原型 UI 覆盖从坊市购买回春玉佩，并在后续战斗胜利后看到 HP 恢复与日志。

## 验收结果

已执行：

```bash
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

结果：

- `dotnet build` 成功，0 Error；保留项目既有 warning。
- `git diff --check` 通过；仅出现 Git 换行提示。
- Unity MCP `refresh_unity(scope=all, mode=if_dirty, compile=request, wait_for_ready=true)` 成功，期间发生一次可恢复断连，最终 Editor ready。
- Unity MCP `GameLogic.EditModeTests`：95/95 passed。
- Unity MCP `execute_code` 探针通过：
  - 返回 `PASS:Artifacts=1,HP=43,BattleHP=43,Stones=5,Log=True`。
  - 证明购买回春玉佩后进入下一场战斗，胜利结算时恢复 3 HP，并同步战斗 HP 与 Run HP。
- Unity MCP `read_console(types=["error","warning"])`：0 条。

## 假设与风险

- 当前只接入 `灵石矿` 和 `回春玉佩` 两个法宝，尚未实现完整 15 件法宝表、稀有度、掉落权重、宝箱或精英战法宝掉落。
- 当前法宝允许重复购买同名法宝；本阶段延续 Phase 27 的原型策略，没有新增唯一性限制。
- 回春玉佩当前只在战斗胜利结算时触发；失败结算、事件扣血、休息和坊市行为暂不触发。

## 后续建议

- 下一阶段可继续接入法宝掉落入口，例如精英战胜利后获得法宝，或宝箱节点获得法宝。
- 也可以继续接入下一个凡品法宝，逐步覆盖不同被动效果类型。
