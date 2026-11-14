using SnowyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;
using static PowerRing.PowerRing;

namespace PowerRing;

internal partial class PowerRing : WearableObject // Turret raycast: Player, Room, InteractableObject, Colliders, PlayerRagdoll
{
    private void EnergyShieldKeyPressed(InputAction.CallbackContext context)
    {
        if (!CanUseRing(withAnimation: true) || !AbilityEnabled[(int)Ability.EnergyShield]) { return; }

        if (AbilityToggle[(int)Ability.EnergyShield]) // SELF CAST TOGGLE
        {
            if (context.canceled) { return; }

            QuickCastAbilitySynced(() =>
            {
                var shield = localPlayer.GetConstruct<EnergyShield>();
                if (shield != null && shield.IsActivated)
                {
                    shield.DeactivateRpc();
                    return;
                }

                if (!CanCastAbility(Ability.EnergyShield)) { CastAbilityFailedRpc(); return; }
                CastEnergyShieldRpc();
            });
        }
        else // SELF CAST HOLD
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.EnergyShield)) { CastAbilityFailedRpc(); return; }
                    CastEnergyShieldRpc();
                }, doPlayerCastAnimation: false);
            }
            else
            {
                StopCastRpc();

                var shield = localPlayer.GetConstruct<EnergyShield>();
                if (shield != null && shield.IsActivated)
                {
                    shield.DeactivateRpc();
                }
            }
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void CastEnergyShieldRpc()
    {
        if (!IsServer) { return; }
        var shieldObj = Instantiate(PowerRingContentHandler.Instance.PowerRing.EnergyShieldPrefab, transform);
        shieldObj.GetComponent<NetworkObject>().Spawn(destroyWithScene: true, (netObj) =>
        {
            if (netObj == null) { logger.LogError("netObj is null"); return; }
            if (this.NetworkObject == null) { logger.LogError("this.NetworkObject is null"); return; }
            netObj.GetComponent<EnergyShield>().ActivateRpc(this.NetworkObject);
        });
    }
}

internal class EnergyShield : PowerRingConstruct
{
    public override Ability Ability => Ability.EnergyShield;

    readonly Vector3 rotationOffset = new Vector3(0, 0, 0);
    readonly Vector3 positionOffset = new Vector3(0, 0, 0);

    public Transform? parentObject => ring?.transform;

    public override void Awake()
    {
        base.Awake();
        collider = gameObject.GetComponent<Collider>();
    }

    public override void Update()
    {
        base.Update();
        if (ring == null) { return; }
        if (ring.playerWornBy == null || !ring.inCastingAnimation)
        {
            Deactivate();
        }
    }

    public void LateUpdate()
    {
        if (parentObject != null)
        {
            base.transform.rotation = parentObject.rotation;
            base.transform.Rotate(rotationOffset);
            base.transform.position = parentObject.position;
            Vector3 _positionOffset = positionOffset;
            _positionOffset = parentObject.rotation * _positionOffset;
            base.transform.position += _positionOffset;
        }
    }
}