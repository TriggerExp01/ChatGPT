# Phase77B 交付说明：Cultivation UI 视觉二次返工

## 结果摘要

Phase77A 工程结构基本通过，但人工视觉验收不通过。本阶段进入 Phase77B，不再沿着“深绿色底 + 粗金边矩形”小修小补，而是把 `MainRunWindow_Golden.prefab` 重构为更接近 `东方修仙卷轴界面 + 秘境地图路线 + 功法卡牌展示` 的黄金样板。

本阶段只修改 UI 视觉资源、黄金样板 Prefab、Editor 生成器、截图验收文档和交付说明。没有修改战斗、路线、秘境事件、奖励、启动链路、Luban 配置生成脚本或 UIModule 核心逻辑，也没有恢复旧肉鸽玩法、旧 UI Prefab 或旧视觉资源。

## Phase77A 为什么不通过

Phase77A 的问题不是功能结构，而是视觉仍停留在程序员美术阶段：

- UI 主要依赖深绿色底和粗金边矩形，材质差异不足。
- 背景缺少远山、云雾、符纹、灵气等修仙题材信号。
- 卡牌像信息块，缺少费用珠、插画区、卡面分区和稀有度差异。
- 路线节点像流程图，缺少秘境地图和灵脉路线感。
- 金边过粗、过多、过统一，视觉拥挤且廉价。
- 左侧角色状态像调试面板，底部操作栏像后台工具栏。
- 整体缺少视觉焦点，中央路线区面积大但吸引力不足。

## Phase77B 视觉方向

本阶段按以下关键词重构：

- 远山剪影
- 云雾层
- 秘境地图
- 灵脉路线
- 淡法阵 / 符纹
- 玉简 / 卷轴 / 暗宣纸
- 功法卡牌
- 费用珠
- 少量暗金与朱砂点缀
- 墨绿色、宣纸色、玉青色为主
- 克制细边框，不再全屏粗金边

## 实际改动

新增或重做的主要视觉资源：

- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/BG_Cultivation_MountainMist.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/BG_Cultivation_RunePattern.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/BG_Cultivation_Vignette.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Panel_Main_JadeThin.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Panel_Map_Mist.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Panel_Bamboo_Tag.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Panel_Scroll_Note.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Button_Primary_Scroll_Normal.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Button_Secondary_Jade_Normal.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Card_Cultivation_Common.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Card_Cultivation_Rare.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Card_Cultivation_Epic.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Card_ArtPlaceholder_Sword.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Card_ArtPlaceholder_Qi.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Card_ArtPlaceholder_Talisman.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/CostOrb_Gold.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/RouteMap_MistPanel.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/RoutePath_SpiritLine.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Node_Battle_Sword.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Node_Event_Scroll.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Node_Chest_Box.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Node_Rest_Meditation.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Node_Market_Pavilion.png`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Generated/Node_Current_Glow.png`

分类目录同步输出到：

- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Background/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Panel/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Button/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Card/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/RouteNode/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Decoration/`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Divider/`

Editor 生成器：

```text
UnityProject/Assets/Editor/CultivationUI/CultivationUIGoldenSampleGenerator.cs
```

黄金样板 Prefab：

```text
UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/MainRunWindow_Golden.prefab
```

新截图：

```text
Doc/UI截图验收/MainRunWindow_Golden_Phase77B.png
```

## 截图自检

提交前自检结果：

- 这张截图是否第一眼像修仙游戏：是。
- 路线区是否像秘境地图：是，中央已改为灵脉路线图、地图雾层、当前节点发光。
- 卡牌是否像真正卡牌：是，已有费用珠、插画区、标题条、描述区、稀有度边框差异。
- 底部是否不像工具栏：是，底部高度和边框减重，主行动按钮更突出。
- 是否减少了粗金边滥用：是，主面板和地图区改为细边框，金色主要用于焦点和强调。
- 是否有视觉焦点：是，当前秘境节点、主行动按钮和右侧功法卡形成主要焦点。
- 是否不再只是绿底金边矩形：是，背景、面板、卡牌、路线节点和按钮已有不同材质语言。

## Unity / MCP 验证结果

- Unity MCP 可读取 Editor 状态，当前实例为 `UnityProject@7216d4a2`，Unity 版本 `2022.3.17f1`。
- 通过 MCP 执行菜单 `Codex/Cultivation UI/Generate Golden Sample And Screenshot` 生成资源、Prefab 和截图。
- Unity Console Error 最终读取结果：0 条。
- `dotnet build UnityProject\UnityProject.sln --no-restore` 通过，0 Error，仅保留既有的 `System.Net.Http` / `System.IO.Compression` / `USG0001` 警告。
- 截图以 Editor RenderTexture 方式输出到 `Doc/UI截图验收/MainRunWindow_Golden_Phase77B.png`。

## 业务边界

本阶段没有修改：

- 战斗逻辑
- 路线逻辑
- 秘境事件逻辑
- 奖励逻辑
- 启动链路
- Luban 配置生成脚本
- UIModule 核心逻辑

## 字体与 TMP 风险

当前项目已有 `NotoSansCJKsc-VF.ttf`，Phase77B 为了保证截图稳定可读，黄金样板内文本暂时使用 UGUI `Text` 绑定该字体。过程中发现当前 Unity Editor 会话里 TMP 动态 SDF 字体资产存在 Atlas 空引用 / TMP Mobile SDF Shader 解析问题，若强制使用 TMP 生成截图会报错。

因此本阶段把“可稳定截图验收”放在第一优先级，没有把当前字体方案包装成最终方案。后续正式 UI 仍建议：

- 确认正式中文字体授权；
- 修复或重新导入 TMP Essentials / TMP Shader；
- 再把黄金样板迁移回 TextMeshProUGUI；
- 重新输出截图验收。

## 仍然需要注意的风险

- 当前视觉资源为程序生成的高质量原型资源，不等于正式商业美术素材包。
- 卡牌插画、路线节点、背景山水仍是占位级资产，后续应替换为正式参考图或素材包。
- 当前截图证明 1920x1080 横屏黄金样板效果，不代表移动端、安全区、多分辨率适配已经完成。
- `EventPopup_StyleSample` 仍是默认隐藏的样板节点，尚未接入真实秘境事件流程。
