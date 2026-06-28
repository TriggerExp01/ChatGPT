# Phase78C MainRoute Final 视觉基准

## 结果摘要

Phase78C 只针对 MainRoute 主界面继续收束，不返工另外 4 张 SteamDemo 界面。

本轮以以下参考图作为 MainRoute 最终视觉目标基准：

```text
Doc/UI参考图/Phase78_Targets/Target_MainRoute.png
```

本轮输出：

```text
UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_MainRoute_Final.prefab
Doc/UI截图验收/Steam_MainRoute_Final.png
Doc/交付说明/Phase78C_MainRoute_Final视觉基准.md
```

## 实际改动清单

- 在 `CultivationUIPhase78BMainRouteGenerator.cs` 中新增 Phase78C / Final 专用生成入口。
- 新增 `Steam_MainRoute_Final.prefab`，保留 `Steam_MainRoute.prefab` 和 `Steam_MainRoute_Phase78B.prefab` 作为对照。
- 新增 `Phase78C` 专用纹理目录，避免覆盖 Phase78B 已提交资源。
- 输出 `Steam_MainRoute_Final.png` 作为本轮视觉验收截图。

## 参考图复刻范围

本轮只复刻目标图的 UI 结构与视觉关系，不逐字复制目标图中的 AI 生成文字。

已对齐的部分：

- 顶部是细资源栏，包含角色/气血/灵力/灵石/秘境层数/天数信息。
- 左侧是修士档案，包含人物立绘占位、姓名、境界、属性列表和状态牌。
- 中央最大区域是卷轴秘境地图，包含纸张底色、左右卷轴轴杆、水墨山形、发光路线和当前节点焦点。
- 右侧是当前手牌 / 功法预览，3 张卡牌竖向排列，具备费用角标、标题、类型标签、插画区和浅色描述区。
- 底部是左侧功能按钮和右侧突出主按钮，未做成整条粗重工具栏。

## 仍为临时占位

以下内容仍是结构样板级占位，不代表最终美术资产：

- 修士立绘为程序生成占位图，后续应替换为正式角色立绘。
- 卡牌插画为程序生成占位图，后续应替换为正式功法卡图。
- 卷轴纸面、山形、路线光效和节点图标仍为程序生成或现有主题资源组合。
- 卡牌文字为正式中文文案方向，但未绑定真实业务数据。

## 禁止范围执行情况

本轮未修改：

- 战斗逻辑
- 路线逻辑
- 事件逻辑
- 奖励逻辑
- 启动链路
- Luban 配置生成脚本
- UIModule 核心逻辑
- Phase77B 黄金样板

## 验证与结果

已执行：

```text
dotnet build UnityProject\UnityProject.sln --no-restore
```

结果：构建成功，0 Error。

已通过 Unity MCP 执行：

```text
Codex/Cultivation UI/Phase78C/Generate MainRoute Final Visual Baseline And Screenshot
```

Unity MCP 验证结果：

- Editor 状态 ready。
- `CultivationUIPhase78BMainRouteGenerator.cs` 导入成功。
- `validate_script` 返回 0 Error、0 Warning。
- `Steam_MainRoute_Final.prefab` 可通过 `manage_asset get_info` 读取，资源类型为 `UnityEngine.GameObject`。
- `Doc/UI截图验收/Steam_MainRoute_Final.png` 已生成。
- 生成后 Console Error 为 0。

## 风险与后续

- `Steam_MainRoute_Final.png` 已可作为 MainRoute 后续结构基准，但素材仍是占位级。
- 下一步如果继续做其余 4 张界面，应同样以对应 `Target_*.png` 为基准逐图复刻，不再批量自由发挥。
- 若要进入正式 UI 制作，需要用正式立绘、卡牌插画、图标、卷轴纹理和字体资产替换占位资源。
