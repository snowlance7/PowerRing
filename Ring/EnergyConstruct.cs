using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool EnergyConstructToggle => PluginInstance.Config.Bind("Energy Construct", "Toggle", true, "If the energy construct ability should toggle or require holding the button down to work.").Value;
    public static float EnergyConstructPowerUsageOnCast => PluginInstance.Config.Bind("Energy Construct", "Power Usage On Cast", 0f, "How much power the energy construct ability consumes when activated.").Value;
    public static float EnergyConstructPowerUsagePerSecond => PluginInstance.Config.Bind("Energy Construct", "Power Usage Per Second", 0f, "How much power the energy construct ability consumes every second while active.").Value;
    public static bool EnergyConstructDeactivateOnDiscard => PluginInstance.Config.Bind("Energy Construct", "Deactivate On Discard", true, "If the energy construct should deactivate when the ring is discarded or put into a pocket.").Value;
    public static float EnergyConstructDuration => PluginInstance.Config.Bind("Energy Construct", "Duration", 0f, "How long the energy construct remains active after being activated.").Value;
    public static float EnergyConstructCooldown => PluginInstance.Config.Bind("Energy Construct", "Cooldown", 0f, "How long before the energy construct ability can be used again.").Value;

    private void EnergyConstructKeyPressed(InputAction.CallbackContext context) // Energy Construct
    {
        throw new NotImplementedException();
    }
}