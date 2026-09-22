using Dusk;

namespace PowerRing;

public class PowerRingContentHandler : ContentHandler<PowerRingContentHandler>
{
    public class PowerRingAssets(DuskMod mod, string filePath) : AssetBundleLoader<PowerRingAssets>(mod, filePath) { }
    public PowerRingAssets? PowerRing;

    public class PowerBatteryAssets(DuskMod mod, string filePath) : AssetBundleLoader<PowerBatteryAssets>(mod, filePath) { }
    public PowerBatteryAssets? PowerBattery;

    public PowerRingContentHandler(DuskMod mod) : base(mod)
    {
        RegisterContent("power_ring", out PowerRing);
        RegisterContent("power_battery", out PowerBattery);
    }
}