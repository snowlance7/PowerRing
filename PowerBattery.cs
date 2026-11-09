using Dawn;
using System.Linq;
using UnityEngine;

namespace PowerRing;

internal class PowerBattery : PhysicsProp
{
    public static bool IsEnabled => LethalContent.Items[PowerRingKeys.PowerBattery] != null;

    [SerializeField] AudioClip insertRingSFX = null!; // TODO
    [SerializeField] AudioClip chargeRingSFX = null!; // TODO

    InteractTrigger interactTrigger = null!;
    AudioSource audioSource = null!;

    public void Awake()
    {
        interactTrigger = gameObject.GetComponentInChildren<InteractTrigger>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    public override void Update()
    {
        base.Update();
        interactTrigger.interactable = PowerRing.Instances.Any(x => x.CanUseRing());
    }

    public void ChargeRing() // InteractTrigger
    {
        var ring = PowerRing.Instances.FirstOrDefault(x => x.CanUseRing());
        if (ring == null) { return; }

        audioSource.PlayOneShot(insertRingSFX);

        ring.QuickCastAbilitySynced(() =>
        {
            audioSource.PlayOneShot(chargeRingSFX);
            ring.ChargeRingRpc();
        });
    }
}