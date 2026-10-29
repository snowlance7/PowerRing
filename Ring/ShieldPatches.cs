using EnergyShieldEffect;
using GameNetcodeStuff;
using HarmonyLib;
using SnowyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static PowerRing.Plugin;

namespace PowerRing;

[HarmonyPatch]
internal static class ShieldPatches
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

            Vector3 hitPoint = enemyColliding.eye != null ? enemyColliding.eye.position : enemyColliding.transform.position;
            //bool shieldHit = EnergyShield.TryHit(__instance, hitPoint, 5f, 1f) || BubbleShield.TryHit(__instance, hitPoint, 5f, 1f); // TODO
            //return !shieldHit;
            return true;
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
            Vector3 direction = Utils.GetSpreadDirection(__instance.aimPoint.forward, 1f); // TODO: Test this
            if (!Physics.Raycast(__instance.aimPoint.position, direction, out RaycastHit hit, 30f, PowerRing.collidersMask)) { return; }
            if (!hit.collider.gameObject.TryGetComponent(out PowerRingConstruct construct)) { return; }
            if (!construct.IsActivated) { return; ; }
            construct.HitShieldRpc(hit.point, hit.normal);
            __result = null;
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(KnifeItem), nameof(KnifeItem.HitKnife))]
    static void KnifeItem_HitKnife_Postfix(KnifeItem __instance)
    {
        try
        {
            foreach (var hit in __instance.objectsHitByKnifeList)
            {
                if (!hit.collider.gameObject.TryGetComponent(out PowerRingConstruct construct)) { continue; }
                if (!construct.IsActivated) { continue; }
                construct.HitShieldRpc(hit.point, hit.normal);
            }
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Shovel), nameof(Shovel.HitShovel))]
    static void Shovel_HitShovel_Postfix(Shovel __instance)
    {
        try
        {
            foreach (var hit in __instance.objectsHitByShovelList)
            {
                if (!hit.collider.gameObject.TryGetComponent(out PowerRingConstruct construct)) { continue; }
                if (!construct.IsActivated) { continue; }
                construct.HitShieldRpc(hit.point, hit.normal);
            }
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ShotgunItem), nameof(ShotgunItem.ShootGun))]
    static void ShotgunItem_ShootGun_Postfix(ShotgunItem __instance, Vector3 shotgunPosition, Vector3 shotgunForward)
    {
        try
        {
            if (!Physics.Raycast(shotgunPosition, shotgunForward, out RaycastHit hit, 30f, PowerRing.collidersMask)) { return; }
            if (!hit.collider.gameObject.TryGetComponent(out PowerRingConstruct construct)) { return; }
            float force = 0f;
            if (hit.distance < 5f)
            {
                force = 5f;
            }
            else if (hit.distance < 15f)
            {
                force = 3f;
            }
            else if (hit.distance < 23f)
            {
                force = 2f;
            }
            else if (hit.distance < 30f)
            {
                force = 1f;
            }
            construct.HitShield(hit.point, hit.normal, force, force);
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    public static void PlayerControllerB_ConnectClientToPlayerObject_Postfix(PlayerControllerB __instance)
    {
        try
        {
            if (localPlayer != __instance || !PowerRing.IsEnabled) { return; }
            if (localPlayer.gameplayCamera.gameObject.GetComponent<ShieldPostprocess>() == null)
            {
                logger.LogDebug("Adding ShieldPostprocess script to player camera");
                ShieldPostprocess shield = localPlayer.gameplayCamera.gameObject.AddComponent<ShieldPostprocess>();
                shield.blurMethod = ShieldPostprocess.BlurMethod.GaussianOnly;
                shield.postprocessPower = 4;
                shield.globalIllumination = false;
            }
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }
}