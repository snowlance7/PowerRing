using FXV;
using GameNetcodeStuff;
using HarmonyLib;
using SnowyLib;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using static PowerRing.Plugin;

namespace PowerRing;

/* bodyparts
 * 0 head
 * 1 right arm
 * 2 left arm
 * 3 right leg
 * 4 left leg
 * 5 chest
 * 6 feet
 * 7 right hip
 * 8 crotch
 * 9 left shoulder
 * 10 right shoulder */

[HarmonyPatch]
public static class TESTING
{
    //public static void DoHitTest()
    //{
    //    var shield = EnergyBubbleShield.GetBubbleShield(localPlayer);
    //    if (shield == null) { return; }
    //    Vector3 firePos = Utils.GetRandomPositionOnSphere(localPlayer.bodyParts[5].position, 3f);
    //    Vector3 directionToPlayer = (localPlayer.bodyParts[5].position - firePos).normalized;
    //    if (!Physics.Raycast(firePos, directionToPlayer, out RaycastHit hit, 5f, LayerMask.GetMask("Player"))) { return; }
    //    shield.HitRpc(hit.point);
    //}

    public static void DoHitTest()
    {
        logger.LogDebug("DoHitTest");
        var shield = localPlayer.GetConstruct<BubbleShield>();
        if (shield == null) { return; }
        Vector3 firePos = Utils.GetRandomPositionOnSphere(shield.transform.position, 2f);
        Vector3 directionToPlayer = (shield.transform.position - firePos).normalized;
        logger.LogDebug("Triggering");
        shield.HitRpc(firePos, directionToPlayer, 5f, 5f);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.PingScan_performed))]
    public static bool HUDManager_PingScan_performed_PreFix()
    {
        try
        {
            if (!Utils.testing) { return true; }
            //DoHitTest();
            //Utils.LogLayerNames(1051400); // Turret raycast: Player, Room, InteractableObject, Colliders, PlayerRagdoll
            return false;
        }
        catch
        {
            return true;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SubmitChat_performed))]
    public static void SubmitChat_performedPrefix(HUDManager __instance)
    {
        try
        {
            if (!Utils.testing) { return; }
            string msg = __instance.chatTextField.text;
            string[] args = msg.Split(" ");

            switch (args[0])
            {
                default:
                    break;
            }
        }
        catch
        {
            return;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.DamagePlayer))]
    private static void PlayerControllerB_DamagePlayer_Prefix(PlayerControllerB __instance) // TODO: RESULT FROM PUFFER ATTACK BELOW
    {
        try
        {
            StackTrace stackTrace = new StackTrace();

            foreach (StackFrame frame in stackTrace.GetFrames())
            {
                var method = frame.GetMethod();

                if (method == null)
                    continue;

                logger.LogInfo($"Caller: {method.DeclaringType?.FullName}.{method.Name}");
            }
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }
}

//[Info   : PowerRing] Caller: PowerRing.TESTING.PlayerControllerB_DamagePlayer_Prefix
//[Info   : PowerRing] Caller: GameNetcodeStuff.PlayerControllerB.DamagePlayer
//[Info   : PowerRing] Caller: PufferAI.OnCollideWithPlayer
//[Info   : PowerRing] Caller: EnemyAICollisionDetect.OnTriggerStay