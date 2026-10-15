using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    private void OathKeyPressed(InputAction.CallbackContext context) // Oath
    {
        if (playerWornBy == null || playerWornBy != localPlayer) { return; }
    }
}