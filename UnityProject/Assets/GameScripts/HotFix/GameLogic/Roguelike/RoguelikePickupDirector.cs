using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public sealed class RoguelikePickupDirector
    {
        private const float PickupMoveSpeed = 7f;
        private const float PickupStopDistance = 0.05f;
        private const float PickupCollectDistance = 0.65f;

        public sealed class Context
        {
            public IList<RoguelikeSurvivalPickup> Pickups { get; set; }
            public Vector2 PlayerPosition { get; set; }
            public float AttractRadius { get; set; }
            public Action<int> GainExperience { get; set; }
            public Action<int> AddGold { get; set; }
            public Func<int, int> ScaleGoldPickupAmount { get; set; }
            public Action<RoguelikeSurvivalPickup> ReleasePickup { get; set; }
            public Action<Vector2> OnPickupCollected { get; set; }
        }

        public void UpdatePickups(Context context, float dt)
        {
            IList<RoguelikeSurvivalPickup> pickups = context?.Pickups;
            if (pickups == null || pickups.Count <= 0)
            {
                return;
            }

            float deltaTime = Mathf.Max(0f, dt);
            for (int i = pickups.Count - 1; i >= 0; i--)
            {
                RoguelikeSurvivalPickup pickup = pickups[i];
                if (pickup == null)
                {
                    pickups.RemoveAt(i);
                    continue;
                }

                if (!TryCollect(context, pickup, deltaTime))
                {
                    continue;
                }

                pickups.RemoveAt(i);
                context.ReleasePickup?.Invoke(pickup);
            }
        }

        private static bool TryCollect(Context context, RoguelikeSurvivalPickup pickup, float dt)
        {
            Vector2 offset = context.PlayerPosition - pickup.Position;
            float distance = offset.magnitude;
            if (distance < context.AttractRadius && distance > PickupStopDistance)
            {
                pickup.Position += offset.normalized * PickupMoveSpeed * dt;
            }

            if (distance > PickupCollectDistance)
            {
                return false;
            }

            if (pickup.Type == RoguelikePickupType.Experience)
            {
                context.GainExperience?.Invoke(pickup.Amount);
            }
            else
            {
                int amount = context.ScaleGoldPickupAmount?.Invoke(pickup.Amount) ?? pickup.Amount;
                context.AddGold?.Invoke(amount);
            }

            context.OnPickupCollected?.Invoke(pickup.Position);
            return true;
        }
    }
}
