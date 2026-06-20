# Phase 29 交付说明：精英战法宝掉落入口
> 日期：2026-06-20  
> 范围：建立精英战胜利后的法宝掉落入口  
> 状态：已完成并通过 Unity MCP 验收

## 目标

设计文档要求法宝通过精英战斗、宝箱、坊市获取，并明确“精英战斗必定掉落法宝”。Phase 27 和 Phase 28 已经完成坊市购买和两件法宝效果，本阶段补上第一个非坊市获取入口。

本阶段先做最小可验收版本：

- 精英节点可以配置一个法宝掉落池。
- 精英战胜利后从掉落池获得 1 件法宝。
- 普通战斗不会掉落法宝。
- 原型路线的 `石魔首领` 配置当前已实现的两件法宝作为掉落池。

## 实际改动

- 扩展 `CultivationRunNode`：
  - 新增 `ArtifactRewardPool`。
  - 允许节点配置可掉落法宝池。
- 扩展 `CultivationRunState`：
  - 新增 `DroppedArtifacts`，记录通过战斗掉落获得的法宝。
- 扩展 `CultivationRunEngine`：
  - 精英战胜利结算时从 `ArtifactRewardPool` 抽取 1 件法宝。
  - 掉落法宝加入 `Artifacts` 和 `DroppedArtifacts`。
  - 战斗日志写入“精英战获得法宝：X。”。
  - 普通战斗和没有法宝池的精英战不会掉落。
- 扩展原型数据：
  - 新增 `CreatePrototypeArtifactRewardPool()`。
  - 原型精英节点 `石魔首领` 使用 `灵石矿`、`回春玉佩` 作为掉落池。
- 扩展原型展示：
  - Run 文本显示 `精英法宝` 计数。
  - Snapshot 新增 `DroppedArtifactCount`。
- 补充测试：
  - 引擎层覆盖普通战斗不掉落、原型精英胜利掉落法宝。
  - Presenter 覆盖掉落法宝计数文本。
  - 原型 UI 覆盖走到石魔首领，胜利后获得法宝并记录日志。

## 验收结果

已执行：

```bash
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

结果：

- `dotnet build` 成功，0 Error；保留项目既有 warning。
- `git diff --check` 通过；仅出现 Git 换行提示。
- Unity MCP `refresh_unity(scope=all, mode=if_dirty, compile=request, wait_for_ready=true)` 成功，期间发生一次可恢复断连，最终 Editor ready。
- Unity MCP `GameLogic.EditModeTests`：99/99 passed。
- Unity MCP `execute_code` 探针通过：
  - 返回 `PASS:Artifacts=1,Dropped=1,Status=Reward,Log=True,Artifact=rejuvenation_jade`。
  - 证明原型路线走到石魔首领后，精英战胜利会获得 1 件法宝并写入日志。
- Unity MCP `read_console(types=["error","warning"])`：0 条。

## 假设与风险

- 当前掉落池只包含已实现效果的 `灵石矿` 和 `回春玉佩`，尚未接入完整 15 件法宝。
- 本阶段只实现精英战掉落入口，宝箱节点和秘境事件法宝奖励尚未接入。
- 当前仍允许重复获得同名法宝；法宝唯一性策略留到正式法宝池阶段统一处理。
- 法宝掉落目前复用 Run 引擎的随机源，后续如果需要独立掉落权重或稀有度权重，应拆分专用随机/权重逻辑。

## 后续建议

- 下一阶段可继续接入宝箱节点，让宝箱成为纯法宝奖励节点。
- 也可以先接入 `储物袋` 或 `聚灵阵`，扩大法宝效果类型覆盖面。
