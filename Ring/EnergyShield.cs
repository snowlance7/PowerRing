using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool EnergyShieldToggle => PluginInstance.Config.Bind("Energy Shield", "Toggle", true, "If the energy shield ability should toggle or require holding the button down to work.").Value;
    public static float EnergyShieldPowerUsageOnCast => PluginInstance.Config.Bind("Energy Shield", "Power Usage On Cast", 0f, "How much power the energy shield ability consumes when activated.").Value;
    public static float EnergyShieldPowerUsagePerSecond => PluginInstance.Config.Bind("Energy Shield", "Power Usage Per Second", 0f, "How much power the energy shield ability consumes every second while active.").Value;
    public static float EnergyShieldDuration => PluginInstance.Config.Bind("Energy Shield", "Duration", 0f, "How long the energy shield remains active after being activated.").Value;
    public static float EnergyShieldCooldown => PluginInstance.Config.Bind("Energy Shield", "Cooldown", 0f, "How long before the energy shield ability can be used again.").Value;

    private void EnergyShieldKeyPressed(InputAction.CallbackContext context) // Energy Shield
    {
        return;
    }
}