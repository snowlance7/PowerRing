using BepInEx;
using BepInEx.Logging;
using Dawn;
using Dusk;
using GameNetcodeStuff;
using HarmonyLib;
using SnowyLib;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace PowerRing;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(DawnLib.PLUGIN_GUID)]
[BepInDependency(SnowyLib.MyPluginInfo.PLUGIN_GUID)]
internal class Plugin : BaseUnityPlugin
{
    public static Plugin PluginInstance { get; private set; } = null!;
    public static ManualLogSource logger => PluginInstance.Logger;
    public static DuskMod Mod { get; private set; } = null!;

    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    public static PlayerControllerB localPlayer => StartOfRound.Instance.localPlayerController;
    public static PlayerControllerB? PlayerFromId(ulong id) => StartOfRound.Instance.allPlayerScripts.Where(x => x.actualClientId == id).FirstOrDefault();
    public static bool IsServerOrHost => NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost;

    private void Awake()
    {
        PluginInstance ??= this;

        harmony.PatchAll();

        AssetBundle? mainBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "powerring_mainassets"));
        Mod = DuskMod.RegisterMod(this, mainBundle);
        Mod.RegisterContentHandlers();

        InitializeNetworkBehaviours();

        // Finished
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private static void InitializeNetworkBehaviours()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
        logger?.LogDebug("Finished initializing network behaviours");
    }
}