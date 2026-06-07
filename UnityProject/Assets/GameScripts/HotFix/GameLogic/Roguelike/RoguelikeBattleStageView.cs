using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public sealed class RoguelikeBattleStageView : MonoBehaviour
    {
        private const string StageName = "Roguelike2DSurvivalStage";
        public const string HitEffectPrefabAddress = "Roguelike_HitEffect";
        public const string KillEffectPrefabAddress = "Roguelike_KillEffect";
        public const string PickupEffectPrefabAddress = "Roguelike_PickupEffect";

        private readonly Dictionary<int, Transform> _enemyViews = new Dictionary<int, Transform>();
        private readonly Dictionary<int, Transform> _pickupViews = new Dictionary<int, Transform>();
        private readonly Dictionary<int, Transform> _projectileViews = new Dictionary<int, Transform>();
        private readonly Dictionary<int, RoguelikeRuntimeEffectView> _activeEffectViews = new Dictionary<int, RoguelikeRuntimeEffectView>();
        private readonly Dictionary<RoguelikeEffectCueType, Stack<Transform>> _effectViewPools = new Dictionary<RoguelikeEffectCueType, Stack<Transform>>();
        private readonly Dictionary<RoguelikeEffectCueType, string> _effectPrefabAddressOverrides = new Dictionary<RoguelikeEffectCueType, string>();
        private readonly Stack<Transform> _enemyViewPool = new Stack<Transform>();
        private readonly Stack<Transform> _pickupViewPool = new Stack<Transform>();
        private readonly Stack<Transform> _projectileViewPool = new Stack<Transform>();
        private readonly List<Transform> _obstacleViews = new List<Transform>();
        private readonly List<int> _expiredEffectIds = new List<int>();
        private Transform _player;
        private Transform _attackView;
        private Transform _pickupPulseView;
        private Camera _camera;
        private RoguelikeCameraFollow _cameraFollow;
        private RoguelikePlayerMotor _playerMotor;
        private int _lastEffectCueSequence;
        private int _effectPrefabLoadedCount;
        private int _effectFallbackCount;
        private static Sprite _whiteSprite;

        public int EnemyViewPoolCount => _enemyViewPool.Count;
        public int PickupViewPoolCount => _pickupViewPool.Count;
        public int ProjectileViewPoolCount => _projectileViewPool.Count;
        public int EffectViewPoolCount
        {
            get
            {
                int count = 0;
                foreach (Stack<Transform> pool in _effectViewPools.Values)
                {
                    count += pool.Count;
                }

                return count;
            }
        }

        public int ViewReuseCount { get; private set; }
        public int ActiveEffectViewCount => _activeEffectViews.Count;
        public int EffectPrefabLoadedCount => _effectPrefabLoadedCount;
        public int EffectFallbackCount => _effectFallbackCount;
        public bool PickupFeedbackVisible => _pickupPulseView != null && _pickupPulseView.gameObject.activeSelf;
        public float LastCameraShakeMagnitude => _cameraFollow != null ? _cameraFollow.LastShakeMagnitude : 0f;

        public void DebugSetEffectPrefabAddressOverride(RoguelikeEffectCueType type, string address)
        {
            _effectPrefabAddressOverrides[type] = address;
        }

        public static RoguelikeBattleStageView Ensure()
        {
            GameObject existing = GameObject.Find(StageName);
            if (existing != null)
            {
                RoguelikeBattleStageView existingView = existing.GetComponent<RoguelikeBattleStageView>();
                return existingView != null ? existingView : existing.AddComponent<RoguelikeBattleStageView>();
            }

            GameObject root = new GameObject(StageName);
            DontDestroyOnLoad(root);
            RoguelikeBattleStageView view = root.AddComponent<RoguelikeBattleStageView>();
            view.BuildStage();
            return view;
        }

        public void Refresh(RoguelikeRunState run)
        {
            if (_player == null)
            {
                BuildStage();
            }

            RoguelikeGame game = RoguelikeGame.Instance;
            if (_playerMotor != null)
            {
                _playerMotor.SetGame(game);
            }

            float angle = Mathf.Atan2(game.AttackDirection.y, game.AttackDirection.x) * Mathf.Rad2Deg;
            _attackView.localPosition = _player.localPosition;
            _attackView.localRotation = Quaternion.Euler(0f, 0f, angle);
            _attackView.localScale = new Vector3(0.9f, 0.25f, 1f);
            _attackView.gameObject.SetActive(game.AttackFlash > 0f);
            UpdateFeedbackViews(game);
            SyncEnemies(game.Enemies);
            SyncProjectiles(game.Projectiles);
            SyncPickups(game.Pickups);
            SyncEffectCues(game.EffectCues);
        }

        private void BuildStage()
        {
            _camera = Camera.main;
            if (_camera != null && _camera.transform.parent == transform)
            {
                _camera.transform.SetParent(null, true);
            }

            ClearChildren();
            _enemyViews.Clear();
            _pickupViews.Clear();
            _projectileViews.Clear();
            _activeEffectViews.Clear();
            _obstacleViews.Clear();
            _enemyViewPool.Clear();
            _pickupViewPool.Clear();
            _projectileViewPool.Clear();
            _effectViewPools.Clear();
            _expiredEffectIds.Clear();
            _lastEffectCueSequence = 0;
            if (_camera == null)
            {
                GameObject cameraObject = new GameObject("MainCamera");
                cameraObject.tag = "MainCamera";
                _camera = cameraObject.AddComponent<Camera>();
            }

            _camera.transform.SetParent(transform, false);
            _camera.transform.localPosition = new Vector3(0f, 0f, -10f);
            _camera.transform.localRotation = Quaternion.identity;
            _camera.orthographic = true;
            _camera.orthographicSize = 5.4f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.025f, 0.045f, 0.065f, 1f);
            _cameraFollow = _camera.GetComponent<RoguelikeCameraFollow>();
            if (_cameraFollow == null)
            {
                _cameraFollow = _camera.gameObject.AddComponent<RoguelikeCameraFollow>();
            }

            CreateSprite("地图底色", Vector3.zero, new Vector3(16f, 9f, 1f), new Color(0.08f, 0.14f, 0.15f, 1f), 0);
            for (int x = -7; x <= 7; x += 2)
            {
                for (int y = -4; y <= 4; y += 2)
                {
                    CreateSprite($"地砖{x}_{y}", new Vector3(x, y, 0f), new Vector3(1.82f, 1.82f, 1f), new Color(0.10f, 0.18f, 0.19f, 1f), 1);
                }
            }

            BuildObstacleViews(RoguelikeGame.Instance.ArenaObstacles);

            _attackView = CreateSprite("攻击范围", Vector3.zero, Vector3.one, new Color(1f, 0.82f, 0.24f, 0.45f), 8);
            _pickupPulseView = CreateSprite("拾取反馈", Vector3.zero, Vector3.one, new Color(0.35f, 0.95f, 1f, 0f), 12);
            _pickupPulseView.gameObject.SetActive(false);
            _player = CreateActor("玩家", Vector3.zero, new Color(0.16f, 0.68f, 1f, 1f), 10);
            Rigidbody2D body = _player.gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            _player.gameObject.AddComponent<CircleCollider2D>().radius = 0.48f;
            _playerMotor = _player.gameObject.AddComponent<RoguelikePlayerMotor>();
            _playerMotor.SetGame(RoguelikeGame.Instance);
            _cameraFollow.SetTarget(_player);
        }

        private void BuildObstacleViews(IReadOnlyList<RoguelikeArenaObstacle> obstacles)
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                RoguelikeArenaObstacle obstacle = obstacles[i];
                Transform body = CreateSprite(
                    $"障碍_{i + 1}",
                    obstacle.Center,
                    new Vector3(obstacle.Size.x, obstacle.Size.y, 1f),
                    new Color(0.32f, 0.35f, 0.42f, 1f),
                    3);
                CreateSprite(
                    "高光",
                    new Vector3(-obstacle.Size.x * 0.18f, obstacle.Size.y * 0.18f, -0.1f),
                    new Vector3(obstacle.Size.x * 0.35f, obstacle.Size.y * 0.16f, 1f),
                    new Color(0.55f, 0.72f, 0.86f, 0.85f),
                    4,
                    body);
                _obstacleViews.Add(body);
            }
        }

        private void SyncEffectCues(IReadOnlyList<RoguelikeEffectCue> cues)
        {
            if (cues != null && cues.Count > 0)
            {
                int newestSequence = cues[cues.Count - 1].Sequence;
                if (newestSequence < _lastEffectCueSequence)
                {
                    _lastEffectCueSequence = 0;
                }

                for (int i = 0; i < cues.Count; i++)
                {
                    RoguelikeEffectCue cue = cues[i];
                    if (cue.Sequence <= _lastEffectCueSequence)
                    {
                        continue;
                    }

                    SpawnEffectView(cue);
                    _lastEffectCueSequence = Mathf.Max(_lastEffectCueSequence, cue.Sequence);
                }
            }

            UpdateEffectViews();
        }

        private void SpawnEffectView(RoguelikeEffectCue cue)
        {
            Transform view = TakeFromPool(GetEffectViewPool(cue.Type));
            if (view == null)
            {
                GameObject instance = LoadEffectGameObject(cue.Type);
                view = instance != null ? instance.transform : null;
            }

            if (view == null)
            {
                view = CreateFallbackEffectView(cue.Type);
                _effectFallbackCount++;
            }

            Vector3 baseScale = GetEffectBaseScale(cue.Type);
            PreparePooledView(
                view,
                $"{GetEffectDisplayName(cue.Type)}_{cue.Sequence}",
                new Vector3(cue.Position.x, cue.Position.y, 0f),
                baseScale,
                Quaternion.identity);

            RoguelikeRuntimeEffectView effectView = view.GetComponent<RoguelikeRuntimeEffectView>();
            if (effectView == null)
            {
                effectView = view.gameObject.AddComponent<RoguelikeRuntimeEffectView>();
            }

            effectView.Play(cue.Type, GetEffectDuration(cue.Type), baseScale);
            _activeEffectViews[cue.Sequence] = effectView;
        }

        private void UpdateEffectViews()
        {
            if (_activeEffectViews.Count <= 0)
            {
                return;
            }

            float dt = Application.isPlaying ? Mathf.Max(Time.unscaledDeltaTime, 0.016f) : 0.016f;
            _expiredEffectIds.Clear();
            foreach (KeyValuePair<int, RoguelikeRuntimeEffectView> pair in _activeEffectViews)
            {
                RoguelikeRuntimeEffectView effectView = pair.Value;
                if (effectView == null || !effectView.Tick(dt))
                {
                    _expiredEffectIds.Add(pair.Key);
                }
            }

            for (int i = 0; i < _expiredEffectIds.Count; i++)
            {
                int id = _expiredEffectIds[i];
                if (!_activeEffectViews.TryGetValue(id, out RoguelikeRuntimeEffectView effectView) || effectView == null)
                {
                    _activeEffectViews.Remove(id);
                    continue;
                }

                RecycleView(effectView.transform, GetEffectViewPool(effectView.Type));
                _activeEffectViews.Remove(id);
            }
        }

        private GameObject LoadEffectGameObject(RoguelikeEffectCueType type)
        {
            string address = GetEffectPrefabAddress(type);
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            try
            {
                TEngine.IResourceModule resource = GameModule.Resource;
                if (resource == null || !resource.CheckLocationValid(address))
                {
                    return null;
                }

                GameObject instance = resource.LoadGameObject(address, transform);
                if (instance != null)
                {
                    _effectPrefabLoadedCount++;
                }

                return instance;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private string GetEffectPrefabAddress(RoguelikeEffectCueType type)
        {
            if (_effectPrefabAddressOverrides.TryGetValue(type, out string overrideAddress))
            {
                return overrideAddress;
            }

            switch (type)
            {
                case RoguelikeEffectCueType.Kill:
                    return KillEffectPrefabAddress;
                case RoguelikeEffectCueType.Pickup:
                    return PickupEffectPrefabAddress;
                default:
                    return HitEffectPrefabAddress;
            }
        }

        private Stack<Transform> GetEffectViewPool(RoguelikeEffectCueType type)
        {
            if (!_effectViewPools.TryGetValue(type, out Stack<Transform> pool))
            {
                pool = new Stack<Transform>();
                _effectViewPools.Add(type, pool);
            }

            return pool;
        }

        private Transform CreateFallbackEffectView(RoguelikeEffectCueType type)
        {
            Transform root = new GameObject("特效回退").transform;
            root.SetParent(transform, false);
            switch (type)
            {
                case RoguelikeEffectCueType.Kill:
                    CreateSprite("击杀光环", Vector3.zero, new Vector3(0.72f, 0.72f, 1f), new Color(1f, 0.36f, 0.62f, 0.76f), 14, root);
                    CreateSprite("击杀星芒", Vector3.zero, new Vector3(0.92f, 0.12f, 1f), new Color(1f, 0.86f, 0.30f, 0.90f), 15, root);
                    root.GetChild(1).localRotation = Quaternion.Euler(0f, 0f, 45f);
                    break;
                case RoguelikeEffectCueType.Pickup:
                    CreateSprite("拾取星核", Vector3.zero, new Vector3(0.32f, 0.32f, 1f), new Color(0.38f, 0.94f, 1f, 0.86f), 14, root);
                    CreateSprite("拾取星轨", Vector3.zero, new Vector3(0.72f, 0.08f, 1f), new Color(0.82f, 1f, 0.52f, 0.82f), 15, root);
                    root.GetChild(0).localRotation = Quaternion.Euler(0f, 0f, 45f);
                    root.GetChild(1).localRotation = Quaternion.Euler(0f, 0f, -25f);
                    break;
                default:
                    CreateSprite("命中闪光", Vector3.zero, new Vector3(0.48f, 0.12f, 1f), new Color(1f, 0.92f, 0.42f, 0.92f), 14, root);
                    CreateSprite("命中火花", Vector3.zero, new Vector3(0.12f, 0.48f, 1f), new Color(1f, 0.50f, 0.66f, 0.86f), 15, root);
                    break;
            }

            return root;
        }

        private static string GetEffectDisplayName(RoguelikeEffectCueType type)
        {
            switch (type)
            {
                case RoguelikeEffectCueType.Kill:
                    return "击杀特效";
                case RoguelikeEffectCueType.Pickup:
                    return "拾取特效";
                default:
                    return "命中特效";
            }
        }

        private static float GetEffectDuration(RoguelikeEffectCueType type)
        {
            switch (type)
            {
                case RoguelikeEffectCueType.Kill:
                    return 0.34f;
                case RoguelikeEffectCueType.Pickup:
                    return 0.24f;
                default:
                    return 0.18f;
            }
        }

        private static Vector3 GetEffectBaseScale(RoguelikeEffectCueType type)
        {
            switch (type)
            {
                case RoguelikeEffectCueType.Kill:
                    return Vector3.one * 0.95f;
                case RoguelikeEffectCueType.Pickup:
                    return Vector3.one * 0.72f;
                default:
                    return Vector3.one * 0.64f;
            }
        }

        private void UpdateFeedbackViews(RoguelikeGame game)
        {
            if (_pickupPulseView != null)
            {
                bool visible = game.PickupFlash > 0f;
                _pickupPulseView.gameObject.SetActive(visible);
                if (visible)
                {
                    float progress = 1f - Mathf.Clamp01(game.PickupFlash / 0.14f);
                    _pickupPulseView.localPosition = _player.localPosition;
                    _pickupPulseView.localScale = Vector3.one * Mathf.Lerp(0.75f, 1.75f, progress);
                    SpriteRenderer renderer = _pickupPulseView.GetComponent<SpriteRenderer>();
                    renderer.color = new Color(0.35f, 0.95f, 1f, Mathf.Lerp(0.45f, 0.05f, progress));
                }
            }

            if (_cameraFollow != null)
            {
                _cameraFollow.SetShake(game.CameraShake);
            }
        }

        private void SyncEnemies(IReadOnlyList<RoguelikeSurvivalEnemy> enemies)
        {
            HashSet<int> active = new HashSet<int>();
            for (int i = 0; i < enemies.Count; i++)
            {
                RoguelikeSurvivalEnemy enemy = enemies[i];
                active.Add(enemy.Id);
                if (!_enemyViews.TryGetValue(enemy.Id, out Transform view))
                {
                    view = SpawnEnemyView($"敌人_{enemy.Id}", enemy.Position);
                    _enemyViews.Add(enemy.Id, view);
                }

                view.GetComponent<RoguelikeEnemyMotor>().SetTarget(enemy.Position);
                SpriteRenderer renderer = view.GetComponent<SpriteRenderer>();
                renderer.color = enemy.HitFlash > 0f ? Color.white : GetEnemyColor(enemy);
                float healthRate = Mathf.Clamp01((float)enemy.Health / Mathf.Max(1, enemy.MaxHealth));
                view.localScale = Vector3.one * GetEnemyScale(enemy, healthRate);
            }

            List<int> removed = new List<int>();
            foreach (KeyValuePair<int, Transform> pair in _enemyViews)
            {
                if (!active.Contains(pair.Key))
                {
                    RecycleView(pair.Value, _enemyViewPool);
                    removed.Add(pair.Key);
                }
            }

            for (int i = 0; i < removed.Count; i++)
            {
                _enemyViews.Remove(removed[i]);
            }
        }

        private void SyncPickups(IReadOnlyList<RoguelikeSurvivalPickup> pickups)
        {
            HashSet<int> active = new HashSet<int>();
            for (int i = 0; i < pickups.Count; i++)
            {
                RoguelikeSurvivalPickup pickup = pickups[i];
                active.Add(pickup.Id);
                if (!_pickupViews.TryGetValue(pickup.Id, out Transform view))
                {
                    Color color = pickup.Type == RoguelikePickupType.Experience
                        ? new Color(0.25f, 0.72f, 1f, 1f)
                        : new Color(1f, 0.78f, 0.16f, 1f);
                    view = SpawnSpriteView(_pickupViewPool, $"掉落_{pickup.Id}", pickup.Position, new Vector3(0.22f, 0.22f, 1f), color, 7);
                    view.localRotation = Quaternion.Euler(0f, 0f, 45f);
                    _pickupViews.Add(pickup.Id, view);
                }

                view.localPosition = new Vector3(pickup.Position.x, pickup.Position.y, 0f);
            }

            List<int> removed = new List<int>();
            foreach (KeyValuePair<int, Transform> pair in _pickupViews)
            {
                if (!active.Contains(pair.Key))
                {
                    RecycleView(pair.Value, _pickupViewPool);
                    removed.Add(pair.Key);
                }
            }

            for (int i = 0; i < removed.Count; i++)
            {
                _pickupViews.Remove(removed[i]);
            }
        }

        private void SyncProjectiles(IReadOnlyList<RoguelikeSurvivalProjectile> projectiles)
        {
            HashSet<int> active = new HashSet<int>();
            for (int i = 0; i < projectiles.Count; i++)
            {
                RoguelikeSurvivalProjectile projectile = projectiles[i];
                active.Add(projectile.Id);
                if (!_projectileViews.TryGetValue(projectile.Id, out Transform view))
                {
                    Vector3 scale;
                    Color color;
                    GetProjectilePresentation(projectile.WeaponType, out scale, out color);
                    view = SpawnSpriteView(_projectileViewPool, $"投射物_{projectile.Id}", projectile.Position, scale, color, 11);
                    _projectileViews.Add(projectile.Id, view);
                }

                view.localPosition = projectile.Position;
                float angle = Mathf.Atan2(projectile.Direction.y, projectile.Direction.x) * Mathf.Rad2Deg;
                view.localRotation = Quaternion.Euler(0f, 0f, angle);
            }

            List<int> removed = new List<int>();
            foreach (KeyValuePair<int, Transform> pair in _projectileViews)
            {
                if (!active.Contains(pair.Key))
                {
                    RecycleView(pair.Value, _projectileViewPool);
                    removed.Add(pair.Key);
                }
            }

            for (int i = 0; i < removed.Count; i++)
            {
                _projectileViews.Remove(removed[i]);
            }
        }

        private static void GetProjectilePresentation(RoguelikeWeaponType weaponType, out Vector3 scale, out Color color)
        {
            switch (weaponType)
            {
                case RoguelikeWeaponType.SpinningBlade:
                    scale = new Vector3(0.34f, 0.10f, 1f);
                    color = new Color(0.72f, 0.95f, 1f, 1f);
                    break;
                case RoguelikeWeaponType.PiercingDart:
                    scale = new Vector3(0.44f, 0.09f, 1f);
                    color = new Color(0.95f, 0.62f, 1f, 1f);
                    break;
                case RoguelikeWeaponType.StarRingPulse:
                    scale = new Vector3(0.22f, 0.22f, 1f);
                    color = new Color(0.42f, 1f, 0.72f, 1f);
                    break;
                default:
                    scale = new Vector3(0.28f, 0.14f, 1f);
                    color = new Color(1f, 0.82f, 0.24f, 1f);
                    break;
            }
        }

        private Transform CreateActor(string name, Vector3 position, Color color, int order)
        {
            Transform body = CreateSprite(name, position, new Vector3(0.75f, 0.75f, 1f), color, order);
            CreateSprite("脸", new Vector3(0f, 0.05f, -0.1f), new Vector3(0.42f, 0.26f, 1f), new Color(0.92f, 0.86f, 0.68f, 1f), order + 1, body);
            CreateSprite("眼睛左", new Vector3(-0.10f, 0.08f, -0.2f), new Vector3(0.06f, 0.06f, 1f), Color.black, order + 2, body);
            CreateSprite("眼睛右", new Vector3(0.10f, 0.08f, -0.2f), new Vector3(0.06f, 0.06f, 1f), Color.black, order + 2, body);
            return body;
        }

        private Transform SpawnEnemyView(string name, Vector3 position)
        {
            Transform view = TakeFromPool(_enemyViewPool);
            if (view == null)
            {
                view = CreateActor(name, position, new Color(0.95f, 0.26f, 0.20f, 1f), 9);
            }
            else
            {
                PreparePooledView(view, name, position, Vector3.one, Quaternion.identity);
                PrepareSpriteRenderer(view, new Color(0.95f, 0.26f, 0.20f, 1f), 9);
            }

            Rigidbody2D body = view.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = view.gameObject.AddComponent<Rigidbody2D>();
            }

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.position = (Vector2)position;

            CircleCollider2D collider = view.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = view.gameObject.AddComponent<CircleCollider2D>();
            }

            collider.radius = 0.5f;
            collider.isTrigger = true;

            RoguelikeEnemyMotor motor = view.GetComponent<RoguelikeEnemyMotor>();
            if (motor == null)
            {
                motor = view.gameObject.AddComponent<RoguelikeEnemyMotor>();
            }

            motor.SetTarget((Vector2)position);
            return view;
        }

        private static Color GetEnemyColor(RoguelikeSurvivalEnemy enemy)
        {
            return enemy != null && enemy.IsBoss
                ? new Color(0.72f, 0.22f, 1f, 1f)
                : new Color(0.95f, 0.26f, 0.20f, 1f);
        }

        private static float GetEnemyScale(RoguelikeSurvivalEnemy enemy, float healthRate)
        {
            return enemy != null && enemy.IsBoss
                ? Mathf.Lerp(1.2f, 1.55f, healthRate)
                : Mathf.Lerp(0.55f, 0.8f, healthRate);
        }

        private Transform SpawnSpriteView(Stack<Transform> pool, string name, Vector3 position, Vector3 scale, Color color, int order)
        {
            Transform view = TakeFromPool(pool);
            if (view == null)
            {
                return CreateSprite(name, position, scale, color, order);
            }

            PreparePooledView(view, name, position, scale, Quaternion.identity);
            PrepareSpriteRenderer(view, color, order);
            return view;
        }

        private Transform TakeFromPool(Stack<Transform> pool)
        {
            while (pool.Count > 0)
            {
                Transform view = pool.Pop();
                if (view == null)
                {
                    continue;
                }

                ViewReuseCount++;
                return view;
            }

            return null;
        }

        private void RecycleView(Transform view, Stack<Transform> pool)
        {
            if (view == null)
            {
                return;
            }

            view.gameObject.SetActive(false);
            view.SetParent(transform, false);
            pool.Push(view);
        }

        private void PreparePooledView(Transform view, string name, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            view.name = name;
            view.SetParent(transform, false);
            view.localPosition = position;
            view.localRotation = rotation;
            view.localScale = scale;
            view.gameObject.SetActive(true);
        }

        private static void PrepareSpriteRenderer(Transform view, Color color, int order)
        {
            SpriteRenderer renderer = view.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = view.gameObject.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = GetWhiteSprite();
            renderer.color = color;
            renderer.sortingOrder = order;
        }

        private Transform CreateSprite(string name, Vector3 position, Vector3 scale, Color color, int order, Transform parent = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : transform, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = GetWhiteSprite();
            renderer.color = color;
            renderer.sortingOrder = order;
            return go.transform;
        }

        private static Sprite GetWhiteSprite()
        {
            if (_whiteSprite != null)
            {
                return _whiteSprite;
            }

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            _whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return _whiteSprite;
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }

    public sealed class RoguelikeRuntimeEffectView : MonoBehaviour
    {
        private SpriteRenderer[] _renderers;
        private Color[] _sourceColors;
        private Color[] _baseColors;
        private Vector3 _baseScale;
        private float _duration;
        private float _remaining;

        public RoguelikeEffectCueType Type { get; private set; }

        public void Play(RoguelikeEffectCueType type, float duration, Vector3 baseScale)
        {
            Type = type;
            _duration = Mathf.Max(0.01f, duration);
            _remaining = _duration;
            _baseScale = baseScale;
            transform.localScale = _baseScale;
            CacheRenderers();
            ApplyProgress(0f);
        }

        public bool Tick(float dt)
        {
            _remaining = Mathf.Max(0f, _remaining - Mathf.Max(0f, dt));
            float progress = 1f - Mathf.Clamp01(_remaining / _duration);
            ApplyProgress(progress);
            return _remaining > 0f;
        }

        private void ApplyProgress(float progress)
        {
            float alpha = Mathf.Lerp(1f, 0f, progress);
            float scale = Mathf.Lerp(1f, GetScaleEnd(), progress);
            transform.localScale = _baseScale * scale;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    continue;
                }

                Color color = _baseColors[i];
                color.a *= alpha;
                _renderers[i].color = color;
            }
        }

        private float GetScaleEnd()
        {
            switch (Type)
            {
                case RoguelikeEffectCueType.Kill:
                    return 1.85f;
                case RoguelikeEffectCueType.Pickup:
                    return 1.55f;
                default:
                    return 1.32f;
            }
        }

        private void CacheRenderers()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            if (_sourceColors == null || _sourceColors.Length != _renderers.Length)
            {
                _sourceColors = new Color[_renderers.Length];
                for (int i = 0; i < _renderers.Length; i++)
                {
                    _sourceColors[i] = _renderers[i].color;
                }
            }

            _baseColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _baseColors[i] = _sourceColors[i];
                _renderers[i].color = _sourceColors[i];
            }
        }
    }

    public sealed class RoguelikePlayerMotor : MonoBehaviour
    {
        private Rigidbody2D _body;
        private RoguelikeGame _game;
        private RoguelikeRunState _run;

        public void SetGame(RoguelikeGame game)
        {
            _game = game;
            SyncRunPosition();
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_game == null)
            {
                return;
            }

            if (SyncRunPosition() || !_game.InRealtimeCombat || _game.IsPaused)
            {
                return;
            }

            Vector2 target = _body.position + _game.MoveInput.normalized * _game.MoveSpeed * Time.fixedDeltaTime;
            target.x = Mathf.Clamp(target.x, -RoguelikeGame.ArenaHalfWidth, RoguelikeGame.ArenaHalfWidth);
            target.y = Mathf.Clamp(target.y, -RoguelikeGame.ArenaHalfHeight, RoguelikeGame.ArenaHalfHeight);
            target = _game.ResolveObstaclePosition(target, 0.48f);
            _body.MovePosition(target);
            _game.SyncPlayerPosition(target);
        }

        private bool SyncRunPosition()
        {
            if (_body == null || _game == null || _run == _game.CurrentRun)
            {
                return false;
            }

            _run = _game.CurrentRun;
            _body.position = _game.PlayerPosition;
            return true;
        }
    }

    public sealed class RoguelikeCameraFollow : MonoBehaviour
    {
        private Transform _target;
        private float _shake;

        public float LastShakeMagnitude { get; private set; }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void SetShake(float shake)
        {
            _shake = shake;
            LastShakeMagnitude = CalculateShakeMagnitude(_shake);
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 targetPosition = new Vector3(_target.position.x, _target.position.y, -10f);
            LastShakeMagnitude = CalculateShakeMagnitude(_shake);
            if (LastShakeMagnitude > 0f)
            {
                float phase = Time.time * 55f;
                targetPosition += new Vector3(Mathf.Sin(phase), Mathf.Cos(phase * 1.3f), 0f) * LastShakeMagnitude;
            }

            transform.position = Vector3.Lerp(transform.position, targetPosition, 10f * Time.deltaTime);
        }

        private static float CalculateShakeMagnitude(float shake)
        {
            return Mathf.Clamp01(shake / 0.16f) * 0.08f;
        }
    }

    public sealed class RoguelikeEnemyMotor : MonoBehaviour
    {
        private Rigidbody2D _body;
        private Vector2 _target;

        public void SetTarget(Vector2 target)
        {
            _target = target;
            Rigidbody2D body = GetBody();
            if (body != null)
            {
                body.position = target;
            }
            else
            {
                transform.localPosition = target;
            }
        }

        private void Awake()
        {
            GetBody();
        }

        private void FixedUpdate()
        {
            Rigidbody2D body = GetBody();
            if (body != null)
            {
                body.MovePosition(_target);
            }
            else
            {
                transform.localPosition = _target;
            }
        }

        private Rigidbody2D GetBody()
        {
            if (_body != null)
            {
                return _body;
            }

            _body = GetComponent<Rigidbody2D>();
            return _body;
        }
    }
}
