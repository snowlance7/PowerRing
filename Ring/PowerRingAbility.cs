using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;
using static PowerRing.Plugin;
using UnityEngine;
using SnowyLib;
using System;
using static PowerRing.PowerRing;

namespace PowerRing
{
    internal abstract class PowerRingAbility : NetworkBehaviour
    {
        public static List<PowerRingAbility> Instances = new List<PowerRingAbility>();

        public abstract PowerRing.Ability Ability { get; }
        public float PowerUsageOnCast => AbilityPowerUsageOnCast[(int)Ability];
        public float PowerUsagePerSecond => AbilityPowerUsagePerSecond[(int)Ability];
        public float Duration => AbilityDuration[(int)Ability];

        public PowerRing? ring;

        float timeActivated;
        public bool IsActive { get; private set; }

        [StaticInit]
        public static void Init()
        {

        }

        private static void InitConfigs()
        {
            foreach (var ability in Enum.GetValues(typeof(PowerRing.Ability)))
            {

            }
        }

        public virtual void Start()
        {
            Instances.Add(this);
        }

        public override void OnDestroy()
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

        public void ActivatePower(PowerRing sourceRing)
        {
            ring = sourceRing;
            IsActive = true;
            timeActivated = 0f;
            ring.UsePower(PowerUsageOnCast);
            ring.SetCooldown(Ability);
            ring.powerUsage += PowerUsagePerSecond;
            OnActivatePower();
        }

        public void DeactivatePower()
        {
            IsActive = false;
            ring?.powerUsage -= PowerUsagePerSecond;
            ring = null;
            OnDeactivatePower();
        }

        public abstract void OnUsedUpPower();

        public abstract void OnActivatePower();

        public abstract void OnDeactivatePower();

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void ActivatePowerRpc(NetworkObjectReference powerRingRef)
        {
            if (!powerRingRef.TryGet(out NetworkObject netObj)) { logger.LogError("ActivatePowerRpc: Unable to get networkobject from networkobjectreference"); return; }
            ActivatePower(netObj.GetComponent<PowerRing>());
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void DeactivatePowerRpc()
        {
            DeactivatePower();
        }
    }
}
