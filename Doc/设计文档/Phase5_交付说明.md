# Phase 5 交付说明：闭关卡牌升级选择

> 日期：2026-06-20  
> 阶段：Phase 5  
> 范围：在 Phase 4 的闭关恢复节点基础上，补齐最小卡牌升级选择  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段让闭关节点从“只恢复 HP”推进到“恢复 HP + 可选择升级一张卡牌”的最小闭环：

- 卡牌定义支持升级选项。
- 闭关节点进入时会生成当前牌组中可升级卡牌的候选列表。
- 玩家可以选择牌组中的一张可升级牌，并选择该牌的一个升级分支。
- 选择后会把牌组中的原卡替换为升级后的卡牌。
- 闭关恢复仍然生效，并在升级后推进到下一节点。

这对应 GDD 中“闭关节点恢复 HP + 升级一张卡牌”和“每次升级从 2 个分支中选择 1 个”的核心方向。

## 2. 实际改动清单

- 新增 `CardUpgradeOption`：
  - 表达升级分支 id、名称、描述和升级后的卡牌定义。
- 扩展 `CardDefinition`：
  - 增加升级选项列表。
  - 增加 `CanUpgrade`。
- 新增 `CultivationRestUpgradeChoice`：
  - 表达闭关时某个可升级牌在牌组中的索引与原卡。
- 扩展 `CultivationRunState`：
  - 增加 `RestUpgradeChoices`。
- 扩展 `CultivationRunEngine`：
  - 闭关节点进入时刷新可升级候选。
  - 新增 `RestAndUpgrade(state, deckIndex, upgradeOptionIndex)`。
  - 升级后执行闭关恢复并推进下一节点。
- 扩展 `CultivationSeedData`：
  - 为剑宗初始牌和当前奖励牌接入第一层升级分支。
- 扩展 `CultivationRunEngineTests`：
  - 验证闭关会生成升级候选。
  - 验证闭关升级会替换牌组卡牌并进入下一战。
  - 验证升级后的牌暂不继续出现在升级候选中。
  - 验证选择不可升级卡会被拒绝。

## 3. 当前覆盖能力

- `剑气诀` 可升级为：
  - `追魂剑气`：伤害提升到 11。
  - `灵动剑气`：灵力消耗降为 0。
- `破甲符`、`护体真气`、`剑步`、`轻身术`、`回春丹` 已有第一层两个分支。
- 当前奖励牌 `御剑式`、`流云护身`、`小还丹` 已有第一层两个分支。
- 闭关节点可以在恢复 HP 的同时升级一张牌。

## 4. 验证与结果

已执行：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

结果：

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过；保留既有 Unity/MCP 引用版本警告，无新增编译错误。
- `git diff --check`：通过。

Unity MCP 验收：

- `refresh_unity(mode=force, scope=scripts, compile=request, wait_for_ready=true)`：通过；过程中发生一次 MCP 断连重试，最终恢复并 ready。
- `read_console(types=["error","warning"])`：刷新后 0 条。
- `GameLogic.EditModeTests`：21/21 通过。
- `execute_code` 探针：默认 Run 连续通过前两战后进入闭关，升级 `剑气诀` 为 `sword_qi_damage_1`，HP 从 55 恢复到 85，并进入 `stone_demon_leader` 战。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 当前只实现第一层升级分支，升级后的卡牌暂不带第二层升级选项。
- 升级数据目前写在 `CultivationSeedData` 中，尚未接入 Luban 配置表。
- 闭关目前没有“只升级不恢复”或“恢复/升级二选一”的 UI 决策；逻辑入口支持 `Rest` 只恢复，也支持 `RestAndUpgrade` 恢复并升级。
- 当前没有正式 UI 展示升级候选，仍通过逻辑测试和 MCP 探针验收。

## 6. 下一阶段建议

Phase 6 可继续推进以下之一：

- 补第二层升级树。
- 把升级数据迁移到配置表。
- 做 Run 调试 UI，展示路线、HP、闭关升级候选和当前牌组。
- 做最小路线分支选择。
