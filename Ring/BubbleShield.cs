using GameNetcodeStuff;
using HarmonyLib;
using SnowyLib;
using System;
using System.Collections.Generic;
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
    public static bool ShowBubbleShieldOnClient => PluginInstance.Config.Bind("Bubble Shield", "Show Shield On Client", true, "If the shield effect should be visible on the client. Does not prevent you from seeing other players shield, just your own.").Value;
    public static int MaxSharedBubbleShields => PluginInstance.Config.Bind("Bubble Shield", "Max Shared Shields", 2, "The max amount of shared bubble shields that can be active at once. If AbilityToggle is off, this is always 1.").Value;

    public const float bubbleShieldCastDistance = 20f;

    private void BubbleShieldKeyPressed(InputAction.CallbackContext context) // SELF CAST
    {
        if (!CanUseRing() || !AbilityEnabled[(int)Ability.BubbleShield]) { return; }

        if (AbilityToggle[(int)Ability.BubbleShield]) // SELF CAST TOGGLE
        {
            if (context.canceled) { return; }

            QuickCastAbilitySynced(() =>
            {
                var shield = localPlayer.GetConstruct<BubbleShield>();
                if (shield != null && shield.IsActivated)
                {
                    shield.DeactivateRpc();
                    return;
                }

                if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }
                CastBubbleShield(localPlayer);
            }, doPlayerCastAnimation: false);
        }
        else // SELF CAST HOLD
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }
                    CastBubbleShield(localPlayer);
                }, doPlayerCastAnimation: false);
            }
            else
            {
                StopCastAnimationRpc();

                var shield = localPlayer.GetConstruct<BubbleShield>();
                if (shield != null && shield.IsActivated)
                {
                    shield.DeactivateRpc();
                }
            }
        }
    }

    private void SharedBubbleShieldKeyPressed(InputAction.CallbackContext context) // SHARED CAST
    {
        if (!CanUseRing() || !AbilityEnabled[(int)Ability.BubbleShield]) { return; }

        logger.LogDebug("Shared cast");

        if (AbilityToggle[(int)Ability.BubbleShield]) // SHARED CAST TOGGLE
        {
            if (context.canceled) { return; }

            QuickCastAbilitySynced(() =>
            {
                PlayerControllerB? player = GetPlayerLookingAt(bubbleShieldCastDistance);
                if (player == null) { CastAbilityFailedRpc(); return; }

                var shield = player.GetConstruct<BubbleShield>();
                if (shield != null)
                {
                    shield.DeactivateRpc();
                    return;
                }

                if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }

                CastBubbleShield(player);
            });
        }
        else // SHARED CAST HOLD
        {
            if (context.started)
            {
                CastAbilitySynced(() =>
                {
                    if (!CanCastAbility(Ability.BubbleShield)) { CastAbilityFailedRpc(); return; }

                    PlayerControllerB? player = GetPlayerLookingAt(bubbleShieldCastDistance);
                    if (player == null) { CastAbilityFailedRpc(); return; }

                    CastBubbleShield(player);
                });
            }
            else
            {
                StopCastAnimationRpc();
                lastConstruct?.DeactivateRpc(); // TODO: Test this
            }
        }
    }

    private void CastBubbleShield(PlayerControllerB player)
    {
        if (player != playerWornBy && AbilityToggle[(int)Ability.BubbleShield])
        {
            var otherShields = PowerRingConstruct.Instances.Where(x => x is BubbleShield && x.ring == this && x.player != playerWornBy);

            if (otherShields.Count() >= MaxSharedBubbleShields)
            {
                otherShields.First().DeactivateRpc();
            }
        }

        SpawnBubbleShieldRpc(player.actualClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SpawnBubbleShieldRpc(ulong shieldedClientId)
    {
        PlayerControllerB player = PlayerFromId(shieldedClientId)!;
        var shieldObj = Instantiate(bubbleShieldPrefab, player.transform);
        shieldObj.GetComponent<NetworkObject>().Spawn(destroyWithScene: true, (netObj) =>
        {
            netObj.GetComponent<BubbleShield>().ActivateRpc(this.NetworkObject, player.actualClientId);
        });
    }
}

internal class BubbleShield : PowerRingConstruct // TODO: Test this // TODO: Works but hit effect doesnt show correctly
{
    const float YOffset = 1.2f;
    public Transform? parentObject => player?.transform;

    public override Ability Ability => Ability.BubbleShield;

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

    public override void OnActivate()
    {
        if (localPlayer != player || ShowBubbleShieldOnClient) { return; }
        shield.SetShieldVisuallyActive(ShowBubbleShieldOnClient);
    }
}