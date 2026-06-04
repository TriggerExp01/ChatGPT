using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public sealed class RoguelikeGame : Singleton<RoguelikeGame>
    {
        private const string MetaGoldKey = "Roguelike.MetaGold";
        public const float ArenaHalfWidth = 7.5f;
        public const float ArenaHalfHeight = 4.2f;

        private readonly List<RoguelikeSurvivalEnemy> _enemies = new List<RoguelikeSurvivalEnemy>();
        private readonly List<RoguelikeSurvivalPickup> _pickups = new List<RoguelikeSurvivalPickup>();
        private readonly List<RoguelikeSurvivalProjectile> _projectiles = new List<RoguelikeSurvivalProjectile>();
        private readonly List<RoguelikeChoiceOption> _rewardOptions = new List<RoguelikeChoiceOption>(3);
        private readonly System.Random _random = new System.Random();
        private Vector2 _moveInput;
        private Vector2 _attackDirection = Vector2.right;
        private float _spawnTimer;
        private float _attackTimer;
        private float _hurtTimer;
        private int _nextEnemyId;
        private int _nextPickupId;
        private int _nextProjectileId;
        private bool _metaSaved;

        public RoguelikeRunState CurrentRun { get; private set; }
        public RoguelikeGamePhase Phase { get; private set; }
        public bool IsPaused { get; private set; }
        public string LastMessage { get; private set; } = "准备开始。";
        public IReadOnlyList<RoguelikeChoiceOption> RewardOptions => _rewardOptions;
        public IReadOnlyList<RoguelikeSurvivalEnemy> Enemies => _enemies;
        public IReadOnlyList<RoguelikeSurvivalPickup> Pickups => _pickups;
        public IReadOnlyList<RoguelikeSurvivalProjectile> Projectiles => _projectiles;
        public Vector2 PlayerPosition { get; private set; }
        public Vector2 MoveInput => _moveInput;
        public Vector2 AttackDirection => _attackDirection;
        public float ElapsedTime { get; private set; }
        public int Level { get; private set; }
        public int Experience { get; private set; }
        public int ExperienceToNextLevel { get; private set; }
        public int KillCount { get; private set; }
        public int MetaGold => PlayerPrefs.GetInt(MetaGoldKey, 0);
        public float MoveSpeed { get; private set; }
        public float AttackRange { get; private set; }
        public float AttackInterval { get; private set; }
        public float AttackFlash { get; private set; }
        public float SkillCooldownRemaining => _attackTimer;
        public float DashCooldownRemaining => 0f;
        public float PlayerLanePosition => PlayerPosition.x;
        public float EnemyLanePosition => _enemies.Count > 0 ? _enemies[0].Position.x : 0f;
        public bool InRealtimeCombat => Phase == RoguelikeGamePhase.Running;
        public string SettlementSummary =>
            $"本局结束\n生存时间 {ElapsedTime:0.0} 秒　等级 {Level}　击杀 {KillCount}\n本局金币 {CurrentRun?.Gold ?? 0}　永久金币 {MetaGold}\n永久金币每累计 100 点，下局初始攻击 +1";

        protected override void OnInit()
        {
        }

        public void StartNewRun()
        {
            StartNewRun(unchecked((int)DateTime.UtcNow.Ticks));
        }

        public void StartNewRun(int seed)
        {
            int attackBonus = MetaGold / 100;
            RoguelikeStats stats = new RoguelikeStats(100, 12 + attackBonus, 1, 0f, 1.5f);
            CurrentRun = new RoguelikeRunState(seed, new RoguelikeActorState("player", "玩家", stats), new List<RoguelikeRoom>());
            Phase = RoguelikeGamePhase.Running;
            IsPaused = false;
            LastMessage = "生存挑战开始。击败敌人并收集经验升级。";
            PlayerPosition = Vector2.zero;
            _moveInput = Vector2.zero;
            _attackDirection = Vector2.right;
            _enemies.Clear();
            _pickups.Clear();
            _projectiles.Clear();
            _rewardOptions.Clear();
            _spawnTimer = 0.2f;
            _attackTimer = 0f;
            _hurtTimer = 0f;
            _nextEnemyId = 1;
            _nextPickupId = 1;
            _nextProjectileId = 1;
            _metaSaved = false;
            ElapsedTime = 0f;
            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 12;
            KillCount = 0;
            MoveSpeed = 4.5f;
            AttackRange = 5.5f;
            AttackInterval = 0.55f;
            AttackFlash = 0f;
        }

        public RoguelikeCombatResult Tick(float deltaTime)
        {
            if (CurrentRun == null)
            {
                StartNewRun();
            }

            if (IsPaused || Phase != RoguelikeGamePhase.Running)
            {
                return new RoguelikeCombatResult(CurrentRun.Player.IsAlive, KillCount, LastMessage);
            }

            float dt = Mathf.Clamp(deltaTime, 0f, 0.1f);
            ElapsedTime += dt;
            _attackTimer = Mathf.Max(0f, _attackTimer - dt);
            _hurtTimer = Mathf.Max(0f, _hurtTimer - dt);
            AttackFlash = Mathf.Max(0f, AttackFlash - dt);

            UpdateSpawning(dt);
            UpdateEnemies(dt);
            UpdateAttack();
            UpdateProjectiles(dt);
            UpdatePickups(dt);

            if (!CurrentRun.Player.IsAlive)
            {
                FinishRun();
            }

            return new RoguelikeCombatResult(CurrentRun.Player.IsAlive, KillCount, LastMessage);
        }

        public RoguelikeCombatResult Tick(float deltaTime, float moveAxisInput)
        {
            SetMoveInput(moveAxisInput);
            return Tick(deltaTime);
        }

        public RoguelikeCombatResult TickAutoBattle()
        {
            return Tick(0.2f);
        }

        public RoguelikeCombatResult ResolveCurrentRoom()
        {
            return TickAutoBattle();
        }

        public RoguelikeCombatResult UpdateRealtimeCombat(float deltaTime)
        {
            return Tick(deltaTime);
        }

        public void SetMoveInput(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void SetMoveInput(float axis)
        {
            SetMoveInput(new Vector2(axis, 0f));
        }

        public void SyncPlayerPosition(Vector2 position)
        {
            PlayerPosition = new Vector2(
                Mathf.Clamp(position.x, -ArenaHalfWidth, ArenaHalfWidth),
                Mathf.Clamp(position.y, -ArenaHalfHeight, ArenaHalfHeight));
        }

        public void SetAttackDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                _attackDirection = direction.normalized;
            }
        }

        public void RequestSkill()
        {
            _attackTimer = 0f;
        }

        public void RequestDash()
        {
        }

        public void TogglePause()
        {
            if (Phase == RoguelikeGamePhase.Running)
            {
                IsPaused = !IsPaused;
                LastMessage = IsPaused ? "游戏已暂停。" : "继续战斗。";
            }
        }

        public void ChooseReward(int index)
        {
            if (Phase != RoguelikeGamePhase.RewardChoice || index < 0 || index >= _rewardOptions.Count)
            {
                return;
            }

            RoguelikeChoiceOption option = _rewardOptions[index];
            option.Apply?.Invoke(CurrentRun);
            LastMessage = $"升级选择：{option.Title}。";
            _rewardOptions.Clear();
            Phase = RoguelikeGamePhase.Running;
        }

        public void DebugAddGold(int amount)
        {
            CurrentRun?.AddGold(Math.Max(1, amount));
        }

        public void DebugAddRandomRelic()
        {
            GainExperience(ExperienceToNextLevel);
        }

        public void DebugSkipRoom(int count = 1)
        {
            ElapsedTime += Math.Max(1, count) * 30f;
        }

        public void DebugForceVictory()
        {
            Phase = RoguelikeGamePhase.Victory;
            SaveMetaGold();
        }

        public void DebugForceDefeat()
        {
            CurrentRun?.Player.TakeDamage(int.MaxValue);
            FinishRun();
        }

        public string RunSmokeSimulation(int runCount, int maxStepsPerRun = 512)
        {
            return $"生存原型冒烟检查：请求 {Math.Max(1, runCount)} 轮，当前敌人 {_enemies.Count}，等级 {Level}。";
        }

        private void UpdateSpawning(float dt)
        {
            _spawnTimer -= dt;
            if (_spawnTimer > 0f || _enemies.Count >= 80)
            {
                return;
            }

            int wave = 1 + Mathf.FloorToInt(ElapsedTime / 30f);
            int spawnCount = 1 + Mathf.FloorToInt(ElapsedTime / 45f);
            for (int i = 0; i < spawnCount; i++)
            {
                bool horizontalEdge = _random.NextDouble() < 0.5;
                float edgeSign = _random.NextDouble() < 0.5 ? -1f : 1f;
                Vector2 position = horizontalEdge
                    ? new Vector2(edgeSign * ArenaHalfWidth, Mathf.Lerp(-ArenaHalfHeight, ArenaHalfHeight, (float)_random.NextDouble()))
                    : new Vector2(Mathf.Lerp(-ArenaHalfWidth, ArenaHalfWidth, (float)_random.NextDouble()), edgeSign * ArenaHalfHeight);
                int health = 18 + wave * 5;
                int attack = 7 + wave * 2;
                float speed = 1.35f + wave * 0.08f;
                _enemies.Add(new RoguelikeSurvivalEnemy(_nextEnemyId++, position, health, attack, speed));
            }

            _spawnTimer = Mathf.Max(0.35f, 1.35f - ElapsedTime * 0.006f);
        }

        private void UpdateEnemies(float dt)
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                RoguelikeSurvivalEnemy enemy = _enemies[i];
                enemy.AttackCooldown = Mathf.Max(0f, enemy.AttackCooldown - dt);
                enemy.HitFlash = Mathf.Max(0f, enemy.HitFlash - dt);
                Vector2 offset = PlayerPosition - enemy.Position;
                float distance = offset.magnitude;
                if (distance > 0.72f)
                {
                    enemy.Position += offset.normalized * enemy.MoveSpeed * dt;
                }
                else if (enemy.AttackCooldown <= 0f && _hurtTimer <= 0f)
                {
                    int taken = CurrentRun.Player.TakeDamage(enemy.Attack);
                    CurrentRun.RecordDamageTaken(taken);
                    enemy.AttackCooldown = 0.9f;
                    _hurtTimer = 0.25f;
                    LastMessage = $"受到 {taken} 点伤害。";
                }

                if (!enemy.IsAlive)
                {
                    _enemies.RemoveAt(i);
                    OnEnemyKilled(enemy.Position);
                }
            }
        }

        private void UpdateAttack()
        {
            if (_attackTimer > 0f)
            {
                return;
            }

            RoguelikeSurvivalEnemy target = null;
            float nearestDistance = AttackRange * AttackRange;
            for (int i = 0; i < _enemies.Count; i++)
            {
                RoguelikeSurvivalEnemy enemy = _enemies[i];
                Vector2 offset = enemy.Position - PlayerPosition;
                float distance = offset.sqrMagnitude;
                if (!enemy.IsAlive || distance > nearestDistance)
                {
                    continue;
                }

                target = enemy;
                nearestDistance = distance;
            }

            if (target == null)
            {
                return;
            }

            _attackTimer = AttackInterval;
            AttackFlash = 0.12f;
            _attackDirection = (target.Position - PlayerPosition).normalized;
            _projectiles.Add(new RoguelikeSurvivalProjectile(
                _nextProjectileId++,
                PlayerPosition,
                _attackDirection,
                9f,
                AttackRange,
                CurrentRun.Player.Stats.Attack));
        }

        private void UpdateProjectiles(float dt)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                RoguelikeSurvivalProjectile projectile = _projectiles[i];
                float distance = projectile.Speed * dt;
                projectile.Position += projectile.Direction * distance;
                projectile.RemainingDistance -= distance;

                bool hit = false;
                for (int enemyIndex = 0; enemyIndex < _enemies.Count; enemyIndex++)
                {
                    RoguelikeSurvivalEnemy enemy = _enemies[enemyIndex];
                    if (!enemy.IsAlive || Vector2.Distance(projectile.Position, enemy.Position) > 0.48f)
                    {
                        continue;
                    }

                    enemy.Health -= projectile.Damage;
                    enemy.HitFlash = 0.12f;
                    CurrentRun.RecordDamageDealt(projectile.Damage);
                    LastMessage = $"自动攻击命中，造成 {projectile.Damage} 点伤害。";
                    hit = true;
                    break;
                }

                if (hit || projectile.RemainingDistance <= 0f)
                {
                    _projectiles.RemoveAt(i);
                }
            }
        }

        private void OnEnemyKilled(Vector2 position)
        {
            KillCount++;
            _pickups.Add(new RoguelikeSurvivalPickup(_nextPickupId++, RoguelikePickupType.Experience, position, 4));
            if (_random.NextDouble() < 0.35)
            {
                _pickups.Add(new RoguelikeSurvivalPickup(_nextPickupId++, RoguelikePickupType.Gold, position + Vector2.right * 0.18f, 2));
            }
        }

        private void UpdatePickups(float dt)
        {
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                RoguelikeSurvivalPickup pickup = _pickups[i];
                Vector2 offset = PlayerPosition - pickup.Position;
                float distance = offset.magnitude;
                if (distance < 2.4f && distance > 0.05f)
                {
                    pickup.Position += offset.normalized * 7f * dt;
                }

                if (distance > 0.65f)
                {
                    continue;
                }

                if (pickup.Type == RoguelikePickupType.Experience)
                {
                    GainExperience(pickup.Amount);
                }
                else
                {
                    CurrentRun.AddGold(pickup.Amount);
                }

                _pickups.RemoveAt(i);
            }
        }

        private void GainExperience(int amount)
        {
            Experience += Math.Max(0, amount);
            if (Experience < ExperienceToNextLevel || Phase != RoguelikeGamePhase.Running)
            {
                return;
            }

            Experience -= ExperienceToNextLevel;
            Level++;
            ExperienceToNextLevel = 10 + Level * 6;
            BuildLevelUpOptions();
            Phase = RoguelikeGamePhase.RewardChoice;
            LastMessage = $"等级提升至 {Level}，请选择一项强化。";
        }

        private void BuildLevelUpOptions()
        {
            List<RoguelikeChoiceOption> pool = new List<RoguelikeChoiceOption>
            {
                new RoguelikeChoiceOption("attack", "锋利武器", "攻击力 +3", run => run.Player.Stats.AddAttack(3)),
                new RoguelikeChoiceOption("health", "强健体魄", "最大生命 +20，并恢复 20", run => { run.Player.Stats.AddMaxHealth(20); run.Player.Heal(20); }),
                new RoguelikeChoiceOption("speed", "轻盈步伐", "移动速度 +10%", run => MoveSpeed *= 1.1f),
                new RoguelikeChoiceOption("range", "延伸攻击", "攻击范围 +15%", run => AttackRange *= 1.15f),
                new RoguelikeChoiceOption("frequency", "快速攻击", "攻击频率 +12%", run => AttackInterval = Mathf.Max(0.15f, AttackInterval * 0.88f)),
            };

            _rewardOptions.Clear();
            while (_rewardOptions.Count < 3)
            {
                int index = _random.Next(pool.Count);
                _rewardOptions.Add(pool[index]);
                pool.RemoveAt(index);
            }
        }

        private void FinishRun()
        {
            Phase = RoguelikeGamePhase.Defeated;
            IsPaused = false;
            LastMessage = "角色死亡，本局结束。";
            SaveMetaGold();
        }

        private void SaveMetaGold()
        {
            if (_metaSaved || CurrentRun == null)
            {
                return;
            }

            _metaSaved = true;
            PlayerPrefs.SetInt(MetaGoldKey, MetaGold + CurrentRun.Gold);
            PlayerPrefs.Save();
        }
    }
}
