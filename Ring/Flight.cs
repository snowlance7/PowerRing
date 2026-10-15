using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool FlightHover => PluginInstance.Config.Bind("Flight", "Hover", true, "If the flight ability should allow hovering instead of requiring continuous movement.").Value;

    private void FlightKeyPressed(InputAction.CallbackContext context) // Flight
    {
        if (playerWornBy == null || playerWornBy != localPlayer) { return; }
    }
}