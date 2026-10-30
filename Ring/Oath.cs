using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public bool OathRequired => PluginInstance.Config.Bind("Power Ring Options", "Power Ring | Oath Required", true, "If true, you will not be able to use the ring until you recite the oath").Value;

    public AudioClip oathSFX = null!;
    private Coroutine? oathRoutine;

    private void OathKeyPressed(InputAction.CallbackContext context) // Oath
    {
        if (!CanUseRing(overrideOath: true)) { return; }

        CastAbilitySynced(() =>
        {
            // TODO
        }, animationStartTime: 0f, maxGlowTime: 1f);
    }

    IEnumerator Oath()
    {
        yield return null;
        
        // TODO
    }
}