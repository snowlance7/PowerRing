using EnergyShieldEffect;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PowerRing;

internal class ShieldCollisionDetect : MonoBehaviour, IHittable
{
    [SerializeField] Shield mainScript = null!;

    public bool Hit(int force, Vector3 hitDirection, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
    {
        throw new System.NotImplementedException();
    }
}