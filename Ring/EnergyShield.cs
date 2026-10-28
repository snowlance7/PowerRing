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

        if (AbilityToggle[(int)Ability.EnergyShield]) // SELF CAST TOGGLE
        {
            if (context.canceled) { return; }

            QuickCastAbilitySynced(() =>
            {
                var shield = localPlayer.GetConstruct<EnergyShield>();
                if (shield != null && shield.IsActivated)
                {
                    shield.DeactivateRpc();
                    return;
                }

                if (!CanCastAbility(Ability.EnergyShield)) { CastAbilityFailedRpc(); return; }
                CastEnergyShield();
            });
        }
        else // SELF CAST HOLD
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.EnergyShield)) { CastAbilityFailedRpc(); return; }
                    CastEnergyShield();
                }, doPlayerCastAnimation: false);
            }
            else
            {
                StopCastAnimationRpc();

                var shield = localPlayer.GetConstruct<EnergyShield>();
                if (shield != null && shield.IsActivated)
                {
                    shield.DeactivateRpc();
                }
            }
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