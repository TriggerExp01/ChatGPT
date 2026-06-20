# Phase35 交付说明 - 完整跑团 UI 与金丹入口可视化

## 结果摘要

本阶段把运行原型从“文本面板 + 操作按钮”升级为覆盖完整跑团链路的 UI 原型。界面现在包含标题/状态、资源条、路线图、运行总览、当前节点、战斗详情、牌组/行囊、操作区、手牌/丹药/坊市操作区，能展示战斗、奖励、路线选择、闭关、坊市、宝箱、秘境、完成/失败等核心状态。

同时承接上一轮未提交的路线扩展：筑基层短路线结束后进入金丹期第一场“金丹魔修”战斗，突破后会显示金丹境界、生命/灵力/手牌成长，并在 UI 路线图中标出金丹节点。

## 实际改动清单

- `CultivationSeedData` 新增金丹期普通敌人 `金丹魔修`，并把默认分支原型路线从“双头冰火蟒”后接到金丹入口战。
- `CultivationRunPrototypePresenter` 新增结构化 `RunPrototypeViewModel`：
  - 资源统计：境界、HP、灵力、手牌、灵石、牌组、丹药、法宝。
  - 路线节点：当前、可选、已走过、未探索，并附带境界层和节点类型。
  - 牌组、丹药、法宝和上下文操作摘要。
  - 状态中文名、节点类型中文名和阶段标题。
- `CultivationRunPrototypeUI` 升级为完整 uGUI 原型：
  - Header 增加标题行、状态文本、资源条、路线图。
  - 主体三栏保留运行总览、战斗详情、牌组/行囊。
  - 操作按钮升级为标题 + 详情两段式卡片，减少长文本堆叠。
  - 新增 StatsBar 和 MapBar 可测节点，方便后续 Prefab 化和视觉验收。
- EditMode 测试新增完整 UI 结构、ViewModel、金丹节点可视化和金丹入口完成路径覆盖。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `System.Net.Http` / `System.IO.Compression` 引用冲突、`USG0001`、`CS8632` 等。
- `git diff --check`
  - 结果：通过。
- Unity MCP `refresh_unity`
  - 结果：通过。过程中出现可恢复 domain reload 断连，最终 Editor ready。
- Unity MCP targeted EditMode 测试
  - `GameLogic.Tests.CultivationRunPrototypeUITests`
  - `GameLogic.Tests.CultivationRunPrototypePresenterTests`
  - `GameLogic.Tests.CultivationRunEngineTests`
  - 结果：107/107 Passed。
- Unity MCP 完整 `GameLogic.EditModeTests`
  - 结果：123/123 Passed。
- Unity MCP `execute_code` UI 探针
  - 结果：`Status=战斗中 / 战斗：InProgress; Stats=8; Map=13; HP=True; Stones=True; GoldenNode=True`
  - 确认 UI 对象树中真实生成资源条、路线图、金丹节点和关键资源卡。
- Unity MCP `read_console`
  - 结果：0 Error / 0 Warning。

## 假设与风险

- 本阶段仍是代码生成的 uGUI 原型，不是最终商业 Prefab；没有导入外部美术、动效、音效或 UI 图集。
- 路线、敌人和商品仍在 `CultivationSeedData` 中硬编码，内容继续扩展时应迁移到配置表或数据资产。
- `金丹魔修` 当前只接入现有意图类型表达普通战压力；设计文档中的金丹被动、强化回复、眩晕等更完整机制还未实现。
- UI 已具备完整状态覆盖，但还需要后续视觉样板、Prefab 生产规范和截图验收来提升美术质量。

## 可选下一步

- Phase36：接入金丹期专属机制，包括金丹被动选择、敌人强化/回复和眩晕 debuff。
- Phase37：把完整 UI 原型拆成可复用 Prefab 或 TEngine UI Window，并接入资源加载流程。
- Phase38：做 UI 视觉样板与截图验收，开始替换临时代码生成控件。
