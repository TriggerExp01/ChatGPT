# Phase79 MainRoute UI 止损回退交付说明

## 1. 背景

当前 `Steam_MainRoute_Componentized.png` 视觉验收不通过，并且相比上一版稳定结构明显退步：

- 整体布局被压扁。
- 左侧修士档案区域出现裁切、贴边、信息混乱。
- 右侧卡牌区退化为信息块。
- 中央卷轴地图变空，视觉焦点不足。
- 顶部资源栏文字和图标重叠。
- 整体观感偏坏掉的调试界面，不符合 Steam 修仙 Roguelite 卡牌 UI 方向。

本轮执行止损方案，不继续美化，不自由调整布局，不批量重做其他界面。

## 2. 回退策略

本轮保留 Phase79 中有价值的内容：

- `SteamMainRouteViewModel` 数据结构。
- `SteamMainRouteDemoData` 假数据。
- `SteamMainRouteDemoBinder` Demo 绑定逻辑。
- Phase79 已拆出的组件 Prefab，作为后续组件库基础保留。

本轮回退 MainRoute 视觉承载：

- 不再使用当前退化的 `Steam_MainRoute_Componentized.prefab` 作为 MainRoute 视觉基准。
- 以相对稳定的 `Steam_MainRoute_Phase78B.prefab` 为源，生成新的独立回退 Prefab。
- 生成时完全 unpack，避免 Restore Prefab 依赖 Phase78B 源 Prefab 漂移。

## 3. 输出文件

回退后的 MainRoute Prefab：

```text
UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_MainRoute_Restore.prefab
```

新截图：

```text
Doc/UI截图验收/Steam_MainRoute_Restore.png
```

交付说明：

```text
Doc/交付说明/Phase79_MainRoute_UI止损回退.md
```

新增 Restore 生成入口：

```text
UnityProject/Assets/Editor/CultivationUI/CultivationUIPhase79MainRouteRestoreGenerator.cs
```

Unity 菜单：

```text
Codex/Cultivation UI/Phase79/Restore MainRoute Visual Baseline And Screenshot
```

## 4. 必要修复内容

本轮只做止损级修复：

- 修复顶部资源栏图标与文字过近的问题。
- 修复右侧卡牌标题与描述文本溢出风险。
- 修复左侧状态牌局部贴边和底部裁切风险。
- 修复主按钮标签与图标对齐。
- 修复根 Canvas 的 1920x1080 缩放基准和锚点适配。
- 让 `SteamMainRouteDemoBinder` 同时兼容 Phase79 组件化命名和 Phase78B 原始稳定节点命名。

## 5. 明确未做事项

- 未继续修改整体布局比例。
- 未重做卡牌视觉风格。
- 未重做中央卷轴地图视觉风格。
- 未重做左侧修士档案视觉风格。
- 未批量修改其他四个界面。
- 未修改战斗逻辑。
- 未修改路线逻辑。
- 未修改事件逻辑。
- 未修改奖励逻辑。
- 未修改启动链路。
- 未修改 Luban。
- 未修改 UIModule 核心逻辑。

## 6. 截图观察

`Steam_MainRoute_Restore.png` 已回到 Phase78B 的稳定结构：

- 左侧仍是修士档案结构。
- 中央仍是卷轴地图与当前节点焦点。
- 右侧仍是 3 张卡牌预览结构，没有继续使用退化的信息块形态。
- 底部按钮区保持 Phase78B 主次关系。
- 顶部资源栏重叠情况较当前退化版有所缓解。

仍需后续人工验收的风险：

- Phase78B 本身不是最终高保真 UI，局部文字仍偏紧。
- 右侧卡牌长描述在正式数据接入前仍需统一文本规则。
- 本轮没有继续做视觉美化，只完成止损回退。

## 7. 验证结果

### Unity MCP

已执行：

```text
refresh_unity
Codex/Cultivation UI/Phase79/Restore MainRoute Visual Baseline And Screenshot
read_console
```

结果：

```text
Unity Console Error = 0
```

### C# 编译

命令：

```bash
dotnet build UnityProject\UnityProject.sln --no-restore
```

结果：

```text
0 Error
```

保留既有警告：

```text
System.Net.Http / System.IO.Compression 版本冲突警告
AdditionalFile.txt 数量警告
nullable 注释上下文警告
```

### Diff 检查

命令：

```bash
git diff --check
```

结果：

```text
通过。
```

## 8. 结论

MainRoute 当前止损方向已经完成：

- 视觉承载回退到 Phase78B 稳定结构。
- Phase79 的数据绑定逻辑被保留并兼容稳定节点。
- 退化版 `Steam_MainRoute_Componentized.prefab` 不再作为 MainRoute 视觉基准继续推进。
- 本轮没有继续自由发挥 UI 风格，也没有触碰业务逻辑链路。
