using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public enum RoguelikeRoomType
    {
        Start,
        Combat,
        Elite,
        Treasure,
        Rest,
        Shop,
        Boss,
    }

    public enum RoguelikeRewardType
    {
        None,
        Gold,
        Heal,
        Relic,
    }

    public enum RoguelikeGamePhase
    {
        Running,
        RewardChoice,
        Victory,
        Defeated,
    }

    public sealed class RoguelikeStats
    {
        public int MaxHealth { get; private set; }

        public int Attack { get; private set; }

        public int Defense { get; private set; }

        public float CritChance { get; private set; }

        public float CritMultiplier { get; private set; }

        public RoguelikeStats(int maxHealth, int attack, int defense, float critChance, float critMultiplier)
        {
            MaxHealth = maxHealth;
            Attack = attack;
            Defense = defense;
            CritChance = critChance;
            CritMultiplier = critMultiplier;
        }

        public RoguelikeStats Clone()
        {
            return new RoguelikeStats(MaxHealth, Attack, Defense, CritChance, CritMultiplier);
        }

        public void AddMaxHealth(int value)
        {
            MaxHealth = Math.Max(1, MaxHealth + value);
        }

        public void AddAttack(int value)
        {
            Attack = Math.Max(0, Attack + value);
        }

        public void AddDefense(int value)
        {
            Defense = Math.Max(0, Defense + value);
        }

        public void AddCritChance(float value)
        {
            CritChance = Clamp01(CritChance + value);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }
    }

    public sealed class RoguelikeActorState
    {
        public string Id { get; }

        public string DisplayName { get; }

        public RoguelikeStats Stats { get; }

        public int Health { get; private set; }

        public bool IsAlive => Health > 0;

        public RoguelikeActorState(string id, string displayName, RoguelikeStats stats)
        {
            Id = id;
            DisplayName = displayName;
            Stats = stats;
            Health = stats.MaxHealth;
        }

        public int TakeDamage(int rawDamage)
        {
            int finalDamage = Math.Max(1, rawDamage - Stats.Defense);
            Health = Math.Max(0, Health - finalDamage);
            return finalDamage;
        }

        public int Heal(int amount)
        {
            int before = Health;
            Health = Math.Min(Stats.MaxHealth, Health + Math.Max(0, amount));
            return Health - before;
        }
    }

    public sealed class RoguelikeEnemyTemplate
    {
        public string Id { get; }

        public string DisplayName { get; }

        public RoguelikeStats Stats { get; }

        public RoguelikeEnemyTemplate(string id, string displayName, RoguelikeStats stats)
        {
            Id = id;
            DisplayName = displayName;
            Stats = stats;
        }

        public RoguelikeActorState CreateActor(int depth)
        {
            RoguelikeStats stats = Stats.Clone();
            int attackGrowth = Math.Max(0, depth / 3);
            int healthGrowth = Math.Max(0, depth * 3);
            stats.AddAttack(attackGrowth);
            stats.AddMaxHealth(healthGrowth);
            return new RoguelikeActorState(Id, DisplayName, stats);
        }
    }

    public sealed class RoguelikeRelicTemplate
    {
        public string Id { get; }

        public string DisplayName { get; }

        public string Description { get; }

        public Action<RoguelikeRunState> Apply { get; }

        public RoguelikeRelicTemplate(string id, string displayName, string description, Action<RoguelikeRunState> apply)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            Apply = apply;
        }
    }

    public sealed class RoguelikeReward
    {
        public RoguelikeRewardType Type { get; }

        public int Amount { get; }

        public RoguelikeRelicTemplate Relic { get; }

        public RoguelikeReward(RoguelikeRewardType type, int amount = 0, RoguelikeRelicTemplate relic = null)
        {
            Type = type;
            Amount = amount;
            Relic = relic;
        }
    }

    public sealed class RoguelikeChoiceOption
    {
        public string Id { get; }

        public string Title { get; }

        public string Description { get; }

        public int Cost { get; }

        public Action<RoguelikeRunState> Apply { get; }

        public RoguelikeChoiceOption(string id, string title, string description, Action<RoguelikeRunState> apply, int cost = 0)
        {
            Id = id;
            Title = title;
            Description = description;
            Apply = apply;
            Cost = Math.Max(0, cost);
        }

        public bool CanAfford(RoguelikeRunState run)
        {
            return run != null && run.Gold >= Cost;
        }
    }

    public sealed class RoguelikeRoom
    {
        public int Index { get; }

        public RoguelikeRoomType Type { get; }

        public RoguelikeActorState Enemy { get; }

        public RoguelikeReward Reward { get; }

        public bool IsCleared { get; private set; }

        public int CombatTurnCount { get; private set; }
        public int DamageDealt { get; private set; }
        public int DamageTaken { get; private set; }

        public RoguelikeRoom(int index, RoguelikeRoomType type, RoguelikeActorState enemy, RoguelikeReward reward)
        {
            Index = index;
            Type = type;
            Enemy = enemy;
            Reward = reward;
        }

        public int AdvanceCombatTurn()
        {
            CombatTurnCount++;
            return CombatTurnCount;
        }

        public void RecordDamageDealt(int amount)
        {
            DamageDealt += Math.Max(0, amount);
        }

        public void RecordDamageTaken(int amount)
        {
            DamageTaken += Math.Max(0, amount);
        }

        public void MarkCleared()
        {
            IsCleared = true;
        }
    }

    public sealed class RoguelikeRunState
    {
        private readonly List<RoguelikeRoom> _rooms;
        private readonly List<RoguelikeRelicTemplate> _relics = new List<RoguelikeRelicTemplate>();

        public int Seed { get; }

        public RoguelikeActorState Player { get; }

        public int Gold { get; private set; }

        public int CurrentRoomIndex { get; private set; }
        public int ClearedRoomCount { get; private set; }
        public int TotalDamageDealt { get; private set; }
        public int TotalDamageTaken { get; private set; }
        public int TotalTurns { get; private set; }

        public IReadOnlyList<RoguelikeRoom> Rooms => _rooms;

        public IReadOnlyList<RoguelikeRelicTemplate> Relics => _relics;

        public RoguelikeRoom CurrentRoom => CurrentRoomIndex >= 0 && CurrentRoomIndex < _rooms.Count ? _rooms[CurrentRoomIndex] : null;

        public bool IsCompleted => _rooms.Count > 0 && CurrentRoomIndex >= _rooms.Count - 1 && _rooms[CurrentRoomIndex].IsCleared;

        public RoguelikeRunState(int seed, RoguelikeActorState player, List<RoguelikeRoom> rooms)
        {
            Seed = seed;
            Player = player;
            _rooms = rooms;
        }

        public void AddGold(int amount)
        {
            Gold = Math.Max(0, Gold + amount);
        }

        public bool TrySpendGold(int amount)
        {
            int cost = Math.Max(0, amount);
            if (Gold < cost)
            {
                return false;
            }

            Gold -= cost;
            return true;
        }

        public void AddRelic(RoguelikeRelicTemplate relic)
        {
            if (relic == null)
            {
                return;
            }

            _relics.Add(relic);
            relic.Apply?.Invoke(this);
        }

        public void RecordTurn()
        {
            TotalTurns++;
        }

        public void RecordDamageDealt(int amount)
        {
            TotalDamageDealt += Math.Max(0, amount);
        }

        public void RecordDamageTaken(int amount)
        {
            TotalDamageTaken += Math.Max(0, amount);
        }

        public void MarkRoomCleared()
        {
            ClearedRoomCount++;
        }

        public bool MoveNextRoom()
        {
            if (CurrentRoomIndex + 1 >= _rooms.Count)
            {
                return false;
            }

            CurrentRoomIndex++;
            return true;
        }
    }

    public sealed class RoguelikeCombatResult
    {
        public bool PlayerWon { get; }

        public int TurnCount { get; }

        public string Summary { get; }

        public RoguelikeCombatResult(bool playerWon, int turnCount, string summary)
        {
            PlayerWon = playerWon;
            TurnCount = turnCount;
            Summary = summary;
        }
    }

    public sealed class RoguelikeSurvivalEnemy
    {
        public int Id { get; }
        public Vector2 Position;
        public int Health;
        public int MaxHealth;
        public int Attack;
        public float MoveSpeed;
        public float AttackCooldown;
        public float HitFlash;

        public bool IsAlive => Health > 0;

        public RoguelikeSurvivalEnemy(int id, Vector2 position, int health, int attack, float moveSpeed)
        {
            Id = id;
            Position = position;
            Health = health;
            MaxHealth = health;
            Attack = attack;
            MoveSpeed = moveSpeed;
        }
    }

    public enum RoguelikePickupType
    {
        Experience,
        Gold,
    }

    public enum RoguelikeWeaponType
    {
        MagicBolt,
        SpinningBlade,
    }

    public sealed class RoguelikeSurvivalWeapon
    {
        public RoguelikeWeaponType Type { get; }
        public string DisplayName { get; }
        public int Level { get; private set; }
        public float CooldownRemaining;

        public RoguelikeSurvivalWeapon(RoguelikeWeaponType type, string displayName, int level)
        {
            Type = type;
            DisplayName = displayName;
            Level = Math.Max(1, level);
        }

        public void LevelUp()
        {
            Level++;
        }
    }

    public sealed class RoguelikeSurvivalPickup
    {
        public int Id { get; }
        public RoguelikePickupType Type { get; }
        public Vector2 Position;
        public int Amount { get; }

        public RoguelikeSurvivalPickup(int id, RoguelikePickupType type, Vector2 position, int amount)
        {
            Id = id;
            Type = type;
            Position = position;
            Amount = amount;
        }
    }

    public sealed class RoguelikeSurvivalProjectile
    {
        public int Id { get; }
        public RoguelikeWeaponType WeaponType { get; }
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed;
        public float RemainingDistance;
        public int Damage;

        public RoguelikeSurvivalProjectile(int id, RoguelikeWeaponType weaponType, Vector2 position, Vector2 direction, float speed, float distance, int damage)
        {
            Id = id;
            WeaponType = weaponType;
            Position = position;
            Direction = direction;
            Speed = speed;
            RemainingDistance = distance;
            Damage = damage;
        }
    }
}
