using BepInEx.Configuration;
using Unity.Netcode;

namespace EnhancedRadarBooster
{
    public class Config
    {
        public static ConfigEntry<bool> eRBNHEnabled;

        public static ConfigEntry<bool> mapRangeRBEnabled;
        public static ConfigEntry<float> mapRangeRBMultiplier;
        public static ConfigEntry<bool> mapRangeRBSync;

        public static ConfigEntry<bool> tpRBEnabled;
        public static ConfigEntry<bool> itpRBEnabled;
        public static ConfigEntry<bool> itpToRBEnabled;
        public static ConfigEntry<bool> tpRBSync;

        public static ConfigEntry<bool> remoteScrapRBFlashEnabled;
        public static ConfigEntry<float> remoteScrapRBFlashRange;
        public static ConfigEntry<bool> remoteScrapRBFlashSync;

        
        public static bool eRBNHEnabledValue;

        public static bool mapRangeRBEnabledValue;
        public static float mapRangeRBMultiplierValue;
        public static bool mapRangeRBSyncValue;

        public static bool tpRBEnabledValue;
        public static bool itpRBEnabledValue;
        public static bool itpToRBEnabledValue;
        public static bool tpRBSyncValue;

        public static bool remoteScrapRBFlashEnabledValue;
        public static float remoteScrapRBFlashRangeValue;
        public static bool remoteScrapRBFlashSyncValue;

        public static bool isHostHaveERBValue;

        private static bool isLocalConfig = true;


        public static void Bind()
        {
            eRBNHEnabled = Plugin.config.Bind<bool>("Networking", "ERBNHEnabled", true, "Enable mod Networking?\nRequired to enable features of [All clients] type.\nWill require having this mod to join lobby.");
            
            mapRangeRBEnabled = Plugin.config.Bind<bool>("Map", "MapRangeRBEnabled", true, "Will Radar Booster provide a bigger vision range on Map Monitor?\n[One Client]");
            mapRangeRBMultiplier = Plugin.config.Bind<float>("Map", "MapRangeRBMultiplier", 2f, "How much will be increased vision range on Map Monitor.\nSuggested values between x1.5-x2. Bigger values can prevent some thing model objects from showing on map.");
            mapRangeRBSync = Plugin.config.Bind<bool>("Map", "MapRangeRBSync", false, "Will synchronize Map settings with all connected clients to have same values?\nIf Host then will force sync settings with all Clients.\nIf Client then will get settings from Host.");

            tpRBEnabled = Plugin.config.Bind<bool>("Teleportation", "TpRBEnabled", true, "Will selected Radar Booster be teleported back to ship by Teleporter?\n[All Clients]");
            itpRBEnabled = Plugin.config.Bind<bool>("Teleportation", "ITpRBEnabled", true, "Will Radar Booster be teleported to random location in facility by Inverse Teleporter?\n[All Clients]");
            itpToRBEnabled = Plugin.config.Bind<bool>("Teleportation", "ITpToRBEnabled", true, "When Radar Booster selected on Ship Monitor and Inverse Teleporter activated, will Player be teleported to selected Radar Booster instead of random location?\n[One Client]");
            tpRBSync = Plugin.config.Bind<bool>("Teleportation", "TpRBSync", true, "Will synchronize Teleportation settings with all connected clients to have same values?\nIf Host then will force sync settings with all Clients.\nIf Client then will get settings from Host.");

            remoteScrapRBFlashEnabled = Plugin.config.Bind<bool>("Remote", "RemoteScrapRBFlashEnabled", true, "Can Remote scrap trigger Radar Booster Flash?\n[One Client, but will make it work for everyone in lobby even without being host]");
            remoteScrapRBFlashRange = Plugin.config.Bind<float>("Remote", "RemoteScrapRBFlashRange", 16f, "Radius around player with Remote in which all Radar Boosters Flash will be triggered.\nSuggested values between 1-20.");
            remoteScrapRBFlashSync = Plugin.config.Bind<bool>("Remote", "RemoteScrapRBFlashSync", true, "Will synchronize Remote settings with all connected clients to have same values?\nIf Host then will force sync settings with all Clients.\nIf Client then will get settings from Host.");

            LoadFromConfig();
        }

        private static void LoadFromConfig()
        {
            eRBNHEnabledValue = eRBNHEnabled?.Value ?? true;

            mapRangeRBEnabledValue = mapRangeRBEnabled?.Value ?? true;
            mapRangeRBMultiplierValue = mapRangeRBMultiplier?.Value ?? 2f;
            mapRangeRBSyncValue = mapRangeRBSync?.Value ?? false;

            tpRBEnabledValue = tpRBEnabled?.Value ?? true;
            itpRBEnabledValue = itpRBEnabled?.Value ?? true;
            itpToRBEnabledValue = itpToRBEnabled?.Value ?? true;
            tpRBSyncValue = tpRBSync?.Value ?? true;

            remoteScrapRBFlashEnabledValue = remoteScrapRBFlashEnabled?.Value ?? true;
            remoteScrapRBFlashRangeValue = remoteScrapRBFlashRange?.Value ?? 16f;
            remoteScrapRBFlashSyncValue = remoteScrapRBFlashSync?.Value ?? true;

            isLocalConfig = true;
        }

