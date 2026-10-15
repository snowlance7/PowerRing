using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    private void EnergyShieldKeyPressed(InputAction.CallbackContext context) // Energy Shield
    {
        if (playerWornBy == null || playerWornBy != localPlayer) { return; }
    }
}