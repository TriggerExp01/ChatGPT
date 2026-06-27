# Phase78 Cultivation SteamDemo UI 结构样板交付说明

## 阶段目标

Phase78 基于 5 张 Steam 版 UI 参考示意图，独立新增一组 `Cultivation / 修仙养成原型` 的 UI 结构样板，用于固定界面结构、信息层级、组件拆分和 Prefab 生产规范。

本阶段不是最终美术定稿。角色立绘、卡牌插画、背景图、图标、字体、纹理和动效均为临时占位，后续可以在不改变 UI 结构的前提下替换。

## 执行范围

本阶段只新增 UI 结构样板、通用组件 Prefab、占位 Sprite、截图验收图和交付文档。

未修改以下内容：

- 战斗逻辑
- 路线逻辑
- 秘境事件逻辑
- 奖励逻辑
- 启动链路
- Luban 配置生成脚本
- UIModule 核心逻辑
- Phase77B 黄金样板 `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/MainRunWindow_Golden.prefab`

## 目录约定

- SteamDemo 界面样板：`UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/`
- 通用组件 Prefab：`UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Components/`
- 临时占位 Sprite：`UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Placeholders/`
- 生成器：`UnityProject/Assets/Editor/CultivationUI/CultivationUISteamDemoGenerator.cs`
- 截图验收：`Doc/UI截图验收/`

## 5 个界面 Prefab 路径

- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_MainRoute.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_EventPopup.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_Battle.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_Reward.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_CardLibrary.prefab`

## 5 张截图路径

- `Doc/UI截图验收/Steam_MainRoute_Concept.png`
- `Doc/UI截图验收/Steam_EventPopup_Concept.png`
- `Doc/UI截图验收/Steam_Battle_Concept.png`
- `Doc/UI截图验收/Steam_Reward_Concept.png`
- `Doc/UI截图验收/Steam_CardLibrary_Concept.png`

截图分辨率均为 1920x1080。

## 通用组件 Prefab 列表

- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Components/Button_Primary.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Components/Button_Secondary.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Components/CardItem.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Components/RouteNode.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Components/PopupWindow.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Components/RewardItem.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Components/TopResourceItem.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Components/CharacterStatusPanel.prefab`

## 界面层级说明

### Steam_MainRoute

结构定位：主界面 / 秘境路线界面。

主要层级：

- `Background_MountainMist`：秘境山水氛围背景。
- `ScreenHeader`：参考图编号与界面说明。
- `TopResource_*`：顶部资源栏，复用 `TopResourceItem`。
- `Left_CharacterStatusPanel`：左侧修士档案，复用 `CharacterStatusPanel`。
- `Center_RouteMapPanel`：中部秘境地图区域。
- `RouteLine_*`：灵脉路线连线占位。
- `RouteNode_*`：路线节点，复用 `RouteNode`。
- `Right_CardDeckPanel`：右侧功法卡组预览。
- `MiniCard_*`：右侧卡牌缩略项，复用 `CardItem`。
- `Bottom_ActionDock`：底部操作区。
- `Btn_Bag`、`Btn_Card`、`Btn_Setting`、`Btn_Continue`：底部按钮。

### Steam_EventPopup

结构定位：秘境事件弹窗。

主要层级：

- `EventPopup_AncientWell`：事件弹窗主体，复用 `PopupWindow`。
- `HeroImage`：事件插图占位。
- `Body`：事件正文。
- `Option_1`、`Option_2`、`Option_3`：事件选择项。
- `RewardBadge*`：可能奖励占位。
- `Btn_Close`：关闭按钮。

### Steam_Battle

结构定位：战斗界面。

主要层级：

- `BattleBackdrop`：战斗场景背景占位。
- `Combatant_*`、`HpBack_*`、`HpFill_*`：敌我目标信息与血条。
- `PlayerSilhouette`、`EnemySilhouette`：人物战斗剪影占位。
- `PlayerStatusDock`：玩家战斗状态区。
- `HandCard_*`：底部手牌，复用 `CardItem`。
- `Btn_EndTurn`：主操作按钮。
- `Command_*`：抽牌堆、弃牌堆、消耗堆状态。

