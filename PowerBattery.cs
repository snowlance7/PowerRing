using PowerRing;
using Unity.Netcode;
using static PowerRing.Plugin;

namespace PowerRing;

internal class PowerBattery : NetworkBehaviour
{
    public InteractTrigger interactTrigger = null!;

    public void Update()
    {
        interactTrigger.interactable = localPlayer.currentlyHeldObjectServer != null && localPlayer.currentlyHeldObjectServer is PowerRing;
    }

    public void ChargeRing()
    {
        throw new System.NotImplementedException();
    }
}