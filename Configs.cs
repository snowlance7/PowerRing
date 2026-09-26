using SnowyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static PowerRing.Plugin;

namespace PowerRing
{
    internal static class Configs
    {
        public static bool GreenLightToggle => PluginInstance.Config.Bind("Green Light", "Toggle", true, "If the green light ability should toggle or require holding the button down to work.").Value;
        public static float GreenLightPowerUsageOnCast => PluginInstance.Config.Bind("Green Light", "Power Usage On Cast", 0f, "How much power the green light ability consumes when activated.").Value;
        public static float GreenLightPowerUsagePerSecond => PluginInstance.Config.Bind("Green Light", "Power Usage Per Second", 0f, "How much power the green light ability consumes every second while active.").Value;


        public static bool BubbleShieldToggle => PluginInstance.Config.Bind("Bubble Shield", "Toggle", true, "If the bubble shield ability should toggle or require holding the button down to work.").Value;
        public static float BubbleShieldPowerUsageOnCast => PluginInstance.Config.Bind("Bubble Shield", "Power Usage On Cast", 0f, "How much power the bubble shield ability consumes when activated.").Value;
        public static float BubbleShieldPowerUsagePerSecond => PluginInstance.Config.Bind("Bubble Shield", "Power Usage Per Second", 0f, "How much power the bubble shield ability consumes every second while active.").Value;
        public static bool BubbleShieldDeactivateOnDiscard => PluginInstance.Config.Bind("Bubble Shield", "Deactivate On Discard", true, "If the bubble shield should deactivate when the ring is discarded or pocketed.").Value;
        public static float BubbleShieldUseDuration => PluginInstance.Config.Bind("Bubble Shield", "Use Duration", 0f, "How long the bubble shield can be activated for with a single use.").Value;
        public static float BubbleShieldDuration => PluginInstance.Config.Bind("Bubble Shield", "Duration", 0f, "How long the bubble shield remains active after being activated.").Value;
        public static float BubbleShieldSelfUseDuration => PluginInstance.Config.Bind("Bubble Shield", "Self Use Duration", 0f, "How long the bubble shield can be activated for when used on the player themselves.").Value;
        public static float BubbleShieldSelfDuration => PluginInstance.Config.Bind("Bubble Shield", "Self Duration", 0f, "How long the bubble shield remains active when used on the player themselves.").Value;
        public static float BubbleShieldCooldown => PluginInstance.Config.Bind("Bubble Shield", "Cooldown", 0f, "How long before the bubble shield ability can be used again.").Value;


        public static bool EnergyConstructToggle => PluginInstance.Config.Bind("Energy Construct", "Toggle", true, "If the energy construct ability should toggle or require holding the button down to work.").Value;
        public static float EnergyConstructPowerUsageOnCast => PluginInstance.Config.Bind("Energy Construct", "Power Usage On Cast", 0f, "How much power the energy construct ability consumes when activated.").Value;
        public static float EnergyConstructPowerUsagePerSecond => PluginInstance.Config.Bind("Energy Construct", "Power Usage Per Second", 0f, "How much power the energy construct ability consumes every second while active.").Value;
        public static bool EnergyConstructDeactivateOnDiscard => PluginInstance.Config.Bind("Energy Construct", "Deactivate On Discard", true, "If the energy construct should deactivate when the ring is discarded or put into a pocket.").Value;
        public static float EnergyConstructUseDuration => PluginInstance.Config.Bind("Energy Construct", "Use Duration", 0f, "How long the energy construct can be activated for with a single use.").Value;
        public static float EnergyConstructDuration => PluginInstance.Config.Bind("Energy Construct", "Duration", 0f, "How long the energy construct remains active after being activated.").Value;
        public static float EnergyConstructCooldown => PluginInstance.Config.Bind("Energy Construct", "Cooldown", 0f, "How long before the energy construct ability can be used again.").Value;


        public static float EnergyBeamPowerUsageOnCast => PluginInstance.Config.Bind("Energy Beam", "Power Usage On Cast", 0f, "How much power the energy beam consumes when fired.").Value;
        public static float EnergyBeamCooldown => PluginInstance.Config.Bind("Energy Beam", "Cooldown", 0f, "How long before the energy beam can be fired again.").Value;
        public static float EnergyBeamEnemyKnockbackForce => PluginInstance.Config.Bind("Energy Beam", "Enemy Knockback Force", 0f, "How much knockback force the energy beam applies to enemies.").Value;
        public static float EnergyBeamPlayerKnockbackForce => PluginInstance.Config.Bind("Energy Beam", "Player Knockback Force", 0f, "How much knockback force the energy beam applies to players.").Value;
        public static float EnergyBeamEnemyDamage => PluginInstance.Config.Bind("Energy Beam", "Enemy Damage", 0f, "How much damage the energy beam deals to enemies.").Value;
        public static float EnergyBeamPlayerDamage => PluginInstance.Config.Bind("Energy Beam", "Player Damage", 0f, "How much damage the energy beam deals to players.").Value;


        public static bool FlightToggle => PluginInstance.Config.Bind("Flight", "Toggle", true, "If the flight ability should toggle or require holding the button down to work.").Value;
        public static float FlightPowerUsageOnCast => PluginInstance.Config.Bind("Flight", "Power Usage On Cast", 0f, "How much power the flight ability consumes when activated.").Value;
        public static float FlightPowerUsagePerSecond => PluginInstance.Config.Bind("Flight", "Power Usage Per Second", 0f, "How much power the flight ability consumes every second while active.").Value;
        public static bool FlightDeactivateOnDiscard => PluginInstance.Config.Bind("Flight", "Deactivate On Discard", true, "If flight should deactivate when the ring is discarded or put into a pocket.").Value;
        public static float FlightUseDuration => PluginInstance.Config.Bind("Flight", "Use Duration", 0f, "How long flight can be activated for with a single use.").Value;
        public static float FlightDuration => PluginInstance.Config.Bind("Flight", "Duration", 0f, "How long flight remains active after being activated.").Value;
        public static float FlightCooldown => PluginInstance.Config.Bind("Flight", "Cooldown", 0f, "How long before the flight ability can be used again.").Value;
        public static bool FlightHover => PluginInstance.Config.Bind("Flight", "Hover", true, "If the flight ability should allow hovering instead of requiring continuous movement.").Value;


