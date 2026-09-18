using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace Something
{
    internal class PowerRingInputs : LcInputActions
    {
        public static PowerRingInputs Instance = new PowerRingInputs();


        [InputAction(KeyboardControl.F, Name = "Green Light")]
        public InputAction GreenLightKey { get; set; } = null!;
        public string GreenLightKey_BindingDisplayString => GreenLightKey.GetBindingDisplayString(StartOfRound.Instance.localPlayerUsingController ? 1 : 0);


        [InputAction(KeyboardControl.B, Name = "Bubble Shield")]
        public InputAction EnergyBubbleShieldKey { get; set; } = null!;
        public string EnergyBubbleShieldKey_BindingDisplayString => EnergyBubbleShieldKey.GetBindingDisplayString(StartOfRound.Instance.localPlayerUsingController ? 1 : 0);


        [InputAction(KeyboardControl.C, Name = "Energy Construct")]
        public InputAction EnergyConstructKey { get; set; } = null!;
        public string EnergyConstructKey_BindingDisplayString => EnergyConstructKey.GetBindingDisplayString(StartOfRound.Instance.localPlayerUsingController ? 1 : 0);


        [InputAction(KeyboardControl.X, Name = "Energy Cage")]
        public InputAction EnergyCageKey { get; set; } = null!;
        public string EnergyCageKey_BindingDisplayString => EnergyCageKey.GetBindingDisplayString(StartOfRound.Instance.localPlayerUsingController ? 1 : 0);


        [InputAction(KeyboardControl.V, Name = "Energy Shield")]
        public InputAction EnergyShieldKey { get; set; } = null!;
        public string EnergyShieldKey_BindingDisplayString => EnergyShieldKey.GetBindingDisplayString(StartOfRound.Instance.localPlayerUsingController ? 1 : 0);
    }
}
