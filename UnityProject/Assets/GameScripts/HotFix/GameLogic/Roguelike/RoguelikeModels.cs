using System;
using System.Collections.Generic;
using TEngine;
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

    public enum RoguelikeEffectCueType
    {
        Hit,
        Critical,
        Kill,
        Pickup,
    }

    public readonly struct RoguelikeEffectCue
    {
        public int Sequence { get; }

        public RoguelikeEffectCueType Type { get; }

        public Vector2 Position { get; }

        public int Damage { get; }

        public bool IsCritical { get; }

        public RoguelikeEffectCue(int sequence, RoguelikeEffectCueType type, Vector2 position, int damage = 0, bool isCritical = false)
        {
            Sequence = sequence;
            Type = type;
            Position = position;
            Damage = Math.Max(0, damage);
            IsCritical = isCritical;
        }
    }

    public readonly struct RoguelikeDamageRoll
    {
        public int Amount { get; }

        public bool IsCritical { get; }

        public RoguelikeDamageRoll(int amount, bool isCritical)
        {
            Amount = Math.Max(1, amount);
            IsCritical = isCritical;
        }
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

    public readonly struct RoguelikeMemoryPoolSnapshot
    {
        public int UsingCount { get; }

        public int UnusedCount { get; }

        public int AcquireCount { get; }

        public int ReleaseCount { get; }

        public RoguelikeMemoryPoolSnapshot(int usingCount, int unusedCount, int acquireCount, int releaseCount)
        {
            UsingCount = Math.Max(0, usingCount);
            UnusedCount = Math.Max(0, unusedCount);
            AcquireCount = Math.Max(0, acquireCount);
            ReleaseCount = Math.Max(0, releaseCount);
        }

        public int TotalCount => UsingCount + UnusedCount;
    }

    public readonly struct RoguelikePerformanceSnapshot
    {
        public float ElapsedTime { get; }

        public int EnemyCount { get; }

        public int ProjectileCount { get; }

        public int PickupCount { get; }

        public int EffectCueCount { get; }

        public int ActiveEffectViewCount { get; }

        public int PooledViewCount { get; }

        public int ViewReuseCount { get; }

        public int EnemyPoolUsingCount { get; }

        public int EnemyPoolUnusedCount { get; }

        public int ProjectilePoolUsingCount { get; }

        public int ProjectilePoolUnusedCount { get; }

        public int PickupPoolUsingCount { get; }

        public int PickupPoolUnusedCount { get; }

        public long ManagedMemoryBytes { get; }

        public float AverageFps { get; }

        public RoguelikePerformanceSnapshot(
            float elapsedTime,
            int enemyCount,
            int projectileCount,
            int pickupCount,
            int effectCueCount,
            int activeEffectViewCount,
            int pooledViewCount,
            int viewReuseCount,
            RoguelikeMemoryPoolSnapshot enemyPool,
            RoguelikeMemoryPoolSnapshot projectilePool,
            RoguelikeMemoryPoolSnapshot pickupPool,
            long managedMemoryBytes,
            float averageFps)
        {
            ElapsedTime = Math.Max(0f, elapsedTime);
            EnemyCount = Math.Max(0, enemyCount);
            ProjectileCount = Math.Max(0, projectileCount);
            PickupCount = Math.Max(0, pickupCount);
            EffectCueCount = Math.Max(0, effectCueCount);
            ActiveEffectViewCount = Math.Max(0, activeEffectViewCount);
            PooledViewCount = Math.Max(0, pooledViewCount);
            ViewReuseCount = Math.Max(0, viewReuseCount);
            EnemyPoolUsingCount = enemyPool.UsingCount;
            EnemyPoolUnusedCount = enemyPool.UnusedCount;
            ProjectilePoolUsingCount = projectilePool.UsingCount;
            ProjectilePoolUnusedCount = projectilePool.UnusedCount;
            PickupPoolUsingCount = pickupPool.UsingCount;
            PickupPoolUnusedCount = pickupPool.UnusedCount;
            ManagedMemoryBytes = Math.Max(0L, managedMemoryBytes);
            AverageFps = Math.Max(0f, averageFps);
        }

        public string ToBaselineLine()
        {
            return $"时间 {ElapsedTime:0.0}s，敌人 {EnemyCount}，投射物 {ProjectileCount}，掉落 {PickupCount}，特效 {EffectCueCount}，活动特效视图 {ActiveEffectViewCount}，池化视图 {PooledViewCount}，视图复用 {ViewReuseCount}，敌人池 {EnemyPoolUsingCount}/{EnemyPoolUnusedCount}，投射物池 {ProjectilePoolUsingCount}/{ProjectilePoolUnusedCount}，掉落池 {PickupPoolUsingCount}/{PickupPoolUnusedCount}，托管内存 {ManagedMemoryBytes / 1024f:0.0}KB，平均帧率 {AverageFps:0.0}";
        }
    }

    public sealed class RoguelikeSurvivalEnemy : IMemory
    {
        public int Id { get; private set; }
        public Vector2 Position;
        public int Health;
        public int MaxHealth;
        public int Attack;
        public float MoveSpeed;
        public float AttackCooldown;
        public float HitFlash;
        public string ConfigId { get; private set; }
        public bool IsBoss { get; private set; }
        public bool IsPoolManaged { get; private set; }

        public bool IsAlive => Health > 0;

        public RoguelikeSurvivalEnemy()
        {
        }

        public RoguelikeSurvivalEnemy(int id, Vector2 position, int health, int attack, float moveSpeed)
        {
            Init(id, position, health, attack, moveSpeed, false);
        }

        public void Init(int id, Vector2 position, int health, int attack, float moveSpeed, bool poolManaged = true, string configId = null, bool isBoss = false)
        {
            Id = id;
            Position = position;
            Health = health;
            MaxHealth = health;
            Attack = attack;
            MoveSpeed = moveSpeed;
            AttackCooldown = 0f;
            HitFlash = 0f;
            ConfigId = configId ?? string.Empty;
            IsBoss = isBoss;
            IsPoolManaged = poolManaged;
        }

        public void Clear()
        {
            Id = 0;
            Position = Vector2.zero;
            Health = 0;
            MaxHealth = 0;
            Attack = 0;
            MoveSpeed = 0f;
            AttackCooldown = 0f;
            HitFlash = 0f;
            ConfigId = string.Empty;
            IsBoss = false;
            IsPoolManaged = false;
        }
    }

    public sealed class RoguelikeArenaObstacle
    {
        public Vector2 Center { get; }
        public Vector2 Size { get; }

        public Vector2 Min => Center - Size * 0.5f;
        public Vector2 Max => Center + Size * 0.5f;

        public RoguelikeArenaObstacle(Vector2 center, Vector2 size)
        {
            Center = center;
            Size = size;
        }

        public bool Contains(Vector2 position, float radius = 0f)
        {
            Vector2 min = Min - Vector2.one * radius;
            Vector2 max = Max + Vector2.one * radius;
            return position.x > min.x && position.x < max.x && position.y > min.y && position.y < max.y;
        }

        public Vector2 Resolve(Vector2 position, float radius)
        {
            if (!Contains(position, radius))
            {
                return position;
            }

            Vector2 min = Min - Vector2.one * radius;
            Vector2 max = Max + Vector2.one * radius;
            float left = Mathf.Abs(position.x - min.x);
            float right = Mathf.Abs(max.x - position.x);
            float bottom = Mathf.Abs(position.y - min.y);
            float top = Mathf.Abs(max.y - position.y);
            float nearest = Mathf.Min(Mathf.Min(left, right), Mathf.Min(bottom, top));

            if (nearest == left)
            {
                position.x = min.x;
            }
            else if (nearest == right)
            {
                position.x = max.x;
            }
            else if (nearest == bottom)
            {
                position.y = min.y;
            }
            else
            {
                position.y = max.y;
            }

            return position;
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
        PiercingDart,
        StarRingPulse,
    }

    public sealed class RoguelikeSurvivalWeapon
    {
        public const int MaxLevel = 5;

        public RoguelikeWeaponType Type { get; }
        public string DisplayName { get; }
        public int Level { get; private set; }
        public float CooldownRemaining;
        public bool IsMaxLevel => Level >= MaxLevel;

        public RoguelikeSurvivalWeapon(RoguelikeWeaponType type, string displayName, int level)
        {
            Type = type;
            DisplayName = displayName;
            Level = Math.Min(MaxLevel, Math.Max(1, level));
        }

        public void LevelUp()
        {
            Level = Math.Min(MaxLevel, Level + 1);
        }
    }

    public sealed class RoguelikeSurvivalPickup : IMemory
    {
        public int Id { get; private set; }
        public RoguelikePickupType Type { get; private set; }
        public Vector2 Position;
        public int Amount { get; private set; }
        public bool IsPoolManaged { get; private set; }

        public RoguelikeSurvivalPickup()
        {
        }

        public RoguelikeSurvivalPickup(int id, RoguelikePickupType type, Vector2 position, int amount)
        {
            Init(id, type, position, amount, false);
        }

        public void Init(int id, RoguelikePickupType type, Vector2 position, int amount, bool poolManaged = true)
        {
            Id = id;
            Type = type;
            Position = position;
            Amount = amount;
            IsPoolManaged = poolManaged;
        }

        public void Clear()
        {
            Id = 0;
            Type = RoguelikePickupType.Experience;
            Position = Vector2.zero;
            Amount = 0;
            IsPoolManaged = false;
        }
    }

    public sealed class RoguelikeSurvivalProjectile : IMemory
    {
        private readonly HashSet<int> _hitEnemyIds = new HashSet<int>();

        public int Id { get; private set; }
        public RoguelikeWeaponType WeaponType { get; private set; }
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed;
        public float RemainingDistance;
        public int Damage;
        public bool IsCritical;
        public bool IsPoolManaged { get; private set; }

        public RoguelikeSurvivalProjectile()
        {
        }

        public RoguelikeSurvivalProjectile(int id, RoguelikeWeaponType weaponType, Vector2 position, Vector2 direction, float speed, float distance, int damage, bool isCritical = false)
        {
            Init(id, weaponType, position, direction, speed, distance, damage, false, isCritical);
        }

        public void Init(int id, RoguelikeWeaponType weaponType, Vector2 position, Vector2 direction, float speed, float distance, int damage, bool poolManaged = true, bool isCritical = false)
        {
            Id = id;
            WeaponType = weaponType;
            Position = position;
            Direction = direction;
            Speed = speed;
            RemainingDistance = distance;
            Damage = damage;
            IsCritical = isCritical;
            IsPoolManaged = poolManaged;
            _hitEnemyIds.Clear();
        }

        public bool HasHitEnemy(int enemyId)
        {
            return _hitEnemyIds.Contains(enemyId);
        }

        public void RecordHitEnemy(int enemyId)
        {
            _hitEnemyIds.Add(enemyId);
        }

        public void Clear()
        {
            Id = 0;
            WeaponType = RoguelikeWeaponType.MagicBolt;
            Position = Vector2.zero;
            Direction = Vector2.zero;
            Speed = 0f;
            RemainingDistance = 0f;
            Damage = 0;
            IsCritical = false;
            IsPoolManaged = false;
            _hitEnemyIds.Clear();
        }
    }
}
