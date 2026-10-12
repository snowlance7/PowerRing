using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

namespace PowerRing.Ring
{
    internal abstract class PowerRingAbility : MonoBehaviour
    {
        public static List<PowerRingAbility> Instances = new List<PowerRingAbility>();

        [HideInInspector]
        public PlayerControllerB? player;
        [HideInInspector]
        public EnemyAI? enemy;

        public abstract float PowerUsagePerSecond { get; }
        public abstract float Duration { get; }

        PowerRing? ring;

        float timeActivated;
        public bool IsActive { get; private set; }

        public virtual void Start()
        {
            Instances.Add(this);
        }

        public virtual void OnDestroy()
        {
            Instances.Remove(this);
        }

        public virtual void Update()
        {
            if (IsActive && Duration > 0)
            {
                timeActivated += Time.deltaTime;

                if (timeActivated > Duration)
                {
                    DeactivatePower();
                    return;
                }
            }

            if (ring == null) { return; }

            if (ring.power <= 0)
            {
                DeactivatePower();
                OnUsedUpPower();
                return;
            }
        }

        public void ActivatePower()
        {
            IsActive = true;
            timeActivated = 0f;
            ring?.powerUsage += PowerUsagePerSecond;
            OnActivatePower();
        }

        public void DeactivatePower()
        {
            IsActive = false;
            ring?.powerUsage -= PowerUsagePerSecond;
            OnDeactivatePower();
        }

        public abstract void OnUsedUpPower();

        public abstract void OnActivatePower();

        public abstract void OnDeactivatePower();

        public void SetRing(PowerRing ring, bool activate = false)
        {
            this.ring = ring;

            if (activate) ActivatePower();
        }

        public void SetRing(PowerRing ring, PlayerControllerB player, bool activate = false)
        {
            this.ring = ring;
            this.player = player;

            if (activate) ActivatePower();
        }

        public void SetRing(PowerRing ring, EnemyAI enemy, bool activate = false)
        {
            this.ring = ring;
            this.enemy = enemy;

            if (activate) ActivatePower();
        }
    }
}
