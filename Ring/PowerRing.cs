using Dawn;
using GameNetcodeStuff;
using SnowyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static readonly bool UsingEnergyShieldEffect = true;
    public static List<PowerRing> Instances { get; private set; } = [];
    public static bool IsEnabled => LethalContent.Items[PowerRingKeys.PowerRing] != null;

    public static CustomPassVolume? CustomPassVolume;

    public Transform castDirection = null!;
    public ParticleSystem energyBlastParticleSystem = null!;
    public ParticleSystem noPowerParticleSystem = null!;
    public GameObject greenLightObj = null!;

    public AudioClip lowPowerSFX = null!;
    public AudioClip noPowerSFX = null!;
    public AudioClip notEnoughPowerSFX = null!;

    MeshRenderer renderer = null!;
    AudioSource audioSource = null!;

    [HideInInspector] public float power = 1f;
    [HideInInspector] public float powerUsage;
    bool noPowerTriggered;
    bool lowPowerTriggered;

    float[] cooldowns = new float[7];

    public static int enemiesAndPlayersMask;
    public static int playerMask;
    public static int enemiesMask;
    public static int roomMask;
    public static int miscLevelGeometryMask;

    [HideInInspector] public bool isCasting { get; set; }
    [HideInInspector] public bool inQuickCastingAnimation;
    [HideInInspector] public bool inCastingAnimation;

    public readonly Vector3 castLocalPositionOffset = new Vector3(-0.0141f, 0.0376f, 0.0576f);
    public readonly Vector3 castLocalRotationOffset = new Vector3(-30.894f, -92.448f, -4.496f);
    public readonly Vector3 castPositionOffset = new Vector3(-0.0453f, 0.0147f, 0.051f);
    public readonly Vector3 castRotationOffset = new Vector3(-30.894f, -92.448f, -4.496f);

    [HideInInspector] public PowerRingConstruct? lastConstruct;

    public bool LocalPlayerUsing => playerHeldBy != null && localPlayer == playerHeldBy && !isPocketed;

    public enum Ability
    {
        GreenLight = 0,
        BubbleShield = 1,
        EnergyConstruct = 2,
        EnergyBlast = 3,
        Flight = 4,
        EnergyCage = 5,
        EnergyShield = 6
    }

    public static float LowPowerWarning => PluginInstance.Config.Bind("Power Ring Options", "Power Ring | Low Power Warning", 0.25f, "When the rings power is lower than this, it will trigger a low power warning to warn the player").Value;
    public static ulong[] RingRestrictedSteamIDs => ParseSteamIds(PluginInstance.Config.Bind("Power Ring Options", "Power Ring | Restricted Steam IDs", "", "List of steam ids separated by commas of players who can use the Power Ring. Leave blank to allow all players to use it.").Value); // TODO

    [StaticInit]
    public static void Init()
    {
        _ = RingRestrictedSteamIDs;

        _ = ShowBubbleShieldOnClient;
        _ = MaxSharedBubbleShields;

        _ = EnergyBlastEnemyKnockbackForce;
        _ = EnergyBlastPlayerKnockbackForce;
        _ = EnergyBlastEnemyDamage;
        _ = EnergyBlastPlayerDamage;

        _ = MaxEnergyCages;

        _ = MaxEnergyConstructs;

        _ = FlightHover;
        _ = FlightSpeed;

        _ = OathRequired;

        ShieldVisuals.SetUpShieldVisualization();
    }

    public void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        renderer = GetComponentInChildren<MeshRenderer>();
        itemProperties.positionOffset = new Vector3(0.06f, 0.1f, 0.01f);
        itemProperties.rotationOffset = new Vector3(45, 110, 0);
        enemiesAndPlayersMask = LayerMask.GetMask("Enemies", "Player");
        playerMask = LayerMask.GetMask("Player");
        enemiesMask = LayerMask.GetMask("Enemies");
        roomMask = LayerMask.GetMask("Room");
        miscLevelGeometryMask = LayerMask.GetMask("MiscLevelGeometry");
    }

    public override void Start()
    {
        base.Start();
        Instances.Add(this);
    }

    public override void OnDestroy()
    {
        Instances.Remove(this);
        base.OnDestroy();
    }

    public void OnEnable()
    {
        logger.LogDebug("Enabling ring keybinds"); // TODO: Test this
        PowerRingInputs.Instance.GreenLightKey.started += GreenLightKeyPressed;
        PowerRingInputs.Instance.GreenLightKey.canceled += GreenLightKeyPressed;

        PowerRingInputs.Instance.BubbleShieldKey.started += BubbleShieldKeyPressed;
        PowerRingInputs.Instance.BubbleShieldKey.canceled += BubbleShieldKeyPressed;

        PowerRingInputs.Instance.SharedBubbleShieldKey.started += SharedBubbleShieldKeyPressed;
        PowerRingInputs.Instance.SharedBubbleShieldKey.canceled += SharedBubbleShieldKeyPressed;

        PowerRingInputs.Instance.EnergyConstructKey.started += EnergyConstructKeyPressed;
        PowerRingInputs.Instance.EnergyConstructKey.canceled += EnergyConstructKeyPressed;

        PowerRingInputs.Instance.EnergyBlastKey.performed += EnergyBlastKeyPressed;

        PowerRingInputs.Instance.FlightKey.started += FlightKeyPressed;
        PowerRingInputs.Instance.FlightKey.canceled += FlightKeyPressed;

        PowerRingInputs.Instance.EnergyCageKey.started += EnergyCageKeyPressed;
        PowerRingInputs.Instance.EnergyCageKey.canceled += EnergyCageKeyPressed;

        PowerRingInputs.Instance.EnergyShieldKey.started += EnergyShieldKeyPressed;
        PowerRingInputs.Instance.EnergyShieldKey.canceled += EnergyShieldKeyPressed;

        PowerRingInputs.Instance.OathKey.performed += OathKeyPressed;
    }

    public void OnDisable()
    {
        logger.LogDebug("Disabling ring keybinds"); // TODO: Test this
        PowerRingInputs.Instance.GreenLightKey.started -= GreenLightKeyPressed;
        PowerRingInputs.Instance.GreenLightKey.canceled -= GreenLightKeyPressed;

        PowerRingInputs.Instance.BubbleShieldKey.started -= BubbleShieldKeyPressed;
        PowerRingInputs.Instance.BubbleShieldKey.canceled -= BubbleShieldKeyPressed;

        PowerRingInputs.Instance.SharedBubbleShieldKey.started -= SharedBubbleShieldKeyPressed;
        PowerRingInputs.Instance.SharedBubbleShieldKey.canceled -= SharedBubbleShieldKeyPressed;

        PowerRingInputs.Instance.EnergyConstructKey.started -= EnergyConstructKeyPressed;
        PowerRingInputs.Instance.EnergyConstructKey.canceled -= EnergyConstructKeyPressed;

        PowerRingInputs.Instance.EnergyBlastKey.performed -= EnergyBlastKeyPressed;

        PowerRingInputs.Instance.FlightKey.started -= FlightKeyPressed;
        PowerRingInputs.Instance.FlightKey.canceled -= FlightKeyPressed;

        PowerRingInputs.Instance.EnergyCageKey.started -= EnergyCageKeyPressed;
        PowerRingInputs.Instance.EnergyCageKey.canceled -= EnergyCageKeyPressed;

        PowerRingInputs.Instance.EnergyShieldKey.started -= EnergyShieldKeyPressed;
        PowerRingInputs.Instance.EnergyShieldKey.canceled -= EnergyShieldKeyPressed;

        PowerRingInputs.Instance.OathKey.performed -= OathKeyPressed;
    }

    public override void Update()
    {
        base.Update();

        UsePower(powerUsage);

        if (power < LowPowerWarning && !lowPowerTriggered)
        {
            lowPowerTriggered = true;
            OnLowPower();
        }

        if (power <= 0 && !noPowerTriggered)
        {
            noPowerTriggered = true;
            OnNoPower();
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

        if (playerWornBy != null)
        {
            if (inCastingAnimation)
            {
                if (!playerWornBy.isHoldingObject)
                {

                }
            }
        }

        FlightUpdate();
        GreenLightUpdate();
    }

    public override void LateUpdate()
    {
        if (inCastingAnimation && parentObject != null && playerWornBy != null)
        {
            transform.rotation = parentObject.rotation;
            transform.Rotate(wearableItemProperties.useLocalOffsets && localPlayer == playerWornBy ? castLocalRotationOffset : castRotationOffset);
            transform.position = parentObject.position;
            Vector3 positionOffset = wearableItemProperties.useLocalOffsets && localPlayer == playerWornBy ? castLocalPositionOffset : castPositionOffset;
            positionOffset = parentObject.rotation * positionOffset;
            transform.position += positionOffset;

            if (rotateObject)
            {
                base.transform.Rotate(new Vector3(0f, Time.deltaTime * 60f, 0f), Space.World);
            }
            if (radarIcon != null)
            {
                radarIcon.position = transform.position;
            }
        }
        else
        {
            base.LateUpdate();
        }
    }

    public override void PocketItem()
    {
        SetPowerRpc(power);
        base.PocketItem();
    }
    public override void DiscardItem()
    {
        SetPowerRpc(power);
        base.DiscardItem();
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        if (!buttonDown) { return; }

        WearItem();
    }

    public bool CanContinueCasting()
    {

    }

    //public bool CanCastAbilityWithAnimation(Ability ability, bool withAnimation)
    //{
    //    return CanUseRing()
    //        && IsOffCooldown(ability)
    //        && CanUsePower(AbilityPowerUsageOnCast[(int)ability])
    //        && (!withAnimation || (!playerWornBy!.twoHanded && !playerWornBy.twoHandedAnimation && !playerWornBy.inSpecialInteractAnimation && !playerWornBy.isClimbingLadder && !playerWornBy.isGrabbingObjectAnimation)); // TODO: Test this
    //}

    public bool CanCastAbility(Ability ability)
    {
        return CanUseRing()
            && IsOffCooldown(ability)
            && CanUsePower(AbilityPowerUsageOnCast[(int)ability])
            && (!inCastingAnimation || (!playerWornBy!.twoHanded && !playerWornBy.twoHandedAnimation && !playerWornBy.inSpecialInteractAnimation && !playerWornBy.isClimbingLadder && !playerWornBy.isGrabbingObjectAnimation)); // TODO: Test this
    }

    public bool CanUseRing(bool withAnimation = false, bool overrideOath = false)
    {
        return playerWornBy != null
            && playerWornBy == localPlayer
            && !playerWornBy.isPlayerDead
            && playerWornBy.isPlayerControlled
            && !playerWornBy.inSpecialMenu
            && !playerWornBy.inShockingMinigame
            && !playerWornBy.inTerminalMenu
            && !playerWornBy.isTypingChat
            && (overrideOath || OathRequirementMet())
            && (!withAnimation || (!playerWornBy.twoHanded && !playerWornBy.twoHandedAnimation && !playerWornBy.inSpecialInteractAnimation && !playerWornBy.isClimbingLadder && !playerWornBy.isGrabbingObjectAnimation));
    }

    public bool CanUsePower(float powerAmount)
    {
        float newPowerAmount = power - powerAmount;
        if (newPowerAmount < 0)
        {
            audioSource.PlayOneShot(notEnoughPowerSFX);
            return false;
        }
        return true;
    }

    public void UsePower(float powerAmount)
    {
        SetPower(power - powerAmount);
    }

    public void OnNoPower()
    {
        // TODO
        ToggleFlight(false);
        ToggleGreenLight(false);
        audioSource.PlayOneShot(noPowerSFX);
    }

    public void OnLowPower()
    {
        // TODO: in the show, the ring flashes on and off to notify them its low power
        audioSource.PlayOneShot(lowPowerSFX);
    }

    public void CastAbilitySynced(Action action, bool doPlayerCastAnimation = true, float maxGlowTime = 0.1f)
    {
        StartCastRpc(doPlayerCastAnimation, maxGlowTime);

        IEnumerator castAbility(Action action)
        {
            yield return null;
            action.Invoke();
        }

        StartCoroutine(castAbility(action));
    }

    public void QuickCastAbilitySynced(Action action, bool doPlayerCastAnimation = true)
    {
        DoQuickCastRpc(doPlayerCastAnimation);

        IEnumerator quickCastAbility(Action action)
        {
            yield return null;
            action.Invoke();
        }

        StartCoroutine(quickCastAbility(action));
    }

    public void SetPower(float powerAmount)
    {
        power = Mathf.Clamp01(powerAmount);
    }

    public void SetCooldown(Ability ability)
    {
        cooldowns[(int)ability] = AbilityCooldown[(int)ability];
    }

    public bool IsOffCooldown(Ability ability)
    {
        return cooldowns[(int)ability] <= 0;
    }

    public PlayerControllerB? GetPlayerLookingAt(float distance, float radius = 2f)
    {
        //if (!Physics.Raycast(localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward, localPlayer.gameplayCamera.transform.forward, out RaycastHit hit, distance, playerMask)) { return null; }
        if (!Physics.SphereCast(localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward, radius, localPlayer.gameplayCamera.transform.forward, out RaycastHit hit, distance, playerMask)) { return null; } // TODO: Test this
        return hit.collider.GetComponent<PlayerControllerB>();
    }

    public EnemyAI? GetEnemyLookingAt(float distance, float radius = 2f)
    {
        //if (!Physics.Raycast(localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward, localPlayer.gameplayCamera.transform.forward, out RaycastHit hit, distance, enemiesMask)) { return null; }
        if (!Physics.SphereCast(localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward, radius, localPlayer.gameplayCamera.transform.forward, out RaycastHit hit, distance, enemiesMask)) { return null; } // TODO: Test this
        var enemyCollision = hit.collider.GetComponent<EnemyAICollisionDetect>();
        if (enemyCollision == null) { return null; }
        return enemyCollision.mainScript;
    }

    public List<EnemyAI> GetEnemiesLookingAt(float distance, float radius = 2f)
    {
        List<EnemyAI> enemies = [];
        RaycastHit[] hits = Physics.SphereCastAll(localPlayer.gameplayCamera.transform.position + localPlayer.gameplayCamera.transform.forward, radius, localPlayer.gameplayCamera.transform.forward, distance, enemiesMask);

        foreach (var hit in hits)
        {
            var collisionDetect = hit.collider.gameObject.GetComponent<EnemyAICollisionDetect>();
            if (collisionDetect == null) { continue; }
            enemies.Add(collisionDetect.mainScript);
        }

        return enemies;
    }

    public void SetRingGlow(float strength)
    {
        renderer.material.SetFloat("_EmissiveIntensity", Mathf.Lerp(0, 10, strength));
    }

    public void StartCastAnimation()
    {
        inCastingAnimation = true;
        // TODO: Do HoldOneHandedItem animation here instead, ChargeItem breaks
        //playerWornBy.playingQuickSpecialAnimation = true;
        //playerWornBy.playerBodyAnimator.Play("ChargeItem", 5, 0.5f);
    }

    public void StopCastAnimation()
    {
        inCastingAnimation = false;
    }

    public void StartCast(bool doCastingAnimation = true, float maxGlowTime = 0.1f)
    {
        isCasting = true;
        if (playerWornBy != null && doCastingAnimation)
        {
            StartCastAnimation();
        }

        IEnumerator startCast(float maxGlowTime)
        {
            float elapsedTime = 0f;

            while (elapsedTime <= maxGlowTime)
            {
                elapsedTime += Time.deltaTime;
                SetRingGlow(elapsedTime / maxGlowTime);
                yield return null;
            }
        }

        StartCoroutine(startCast(maxGlowTime));
    }

    public void StopCast(float minGlowTime = 1f)
    {
        isCasting = false;
        if (playerWornBy != null && inCastingAnimation)
        {
            StopCastAnimation();
        }
        inCastingAnimation = false;

        IEnumerator stopCast(float minGlowTime)
        {
            float elapsedTime = 0f;

            while (elapsedTime <= minGlowTime)
            {
                elapsedTime += Time.deltaTime;
                SetRingGlow(1 - (elapsedTime / minGlowTime));
                yield return null;
            }
        }

        StartCoroutine(stopCast(minGlowTime));
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void StartCastRpc(bool doCastingAnimation = true, float maxGlowTime = 0.1f) // TODO: Test this
    {
        StartCast(doCastingAnimation, maxGlowTime);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void StopCastRpc(float minGlowTime = 1f) // TODO: Test this
    {
        StopCast(minGlowTime);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DoQuickCastRpc(bool doCastingAnimation = true) // TODO: Test this
    {
        if (playerWornBy == null) { return; }
        inQuickCastingAnimation = true;
        StartCast(doCastingAnimation);
        Utils.InvokeAfterDelay(() =>
        {
            StopCast();
            inQuickCastingAnimation = false;
        }, 1f);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void CastAbilityFailedRpc()
    {
        if (inCastingAnimation)
        {
            noPowerParticleSystem.Play();
        }

        if (isCasting && !inQuickCastingAnimation)
        {
            IEnumerator castAbilityFailed()
            {
                yield return null;
                StopCast();
            }

            StartCoroutine(castAbilityFailed());
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SetPowerRpc(float powerAmount)
    {
        SetPower(powerAmount);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void ChargeRingRpc()
    {
        lowPowerTriggered = false;
        noPowerTriggered = false;
        power = 1f;
    }

    public static ulong[] ParseSteamIds(string input)
    {
        string[] output = input.Split(",");
        if (output.Length == 0) { return []; }

        List<ulong> ulongs = new List<ulong>();

        foreach (var item in output)
        {
            if (!ulong.TryParse(item, out ulong result)) { continue; }
            ulongs.Add(result);
        }

        return ulongs.ToArray();
    }
}