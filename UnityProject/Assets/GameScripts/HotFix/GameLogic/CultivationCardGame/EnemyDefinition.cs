using System;
using System.Collections.Generic;

namespace GameLogic.Cultivation
{
    public sealed class EnemyDefinition
    {
        public EnemyDefinition(string id, string name, int maxHp, int defense, params EnemyIntent[] intentLoop)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Enemy id is required.", nameof(id));
            }

            if (intentLoop == null || intentLoop.Length == 0)
            {
                throw new ArgumentException("Enemy requires at least one intent.", nameof(intentLoop));
            }

            Id = id;
            Name = name;
            MaxHp = maxHp;
            Defense = defense;
            IntentLoop = new List<EnemyIntent>(intentLoop).AsReadOnly();
        }

        public string Id { get; }

        public string Name { get; }

        public int MaxHp { get; }

        public int Defense { get; }

        public IReadOnlyList<EnemyIntent> IntentLoop { get; }
    }
}
