# Phase67 交付说明 - 魔道专属法宝接入

## 结果摘要

本阶段承接 Phase66 留下的魔道专属法宝缺口，接入 `血魔珠` 与 `天魔心` 两件魔道专属法宝。当前魔道 Run 的精英/宝箱法宝池会在通用原型法宝基础上追加这两件专属法宝；获得后会在后续战斗开始时注入战斗内被动。

## 实际改动清单

- 法宝数据
  - 新增 `血魔珠`：每场战斗前 1 次自伤被免疫。
  - 新增 `天魔心`：每损失 10% 最大 HP，卡牌伤害 +3%；HP 低于 20% 时额外 +20%。
  - 新增 `CreateArtifactRewardPool(CultivationSect sect)`，让魔道路线包含专属法宝，其他门派保持通用原型法宝池。
- 战斗结算
  - `BattleState` 新增自伤免疫次数与法宝损血增伤状态。
  - `BattleEngine.ApplySelfHpLoss()` 接入血魔珠免疫首次自伤。
  - `DealCardDamageValue()` 接入天魔心损血档位增伤与低血额外增伤，覆盖普通攻击、噬血、多段、连锁和已统一走卡牌伤害管线的效果。
- Run 流程
  - `CultivationRunEngine` 在进入战斗时按已持有法宝注入战斗被动。
  - 宝箱与精英掉落沿用现有 `Artifacts`、`ChestArtifacts`、`DroppedArtifacts` 记录链路。
- 测试
  - 新增 EditMode 覆盖：魔道法宝池包含专属法宝且非魔道不包含。
  - 新增 EditMode 覆盖：通过宝箱获得血魔珠后，下一场战斗第一张血祭掌不损失 HP。
  - 新增 EditMode 覆盖：通过宝箱获得天魔心后，10/100 HP 状态下剑气诀伤害获得法宝加成。

## 验证与结果

- 已执行 `dotnet build UnityProject\UnityProject.sln --no-restore`。
  - 结果：通过，0 Error。
  - 仍有项目既有 warning：`System.Net.Http` / `System.IO.Compression` 版本冲突、AdditionalFile 警告。
- 已执行 `git diff --check`。
  - 结果：通过；仅有 Git 工作区 LF/CRLF 策略提示。
- 已通过 Unity MCP `refresh_unity`。
  - 结果：成功，编辑器 ready；刷新期间出现可恢复的 Unity MCP disconnect/retry。
- 已通过 Unity MCP EditMode 测试。
  - 结果：新增 3 条测试全部通过。
  - 测试项：
    - `DemonicSectArtifactPoolIncludesExclusiveArtifacts`
    - `BloodDemonOrbPreventsFirstSelfHpLossEachBattle`
    - `HeavenlyDemonHeartAddsMissingHpDamageBonus`
- 已检查 Unity 控制台。
  - 结果：0 条 error/warning。

## 假设与风险

- `天魔心` 的低血阈值按“HP 低于或等于 20%”实现；设计文档写“低于 20%”，若需要严格小于 20%，后续可单独调整。
- `天魔心` 目前只加成统一卡牌伤害管线，不加成中毒、灼烧等回合开始 DOT 与敌人反击类非卡牌直接伤害。
- PlayMode 测试 runner 本次曾启动后停在 0/6 未进入用例，控制台无错误；本阶段改用 EditMode 纯逻辑测试完成规则验收。后续做 UI/真实试玩阶段时应重新排查 PlayMode runner 状态。

## 可选下一步

- Phase68 可做六门派专属法宝池完整度审计，统一把其他门派的 10 件专属法宝接入奖励池和战斗被动。
- 也可以先做魔道 `天魔真解` 的 Run 级三选一 UI，把当前原型效果拆成真正的 Run 被动选择。