        public static bool EnergyTrapToggle => PluginInstance.Config.Bind("Energy Trap", "Toggle", true, "If the energy trap ability should toggle or require holding the button down to work.").Value;
        public static float EnergyTrapPowerUsageOnCast => PluginInstance.Config.Bind("Energy Trap", "Power Usage On Cast", 0f, "How much power the energy trap ability consumes when activated.").Value;
        public static float EnergyTrapPowerUsagePerSecond => PluginInstance.Config.Bind("Energy Trap", "Power Usage Per Second", 0f, "How much power the energy trap ability consumes every second while active.").Value;
        public static bool EnergyTrapDeactivateOnDiscard => PluginInstance.Config.Bind("Energy Trap", "Deactivate On Discard", true, "If the energy trap should deactivate when the ring is discarded or put into a pocket.").Value;
        public static float EnergyTrapUseDuration => PluginInstance.Config.Bind("Energy Trap", "Use Duration", 0f, "How long the energy trap can be activated for with a single use.").Value;
        public static float EnergyTrapDuration => PluginInstance.Config.Bind("Energy Trap", "Duration", 0f, "How long the energy trap remains active after being activated.").Value;
        public static float EnergyTrapCooldown => PluginInstance.Config.Bind("Energy Trap", "Cooldown", 0f, "How long before the energy trap ability can be used again.").Value;
        public static float EnergyTrapSlowMultiplier => PluginInstance.Config.Bind("Energy Trap", "Slow Multiplier", 1f, "Multiplier applied to movement speed while affected by the energy trap. A value of 1 stops movement completely.").Value;


        public static bool EnergyShieldToggle => PluginInstance.Config.Bind("Energy Shield", "Toggle", true, "If the energy shield ability should toggle or require holding the button down to work.").Value;
        public static float EnergyShieldPowerUsageOnCast => PluginInstance.Config.Bind("Energy Shield", "Power Usage On Cast", 0f, "How much power the energy shield ability consumes when activated.").Value;
        public static float EnergyShieldPowerUsagePerSecond => PluginInstance.Config.Bind("Energy Shield", "Power Usage Per Second", 0f, "How much power the energy shield ability consumes every second while active.").Value;
        public static float EnergyShieldUseDuration => PluginInstance.Config.Bind("Energy Shield", "Use Duration", 0f, "How long the energy shield can be activated for with a single use.").Value;
        public static float EnergyShieldDuration => PluginInstance.Config.Bind("Energy Shield", "Duration", 0f, "How long the energy shield remains active after being activated.").Value;
        public static float EnergyShieldCooldown => PluginInstance.Config.Bind("Energy Shield", "Cooldown", 0f, "How long before the energy shield ability can be used again.").Value;

        [StaticInit]
        public static void InitConfigs()
        {
            _ = GreenLightToggle;
            _ = GreenLightPowerUsageOnCast;
            _ = GreenLightPowerUsagePerSecond;

            _ = BubbleShieldToggle;
            _ = BubbleShieldPowerUsageOnCast;
            _ = BubbleShieldPowerUsagePerSecond;
            _ = BubbleShieldDeactivateOnDiscard;
            _ = BubbleShieldUseDuration;
            _ = BubbleShieldDuration;
            _ = BubbleShieldSelfUseDuration;
            _ = BubbleShieldSelfDuration;
            _ = BubbleShieldCooldown;

            _ = EnergyConstructToggle;
            _ = EnergyConstructPowerUsageOnCast;
            _ = EnergyConstructPowerUsagePerSecond;
            _ = EnergyConstructDeactivateOnDiscard;
            _ = EnergyConstructUseDuration;
            _ = EnergyConstructDuration;
            _ = EnergyConstructCooldown;

            _ = EnergyBeamPowerUsageOnCast;
            _ = EnergyBeamCooldown;
            _ = EnergyBeamEnemyKnockbackForce;
            _ = EnergyBeamPlayerKnockbackForce;
            _ = EnergyBeamEnemyDamage;
            _ = EnergyBeamPlayerDamage;

            _ = FlightToggle;
            _ = FlightPowerUsageOnCast;
            _ = FlightPowerUsagePerSecond;
            _ = FlightDeactivateOnDiscard;
            _ = FlightUseDuration;
            _ = FlightDuration;
            _ = FlightCooldown;
            _ = FlightHover;

            _ = EnergyTrapToggle;
            _ = EnergyTrapPowerUsageOnCast;
            _ = EnergyTrapPowerUsagePerSecond;
            _ = EnergyTrapDeactivateOnDiscard;
            _ = EnergyTrapUseDuration;
            _ = EnergyTrapDuration;
            _ = EnergyTrapCooldown;
            _ = EnergyTrapSlowMultiplier;

            _ = EnergyShieldToggle;
            _ = EnergyShieldPowerUsageOnCast;
            _ = EnergyShieldPowerUsagePerSecond;
            _ = EnergyShieldUseDuration;
            _ = EnergyShieldDuration;
            _ = EnergyShieldCooldown;
        }
    }
}
