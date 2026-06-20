# Phase 21 交付说明：战斗内使用丹药

> 日期：2026-06-20  
> 阶段：Phase 21  
> 范围：在 Phase 20 丹药槽基础上，接入小还丹战斗内使用闭环  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段把丹药从“可购买、可携带”推进到“可在战斗内使用”的最小闭环，对齐设计文档中“丹药为战斗内消耗品”的方向。

当前已支持：

- 小还丹具备战斗效果数据：恢复 10 HP。
- Run 战斗状态下可使用丹药槽中的小还丹。
- 使用后直接恢复当前战斗中的玩家 HP，并从丹药槽移除。
- 恢复不会超过玩家最大 HP。
- 非战斗状态、战斗已结束或非法丹药索引会拒绝使用。
- 原型 UI 在战斗手牌区显示可用丹药按钮。
- Presenter 快照和 Run 文本在战斗中读取实时战斗 HP，避免显示滞后。

## 2. 实际改动清单

- 更新 `PillDefinition`：
  - 新增 `HealAmount` 字段，作为当前最小战斗效果载体。
- 更新 `CultivationSeedData`：
  - `small_restore_pill` 配置为恢复 10 HP。
- 更新 `CultivationRunEngine`：
  - 新增 `UsePillInBattle()`。
  - 校验 Run 必须处于战斗中，且战斗仍在进行。
  - 使用丹药后恢复当前战斗玩家 HP，移除对应丹药并写入战斗日志。
- 更新 `CultivationRunPrototypePresenter`：
  - 快照中的 `PlayerHp` 在战斗中取 `CurrentBattle.Player.CurrentHp`。
  - Run 文本中的 HP 在战斗中也取实时战斗 HP。
- 更新 `CultivationRunPrototypeUI`：
  - 新增 `UsePillInBattle()` 交互入口。
  - 战斗手牌区显示丹药按钮，点击后使用对应丹药。
- 更新 EditMode 测试：
  - 覆盖小还丹恢复并消耗。
  - 覆盖不超量恢复。
  - 覆盖非战斗状态拒绝使用且保留丹药。
  - 覆盖 Presenter 快照读取战斗实时 HP。
  - 覆盖原型 UI 从坊市购买丹药后，在下一场战斗使用。

## 3. 当前覆盖能力

- 丹药已经具备“坊市购买 -> 进入丹药槽 -> 下一场战斗使用 -> 从丹药槽消耗”的完整最小链路。
- 当前丹药效果模型只覆盖恢复 HP，但已经为后续扩展增元丹、解毒丹、破境丹等不同效果留出定义位置。
- 使用丹药不消耗灵力，不进入牌堆、弃牌堆或消耗堆，保持与 Phase 20 的“丹药不占牌库位置”一致。

## 4. 验证与结果

已执行：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

结果：

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过；仍保留既有 AdditionalFile、nullable 注释上下文和 Unity/MCP 程序集版本 warning，无新增编译错误。
- `git diff --check`：通过。

Unity MCP 验收：

- `refresh_unity(scope=all, mode=if_dirty, compile=request, wait_for_ready=true)`：通过；刷新期间出现一次 Unity disconnect/retry，随后恢复 ready。
- `GameLogic.EditModeTests`：77/77 通过。
- `execute_code` 探针：小还丹使用后返回 `PASS:Hp=60,Pills=0,Outcome=InProgress`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 当前只接入小还丹的战斗内恢复效果，暂未实现增元丹、解毒丹、破境丹等其他丹药。
- 丹药使用目前不消耗灵力、不占用出牌次数，也没有“每回合限制”；这些规则后续需要结合完整丹药系统和 UI 设计再定。
- 正式丹药槽视觉、按钮布局和战斗 HUD 仍未制作；当前仍是功能验证型 uGUI 原型。
