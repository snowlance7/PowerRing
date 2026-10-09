using Dawn;
using GameNetcodeStuff;
using HarmonyLib;
using PowerRing.ProceduralForceField;
using PowerRing.Ring;
using SnowyLib;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool BubbleShieldToggle => PluginInstance.Config.Bind("Bubble Shield", "Toggle", true, "If the bubble shield ability should toggle or require holding the button down to work.").Value;
    public static float BubbleShieldPowerUsageOnCast => PluginInstance.Config.Bind("Bubble Shield", "Power Usage On Cast", 0f, "How much power the bubble shield ability consumes when activated.").Value;
    public static float BubbleShieldPowerUsagePerSecond => PluginInstance.Config.Bind("Bubble Shield", "Power Usage Per Second", 0f, "How much power the bubble shield ability consumes every second while active.").Value;
    public static bool BubbleShieldDeactivateOnDiscard => PluginInstance.Config.Bind("Bubble Shield", "Deactivate On Discard", true, "If the bubble shield should deactivate when the ring is discarded or pocketed.").Value;
    public static float BubbleShieldDuration => PluginInstance.Config.Bind("Bubble Shield", "Duration", 0f, "How long the bubble shield remains active after being activated.").Value;
    public static float BubbleShieldCooldown => PluginInstance.Config.Bind("Bubble Shield", "Cooldown", 0f, "How long before the bubble shield ability can be used again.").Value;

    private void BubbleShieldKeyPressed(InputAction.CallbackContext context) // Bubble Shield
    {
        if (context.canceled) { return; }
        logger.LogDebug("BubbleShieldKeyPressed");
        // TODO: make it so you cast on yourself by default but if you use the point emote, it tries to cast on a player

        if (localPlayer.GetCurrentEmote() == 2)
        {
            logger.LogDebug("Cast");
        }
        else
        {
            logger.LogDebug("Self cast");
            BubbleShieldRpc(localPlayer.actualClientId);
        }

        //throw new NotImplementedException();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void BubbleShieldRpc(ulong clientId)
    {
        logger.LogDebug("BubbleShieldRpc");
        PlayerControllerB player = PlayerFromId(clientId)!;
        var shieldObj = Instantiate(bubbleShieldPrefab, player.transform.position, Quaternion.identity);
        NetworkObject netObj = shieldObj.GetComponent<NetworkObject>();
        netObj.Spawn(destroyWithScene: true);
        netObj.DoActionAfterSpawn((netObj) => netObj.GetComponent<EnergyBubbleShield>().SetRingRpc(NetworkObject, clientId, true));
    }
}

internal class EnergyBubbleShield : PowerRingAbility // TODO: Test this
{
    [SerializeField] ProceduralForceFieldOverlay overlay = null!;

    public override float PowerUsagePerSecond => PowerRing.BubbleShieldPowerUsagePerSecond;

    public override float Duration => PowerRing.BubbleShieldDuration;

    public override void Update()
    {
        base.Update();

        if (player != null) transform.position = player.transform.position;
    }

    public override void OnActivatePower()
    {
        return;
    }

    public override void OnDeactivatePower()
    {
        Despawn();
    }

    public override void OnUsedUpPower()
    {
        Despawn();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void HitRpc(Vector3 position)
    {
        logger.LogDebug("HitRpc");
        overlay.Trigger(position);
    }

    public static EnergyBubbleShield? GetBubbleShield(PlayerControllerB player)
    {
        return Instances.OfType<EnergyBubbleShield>().FirstOrDefault(x => x.player == player);
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

            var shield = EnergyBubbleShield.GetBubbleShield(__instance);
            if (shield == null) { return true; }
            Vector3 hitPoint = enemyColliding.eye != null ? enemyColliding.eye.position : enemyColliding.transform.position;
            shield.HitRpc(hitPoint);
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
            if (EnergyBubbleShield.GetBubbleShield(__instance) == null) { return; }
            __result = false;
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }
}