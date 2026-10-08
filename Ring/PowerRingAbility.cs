using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PowerRing.Ring
{
    internal abstract class PowerRingAbility : NetworkBehaviour
    {
        public static List<PowerRingAbility> Instances = new List<PowerRingAbility>();

        public abstract float PowerUsagePerSecond { get; }
        public abstract float Duration { get; }

        PowerRing? ring;

        float timeActivated;
        public bool IsActive { get; private set; }

        public override void OnNetworkPostSpawn()
        {
            base.OnNetworkPostSpawn();
            Instances.Add(this);
        }

        public override void OnNetworkDespawn()
        {
            Instances.Remove(this);
            base.OnNetworkDespawn();
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

        public void Despawn()
        {
            if (!IsServer) { return; }
            NetworkObject.Despawn(destroy: true);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void SetRingRpc(NetworkObjectReference netRef, bool activate = false)
        {
            netRef.TryGet(out NetworkObject netObj);
            ring = netObj.GetComponent<PowerRing>();

            if (activate) ActivatePower();
        }
    }
}
