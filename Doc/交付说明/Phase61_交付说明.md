# Phase61 交付说明 - 天雷阁受击反伤与 UI 资产补充

## 结果摘要

本阶段继续补全天雷阁卡牌池，新增“受击反伤”战斗机制，并接入两张防御向雷系奖励牌：

- 雷盾：获得护盾，并以概率对攻击者反伤。
- 雷击反弹：获得少量护盾，本回合每次受到攻击时对攻击者反伤。

同时补充了当前机制需要的 UI 占位图标 `ui_icon_counter.png`，纳入 `CultivationUIAssetCatalog` 和 `ui_asset_manifest.json`，后续正式 UI 资产可按同路径替换。

## 实际改动清单

- 战斗机制
  - `BattleState` 新增 `AttackCounterDamage`、`AttackCounterChancePercent`。
  - `CardEffectType` 新增 `AttackCounter`。
  - `BattleEngine` 在敌人攻击命中后触发受击反伤；闪避成功不会触发受击反伤。
  - 受击反伤在下一个玩家回合开始时清空，符合“本回合受到攻击”窗口。
- 天雷阁卡牌池
  - 新增 `ThunderShield` / 雷盾。
  - 新增 `ThunderStrikeRebound` / 雷击反弹。
  - 两张牌加入天雷阁奖励池，不加入起始牌组。
  - 补充两张牌的一层与二层升级树，优先落地护盾、降费、反伤数值、反伤概率等当前已支持效果。
- UI 与资产
  - 原型战斗 UI 显示受击反伤状态。
  - 卡牌摘要支持展示 `AttackCounter`。
  - 新增 `Assets/AssetRaw/UIRaw/Single/CultivationCardGame/Icons/ui_icon_counter.png`。
  - `CultivationUIAssetCatalog` 与 `ui_asset_manifest.json` 纳入反伤图标。
- 测试
  - 覆盖受击反伤命中、概率未触发、护盾吸收后仍反伤、非攻击意图不触发、下一回合清空。
  - 覆盖天雷阁新奖励牌进入奖励池但不进入起始牌组。
  - 覆盖 UI 资产清单包含反伤图标。
  - 覆盖 Presenter 卡牌摘要显示受击反伤。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 通过，0 Error。
  - 仍有既有 warning：`MSB3277`、`USG0001` 等，本阶段未新增阻断错误。
- `git diff --check`
  - 通过。
- Unity MCP
  - `refresh_unity(... wait_for_ready=true)`：通过；刷新期间发生域重载短暂断连，工具自动恢复并返回 ready。
  - `run_tests(EditMode, assembly_names=["GameLogic.EditModeTests"])`：通过，189/189 Passed。
  - `read_console(types=["error","warning"])`：0 条 Error/Warning。

## 假设与风险

- 本阶段先不实现“反伤可暴击”和“反伤附带眩晕”，因为它们需要把反伤接入暴击/控制概率链路；已在升级树中优先使用当前稳定可测效果。
- 受击反伤只在敌人攻击命中后触发；闪避成功时仍走已有闪避反击机制，避免两套反击同时结算。
- 新增 UI 资产仍是项目自有程序化占位图，不代表最终美术品质。
- 当前验收仍以单元测试、EditMode 测试、资源导入和控制台检查为主，不能替代真实游玩时对整体节奏、反馈、可读性和卡组压力的判断。下一阶段应优先补 PlayMode 走局验收与可玩性调试入口。

## 可选下一步

- Phase62 优先建立可玩性调试入口与 PlayMode 固定种子走局验收，用真实 Unity MCP 进入场景、截图、点按钮、走一局，降低“只靠冒烟/单元测试”的盲区。
- 之后再继续补天雷阁防御牌高阶效果，或开始下一个门派的最小起始牌组与奖励池切片。
