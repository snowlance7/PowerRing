using SnowyLib;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PowerRing.Plugin;
using static PowerRing.Configs;
using Unity.Netcode;
using GameNetcodeStuff;
using PowerRing.ProceduralForceField;

namespace PowerRing;

internal class PowerRing : PhysicsProp
{
    public MeshRenderer renderer = null!;
    public Transform castDirection = null!;
    public ParticleSystem energyBlastParticleSystem = null!;
    public GameObject energyHitPrefab = null!;
    public ParticleSystem noPowerParticleSystem = null!;

    public GameObject bubbleShieldPrefab = null!;

    public GameObject energyConstructPrefab = null!;

    public GameObject energyCagePrefab = null!;

    public GameObject energyShieldObj = null!;

    public float power = 1f;
    public float powerUsage;

    float[] cooldowns = new float[7];

    int enemiesAndPlayersMask;

    public bool LocalPlayerUsing => playerHeldBy != null && localPlayer == playerHeldBy && !isPocketed;

    public enum Ability
    {
        GreenLight,
        BubbleShield,
        EnergyConstruct,
        EnergyBeam,
        Flight,
        EnergyTrap,
        EnergyShield
    }

    public void Awake()
    {
        itemProperties.positionOffset = new Vector3(0, 0, 0);
        itemProperties.rotationOffset = new Vector3(0, 0, 0);
        enemiesAndPlayersMask = LayerMask.GetMask("Enemies", "Player");
    }

    public void OnEnable()
    {
        PowerRingInputs.Instance.GreenLightKey.performed += GreenLightKeyPressed;
        PowerRingInputs.Instance.EnergyBubbleShieldKey.performed += EnergyBubbleShieldKeyPressed;
        PowerRingInputs.Instance.EnergyConstructKey.performed += EnergyConstructKeyPressed;
        PowerRingInputs.Instance.EnergyCageKey.performed += EnergyCageKeyPressed;
        PowerRingInputs.Instance.EnergyShieldKey.performed += EnergyShieldKeyPressed;
        PowerRingInputs.Instance.OathKey.performed += OathKeyPressed;
    }

    public void OnDisable()
    {
        PowerRingInputs.Instance.GreenLightKey.performed -= GreenLightKeyPressed;
        PowerRingInputs.Instance.EnergyBubbleShieldKey.performed -= EnergyBubbleShieldKeyPressed;
        PowerRingInputs.Instance.EnergyConstructKey.performed -= EnergyConstructKeyPressed;
        PowerRingInputs.Instance.EnergyCageKey.performed -= EnergyCageKeyPressed;
        PowerRingInputs.Instance.EnergyShieldKey.performed -= EnergyShieldKeyPressed;
        PowerRingInputs.Instance.OathKey.performed -= OathKeyPressed;
    }

    public override void SetControlTipsForItem()
    {
        string[] tooltips =
        [
            "Energy Beam [LMB]",
                "Flight [Q]",
                $"Green Light [{PowerRingInputs.Instance.GreenLightKey_BindingDisplayString}]",
                $"Energy Bubble Shield [{PowerRingInputs.Instance.EnergyBubbleShieldKey_BindingDisplayString}]",
                $"Energy Construct [{PowerRingInputs.Instance.EnergyConstructKey_BindingDisplayString}]",
                $"Energy Cage [{PowerRingInputs.Instance.EnergyCageKey_BindingDisplayString}]",
                $"Energy Shield [{PowerRingInputs.Instance.EnergyShieldKey_BindingDisplayString}]"
        ];
        HUDManager.Instance.ChangeControlTipMultiple(tooltips, holdingItem: true, itemProperties);
    }

    public override void Update()
    {
        base.Update();

        if (IsOwner)
        {
            power -= Time.deltaTime * powerUsage;
        }

        for (int i = 0; i < cooldowns.Length; i++)
        {
            if (cooldowns[i] <= 0)
            {
                cooldowns[i] = 0;
                continue;
            }

            cooldowns[i] -= Time.deltaTime;
        }
    }

    public override void EquipItem()
    {
        base.EquipItem();
        playerHeldBy.equippedUsableItemQE = true;
        SetControlTipsForItem();
    }
    public override void PocketItem()
    {
        playerHeldBy.equippedUsableItemQE = false;
        SetPowerRpc(power);
        base.PocketItem();
    }
    public override void DiscardItem()
    {
        playerHeldBy.equippedUsableItemQE = false;
        SetPowerRpc(power);
        base.DiscardItem();
    }

    public void UsedUpPower()
    {

    }

