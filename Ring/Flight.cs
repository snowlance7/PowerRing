using System;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool FlightToggle => PluginInstance.Config.Bind("Flight", "Toggle", true, "If the flight ability should toggle or require holding the button down to work.").Value;
    public static float FlightPowerUsageOnCast => PluginInstance.Config.Bind("Flight", "Power Usage On Cast", 0f, "How much power the flight ability consumes when activated.").Value;
    public static float FlightPowerUsagePerSecond => PluginInstance.Config.Bind("Flight", "Power Usage Per Second", 0f, "How much power the flight ability consumes every second while active.").Value;
    public static bool FlightDeactivateOnDiscard => PluginInstance.Config.Bind("Flight", "Deactivate On Discard", true, "If flight should deactivate when the ring is discarded or put into a pocket.").Value;
    public static float FlightDuration => PluginInstance.Config.Bind("Flight", "Duration", 0f, "How long flight remains active after being activated.").Value;
    public static float FlightCooldown => PluginInstance.Config.Bind("Flight", "Cooldown", 0f, "How long before the flight ability can be used again.").Value;
    public static bool FlightHover => PluginInstance.Config.Bind("Flight", "Hover", true, "If the flight ability should allow hovering instead of requiring continuous movement.").Value;

    private void FlightKeyPressed(InputAction.CallbackContext context) // Flight
    {
        throw new NotImplementedException();
    }
}