using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunAutoPlayReport
    {
        private readonly List<string> _events = new List<string>();

        public int StepsExecuted { get; private set; }

        public int BattlesResolved { get; private set; }

        public int CardsPlayed { get; private set; }

        public int TurnsEnded { get; private set; }

        public int RewardsChosen { get; private set; }

        public int RewardsSkipped { get; private set; }

        public int RouteChoicesMade { get; private set; }

        public int RestsTaken { get; private set; }

        public int ChestsOpened { get; private set; }

        public int MysticChoicesMade { get; private set; }

        public int MarketPurchases { get; private set; }

        public int MarketsLeft { get; private set; }

        public int GoldenCorePassivesChosen { get; private set; }

        public bool HitStepLimit { get; private set; }

        public CultivationRunStatus FinalStatus { get; private set; }

        public BattleOutcome FinalBattleOutcome { get; private set; }

        public CultivationRealm FinalRealm { get; private set; }

        public int FinalNodeIndex { get; private set; }

        public string FinalNodeName { get; private set; } = string.Empty;

        public int FinalPlayerHp { get; private set; }

        public int FinalPlayerMaxHp { get; private set; }

        public int FinalDeckCount { get; private set; }

        public int FinalSpiritStones { get; private set; }

        public int FinalArtifactCount { get; private set; }

        public IReadOnlyList<string> Events => _events;

        public bool Completed => FinalStatus == CultivationRunStatus.Completed;

        public bool Defeated => FinalStatus == CultivationRunStatus.Defeated;

        public bool ReachedGoldenCore => FinalRealm >= CultivationRealm.GoldenCore;

        public string Summary => BuildSummary();

        internal void Step(CultivationRunState state)
        {
            StepsExecuted++;
            CaptureFinalState(state);
        }

        internal void AddCardPlayed(CardDefinition card)
        {
            CardsPlayed++;
            AddEvent($"出牌：{card?.Name ?? "未知"}");
        }

        internal void AddTurnEnded()
        {
            TurnsEnded++;
            AddEvent("结束回合");
        }

        internal void AddBattleResolved(CultivationRunState state)
        {
            BattlesResolved++;
            AddEvent($"结算战斗：{state?.CurrentNode?.Name ?? "未知"}");
        }

        internal void AddRewardChosen(CultivationRunReward reward)
        {
            RewardsChosen++;
            AddEvent($"选择奖励：{reward?.Card?.Name ?? "未知"}");
        }

        internal void AddRewardSkipped()
        {
            RewardsSkipped++;
            AddEvent("跳过奖励");
        }

        internal void AddRouteChoice(CultivationRunRouteChoice choice)
        {
            RouteChoicesMade++;
            AddEvent($"选择路线：{choice?.TargetNode?.Name ?? "未知"}");
        }

        internal void AddRest(bool upgraded, CardDefinition upgradedCard)
        {
            RestsTaken++;
            AddEvent(upgraded ? $"闭关升级：{upgradedCard?.Name ?? "未知"}" : "闭关恢复");
        }

        internal void AddChestOpened()
        {
            ChestsOpened++;
            AddEvent("打开宝箱");
        }

        internal void AddMysticChoice(MysticEventOption option)
        {
            MysticChoicesMade++;
            AddEvent($"秘境选择：{option?.Name ?? "未知"}");
        }

        internal void AddMarketPurchase(CultivationMarketItem item)
        {
            MarketPurchases++;
            AddEvent($"坊市购买：{CultivationRunPrototypePresenter.FormatMarketItemName(item)}");
        }

        internal void AddMarketLeft()
        {
            MarketsLeft++;
            AddEvent("离开坊市");
        }

        internal void AddGoldenCorePassive(GoldenCorePassiveDefinition passive)
        {
            GoldenCorePassivesChosen++;
            AddEvent($"选择金丹被动：{passive?.Name ?? "未知"}");
        }

        internal void Finish(CultivationRunState state, bool hitStepLimit)
        {
            HitStepLimit = hitStepLimit;
            CaptureFinalState(state);
        }

        private void AddEvent(string message)
        {
            if (_events.Count >= 64)
            {
                return;
            }

            _events.Add(message);
        }

        private void CaptureFinalState(CultivationRunState state)
        {
            if (state == null)
            {
                return;
            }

            FinalStatus = state.Status;
            FinalBattleOutcome = state.CurrentBattle?.Outcome ?? BattleOutcome.InProgress;
            FinalRealm = state.CurrentRealm;
            FinalNodeIndex = state.CurrentNodeIndex;
            FinalNodeName = state.CurrentNode?.Name ?? string.Empty;
            FinalPlayerHp = state.CurrentBattle?.Player.CurrentHp ?? state.PlayerCurrentHp;
            FinalPlayerMaxHp = state.CurrentBattle?.Player.MaxHp ?? state.PlayerMaxHp;
            FinalDeckCount = state.Deck.Count;
            FinalSpiritStones = state.SpiritStones;
            FinalArtifactCount = state.Artifacts.Count;
        }

        private string BuildSummary()
        {
            var builder = new StringBuilder();
            builder.Append(Completed && !HitStepLimit ? "PASS" : "CHECK");
            builder.Append(" status=").Append(FinalStatus);
            builder.Append(" realm=").Append(FinalRealm);
            builder.Append(" node=").Append(FinalNodeIndex + 1).Append(":").Append(FinalNodeName);
            builder.Append(" hp=").Append(FinalPlayerHp).Append("/").Append(FinalPlayerMaxHp);
            builder.Append(" deck=").Append(FinalDeckCount);
            builder.Append(" artifacts=").Append(FinalArtifactCount);
            builder.Append(" stones=").Append(FinalSpiritStones);
            builder.Append(" battles=").Append(BattlesResolved);
            builder.Append(" cards=").Append(CardsPlayed);
            builder.Append(" steps=").Append(StepsExecuted);
            if (HitStepLimit)
            {
                builder.Append(" step_limit=true");
            }

            return builder.ToString();
        }
    }

    public sealed class CultivationRunAutoPlayer
    {
        public const int DefaultMaxSteps = 512;

        public CultivationRunAutoPlayReport Run(CultivationRunPrototypeUI ui, int maxSteps = DefaultMaxSteps)
        {
            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui));
            }

            var stepLimit = Math.Max(1, maxSteps);
            var report = new CultivationRunAutoPlayReport();
            for (var i = 0; i < stepLimit; i++)
            {
                var state = ui.DebugRunState;
                report.Step(state);
                if (state == null || state.Status == CultivationRunStatus.Completed || state.Status == CultivationRunStatus.Defeated)
                {
                    report.Finish(state, false);
                    return report;
                }

                switch (state.Status)
                {
                    case CultivationRunStatus.InBattle:
                        StepBattle(ui, state, report);
                        break;
                    case CultivationRunStatus.Reward:
                        StepReward(ui, state, report);
                        break;
                    case CultivationRunStatus.RouteChoice:
                        StepRouteChoice(ui, state, report);
                        break;
                    case CultivationRunStatus.Rest:
                        StepRest(ui, state, report);
                        break;
                    case CultivationRunStatus.Market:
                        StepMarket(ui, state, report);
                        break;
                    case CultivationRunStatus.Chest:
                        report.AddChestOpened();
                        ui.OpenChest();
                        break;
                    case CultivationRunStatus.Mystic:
                        StepMystic(ui, state, report);
                        break;
                    case CultivationRunStatus.GoldenCorePassiveChoice:
                        StepGoldenCorePassive(ui, state, report);
                        break;
                    default:
                        report.Finish(state, false);
                        return report;
                }
            }

            report.Finish(ui.DebugRunState, true);
            return report;
        }

        private static void StepBattle(CultivationRunPrototypeUI ui, CultivationRunState state, CultivationRunAutoPlayReport report)
        {
            var battle = state.CurrentBattle;
            if (battle == null)
            {
                report.Finish(state, false);
                return;
            }

            if (battle.Outcome != BattleOutcome.InProgress)
            {
                report.AddBattleResolved(state);
                ui.ResolveBattle();
                return;
            }

            var playable = battle.Hand
                .Select((card, index) => new { Card = card, Index = index, Score = ScoreCard(battle, card) })
                .Where(item => battle.Spirit >= battle.GetEffectiveSpiritCost(item.Card))
                .OrderByDescending(item => item.Score)
                .ThenBy(item => battle.GetEffectiveSpiritCost(item.Card))
                .FirstOrDefault(item => item.Score > 0);

            if (playable != null)
            {
                report.AddCardPlayed(playable.Card);
                ui.PlayCardAt(playable.Index);
                return;
            }

            report.AddTurnEnded();
            ui.EndTurn();
        }

        private static void StepReward(CultivationRunPrototypeUI ui, CultivationRunState state, CultivationRunAutoPlayReport report)
        {
            if (state.CurrentRewards.Count == 0)
            {
                report.AddRewardSkipped();
                ui.SkipReward();
                return;
            }

            var reward = state.CurrentRewards
                .Select((item, index) => new { Reward = item, Index = index, Score = ScoreReward(item.Card) })
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Index)
                .First();

            report.AddRewardChosen(reward.Reward);
            ui.ChooseReward(reward.Index);
        }

        private static void StepRouteChoice(CultivationRunPrototypeUI ui, CultivationRunState state, CultivationRunAutoPlayReport report)
        {
            if (state.CurrentRouteChoices.Count == 0)
            {
                return;
            }

            var choice = state.CurrentRouteChoices
                .Select((item, index) => new { Choice = item, Index = index, Score = ScoreRouteChoice(item) })
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Index)
                .First();

            report.AddRouteChoice(choice.Choice);
            ui.ChooseRoute(choice.Index);
        }

        private static void StepRest(CultivationRunPrototypeUI ui, CultivationRunState state, CultivationRunAutoPlayReport report)
        {
            var upgrade = state.RestUpgradeChoices
                .SelectMany(choice => choice.SourceCard.UpgradeOptions.Select((option, optionIndex) => new
                {
                    Choice = choice,
                    Option = option,
                    OptionIndex = optionIndex,
                    Score = ScoreReward(option.UpgradedCard) - ScoreReward(choice.SourceCard),
                }))
                .OrderByDescending(item => item.Score)
                .FirstOrDefault(item => item.Score > 0);

            if (upgrade != null)
            {
                report.AddRest(true, upgrade.Option.UpgradedCard);
                ui.RestAndUpgrade(upgrade.Choice.DeckIndex, upgrade.OptionIndex);
                return;
            }

            report.AddRest(false, null);
            ui.Rest();
        }

        private static void StepMarket(CultivationRunPrototypeUI ui, CultivationRunState state, CultivationRunAutoPlayReport report)
        {
            var purchase = state.CurrentMarketItems
                .Select((item, index) => new { Item = item, Index = index, Score = ScoreMarketItem(state, item) })
                .Where(item => item.Item.Price <= state.SpiritStones && item.Score > 0 && (!item.Item.IsPill || state.Pills.Count < state.PillSlotLimit))
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Item.Price)
                .FirstOrDefault();

            if (purchase != null && report.MarketPurchases < 2)
            {
                report.AddMarketPurchase(purchase.Item);
                ui.BuyMarketItem(purchase.Index);
                return;
            }

            report.AddMarketLeft();
            ui.LeaveMarket();
        }

        private static void StepMystic(CultivationRunPrototypeUI ui, CultivationRunState state, CultivationRunAutoPlayReport report)
        {
            if (state.MysticEventChoices.Count == 0)
            {
                return;
            }

            var option = state.MysticEventChoices
                .Select((item, index) => new { Option = item, Index = index, Score = ScoreMysticOption(state, item) })
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Index)
                .First();

            report.AddMysticChoice(option.Option);
            ui.ChooseMysticEventOption(option.Index);
        }

        private static void StepGoldenCorePassive(CultivationRunPrototypeUI ui, CultivationRunState state, CultivationRunAutoPlayReport report)
        {
            if (state.CurrentGoldenCorePassiveChoices.Count == 0)
            {
                return;
            }

            var passive = state.CurrentGoldenCorePassiveChoices
                .Select((item, index) => new { Passive = item, Index = index, Score = ScoreGoldenCorePassive(item) })
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Index)
                .First();

            report.AddGoldenCorePassive(passive.Passive);
            ui.ChooseGoldenCorePassive(passive.Index);
        }

        private static int ScoreCard(BattleState battle, CardDefinition card)
        {
            if (battle == null || card == null)
            {
                return 0;
            }

            var enemy = battle.Enemies.FirstOrDefault(item => !item.Body.IsDefeated);
            var incomingAttack = IsIncomingAttack(enemy?.CurrentIntent.Type);
            var missingHp = Math.Max(0, battle.Player.MaxHp - battle.Player.CurrentHp);
            var enemyPoisonStacks = enemy?.Body.PoisonStacks ?? 0;
            var score = card.SpiritCost == 0 ? 4 : 0;

            foreach (var effect in card.Effects)
            {
                score += ScoreEffect(effect, incomingAttack, missingHp, enemyPoisonStacks);
            }

            score -= battle.GetEffectiveSpiritCost(card) * 2;
            return score;
        }

        private static int ScoreReward(CardDefinition card)
        {
            if (card == null)
            {
                return 0;
            }

            var score = card.SpiritCost == 0 ? 4 : 0;
            foreach (var effect in card.Effects)
            {
                score += ScoreEffect(effect, true, 20, 8);
            }

            return score - card.SpiritCost;
        }

        private static int ScoreEffect(CardEffect effect, bool incomingAttack, int missingHp, int targetPoisonStacks)
        {
            switch (effect.Type)
            {
                case CardEffectType.Damage:
                    return effect.Value * Math.Max(1, effect.RepeatCount);
                case CardEffectType.ChanceDamage:
                    return AverageChanceValue(effect.Value, effect.FallbackValue, effect.ChancePercent) * Math.Max(1, effect.RepeatCount);
                case CardEffectType.ChanceDamageWithStun:
                case CardEffectType.ChanceDamageWithChain:
                    return AverageChanceValue(effect.Value, effect.FallbackValue, effect.ChancePercent) * Math.Max(1, effect.RepeatCount) + effect.SecondaryValue / 2;
                case CardEffectType.ChainOnChanceDamage:
                    return AverageChanceValue(effect.Value + effect.SecondaryValue, effect.FallbackValue, effect.ChancePercent);
                case CardEffectType.ChanceChainDamage:
                case CardEffectType.ChanceChainDamageWithStun:
                case CardEffectType.ChanceChainDamageRepeatTarget:
                    return effect.Value + effect.SecondaryValue * Math.Max(1, effect.ChancePercent) / 100;
                case CardEffectType.DamageAfterCriticalTriggered:
                case CardEffectType.DamageAfterCriticalTriggeredWithStun:
                case CardEffectType.DamageAfterCriticalTriggeredChainAll:
                    return effect.Value + effect.FallbackValue / 2;
                case CardEffectType.DamagePerSwordMark:
                    return effect.Value * 2;
                case CardEffectType.Shield:
                    return incomingAttack ? effect.Value * 2 : effect.Value;
                case CardEffectType.Dodge:
                    return incomingAttack ? effect.Value * 14 : effect.Value * 5;
                case CardEffectType.DodgeCounter:
                case CardEffectType.AttackCounter:
                    return incomingAttack ? effect.Value * 2 : effect.Value;
                case CardEffectType.Draw:
                    return effect.Value * 5;
                case CardEffectType.Heal:
                    return Math.Min(effect.Value, missingHp) * 2;
                case CardEffectType.BreakDefense:
                    return effect.Value * 8;
                case CardEffectType.Burn:
                    return effect.Value * Math.Max(1, effect.Duration);
                case CardEffectType.Poison:
                    return effect.Value * 5;
                case CardEffectType.PoisonBurst:
                    return targetPoisonStacks > 0 ? targetPoisonStacks * effect.Value + effect.SecondaryValue * 4 : 0;
                case CardEffectType.Leech:
                    return effect.Value + Math.Min(missingHp, Math.Max(1, effect.Value * Math.Max(0, effect.SecondaryValue) / 100)) * 2;
                case CardEffectType.Regeneration:
                    return Math.Min(missingHp + 8, effect.Value * Math.Max(1, effect.Duration)) * 2;
                case CardEffectType.PoisonAttackCounter:
                    return incomingAttack ? effect.Value * Math.Max(1, effect.Duration) * 5 : effect.Value * 2;
                case CardEffectType.SwordMark:
                    return effect.Value * 7;
                case CardEffectType.Sharpness:
                    return effect.Value * Math.Max(1, effect.Duration) * 2;
                case CardEffectType.Exhaust:
                    return 1;
                case CardEffectType.Stun:
                case CardEffectType.ChanceStun:
                case CardEffectType.ChainOnChanceStun:
                    return effect.ChancePercent * Math.Max(1, effect.Duration) / 5 + effect.Value;
                case CardEffectType.ChargeDamage:
                    return effect.Value * 8;
                default:
                    return 0;
            }
        }

        private static int ScoreRouteChoice(CultivationRunRouteChoice choice)
        {
            if (choice?.TargetNode == null)
            {
                return 0;
            }

            switch (choice.TargetNode.Type)
            {
                case CultivationRunNodeType.Rest:
                    return 100;
                case CultivationRunNodeType.Chest:
                    return 90;
                case CultivationRunNodeType.Mystic:
                    return 80;
                case CultivationRunNodeType.Market:
                    return 70;
                case CultivationRunNodeType.Battle:
                    return 55 + (int)choice.TargetNode.Realm * 5;
                case CultivationRunNodeType.Elite:
                    return 45 + (int)choice.TargetNode.Realm * 5;
                default:
                    return 0;
            }
        }

        private static int ScoreMarketItem(CultivationRunState state, CultivationMarketItem item)
        {
            if (item == null)
            {
                return 0;
            }

            if (item.IsArtifact)
            {
                return 100 - item.Price;
            }

            if (item.IsPill)
            {
                return state.Pills.Count < state.PillSlotLimit ? 70 - item.Price / 2 : 0;
            }

            return ScoreReward(item.Card) - item.Price / 2;
        }

        private static int ScoreMysticOption(CultivationRunState state, MysticEventOption option)
        {
            if (option == null)
            {
                return 0;
            }

            switch (option.EffectType)
            {
                case MysticEventEffectType.GainArtifact:
                    return 100;
                case MysticEventEffectType.Heal:
                    return Math.Max(0, state.PlayerMaxHp - state.PlayerCurrentHp) + 50;
                case MysticEventEffectType.GainPill:
                    return state.Pills.Count < state.PillSlotLimit ? 80 : 10;
                case MysticEventEffectType.GainCard:
                    return 70 + ScoreReward(option.CardReward);
                case MysticEventEffectType.GainSpiritStones:
                    return 60 + option.EffectValue;
                case MysticEventEffectType.Leave:
                    return 1;
                default:
                    return 0;
            }
        }

        private static int ScoreGoldenCorePassive(GoldenCorePassiveDefinition passive)
        {
            if (passive == null)
            {
                return 0;
            }

            switch (passive.Type)
            {
                case GoldenCorePassiveType.ThunderSeed:
                    return 100;
                case GoldenCorePassiveType.FlowingWater:
                    return 90;
                case GoldenCorePassiveType.SwordHeart:
                    return 80;
                default:
                    return 0;
            }
        }

        private static bool IsIncomingAttack(EnemyIntentType? intentType)
        {
            switch (intentType)
            {
                case EnemyIntentType.Attack:
                case EnemyIntentType.Sweep:
                case EnemyIntentType.AttackAndBurn:
                case EnemyIntentType.AttackAndFreeze:
                case EnemyIntentType.AttackAndStun:
                    return true;
                default:
                    return false;
            }
        }

        private static int AverageChanceValue(int successValue, int fallbackValue, int chancePercent)
        {
            return (successValue * chancePercent + fallbackValue * (100 - chancePercent)) / 100;
        }
    }
}
