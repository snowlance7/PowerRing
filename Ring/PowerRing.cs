using Dawn;
using FXV;
using GameNetcodeStuff;
using HarmonyLib;
using SnowyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using WearableItemsAPI;
using static PowerRing.Plugin;

namespace PowerRing;

internal partial class PowerRing : WearableObject
{
    public static bool IsEnabled => LethalContent.Items[PowerRingKeys.PowerRing] != null;

    public Transform castDirection = null!;
    public ParticleSystem energyBlastParticleSystem = null!;
    public GameObject energyHitPrefab = null!;
    public ParticleSystem noPowerParticleSystem = null!;
    //public AudioClip oathSFX = null!;
    //public AudioClip energyBlastSFX = null!;

    public GameObject bubbleShieldPrefab = null!;

    public GameObject energyConstructPrefab = null!;

    public GameObject energyCagePrefab = null!;

    public GameObject energyShieldPrefab = null!;

    MeshRenderer renderer = null!;
    AudioSource audioSource = null!;

    [HideInInspector] public float power = 1f;
    [HideInInspector] public float powerUsage;

    float[] cooldowns = new float[7];

    public static int enemiesAndPlayersMask;
    public static int playerMask;
    public static int enemiesMask;
    public static int roomMask;
    public static int collidersMask;

    [HideInInspector] public bool isCasting;
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

    public static ulong[] RingRestrictedSteamIDs => ParseSteamIds(PluginInstance.Config.Bind("Power Ring Options", "Power Ring | Restricted Steam IDs", "", "List of steam ids separated by commas of players who can use the Power Ring. Leave blank to allow all players to use it.").Value); // TODO

    [StaticInit]
    public static void Init()
    {
        _ = RingRestrictedSteamIDs;

        _ = EnergyBlastEnemyKnockbackForce;
        _ = EnergyBlastPlayerKnockbackForce;
        _ = EnergyBlastEnemyDamage;
        _ = EnergyBlastPlayerDamage;

        _ = FlightHover;

        _ = ShowBubbleShieldOnClient;

        SetUpShieldVisualization();
    }

    private static void SetUpShieldVisualization()
    {
        if (!IsEnabled) { return; }

        var systemsRoot = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(obj => obj.name == "Systems");

        if (systemsRoot == null)
        {
            logger.LogError("Systems root not found");
            return;
        }

        var renderingRoot = systemsRoot.transform.Find("Rendering");

        if (renderingRoot == null)
        {
            logger.LogError("Systems/Rendering not found");
            return;
        }

        //var shieldCustomPass = PowerRingContentHandler.Instance.PowerRing!.ShieldCustomPassPrefab.GetComponent<CustomPassVolume>().customPasses[0];

        //var customPass = renderingRoot.Find("CustomPass (1)").GetComponent<CustomPassVolume>();
        //customPass.customPasses.Add(shieldCustomPass);
        //return;

        var shieldCustomPassTransform = renderingRoot.Find("ShieldCustomPass");

        if (shieldCustomPassTransform == null)
        {
            logger.LogDebug("Creating ShieldCustomPass");

            var instance = Instantiate(
                PowerRingContentHandler.Instance.PowerRing!
                    .ShieldCustomPassPrefab,
                renderingRoot);

            instance.name = "ShieldCustomPass";
        }
        else
        {
            logger.LogDebug("ShieldCustomPass already exists");
        }
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
        collidersMask = LayerMask.GetMask("Colliders");
    }

