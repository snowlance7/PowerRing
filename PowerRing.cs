using SnowyLib;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PowerRing.Plugin;

namespace PowerRing;

internal class PowerRing : PhysicsProp
{
    public MeshRenderer renderer = null!;
    public GameObject bubbleShieldPrefab = null!;
    public GameObject energyConstructPrefab = null!;
    public GameObject energyCagePrefab = null!;
    public GameObject energyShieldObj = null!;

    public float charge = 1f;

    public bool LocalPlayerUsing => playerHeldBy != null && localPlayer == playerHeldBy && !isPocketed;

    public static readonly bool energyShieldToggle = true;


    [StaticInit]
    public static void InitConfigs()
    {
        // TODO: Look at configs.txt
    }

    public void Awake()
    {
        itemProperties.positionOffset = new Vector3(0, 0, 0);
        itemProperties.rotationOffset = new Vector3(0, 0, 0);
    }

    public void OnEnable()
    {
        PowerRingInputs.Instance.GreenLightKey.performed += GreenLightKeyPressed;
        PowerRingInputs.Instance.EnergyBubbleShieldKey.performed += EnergyBubbleShieldKeyPressed;
        PowerRingInputs.Instance.EnergyConstructKey.performed += EnergyConstructKeyPressed;
        PowerRingInputs.Instance.EnergyCageKey.performed += EnergyCageKeyPressed;
        PowerRingInputs.Instance.EnergyShieldKey.performed += EnergyShieldKeyPressed;
    }

    public void OnDisable()
    {
        PowerRingInputs.Instance.GreenLightKey.performed -= GreenLightKeyPressed;
        PowerRingInputs.Instance.EnergyBubbleShieldKey.performed -= EnergyBubbleShieldKeyPressed;
        PowerRingInputs.Instance.EnergyConstructKey.performed -= EnergyConstructKeyPressed;
        PowerRingInputs.Instance.EnergyCageKey.performed -= EnergyCageKeyPressed;
        PowerRingInputs.Instance.EnergyShieldKey.performed -= EnergyShieldKeyPressed;
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

    public override void ItemActivate(bool used, bool buttonDown = true) // Energy Beam
    {
        base.ItemActivate(used, buttonDown);

        // TODO: Find out if it should be a constant beam or a burst
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
