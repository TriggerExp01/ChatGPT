# Phase77A 交付说明：Cultivation UI 视觉返工

## 结果摘要

本阶段按 `Cultivation / 修仙养成原型` 方向返工 UGUI 视觉基线，建立了 UI 审计、视觉规范、Prefab 制作规范、截图验收标准、参考图需求、主题资源目录、可复用 PNG / Sprite 资产，以及 `MainRunWindow_Golden.prefab` 黄金样板。

本阶段没有修改核心业务逻辑、启动链路、战斗 / 路线 / 秘境 / 奖励玩法 C#、Luban 配置生成脚本，也没有恢复旧肉鸽 UI 或旧视觉资源。

## 上一版为什么不通过

上一版的问题不是单个颜色或字号，而是视觉生产链缺失：

- 缺少 UI 丑因审计，无法判断问题来自资源、Prefab 结构还是截图流程。
- 缺少统一主题资源，容易继续使用默认灰按钮、默认白 Panel 或纯色矩形。
- 缺少黄金样板，后续 UI 没有可对齐的视觉基线。
- 缺少 1920x1080 截图门槛，容易提交“编辑器里能看见但截图很丑”的 Prefab。
- 早期截图曾出现文字不渲染或显示不完整的问题，因此不能作为通过证据。

## 本次目标

- 第一眼不再像 Unity 默认工程灰盒。
- 明确体现东方修仙、秘境路线、卡牌预览和游戏 UI 层级。
- 建立后续 UGUI Prefab 可复用的视觉资产与制作规则。
- 输出指定截图：`Doc/UI截图验收/MainRunWindow_Golden_Phase77A.png`。

## 实际新增 / 修改文件清单

文档：

- `Doc/UI规范/UI丑因审计.md`
- `Doc/UI规范/UIStyleGuide.md`
- `Doc/UI规范/UIPrefab制作规范.md`
- `Doc/UI规范/UI截图验收标准.md`
- `Doc/UI规范/UI参考图需求.md`
- `Doc/交付说明/Phase77A_Cultivation_UI视觉返工.md`

Editor 工具：

- `UnityProject/Assets/Editor/CultivationUI/CultivationUIGoldenSampleGenerator.cs`

主题资源：

- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Background/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Panel/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Button/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Card/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/RouteNode/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/IconFrame/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Divider/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Decoration/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Popup/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/`

字体与 TMP：

- `UnityProject/Assets/TextMesh Pro/`
- `UnityProject/Assets/AssetRaw/Fonts/NotoSansCJKsc-VF.ttf`
- `UnityProject/Assets/AssetRaw/Fonts/NotoSansCJKsc_Phase77A SDF.asset`
- `UnityProject/Assets/AssetRaw/Fonts/NotoCJK_LICENSE_OFL.txt`
- `UnityProject/Assets/AssetRaw/Fonts/NotoCJK_README_source.md`

Prefab 与截图：

- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/MainRunWindow_Golden.prefab`
- `Doc/UI截图验收/MainRunWindow_Golden_Phase77A.png`

## 游戏类型与 UI 风格确认

当前 UI 按 `Cultivation / 修仙养成原型` 理解，核心体验包括修仙、养成、路线选择、秘境事件、奖励、休息、市场、宝箱和卡牌战斗。

本次视觉方向是东方修仙、玄幻养成、秘境探索、卡牌路线、暗色玉石、旧金 / 暗金 / 铜金描边、低饱和、厚重、清晰、有游戏 UI 感。

这不是最终产品方向锁死。正式产品名、完整玩法边界、商业化定位和最终美术风格仍需后续阶段确认。

## 是否新增视觉资源

已新增一套临时但可复用的高质量原型 PNG / Sprite 资产，统一放在：

```text
UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/
```

至少包含：

- `BG_DarkJade_Gradient.png`
- `BG_Cultivation_DarkPattern.png`
- `Panel_DarkJade_GoldBorder.png`
- `Panel_DarkJade_Inner.png`
- `Panel_Parchment_Dark.png`
- `Button_Gold_Primary_Normal.png`
- `Button_Gold_Primary_Pressed.png`
- `Button_Gold_Primary_Disabled.png`
- `Button_Jade_Secondary_Normal.png`
- `Button_Jade_Secondary_Pressed.png`
- `Button_Jade_Secondary_Disabled.png`
- `Card_Frame_Common.png`
- `Card_Frame_Rare.png`
- `Card_Frame_Epic.png`
- `RouteNode_Battle.png`
- `RouteNode_Event.png`
- `RouteNode_Chest.png`
- `RouteNode_Rest.png`
- `RouteNode_Market.png`
- `RouteLine_Gold.png`
- `IconFrame_Jade.png`
- `Divider_Gold.png`
- `Corner_Decoration_Gold.png`
- `Popup_DarkJade_GoldBorder.png`
- `Mask_Dark.png`