        public static FastBufferWriter WriteBuff()
        {
            if (!isLocalConfig)
            {
                LoadFromConfig();
            }
            FastBufferWriter FBW = new FastBufferWriter((sizeof(bool) * 9) + (sizeof(float) * 2), Unity.Collections.Allocator.Temp);
            FBW.WriteValueSafe<bool>(eRBNHEnabledValue, default(FastBufferWriter.ForPrimitives));
            FBW.WriteValueSafe<bool>(tpRBEnabledValue, default(FastBufferWriter.ForPrimitives));
            FBW.WriteValueSafe<bool>(itpRBEnabledValue, default(FastBufferWriter.ForPrimitives));
            FBW.WriteValueSafe<bool>(itpToRBEnabledValue, default(FastBufferWriter.ForPrimitives));
            FBW.WriteValueSafe<bool>(tpRBSyncValue, default(FastBufferWriter.ForPrimitives));
            FBW.WriteValueSafe<bool>(mapRangeRBEnabledValue, default(FastBufferWriter.ForPrimitives));
            FBW.WriteValueSafe<bool>(mapRangeRBSyncValue, default(FastBufferWriter.ForPrimitives));
            FBW.WriteValueSafe<bool>(remoteScrapRBFlashEnabledValue, default(FastBufferWriter.ForPrimitives));
            FBW.WriteValueSafe<bool>(remoteScrapRBFlashSyncValue, default(FastBufferWriter.ForPrimitives));
            FBW.WriteValueSafe<float>(mapRangeRBMultiplierValue, default(FastBufferWriter.ForPrimitives));
            FBW.WriteValueSafe<float>(remoteScrapRBFlashRangeValue, default(FastBufferWriter.ForPrimitives));
            return FBW;
        }

        public static void ReadBuff(FastBufferReader reader)
        {
            string importedValues = "";
            reader.ReadValueSafe<bool>(out bool eRBNHEnabledBuff);
            eRBNHEnabledValue = eRBNHEnabledBuff;
            importedValues += $"\neRBNHEnabled = {eRBNHEnabledValue} / {eRBNHEnabled.Value}";
            reader.ReadValueSafe<bool>(out bool tpRBEnabledBuff);
            reader.ReadValueSafe<bool>(out bool itpRBEnabledBuff);
            reader.ReadValueSafe<bool>(out bool itpToRBEnabledBuff);
            reader.ReadValueSafe<bool>(out bool tpRBSyncBuff);
            tpRBSyncValue = tpRBSyncBuff || tpRBSyncValue;
            importedValues += $"tpRBSync = {tpRBSyncValue}";
            if (tpRBSyncValue)
            {
                tpRBEnabledValue = tpRBEnabledBuff;
                itpRBEnabledValue = itpRBEnabledBuff;
                itpToRBEnabledValue = itpToRBEnabledBuff;
            }
            importedValues += $"\ntpRBEnabled = {tpRBEnabledValue} / {tpRBEnabled.Value}";
            importedValues += $"\nitpRBEnabled = {itpRBEnabledValue} / {itpRBEnabled.Value}";
            importedValues += $"\nitpToRBEnabled = {itpToRBEnabledValue} / {itpToRBEnabled.Value}";

            reader.ReadValueSafe<bool>(out bool mapRangeRBEnabledBuff);
            reader.ReadValueSafe<bool>(out bool mapRangeRBSyncBuff);
            mapRangeRBSyncValue = mapRangeRBSyncBuff || mapRangeRBSyncValue;
            importedValues += $"\nmapRangeRBSync = {mapRangeRBSyncValue}";
            reader.ReadValueSafe<bool>(out bool remoteScrapRBFlashEnabledBuff);
            reader.ReadValueSafe<bool>(out bool remoteScrapRBFlashSyncBuff);
            remoteScrapRBFlashSyncValue = remoteScrapRBFlashSyncBuff || remoteScrapRBFlashSyncValue;

            reader.ReadValueSafe<float>(out float mapRangeRBMultiplierBuff);
            if (mapRangeRBSyncValue)
            {
                mapRangeRBEnabledValue = mapRangeRBEnabledBuff;
                mapRangeRBMultiplierValue = mapRangeRBMultiplierBuff;
            }
            importedValues += $"\nmapRangeRBEnabled = {mapRangeRBEnabledValue} / {mapRangeRBEnabled.Value}";
            importedValues += $"\nmapRangeRBMultiplier = {mapRangeRBMultiplierValue} / {mapRangeRBMultiplier.Value}";

            reader.ReadValueSafe<float>(out float remoteScrapRBFlashRangeBuff);
            if (remoteScrapRBFlashSyncValue)
            {
                remoteScrapRBFlashEnabledValue = remoteScrapRBFlashEnabledBuff;
                remoteScrapRBFlashRangeValue = remoteScrapRBFlashRangeBuff;
            }
            importedValues += $"\nremoteScrapRBFlashSync = {remoteScrapRBFlashSyncValue}";
            importedValues += $"\nremoteScrapRBFlashEnabled = {remoteScrapRBFlashEnabledValue} / {remoteScrapRBFlashEnabled.Value}";
            importedValues += $"\nremoteScrapRBFlashRange = {remoteScrapRBFlashRangeValue} / {remoteScrapRBFlashRange.Value}";

            isLocalConfig = false;

            Plugin.MLogS.LogInfo(importedValues);
        }
    }
}
