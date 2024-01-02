using BepInEx;
using System.Runtime.CompilerServices;
using System;
using BepInEx.Logging;
using UnityEngine;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;

namespace EnhancedRadarBooster
{
    [BepInPlugin(MOD_GUID, MOD_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private const string MOD_GUID = "MrHydralisk.EnhancedRadarBooster";
        private const string MOD_NAME = "Enhanced Radar Booster";

        public static Plugin instance;

        public static ManualLogSource MLogS;
        public static ConfigFile config;
        public GameObject enhancedRadarBoosterNetworkManager;

        private void Awake()
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

            MLogS = BepInEx.Logging.Logger.CreateLogSource(MOD_GUID);
            config = Config;
            EnhancedRadarBooster.Config.Load();
            instance = this;
            try
            {
                RuntimeHelpers.RunClassConstructor(typeof(HarmonyPatches).TypeHandle);
            }
            catch (Exception ex)
            {
                MLogS.LogError(string.Concat("Error in static constructor of ", typeof(HarmonyPatches), ": ", ex));
            }
            LoadBundle();
            MLogS.LogInfo($"Plugin is loaded!");
        }

        private void LoadBundle()
        {
            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "enhancedradarbooster");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            enhancedRadarBoosterNetworkManager = bundle.LoadAsset<GameObject>("Assets/Mods/EnhancedRadarBooster/EnhancedRadarBoosterNetworkManager.prefab");
            enhancedRadarBoosterNetworkManager.AddComponent<EnhancedRadarBoosterNetworkHandler>();
        }
    }
}