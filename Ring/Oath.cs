using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool OathRequired => PluginInstance.Config.Bind("Power Ring Options", "Power Ring | Oath Required", true, "If true, you will not be able to use the ring until you recite the oath").Value;

    public bool LocalPlayerRecitedOath => ringOwner != null && ringOwner == localPlayer;
    public PlayerControllerB? ringOwner;
    public AudioSource oathAudioSource = null!;
    public AudioClip oathSFX = null!;
    private Coroutine? oathRoutine;

    private void OathKeyPressed(InputAction.CallbackContext context) // Oath
    {
        if (!CanReciteOath() || oathRoutine != null) { return; }

        CastAbilitySynced(() =>
        {
            IEnumerator Oath()
            {
                yield return null;

                oathAudioSource.Play();
                while (oathAudioSource.isPlaying)
                {
                    yield return null;
                    if (!CanReciteOath())
                    {
                        oathAudioSource.Stop();
                        StopCastAnimationRpc();
                        oathRoutine = null;
                        yield break;
                    }
                }

                ChangeRingOwnershipRpc(localPlayer.actualClientId);
                StopCastAnimationRpc();
                oathRoutine = null;
            }

            oathRoutine = StartCoroutine(Oath());

        }, animationStartTime: 0f, maxGlowTime: 1f);
    }

    public bool OathRequirementMet()
    {
        return !OathRequired || LocalPlayerRecitedOath;
    }

    public bool CanReciteOath()
    {
        return CanUseRing(overrideOath: true)
            && !playerWornBy!.twoHanded
            && !playerWornBy.twoHandedAnimation
            && !playerWornBy.inSpecialInteractAnimation
            && !playerWornBy.isClimbingLadder
            && !playerWornBy.isGrabbingObjectAnimation
            && !OathRequirementMet();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ChangeRingOwnershipRpc(ulong clientId)
    {
        ringOwner = PlayerFromId(clientId);
    }
}