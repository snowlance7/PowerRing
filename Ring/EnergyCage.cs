using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool EnergyTrapToggle => PluginInstance.Config.Bind("Energy Trap", "Toggle", true, "If the energy trap ability should toggle or require holding the button down to work.").Value;
    public static float EnergyTrapPowerUsageOnCast => PluginInstance.Config.Bind("Energy Trap", "Power Usage On Cast", 0f, "How much power the energy trap ability consumes when activated.").Value;
    public static float EnergyTrapPowerUsagePerSecond => PluginInstance.Config.Bind("Energy Trap", "Power Usage Per Second", 0f, "How much power the energy trap ability consumes every second while active.").Value;
    public static bool EnergyTrapDeactivateOnDiscard => PluginInstance.Config.Bind("Energy Trap", "Deactivate On Discard", true, "If the energy trap should deactivate when the ring is discarded or put into a pocket.").Value;
    public static float EnergyTrapDuration => PluginInstance.Config.Bind("Energy Trap", "Duration", 0f, "How long the energy trap remains active after being activated.").Value;
    public static float EnergyTrapCooldown => PluginInstance.Config.Bind("Energy Trap", "Cooldown", 0f, "How long before the energy trap ability can be used again.").Value;

    private void EnergyCageKeyPressed(InputAction.CallbackContext context) // Energy Cage
    {
        throw new NotImplementedException();
    }
}