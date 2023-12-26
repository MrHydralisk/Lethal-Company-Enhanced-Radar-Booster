using BepInEx;
using System.Runtime.CompilerServices;
using System;
using BepInEx.Logging;

namespace EnhancedRadarBooster
{
    [BepInPlugin(MOD_GUID, MOD_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private const string MOD_GUID = "MrHydralisk.EnhancedRadarBooster";
        private const string MOD_NAME = "Enhanced Radar Booster";

        public static ManualLogSource MLogS;
		
        private void Awake()
        {
            MLogS = BepInEx.Logging.Logger.CreateLogSource(MOD_GUID);
            try
            {
                RuntimeHelpers.RunClassConstructor(typeof(HarmonyPatches).TypeHandle);
            }
            catch (Exception ex)
            {
                MLogS.LogError(string.Concat("Error in static constructor of ", typeof(HarmonyPatches), ": ", ex));
            }
            MLogS.LogInfo($"Plugin is loaded!");
        }
    }
}