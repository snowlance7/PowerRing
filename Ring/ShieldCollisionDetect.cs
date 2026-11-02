using GameNetcodeStuff;
using UnityEngine;

namespace PowerRing;

internal class ShieldCollisionDetect : MonoBehaviour, IHittable
{
    [SerializeField] PowerRingConstruct mainScript = null!;

    public bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
    {
        if (playerWhoHit == null || mainScript.collider == null) { return false; }
        Vector3 hitPoint = mainScript.collider.ClosestPoint(playerWhoHit.gameplayCamera.transform.position);
        mainScript.HitShieldRpc(hitPoint, hitDirection, force, force);
        return true;
    }
}