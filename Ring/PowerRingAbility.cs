using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;
using static PowerRing.Plugin;
using UnityEngine;

namespace PowerRing.Ring
{
    internal abstract class PowerRingAbility : NetworkBehaviour
    {
        public static List<PowerRingAbility> Instances = new List<PowerRingAbility>();

        public abstract float PowerUsagePerSecond { get; }
        public abstract float Duration { get; }

        public PowerRing? ring;

        float timeActivated;
        public bool IsActive { get; private set; }

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
