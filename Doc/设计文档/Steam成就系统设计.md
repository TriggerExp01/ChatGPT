# Steam 成就系统设计 · 《仙途·天命》

> **项目适配说明**：本文档由 `C:/Users/hirusumi/Downloads/仙途天命_完整设计文档.md` 拆分并适配到当前 Unity + TEngine 项目。实现以 `D:/Work/Unity_Project/TEngine_Game` 仓库为准；设计文档统一存放在 `Doc/设计文档`。
> **拆分日期**：2026-06-20


> **设计原则**：成就应该引导玩家探索游戏深度，而非要求无意义的重复劳动。每个成就都应该让玩家说"这个有意思"而非"又要肝"。

---

## 一、成就设计原则

| 原则 | 说明 |
|------|------|
| **分层递进** | 从"自然获得"到"极限挑战"，覆盖所有玩家层级 |
| **引导探索** | 鼓励玩家尝试不同门派、不同构筑、不同策略 |
| **不肝** | 不设"打1000场""玩100小时"等纯时间成就 |
| **有趣味** | 部分成就是"彩蛋"，让玩家会心一笑 |
| **可见进度** | 长期成就显示当前进度(如"已解锁3/6门派") |

---

## 二、成就完整列表

### 2.1 修行入门（自然获得，100%玩家会获得）

| 成就名 | 图标 | 解锁条件 | 预估获得率 |
|--------|------|----------|-----------|
| **初入仙途** | 🌱 | 完成第一次Run（无论胜负） | 95% |
| **筑基成功** | 🏗️ | 第一次击败炼气期Boss，进入筑基期 | 80% |
| **结成金丹** | 💊 | 第一次击败筑基期Boss，进入金丹期 | 60% |
| **元婴出窍** | 👶 | 第一次击败金丹期Boss，进入元婴期 | 40% |
| **化神通天** | ✨ | 第一次击败元婴期Boss，进入化神期 | 25% |

### 2.2 飞升成就（核心挑战）

| 成就名 | 图标 | 解锁条件 | 预估获得率 |
|--------|------|----------|-----------|
| **渡劫飞升** | ⚡ | 第一次通关Run（击败最终天劫） | 15% |
| **各门派飞升** | — | 用每个门派各通关1次（6个子成就） | 5-10% |
| **道心坚定** | 🧘 | 在道心难度5+通关 | 8% |
| **道心通明** | 🧘‍♂️ | 在道心难度10通关 | 3% |
| **道心圆满** | 🧘‍♀️ | 在道心难度20通关 | 0.5% |

### 2.3 门派探索（鼓励多尝试）

| 成就名 | 图标 | 解锁条件 | 预估获得率 |
|--------|------|----------|-----------|
| **剑修** | ⚔️ | 用剑宗通关1次 | 10% |
| **火修** | 🔥 | 用火云宗通关1次 | 8% |
| **药修** | 🌿 | 用药王谷通关1次 | 5% |
| **雷修** | ⚡ | 用天雷阁通关1次 | 6% |
| **土修** | 🛡️ | 用玄黄宗通关1次 | 5% |
| **魔修** | 💀 | 用魔道通关1次 | 4% |
| **六道轮回** | 🔄 | 用全部6个门派各通关1次 | 2% |

### 2.4 构筑大师（鼓励不同策略）

| 成就名 | 图标 | 解锁条件 | 预估获得率 |
|--------|------|----------|-----------|
| **一剑破万法** | ⚔️ | 单回合造成100+伤害 | 20% |
| **烈火焚天** | 🔥 | 单场战斗施加累计30层灼烧 | 15% |
| **万毒噬心** | ☠️ | 单场战斗施加累计50层中毒 | 10% |
| **雷帝降世** | ⚡ | 单回合触发5次暴击 | 12% |
| **不破之盾** | 🛡️ | 单回合获得50+护盾 | 18% |
| **血魔之道** | 💉 | 在1HP状态下赢得一场战斗 | 8% |
| **精简之道** | 📉 | 以10张或更少的牌库通关 | 5% |
| **膨胀之道** | 📈 | 以30张或更多的牌库通关 | 10% |
| **跳过大师** | 🚫 | 单次Run跳过10次卡牌奖励 | 15% |

### 2.5 战斗挑战

| 成就名 | 图标 | 解锁条件 | 预估获得率 |
|--------|------|----------|-----------|
| **无伤** | 🎯 | 一场战斗中未受到任何伤害 | 25% |
| **速战速决** | ⏱️ | 3回合内击败一个精英 | 15% |
| **以一敌众** | 💪 | 在1回合内击杀3个敌人 | 12% |
| **逆转** | 🔄 | HP低于10时赢得战斗 | 10% |
| **完美渡劫** | ✨ | Boss战不使用丹药且HP>50%获胜 | 5% |

### 2.6 探索与收集

