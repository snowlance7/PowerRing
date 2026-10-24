using GameNetcodeStuff;
using System.Linq;

namespace PowerRing;

internal static class Extensions
{
    public static T? GetConstruct<T>(this EnemyAI enemy) where T : PowerRingConstruct
    {
        return PowerRingConstruct.Instances.OfType<T>().FirstOrDefault(x => x.enemy == enemy);
    }

    public static T? GetConstruct<T>(this PlayerControllerB player) where T : PowerRingConstruct
    {
        return PowerRingConstruct.Instances.OfType<T>().FirstOrDefault(x => x.player == player);
    }
}