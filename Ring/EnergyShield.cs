using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;
using static PowerRing.PowerRing;

namespace PowerRing;

internal partial class PowerRing : WearableObject // Turret raycast: Player, Room, InteractableObject, Colliders, PlayerRagdoll
{
    private void EnergyShieldKeyPressed(InputAction.CallbackContext context)
    {
        if (!CanUseRing() || !AbilityEnabled[(int)Ability.EnergyShield]) { return; }

        if (AbilityToggle[(int)Ability.EnergyShield])
        {
            if (context.canceled) { return; }

            CastAbilitySynced(() =>
            {
                return;
            });
        }
        else
        {

        }
    }

    public void CastEnergyShield()
    {

    }

    public void EnergyShieldUpdate()
    {

    }
}

internal class EnergyShield : PowerRingConstruct
{
    public override Ability Ability => Ability.EnergyShield;

    public override void OnActivate()
    {
        return;
    }

    public override void OnDeactivate()
    {
        throw new NotImplementedException();
    }
}