| 成就名 | 图标 | 解锁条件 | 预估获得率 |
|--------|------|----------|-----------|
| **道基初成** | 🌳 | 解锁道基树第1层全部节点 | 40% |
| **道基大成** | 🌲 | 解锁道基树第3层全部节点 | 15% |
| **道基圆满** | 🎄 | 解锁道基树全部节点 | 3% |
| **收藏家** | 📚 | 收集全部凡品卡牌 | 30% |
| **鉴赏家** | 📖 | 收集全部灵品卡牌 | 15% |
| **集大成者** | 📕 | 收集全部卡牌 | 1% |
| **法宝猎人** | 💎 | 收集15件不同法宝 | 20% |
| **秘境探索者** | 🌀 | 遭遇全部12种秘境事件 | 10% |

### 2.7 特殊/彩蛋

| 成就名 | 图标 | 解锁条件 | 预估获得率 |
|--------|------|----------|-----------|
| **镜像对决** | 🪞 | 在镜像道人Boss战中，被自己的牌击败 | 3% |
| **赌徒** | 🎲 | 天雷阁一回合内全部概率判定成功 | 5% |
| **不死之身** | ♾️ | 一场战斗中触发3次"免死"效果 | 4% |
| **天雷淬体** | ⚡ | 在九天神雷Boss战中，让Boss被自己的神雷击杀 | 2% |
| **道心拷问** | ❓ | 在道心拷问Boss战中受到0伤害 | 1% |
| **每日修行者** | 📅 | 连续7天完成每日修行 | 15% |
| **种子分享** | 🌱 | 使用好友分享的种子完成Run | 8% |

---

## 三、成就统计

| 分类 | 数量 | 预估平均获得率 |
|------|------|---------------|
| 修行入门 | 5 | 61% |
| 飞升成就 | 10 | 5% |
| 门派探索 | 7 | 7% |
| 构筑大师 | 9 | 13% |
| 战斗挑战 | 5 | 13% |
| 探索与收集 | 8 | 17% |
| 特殊/彩蛋 | 7 | 5% |
| **总计** | **51** | — |

**Steam成就稀有度目标**：
- 常见(>50%): 3个
- 普通(20-50%): 12个
- 稀有(5-20%): 20个
- 珍贵(1-5%): 12个
- 极珍(<1%): 4个

---

## 四、成就实现技术要点

### 4.1 成就触发架构

```csharp
// 文件: GameScripts/HotFix/GameLogic/Achievement/AchievementSystem.cs

public class AchievementSystem
{
    // 成就检测在关键节点触发，而非每帧检测
    // 使用TEngine GameEvent订阅游戏事件

    public void Init()
    {
        GameEvent.AddEventListener(GameEvents.CombatEnd, OnCombatEnd);
        GameEvent.AddEventListener(GameEvents.RunEnd, OnRunEnd);
        GameEvent.AddEventListener(GameEvents.RealmBreakthrough, OnBreakthrough);
        GameEvent.AddEventListener(GameEvents.CardAdded, OnCardAdded);
        GameEvent.AddEventListener(GameEvents.DaoTreeUnlocked, OnDaoTreeUnlock);
    }

    void OnCombatEnd(IEventArgs args)
    {
        var data = args as CombatEndArgs;

        // 检查"无伤"成就
        if (data.damageTakenThisCombat == 0)
            UnlockAchievement("ACH_NO_DAMAGE");

        // 检查"逆转"成就
        if (data.victory && data.playerHP < 10)
            UnlockAchievement("ACH_REVERSAL");

        // 检查"一剑破万法"成就
        if (data.maxSingleTurnDamage >= 100)
            UnlockAchievement("ACH_100_DAMAGE");
    }

    void UnlockAchievement(string achievementId)
    {
        if (IsAlreadyUnlocked(achievementId)) return;

        // 标记解锁
        SaveUnlocked(achievementId);

        // 调用Steam API
        SteamUserStats.SetAchievement(achievementId);
        SteamUserStats.StoreStats();

        // 游戏内通知
        GameEvent.Send(GameEvents.AchievementUnlocked, new AchievementArgs(achievementId));
    }
}
```

### 4.2 Steam成就ID命名规范

```
格式: ACH_<分类>_<名称>
示例:
  ACH_PROGRESSION_FIRST_RUN      // 初入仙途
  ACH_PROGRESSION_BREAKTHROUGH_1 // 筑基成功
  ACH_VICTORY_FIRST_CLEAR        // 渡劫飞升
  ACH_SECT_SWORD_CLEAR           // 剑修
  ACH_BUILD_100_DAMAGE           // 一剑破万法
  ACH_COMBAT_NO_DAMAGE           // 无伤
  ACH_SPECIAL_MIRROR_DEATH       // 镜像对决
```

### 4.3 成就存档

```csharp
[System.Serializable]
public class AchievementSaveData
{
    public List<string> unlockedAchievements;  // 已解锁成就ID
    public Dictionary<string, int> progress;   // 进度型成就的当前进度
}
```

成就数据同时存在本地存档和Steam云端——即使换电脑也不丢失。


---