### Steam_Reward

结构定位：奖励 / 宝箱 / 战斗结算界面。

主要层级：

- `RewardTitle`：奖励界面标题。
- `Reward_Card`、`Reward_Jade`、`Reward_Scroll`：奖励选项，复用 `RewardItem`。
- `ChoiceHint`：选择提示。
- `Btn_Skip`、`Btn_Confirm`：底部操作按钮。

### Steam_CardLibrary

结构定位：功法 / 卡牌详情界面。

主要层级：

- `CategoryTabs`：左侧分类栏。
- `Tab_*`：功法分类按钮。
- `LibraryCard_*`：卡牌列表项，复用 `CardItem`。
- `DetailPanel`：右侧详情区。
- `SelectedCardPreview`：选中卡牌大图，复用 `CardItem`。
- `UpgradeTitle`、`UpgradeDesc`：升级说明。
- `Btn_Upgrade`、`Btn_Equip`、`Btn_Close`：详情区操作按钮。
- `Currency`：资源消耗提示。

## 后续接入 UIModule 的绑定建议

本阶段 Prefab 先作为结构样板，不直接接入 UIModule。后续接入时建议：

- 每个 SteamDemo 界面单独映射一个窗口或样板窗口配置，先不要复用到正式主流程。
- 通用组件优先包装成可绑定 View Item，例如卡牌项、路线节点、奖励项和资源项。
- 绑定字段以层级命名为准，例如 `TopResource_*`、`RouteNode_*`、`HandCard_*`、`LibraryCard_*`。
- 列表类数据建议由 View 层统一生成或刷新，不在 Prefab 内写死真实玩法逻辑。
- 事件弹窗的 `Option_*` 可绑定事件选项数据，点击回调仍由业务事件系统控制。
- 战斗界面手牌、敌人意图、牌堆数量只做 UI 显示绑定，不把战斗计算逻辑放进 UI Service。

## 结构定稿范围

本阶段可以视为结构定稿的内容：

- 5 个核心界面的屏幕信息区划分。
- 顶部资源栏、左侧角色状态、中央主内容区、右侧详情/卡组区、底部操作区的基本关系。
- 通用组件拆分列表和命名。
- 卡牌、路线节点、奖励项、弹窗、资源项的基础层级。
- 1920x1080 参考分辨率下的主要元素比例与布局方向。

## 临时美术占位范围

以下内容均为临时占位：

- 角色立绘和战斗人物剪影。
- 卡牌插图。
- 秘境事件插图。
- 奖励图标。
- 战斗背景。
- 纹理细节、光效、字体气质和最终颜色。
- 按钮、面板、卡牌边框的最终材质表现。

## Unity Console Error 状态

提交前通过 Unity MCP 读取 Console Error，结果为 0 条 Error。

## MCP 验证结果

通过 Unity MCP 执行 Phase78 生成器并验证：

- 8 个通用组件 Prefab 均可通过 `AssetDatabase.LoadAssetAtPath<GameObject>` 加载。
- 5 个 SteamDemo 界面 Prefab 均可通过 `AssetDatabase.LoadAssetAtPath<GameObject>` 加载。
- 5 个界面 Prefab 均包含 `Canvas`、`CanvasScaler`、`GraphicRaycaster`。
- 5 张截图文件均生成到 `Doc/UI截图验收/`，尺寸为 1920x1080。
- `Steam_MainRoute`、`Steam_EventPopup`、`Steam_Battle`、`Steam_Reward`、`Steam_CardLibrary` 均为独立新增样板，没有覆盖 Phase77B 黄金样板。

## 风险与后续

- 当前样板仍为结构参考，不代表最终 Steam 商店素材质量。
- 后续需要正式角色、背景、卡牌插画、图标、字体和纹理素材包替换占位资源。
- 后续接入 UIModule 时需要补充窗口脚本、数据绑定、列表复用策略和交互状态。
- 若要进入最终 UI 制作，应先对这 5 张截图做人工审美验收，再开始把结构样板迁移到正式窗口。
