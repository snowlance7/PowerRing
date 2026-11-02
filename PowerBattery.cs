using Dawn;
using System.Linq;

namespace PowerRing;

internal class PowerBattery : PhysicsProp
{
    public static bool IsEnabled => LethalContent.Unlockables[PowerRingKeys.PowerBattery] != null;

    public InteractTrigger interactTrigger = null!;

    public override void Update()
    {
        base.Update();
        interactTrigger.interactable = PowerRing.Instances.Any(x => x.CanUseRing());
    }

    public void ChargeRing() // InteractTrigger
    {
        var ring = PowerRing.Instances.FirstOrDefault(x => x.CanUseRing());
        if (ring == null) { return; }

        ring.QuickCastAbilitySynced(() =>
        {
            ring.ChargeRingRpc();
        });
    }
}