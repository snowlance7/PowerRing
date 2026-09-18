using Something;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PowerRing
{
    internal class PowerRingBehavior : PhysicsProp
    {
        public MeshRenderer renderer = null!;

        public override void Start()
        {
            base.Start();
            PowerRingInputs.Instance.GreenLightKey.performed += GreenLightKeyPressed;
            PowerRingInputs.Instance.GreenLightKey.performed += EnergyBubbleShieldKeyPressed;
            PowerRingInputs.Instance.GreenLightKey.performed += EnergyConstructKeyPressed;
            PowerRingInputs.Instance.GreenLightKey.performed += EnergyCageKeyPressed;
            PowerRingInputs.Instance.GreenLightKey.performed += EnergyShieldKeyPressed;
        }

        public override void OnDestroy()
        {
            PowerRingInputs.Instance.GreenLightKey.performed -= GreenLightKeyPressed;
            PowerRingInputs.Instance.GreenLightKey.performed -= EnergyBubbleShieldKeyPressed;
            PowerRingInputs.Instance.GreenLightKey.performed -= EnergyConstructKeyPressed;
            PowerRingInputs.Instance.GreenLightKey.performed -= EnergyCageKeyPressed;
            PowerRingInputs.Instance.GreenLightKey.performed -= EnergyShieldKeyPressed;
            base.OnDestroy();
        }

        public override void EquipItem()
        {
            base.EquipItem();
            playerHeldBy.equippedUsableItemQE = true;
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

        }

        public override void ItemInteractLeftRight(bool right) // Flight
        {
            base.ItemInteractLeftRight(right);
            if (right) { return; }

        }

        private void EnergyShieldKeyPressed(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        private void EnergyCageKeyPressed(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        private void EnergyConstructKeyPressed(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        private void EnergyBubbleShieldKeyPressed(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        private void GreenLightKeyPressed(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }
    }
}
