# Phase78B MainRoute 参考图复刻返工

## 结果摘要

Phase78B 本轮只返工 `Steam_MainRoute_Concept`，没有继续批量修改另外 4 个界面。

本轮输出目标：

- Prefab：`UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_MainRoute_Phase78B.prefab`
- 截图：`Doc/UI截图验收/Steam_MainRoute_Concept_Phase78B.png`
- 参考图：`Doc/UI参考图/Phase78_Targets/Target_MainRoute.png`
- 失败对照图：`Doc/UI参考图/Phase78_Failed/Failed_MainRoute.png`

## Phase78 不通过原因

Phase78 旧版 `Steam_MainRoute.prefab` 和失败截图 `Failed_MainRoute.png` 的问题不是文件缺失，也不是 Prefab 没生成，而是视觉结构没有按参考图复刻：

- 中央区域仍像绿色半透明流程图面板，不像卷轴地图。
- 路径节点更接近普通流程图，缺少灵脉曲线、卷轴纸面和地图焦点。
- 左侧修士信息偏调试属性列表，角色档案感不足。
- 右侧卡牌是信息块式列表，缺少真实卡牌的标题、费用、插画和描述结构。
- 底部操作区像后台工具条，主按钮视觉权重不足。

## 本轮只返工 MainRoute 的原因

用户明确要求 Phase78B 暂停 5 张界面批量制作，先只复刻主界面 / 秘境路线界面。MainRoute 是后续 Steam UI 结构样板的第一张基准图，只有主界面验收通过后，才继续返工：

- `Steam_EventPopup_Concept`
- `Steam_Battle_Concept`
- `Steam_Reward_Concept`
- `Steam_CardLibrary_Concept`

## 路径清单

参考图路径：

```text
Doc/UI参考图/Phase78_Targets/Target_MainRoute.png
```

失败图路径：

```text
Doc/UI参考图/Phase78_Failed/Failed_MainRoute.png
```

新 Prefab 路径：

```text
UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_MainRoute_Phase78B.prefab
```

新截图路径：

```text
Doc/UI截图验收/Steam_MainRoute_Concept_Phase78B.png
```

生成器路径：

```text
UnityProject/Assets/Editor/CultivationUI/CultivationUIPhase78BMainRouteGenerator.cs
```

## 结构对照

| 区域 | Target_MainRoute.png | Phase78B 新截图 | 结果 |
|---|---|---|---|
| 顶部栏 | 约 70-80 高的细资源栏，包含角色名、气血、灵力、灵石、秘境层数和天数 | 顶部使用细黑底资源栏，包含 `凌云舟`、气血、灵力、灵石、`云雾秘境 · 第一层，第 7 天` | 已按结构复刻 |
| 左侧档案 | 宽约 330-380，立绘占主要视觉，下面是境界、属性和状态牌 | 左侧宽约 390，包含头像纸面、修士名、境界、属性列表和 3 个状态牌 | 已按结构复刻，立绘仍为占位 |
| 中央地图 | 视觉中心是宣纸卷轴地图，两侧卷轴轴杆，水墨山形，节点通过发光灵脉连接 | 中央改为卷轴纸面、左右轴杆、水墨山形、发光灵脉曲线和当前节点光环 | 已按结构复刻 |
| 路线节点 | 节点有上下层次，当前节点有明显光环和视觉焦点 | 战斗、奇遇、宝箱、休憩、坊市围绕当前节点分布，当前节点有双层光环 | 已按结构复刻 |
| 右侧卡牌 | 右侧是当前手牌，3 张卡牌有费用珠、标题、插画、描述和类型标签 | 右侧 3 张卡牌按竖向排列，具备费用珠、标题、插画、描述和类型标签 | 已按结构复刻，插画仍为占位 |
| 底部操作区 | 左侧小功能按钮，右侧突出主按钮，不是整条工具栏 | 左侧为背包、功法、图鉴、状态 4 个按钮，右侧为突出 `进入秘境` 主按钮 | 已按结构复刻 |

## 已按参考图复刻的区域

- 页面从绿色流程图面板改为暗色桌面背景加中央卷轴地图。
- 中央地图从直线流程节点改为卷轴纸面、淡水墨山形、灵脉曲线和中心焦点。
- 左侧从调试面板改为修士档案：头像区域、名字、境界、属性、状态牌。
- 右侧从普通信息块改为三张卡牌预览：费用珠、标题区、插画区、描述区、类型标签。
- 顶部栏减重为细资源栏，信息完整但不过度 MMO 化。
- 底部操作区去掉整条粗工具栏，主按钮独立突出。

## 仍是临时占位的区域

以下内容只用于结构样板，不代表最终 Steam 商店级美术质量：

- 左侧修士立绘仍是程序生成占位图，不是正式角色立绘。
- 右侧卡牌插画仍是程序生成占位图，不是正式功法卡图。
- 卷轴纸面、水墨山形、灵脉、节点图标为程序生成和既有主题资源组合，仍需正式美术替换。
- 图标符号、费用珠和部分按钮材质仍沿用现有 Cultivation UI 主题资源。
- 文字内容是结构样板数据，尚未绑定真实业务数据。

## 禁止范围执行情况

本轮未覆盖：

- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_MainRoute.prefab`
- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/MainRunWindow_Golden.prefab`

本轮未修改：

- 战斗逻辑
- 路线逻辑
- 事件逻辑
- 奖励逻辑
- 启动链路
- Luban 配置生成脚本
- UIModule 核心逻辑

## Console Error 状态

执行 Phase78B 专用生成菜单前，Unity Console 已清空，用于隔离本轮结果。

执行菜单：

```text
Codex/Cultivation UI/Phase78B/Generate MainRoute Ref Replica And Screenshot
```

生成后 Unity Console 只记录 4 条 Log：

- MCP 执行菜单命令日志。
- `Generated Phase78B MainRoute prefab: Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_MainRoute_Phase78B.prefab`
- `Captured Phase78B MainRoute screenshot: D:\Work\Unity_Project\TEngine_Game\Doc\UI截图验收\Steam_MainRoute_Concept_Phase78B.png`
- `Phase78B MainRoute reference replica generated.`

本轮生成后未观察到新增 Console Error。

## MCP 验证结果

Unity MCP 已验证：

- Editor 状态为 ready，非 PlayMode，未编译中。
- `Assets/Editor/CultivationUI/CultivationUIPhase78BMainRouteGenerator.cs` 成功导入。
- `validate_script` 返回 0 个 Error、0 个 Warning。
- 通过 MCP 执行 Phase78B 专用菜单成功。
- `Steam_MainRoute_Phase78B.prefab` 可通过 `manage_asset get_info` 读取，资源类型为 `UnityEngine.GameObject`。
- 截图文件已生成到 `Doc/UI截图验收/Steam_MainRoute_Concept_Phase78B.png`。

## 自动化验证结果

已执行：

```text
dotnet build UnityProject\UnityProject.sln --no-restore
```

结果：构建成功，0 个 Error。存在仓库既有的 `System.Net.Http` 和 `System.IO.Compression` 版本冲突警告，和本轮 UI 生成器无直接关系。

## 风险与后续

- Phase78B 新截图已经从结构上贴近目标图，但仍是程序生成 UI 样板，不是最终美术资源。
- 下一步不应继续自由发挥，应先由人工对 `Steam_MainRoute_Concept_Phase78B.png` 做主观视觉验收。
- 只有 MainRoute 通过后，再按同样方式逐图返工 `EventPopup`、`Battle`、`Reward`、`CardLibrary`。
