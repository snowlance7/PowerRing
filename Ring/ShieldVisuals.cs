using EnergyShieldEffect;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using static PowerRing.Plugin;


namespace PowerRing;

[HarmonyPatch]
internal static class ShieldVisuals
{

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    public static void PlayerControllerB_ConnectClientToPlayerObject_Postfix(PlayerControllerB __instance)
    {
        try
        {
            if (localPlayer != __instance || !PowerRing.IsEnabled || !PowerRing.UsingEnergyShieldEffect) { return; }

            if (PowerRing.CustomPassVolume != null)
            {
                PowerRing.CustomPassVolume.targetCamera = localPlayer.gameplayCamera;
                PowerRing.CustomPassVolume.useTargetCamera = true;
            }

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

    public static void SetUpShieldVisualization()
    {
        if (!PowerRing.IsEnabled || !PowerRing.UsingEnergyShieldEffect) { return; }

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

            var instance = GameObject.Instantiate(PowerRingContentHandler.Instance.PowerRing!.ShieldCustomPassPrefab, renderingRoot);

            instance.name = "ShieldCustomPass";
            PowerRing.CustomPassVolume = instance.GetComponent<CustomPassVolume>();
        }
        else
        {
            logger.LogDebug("ShieldCustomPass already exists");
        }
    }
}