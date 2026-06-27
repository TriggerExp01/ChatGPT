# UI 丑因审计

## 审计范围

本次审计面向当前 Dev 分支的 UGUI / UI 资源现状，重点检查：

- `UnityProject/Assets/GameScripts/HotFix/GameLogic/Module/UIModule/`
- `UnityProject/Assets/AssetRaw/UIRaw/Single/CultivationCardGame/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/`
- 当前 Cultivation 原型运行窗口与已生成 UI 资源

本次只做 UI 视觉返工审计，不修改战斗、路线、秘境、奖励、启动链路或 Luban 生成脚本。

## 直接结论

当前项目已经有 Cultivation 原型入口和部分 UI 资源，但视觉生产线不完整。问题不是单个颜色不对，而是缺少可复用主题资产、黄金样板、截图验收和明确的 Prefab 制作规则。

上一版 UI 容易显得丑，主要原因是：

- 面板、按钮、卡牌、路线节点没有统一的主题资产体系。
- 视觉层级过度依赖纯色底、文字和简单边框。
- 当前 `CultivationRunWindow` 更像运行时拼出来的原型容器，不是可复用的高质量 UGUI Prefab。
- 缺少“先截图自检，再提交 Prefab”的硬门槛。
- 没有真实参考图或正式素材包时，容易把临时占位误认为最终美术。

## 当前 UI 为什么丑

### 默认控件感

`UIModule` 本身是窗口系统，不负责美术质量。当前问题出在具体窗口和资源层：如果窗口继续依赖默认 `Button`、默认 `Panel` 或运行时代码快速拼控件，即使颜色换成暗色，也会保留 Unity 默认控件的工程感。

当前 Cultivation 原型已有资源在 `Single/CultivationCardGame/` 下，但多数资源偏早期占位：

- 按钮、面板、节点、卡框体量小，缺少完整九宫格拉伸体系。
- 资源分散在背景、按钮、卡牌、节点等目录，缺少统一主题入口。
- 没有明确的黄金样板 Prefab 作为后续 UI 对齐对象。

### 默认灰按钮

已有业务 UI 如果继续使用 Unity 默认 `Button` 的灰色背景或只做 `Color Tint`，截图会直接失败。Phase77A 后必须使用主题按钮 Sprite，并配置 normal / pressed / disabled 状态。

### 默认白 Panel

默认白色 `Panel` 不能作为最终 UI。新增 Prefab 必须使用深玉色、暗褐色或其他主题 Sprite 面板，并带暗金描边、内阴影和统一内边距。

### 纯色矩形面板

纯色矩形即使不是白色，也会显得像调试面板。面板必须有：

- 可辨认边框；
- 内部层次；
- 轻微材质纹理或渐变；
- 统一的安全边距；
- 标题区、内容区、操作区。

### 字号混乱

旧占位 UI 容易在同一区域里混用多个相近字号，导致层级不清。Phase77A 规定 1920x1080 下的标题、正文、按钮、卡牌、图标标签各自使用稳定字号区间。

### 布局贴边

工程灰盒常见问题是大面板贴边、按钮贴底、文字贴框。黄金样板必须保留顶部、左右、底部安全边距，并保证卡牌和路线节点有足够呼吸空间。

### 无游戏感

如果只有“状态列表 + 按钮 + 文本框”，看起来会像后台工具。Cultivation UI 必须在第一眼体现：

- 修仙氛围；
- 秘境路线探索；
- 卡牌展示；
- 奖励预览；
- 当前节点状态；
- 主行动按钮。

## 当前缺少哪些视觉资产

本次返工前缺少一套统一可复用的主题资产：

- 暗色玉石背景；
- 暗纹背景；
- 深玉面板；
- 旧金描边面板；
- 主按钮 normal / pressed / disabled；
- 次按钮 normal / pressed / disabled；
- 常见 / 稀有 / 高阶卡框；
- 战斗、秘境、宝箱、休息、市场路线节点；
- 路线连线；
- 图标框；
- 分割线；
- 角花装饰；
- 秘境事件弹窗框；
- 遮罩。

## 当前是否有可复用 UI 素材

有早期占位素材，但不足以作为后续 UI 生产基线：

- `Single/CultivationCardGame/` 可作为历史占位参考，不作为新黄金样板的最终资源目录。
- Phase77A 新增主题目录 `Theme/Cultivation/Generated/`，作为当前高质量原型视觉资产统一入口。
- 分类目录 `Background/Panel/Button/Card/RouteNode/IconFrame/Divider/Decoration/Popup/` 保留，便于人工查找和后续分包。

## 当前是否有截图验收流程

本次 Phase77A 前没有足够硬的截图门槛。后续要求：

- 每个新增 UI Prefab 至少输出一张 1920x1080 截图。
- 截图路径写入交付说明。
- 截图中出现默认灰按钮、默认白 Panel、纯色矩形面板、文字拥挤或看不出游戏感时，不允许提交。
- MCP 截图不可用时，可以使用 Editor 工具通过 RenderTexture 截图，但必须写清原因。

## 审计后的处理策略

Phase77A 不继续微调旧灰盒，而是建立：

1. UI 视觉规范。
2. Prefab 制作规范。
3. 截图验收标准。
4. 参考图需求。
5. 统一主题目录。
6. 可复用 PNG / Sprite 资产。
7. `MainRunWindow_Golden.prefab` 黄金样板。
8. `MainRunWindow_Golden_Phase77A.png` 截图证据。

当前修仙方向只作为 Dev 分支默认原型方向，不等于最终产品方向锁死。
