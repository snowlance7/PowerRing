using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public AudioClip greenLightActivateSFX = null!;
    public AudioClip greenLightDeactivateSFX = null!;

    private void GreenLightKeyPressed(InputAction.CallbackContext context) // Green Light
    {
        if (!CanUseRing() || !AbilityEnabled[(int)Ability.GreenLight]) { return; }

        if (AbilityToggle[(int)Ability.GreenLight])
        {
            if (greenLightObj.activeSelf)
            {
                ToggleGreenLight(false);
                return;
            }

            CastAbilitySynced(() =>
            {
                if (!CanCastAbility(Ability.GreenLight)) { CastAbilityFailedRpc(); return; }

                ToggleGreenLight(true);

            });
        }
        else
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.GreenLight)) { CastAbilityFailedRpc(); return; }

                    ToggleGreenLight(true);

                });
            }
            else
            {
                ToggleGreenLight(false);
            }
        }
    }

    public void ToggleGreenLight(bool value)
    {
        if (greenLightObj.activeSelf == value) { return; }

        if (value)
        {
            UsePower(AbilityPowerUsageOnCast[(int)Ability.GreenLight]);
            SetCooldown(Ability.GreenLight);
            powerUsage += AbilityPowerUsagePerSecond[(int)Ability.GreenLight];
            greenLightObj.SetActive(true);
        }
        else
        {
            greenLightObj.SetActive(false);
            powerUsage -= AbilityPowerUsagePerSecond[(int)Ability.GreenLight];
            StopCastAnimationRpc();
        }
    }
}