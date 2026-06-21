# Phase70 交付说明：通用法宝规则接入

## 结果摘要

本阶段按设计文档补齐 4 件通用法宝的实际规则，并把牌库上限规则纳入 Run 状态与原型 Presenter 数据层。范围控制在纯逻辑、测试和展示数据，不修改 UI Prefab、场景、视觉资源或交互外观。

本阶段落地的法宝：

- `储物袋`：牌库上限 +3。
- `聚灵阵`：每场战斗首回合额外 +1 灵力。
- `护心镜`：每场战斗首次受到伤害 -50%。
- `飞剑令`：每场战斗首次攻击伤害 +5。

## 实际改动清单

- `CultivationRunState`
  - 新增 `DefaultDeckLimit = 15` 与 `DeckLimit`。
  - 境界突破时牌库上限每层 +2。
  - 新增 `AddDeckLimitBonus()` 供 Run 级法宝效果使用。
- `CultivationRunReward`
  - 新增 4 类通用法宝效果类型：牌库上限、首回合灵力、每战首次受击减伤、每战首次攻击加伤。
  - 为 `ArtifactDefinition` 增加对应效果读取属性。
- `BattleState` / `BattleEngine`
  - 新增每场首次攻击加伤的触发状态与消费逻辑。
  - 新增每场首次受击减伤的触发状态与消费逻辑。
  - 伤害日志会标注 `飞剑令 +N` 与 `护心镜触发`。
- `CultivationRunEngine`
  - 选择奖励和坊市买牌时检查 `DeckLimit`，达到上限时拒绝并保持资源不变。
  - 统一通过 `AddArtifact()` 添加法宝，确保宝箱、精英掉落、坊市购买都会应用 Run 级效果。
  - 开战时应用聚灵阵、护心镜、飞剑令等战斗级法宝效果。
- `CultivationSeedData`
  - 新增 `储物袋`、`聚灵阵`、`护心镜`、`飞剑令` 定义。
  - 将 4 件新法宝加入原型坊市和通用法宝奖励池；旧坊市商品顺序保持稳定，新增商品追加在后。
- `CultivationRunPrototypePresenter`
  - Run 文本和资源栏显示牌库上限。
  - 增加 4 件新通用法宝的效果摘要、图标键和战斗状态摘要。
- EditMode 测试
  - 覆盖默认牌库上限、突破 +2、储物袋 +3。
  - 覆盖满牌库时奖励选牌与坊市买牌拒绝，且不扣灵石、不移除商品、不改变奖励状态。
  - 覆盖聚灵阵开战加灵力、护心镜每战只触发一次、飞剑令每战只触发一次。
  - 覆盖 Presenter 快照、资源栏、法宝图标键和状态摘要。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过。
  - 仍有既有警告：`USG0001`、`CS8632`、`System.Net.Http/System.IO.Compression` 版本冲突警告；本阶段未新增编译错误。
- `git diff --check`：通过。
  - Git 在 Windows 下提示 LF 将被替换为 CRLF，不是空白错误。
- Unity MCP `refresh_unity(mode=if_dirty, scope=scripts, compile=request, wait_for_ready=true)`：通过。
  - 期间 Unity MCP 出现 transient disconnect，但工具自动恢复并返回 Editor ready。
- Unity MCP EditMode 相关回归：
  - `GameLogic.Tests.CultivationRunEngineTests`
  - `GameLogic.Tests.CultivationRunPrototypePresenterTests`
  - `GameLogic.Tests.CultivationBattleEngineTests`
  - 合计 `199/199` 通过。
- Unity MCP 控制台 Error 检查：0 条 Error。

## 假设与风险

- 本阶段只落地规则和展示数据，不做法宝 UI 面板、图标资源、Prefab 布局和动效表现。
- 新增法宝加入通用奖励池后，后续依赖随机法宝掉落的种子结果可能发生变化；本阶段已通过当前相关测试回归。
- `聚灵阵` 目前按“进入战斗时直接把当前灵力加 1”实现，因此首回合可显示为 `4/3` 这类临时超上限状态，符合“额外获得 1 灵力”的临时资源语义。
- 护心镜与已有 `不动明王印` 首次受击减伤属于不同法宝来源；当前执行顺序为护心镜先计算，再执行不动明王印既有逻辑。

## 可选下一步

- 继续补齐剩余通用法宝或门派法宝规则，保持每阶段只落一组可验证内容。
- 在确认 UI 方向后，把本阶段 Presenter 的 `IconKey` 和 `StatusSummary` 接入真实法宝列表/面板。
- 针对新法宝做固定种子小路线试玩采集，输出操作序列、日志和截图，用于后续数值打磨。
