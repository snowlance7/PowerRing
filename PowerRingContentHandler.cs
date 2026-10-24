using BepInEx.Logging;
using Dusk;
using UnityEngine;

namespace PowerRing;

public class PowerRingContentHandler : ContentHandler<PowerRingContentHandler>
{
    public class PowerRingAssets(DuskMod mod, string filePath) : AssetBundleLoader<PowerRingAssets>(mod, filePath)
    {

        [LoadFromBundle("ShieldCustomPass.prefab")]
        public GameObject ShieldCustomPassPrefab { get; private set; } = null!;

        [LoadFromBundle("FXVPostprocessBlitAdd_HDRP.shader")]
        public Shader FXVPostprocessBlitAdd_HDRPShader { get; private set; } = null!;

        [LoadFromBundle("FXVPostprocessShield_HDRP.shader")]
        public Shader FXVPostprocessShield_HDRPShader { get; private set; } = null!;

        [LoadFromBundle("FXVPostprocessBlitAdd.shader")]
        public Shader FXVPostprocessBlitAddShader { get; private set; } = null!;

        [LoadFromBundle("FXVPostprocessShield.shader")]
        public Shader FXVPostprocessShieldShader { get; private set; } = null!;

        [LoadFromBundle("RandomNoise.png")]
        public Texture2D RandomNoiseTexture { get; private set; } = null!;
    }
    public PowerRingAssets? PowerRing;

    public class PowerBatteryAssets(DuskMod mod, string filePath) : AssetBundleLoader<PowerBatteryAssets>(mod, filePath) { }
    public PowerBatteryAssets? PowerBattery;

    public PowerRingContentHandler(DuskMod mod) : base(mod)
    {
        RegisterContent("power_ring", out PowerRing);
        //RegisterContent("power_battery", out PowerBattery);
    }
}