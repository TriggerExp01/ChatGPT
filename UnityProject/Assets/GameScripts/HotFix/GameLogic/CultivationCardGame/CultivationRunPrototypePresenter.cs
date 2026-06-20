using System;
using System.Linq;
using System.Text;

namespace GameLogic.Cultivation
{
    public static class CultivationRunPrototypePresenter
    {
        public const int DefaultMaxLogLines = 8;

        public static RunPrototypeSnapshot CreateSnapshot(CultivationRunState state)
        {
            return RunPrototypeSnapshot.From(state);
        }

        public static RunPrototypeText BuildText(CultivationRunState state, int maxLogLines = DefaultMaxLogLines)
        {
            if (state == null)
            {
                return RunPrototypeText.Empty;
            }

            return new RunPrototypeText(
                BuildRunText(state),
                BuildNodeText(state),
                BuildBattleText(state),
                BuildDeckText(state),
                BuildLogText(state, maxLogLines));
        }

        public static string FormatCardSummary(CardDefinition card)
        {
            if (card == null)
            {
                return string.Empty;
            }

            return $"灵力 {card.SpiritCost}\n{string.Join("\n", card.Effects.Select(FormatEffect))}";
        }

        public static string FormatEffect(CardEffect effect)
        {
            switch (effect.Type)
            {
                case CardEffectType.Damage:
                    return effect.RepeatCount > 1
                        ? $"造成 {effect.Value} 伤害 × {effect.RepeatCount}"
                        : $"造成 {effect.Value} 伤害";
                case CardEffectType.Shield:
                    return $"获得 {effect.Value} 护盾";
                case CardEffectType.Draw:
                    return $"抽 {effect.Value} 张牌";
                case CardEffectType.Heal:
                    return $"恢复 {effect.Value} HP";
                case CardEffectType.BreakDefense:
                    return $"破防 {effect.Value}";
                case CardEffectType.Burn:
                    return $"灼烧 {effect.Value} / {effect.Duration} 回合";
                case CardEffectType.SwordMark:
                    return $"剑气印记 {effect.Value}";
                case CardEffectType.Sharpness:
                    return $"锋锐 {effect.Value} / {effect.Duration} 回合";
                case CardEffectType.Exhaust:
                    return "消耗";
                case CardEffectType.DamagePerSwordMark:
                    return $"每层剑气印记 +{effect.Value} 伤害";
                default:
                    return effect.Type.ToString();
            }
        }

        private static string BuildRunText(CultivationRunState state)
        {
            var playerHp = state.CurrentBattle?.Player.CurrentHp ?? state.PlayerCurrentHp;
            var playerMaxHp = state.CurrentBattle?.Player.MaxHp ?? state.PlayerMaxHp;
            return $"状态：{state.Status}\n境界：{FormatRealmName(state.CurrentRealm)}\n节点：{state.CurrentNodeIndex + 1}/{state.Route.Count}\nHP：{playerHp}/{playerMaxHp}\n灵力上限：{state.SpiritMax}\n手牌上限：{state.HandLimit}\n灵石：{state.SpiritStones}\n牌组：{state.Deck.Count} 张\n丹药：{state.Pills.Count}/{state.PillSlotLimit}\n法宝：{state.Artifacts.Count}\n精英法宝：{state.DroppedArtifacts.Count}\n宝箱法宝：{state.ChestArtifacts.Count}\n已拿奖励：{state.ClaimedRewards.Count}\n已突破：{state.RealmBreakthroughCount}\n坊市删牌：{state.RemovedMarketCards.Count}\n坊市售牌：{state.SoldMarketCards.Count}";
        }

