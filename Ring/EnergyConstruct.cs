using SnowyLib;
using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication.Generated;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;
using static PowerRing.PowerRing;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    const float energyConstructCastDistance = 10f;
    public static int MaxEnergyConstructs => PluginInstance.Config.Bind("Energy Construct", "Max Energy Constructs", 2, "The max amount of energy constructs that can be active at once. If AbilityToggle is off, this is always 1.").Value;

    private void EnergyConstructKeyPressed(InputAction.CallbackContext context)
    {
        if (!CanUseRing() || !AbilityEnabled[(int)Ability.EnergyConstruct]) { return; }

        if (AbilityToggle[(int)Ability.EnergyConstruct])
        {
            if (context.canceled) { return; }

            QuickCastAbilitySynced(() =>
            {
                if (Physics.Raycast(localPlayer.gameplayCamera.transform.position, localPlayer.gameplayCamera.transform.forward, out RaycastHit hit, energyConstructCastDistance, collidersMask, QueryTriggerInteraction.Ignore))
                {
                    var construct = hit.collider.gameObject.GetComponent<EnergyConstruct>();
                    if (construct != null)
                    {
                        construct.DeactivateRpc();
                        return;
                    }
                }

                if (!CanCastAbility(Ability.EnergyConstruct)) { CastAbilityFailedRpc(); return; }

                CastEnergyConstruct();
            });
        }
        else
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.EnergyConstruct)) { CastAbilityFailedRpc(); return; }
                    CastEnergyConstruct();
                });
            }
            else
            {
                StopCastAnimationRpc();
                lastConstruct?.DeactivateRpc(); // TODO: Test this
            }
        }
    }

    private void CastEnergyConstruct()
    {
        if (!Physics.Raycast(localPlayer.gameplayCamera.transform.position, localPlayer.gameplayCamera.transform.forward, out RaycastHit hit, energyConstructCastDistance, roomMask, QueryTriggerInteraction.Ignore)) { CastAbilityFailedRpc(); return; }

        if (AbilityToggle[(int)Ability.EnergyConstruct])
        {
            var otherConstructs = PowerRingConstruct.Instances.Where(x => x is EnergyConstruct && x.ring == this);

            if (otherConstructs.Count() >= MaxEnergyConstructs)
            {
                otherConstructs.First().DeactivateRpc();
            }
        }

        SpawnEnergyConstructRpc(hit.point, playerWornBy!.transform.rotation);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SpawnEnergyConstructRpc(Vector3 position, Quaternion rotation)
    {
        var constructObj = Instantiate(energyConstructPrefab, position, rotation, RoundManager.Instance.mapPropsContainer.transform);
        constructObj.GetComponent<NetworkObject>().Spawn(destroyWithScene: true, (netObj) =>
        {
            netObj.GetComponent<EnergyConstruct>().ActivateRpc(this.NetworkObject);
        });
    }
}

internal class EnergyConstruct : PowerRingConstruct // TODO
{
    public override Ability Ability => Ability.EnergyConstruct;

    public override void OnActivate()
    {
        return;
    }

    public override void OnDeactivate()
    {
        return;
    }
}