## 黄金样板 Prefab 路径

```text
UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/MainRunWindow_Golden.prefab
```

Prefab 内容包括：

- 顶部角色与资源信息。
- 左侧角色状态。
- 中央秘境路线。
- 右侧卡牌 / 奖励预览。
- 底部操作按钮。
- 默认隐藏的 `EventPopup_StyleSample` 秘境事件弹窗样板。

## 截图路径

```text
Doc/UI截图验收/MainRunWindow_Golden_Phase77A.png
```

截图分辨率为 1920x1080。

## Unity / MCP 验证结果

本阶段使用 Unity Editor 工具生成 Prefab 与截图。Unity MCP 可读取 Editor 状态，并通过操作型工具执行生成器、读取控制台和检查 Prefab。

MCP GameView 截图在当前非 Play 预览状态下可能得到黑图，因此最终截图采用 Editor 工具以 RenderTexture 方式保存到 `Doc/UI截图验收/`。该方式与 Prefab 同源实例化，适合本阶段视觉验收。

已验证：

- `MainRunWindow_Golden.prefab` 可被 Unity 正常加载。
- Prefab 根节点包含 `RectTransform`、`Canvas`、`CanvasScaler`、`GraphicRaycaster`。
- Prefab 层级中包含默认隐藏的 `EventPopup_StyleSample`。
- Prefab 中 `TextMeshProUGUI` 数量为 89，旧 `UnityEngine.UI.Text` 数量为 0。
- Prefab 中按钮数量为 7，覆盖底部按钮和弹窗选项按钮。
- 中文 TMP 字体资产 `NotoSansCJKsc_Phase77A SDF.asset` 可加载。
- 最终截图已输出到 `Doc/UI截图验收/MainRunWindow_Golden_Phase77A.png`。

## Console Error 状态

最终 Unity 控制台 Error 读取结果为 0 条。

过程中曾出现一次 MCP 插件自身的 `System.Net.Sockets.NetworkStream` disposed 日志，发生在 Unity 脚本域重载后的 MCP 连接恢复阶段；后续工具调用恢复正常，最终控制台 Error 重新读取为 0 条。该日志不属于游戏代码、Prefab 或生成器编译错误。

## 做了什么

- 做了 UI 丑因审计。
- 建立 Cultivation UI 风格规范。
- 建立 UGUI Prefab 制作规范。
- 建立截图验收标准。
- 建立参考图需求文档。
- 建立 `Theme/Cultivation` 主题目录。
- 生成可复用 PNG / Sprite 视觉资产。
- 引入 TMP Essentials 和中文 TMP 字体资源，使样板文本和按钮文字使用 TMP。
- 返工 `MainRunWindow_Golden.prefab`。
- 输出 1920x1080 截图。

## 没有做什么

- 没有修改核心业务逻辑。
- 没有修改战斗、路线、秘境、奖励等玩法 C#。
- 没有修改启动链路。
- 没有修改 Luban 配置生成脚本。
- 没有批量重做所有 UI。
- 没有恢复旧肉鸽玩法。
- 没有恢复旧 UI Prefab。
- 没有恢复旧视觉资源。
- 没有把修仙方向写成最终商业产品锁死。

## 当前风险

- 如果没有正式 UI 素材包或参考图，本阶段只是高质量原型视觉，不等于最终美术品质。
- `NotoSansCJKsc-VF.ttf` 作为中文 TMP 样板字体资源引入，后续正式产品应由用户确认字体授权、体积和平台适配策略。
- 黄金样板使用假数据和占位图标，尚未接入真实业务数据绑定。
- 当前截图证明的是 1920x1080 横屏黄金样板效果，不代表移动端、安全区、多分辨率已经完成适配。
- 当前 EventPopup 是默认隐藏的样板节点，尚未接入正式秘境事件流程。

## 下一步建议

- 提供 2-3 张修仙主界面、卡牌展示、路线节点和秘境事件弹窗参考图，再做正式视觉风格二次收敛。
- 基于 `MainRunWindow_Golden.prefab` 复制拆分可复用组件：按钮、卡牌、节点、弹窗。
- 将黄金样板接入真实 UIModule 加载链路前，先做单屏 PlayMode 或 Editor 预览验证。
- 后续任何新增 UI Prefab 都必须先输出截图并对照 `UI截图验收标准.md` 自检。
