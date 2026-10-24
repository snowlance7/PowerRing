using FXV;
using GameNetcodeStuff;
using HarmonyLib;
using SnowyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;
using static PowerRing.PowerRing;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool ShowBubbleShieldOnClient => PluginInstance.Config.Bind("Bubble Shield", "Show Shield On Client", true, "If the shield effect should be visible on the client. Does not prevent you from seeing other players shield, just your own.").Value;
    public static int MaxSharedBubbleShields => PluginInstance.Config.Bind("Bubble Shield", "Max Shared Shields", 2, "The max amount of shared bubble shields that can be active at once. If AbilityToggle is off, this is always 1.").Value;

    public const float bubbleShieldCastDistance = 20f;

    private void BubbleShieldKeyPressed(InputAction.CallbackContext context) // SELF CAST
    {
        if (!CanUseRing() || !AbilityEnabled[(int)Ability.BubbleShield]) { return; }

        if (AbilityToggle[(int)Ability.BubbleShield]) // SELF CAST TOGGLE
        {
            if (context.canceled) { return; }

            QuickCastAbilitySynced(() =>
            {
                var shield = localPlayer.GetConstruct<BubbleShield>();
                if (shield != null && shield.IsActivated)
                {
                    shield.DeactivateRpc();
                    return;
                }

                if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }
                CastBubbleShield(localPlayer);
            }, doPlayerCastAnimation: false);
        }
        else // SELF CAST HOLD
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }
                    CastBubbleShield(localPlayer);
                }, doPlayerCastAnimation: false);
            }
            else
            {
                StopCastAnimationRpc();

                var shield = localPlayer.GetConstruct<BubbleShield>();
                if (shield != null && shield.IsActivated)
                {
                    shield.DeactivateRpc();
                }
            }
        }
    }

    private void SharedBubbleShieldKeyPressed(InputAction.CallbackContext context) // SHARED CAST
    {
        if (!CanUseRing() || !AbilityEnabled[(int)Ability.BubbleShield]) { return; }

        logger.LogDebug("Shared cast");

        if (AbilityToggle[(int)Ability.BubbleShield]) // SHARED CAST TOGGLE
        {
            if (context.canceled) { return; }

            QuickCastAbilitySynced(() =>
            {
                PlayerControllerB? player = GetPlayerLookingAt(bubbleShieldCastDistance);
                if (player == null) { CastAbilityFailedRpc(); return; }

                var shield = player.GetConstruct<BubbleShield>();
                if (shield != null)
                {
                    shield.DeactivateRpc();
                    return;
                }

                if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }

                CastBubbleShield(player);
            });
        }
        else // SHARED CAST HOLD
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }

                    PlayerControllerB? player = GetPlayerLookingAt(bubbleShieldCastDistance);
                    if (player == null) { CastAbilityFailedRpc(); return; }

                    CastBubbleShield(player);
                });
            }
            else
            {
                StopCastAnimationRpc();
                lastConstruct?.DeactivateRpc(); // TODO: Test this
            }
        }
    }

    private void CastBubbleShield(PlayerControllerB player)
    {
        if (player != playerWornBy && AbilityToggle[(int)Ability.BubbleShield])
        {
            var otherShields = PowerRingConstruct.Instances.Where(x => x is BubbleShield && x.ring == this && x.player != playerWornBy);

            if (otherShields.Count() >= MaxSharedBubbleShields)
            {
                otherShields.First().DeactivateRpc();
            }
        }

        SpawnBubbleShieldRpc(player.actualClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SpawnBubbleShieldRpc(ulong shieldedClientId)
    {
        PlayerControllerB player = PlayerFromId(shieldedClientId)!;
        var shieldObj = Instantiate(bubbleShieldPrefab, player.transform);
        shieldObj.GetComponent<NetworkObject>().Spawn(destroyWithScene: true, (netObj) =>
        {
            netObj.GetComponent<BubbleShield>().ActivateRpc(this.NetworkObject, player.actualClientId);
        });
    }
}

internal class BubbleShield : PowerRingConstruct, IHittable // TODO: Test this // TODO: Works but hit effect doesnt show correctly
{
    Collider collider = null!;

    const float YOffset = 1.2f;
    public Transform? parentObject => player?.transform;

    public override Ability Ability => Ability.BubbleShield;

    public override void Awake()
    {
        base.Awake();
        collider = gameObject.GetComponent<Collider>();
    }

    public void LateUpdate()
    {
        if (parentObject != null)
        {
            base.transform.position = parentObject.position;
            Vector3 _positionOffset = new Vector3(0f, YOffset, 0f);
            _positionOffset = parentObject.rotation * _positionOffset;
            base.transform.position += _positionOffset;
        }
    }

    public override void OnActivate()
    {
        if (localPlayer != player || ShowBubbleShieldOnClient) { return; }
        shield.SetShieldVisuallyActive(ShowBubbleShieldOnClient);
    }

    public override void OnDeactivate()
    {
        return;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void HitRpc(Vector3 hitPos, Vector3 hitDirection, float hitScale, float hitDuration)
    {
        logger.LogDebug("HitRpc");
        shield.OnHit(collider.ClosestPoint(hitPos), hitDirection, hitScale, hitDuration);
    }

    public bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1) // TODO
    {
        logger.LogDebug("ON HIT"); // TODO: Test this
        return true;
    }
}

[HarmonyPatch]
internal static class BubbleShieldPatches
{
    static PlayerControllerB? collidedWith;
    static EnemyAI? enemyColliding;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.OnCollideWithPlayer))]
    static void EnemyAI_OnCollideWithPlayer_Prefix(EnemyAI __instance, Collider other) // TODO: Test this
    {
        try
        {
            if (!__instance.IsServer) { return; }

            if (!other.gameObject.TryGetComponent(out PlayerControllerB player)) { return; }
            collidedWith = player;
            enemyColliding = __instance;
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayerServerRpc))]
    static bool PlayerControllerB_DamagePlayerServerRpc_Prefix(PlayerControllerB __instance) // TODO: Test this
    {
        try
        {
            if (!__instance.IsServer) { return true; }

            if (collidedWith == null || enemyColliding == null) { return true; }
            if (__instance != collidedWith) { return true; }

            var shield = __instance.GetConstruct<BubbleShield>();
            if (shield == null || !shield.IsActivated) { return true; }
            Vector3 hitPoint = enemyColliding.eye != null ? enemyColliding.eye.position : enemyColliding.transform.position;
            shield.HitRpc(hitPoint, (shield.transform.position - hitPoint).normalized, 5f, 1f); // TODO: Test this
            return false;
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.OnCollideWithPlayer))]
    static void EnemyAI_OnCollideWithPlayer_Postfix(EnemyAI __instance, Collider other) // TODO: Test this
    {
        try
        {
            if (!__instance.IsServer) { return; }

            collidedWith = null;
            enemyColliding = null;
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.AllowPlayerDeath))]
    static void PlayerControllerB_AllowPlayerDeath_Postfix(PlayerControllerB __instance, ref bool __result) // TODO: Test this
    {
        try
        {
            var shield = __instance.GetConstruct<BubbleShield>();
            if (shield == null || !shield.IsActivated) { return; }
            __result = false;
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Turret), nameof(Turret.CheckForPlayersInLineOfSight))]
    static void Turret_CheckForPlayersInLineOfSight_Postfix(Turret __instance, ref PlayerControllerB? __result)
    {
        try
        {
            if (__instance.turretMode == TurretMode.Detection || __instance.turretMode == TurretMode.Charging) { return; }
            if (__result != localPlayer) { return; }
            var shield = localPlayer.GetConstruct<BubbleShield>();
            if (shield == null || !shield.IsActivated) { return; }
            shield.HitRpc(__instance.hit.point, __instance.hit.normal, 2.5f, 2f);
            __result = null;
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }
}