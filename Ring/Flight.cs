using EnergyShieldEffect;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static readonly bool FlightHover = PluginInstance.Config.Bind("Flight", "Hover", true, "If the flight ability should allow hovering instead of requiring continuous movement.").Value;
    public static readonly float FlightSpeed = PluginInstance.Config.Bind("Flight", "Speed", 1f, "How fast the player should move while flying").Value;

    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Sprinting = Animator.StringToHash("Sprinting");
    private static readonly int Sideways = Animator.StringToHash("Sideways");
    private static readonly int Crouching = Animator.StringToHash("Crouching");
    private static readonly int Jumping = Animator.StringToHash("Jumping");

    [HideInInspector] public bool isFlying;
    public AudioClip flightActivateSFX = null!;
    public AudioClip flightDeactivateSFX = null!;

    private void FlightKeyPressed(InputAction.CallbackContext context) // Flight
    {
        if (!CanUseRing() || !AbilityEnabled[(int)Ability.Flight]) { return; }

        if (AbilityToggle[(int)Ability.Flight])
        {
            if (isFlying)
            {
                ToggleFlight(false);
                return;
            }

            CastAbilitySynced(() =>
            {
                if (!CanCastAbility(Ability.Flight)) { CastAbilityFailedRpc(); return; }

                ToggleFlight(true);

            }, doPlayerCastAnimation: false);
        }
        else
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.Flight)) { CastAbilityFailedRpc(); return; }

                    ToggleFlight(true);

                }, doPlayerCastAnimation: false);
            }
            else
            {
                ToggleFlight(false);
            }
        }
    }

    public void ToggleFlight(bool value)
    {
        if (isFlying == value) { return; }

        if (value)
        {
            UsePower(AbilityPowerUsageOnCast[(int)Ability.Flight]);
            SetCooldown(Ability.Flight);
            powerUsage += AbilityPowerUsagePerSecond[(int)Ability.Flight];
            isFlying = true;
        }
        else
        {
            isFlying = false;
            powerUsage -= AbilityPowerUsagePerSecond[(int)Ability.Flight];
            StopCastAnimationRpc();
        }
    }

    public void FlightUpdate()
    {
        if (playerWornBy == null) { return; }
        if (!playerWornBy.quickMenuManager.isMenuOpen && !playerWornBy.inTerminalMenu && !playerWornBy.isTypingChat && !playerWornBy.isClimbingLadder && !playerWornBy.inSpecialInteractAnimation && !playerWornBy.jetpackControls)
        {
            Vector2 vector = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
            //float ascendingAmount = (Imperium.PlayerManager.FlyIsAscending ? IngamePlayerSettings.Instance.playerInput.actions.FindAction("Jump").ReadValue<float>() : 0f);
            //float descendingAmount = (Imperium.PlayerManager.FlyIsDescending ? IngamePlayerSettings.Instance.playerInput.actions.FindAction("Crouch").ReadValue<float>() : 0f);
            float ascendingAmount = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Jump").ReadValue<float>();
            float descendingAmount = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Crouch").ReadValue<float>();
            ascendingAmount *= FlightSpeed * 1.5f;
            descendingAmount *= FlightSpeed * 1.5f;
            Vector3 vector2 = new Vector3(vector.x * FlightSpeed, ascendingAmount - descendingAmount, vector.y * FlightSpeed);
            vector2 = Quaternion.AngleAxis(playerWornBy.transform.eulerAngles.y, Vector3.up) * vector2;
            playerWornBy.externalForces += vector2;
        }
        playerWornBy.fallValue = 0f;
        playerWornBy.fallValueUncapped = 0f;
        playerWornBy.playerBodyAnimator.SetBool(Walking, value: false);
        playerWornBy.playerBodyAnimator.SetBool(Sprinting, value: false);
        playerWornBy.playerBodyAnimator.SetBool(Sideways, value: false);
        playerWornBy.playerBodyAnimator.SetBool(Crouching, value: false);
        playerWornBy.playerBodyAnimator.SetBool(Jumping, value: true);
        playerWornBy.isCrouching = false;
    }
}

[HarmonyPatch]
internal static class FlightPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
    static void PlayerControllerB_Update_Prefix(PlayerControllerB __instance) // TODO: Test this
    {
        try
        {
            if (localPlayer != __instance) { return; }
            PowerRing? ring = __instance.GetWearables().FirstOrDefault(x => x is PowerRing) as PowerRing;
            if (ring == null || !ring.isFlying) { return; }
            ring.FlightUpdate();
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }
}