using Unity.Netcode;
using UnityEngine;

namespace PowerRing.Ring
{
    internal abstract class PowerRingAbility : NetworkBehaviour
    {
        public abstract float PowerUsagePerSecond { get; }
        public abstract float Duration { get; }

        PowerRing? ring;

        float timeActivated;
        bool active;

        public virtual void Update()
        {
            if (active && Duration > 0)
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
            active = true;
            timeActivated = 0f;
            ring?.powerUsage += PowerUsagePerSecond;
            OnActivatePower();
        }

        public void DeactivatePower()
        {
            active = false;
            ring?.powerUsage -= PowerUsagePerSecond;
            OnDeactivatePower();
        }

        public abstract void OnUsedUpPower();

        public abstract void OnActivatePower();

        public abstract void OnDeactivatePower();

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void SetRingRpc(NetworkObjectReference netRef, bool activate = false)
        {
            netRef.TryGet(out NetworkObject netObj);
            ring = netObj.GetComponent<PowerRing>();

            if (activate) ActivatePower();
        }
    }
}
