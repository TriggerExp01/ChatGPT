using System.Collections.Generic;
using GameConfig;
using GameConfig.roguelike;

namespace GameLogic
{
    public sealed class RoguelikeChoiceCatalog
    {
        private readonly RoguelikeChoiceOption[] _rewardOptions;
        private readonly RoguelikeChoiceOption[] _restOptions;
        private readonly RoguelikeChoiceOption[] _treasureOptions;
        private readonly RoguelikeChoiceOption[] _shopOptions;

        public RoguelikeChoiceCatalog(Tables tables)
        {
            _rewardOptions = BuildChoices(tables.TbRoguelikeChoice.DataList, EChoicePool.Reward);
            _restOptions = BuildChoices(tables.TbRoguelikeChoice.DataList, EChoicePool.Rest);
            _treasureOptions = BuildChoices(tables.TbRoguelikeChoice.DataList, EChoicePool.Treasure);
            _shopOptions = BuildChoices(tables.TbRoguelikeChoice.DataList, EChoicePool.Shop);
        }

        public List<RoguelikeChoiceOption> CreateChoicePool(RoguelikeRoomType roomType)
        {
            switch (roomType)
            {
                case RoguelikeRoomType.Rest:
                    return new List<RoguelikeChoiceOption>(_restOptions);
                case RoguelikeRoomType.Treasure:
                    return new List<RoguelikeChoiceOption>(_treasureOptions);
                case RoguelikeRoomType.Shop:
                    return new List<RoguelikeChoiceOption>(_shopOptions);
                default:
                    return new List<RoguelikeChoiceOption>(_rewardOptions);
            }
        }

        public RoguelikeChoiceOption CreateLeaveShopOption()
        {
            return new RoguelikeChoiceOption("shop_leave", "离开商店", "保留金币", run => { });
        }

        private static RoguelikeChoiceOption[] BuildChoices(IReadOnlyList<RoguelikeChoice> rows, EChoicePool pool)
        {
            List<RoguelikeChoiceOption> result = new List<RoguelikeChoiceOption>();
            for (int i = 0; i < rows.Count; i++)
            {
                RoguelikeChoice row = rows[i];
                if (row.PoolType != pool)
                {
                    continue;
                }

                result.Add(new RoguelikeChoiceOption(row.Id, row.Title, row.Desc, run => ApplyEffects(run, row.Effects), row.Cost));
            }

            return result.ToArray();
        }

        private static void ApplyEffects(RoguelikeRunState run, IReadOnlyList<Effect> effects)
        {
            if (effects == null)
            {
                return;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                RoguelikeEffectResolver.Apply(run, effects[i]);
            }
        }
    }
}
