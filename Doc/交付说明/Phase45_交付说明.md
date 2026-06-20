# Phase45 交付说明 - 火云宗最小卡池数据入口

## 结果摘要

本阶段在现有剑宗跑团原型基础上，补入火云宗的最小可运行数据入口。当前 `CultivationSeedData` 新增火云宗 3 张核心卡牌、12 张初始牌组和奖励池方法，覆盖 GDD 中火云宗前期的核心定位：灼烧 DOT、全体灼烧和攻防循环。

本阶段不新增暴击、雷击连锁、中毒等尚未在战斗引擎中实现的机制，也不修改 UI 和资源加载链路；目标是让第二个门派先能用现有战斗系统跑通，并为后续门派选择界面、解锁系统和完整卡池扩展提供数据入口。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 新增火云宗卡牌 `BurningPalm` / `焚天掌`：单体伤害 + 灼烧。
  - 新增火云宗卡牌 `FlameFormula` / `烈火诀`：全体灼烧。
  - 新增火云宗卡牌 `FireCloudStep` / `火云步`：护盾 + 抽牌，作为火云宗循环/防御牌。
  - 新增 `CreateFireCloudSectStarterDeck()`，提供 12 张火云宗初始牌组。
  - 新增 `CreateFireCloudSectRewardPool()`，提供火云宗最小奖励池入口。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 新增 `FireCloudSectStarterDeckUsesBurnAndCyclingCards`，验证火云宗初始牌组结构、数量和关键效果。
  - 新增 `FireCloudSectCardsCanApplyAreaBurnInBattle`，验证火云宗全体灼烧卡能在现有战斗引擎中对多名敌人施加灼烧并结算伤害。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `USG0001`、`CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突。
- `git diff --check`
  - 结果：通过。
- `dotnet test UnityProject\GameLogic.EditModeTests.csproj --no-build --list-tests --verbosity normal`
  - 结果：普通 dotnet runner 未发现 Unity EditMode 测试，仅能证明项目可构建，不能替代 Unity Test Framework。
- Unity MCP
  - 结果：未恢复。
  - 现象：`mcpforunity://editor/state` 持续 `stale_status`；`read_console`、`execute_code` 超时；直接 TCP 连接 Unity MCP 端口 6400 发送 framed `ping` 也超时。

## 假设与风险

- 火云宗当前只接入最小卡池数据，不代表完整 `卡牌池设计_火云宗.md` 的 27 张专属卡已经实现。
- 火云宗暂时复用现有战斗引擎机制；灼烧、全体目标、护盾、抽牌已可用，但火云宗后续高级牌若需要烈焰印记、引爆、灼烧转化等新机制，需要单独扩展 `CardEffectType` 和 `BattleEngine`。
- Unity MCP 当前桥接失效，本阶段缺少正式 Unity Test Framework 验收，不能视为完整阶段通过。

## 可选下一步

- Phase46：恢复 Unity MCP 后补跑 Phase44 / Phase45 定向 EditMode 测试和完整 `GameLogic.EditModeTests`。
- Phase47：加入门派选择入口，让跑团可以在剑宗和火云宗初始牌组之间切换。
- Phase48：扩展火云宗中期奖励牌和烈焰印记/引爆机制。
