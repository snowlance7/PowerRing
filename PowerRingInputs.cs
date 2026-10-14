using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace PowerRing;

internal class PowerRingInputs : LcInputActions
{
    public static PowerRingInputs Instance = new PowerRingInputs();

    [InputAction(KeyboardControl.F, Name = "Green Light")]
    public InputAction GreenLightKey { get; set; } = null!;

    [InputAction(KeyboardControl.B, Name = "Bubble Shield")]
    public InputAction BubbleShieldKey { get; set; } = null!;

    [InputAction(KeyboardControl.H, Name = "Shared Bubble Shield")]
    public InputAction SharedBubbleShieldKey { get; set; } = null!;

    [InputAction(KeyboardControl.C, Name = "Energy Construct")]
    public InputAction EnergyConstructKey { get; set; } = null!;

    [InputAction(KeyboardControl.R, Name = "Energy Blast")]
    public InputAction EnergyBlastKey { get; set; } = null!;

    [InputAction(KeyboardControl.Y, Name = "Flight")]
    public InputAction FlightKey { get; set; } = null!;

    [InputAction(KeyboardControl.X, Name = "Energy Cage")]
    public InputAction EnergyCageKey { get; set; } = null!;

    [InputAction(KeyboardControl.V, Name = "Energy Shield")]
    public InputAction EnergyShieldKey { get; set; } = null!;

    [InputAction(KeyboardControl.O, Name = "Oath")]
    public InputAction OathKey { get; set; } = null!;
}

//public string GreenLightKey_BindingDisplayString => GreenLightKey.GetBindingDisplayString(StartOfRound.Instance.localPlayerUsingController ? 1 : 0);