using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    private void EnergyConstructKeyPressed(InputAction.CallbackContext context) // Energy Construct
    {
        if (playerWornBy == null || playerWornBy != localPlayer) { return; }
    }
}