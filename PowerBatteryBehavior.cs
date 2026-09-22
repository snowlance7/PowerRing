using PowerRing;
using Unity.Netcode;
using static PowerRing.Plugin;

namespace PowerRing;

internal class PowerBatteryBehavior : NetworkBehaviour
{
    public InteractTrigger interactTrigger = null!;

    public void Update()
    {
        interactTrigger.interactable = localPlayer.currentlyHeldObjectServer != null && localPlayer.currentlyHeldObjectServer is PowerRingBehavior;
    }

    public void ChargeRing()
    {
        throw new System.NotImplementedException();
    }
}