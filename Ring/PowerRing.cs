using Dawn;
using FXV;
using GameNetcodeStuff;
using HarmonyLib;
using SnowyLib;
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
    public static bool IsEnabled => LethalContent.Items[PowerRingKeys.PowerRing] != null;

    public MeshRenderer renderer = null!;
    public Transform castDirection = null!;
    public ParticleSystem energyBlastParticleSystem = null!;
    public GameObject energyHitPrefab = null!;
    public ParticleSystem noPowerParticleSystem = null!;
    public AudioSource audioSource = null!;
    public AudioClip oathSFX = null!;
    public AudioClip energyBlastSFX = null!;

    public GameObject bubbleShieldPrefab = null!;

    public GameObject energyConstructPrefab = null!;

    public GameObject energyCagePrefab = null!;

    public GameObject energyShieldObj = null!;

    [HideInInspector]
    public float power = 1f;
    [HideInInspector]
    public float powerUsage;

    float[] cooldowns = new float[7];

    int enemiesAndPlayersMask;

    public bool LocalPlayerUsing => playerHeldBy != null && localPlayer == playerHeldBy && !isPocketed;

    public enum Ability
    {
        GreenLight,
        BubbleShield,
        EnergyConstruct,
        EnergyBlast,
        Flight,
        EnergyCage,
        EnergyShield,
        Oath
    }

    // TODO: Configs to add:
    // ring usable when holding one handed item
    // ring usable when holding two handed item

    public static ulong[] RingRestrictedSteamIDs => ParseSteamIds(PluginInstance.Config.Bind("Power Ring Options", "Power Ring | Restricted Steam IDs", "", "List of steam ids separated by commas of players who can use the Power Ring. Leave blank to allow all players to use it.").Value);

    [StaticInit]
    public static void Init()
    {
        _ = RingRestrictedSteamIDs;

        _ = GreenLightToggle;
        _ = GreenLightPowerUsageOnCast;
        _ = GreenLightPowerUsagePerSecond;

        _ = BubbleShieldToggle;
        _ = BubbleShieldPowerUsageOnCast;
        _ = BubbleShieldPowerUsagePerSecond;
        _ = BubbleShieldDeactivateOnDiscard;
        _ = BubbleShieldDuration;
        _ = BubbleShieldCooldown;

        _ = EnergyConstructToggle;
        _ = EnergyConstructPowerUsageOnCast;
        _ = EnergyConstructPowerUsagePerSecond;
        _ = EnergyConstructDeactivateOnDiscard;
        _ = EnergyConstructDuration;
        _ = EnergyConstructCooldown;

        _ = EnergyBlastPowerUsageOnCast;
        _ = EnergyBlastCooldown;
        _ = EnergyBlastEnemyKnockbackForce;
        _ = EnergyBlastPlayerKnockbackForce;
        _ = EnergyBlastEnemyDamage;
        _ = EnergyBlastPlayerDamage;

        _ = FlightToggle;
        _ = FlightPowerUsageOnCast;
        _ = FlightPowerUsagePerSecond;
        _ = FlightDeactivateOnDiscard;
        _ = FlightDuration;
        _ = FlightCooldown;
        _ = FlightHover;

        _ = EnergyTrapToggle;
        _ = EnergyTrapPowerUsageOnCast;
        _ = EnergyTrapPowerUsagePerSecond;
        _ = EnergyTrapDeactivateOnDiscard;
        _ = EnergyTrapDuration;
        _ = EnergyTrapCooldown;

        _ = EnergyShieldToggle;
        _ = EnergyShieldPowerUsageOnCast;
        _ = EnergyShieldPowerUsagePerSecond;
        _ = EnergyShieldDuration;
        _ = EnergyShieldCooldown;

        SetUpShieldVisualization();
    }

    private static void SetUpShieldVisualization()
    {
        if (!IsEnabled) { return; }

        var systemsRoot = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(obj => obj.name == "Systems");
        var renderingRoot = systemsRoot.transform.Find("Rendering");
        var shieldCustomPassTransform = renderingRoot.transform.Find("ShieldCustomPass");

        if (shieldCustomPassTransform == null)
        {
            logger.LogDebug("Creating ShieldCustomPass");
            GameObject shieldCustomPassObj = Instantiate(PowerRingContentHandler.Instance.PowerRing!.ShieldCustomPassPrefab, renderingRoot);
            DontDestroyOnLoad(shieldCustomPassObj);
        }
    }

    public void Awake()
    {
        itemProperties.positionOffset = new Vector3(0.06f, 0.1f, 0.01f);
        itemProperties.rotationOffset = new Vector3(45, 110, 0);
        enemiesAndPlayersMask = LayerMask.GetMask("Enemies", "Player");

        wearableItemProperties.showWearableOnClient = true; // .7
        wearableItemProperties.showWearable = true;

        wearableItemProperties.useLocalOffsets = true;

        wearableItemProperties.boneTransform = "spine.001/spine.002/spine.003/shoulder.R/arm.R_upper/arm.R_lower/hand.R/finger4.R";
        wearableItemProperties.boneTransformLocal = "metarig/spine.003/shoulder.R/arm.R_upper/arm.R_lower/hand.R/finger4.R";

        wearableItemProperties.wornScaleLocal = new Vector3(0.7f, 0.7f, 0.7f);
        wearableItemProperties.wornPositionOffsetLocal = new Vector3(-0.0079f, 0.0024f, 0.0003f);
        wearableItemProperties.wornRotationOffsetLocal = new Vector3(14.245f, -92.178f, -0.946f);
        //wearableItemProperties.wornPositionOffsetLocal = new Vector3(0.03f, -0.05f, 0f);
        //wearableItemProperties.wornRotationOffsetLocal = new Vector3(30, -90, 0);

        wearableItemProperties.wornScale = new Vector3(0.7f, 0.7f, 0.7f);
        wearableItemProperties.wornPositionOffset = new Vector3(-0.0067f, 0.0013f, 0.0016f);
        wearableItemProperties.wornRotationOffset = new Vector3(8.992f, -80.749f, 1.765f);
    }

    public void OnEnable()
    {
        PowerRingInputs.Instance.GreenLightKey.started += GreenLightKeyPressed;
        PowerRingInputs.Instance.GreenLightKey.canceled += GreenLightKeyPressed;

        PowerRingInputs.Instance.BubbleShieldKey.started += BubbleShieldKeyPressed;
        PowerRingInputs.Instance.BubbleShieldKey.canceled += BubbleShieldKeyPressed;

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
        PowerRingInputs.Instance.GreenLightKey.started -= GreenLightKeyPressed;
        PowerRingInputs.Instance.GreenLightKey.canceled -= GreenLightKeyPressed;

        PowerRingInputs.Instance.BubbleShieldKey.started -= BubbleShieldKeyPressed;
        PowerRingInputs.Instance.BubbleShieldKey.canceled -= BubbleShieldKeyPressed;

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

    public void UsedUpPowerOnLocalClient()
    {
        DoCastFailedEffects();
        power = 0f;
    }

    public bool CanUsePower(float powerAmount)
    {
        float newPowerAmount = power - powerAmount;
        if (newPowerAmount < 0) { return false; }
        return true;
    }

    public void UsePower(float powerAmount)
    {
        SetPower(power - powerAmount);
    }

    public void SetPower(float powerAmount)
    {
        power = Mathf.Clamp01(powerAmount);
    }

    public void SetCooldown(Ability ability, float value)
    {
        cooldowns[(int)ability] = value;
    }

    public bool IsOffCooldown(Ability ability)
    {
        return cooldowns[(int)ability] <= 0;
    }

    public void DoCastFailedEffects()
    {
        noPowerParticleSystem.Play();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void UsedUpPowerRpc()
    {
        UsedUpPowerOnLocalClient();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DoCastFailedEffectsRpc()
    {
        DoCastFailedEffects();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SetPowerRpc(float powerAmount)
    {
        SetPower(powerAmount);
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

[HarmonyPatch]
internal static class PowerRingPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    public static void PlayerControllerB_ConnectClientToPlayerObject_Postfix(PlayerControllerB __instance)
    {
        try
        {
            if (localPlayer != __instance || !PowerRing.IsEnabled) { return; }
            if (localPlayer.gameplayCamera.gameObject.GetComponent<ShieldPostprocess>() == null)
            {
                logger.LogDebug("Adding ShieldPostprocess script to player camera");
                ShieldPostprocess shield = localPlayer.gameplayCamera.gameObject.AddComponent<ShieldPostprocess>();
                shield.blurMethod = ShieldPostprocess.BlurMethod.GaussianOnly;
                shield.postprocessPower = 4;
                shield.globalIllumination = true;
                shield.giSampleRadius = 2;
                shield.giIntensity = 0.25f;
                shield.giDenoiseStepWidth = 2;
                shield.giAtIteration = 2;
            }
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }
}