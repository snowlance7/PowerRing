using Dusk;

namespace PowerRing
{
    public class PowerRingContentHandler : ContentHandler<PowerRingContentHandler>
    {
        public class PowerRingAssets(DuskMod mod, string filePath) : AssetBundleLoader<PowerRingAssets>(mod, filePath) { }
        public PowerRingAssets? PowerRing;

        public PowerRingContentHandler(DuskMod mod) : base(mod)
        {
            RegisterContent("power_ring", out PowerRing);
        }
    }
}