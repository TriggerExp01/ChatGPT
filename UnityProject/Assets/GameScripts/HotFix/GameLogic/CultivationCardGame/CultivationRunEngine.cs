using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunEngine
    {
        private const int RewardChoiceCount = 3;
        public const int MarketCardRemovalCost = 35;
        public const int MarketCardUpgradeCost = 50;

        private readonly BattleEngine _battleEngine;
        private readonly Random _rewardRandom;

        public CultivationRunEngine(BattleEngine battleEngine = null, int rewardSeed = 0)
        {
            _battleEngine = battleEngine ?? new BattleEngine(20260620);
            _rewardRandom = new Random(rewardSeed);
        }

        public CultivationRunState StartRun(IEnumerable<CardDefinition> deck = null, IEnumerable<CultivationRunNode> route = null, int playerMaxHp = 100, int? playerCurrentHp = null, int initialSpiritStones = 0, CultivationSect sect = CultivationSect.Sword)
        {
            var state = new CultivationRunState(
                deck ?? CultivationSeedData.CreateStarterDeck(sect),
                route ?? CultivationSeedData.CreateFirstPrototypeRoute(sect),
                playerMaxHp,
                playerCurrentHp,
                initialSpiritStones,
                sect);

            EnterCurrentNode(state);
            return state;
        }

        public void ResolveBattleResult(CultivationRunState state)
        {
            EnsureState(state);

            if (state.Status != CultivationRunStatus.InBattle)
            {
                throw new InvalidOperationException("Run is not waiting for a battle result.");
            }

            if (state.CurrentBattle == null)
            {
                throw new InvalidOperationException("Run does not have an active battle.");
            }

            switch (state.CurrentBattle.Outcome)
            {
                case BattleOutcome.Victory:
                    state.PlayerCurrentHp = state.CurrentBattle.Player.CurrentHp;
                    var artifactHeal = GetHealAfterVictory(state);
                    if (artifactHeal > 0)
                    {
                        var hpBeforeHeal = state.CurrentBattle.Player.CurrentHp;
                        state.CurrentBattle.Player.Heal(artifactHeal);
                        var healed = state.CurrentBattle.Player.CurrentHp - hpBeforeHeal;
                        state.PlayerCurrentHp = state.CurrentBattle.Player.CurrentHp;
                        if (healed > 0)
                        {
                            state.CurrentBattle.Logs.Add(new BattleLogEntry($"法宝恢复 {healed} HP。"));
                        }
                    }

                    var baseSpiritStoneReward = state.CurrentNode.SpiritStoneReward;
                    var artifactSpiritStoneReward = GetBonusSpiritStonesOnVictory(state);
                    var totalSpiritStoneReward = baseSpiritStoneReward + artifactSpiritStoneReward;
                    state.SpiritStones += totalSpiritStoneReward;
                    if (baseSpiritStoneReward > 0)
                    {
                        state.CurrentBattle.Logs.Add(new BattleLogEntry($"获得 {baseSpiritStoneReward} 灵石。"));
                    }

                    if (artifactSpiritStoneReward > 0)
                    {
                        state.CurrentBattle.Logs.Add(new BattleLogEntry($"法宝额外获得 {artifactSpiritStoneReward} 灵石。"));
                    }

                    AwardArtifactAfterEliteVictory(state);
                    RemoveExhaustedCardsFromDeck(state);
                    state.Status = CultivationRunStatus.Reward;
                    state.CurrentRewards.Clear();
                    state.CurrentRewards.AddRange(CreateRewardChoices(state.CurrentNode));
                    break;
                case BattleOutcome.Defeat:
                    state.PlayerCurrentHp = 0;
                    state.Status = CultivationRunStatus.Defeated;
                    state.CurrentRewards.Clear();
                    state.CurrentBattle = null;
                    break;
                case BattleOutcome.InProgress:
                    throw new InvalidOperationException("Battle is still in progress.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(state.CurrentBattle.Outcome), state.CurrentBattle.Outcome, "Unsupported battle outcome.");
            }
        }

        public void ChooseReward(CultivationRunState state, int rewardIndex)
        {
            EnsureRewardState(state);

            if (rewardIndex < 0 || rewardIndex >= state.CurrentRewards.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rewardIndex), "Reward index is outside the current reward choices.");
            }

            var reward = state.CurrentRewards[rewardIndex];
            state.Deck.Add(reward.Card);
            state.ClaimedRewards.Add(reward);
            AdvanceAfterReward(state);
        }

        public void SkipReward(CultivationRunState state)
        {
            EnsureRewardState(state);
            AdvanceAfterReward(state);
        }

        public void ChooseRoute(CultivationRunState state, int choiceIndex)
        {
            EnsureRouteChoiceState(state);

            if (choiceIndex < 0 || choiceIndex >= state.CurrentRouteChoices.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(choiceIndex), "Route choice index is outside the current route choices.");
            }

            var choice = state.CurrentRouteChoices[choiceIndex];
            state.CurrentNodeIndex = choice.TargetNodeIndex;
            EnterCurrentNode(state);
        }

        public void Rest(CultivationRunState state)
        {
            EnsureState(state);

            if (state.Status != CultivationRunStatus.Rest)
            {
                throw new InvalidOperationException("Run is not in rest state.");
            }

            state.PlayerCurrentHp = Math.Min(state.PlayerMaxHp, state.PlayerCurrentHp + state.CurrentNode.RestHealAmount);
            AdvanceToNextNode(state);
        }

        public void RestAndUpgrade(CultivationRunState state, int deckIndex, int upgradeOptionIndex)
        {
            EnsureState(state);

            if (state.Status != CultivationRunStatus.Rest)
            {
                throw new InvalidOperationException("Run is not in rest state.");
            }

            UpgradeDeckCard(state, deckIndex, upgradeOptionIndex);
            Rest(state);
        }

        public void UsePillInBattle(CultivationRunState state, int pillIndex)
        {
            EnsureState(state);

            if (state.Status != CultivationRunStatus.InBattle || state.CurrentBattle == null)
            {
                throw new InvalidOperationException("Run is not in battle state.");
            }

            if (state.CurrentBattle.Outcome != BattleOutcome.InProgress)
            {
                throw new InvalidOperationException("Battle is not active.");
            }

            if (pillIndex < 0 || pillIndex >= state.Pills.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(pillIndex), "Pill index is outside the current pill slots.");
            }

            var pill = state.Pills[pillIndex];
            if (pill.EffectValue <= 0 || !pill.IsBattleEffect)
            {
                throw new InvalidOperationException("Selected pill does not have a battle effect.");
            }

            var logMessage = ResolvePillEffect(state.CurrentBattle, pill);
            state.Pills.RemoveAt(pillIndex);
            state.CurrentBattle.Logs.Add(new BattleLogEntry(logMessage));
        }

        public string UsePillInRun(CultivationRunState state, int pillIndex)
        {
            EnsureState(state);

            if (state.Status == CultivationRunStatus.InBattle)
            {
                throw new InvalidOperationException("Run-level pills cannot be used during battle.");
            }

            if (state.Status == CultivationRunStatus.Completed || state.Status == CultivationRunStatus.Defeated)
            {
                throw new InvalidOperationException("Run is not active.");
            }

            if (pillIndex < 0 || pillIndex >= state.Pills.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(pillIndex), "Pill index is outside the current pill slots.");
            }

            var pill = state.Pills[pillIndex];
            if (pill.EffectValue <= 0 || !pill.IsRunEffect)
            {
                throw new InvalidOperationException("Selected pill does not have a run effect.");
            }

            var logMessage = ResolveRunPillEffect(state, pill);
            state.Pills.RemoveAt(pillIndex);
            return logMessage;
        }

        public void BuyMarketItem(CultivationRunState state, int itemIndex)
        {
            EnsureMarketState(state);

            if (itemIndex < 0 || itemIndex >= state.CurrentMarketItems.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(itemIndex), "Market item index is outside the current market items.");
            }

            var item = state.CurrentMarketItems[itemIndex];
            if (state.SpiritStones < item.Price)
            {
                throw new InvalidOperationException("Not enough spirit stones to buy the selected market item.");
            }

            if (item.IsPill && state.Pills.Count >= state.PillSlotLimit)
            {
                throw new InvalidOperationException("Pill slots are full.");
            }

            state.SpiritStones -= item.Price;
            state.PurchasedMarketItems.Add(item);
            state.CurrentMarketItems.RemoveAt(itemIndex);

            if (item.IsCard)
            {
                state.Deck.Add(item.Card);
                return;
            }

            if (item.IsArtifact)
            {
                state.Artifacts.Add(item.Artifact);
                state.PurchasedMarketArtifacts.Add(item.Artifact);
                return;
            }

            state.Pills.Add(item.Pill);
            state.PurchasedMarketPills.Add(item.Pill);
        }

        public void RemoveDeckCardAtMarket(CultivationRunState state, int deckIndex)
        {
            EnsureMarketState(state);
            EnsureDeckCardCanLeaveDeckAtMarket(state, deckIndex);

            if (state.SpiritStones < MarketCardRemovalCost)
            {
                throw new InvalidOperationException("Not enough spirit stones to remove a deck card.");
            }

            var removedCard = state.Deck[deckIndex];
            state.SpiritStones -= MarketCardRemovalCost;
            state.Deck.RemoveAt(deckIndex);
            state.RemovedMarketCards.Add(removedCard);
        }

        public int GetMarketSellValue(CultivationRunState state, int deckIndex)
        {
            EnsureMarketState(state);

            if (deckIndex < 0 || deckIndex >= state.Deck.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(deckIndex), "Deck index is outside the run deck.");
            }

            var card = state.Deck[deckIndex];
            var matchingMarketItem = state.CurrentMarketItems.FirstOrDefault(item => item.IsCard && item.Card.Id == card.Id);
            if (matchingMarketItem != null)
            {
                return matchingMarketItem.Price / 2;
            }

            var lowestCardMarketPrice = state.CurrentMarketItems
                .Where(item => item.IsCard)
                .Select(item => item.Price)
                .DefaultIfEmpty(20)
                .Min();
            if (lowestCardMarketPrice > 0)
            {
                return Math.Max(1, lowestCardMarketPrice / 2);
            }

            return 10;
        }

        public void SellDeckCardAtMarket(CultivationRunState state, int deckIndex)
        {
            EnsureMarketState(state);
            EnsureDeckCardCanLeaveDeckAtMarket(state, deckIndex);

            var soldCard = state.Deck[deckIndex];
            var sellValue = GetMarketSellValue(state, deckIndex);
            state.SpiritStones += sellValue;
            state.Deck.RemoveAt(deckIndex);
            state.SoldMarketCards.Add(soldCard);
        }

        public void UpgradeDeckCardAtMarket(CultivationRunState state, int deckIndex, int upgradeOptionIndex)
        {
            EnsureMarketState(state);

            var upgradedCard = ResolveUpgradedCard(state, deckIndex, upgradeOptionIndex);
            if (state.SpiritStones < MarketCardUpgradeCost)
            {
                throw new InvalidOperationException("Not enough spirit stones to upgrade a deck card.");
            }

            state.SpiritStones -= MarketCardUpgradeCost;
            state.Deck[deckIndex] = upgradedCard;
            state.MarketUpgradedCards.Add(upgradedCard);
        }

        public void LeaveMarket(CultivationRunState state)
        {
            EnsureMarketState(state);
            AdvanceToNextNode(state);
        }

        public ArtifactDefinition OpenChest(CultivationRunState state)
        {
            EnsureChestState(state);

            var artifact = CreateArtifactReward(state.CurrentNode);
            if (artifact == null)
            {
                throw new InvalidOperationException("Chest node does not have an artifact reward.");
            }

            state.Artifacts.Add(artifact);
            state.ChestArtifacts.Add(artifact);
            AdvanceToNextNode(state);
            return artifact;
        }

        public MysticEventOption ChooseMysticEventOption(CultivationRunState state, int optionIndex)
        {
            EnsureMysticState(state);

            if (optionIndex < 0 || optionIndex >= state.MysticEventChoices.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(optionIndex), "Mystic event option index is outside the current event choices.");
            }

            var option = state.MysticEventChoices[optionIndex];
            ResolveMysticEventOption(state, option);
            state.ResolvedMysticEventOptions.Add(option);
            AdvanceToNextNode(state);
            return option;
        }

        public void ChooseGoldenCorePassive(CultivationRunState state, int passiveIndex)
        {
            EnsureGoldenCorePassiveChoiceState(state);

            if (passiveIndex < 0 || passiveIndex >= state.CurrentGoldenCorePassiveChoices.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(passiveIndex), "Golden core passive index is outside the current choices.");
            }

            var passive = state.CurrentGoldenCorePassiveChoices[passiveIndex];
            state.SelectGoldenCorePassive(passive);
            StartCurrentBattle(state);
            state.CurrentBattle.Logs.Add(new BattleLogEntry($"选择金丹被动：{passive.Name}。{passive.Description}"));
        }

        private void EnterCurrentNode(CultivationRunState state)
        {
            var breakthrough = state.TryBreakthroughTo(state.CurrentNode.Realm);
            if (breakthrough && state.CurrentRealm == CultivationRealm.GoldenCore && state.SelectedGoldenCorePassive == null)
            {
                state.CurrentBattle = null;
                state.CurrentRewards.Clear();
                state.RestUpgradeChoices.Clear();
                state.CurrentRouteChoices.Clear();
                state.CurrentMarketItems.Clear();
                state.MysticEventChoices.Clear();
                state.SetGoldenCorePassiveChoices(CultivationSeedData.CreateGoldenCorePassiveChoices());
                state.Status = CultivationRunStatus.GoldenCorePassiveChoice;
                return;
            }

            switch (state.CurrentNode.Type)
            {
                case CultivationRunNodeType.Battle:
                case CultivationRunNodeType.Elite:
                    StartCurrentBattle(state);
                    break;
                case CultivationRunNodeType.Rest:
                    state.CurrentBattle = null;
                    state.CurrentRewards.Clear();
                    state.CurrentRouteChoices.Clear();
                    state.CurrentMarketItems.Clear();
                    RefreshRestUpgradeChoices(state);
                    state.Status = CultivationRunStatus.Rest;
                    break;
                case CultivationRunNodeType.Market:
                    state.CurrentBattle = null;
                    state.CurrentRewards.Clear();
                    state.RestUpgradeChoices.Clear();
                    state.CurrentRouteChoices.Clear();
                    state.CurrentMarketItems.Clear();
                    state.CurrentMarketItems.AddRange(state.CurrentNode.MarketItems);
                    state.Status = CultivationRunStatus.Market;
                    break;
                case CultivationRunNodeType.Chest:
                    state.CurrentBattle = null;
                    state.CurrentRewards.Clear();
                    state.RestUpgradeChoices.Clear();
                    state.CurrentRouteChoices.Clear();
                    state.CurrentMarketItems.Clear();
                    state.Status = CultivationRunStatus.Chest;
                    break;
                case CultivationRunNodeType.Mystic:
                    state.CurrentBattle = null;
                    state.CurrentRewards.Clear();
                    state.RestUpgradeChoices.Clear();
                    state.CurrentRouteChoices.Clear();
                    state.CurrentMarketItems.Clear();
                    state.MysticEventChoices.Clear();
                    if (state.CurrentNode.MysticEvent != null)
                    {
                        state.MysticEventChoices.AddRange(state.CurrentNode.MysticEvent.Options);
                    }

                    state.Status = CultivationRunStatus.Mystic;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state.CurrentNode.Type), state.CurrentNode.Type, "Unsupported run node type.");
            }
        }

        private void StartCurrentBattle(CultivationRunState state)
        {
            state.CurrentRewards.Clear();
            state.RestUpgradeChoices.Clear();
            state.CurrentRouteChoices.Clear();
            state.CurrentBattle = _battleEngine.CreateBattle(state.Deck, state.CurrentNode.Enemy, state.PlayerCurrentHp, state.PlayerMaxHp, state.SpiritMax, state.HandLimit, state.SelectedGoldenCorePassive);
            ApplyArtifactBattleEffects(state);
            state.Status = CultivationRunStatus.InBattle;
        }

        private static void ApplyArtifactBattleEffects(CultivationRunState state)
        {
            if (state.CurrentBattle == null)
            {
                return;
            }

            foreach (var artifact in state.Artifacts)
            {
                switch (artifact.EffectType)
                {
                    case ArtifactEffectType.PreventFirstSelfHpLossEachBattle:
                        state.CurrentBattle.AddPreventSelfHpLossCharges(artifact.PreventFirstSelfHpLossEachBattleCharges);
                        state.CurrentBattle.Logs.Add(new BattleLogEntry($"{artifact.Name} 生效：本场战斗前 {artifact.PreventFirstSelfHpLossEachBattleCharges} 次自伤被免疫。"));
                        break;
                    case ArtifactEffectType.MissingHpDamageBonus:
                        state.CurrentBattle.AddArtifactMissingHpDamageBonus(artifact.MissingHpDamageBonusPerStepPercent);
                        state.CurrentBattle.Logs.Add(new BattleLogEntry($"{artifact.Name} 生效：每损失 10% 最大 HP，卡牌伤害 +{artifact.MissingHpDamageBonusPerStepPercent}%。"));
                        break;
                }
            }
        }

        private static string ResolvePillEffect(BattleState battle, PillDefinition pill)
        {
            switch (pill.EffectType)
            {
                case PillEffectType.Heal:
                    battle.Player.Heal(pill.HealAmount);
                    return $"使用 {pill.Name}，恢复 {pill.HealAmount} HP。";
                case PillEffectType.Spirit:
                    battle.Spirit += pill.SpiritAmount;
                    return $"使用 {pill.Name}，本回合灵力 +{pill.SpiritAmount}。";
                case PillEffectType.Cleanse:
                    battle.Player.ClearNegativeStatuses();
                    battle.Player.Heal(pill.CleanseHealAmount);
                    return $"使用 {pill.Name}，清除负面状态并恢复 {pill.CleanseHealAmount} HP。";
                case PillEffectType.CostReduction:
                    battle.AddSpiritCostReduction(pill.CostReductionAmount);
                    return $"使用 {pill.Name}，本场战斗功法灵力消耗 -{pill.CostReductionAmount}。";
                default:
                    throw new ArgumentOutOfRangeException(nameof(pill.EffectType), pill.EffectType, "Unsupported pill effect type.");
            }
        }

        private static string ResolveRunPillEffect(CultivationRunState state, PillDefinition pill)
        {
            switch (pill.EffectType)
            {
                case PillEffectType.MaxHp:
                    state.IncreasePlayerMaxHp(pill.MaxHpAmount);
                    return $"使用 {pill.Name}，本 Run 最大 HP +{pill.MaxHpAmount}。";
                default:
                    throw new ArgumentOutOfRangeException(nameof(pill.EffectType), pill.EffectType, "Unsupported run pill effect type.");
            }
        }

        private IReadOnlyList<CultivationRunReward> CreateRewardChoices(CultivationRunNode node)
        {
            var rewards = node.RewardPool.ToList();
            var choiceCount = Math.Min(RewardChoiceCount, rewards.Count);
            for (var i = 0; i < choiceCount; i++)
            {
                var selectedIndex = _rewardRandom.Next(i, rewards.Count);
                (rewards[i], rewards[selectedIndex]) = (rewards[selectedIndex], rewards[i]);
            }

            return rewards.Take(choiceCount).ToArray();
        }

        private static int GetBonusSpiritStonesOnVictory(CultivationRunState state)
        {
            return state.Artifacts.Sum(artifact => artifact.BonusSpiritStonesOnVictory);
        }

        private static int GetHealAfterVictory(CultivationRunState state)
        {
            return state.Artifacts.Sum(artifact => artifact.HealAfterVictoryAmount);
        }

        private ArtifactDefinition CreateArtifactReward(CultivationRunNode node)
        {
            if (node.Type != CultivationRunNodeType.Elite && node.Type != CultivationRunNodeType.Chest)
            {
                return null;
            }

            if (node.ArtifactRewardPool.Count == 0)
            {
                return null;
            }

            return node.ArtifactRewardPool[_rewardRandom.Next(node.ArtifactRewardPool.Count)];
        }

        private void AwardArtifactAfterEliteVictory(CultivationRunState state)
        {
            var artifact = CreateArtifactReward(state.CurrentNode);
            if (artifact == null)
            {
                return;
            }

            state.Artifacts.Add(artifact);
            state.DroppedArtifacts.Add(artifact);
            state.CurrentBattle.Logs.Add(new BattleLogEntry($"精英战获得法宝：{artifact.Name}。"));
        }

        private static void RemoveExhaustedCardsFromDeck(CultivationRunState state)
        {
            if (state.CurrentBattle.ExhaustPile.Count == 0)
            {
                return;
            }

            foreach (var exhaustedCard in state.CurrentBattle.ExhaustPile)
            {
                var deckIndex = state.Deck.FindIndex(card => ReferenceEquals(card, exhaustedCard));
                if (deckIndex < 0)
                {
                    deckIndex = state.Deck.FindIndex(card => card.Id == exhaustedCard.Id);
                }

                if (deckIndex >= 0)
                {
                    state.Deck.RemoveAt(deckIndex);
                }
            }
        }

        private void AdvanceAfterReward(CultivationRunState state)
        {
            AdvanceToNextNode(state);
        }

        private void AdvanceToNextNode(CultivationRunState state)
        {
            state.CurrentRewards.Clear();
            state.RestUpgradeChoices.Clear();
            state.CurrentRouteChoices.Clear();
            state.CurrentMarketItems.Clear();
            state.MysticEventChoices.Clear();
            state.CurrentBattle = null;

            var nextNodeIndices = ResolveNextNodeIndices(state);
            if (nextNodeIndices.Count == 0)
            {
                state.Status = CultivationRunStatus.Completed;
                return;
            }

            if (nextNodeIndices.Count > 1)
            {
                EnterRouteChoice(state, nextNodeIndices);
                return;
            }

            EnsureRouteTarget(state, nextNodeIndices[0]);
            state.CurrentNodeIndex = nextNodeIndices[0];
            EnterCurrentNode(state);
        }

        private static void EnsureRewardState(CultivationRunState state)
        {
            EnsureState(state);

            if (state.Status != CultivationRunStatus.Reward)
            {
                throw new InvalidOperationException("Run is not in reward state.");
            }
        }

        private static void EnsureRouteChoiceState(CultivationRunState state)
        {
            EnsureState(state);

            if (state.Status != CultivationRunStatus.RouteChoice)
            {
                throw new InvalidOperationException("Run is not waiting for a route choice.");
            }
        }

        private static void EnsureMarketState(CultivationRunState state)
        {
            EnsureState(state);

            if (state.Status != CultivationRunStatus.Market)
            {
                throw new InvalidOperationException("Run is not in market state.");
            }
        }

        private static void EnsureChestState(CultivationRunState state)
        {
            EnsureState(state);

            if (state.Status != CultivationRunStatus.Chest)
            {
                throw new InvalidOperationException("Run is not in chest state.");
            }
        }

        private static void EnsureMysticState(CultivationRunState state)
        {
            EnsureState(state);

            if (state.Status != CultivationRunStatus.Mystic)
            {
                throw new InvalidOperationException("Run is not in mystic event state.");
            }
        }

        private static void EnsureGoldenCorePassiveChoiceState(CultivationRunState state)
        {
            EnsureState(state);

            if (state.Status != CultivationRunStatus.GoldenCorePassiveChoice)
            {
                throw new InvalidOperationException("Run is not in golden core passive choice state.");
            }
        }

        private static void ResolveMysticEventOption(CultivationRunState state, MysticEventOption option)
        {
            switch (option.EffectType)
            {
                case MysticEventEffectType.GainSpiritStones:
                    state.SpiritStones += option.EffectValue;
                    break;
                case MysticEventEffectType.Heal:
                    state.PlayerCurrentHp = Math.Min(state.PlayerMaxHp, state.PlayerCurrentHp + option.EffectValue);
                    break;
                case MysticEventEffectType.GainCard:
                    if (option.CardReward == null)
                    {
                        throw new InvalidOperationException("Mystic event card option does not have a card reward.");
                    }

                    state.Deck.Add(option.CardReward);
                    break;
                case MysticEventEffectType.GainPill:
                    if (option.PillReward == null)
                    {
                        throw new InvalidOperationException("Mystic event pill option does not have a pill reward.");
                    }

                    if (state.Pills.Count >= state.PillSlotLimit)
                    {
                        throw new InvalidOperationException("Pill slots are full.");
                    }

                    state.Pills.Add(option.PillReward);
                    break;
                case MysticEventEffectType.GainArtifact:
                    if (option.ArtifactReward == null)
                    {
                        throw new InvalidOperationException("Mystic event artifact option does not have an artifact reward.");
                    }

                    state.Artifacts.Add(option.ArtifactReward);
                    break;
                case MysticEventEffectType.Leave:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(option.EffectType), option.EffectType, "Unsupported mystic event effect type.");
            }
        }

        private static IReadOnlyList<int> ResolveNextNodeIndices(CultivationRunState state)
        {
            if (state.CurrentNode.NextNodeIndices.Count > 0)
            {
                return state.CurrentNode.NextNodeIndices;
            }

            if (state.CurrentNodeIndex >= state.Route.Count - 1)
            {
                return Array.Empty<int>();
            }

            return new[] { state.CurrentNodeIndex + 1 };
        }

        private static void EnterRouteChoice(CultivationRunState state, IReadOnlyList<int> nextNodeIndices)
        {
            for (var i = 0; i < nextNodeIndices.Count; i++)
            {
                var targetIndex = nextNodeIndices[i];
                EnsureRouteTarget(state, targetIndex);
                state.CurrentRouteChoices.Add(new CultivationRunRouteChoice(targetIndex, state.Route[targetIndex]));
            }

            state.Status = CultivationRunStatus.RouteChoice;
        }

        private static void EnsureRouteTarget(CultivationRunState state, int targetNodeIndex)
        {
            if (targetNodeIndex <= state.CurrentNodeIndex || targetNodeIndex >= state.Route.Count)
            {
                throw new InvalidOperationException("Route targets must point to later nodes inside the current route.");
            }
        }

        private static void EnsureDeckCardCanLeaveDeckAtMarket(CultivationRunState state, int deckIndex)
        {
            if (deckIndex < 0 || deckIndex >= state.Deck.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(deckIndex), "Deck index is outside the run deck.");
            }

            if (state.Deck.Count <= 1)
            {
                throw new InvalidOperationException("Cannot remove the last card from the run deck.");
            }
        }

        private static void RefreshRestUpgradeChoices(CultivationRunState state)
        {
            state.RestUpgradeChoices.Clear();
            for (var i = 0; i < state.Deck.Count; i++)
            {
                var card = state.Deck[i];
                if (card.CanUpgrade)
                {
                    state.RestUpgradeChoices.Add(new CultivationRestUpgradeChoice(i, card));
                }
            }
        }

        private static void UpgradeDeckCard(CultivationRunState state, int deckIndex, int upgradeOptionIndex)
        {
            state.Deck[deckIndex] = ResolveUpgradedCard(state, deckIndex, upgradeOptionIndex);
            RefreshRestUpgradeChoices(state);
        }

        private static CardDefinition ResolveUpgradedCard(CultivationRunState state, int deckIndex, int upgradeOptionIndex)
        {
            if (deckIndex < 0 || deckIndex >= state.Deck.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(deckIndex), "Deck index is outside the run deck.");
            }

            var card = state.Deck[deckIndex];
            if (!card.CanUpgrade)
            {
                throw new InvalidOperationException("Selected card cannot be upgraded.");
            }

            if (upgradeOptionIndex < 0 || upgradeOptionIndex >= card.UpgradeOptions.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(upgradeOptionIndex), "Upgrade option index is outside the selected card options.");
            }

            return card.UpgradeOptions[upgradeOptionIndex].UpgradedCard;
        }

        private static void EnsureState(CultivationRunState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
        }
    }
}
