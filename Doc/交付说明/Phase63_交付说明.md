# Phase63 交付说明 - 玄黄宗最小可玩门派切片

## 结果摘要

本阶段开始补齐六门派目标中的第四个门派：玄黄宗。当前先按“可进入 Run、可拿奖励、可升级、可被固定种子自动走局跑通”的最小范围落地，不在本阶段引入新的岩甲、崩甲或永久防御状态系统。

玄黄宗当前战斗定位为重防御、破防、抽牌循环与受击反伤，全部复用现有稳定机制：护盾、破防、抽牌、受击反伤和基础伤害。

## 实际改动清单

- 门派入口
  - `CultivationSect` 新增 `Earth`。
  - `CultivationRunPrototypePresenter` 增加“玄黄宗”中文显示。
  - 原型 Run UI 增加“玄黄宗开局”按钮。
  - `RunFixedSeedAutoPlay` 支持指定门派，默认仍保持剑宗，兼容既有测试和调试入口。
- 玄黄宗卡牌池
  - 新增 `RockStrike` / 岩击。
  - 新增 `EarthSplittingPalm` / 裂地掌。
  - 新增 `RockWall` / 岩壁。
  - 新增 `StoneSkinArt` / 石皮术。
  - 新增 `EarthEscape` / 岩遁。
  - 新增 `CounterSlash` / 反击斩。
  - 每张牌补充一层与二层升级树，优先覆盖护盾、破防、抽牌、降费和受击反伤。
- Run 数据
  - 新增 `CreateEarthSectStarterDeck()`。
  - 新增 `CreateEarthSectRewardPool()`。
  - `CreateStarterDeck`、`CreateRewardPool`、原型路线奖励池分发均接入玄黄宗。
- 测试
  - EditMode 覆盖玄黄宗起始牌组、奖励池和分支路线奖励池。
  - PlayMode 新增玄黄宗固定种子自动走局，验证原型 UI 可以从玄黄宗开局推进到当前路线完成。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 通过，0 Error。
  - 仍保留项目既有 warning：`USG0001`、nullable `CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突等。
- `git diff --check`
  - 通过；仅有 Git 工作副本 LF/CRLF 转换提示。
- Unity MCP 验证
  - `mcpforunity://editor/state`：可读取，Editor ready。
  - `refresh_unity(mode="force", scope="all", compile="request", wait_for_ready=true)`：通过；刷新期间发生一次 Unity 域重载断连，工具自动恢复并返回 ready。
  - `run_tests(EditMode, assembly_names=["GameLogic.EditModeTests"])`：通过，191/191。
  - `run_tests(PlayMode, assembly_names=["GameLogic.PlayModeTests"])`：通过，2/2。
  - `read_console(types=["error","warning"])`：0 条 Error/Warning。

## 假设与风险

- 本阶段没有实现玄黄宗设计文档中更完整的“岩甲”“崩甲”“永久防御成长”等专属机制；这些需要新增状态、结算顺序和 UI 表达，建议另开阶段处理。
- 当前玄黄宗使用现有护盾、破防、抽牌和受击反伤近似设计定位，目标是先完成可玩门派闭环。
- 自动走局证明的是固定种子、固定策略和当前原型路线的可通行性，不代表真实玩家策略、失败局、多路线平衡或手感体验已经完成。

## 可选下一步

- Phase64 可继续补药王谷，建议先明确是否新增“中毒/毒爆/持续恢复”完整状态系统。
- 也可以先扩玄黄宗专属机制，把当前近似实现升级为设计文档中的岩甲/崩甲闭环。
