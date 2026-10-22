using FXV;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;
using static PowerRing.PowerRing;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    private void EnergyCageKeyPressed(InputAction.CallbackContext context) // Energy Cage
    {
        if (playerWornBy == null || playerWornBy != localPlayer || !AbilityEnabled[(int)Ability.EnergyCage] || context.canceled) { return; }

        DoQuickCastAnimationRpc();

        IEnumerator energyCage()
        {
            yield return null;

            if (!CanCastAbility(Ability.EnergyCage))
            {
                DoCastFailedEffectsRpc();
                yield break;
            }

            List<ulong> netObjIds = [];
            RaycastHit[] hits = Physics.SphereCastAll(localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward, 2f, localPlayer.gameplayCamera.transform.forward, energyBlastDistance, enemiesAndPlayersMask);

            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    var collisionDetect = hit.collider.gameObject.GetComponent<EnemyAICollisionDetect>();
                    if (collisionDetect == null) { continue; }
                    EnemyAI enemy = collisionDetect.mainScript;
                    logger.LogDebug($"{enemy.enemyType.enemyName} hit");
                    netObjIds.Add(enemy.NetworkObjectId);
                }
                else if (hit.collider.CompareTag("Player"))
                {
                    PlayerControllerB player = hit.collider.gameObject.GetComponent<PlayerControllerB>();
                    if (player == playerWornBy) { continue; }
                    logger.LogDebug($"{player.playerUsername} hit");
                    netObjIds.Add(player.NetworkObjectId);
                }
            }

            Vector3 forwardDirection = localPlayer.transform.TransformDirection(Vector3.forward).normalized;
            Vector3 upDirection = localPlayer.transform.TransformDirection(Vector3.up).normalized * 2;
            Vector3 throwDirection = (forwardDirection + upDirection).normalized;

            EnergyBlastRpc(netObjIds.ToArray(), throwDirection); // TODO: Test this
        }

        StartCoroutine(energyCage());
    }

    private Bounds GetEnemyBounds(GameObject enemy)
    {
        Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>();

        Bounds bounds = new Bounds();
        bool initialized = false;

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled)
                continue;

            if (!initialized)
            {
                bounds = renderer.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return bounds;
    }

    public void FitCageToEnemy(
    Transform cage,
    GameObject enemy,
    Vector3 baseCageSize)
    {
        Bounds bounds = GetEnemyBounds(enemy);

        if (bounds.size == Vector3.zero)
            return;

        Vector3 enemySize = bounds.size;

        Vector3 scale = new Vector3(
            enemySize.x / baseCageSize.x,
            enemySize.y / baseCageSize.y,
            enemySize.z / baseCageSize.z
        );

        cage.localScale = scale;
    }
}

internal class EnergyCage : PowerRingAbility // TODO
{
    public override Ability Ability => Ability.EnergyCage;

    public override void OnActivatePower()
    {

    }

    public override void OnDeactivatePower()
    {
        if (!IsServer) { return; }
        NetworkObject.Despawn(destroy: true);
    }

    public override void OnUsedUpPower()
    {
        if (!IsServer) { return; }
        NetworkObject.Despawn(destroy: true);
    }
}