    public void OnEnable()
    {
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
        EnergyShieldUpdate();

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

        if (playerWornBy != null)
        {
            if (inCastingAnimation)
            {
                if (localPlayer == playerWornBy)
                {
                    playerWornBy.playerBodyAnimator.speed = 0f;
                }
                else
                {
                    if (playerWornBy.isWalking)
                    {
                        if (playerWornBy.playingQuickSpecialAnimation)
                        {
                            playerWornBy.playerBodyAnimator.StopPlayback(); // TODO: Test this
                            playerWornBy.playingQuickSpecialAnimation = false;
                        }
                    }
                    else
                    {
                        if (!playerWornBy.playingQuickSpecialAnimation)
                        {
                            playerWornBy.playingQuickSpecialAnimation = true;
                            playerWornBy.playerBodyAnimator.Play("ChargeItem", 5, 0.5f);
                        }

                        playerWornBy.playerBodyAnimator.speed = 0f;
                    }
                }
            }
        }
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

    public bool CanCastAbility(Ability ability, bool withAnimation)
    {
        return CanUseRing()
            && IsOffCooldown(ability) 
            && CanUsePower(AbilityPowerUsageOnCast[(int)ability])
            && (!withAnimation || (!playerWornBy!.twoHanded && !playerWornBy.twoHandedAnimation && !playerWornBy.inSpecialInteractAnimation && !playerWornBy.isClimbingLadder && !playerWornBy.isGrabbingObjectAnimation)); // TODO: Test this
    }

    public bool CanCastAbility(Ability ability)
    {
        return CanUseRing()
            && IsOffCooldown(ability)
            && CanUsePower(AbilityPowerUsageOnCast[(int)ability])
            && (!inCastingAnimation || (!playerWornBy!.twoHanded && !playerWornBy.twoHandedAnimation && !playerWornBy.inSpecialInteractAnimation && !playerWornBy.isClimbingLadder && !playerWornBy.isGrabbingObjectAnimation)); // TODO: Test this
    }

    public bool CanUseRing()
    {
        return playerWornBy != null
            && playerWornBy == localPlayer
            && !playerWornBy.isPlayerDead
            && playerWornBy.isPlayerControlled
            && !playerWornBy.inSpecialMenu
            && !playerWornBy.inShockingMinigame
            && !playerWornBy.inTerminalMenu
            && !playerWornBy.isTypingChat;
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

    public void LowPowerEffects()
    {
        // TODO: in the show, the ring flashes on and off to notify them its low power
    }

    public void CastAbilitySynced(Action action, bool doPlayerCastAnimation = true, float animationStartTime = 0.5f, float maxGlowTime = 0.1f)
    {
        StartCastAnimationRpc(doPlayerCastAnimation, animationStartTime, maxGlowTime);

        IEnumerator castAbility(Action action)
        {
            yield return null;
            action.Invoke();
        }

        StartCoroutine(castAbility(action));
    }

    public void QuickCastAbilitySynced(Action action, bool doPlayerCastAnimation = true)
    {
        DoQuickCastAnimationRpc(doPlayerCastAnimation);

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

    public void StartCastAnimation(bool doCastingAnimation = true, float animationStartTime = 0.5f, float maxGlowTime = 0.1f)
    {
        isCasting = true;
        if (playerWornBy != null && doCastingAnimation)
        {
            inCastingAnimation = true;
            playerWornBy.playingQuickSpecialAnimation = true;
            playerWornBy.playerBodyAnimator.Play("ChargeItem", 5, animationStartTime);
        }

        IEnumerator startCastAnimation(float maxGlowTime)
        {
            float elapsedTime = 0f;

            while (elapsedTime <= maxGlowTime)
            {
                elapsedTime += Time.deltaTime;
                SetRingGlow(elapsedTime / maxGlowTime);
                yield return null;
            }
        }

        StartCoroutine(startCastAnimation(maxGlowTime));
    }

    public void StopCastAnimation(float minGlowTime = 1f)
    {
        isCasting = false;
        if (playerWornBy != null && inCastingAnimation)
        {
            playerWornBy.playingQuickSpecialAnimation = false;
        }
        inCastingAnimation = false;

        IEnumerator stopCastAnimation(float minGlowTime)
        {
            float elapsedTime = 0f;

            while (elapsedTime <= minGlowTime)
            {
                elapsedTime += Time.deltaTime;
                SetRingGlow(1 - (elapsedTime / minGlowTime));
                yield return null;
            }
        }

        StartCoroutine(stopCastAnimation(minGlowTime));
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void StartCastAnimationRpc(bool doCastingAnimation = true, float animationStartTime = 0.5f, float maxGlowTime = 0.1f) // TODO: Test this
    {
        StartCastAnimation(doCastingAnimation, animationStartTime, maxGlowTime);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void StopCastAnimationRpc(float minGlowTime = 1f) // TODO: Test this
    {
        StopCastAnimation(minGlowTime);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void DoQuickCastAnimationRpc(bool doCastingAnimation = true) // TODO: Test this
    {
        if (playerWornBy == null) { return; }
        inQuickCastingAnimation = true;
        StartCastAnimation(doCastingAnimation);
        Utils.InvokeAfterDelay(() =>
        {
            StopCastAnimation();
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
                StopCastAnimation();
            }

            StartCoroutine(castAbilityFailed());
        }
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
                shield.globalIllumination = false;
            }
        }
        catch (System.Exception e)
        {
            logger.LogError(e);
            return;
        }
    }
}