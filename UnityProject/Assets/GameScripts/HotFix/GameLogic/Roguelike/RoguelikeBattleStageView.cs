using UnityEngine;

namespace GameLogic
{
    public sealed class RoguelikeBattleStageView : MonoBehaviour
    {
        private const string StageName = "RoguelikeBattleStage";
        private const float LaneToWorldScale = 1.35f;

        private Transform _player;
        private Transform _enemy;
        private Transform _playerHpFill;
        private Transform _enemyHpFill;
        private TextMesh _playerLabel;
        private TextMesh _enemyLabel;
        private TextMesh _roomLabel;
        private Camera _camera;
        private int _lastRoomIndex = -1;

        public static RoguelikeBattleStageView Ensure()
        {
            GameObject existing = GameObject.Find(StageName);
            if (existing != null)
            {
                RoguelikeBattleStageView view = existing.GetComponent<RoguelikeBattleStageView>();
                return view != null ? view : existing.AddComponent<RoguelikeBattleStageView>();
            }

            GameObject root = new GameObject(StageName);
            DontDestroyOnLoad(root);
            RoguelikeBattleStageView created = root.AddComponent<RoguelikeBattleStageView>();
            created.BuildStage();
            return created;
        }

        public void Refresh(RoguelikeRunState run)
        {
            if (run == null)
            {
                return;
            }

            if (_player == null || _enemy == null)
            {
                BuildStage();
            }

            RoguelikeRoom room = run.CurrentRoom;
            bool hasEnemy = room != null && room.Enemy != null && !room.IsCleared;
            if (room != null && room.Index != _lastRoomIndex)
            {
                _lastRoomIndex = room.Index;
                _roomLabel.text = $"Room {room.Index + 1}/{run.Rooms.Count}  {RoguelikeText.GetRoomName(room.Type)}";
            }

            Vector3 playerTarget = new Vector3(RoguelikeGame.Instance.PlayerLanePosition * LaneToWorldScale, 0.95f, 0f);
            _player.position = Vector3.Lerp(_player.position, playerTarget, 18f * Time.deltaTime);
            _player.gameObject.SetActive(run.Player.IsAlive);
            _playerLabel.text = $"Player\nHP {run.Player.Health}/{run.Player.Stats.MaxHealth}";
            SetBar(_playerHpFill, run.Player.Health, run.Player.Stats.MaxHealth);
            PositionLabel(_playerLabel.transform, _player.position + new Vector3(0f, 1.65f, 0f));

            _enemy.gameObject.SetActive(hasEnemy);
            _enemyLabel.gameObject.SetActive(hasEnemy);
            if (hasEnemy)
            {
                Vector3 enemyTarget = new Vector3(RoguelikeGame.Instance.EnemyLanePosition * LaneToWorldScale, 0.95f, 0f);
                _enemy.position = Vector3.Lerp(_enemy.position, enemyTarget, 18f * Time.deltaTime);
                _enemyLabel.text = $"{room.Enemy.DisplayName}\nHP {room.Enemy.Health}/{room.Enemy.Stats.MaxHealth}";
                SetBar(_enemyHpFill, room.Enemy.Health, room.Enemy.Stats.MaxHealth);
                PositionLabel(_enemyLabel.transform, _enemy.position + new Vector3(0f, 1.65f, 0f));
            }

            _camera.transform.position = new Vector3(0f, 7.5f, -10.5f);
            _camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
        }

        private void BuildStage()
        {
            transform.position = Vector3.zero;
            ClearChildren();

            _camera = Camera.main;
            if (_camera == null)
            {
                GameObject cameraObject = new GameObject("MainCamera");
                _camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.08f, 0.09f, 0.10f, 1f);
            _camera.fieldOfView = 45f;
            _camera.transform.SetParent(transform, false);

            GameObject lightObject = new GameObject("KeyLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Arena";
            ground.transform.SetParent(transform, false);
            ground.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            ground.transform.localScale = new Vector3(18f, 0.16f, 5.2f);
            SetMaterial(ground, new Color(0.13f, 0.15f, 0.17f, 1f));

            GameObject lane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lane.name = "CombatLane";
            lane.transform.SetParent(transform, false);
            lane.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            lane.transform.localScale = new Vector3(16f, 0.08f, 0.18f);
            SetMaterial(lane, new Color(0.75f, 0.62f, 0.28f, 1f));

            _player = CreateActor("PlayerActor", new Color(0.18f, 0.55f, 0.95f, 1f), new Vector3(-3f, 0.95f, 0f));
            _enemy = CreateActor("EnemyActor", new Color(0.88f, 0.24f, 0.20f, 1f), new Vector3(3f, 0.95f, 0f));
            _playerLabel = CreateLabel("PlayerLabel", new Vector3(-3f, 2.6f, 0f), TextAnchor.MiddleCenter);
            _enemyLabel = CreateLabel("EnemyLabel", new Vector3(3f, 2.6f, 0f), TextAnchor.MiddleCenter);
            _roomLabel = CreateLabel("RoomLabel", new Vector3(0f, 3.8f, 0f), TextAnchor.MiddleCenter);
            _roomLabel.fontSize = 42;

            _playerHpFill = CreateWorldBar("PlayerHpBar", _player, new Vector3(0f, 1.28f, 0f), new Color(0.20f, 0.85f, 0.34f, 1f));
            _enemyHpFill = CreateWorldBar("EnemyHpBar", _enemy, new Vector3(0f, 1.28f, 0f), new Color(0.95f, 0.25f, 0.20f, 1f));
        }

        private Transform CreateActor(string name, Color color, Vector3 position)
        {
            GameObject actor = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            actor.name = name;
            actor.transform.SetParent(transform, false);
            actor.transform.localPosition = position;
            actor.transform.localScale = new Vector3(0.72f, 0.95f, 0.72f);
            SetMaterial(actor, color);
            return actor.transform;
        }

        private TextMesh CreateLabel(string name, Vector3 position, TextAnchor anchor)
        {
            GameObject labelObject = new GameObject(name);
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = position;
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.anchor = anchor;
            label.alignment = TextAlignment.Center;
            label.fontSize = 34;
            label.characterSize = 0.08f;
            label.color = Color.white;
            return label;
        }

        private Transform CreateWorldBar(string name, Transform parent, Vector3 offset, Color color)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = offset;

            GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "Bg";
            bg.transform.SetParent(root.transform, false);
            bg.transform.localScale = new Vector3(1.35f, 0.08f, 0.08f);
            SetMaterial(bg, new Color(0.06f, 0.06f, 0.06f, 1f));

            GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.name = "Fill";
            fill.transform.SetParent(root.transform, false);
            fill.transform.localPosition = new Vector3(0f, 0.01f, -0.02f);
            fill.transform.localScale = new Vector3(1.28f, 0.09f, 0.09f);
            SetMaterial(fill, color);
            return fill.transform;
        }

        private static void SetBar(Transform fill, int value, int maxValue)
        {
            float rate = Mathf.Clamp01((float)value / Mathf.Max(1, maxValue));
            fill.localScale = new Vector3(1.28f * rate, fill.localScale.y, fill.localScale.z);
            fill.localPosition = new Vector3(-0.64f + 0.64f * rate, fill.localPosition.y, fill.localPosition.z);
        }

        private void PositionLabel(Transform label, Vector3 position)
        {
            label.position = position;
            if (_camera != null)
            {
                label.rotation = _camera.transform.rotation;
            }
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        private static void SetMaterial(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            renderer.sharedMaterial = material;
        }
    }
}
