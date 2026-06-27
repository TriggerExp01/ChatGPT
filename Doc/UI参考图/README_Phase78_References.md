# Phase78 参考图包

将本压缩包解压到仓库根目录后，会得到：

- `Doc/UI参考图/Phase78_Targets/`
- `Doc/UI参考图/Phase78_Failed/`

## Target 图

- `Target_MainRoute.png`：Steam 风格主界面 / 秘境路线目标参考。
- `Target_EventPopup.png`：秘境事件弹窗目标参考。
- `Target_Battle.png`：战斗界面目标参考。
- `Target_Reward.png`：奖励 / 宝箱 / 战斗结算目标参考。
- `Target_CardLibrary.png`：功法 / 卡牌详情界面目标参考。
- `Target_Composite_All5.png`：5 张界面的总览参考，只作辅助，不作为单屏复刻目标。

## Failed 图

- `Failed_MainRoute.png`
- `Failed_EventPopup.png`
- `Failed_Battle.png`
- `Failed_Reward.png`
- `Failed_CardLibrary.png`

这些是 Phase78 失败样板，用于 Codex 对照哪些布局、比例和美术语言不应继续沿用。

## 使用规则

Phase78B 先只复刻 `Target_MainRoute.png`，输出：

- `UnityProject/Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_MainRoute_Phase78B.prefab`
- `Doc/UI截图验收/Steam_MainRoute_Concept_Phase78B.png`
- `Doc/交付说明/Phase78B_MainRoute_参考图复刻返工.md`

不要同时返工 5 张，避免继续批量生成失败占位 UI。
