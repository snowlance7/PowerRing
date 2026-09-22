using Dawn;

namespace PowerRing;

public static class LethalDiseasesKeys
{
    public static readonly NamespacedKey<DawnItemInfo> PowerRing = NamespacedKey<DawnItemInfo>.From("powerring", "power_ring");
    public static readonly NamespacedKey<DawnUnlockableItemInfo> PowerBattery = NamespacedKey<DawnUnlockableItemInfo>.From("powerring", "power_battery");
}