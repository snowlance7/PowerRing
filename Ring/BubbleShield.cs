using GameNetcodeStuff;
using PowerRing.ProceduralForceField;
using PowerRing.Ring;
using SnowyLib;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool BubbleShieldToggle => PluginInstance.Config.Bind("Bubble Shield", "Toggle", true, "If the bubble shield ability should toggle or require holding the button down to work.").Value;
    public static float BubbleShieldPowerUsageOnCast => PluginInstance.Config.Bind("Bubble Shield", "Power Usage On Cast", 0f, "How much power the bubble shield ability consumes when activated.").Value;
    public static float BubbleShieldPowerUsagePerSecond => PluginInstance.Config.Bind("Bubble Shield", "Power Usage Per Second", 0f, "How much power the bubble shield ability consumes every second while active.").Value;
    public static bool BubbleShieldDeactivateOnDiscard => PluginInstance.Config.Bind("Bubble Shield", "Deactivate On Discard", true, "If the bubble shield should deactivate when the ring is discarded or pocketed.").Value;
    public static float BubbleShieldDuration => PluginInstance.Config.Bind("Bubble Shield", "Duration", 0f, "How long the bubble shield remains active after being activated.").Value;
    public static float BubbleShieldCooldown => PluginInstance.Config.Bind("Bubble Shield", "Cooldown", 0f, "How long before the bubble shield ability can be used again.").Value;

    private void BubbleShieldKeyPressed(InputAction.CallbackContext context) // Bubble Shield
    {
        // TODO: make it so you cast on yourself by default but if you use the point emote, it tries to cast on a player
        bool selfCast = localPlayer.GetCurrentEmote() == 2;

        if (selfCast)
        {

        }
        else
        {

        }

        throw new NotImplementedException();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void BubbleShieldRpc(ulong clientId)
    {
        PlayerControllerB player = PlayerFromId(clientId)!;
        var shieldObj = Instantiate(bubbleShieldPrefab, player.transform);
        NetworkObject netObj = shieldObj.GetComponent<NetworkObject>();
        netObj.Spawn(destroyWithScene: true);
        
        netObj.DoActionAfterSpawn((netObj) =>
        {
            var shield = netObj.GetComponent<EnergyBubbleShield>();
            shield.SetRingRpc(NetworkObject, true);
        });
    }
}

internal class EnergyBubbleShield : PowerRingAbility // TODO: Test this
{
    [SerializeField] ProceduralForceFieldOverlay overlay = null!;

    public override float PowerUsagePerSecond => PowerRing.BubbleShieldPowerUsagePerSecond;

    public override float Duration => PowerRing.BubbleShieldDuration;

    public override void OnActivatePower()
    {
        throw new NotImplementedException();
    }

    public override void OnDeactivatePower()
    {
        throw new NotImplementedException();
    }

    public override void OnUsedUpPower()
    {
        throw new NotImplementedException();
    }
}