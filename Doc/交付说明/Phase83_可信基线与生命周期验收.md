# Phase83 可信基线与生命周期验收交付说明

## 1. 阶段目标

Phase83 是《仙途·天命》完整游戏合同生效后的第一个工程阶段。本阶段不增加新玩法和 UI，而是修复会让后续交付结论失真的基础问题：

- Fast Enter Play Mode 测试内部失败却被强制标记为通过。
- Editor `Awake` 与 `AfterSceneLoad` 重复打开流程并重置运行数据。
- TEngine 本地化模块缺少内置 CSV，导致每次 Play Mode 产生警告。
- 特殊生命周期测试与普通 EditMode 套件混跑时可能阻塞或取消整套回归。
- Phase82 交付说明存在无效 UTF-8 与大量替换字符。

## 2. 修改内容

### 2.1 生命周期测试不再假阳性

`FastEnterPlayModeTwiceEditModeTest` 调整为：

- `EnterPlayMode` / `ExitPlayMode` 直接由顶层测试枚举器返回，兼容 Test Framework 1.1.33。
- 根据当前 Enter Play Mode Options 判断是否期望 Domain Reload。
- 记录两轮是否完整执行。
- 使用 `finally` 恢复日志监听和 `LogAssert` 状态并生成报告。
- `_result` 不是 `PASS` 时使用真实断言令 Test Runner 失败，不再无条件 `Assert.Pass()`。

新增 `GameLogic.LifecycleTests.asmdef`，将两次 Play Mode 的特殊生命周期测试与普通 `GameLogic.EditModeTests` 分离。

### 2.2 最小游戏流程重复打开改为幂等

`GameEntry` 的 `Awake` 与 `AfterSceneLoad` 统一调用 `OpenCultivationPrototypeIfNeeded()`。

`CultivationGameFlowController.OpenOrCreate()` 现在：

- 已有实例时返回同一实例。
- 运行数据已经存在时不再调用 `EnterBoot()`。
- 仅在视图缺失时重建视图并重新绑定当前数据。
- EditMode 下创建 EventSystem 时不调用 `DontDestroyOnLoad`。

新增 `CultivationGameFlowControllerTests`，验证连续两次打开后：

- 控制器引用不变。
- RuntimeData 引用不变。
- Day、NodeIndex、HP、灵石和牌组不被重置。
- 场景中只有一个控制器。

### 2.3 建立可运行的三语基础表

新增：

```text
UnityProject/Assets/AssetRaw/Localization/Localization.csv
```

当前基础语言：

- 简体中文：`Chinese`
- 繁体中文：`ChineseTraditional [zh-TW]`
- 英文：`English`

基础表先覆盖产品名、确认、取消、返回、开始/继续修行、设置、退出和 Run 胜负标题，并绑定到 `GameEntry.prefab` 的 `LocalizationManager.innerLocalizationCsv`。

### 2.4 文档与本地输出

- 重写损坏的 `Phase82_Cultivation_最小游戏流程闭环.md`，恢复有效 UTF-8 和历史边界说明。
- `.gitignore` 忽略本地生命周期报告、日志、Unity 布局和脚本包装输出。
- 补齐 `Assets/Editor/Verification.meta` 与 `Assets/Tests/EditMode/FastEnterPlayMode.meta`，避免 Unity 文件夹 GUID 漂移。

## 3. 验证结果

### 3.1 Unity Test Runner

普通 EditMode：

```text
GameLogic.EditModeTests
266 / 266 Passed
```

独立生命周期：

```text
GameLogic.LifecycleTests
1 / 1 Passed
```

生命周期测试真实完成两轮 Play Mode，每轮等待至少 10 秒；补入三语表后两轮均为：

```text
Error = 0
Warning = 0
Exception = 0
Assert = 0
```

PlayMode：

```text
GameLogic.PlayModeTests
4 / 4 Passed
```

### 3.2 幂等回归

定向测试：

```text
OpenMinimalGameplayLoopTwice_ReusesControllerAndPreservesRuntimeData
1 / 1 Passed
```

测试输出只包含一次：

```text
[CultivationFlow] Boot -> MainRoute
```

### 3.3 编译与差异

```text
dotnet build UnityProject\UnityProject.sln --no-restore
结果：0 Error，10 个既有 warning
```

独立测试工程：

```text
GameLogic.EditModeTests.csproj：0 Error / 0 Warning
GameLogic.LifecycleTests.csproj：0 Error / 0 Warning
```

```text
git diff --check
结果：通过
```

Unity 最终状态：

- Unity `2022.3.17f1`。
- 活动场景 `Assets/Scenes/main.unity`。
- Console 清空后重新读取：`0 Error / 0 Warning`。

## 4. 既有警告

解决方案构建仍包含以下历史警告，未在本阶段扩大范围处理：

- Source Generator `USG0001` AdditionalFile 数量警告。
- `UIBase.cs` 可空引用注释上下文警告。
- MCP 编辑器程序集与 Unity 自带 `System.Net.Http` / `System.IO.Compression` 版本冲突。

这些警告不影响本阶段编译和 Unity Test Runner，但必须在发布收口前消除或形成明确的第三方隔离策略。

## 5. 本阶段未完成与下一阶段

本阶段没有把 Phase82 占位流程包装成完整游戏。当前正式入口仍未接入已有的真实 `CultivationRunEngine + BattleEngine`。

下一阶段固定为：

1. 以 `CultivationRunState + CultivationRunEngine + BattleEngine` 作为唯一业务事实。
2. 淘汰 Phase82 固定 14 / 6 伤害、假奖励和第二套路由数据。
3. 串联真实 MainRoute、卡牌战斗、奖励/跳过、坊市、闭关、秘境、宝箱和 Boss。
4. 在真实业务入口成立后制作 `MainRoute_GoldenV2` 与 `Battle_Golden`，不再继续给 Restore 占位 UI 调颜色。

## 6. 边界

本阶段没有修改：

- 战斗、路线、事件、奖励的业务规则。
- MainRoute 视觉、UI Prefab 或美术资产。
- `ProjectSettings`。
- Luban 配置生成链路。
- 已删除的旧肉鸽内容。
