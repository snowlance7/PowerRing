using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    private void GreenLightKeyPressed(InputAction.CallbackContext context) // Green Light
    {
        if (playerWornBy == null || playerWornBy != localPlayer) { return; }
    }
}