using GameNetcodeStuff;

namespace PowerRing;

internal static class Extensions
{
    public static BubbleShield? GetBubbleShield(this PlayerControllerB player)
    {
        return player.transform.GetComponentInChildren<BubbleShield>();
    }
}