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
    [HideInInspector] public PlayerControllerB? ringOwner;
    public AudioSource oathAudioSource = null!;
    public AudioClip oathSFX = null!;
    private Coroutine? oathRoutine;

    private void OathKeyPressed(InputAction.CallbackContext context) // Oath
    {
        if (!CanReciteOath() || oathRoutine != null || context.canceled) { return; }

        logger.LogDebug("Casting oath");

        CastAbilitySynced(() =>
        {
            IEnumerator Oath()
            {
                yield return null;

                logger.LogDebug("Starting oath");
                oathAudioSource.Play();
                logger.LogDebug("Starting oath");

                float elapsedTime = 0f;
                while (elapsedTime < 5f)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                    if (!CanReciteOath())
                    {
                        logger.LogDebug("Cant recite oath, stopping animation");
                        oathAudioSource.Stop();
                        StopCastRpc();
                        oathRoutine = null;
                        yield break;
                    }
                }

                logger.LogDebug("Changing ring ownership");
                ChangeRingOwnershipRpc(localPlayer.actualClientId);
                StopCastRpc();
                oathRoutine = null;
            }

            oathRoutine = StartCoroutine(Oath());

        }, maxGlowTime: 1f);
    }

    public bool OathRequirementMet()
    {
        return !OathRequired || LocalPlayerRecitedOath;
    }

    public bool CanReciteOath()
    {
        return CanUseRing(withAnimation: true, overrideOath: true)
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