# Phase79 Cultivation UI 组件化与 Demo 数据绑定交付说明

## 1. 阶段目标

Phase79 基于已经锁定为 Steam 原型结构基准的 `Steam_MainRoute_Phase78B.prefab` 继续推进，不再对 MainRoute 做整屏视觉返工。

本轮目标是把 MainRoute 中后续会被 EventPopup、Reward、CardLibrary、Battle 复用的 UI 形态拆成组件 Prefab，并建立 Demo ViewModel 与假数据绑定，让主界面可以通过数据刷新主要业务文本。

## 2. 本轮没有修改的范围

- 未修改战斗逻辑。
- 未修改路线逻辑。
- 未修改事件逻辑。
- 未修改奖励逻辑。
- 未修改启动链路。
- 未修改 Luban 配置生成脚本。
- 未修改 UIModule 核心逻辑。
- 未覆盖 Phase77B 黄金样板。
- 未继续大幅视觉返工 MainRoute。

## 3. 新增组件 Prefab

组件目录：

```text
UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Components/Steam/
```

本轮已生成以下 9 个组件：

```text
UI_CardItem.prefab
UI_RouteNode.prefab
UI_TopResourceItem.prefab
UI_CharacterStatusPanel.prefab
UI_PrimaryButton.prefab
UI_SecondaryButton.prefab
UI_StatusTag.prefab
UI_RewardItem.prefab
UI_PopupWindow.prefab
```

## 4. 新增 Demo 数据绑定脚本

ViewModel 目录：

```text
UnityProject/Assets/GameScripts/HotFix/GameLogic/Cultivation/UI/ViewModel/
```

新增文件：

```text
SteamMainRouteViewModel.cs
```

Demo 目录：

```text
UnityProject/Assets/GameScripts/HotFix/GameLogic/Cultivation/UI/Demo/
```

新增文件：

```text
SteamMainRouteDemoData.cs
SteamMainRouteDemoBinder.cs
```

绑定能力：

- 顶部资源：角色名、气血、灵力、灵石、秘境层数、天数。
- 左侧修士档案：姓名、境界、属性列表、状态牌。
- 中央路线节点：事件、战斗、宝箱、休憩、坊市等节点标签。
- 右侧卡牌预览：费用、标题、类型、描述、数量。

当前绑定只接入 `SteamMainRouteDemoData.Create()` 生成的假数据，不连接真实玩法系统。

## 5. 新增组件化 MainRoute Prefab

组件化样板 Prefab：

```text
UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_MainRoute_Componentized.prefab
```

说明：

- 保持 Phase78B 主界面的整体结构、比例和视觉层级。
- 通过组件 Prefab 实例搭建顶部资源、左侧档案、中央路线节点、右侧卡牌和底部按钮。
- 根节点挂载 `SteamMainRouteDemoBinder`，用于 Demo 数据刷新。

## 6. 新增 Editor 生成入口

新增文件：

```text
UnityProject/Assets/Editor/CultivationUI/CultivationUIPhase79ComponentizedGenerator.cs
```

Unity 菜单入口：

```text
Codex/Cultivation UI/Phase79/Generate Componentized MainRoute And Screenshot
Codex/Cultivation UI/Phase79/Generate Steam Components
Codex/Cultivation UI/Phase79/Generate Componentized MainRoute Prefab
Codex/Cultivation UI/Phase79/Capture Componentized MainRoute Screenshot
```

## 7. 截图输出

验收截图：

```text
Doc/UI截图验收/Steam_MainRoute_Componentized.png
```

截图观察结果：

- MainRoute 未整体重做，仍沿用 Phase78B 的三栏结构、卷轴地图中心和底部操作区。
- 顶部资源、左侧角色、路线节点、右侧卡牌文本已由 Demo 数据刷新。
- 截图不是空白图，1920x1080 下主体区域完整可见。

## 8. 仍是临时占位的内容

- 角色立绘、卡牌插画、地图纹理仍复用 Phase78B 临时素材。
- Demo 数据未接真实玩法逻辑，仅用于验证 UI 可刷新。
- 部分较长中文文案在卡牌和属性区存在拥挤风险，后续正式接入真实数据前需要继续做文本适配。
- 组件目前保留 uGUI `Text` 方案，未迁移到 TMP。

## 9. 验证结果

### Unity MCP

- 已通过 Unity MCP `refresh_unity` 刷新并等待 Editor ready。
- 已执行 Unity 菜单：

```text
Codex/Cultivation UI/Phase79/Generate Componentized MainRoute And Screenshot
```

- 已生成组件 Prefab、组件化 MainRoute Prefab 和截图。
- Unity Console Error：0。

### C# 编译

命令：

```bash
dotnet build UnityProject\UnityProject.sln --no-restore
```

结果：

```text
0 Error
```

存在既有警告：

```text
CSC warning USG0001: AdditionalFile.txt 数量警告
CS8632 nullable 注释上下文警告
System.Net.Http 版本冲突警告
System.IO.Compression 版本冲突警告
```

### 差异检查

命令：

```bash
git diff --check
```

结果在最终提交前执行并记录。

```text
通过，无空白或行尾问题。
```

## 10. Phase79 验收结论

Phase79 已完成 MainRoute 的 UI 组件化与 Demo 数据绑定基础：

- MainRoute 结构未大改。
- 9 个可复用组件 Prefab 已生成。
- Demo ViewModel 与 Binder 已建立。
- 顶部资源、左侧角色、路线节点、右侧卡牌可通过假数据刷新。
- 本轮未接入真实玩法逻辑，符合 Phase79 边界。
