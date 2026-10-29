using GameNetcodeStuff;
using SnowyLib;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static PowerRing.Plugin;
using EnergyShieldEffect;
using static PowerRing.PowerRing;

namespace PowerRing;

[RequireComponent(typeof(Shield), typeof(Collider))]
internal abstract class PowerRingConstruct : NetworkBehaviour
{
    public static List<PowerRingConstruct> Instances = new List<PowerRingConstruct>();

    public abstract PowerRing.Ability Ability { get; }
    public float PowerUsageOnCast => AbilityPowerUsageOnCast[(int)Ability];
    public float PowerUsagePerSecond => AbilityPowerUsagePerSecond[(int)Ability];
    public float Duration => AbilityDuration[(int)Ability];

    [HideInInspector] public PowerRing? ring;
    [HideInInspector] public PlayerControllerB? player;
    [HideInInspector] public EnemyAI? enemy;

    [HideInInspector] public Shield shield = null!;
    [HideInInspector] public Collider collider = null!;

    float timeActivated;

    public bool IsActivated => ring != null;

    public virtual void Awake()
    {
        shield = GetComponent<Shield>();
        collider = GetComponent<Collider>();
    }

    public virtual void Start()
    {
        Instances.Add(this);
    }

    public override void OnDestroy()
    {
        Instances.Remove(this);
    }

    public virtual void Update()
    {
        if (IsActivated && Duration > 0)
        {
            timeActivated += Time.deltaTime;

            if (timeActivated > Duration)
            {
                Deactivate();
                return;
            }
        }

        if (ring == null) { return; }

        if (ring.power <= 0)
        {
            Deactivate();
            return;
        }
    }

    public void Activate(PowerRing sourceRing)
    {
        logger.LogDebug($"Activating {Ability.ToString()} ability casted from {sourceRing.lastPlayerWornBy!.playerUsername}");

        if (IsActivated && ring != null)
        {
            if (ring == sourceRing)
            {
                timeActivated = 0f;
                ring.UsePower(PowerUsageOnCast);
                ring.SetCooldown(Ability);
            }
            return;
        }

        ring = sourceRing;
        timeActivated = 0f;
        ring.UsePower(PowerUsageOnCast);
        ring.SetCooldown(Ability);
        ring.powerUsage += PowerUsagePerSecond;
        ring.lastConstruct = this;
        shield.enabled = true;
        shield.SetShieldActive(true);
        OnActivate();
    }

    public void Deactivate()
    {
        ring?.powerUsage -= PowerUsagePerSecond;
        ring = null;
        shield.SetShieldActive(false);
        OnDeactivate();
        NetworkObject.Despawn(destroy: true, 1f);
    }

    public void HitShield(Vector3 hitPos, Vector3 hitDirection, float hitScale = 1f, float hitDuration = 1f)
    {
        shield.OnHit(collider.ClosestPoint(hitPos), hitDirection, hitScale, hitDuration);
    }

    public virtual void OnActivate() { }

    public virtual void OnDeactivate() { }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateRpc(NetworkObjectReference powerRingRef)
    {
        logger.LogDebug("ActivatePowerRpc");
        if (!powerRingRef.TryGet(out NetworkObject netObj)) { logger.LogError("ActivatePowerRpc: Unable to get networkobject from networkobjectreference"); return; }
        if (!netObj.TryGetComponent(out PowerRing ring)) { logger.LogError("ActivatePowerRpc: Failed to get ring from networkobject"); return; }
        Activate(ring);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateRpc(NetworkObjectReference powerRingRef, ulong clientId)
    {
        logger.LogDebug("ActivatePowerRpc");
        if (!powerRingRef.TryGet(out NetworkObject netObj)) { logger.LogError("ActivatePowerRpc: Unable to get networkobject from networkobjectreference"); return; }
        if (!netObj.TryGetComponent(out PowerRing ring)) { logger.LogError("ActivatePowerRpc: Failed to get ring from networkobject"); return; }
        player = PlayerFromId(clientId);
        Activate(ring);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ActivateRpc(NetworkObjectReference powerRingRef, NetworkObjectReference enemyNetRef)
    {
        logger.LogDebug("ActivatePowerRpc");
        if (!powerRingRef.TryGet(out NetworkObject netObj)) { logger.LogError("ActivatePowerRpc: Unable to get networkobject from networkobjectreference"); return; }
        if (!netObj.TryGetComponent(out PowerRing ring)) { logger.LogError("ActivatePowerRpc: Failed to get ring from networkobject"); return; }
        if (enemyNetRef.TryGet(out NetworkObject enemyNetObj)) { enemy = enemyNetObj.GetComponent<EnemyAI>(); }
        Activate(ring);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DeactivateRpc()
    {
        Deactivate();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void HitShieldRpc(Vector3 hitPos, Vector3 hitDirection, float hitScale = 1f, float hitDuration = 1f)
    {
        HitShield(hitPos, hitDirection, hitScale, hitDuration);
    }
}