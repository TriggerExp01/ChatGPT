# UIPrefab 制作规范

## Prefab 命名规则

- 黄金样板使用 `*_Golden.prefab` 后缀，例如 `MainRunWindow_Golden.prefab`。
- 正式窗口 Prefab 使用窗口名，例如 `MainRunWindow.prefab`、`EventPopup.prefab`。
- 主题模板放在 `Theme/Cultivation/Templates/`。
- 视觉基础资源使用语义化命名，例如 `Panel_DarkJade_GoldBorder`、`Button_Gold_Primary_Normal`。
- 不使用 `New Panel`、`Button (1)`、`Image Copy` 等临时名称提交。

## 目录规则

Cultivation 主题资源目录固定为：

```text
UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/
├── Background/
├── Panel/
├── Button/
├── Card/
├── RouteNode/
├── IconFrame/
├── Divider/
├── Decoration/
├── Popup/
├── Generated/
└── Templates/
```

UI 文档目录固定为：

```text
Doc/UI规范/
Doc/UI截图验收/
Doc/交付说明/
```

## RectTransform 规则

- 目标画布为 1920x1080 横屏。
- 全屏根节点锚点使用 `stretch`，偏移归零。
- 顶部栏锚定顶部并左右拉伸。
- 左侧角色区锚定左中，右侧卡牌区锚定右中。
- 中央路线区使用固定区域布局，节点不要依赖自由拖放的随机位置。
- 底部操作区锚定底部并左右留出安全边距。
- 常用间距以 8 的倍数为基准，优先使用 16、24、32、40。
- 文本、按钮、卡牌和节点不得贴边。

## Canvas / CanvasScaler / GraphicRaycaster 规则

- 独立窗口 Prefab 根节点必须具备 `RectTransform`、`Canvas`、`GraphicRaycaster`。
- 用于独立预览的样板可以包含 `CanvasScaler`，Reference Resolution 设置为 1920x1080。
- 接入 TEngine `UIModule` 的正式窗口需要兼容外部 `UIRoot` 的 Canvas 设置。
- 全屏窗口根 Canvas 需要 `overrideSorting = true`，具体层级交给 `UIModule` 管理。
- 不在业务 UI 中创建额外主相机，除非专门用于截图或 Editor 预览。

## TMP 使用规则

- 新增可读文本优先使用 TextMeshProUGUI。
- 按钮文字必须使用 TMP；若中文 TMP 字体资源缺失，不允许用空白或乱码截图提交。
- Phase77A 样板使用 `UnityProject/Assets/AssetRaw/Fonts/NotoSansCJKsc_Phase77A SDF.asset` 作为中文 TMP 字体资产。
- 标题、按钮、卡牌名、正文分别使用统一字号。
- 文本颜色优先使用米白、浅金、灰金、玉青。
- 重要文本允许描边或阴影，但不能影响可读性。
- 长文本必须设置自动换行、溢出策略或固定容器高度。
- 中文文本需要在 1920x1080 截图中确认不被裁切。

## 按钮状态规则

- 每个按钮必须规划 `normal`、`pressed`、`disabled`。
- 主按钮需要比次按钮更亮、更大、更突出。
- 次按钮统一尺寸、圆角、描边和文本样式。
- 禁用态使用低饱和灰绿，不使用 Unity 默认灰。
- 交互状态可以通过 Sprite Swap、Color Tint 或自定义组件实现，但视觉必须稳定。

## Panel 层级规则

- 建议层级顺序：背景层 -> 主布局区 -> 面板 -> 内容 -> 操作 -> 弹窗。
- 弹窗样板可默认 inactive，但层级必须清楚。
- 大面板不直接嵌套过多无命名空物体。
- 每个区域命名应表达职责，例如 `TopInfoBar`、`RouteArea`、`CardPreviewPanel`。
- 纯色矩形只能作为临时调试，不允许作为最终可交付 Prefab 的主面板。

## 安全区规则

- 顶部栏、底部按钮和左右主面板至少留出 40 像素安全边距。
- 移动端或异形屏适配阶段需要通过 `SetUISafeFitHelper` 或等效安全区方案处理。
- 本阶段黄金样板以 PC 横屏 1920x1080 为主，但布局不得依赖贴边。

## 可复用组件规则

- 按钮、卡牌、节点、标签、分隔线、弹窗框应做成可复制的结构块。
- 可复用块命名必须稳定，便于后续脚本或人工查找。
- 样板 Prefab 可以使用假数据，但结构要能迁移到真实数据绑定。
- 不把玩法规则、路线推进、战斗结算写进视觉 Prefab 或 UI 模板。

## 九宫格 Sprite 使用规则

- 面板、按钮、卡牌框、弹窗框等需要拉伸的 Sprite 必须设置 `Texture Type = Sprite`。
- 需要横向或纵向拉伸的 Sprite 必须设置合适 `Sprite Border`，并在 Prefab 中使用 `Image.Type = Sliced`。
- 九宫格边框必须保留角部装饰和描边厚度，不能因为拉伸变成糊边或直角纯色块。
- 大尺寸面板优先使用可切片的底图，不直接放大小图导致纹理模糊。
- 不能把适合图标的非切片 Sprite 强行拉成按钮或面板。

## Codex 制作 UI 的固定流程

1. 阅读 `AGENTS.md`、`Doc/产品方向.md`、当前阶段交付说明和相关 UI 规范。
2. 明确本阶段只做哪些 UI，默认一批 1-3 个 Prefab。
3. 先建立或更新黄金样板，不直接批量制作所有界面。
4. 制作主题资源和 Prefab 时遵守目录、命名、RectTransform 和 TMP 规则。
5. 生成 1920x1080 截图。
6. 对照 `Doc/UI规范/UI截图验收标准.md` 自检。
7. 截图仍像工程灰盒时先继续调整，不提交 Prefab。
8. 写交付说明，区分自动化验证、截图观察和人工主观风险。

## 截图后自检再提交

提交前必须确认：

- 有截图证据。
- 截图没有默认灰色按钮。
- 截图没有默认白色 Panel。
- 截图没有纯色矩形面板作为主 UI。
- 文字、间距、按钮、卡牌、节点没有明显错位或重叠。
- 第一眼能看出是修仙 / 卡牌 / 游戏 UI。
- 工作区没有混入 Unity `UserSettings` 布局噪声。
