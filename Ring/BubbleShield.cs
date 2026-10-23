using FXV;
using GameNetcodeStuff;
using HarmonyLib;
using SnowyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;
using static PowerRing.PowerRing;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool ShowShieldOnClient => PluginInstance.Config.Bind("Bubble Shield", "Show Shield On Client", true, "If the shield effect should be visible on the client. Does not prevent you from seeing other players shield, just your own. \n CLIENT SIDE CONFIG").Value;

    BubbleShield? lastBubbleShield;

    private void BubbleShieldKeyPressed(InputAction.CallbackContext context) // Bubble Shield
    {
        if (!CanUseAbility(Ability.BubbleShield)) { return; }

        if (AbilityToggle[(int)Ability.BubbleShield])
        {
            if (context.canceled) { return; }

            QuickCastAbilitySynced(() =>
            {
                if (lastBubbleShield != null && lastBubbleShield.IsActive)
                {
                    logger.LogDebug("Deactivating shield");
                    lastBubbleShield.DeactivatePowerRpc();
                    return;
                }

                if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }
                CastBubbleShield(localPlayer);
            });
        }
        else
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }
                    CastBubbleShield(localPlayer);
                });
            }
            else
            {
                StopCastAnimationRpc();

                if (lastBubbleShield != null && lastBubbleShield.IsActive)
                {
                    logger.LogDebug("Deactivating shield");
                    lastBubbleShield.DeactivatePowerRpc();
                    return;
                }
            }
        }
    }

    private void SharedBubbleShieldKeyPressed(InputAction.CallbackContext context) // Shared Bubble Shield
    {
        if (!CanUseAbility(Ability.BubbleShield)) { return; }

        logger.LogDebug("Shared cast");

        if (AbilityToggle[(int)Ability.BubbleShield])
        {
            if (context.canceled) { return; }

            QuickCastAbilitySynced(() =>
            {
                if (lastBubbleShield != null && lastBubbleShield.IsActive)
                {
                    lastBubbleShield.DeactivatePowerRpc();
                    return;
                }

                if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }

                PlayerControllerB? player = GetPlayerLookingAt(20f);
                if (player == null) { CastAbilityFailedRpc(); return; }

                CastBubbleShield(player);
            });
        }
        else
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }

                    PlayerControllerB? player = GetPlayerLookingAt(20f);
                    if (player == null) { CastAbilityFailedRpc(); return; }

                    CastBubbleShield(player);
                });
            }
            else
            {
                StopCastAnimationRpc();

                if (lastBubbleShield != null && lastBubbleShield.IsActive)
                {
                    lastBubbleShield.DeactivatePowerRpc();
                    return;
                }
            }
        }
    }

    private void CastBubbleShield(PlayerControllerB player)
    {
        var shield = BubbleShield.GetShield(player);
        if (shield != null)
        {
            logger.LogDebug("Activating shield");
            shield.ActivatePowerRpc(NetworkObject);
            lastBubbleShield = shield;
        }
        else
        {
            logger.LogDebug("Activating shield");
            SpawnBubbleShieldServerRpc(player.actualClientId);
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SpawnBubbleShieldServerRpc(ulong shieldedClientId)
    {
        PlayerControllerB player = PlayerFromId(shieldedClientId)!;
        var shieldObj = Instantiate(PowerRingContentHandler.Instance.PowerRing!.EnergyBubbleShieldPrefab, player.transform);
        var shield = shieldObj.GetComponent<BubbleShield>();
        shield.NetworkObject.Spawn(destroyWithScene: false);
        shield.DoActionAfterSpawn((shield) =>
        {
            ((BubbleShield)shield).ActivatePowerRpc(NetworkObject, player.actualClientId);
            SpawnBubbleShieldClientRpc(shieldedClientId);
        });
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void SpawnBubbleShieldClientRpc(ulong shieldedClientId)
    {
        PlayerControllerB player = PlayerFromId(shieldedClientId)!;
        var shield = BubbleShield.GetShield(player);
        if (shield == null) { logger.LogError("SpawnBubbleShieldClientRpc: Shield is null"); return; }
        lastBubbleShield = shield;
    }
}

internal class BubbleShield : PowerRingAbility, IHittable // TODO: Test this // TODO: Works but hit effect doesnt show correctly
{
    Shield shield = null!;
    Collider collider = null!;

    public float YOffset = 1.2f;
    public Transform? parentObject => player?.transform;

    public override Ability Ability => Ability.BubbleShield;

    public void Awake()
    {
        shield = gameObject.GetComponent<Shield>();
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

    public static BubbleShield? GetShield(PlayerControllerB player)
    {
        return player.transform.GetComponentInChildren<BubbleShield>();
    }

    public override void OnActivatePower()
    {
        logger.LogDebug("OnActivatePower");

        shield.SetShieldActive(true);

        if (localPlayer != player) { return; }

        shield.SetShieldVisuallyActive(ShowShieldOnClient);
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

            var shield = BubbleShield.GetShield(__instance);
            if (shield == null || !shield.IsActive) { return true; }
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
            var shield = BubbleShield.GetShield(__instance);
            if (shield == null || !shield.IsActive) { return; }
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
            var shield = BubbleShield.GetShield(localPlayer);
            if (shield == null || !shield.IsActive) { return; }
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