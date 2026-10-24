using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;
using static PowerRing.PowerRing;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    private void EnergyConstructKeyPressed(InputAction.CallbackContext context) // Energy Construct
    {
        //if (playerWornBy == null || playerWornBy != localPlayer || !AbilityEnabled[(int)Ability.EnergyCage] || context.canceled) { return; }

        //DoQuickCastAnimationRpc();

        //IEnumerator energyCage()
        //{
        //    yield return null;

        //    if (!CanCastAbility(Ability.EnergyCage))
        //    {
        //        DoCastFailedEffectsRpc();
        //        yield break;
        //    }

        //    RaycastHit[] hits = Physics.SphereCastAll(localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward, 2f, localPlayer.gameplayCamera.transform.forward, energyCageCastDistance, enemiesMask);

        //    foreach (var hit in hits)
        //    {
        //        if (!hit.collider.CompareTag("Enemy")) { continue; }
        //        var collisionDetect = hit.collider.gameObject.GetComponent<EnemyAICollisionDetect>();
        //        if (collisionDetect == null) { continue; }
        //        EnemyAI enemy = collisionDetect.mainScript;
        //        logger.LogDebug($"{enemy.enemyType.enemyName} hit");
        //        SpawnEnergyCageRpc(enemy.NetworkObject);
        //        yield break;
        //    }

        //    DoCastFailedEffectsRpc();
        //}

        //StartCoroutine(energyCage());
    }
}

internal class EnergyConstruct : PowerRingConstruct
{
    public override PowerRing.Ability Ability => PowerRing.Ability.EnergyConstruct;

    public override void OnActivate()
    {
        throw new NotImplementedException();
    }

    public override void OnDeactivate()
    {
        throw new NotImplementedException();
    }
}

//internal class EnergyCage : PowerRingAbility // TODO
//{
//    public override Ability Ability => Ability.EnergyCage;

//    Bounds enemyBounds;

//    public override void Update()
//    {
//        base.Update();
//        if (enemy == null) { return; }
//        transform.position = enemyBounds.center;
//        enemy?.agent.enabled = false;

//        if (enemy == null || enemy.isEnemyDead)
//        {
//            DeactivatePower();
//        }
//    }

//    public override void OnActivatePower()
//    {
//        enemyBounds = GetEnemyBounds(enemy!.gameObject);
//        FitCageToEnemy();
//    }

//    public override void OnDeactivatePower()
//    {
//        if (!IsServer) { return; }
//        enemy?.agent.enabled = true;
//        NetworkObject.Despawn(destroy: true);
//    }

//    public override void OnUsedUpPower()
//    {
//        if (!IsServer) { return; }
//        NetworkObject.Despawn(destroy: true);
//    }

//    private Bounds GetEnemyBounds(GameObject enemyObj)
//    {
//        Renderer[] renderers = enemyObj.GetComponentsInChildren<Renderer>();

//        Bounds bounds = new Bounds();
//        bool initialized = false;

//        foreach (Renderer renderer in renderers)
//        {
//            if (!renderer.enabled)
//                continue;

//            if (!initialized)
//            {
//                bounds = renderer.bounds;
//                initialized = true;
//            }
//            else
//            {
//                bounds.Encapsulate(renderer.bounds);
//            }
//        }

//        return bounds;
//    }

//    private void FitCageToEnemy()
//    {
//        Vector3 baseCageSize = new Vector3(1f, 1f, 1f);

//        if (enemyBounds.size == Vector3.zero)
//            return;

//        Vector3 enemySize = enemyBounds.size;

//        Vector3 scale = new Vector3(
//            enemySize.x / baseCageSize.x,
//            enemySize.y / baseCageSize.y,
//            enemySize.z / baseCageSize.z
//        );

//        transform.localScale = scale;
//    }
//}