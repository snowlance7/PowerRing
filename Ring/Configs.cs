using SnowyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static PowerRing.PowerRing;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing
{
    //public static float[] AbilityPowerUsageOnCast = [0, 0, 0, 0, 0, 0, 0] // TODO: Do this for setting defaults
    public static bool[] AbilityEnabled = new bool[7];
    public static bool[] AbilityToggle = new bool[7];
    public static float[] AbilityPowerUsageOnCast = new float[7];
    public static float[] AbilityPowerUsagePerSecond = new float[7];
    public static float[] AbilityDuration = new float[7];
    public static float[] AbilityCooldown = new float[7];

    [StaticInit]
    public static void InitConfigs()
    {
        foreach (var ability in Enum.GetValues(typeof(Ability)))
        {
            string abilityName = ability.ToString().AddSpacesToPascalCase();

            AbilityEnabled[(int)ability] = PluginInstance.Config.Bind(abilityName, "Enabled", AbilityEnabled[(int)ability], $"If the {abilityName} ability should be enabled").Value;
            AbilityToggle[(int)ability] = PluginInstance.Config.Bind(abilityName, "Toggle", AbilityToggle[(int)ability], $"If the {abilityName} ability should toggle or require holding the button down to cast").Value;
            AbilityPowerUsageOnCast[(int)ability] = PluginInstance.Config.Bind(abilityName, "Power Usage On Cast", AbilityPowerUsageOnCast[(int)ability], $"How much power the {abilityName} ability consumes when activated").Value;
            AbilityPowerUsagePerSecond[(int)ability] = PluginInstance.Config.Bind(abilityName, "Power Usage Per second", AbilityPowerUsagePerSecond[(int)ability], $"How much power the {abilityName} ability consumes every second while active").Value;
            AbilityDuration[(int)ability] = PluginInstance.Config.Bind(abilityName, "Duration", AbilityDuration[(int)ability], $"How long the {abilityName} remains active after being activated").Value;
            AbilityCooldown[(int)ability] = PluginInstance.Config.Bind(abilityName, "Cooldown", AbilityCooldown[(int)ability], $"How long before the {abilityName} ability can be used again").Value;
        }
    }
}