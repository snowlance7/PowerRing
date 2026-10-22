using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    private void EnergyConstructKeyPressed(InputAction.CallbackContext context) // Energy Construct
    {
        if (playerWornBy == null || playerWornBy != localPlayer) { return; }
    }
}

internal class EnergyConstruct : PowerRingAbility
{
    public override PowerRing.Ability Ability => PowerRing.Ability.EnergyConstruct;

    public override void OnActivatePower()
    {
        throw new NotImplementedException();
    }

    public override void OnDeactivatePower()
    {
        throw new NotImplementedException();
    }

    public override void OnUsedUpPower()
    {
        throw new NotImplementedException();
    }
}