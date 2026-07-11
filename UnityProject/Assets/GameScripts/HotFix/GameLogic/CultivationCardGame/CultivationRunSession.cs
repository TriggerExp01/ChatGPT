using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Cultivation
{
    /// <summary>
    /// 一局修仙行程的业务会话。视图只负责展示和转发操作，所有状态变化都由
    /// <see cref="BattleEngine"/> 与 <see cref="CultivationRunEngine"/> 驱动。
    /// </summary>
    public sealed class CultivationRunSession
    {
        public const int DefaultBattleSeed = 20260620;

        private readonly BattleEngine _battleEngine;
        private readonly CultivationRunEngine _runEngine;

        public CultivationRunSession(BattleEngine battleEngine = null, int rewardSeed = 0)
        {
            _battleEngine = battleEngine ?? new BattleEngine(DefaultBattleSeed);
            _runEngine = new CultivationRunEngine(_battleEngine, rewardSeed);
        }

        public CultivationRunState State { get; private set; }

        public CultivationRunState StartNewRun(
            CultivationSect sect = CultivationSect.Sword,
            IEnumerable<CardDefinition> deck = null,
            IEnumerable<CultivationRunNode> route = null,
            int playerMaxHp = 100,
            int? playerCurrentHp = null,
            int initialSpiritStones = 0)
        {
            State = _runEngine.StartRun(
                deck,
                route ?? CultivationSeedData.CreateFirstPrototypeBranchingRoute(sect),
                playerMaxHp,
                playerCurrentHp,
                initialSpiritStones,
                sect);
            return State;
        }

        public bool CanPlayCardAt(int handIndex)
        {
            if (!HasActiveBattle() || handIndex < 0 || handIndex >= State.CurrentBattle.Hand.Count)
            {
                return false;
            }

            return _battleEngine.CanPlay(State.CurrentBattle, State.CurrentBattle.Hand[handIndex]);
        }

        public bool PlayCardAt(int handIndex)
        {
            if (!HasActiveBattle() || handIndex < 0 || handIndex >= State.CurrentBattle.Hand.Count)
            {
                return false;
            }

            var card = State.CurrentBattle.Hand[handIndex];
            if (!_battleEngine.CanPlay(State.CurrentBattle, card))
            {
                State.CurrentBattle.Logs.Add(new BattleLogEntry($"{card.Name} 灵力不足，无法打出。"));
                return false;
            }

            var target = State.CurrentBattle.Enemies.FirstOrDefault(enemy => !enemy.Body.IsDefeated);
            _battleEngine.PlayCard(State.CurrentBattle, card, target);
            return true;
        }

        public bool EndTurn()
        {
            if (!HasActiveBattle())
            {
                return false;
            }

            _battleEngine.EndPlayerTurn(State.CurrentBattle);
            return true;
        }

        public bool UsePillInBattle(int pillIndex)
        {
            if (!HasActiveBattle() || pillIndex < 0 || pillIndex >= State.Pills.Count)
            {
                return false;
            }

            _runEngine.UsePillInBattle(State, pillIndex);
            return true;
        }

        public string UsePillInRun(int pillIndex)
        {
            if (State == null || State.Status == CultivationRunStatus.InBattle ||
                State.Status == CultivationRunStatus.Completed || State.Status == CultivationRunStatus.Defeated ||
                pillIndex < 0 || pillIndex >= State.Pills.Count)
            {
                return string.Empty;
            }

            return _runEngine.UsePillInRun(State, pillIndex);
        }

        public bool ResolveBattle()
        {
            if (State == null || State.Status != CultivationRunStatus.InBattle || State.CurrentBattle == null ||
                State.CurrentBattle.Outcome == BattleOutcome.InProgress)
            {
                return false;
            }

            _runEngine.ResolveBattleResult(State);
            return true;
        }

        public bool ChooseReward(int rewardIndex)
        {
            if (State == null || State.Status != CultivationRunStatus.Reward ||
                rewardIndex < 0 || rewardIndex >= State.CurrentRewards.Count)
            {
                return false;
            }

            _runEngine.ChooseReward(State, rewardIndex);
            return true;
        }

        public bool SkipReward()
        {
            if (State == null || State.Status != CultivationRunStatus.Reward)
            {
                return false;
            }

            _runEngine.SkipReward(State);
            return true;
        }

        public bool Rest()
        {
            if (State == null || State.Status != CultivationRunStatus.Rest)
            {
                return false;
            }

            _runEngine.Rest(State);
            return true;
        }

        public bool RestAndUpgrade(int deckIndex, int upgradeOptionIndex)
        {
            if (State == null || State.Status != CultivationRunStatus.Rest)
            {
                return false;
            }

            _runEngine.RestAndUpgrade(State, deckIndex, upgradeOptionIndex);
            return true;
        }

        public bool ChooseRoute(int choiceIndex)
        {
            if (State == null || State.Status != CultivationRunStatus.RouteChoice ||
                choiceIndex < 0 || choiceIndex >= State.CurrentRouteChoices.Count)
            {
                return false;
            }

            _runEngine.ChooseRoute(State, choiceIndex);
            return true;
        }

        public bool BuyMarketItem(int itemIndex)
        {
            if (State == null || State.Status != CultivationRunStatus.Market ||
                itemIndex < 0 || itemIndex >= State.CurrentMarketItems.Count)
            {
                return false;
            }

            _runEngine.BuyMarketItem(State, itemIndex);
            return true;
        }

        public bool RemoveDeckCardAtMarket(int deckIndex)
        {
            if (State == null || State.Status != CultivationRunStatus.Market ||
                deckIndex < 0 || deckIndex >= State.Deck.Count)
            {
                return false;
            }

            _runEngine.RemoveDeckCardAtMarket(State, deckIndex);
            return true;
        }

        public int GetMarketSellValue(int deckIndex)
        {
            if (State == null || State.Status != CultivationRunStatus.Market ||
                deckIndex < 0 || deckIndex >= State.Deck.Count)
            {
                return 0;
            }

            return _runEngine.GetMarketSellValue(State, deckIndex);
        }

        public bool SellDeckCardAtMarket(int deckIndex)
        {
            if (State == null || State.Status != CultivationRunStatus.Market ||
                deckIndex < 0 || deckIndex >= State.Deck.Count)
            {
                return false;
            }

            _runEngine.SellDeckCardAtMarket(State, deckIndex);
            return true;
        }

        public bool UpgradeDeckCardAtMarket(int deckIndex, int upgradeOptionIndex)
        {
            if (State == null || State.Status != CultivationRunStatus.Market)
            {
                return false;
            }

            _runEngine.UpgradeDeckCardAtMarket(State, deckIndex, upgradeOptionIndex);
            return true;
        }

        public bool LeaveMarket()
        {
            if (State == null || State.Status != CultivationRunStatus.Market)
            {
                return false;
            }

            _runEngine.LeaveMarket(State);
            return true;
        }

        public bool OpenChest()
        {
            if (State == null || State.Status != CultivationRunStatus.Chest)
            {
                return false;
            }

            _runEngine.OpenChest(State);
            return true;
        }

        public bool ChooseMysticEventOption(int optionIndex)
        {
            if (State == null || State.Status != CultivationRunStatus.Mystic ||
                optionIndex < 0 || optionIndex >= State.MysticEventChoices.Count)
            {
                return false;
            }

            _runEngine.ChooseMysticEventOption(State, optionIndex);
            return true;
        }

        public bool ChooseGoldenCorePassive(int passiveIndex)
        {
            if (State == null || State.Status != CultivationRunStatus.GoldenCorePassiveChoice ||
                passiveIndex < 0 || passiveIndex >= State.CurrentGoldenCorePassiveChoices.Count)
            {
                return false;
            }

            _runEngine.ChooseGoldenCorePassive(State, passiveIndex);
            return true;
        }

        private bool HasActiveBattle()
        {
            return State != null &&
                   State.Status == CultivationRunStatus.InBattle &&
                   State.CurrentBattle != null &&
                   State.CurrentBattle.Outcome == BattleOutcome.InProgress;
        }
    }
}
