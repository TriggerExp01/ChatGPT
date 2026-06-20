# Phase69 交付说明：法宝展示数据与战斗生效摘要

## 结果摘要

本阶段在不调整 UI 外观、Prefab 或场景布局的前提下，补强原型 UI 的 Presenter 数据层，让法宝条目和战斗文本能直接展示图标键、来源、效果摘要与当前战斗生效状态。该阶段服务于后续法宝 UI 面板细化，范围控制在 `CultivationRunPrototypePresenter` 与对应 EditMode 测试。

## 实际改动清单

- `RunPrototypeInventoryItem` 新增：
  - `IconKey`：提供法宝占位图标键，例如 `artifact_sword`、`artifact_fire`、`artifact_thunder`。
  - `SourceTag`：标记法宝来源，例如 `坊市`、`精英`、`宝箱`、`秘境/其他`。
  - `StatusSummary`：提供当前状态摘要，战斗中会显示剩余次数、计数、护盾、中毒上限、当前增伤等动态信息。
- `CultivationRunPrototypePresenter` 新增 `FormatArtifactEffectSummary()`：
  - 为通用法宝与门派专属法宝生成统一中文效果摘要。
- 战斗文本新增 `法宝生效：` 段落：
  - 战斗中列出已持有法宝及当前战斗生效状态。
  - 示例：`天魔心(当前伤害 +47%（含低血加成）)`、`万剑归宗剑(下一张攻击功法伤害 +50%)`。
- 新增 Presenter 测试：
  - 覆盖法宝图标键、来源标签、状态摘要。
  - 覆盖法宝战斗生效文本。
  - 覆盖剩余 10 件门派专属法宝的效果摘要关键词。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过。
- `git diff --check`：通过。
- Unity MCP `refresh_unity`：通过；期间出现 transient disconnect，但工具自动恢复且 Editor ready。
- Unity MCP EditMode `CultivationRunPrototypePresenterTests`：25/25 通过。
- Unity MCP EditMode 相关回归：
  - `CultivationRunPrototypePresenterTests`
  - `CultivationBattleEngineTests`
  - `CultivationRunEngineTests`
  - 合计 189/189 通过。

## 假设与风险

- 本阶段只产出 Presenter 数据与文本摘要，不修改真实 UI 布局；图标键只是稳定占位，不代表已有美术资源。
- 秘境获得法宝当前没有独立来源列表，因此暂时归类为 `秘境/其他`。
- 后续如果新增独立法宝面板或图标资源，应直接复用 `IconKey`、`SourceTag`、`StatusSummary`，避免 UI 层反推法宝逻辑。

## 可选下一步

- Phase70 可继续做法宝面板的实际 UI 渲染：在现有原型界面中展示法宝列表、来源标签和状态摘要。
- 也可以转向设计表中尚未落地的通用法宝完整化，如储物袋、聚灵阵、护心镜、飞剑令等更广泛 Run/战斗规则。
