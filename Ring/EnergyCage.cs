using SnowyLib;
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
    public const float energyCageCastDistance = 5f;
    public static int MaxEnergyCages => PluginInstance.Config.Bind("Energy Cage", "Max Energy Cages", 2, "The max amount of energy cages that can be active at once. If AbilityToggle is off, this is always 1.").Value;

    private void EnergyCageKeyPressed(InputAction.CallbackContext context) // Energy Cage
    {
        if (!CanUseRing() || !AbilityEnabled[(int)Ability.EnergyCage]) { return; }

        if (AbilityToggle[(int)Ability.EnergyCage])
        {
            if (context.canceled) { return; }

            QuickCastAbilitySynced(() =>
            {
                EnemyAI? enemy = GetEnemyLookingAt(energyCageCastDistance);
                if (enemy == null) { CastAbilityFailedRpc(); return; }

                var cage = enemy.GetConstruct<EnergyCage>();
                if (cage != null)
                {
                    cage.DeactivateRpc();
                    return;
                }

                if (!CanCastAbility(Ability.EnergyCage)) { CastAbilityFailedRpc(); return; }

                CastEnergyCage(enemy);
            });
        }
        else
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.EnergyCage)) { CastAbilityFailedRpc(); return; }

                    EnemyAI? enemy = GetEnemyLookingAt(energyCageCastDistance);
                    if (enemy == null) { CastAbilityFailedRpc(); return; }

                    CastEnergyCage(enemy);
                });
            }
            else
            {
                StopCastAnimationRpc();
                lastConstruct?.DeactivateRpc(); // TODO: Test this
            }
        }
    }

    public void CastEnergyCage(EnemyAI enemy)
    {
        if (AbilityToggle[(int)Ability.EnergyCage])
        {
            var otherCages = PowerRingConstruct.Instances.Where(x => x is EnergyCage && x.ring == this);

            if (otherCages.Count() >= MaxEnergyCages)
            {
                otherCages.First().DeactivateRpc();
            }
        }

        SpawnEnergyCageRpc(enemy.NetworkObject);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SpawnEnergyCageRpc(NetworkObjectReference netRef)
    {
        if (!netRef.TryGet(out NetworkObject netObj)) { logger.LogError("EnergyCageRpc: Failed to get netObj from netRef"); return; }
        EnemyAI enemy = netObj.GetComponent<EnemyAI>();
        var cageObj = Instantiate(energyCagePrefab, enemy.transform.position, Quaternion.identity);
        cageObj.GetComponent<NetworkObject>().Spawn(destroyWithScene: true, (cageNetObj) => cageNetObj.GetComponent<EnergyCage>().ActivateRpc(this.NetworkObject, netRef));
    }
}

internal class EnergyCage : PowerRingConstruct // TODO
{
    public override Ability Ability => Ability.EnergyCage;

    Vector3 centerPosition;

    public override void Update()
    {
        base.Update();

        if (enemy == null)
        {
            if (IsActivated)
                Deactivate();
            return;
        }

        transform.position = centerPosition;
        enemy.agent.enabled = false;
        
        if (enemy.isEnemyDead)
        {
            Deactivate();
        }
    }

    public override void OnActivate()
    {
        SetCenterPositionAndSize();
    }

    public override void OnDeactivate()
    {
        if (!IsServer) { return; }
        enemy?.agent.enabled = true;
    }

    public void SetCenterPositionAndSize()
    {
        var colliders = enemy!.gameObject.GetComponentsInChildren<EnemyAICollisionDetect>().Select(x => x.gameObject.GetComponent<Collider>());
        if (colliders.Count() == 0) { centerPosition = enemy.eye.position; return; }

        var largestCollider = Utils.GetLargestCollider(colliders.ToArray());
        centerPosition = largestCollider.bounds.center;
        //float scaleToUse = Mathf.Max(largestCollider.bounds.size.x, largestCollider.bounds.size.y, largestCollider.bounds.size.z);
        //transform.localScale = new Vector3(scaleToUse, scaleToUse, scaleToUse);
    }
}