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
                default:
                    return effect.Type.ToString();
            }
        }

        private static string BuildRunText(CultivationRunState state)
        {
            return $"状态：{state.Status}\n节点：{state.CurrentNodeIndex + 1}/{state.Route.Count}\nHP：{state.PlayerCurrentHp}/{state.PlayerMaxHp}\n牌组：{state.Deck.Count} 张\n已拿奖励：{state.ClaimedRewards.Count}";
        }

        private static string BuildNodeText(CultivationRunState state)
        {
            var builder = new StringBuilder();
            builder.Append("当前节点：").Append(state.CurrentNode.Name).AppendLine();
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

            return builder.ToString();
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
            return $"玩家\nHP {battle.Player.CurrentHp}/{battle.Player.MaxHp}  护盾 {battle.Player.Shield}\n灵力 {battle.Spirit}/{battle.SpiritMax}  回合 {battle.TurnNumber}\n锋锐 {battle.Player.Sharpness}/{battle.Player.SharpnessTurns}\n\n敌人：{enemy?.Body.Name ?? string.Empty}\nHP {enemy?.Body.CurrentHp ?? 0}/{enemy?.Body.MaxHp ?? 0}  护盾 {enemy?.Body.Shield ?? 0}\n破防 {enemy?.Body.BreakDefenseStacks ?? 0}  灼烧 {enemy?.Body.BurnStacks ?? 0}/{enemy?.Body.BurnTurns ?? 0}  剑气印记 {enemy?.Body.SwordMarkStacks ?? 0}\n意图：{enemy?.CurrentIntent.Description ?? string.Empty}\n\n战斗结果：{battle.Outcome}";
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

        public int DeckCount { get; private set; }

        public int HandCount { get; private set; }

        public int RewardCount { get; private set; }

        public int RestUpgradeChoiceCount { get; private set; }

        public int RouteChoiceCount { get; private set; }

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
                PlayerHp = state.PlayerCurrentHp,
                PlayerMaxHp = state.PlayerMaxHp,
                DeckCount = state.Deck.Count,
                HandCount = state.CurrentBattle?.Hand.Count ?? 0,
                RewardCount = state.CurrentRewards.Count,
                RestUpgradeChoiceCount = state.RestUpgradeChoices.Count,
                RouteChoiceCount = state.CurrentRouteChoices.Count,
                BattleOutcome = state.CurrentBattle?.Outcome ?? BattleOutcome.InProgress,
            };
        }
    }
}
