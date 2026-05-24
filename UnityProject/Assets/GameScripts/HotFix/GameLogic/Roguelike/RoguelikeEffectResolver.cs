using GameConfig.roguelike;

namespace GameLogic
{
    public static class RoguelikeEffectResolver
    {
        public static void Apply(RoguelikeRunState run, Effect effect)
        {
            if (run == null || effect == null)
            {
                return;
            }

            switch (effect.Type)
            {
                case EEffectType.AddAttack:
                    run.Player.Stats.AddAttack((int)effect.Value);
                    break;
                case EEffectType.AddDefense:
                    run.Player.Stats.AddDefense((int)effect.Value);
                    break;
                case EEffectType.Heal:
                    run.Player.Heal((int)effect.Value);
                    break;
                case EEffectType.AddMaxHealth:
                    run.Player.Stats.AddMaxHealth((int)effect.Value);
                    break;
                case EEffectType.AddCritChance:
                    run.Player.Stats.AddCritChance(effect.Value);
                    break;
                case EEffectType.AddGold:
                    run.AddGold((int)effect.Value);
                    break;
            }
        }
    }
}
