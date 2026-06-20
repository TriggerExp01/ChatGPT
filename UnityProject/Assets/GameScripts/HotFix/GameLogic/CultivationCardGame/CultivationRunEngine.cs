using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunEngine
    {
        private const int RewardChoiceCount = 3;

        private readonly BattleEngine _battleEngine;

        public CultivationRunEngine(BattleEngine battleEngine = null)
        {
            _battleEngine = battleEngine ?? new BattleEngine(20260620);
        }

        public CultivationRunState StartRun(IEnumerable<CardDefinition> deck = null, IEnumerable<CultivationRunNode> route = null, int playerMaxHp = 100, int? playerCurrentHp = null)
        {
            var state = new CultivationRunState(
                deck ?? CultivationSeedData.CreateSwordSectStarterDeck(),
                route ?? CultivationSeedData.CreateFirstPrototypeRoute(),
                playerMaxHp,
                playerCurrentHp);

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

        private void EnterCurrentNode(CultivationRunState state)
        {
            switch (state.CurrentNode.Type)
            {
                case CultivationRunNodeType.Battle:
                case CultivationRunNodeType.Elite:
                    StartCurrentBattle(state);
                    break;
                case CultivationRunNodeType.Rest:
                    state.CurrentBattle = null;
                    state.CurrentRewards.Clear();
                    state.Status = CultivationRunStatus.Rest;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state.CurrentNode.Type), state.CurrentNode.Type, "Unsupported run node type.");
            }
        }

        private void StartCurrentBattle(CultivationRunState state)
        {
            state.CurrentRewards.Clear();
            state.CurrentBattle = _battleEngine.CreateBattle(state.Deck, state.CurrentNode.Enemy, state.PlayerCurrentHp, state.PlayerMaxHp);
            state.Status = CultivationRunStatus.InBattle;
        }

        private static IReadOnlyList<CultivationRunReward> CreateRewardChoices(CultivationRunNode node)
        {
            return node.RewardPool.Take(RewardChoiceCount).ToArray();
        }

        private void AdvanceAfterReward(CultivationRunState state)
        {
            AdvanceToNextNode(state);
        }

        private void AdvanceToNextNode(CultivationRunState state)
        {
            state.CurrentRewards.Clear();
            state.CurrentBattle = null;

            if (state.CurrentNodeIndex >= state.Route.Count - 1)
            {
                state.Status = CultivationRunStatus.Completed;
                return;
            }

            state.CurrentNodeIndex++;
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

        private static void EnsureState(CultivationRunState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
        }
    }
}
