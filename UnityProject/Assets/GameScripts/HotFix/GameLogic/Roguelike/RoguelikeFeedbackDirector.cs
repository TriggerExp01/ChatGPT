using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public sealed class RoguelikeFeedbackDirector
    {
        private const int MaxEffectCueCount = 96;

        private readonly Dictionary<string, float> _soundCooldowns = new Dictionary<string, float>();
        private readonly List<string> _soundCooldownKeys = new List<string>(8);

        public float AttackFlash { get; private set; }
        public float PickupFlash { get; private set; }
        public float CameraShake { get; private set; }

        public void Reset()
        {
            _soundCooldowns.Clear();
            _soundCooldownKeys.Clear();
            AttackFlash = 0f;
            PickupFlash = 0f;
            CameraShake = 0f;
        }

        public void Tick(float dt)
        {
            float deltaTime = Mathf.Max(0f, dt);
            TickSoundCooldowns(deltaTime);
            AttackFlash = Mathf.Max(0f, AttackFlash - deltaTime);
            PickupFlash = Mathf.Max(0f, PickupFlash - deltaTime);
            CameraShake = Mathf.Max(0f, CameraShake - deltaTime);
        }

        public void TickSoundCooldowns(float dt)
        {
            UpdateSoundCooldowns(Mathf.Max(0f, dt));
        }

        public void SetAttackFlash(float value)
        {
            AttackFlash = Mathf.Max(AttackFlash, Mathf.Max(0f, value));
        }

        public void SetPickupFlash(float value)
        {
            PickupFlash = Mathf.Max(PickupFlash, Mathf.Max(0f, value));
        }

        public void TriggerCameraShake(float duration)
        {
            CameraShake = Mathf.Max(CameraShake, Mathf.Max(0f, duration));
        }

        public void AddEffectCue(List<RoguelikeEffectCue> effectCues, ref int nextSequence, RoguelikeEffectCueType type, Vector2 position, int damage = 0, bool isCritical = false)
        {
            if (effectCues == null)
            {
                return;
            }

            effectCues.Add(new RoguelikeEffectCue(nextSequence++, type, position, damage, isCritical));
            if (effectCues.Count > MaxEffectCueCount)
            {
                effectCues.RemoveRange(0, effectCues.Count - MaxEffectCueCount);
            }
        }

        public void PlaySound(string path, float volume, float cooldown)
        {
            if (string.IsNullOrEmpty(path) || GetSoundCooldown(path) > 0f)
            {
                return;
            }

            try
            {
                GameModule.Audio.Play(TEngine.AudioType.Sound, path, false, Mathf.Clamp01(volume), true);
                SetSoundCooldown(path, cooldown);
            }
            catch (Exception)
            {
                SetSoundCooldown(path, 0.2f);
            }
        }

        public float GetSoundCooldown(string path)
        {
            return !string.IsNullOrEmpty(path) && _soundCooldowns.TryGetValue(path, out float value) ? value : 0f;
        }

        public void SetSoundCooldown(string path, float cooldown)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            _soundCooldowns[path] = Mathf.Max(0.01f, cooldown);
        }

        private void UpdateSoundCooldowns(float dt)
        {
            if (_soundCooldowns.Count <= 0)
            {
                return;
            }

            _soundCooldownKeys.Clear();
            foreach (string key in _soundCooldowns.Keys)
            {
                _soundCooldownKeys.Add(key);
            }

            for (int i = 0; i < _soundCooldownKeys.Count; i++)
            {
                string key = _soundCooldownKeys[i];
                float remaining = Mathf.Max(0f, _soundCooldowns[key] - dt);
                if (remaining <= 0f)
                {
                    _soundCooldowns.Remove(key);
                }
                else
                {
                    _soundCooldowns[key] = remaining;
                }
            }
        }
    }
}