    public bool CanUsePower(float powerAmount)
    {
        float newPowerAmount = power - powerAmount;
        if (newPowerAmount < 0) { return false; }
        return true;
    }

    public bool IsOffCooldown(Ability ability)
    {
        return cooldowns[(int)ability] <= 0;
    }

    public void DoCastFailedEffects()
    {
        noPowerParticleSystem.Play();
    }

    public override void ItemActivate(bool used, bool buttonDown = true) // Energy Beam
    {
        base.ItemActivate(used, buttonDown);
        if (!buttonDown) { return; }
        return;
        if (!IsOffCooldown(Ability.EnergyBeam) || !CanUsePower(EnergyBeamPowerUsageOnCast))
        {
            DoCastFailedEffects();
            return;
        }

        energyBlastParticleSystem.Play();

        if (!Physics.SphereCast(localPlayer.transform.position + localPlayer.transform.forward, 1f, localPlayer.transform.forward, out RaycastHit hitInfo, 5f, enemiesAndPlayersMask)) { return; }
        if (hitInfo.collider.CompareTag("Enemies"))
        {
            EnemyAI enemy = hitInfo.collider.gameObject.GetComponent<EnemyAICollisionDetect>().mainScript;

        }
        else
        {
            PlayerControllerB player = hitInfo.collider.gameObject.GetComponent<PlayerControllerB>();

        }
    }

    public override void ItemInteractLeftRight(bool right) // Flight
    {
        base.ItemInteractLeftRight(right);
        if (right) { return; }

        // TODO
    }

    private void EnergyShieldKeyPressed(InputAction.CallbackContext context) // Energy Shield
    {
        throw new NotImplementedException();
    }

    private void EnergyCageKeyPressed(InputAction.CallbackContext context) // Energy Cage
    {
        throw new NotImplementedException();
    }

    private void EnergyConstructKeyPressed(InputAction.CallbackContext context) // Energy Construct
    {
        throw new NotImplementedException();
    }

    private void EnergyBubbleShieldKeyPressed(InputAction.CallbackContext context) // Energy Bubble Shield
    {
        // TODO: make it so you cast on yourself by default but if you use the point emote, it tries to cast on a player
        bool selfCast = localPlayer.GetCurrentEmote() == 2;

        if (selfCast)
        {

        }
        else
        {

        }

        throw new NotImplementedException();
    }

    private void GreenLightKeyPressed(InputAction.CallbackContext context) // Green Light
    {
        throw new NotImplementedException();
    }

    private void OathKeyPressed(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SetPowerRpc(float powerAmount)
    {
        power = powerAmount;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void EnergyBlastRpc(NetworkObjectReference netRef, Vector3 direction)
    {
        if (!netRef.TryGet(out NetworkObject netObj)) { logger.LogError("EnergyBlastRpc: failed to get networkobject from networkobjectreference"); return; }
        EnemyAI enemy = netObj.GetComponent<EnemyAI>();

        energyBlastParticleSystem.Play();
        GameObject energyHitObj = Instantiate(energyHitPrefab, enemy.eye);
        Destroy(energyHitObj, 2f);

        Utils.AddForceToEnemy(enemy, direction, EnergyBeamEnemyKnockbackForce);
        enemy.HitEnemy(EnergyBeamEnemyDamage, playerHeldBy);

        power = Mathf.Clamp01(power - EnergyBeamPowerUsageOnCast);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void EnergyBlastRpc(ulong playerHitId, Vector3 direction)
    {
        PlayerControllerB? player = PlayerFromId(playerHitId);
        if (player == null) { logger.LogError("Failed to get player from player id " + playerHitId); return; }

        energyBlastParticleSystem.Play();
        GameObject energyHitObj = Instantiate(energyHitPrefab, player.bodyParts[5]);
        Destroy(energyHitObj, 2f);

        Utils.AddForceToPlayer(player, direction, EnergyBeamPlayerKnockbackForce);
        player.DamagePlayer(EnergyBeamPlayerDamage, callRPC: false, causeOfDeath: CauseOfDeath.Blast, force: direction * EnergyBeamPlayerKnockbackForce);

        power = Mathf.Clamp01(power - EnergyBeamPowerUsageOnCast);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void BubbleShieldRpc(ulong clientId)
    {
        PlayerControllerB player = PlayerFromId(clientId)!;
        var shieldObj = Instantiate(bubbleShieldPrefab, player.transform);
        ProceduralForceFieldOverlay overlay = 
    }
}

public class EnergyBubbleShield : NetworkBehaviour
{
    [SerializeField] ProceduralForceFieldOverlay overlay = null!;

}