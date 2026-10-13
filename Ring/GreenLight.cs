using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool GreenLightToggle => PluginInstance.Config.Bind("Green Light", "Toggle", true, "If the green light ability should toggle or require holding the button down to work.").Value;
    public static float GreenLightPowerUsageOnCast => PluginInstance.Config.Bind("Green Light", "Power Usage On Cast", 0f, "How much power the green light ability consumes when activated.").Value;
    public static float GreenLightPowerUsagePerSecond => PluginInstance.Config.Bind("Green Light", "Power Usage Per Second", 0f, "How much power the green light ability consumes every second while active.").Value;

    private void GreenLightKeyPressed(InputAction.CallbackContext context) // Green Light
    {
        return;
    }
}