        private static string BuildNodeText(CultivationRunState state)
        {
            var builder = new StringBuilder();
            builder.Append("当前节点：").Append(state.CurrentNode.Name).AppendLine();
            builder.Append("境界层：").Append(FormatRealmName(state.CurrentNode.Realm)).AppendLine();
            builder.Append("类型：").Append(state.CurrentNode.Type).AppendLine();
            if (state.CurrentNode.Enemy != null)
            {
                builder.Append("敌人：").Append(state.CurrentNode.Enemy.Name).AppendLine();
            }

            if (state.Status == CultivationRunStatus.RouteChoice)
            {
                builder.AppendLine();
                builder.AppendLine("可选路线：");
                for (var i = 0; i < state.CurrentRouteChoices.Count; i++)
                {
                    var choice = state.CurrentRouteChoices[i];
                    builder.Append(i + 1).Append(". ").Append(choice.TargetNode.Name).Append(" / ").Append(choice.TargetNode.Type).AppendLine();
                }
            }

            if (state.Status == CultivationRunStatus.Market)
            {
                builder.AppendLine();
                builder.AppendLine("坊市商品：");
                for (var i = 0; i < state.CurrentMarketItems.Count; i++)
                {
                    var item = state.CurrentMarketItems[i];
                    builder.Append(i + 1).Append(". ").Append(FormatMarketItemName(item)).Append(" / ").Append(item.Price).Append(" 灵石").AppendLine();
                }

                builder.Append("移除卡牌：").Append(CultivationRunEngine.MarketCardRemovalCost).Append(" 灵石 / 已移除 ").Append(state.RemovedMarketCards.Count).Append(" 张").AppendLine();
                builder.Append("升级卡牌：").Append(CultivationRunEngine.MarketCardUpgradeCost).Append(" 灵石 / 已升级 ").Append(state.MarketUpgradedCards.Count).Append(" 张").AppendLine();
                builder.Append("出售卡牌：半价回收 / 已出售 ").Append(state.SoldMarketCards.Count).Append(" 张").AppendLine();
            }

            if (state.Status == CultivationRunStatus.Chest)
            {
                builder.AppendLine();
                builder.Append("宝箱法宝池：").Append(state.CurrentNode.ArtifactRewardPool.Count).Append(" 件").AppendLine();
            }

            if (state.Status == CultivationRunStatus.Mystic)
            {
                builder.AppendLine();
                if (state.CurrentNode.MysticEvent == null)
                {
                    builder.AppendLine("Mystic event: none");
                }
                else
                {
                    builder.Append("Mystic event: ").Append(state.CurrentNode.MysticEvent.Name).AppendLine();
                    builder.AppendLine(state.CurrentNode.MysticEvent.Description);
                    for (var i = 0; i < state.MysticEventChoices.Count; i++)
                    {
                        var option = state.MysticEventChoices[i];
                        builder.Append(i + 1).Append(". ").Append(option.Name).Append(" / ").Append(option.Description).AppendLine();
                    }
                }
            }

            return builder.ToString();
        }

        public static string FormatMarketItemName(CultivationMarketItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (item.IsCard)
            {
                return item.Card.Name;
            }

            if (item.IsPill)
            {
                return $"{item.Pill.Name}（丹药）";
            }

            return $"{item.Artifact.Name}（法宝）";
        }

        public static string FormatRealmName(CultivationRealm realm)
        {
            switch (realm)
            {
                case CultivationRealm.QiRefining:
                    return "炼气";
                case CultivationRealm.Foundation:
                    return "筑基";
                case CultivationRealm.GoldenCore:
                    return "金丹";
                case CultivationRealm.NascentSoul:
                    return "元婴";
                case CultivationRealm.SoulTransformation:
                    return "化神";
                case CultivationRealm.Tribulation:
                    return "渡劫";
                default:
                    return realm.ToString();
            }
        }

        private static string BuildBattleText(CultivationRunState state)
        {
            if (state.Status == CultivationRunStatus.Completed)
            {
                return "本轮修行完成。";
            }

            if (state.Status == CultivationRunStatus.Defeated)
            {
                return "本轮修行失败。";
            }

            if (state.Status != CultivationRunStatus.InBattle || state.CurrentBattle == null)
            {
                return $"等待操作：{state.Status}";
            }

            var battle = state.CurrentBattle;
            var enemy = battle.Enemies.FirstOrDefault();
            return $"玩家\nHP {battle.Player.CurrentHp}/{battle.Player.MaxHp}  护盾 {battle.Player.Shield}\n灵力 {battle.Spirit}/{battle.SpiritMax}  回合 {battle.TurnNumber}\n锋锐 {battle.Player.Sharpness}/{battle.Player.SharpnessTurns}  破防 {battle.Player.BreakDefenseStacks}  灼烧 {battle.Player.BurnStacks}/{battle.Player.BurnTurns}  冰冻 {battle.Player.FreezeStacks}/{battle.Player.FreezeTurns}  灵力消耗 -{battle.SpiritCostReduction}\n\n敌人：{enemy?.Body.Name ?? string.Empty}\nHP {enemy?.Body.CurrentHp ?? 0}/{enemy?.Body.MaxHp ?? 0}  护盾 {enemy?.Body.Shield ?? 0}\n破防 {enemy?.Body.BreakDefenseStacks ?? 0}  灼烧 {enemy?.Body.BurnStacks ?? 0}/{enemy?.Body.BurnTurns ?? 0}  冰冻 {enemy?.Body.FreezeStacks ?? 0}/{enemy?.Body.FreezeTurns ?? 0}  剑气印记 {enemy?.Body.SwordMarkStacks ?? 0}\n意图：{enemy?.CurrentIntent.Description ?? string.Empty}\n\n战斗结果：{battle.Outcome}";
        }

