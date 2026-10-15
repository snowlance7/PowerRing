using FXV;
using GameNetcodeStuff;
using HarmonyLib;
using PowerRing.Ring;
using SnowyLib;
using System;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication.Generated;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;
using static UnityEngine.Rendering.DebugUI;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    BubbleShield? lastBubbleShield;

    private void BubbleShieldKeyPressed(InputAction.CallbackContext context) // Bubble Shield
    {
        logger.LogDebug("Self cast");

        if (AbilityToggle[(int)Ability.BubbleShield])
        {
            if (context.canceled) { return; }

            if (lastBubbleShield != null && lastBubbleShield.IsActive)
            {
                lastBubbleShield.DeactivatePowerRpc();
                return;
            }

            if (!CanCastAbility(Ability.BubbleShield))
            {
                DoCastFailedEffectsRpc();
                return;
            }

            var shield = BubbleShield.GetShield(localPlayer);

            shield.ActivatePowerRpc(NetworkObject);
            lastBubbleShield = shield;
        }
        else
        {
            if (context.started)
            {
                if (!CanCastAbility(Ability.BubbleShield))
                {
                    DoCastFailedEffectsRpc();
                    return;
                }

                var shield = BubbleShield.GetShield(localPlayer);
                shield.ActivatePowerRpc(NetworkObject);
                lastBubbleShield = shield;
            }
            else
            {
                if (lastBubbleShield != null && lastBubbleShield.IsActive)
                {
                    lastBubbleShield.DeactivatePowerRpc();
                    return;
                }
            }
        }
    }

    private void SharedBubbleShieldKeyPressed(InputAction.CallbackContext context) // Shared Bubble Shield
    {
        if (!BubbleShieldEnabled) { return; }

        logger.LogDebug("Shared cast");

        if (BubbleShieldSharedToggle)
        {
            if (context.canceled) { return; }

            if (lastBubbleShield != null && lastBubbleShield.IsActive)
            {
                lastBubbleShield.DeactivatePowerRpc();
                return;
            }
        }
        else
        {

        }
    }
}

internal class BubbleShield : PowerRingAbility, IHittable // TODO: Test this
{
    Shield shield = null!;
    Light light = null!;
    Collider collider = null!;

    public override float PowerUsagePerSecond => PowerRing.BubbleShieldPowerUsagePerSecond;

    public override float Duration => PowerRing.BubbleShieldDuration;

    public PlayerControllerB? player;

    public float YOffset = 1.2f;
    public Transform? parentObject => player?.transform;

    public void Awake()
    {
        shield = gameObject.GetComponent<Shield>();
        light = gameObject.GetComponentInChildren<Light>();
        collider = gameObject.GetComponent<Collider>();
    }

    public override void Start()
    {
        base.Start();
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

    public static BubbleShield GetShield(PlayerControllerB player)
    {
        if (!player.transform.TryGetComponentInChildren(out BubbleShield? shield) || shield == null)
        {
            GameObject shieldObj = GameObject.Instantiate(PowerRingContentHandler.Instance.PowerRing!.EnergyBubbleShieldPrefab, player.transform);
            shield = shieldObj.AddComponent<BubbleShield>();
        }

        return shield;
    }

    public override void OnActivatePower()
    {
        logger.LogDebug("OnActivatePower");
        shield.SetShieldActive(true);
        if (localPlayer != player) { return; }
        light.enabled = false;
        return;
    }

    public override void OnDeactivatePower()
    {
        shield.SetShieldActive(false);
    }

    public override void OnUsedUpPower()
    {
        return;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void HitRpc(Vector3 hitPos)
    {
        logger.LogDebug("HitRpc");
        shield.OnHit(collider.ClosestPoint(hitPos), (transform.position - hitPos).normalized, 1f, 1f);
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

            var shield = BubbleShield.GetShield(__instance);
            if (shield == null) { return true; }
            Vector3 hitPoint = enemyColliding.eye != null ? enemyColliding.eye.position : enemyColliding.transform.position;
            shield.HitRpc(hitPoint); // TODO: Test this
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
            if (!BubbleShield.GetShield(__instance).IsActive) { return; }
            __result = false;
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }
}