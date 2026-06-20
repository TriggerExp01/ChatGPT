# Phase 10 交付说明：剑宗初始牌组第二层升级

> 日期：2026-06-20  
> 阶段：Phase 10  
> 范围：为剑宗初始牌组补齐可运行的第二层升级切片  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段按 GDD 和 `卡牌池设计_剑宗.md` 中“每张卡最多 2 次升级、每次 2 分支”的方向推进，先为当前已经进入原型的剑宗初始牌组补上第二层升级数据：

- `剑气诀`、`破甲符`、`护体真气`、`剑步`、`轻身术`、`回春丹` 的第一层升级牌现在都继续提供 2 个第二层升级分支。
- 第二层升级只使用当前战斗引擎已经支持的效果：伤害、护盾、抽牌、治疗、破防。
- Run 引擎不需要改状态机；闭关升级逻辑天然支持“升级后的卡继续可升级”。

## 2. 实际改动清单

- 更新 `CultivationSeedData`：
  - 为 6 张初始牌的 12 个第一层升级结果增加第二层分支。
  - 第二层终态牌不再继续带升级项，符合当前“最多 2 次升级”的切片边界。
- 更新 `CultivationRunEngineTests`：
  - 原“第一层升级后不可再升级”的测试改为验证第一层升级后仍有第二层分支。
  - 新增双闭关路线测试，验证同一张牌可在后续闭关中完成第二层升级并继续推进 Run。

## 3. 当前覆盖能力

- 初始牌组已经具备两次升级的最小闭环。
- 闭关节点可以连续升级同一张牌，第二层终态不会继续出现在升级候选中。
- 后续加入剑气印记、锋锐、多段攻击、消耗等机制后，可以把当前部分占位分支替换为更贴近完整设计的终态。

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

- `refresh_unity(mode=force, scope=scripts, compile=request, wait_for_ready=true)`：通过；期间出现一次常见域重载断连并自动恢复。
- `GameLogic.EditModeTests`：32/32 通过。
- `execute_code` 探针：构造双闭关路线，连续升级 `剑气诀 -> 灵动剑气 -> 无影剑气`，最终进入精英战；返回 `PASS second-layer card=无影剑气, effects=2, node=elite`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 当前第二层升级没有实现剑气印记、锋锐、多段攻击、消耗等尚未进入战斗引擎的关键词。
- `CultivationSeedData` 已开始膨胀，后续应优先迁移到配置/Luban 或至少拆分种子数据文件。
- 本阶段只覆盖剑宗初始牌组，不代表完整剑宗 28 张卡牌池已经落地。