        private static string BuildDeckText(CultivationRunState state)
        {
            var builder = new StringBuilder();
            builder.AppendLine("当前牌组：");
            for (var i = 0; i < state.Deck.Count; i++)
            {
                var card = state.Deck[i];
                builder.Append(i + 1).Append(". ").Append(card.Name).Append("  灵力 ").Append(card.SpiritCost);
                if (card.CanUpgrade)
                {
                    builder.Append("  可升级");
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildLogText(CultivationRunState state, int maxLogLines)
        {
            if (state.CurrentBattle == null || state.CurrentBattle.Logs.Count == 0)
            {
                return "日志：等待行动...";
            }

            var lineCount = Math.Max(1, maxLogLines);
            var logs = state.CurrentBattle.Logs.Skip(Math.Max(0, state.CurrentBattle.Logs.Count - lineCount)).ToArray();
            var builder = new StringBuilder();
            builder.AppendLine("战斗日志：");
            foreach (var log in logs)
            {
                builder.Append("- ").Append(log.Message).AppendLine();
            }

            return builder.ToString();
        }
    }

    public sealed class RunPrototypeText
    {
        public static RunPrototypeText Empty { get; } = new RunPrototypeText(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        public RunPrototypeText(string runText, string nodeText, string battleText, string deckText, string logText)
        {
            RunText = runText ?? string.Empty;
            NodeText = nodeText ?? string.Empty;
            BattleText = battleText ?? string.Empty;
            DeckText = deckText ?? string.Empty;
            LogText = logText ?? string.Empty;
        }

        public string RunText { get; }

        public string NodeText { get; }

        public string BattleText { get; }

        public string DeckText { get; }

        public string LogText { get; }
    }

    public sealed class RunPrototypeSnapshot
    {
        public CultivationRunStatus Status { get; private set; }

        public int CurrentNodeIndex { get; private set; }

        public string CurrentNodeName { get; private set; }

        public int PlayerHp { get; private set; }

        public int PlayerMaxHp { get; private set; }

        public int SpiritStones { get; private set; }

        public CultivationRealm CurrentRealm { get; private set; }

        public int RealmBreakthroughCount { get; private set; }

        public int SpiritMax { get; private set; }

        public int HandLimit { get; private set; }

        public int DeckCount { get; private set; }

        public int PillCount { get; private set; }

        public int PillSlotLimit { get; private set; }

        public int PurchasedMarketPillCount { get; private set; }

        public int ArtifactCount { get; private set; }

        public int PurchasedMarketArtifactCount { get; private set; }

        public int DroppedArtifactCount { get; private set; }

        public int ChestArtifactCount { get; private set; }

        public int MysticEventChoiceCount { get; private set; }

        public int ResolvedMysticEventCount { get; private set; }

        public int HandCount { get; private set; }

        public int RewardCount { get; private set; }

        public int RestUpgradeChoiceCount { get; private set; }

        public int RouteChoiceCount { get; private set; }

        public int MarketItemCount { get; private set; }

        public int PurchasedMarketItemCount { get; private set; }

        public int RemovedMarketCardCount { get; private set; }

        public int MarketUpgradedCardCount { get; private set; }

        public int SoldMarketCardCount { get; private set; }

        public BattleOutcome BattleOutcome { get; private set; }

        public static RunPrototypeSnapshot From(CultivationRunState state)
        {
            if (state == null)
            {
                return new RunPrototypeSnapshot();
            }

            return new RunPrototypeSnapshot
            {
                Status = state.Status,
                CurrentNodeIndex = state.CurrentNodeIndex,
                CurrentNodeName = state.CurrentNode.Name,
                PlayerHp = state.CurrentBattle?.Player.CurrentHp ?? state.PlayerCurrentHp,
                PlayerMaxHp = state.CurrentBattle?.Player.MaxHp ?? state.PlayerMaxHp,
                SpiritStones = state.SpiritStones,
                CurrentRealm = state.CurrentRealm,
                RealmBreakthroughCount = state.RealmBreakthroughCount,
                SpiritMax = state.SpiritMax,
                HandLimit = state.HandLimit,
                DeckCount = state.Deck.Count,
                PillCount = state.Pills.Count,
                PillSlotLimit = state.PillSlotLimit,
                PurchasedMarketPillCount = state.PurchasedMarketPills.Count,
                ArtifactCount = state.Artifacts.Count,
                PurchasedMarketArtifactCount = state.PurchasedMarketArtifacts.Count,
                DroppedArtifactCount = state.DroppedArtifacts.Count,
                ChestArtifactCount = state.ChestArtifacts.Count,
                MysticEventChoiceCount = state.MysticEventChoices.Count,
                ResolvedMysticEventCount = state.ResolvedMysticEventOptions.Count,
                HandCount = state.CurrentBattle?.Hand.Count ?? 0,
                RewardCount = state.CurrentRewards.Count,
                RestUpgradeChoiceCount = state.RestUpgradeChoices.Count,
                RouteChoiceCount = state.CurrentRouteChoices.Count,
                MarketItemCount = state.CurrentMarketItems.Count,
                PurchasedMarketItemCount = state.PurchasedMarketItems.Count,
                RemovedMarketCardCount = state.RemovedMarketCards.Count,
                MarketUpgradedCardCount = state.MarketUpgradedCards.Count,
                SoldMarketCardCount = state.SoldMarketCards.Count,
                BattleOutcome = state.CurrentBattle?.Outcome ?? BattleOutcome.InProgress,
            };
        }
    }
}
