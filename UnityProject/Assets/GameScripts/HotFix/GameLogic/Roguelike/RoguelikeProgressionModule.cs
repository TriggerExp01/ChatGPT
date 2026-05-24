using System.Collections.Generic;

namespace GameLogic
{
    public sealed class RoguelikeProgressionModule
    {
        private readonly RoguelikeConfigModule _configModule;

        public RoguelikeProgressionModule(RoguelikeConfigModule configModule)
        {
            _configModule = configModule;
        }

        public void BuildRewardOptions(RoguelikeRunState run, RoguelikeRoom clearedRoom, List<RoguelikeChoiceOption> targetOptions)
        {
            targetOptions.Clear();
            bool isShop = clearedRoom.Type == RoguelikeRoomType.Shop;
            List<RoguelikeChoiceOption> pool = _configModule.CreateChoicePool(clearedRoom.Type);

            int randomOptionCount = isShop ? 2 : 3;
            int seed = run.Seed + clearedRoom.Index * 3571 + clearedRoom.CombatTurnCount * 997;
            System.Random random = new System.Random(seed);
            while (targetOptions.Count < randomOptionCount && pool.Count > 0)
            {
                int index = random.Next(pool.Count);
                targetOptions.Add(pool[index]);
                pool.RemoveAt(index);
            }

            if (isShop)
            {
                targetOptions.Add(_configModule.CreateLeaveShopOption());
            }
        }

        public bool TryApplyChoice(RoguelikeRunState run, RoguelikeChoiceOption option, out string message)
        {
            if (run == null || option == null)
            {
                message = "选择无效。";
                return false;
            }

            if (!option.CanAfford(run))
            {
                message = $"金币不足：{option.Title} 需要 {option.Cost} 金币。";
                return false;
            }

            run.TrySpendGold(option.Cost);
            option.Apply?.Invoke(run);
            message = option.Cost > 0 ? $"已购买：{option.Title}。" : $"已选择：{option.Title}。";
            return true;
        }
    }
}
