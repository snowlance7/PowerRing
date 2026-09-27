using SnowyLib;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PowerRing.Plugin;
using static PowerRing.Configs;

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

    float[] cooldowns = new float[7];

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
    }

    public void OnEnable()
    {
        PowerRingInputs.Instance.GreenLightKey.performed += GreenLightKeyPressed;

        PowerRingInputs.Instance.EnergyBubbleShieldKey.started += EnergyBubbleShieldKeyPressed;
        PowerRingInputs.Instance.EnergyBubbleShieldKey.canceled += EnergyBubbleShieldKeyPressed;

        PowerRingInputs.Instance.EnergyConstructKey.started += EnergyConstructKeyPressed;
        PowerRingInputs.Instance.EnergyConstructKey.canceled += EnergyConstructKeyPressed;

        PowerRingInputs.Instance.EnergyCageKey.started += EnergyCageKeyPressed;
        PowerRingInputs.Instance.EnergyCageKey.canceled += EnergyCageKeyPressed;

        PowerRingInputs.Instance.EnergyShieldKey.started += EnergyShieldKeyPressed;
        PowerRingInputs.Instance.EnergyShieldKey.canceled += EnergyShieldKeyPressed;
    }

    public void OnDisable()
    {
        PowerRingInputs.Instance.GreenLightKey.performed -= GreenLightKeyPressed;

        PowerRingInputs.Instance.EnergyBubbleShieldKey.started -= EnergyBubbleShieldKeyPressed;
        PowerRingInputs.Instance.EnergyBubbleShieldKey.canceled -= EnergyBubbleShieldKeyPressed;

        PowerRingInputs.Instance.EnergyConstructKey.started -= EnergyConstructKeyPressed;
        PowerRingInputs.Instance.EnergyConstructKey.canceled -= EnergyConstructKeyPressed;

        PowerRingInputs.Instance.EnergyCageKey.started -= EnergyCageKeyPressed;
        PowerRingInputs.Instance.EnergyCageKey.canceled -= EnergyCageKeyPressed;

        PowerRingInputs.Instance.EnergyShieldKey.started -= EnergyShieldKeyPressed;
        PowerRingInputs.Instance.EnergyShieldKey.canceled -= EnergyShieldKeyPressed;
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
        base.PocketItem();
    }
    public override void DiscardItem()
    {
        playerHeldBy.equippedUsableItemQE = false;
        base.DiscardItem();
    }

    public bool UsePower(float powerAmount)
    {
        if (!CanUsePower(powerAmount)) { return false; }
        power -= powerAmount;
        return true;
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

        if (!CanCastAbility(Ability.EnergyBeam))
        {
            DoCastFailedEffects();
            return;
        }

        if (!UsePower(EnergyBeamPowerUsageOnCast)) { return; }
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
        throw new NotImplementedException();
    }

    private void GreenLightKeyPressed(InputAction.CallbackContext context) // Green Light
    {
        throw new NotImplementedException();
    }
}
