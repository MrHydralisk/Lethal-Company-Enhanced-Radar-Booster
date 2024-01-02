using BepInEx.Configuration;
using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnhancedRadarBooster
{
    public class Config
    {
        public static ConfigEntry<bool> mapRangeRBFlashEnabled;
        public static ConfigEntry<float> mapRangeRBFlashMultiplier;

        public static ConfigEntry<bool> tpRBEnabled;
        public static ConfigEntry<bool> itpRBEnabled;
        public static ConfigEntry<bool> itpToRBEnabled;

        public static ConfigEntry<bool> remoteScrapRBFlashEnabled;
        public static ConfigEntry<int> remoteScrapRBFlashRange;

        public static void Load()
        {
            mapRangeRBFlashEnabled = Plugin.config.Bind<bool>("Map", "MapRangeRBFlashEnabled", true, "Will Radar Booster provide a bigger vision range on Map Monitor?\n[One Client]");
            mapRangeRBFlashMultiplier = Plugin.config.Bind<float>("Map", "MapRangeRBFlashMultiplier", 2f, "How much will be increased vision range on Map Monitor.\nSuggested values between x1.5-x2. Bigger values can prevent some thing model objects from showing on map.");

            tpRBEnabled = Plugin.config.Bind<bool>("Teleportation", "TpRBEnabled", true, "Will selected Radar Booster be teleported back to ship by Teleporter?\n[All Clients]");
            itpRBEnabled = Plugin.config.Bind<bool>("Teleportation", "ITpRBEnabled", true, "Will Radar Booster be teleported to random location in facility by Inverse Teleporter?\n[All Clients]");
            itpToRBEnabled = Plugin.config.Bind<bool>("Teleportation", "ITpToRBEnabled", true, "When Radar Booster selected on Ship Monitor and Inverse Teleporter activated, will Player be teleported to selected Radar Booster instead of random location?\n[All Clients]");

            remoteScrapRBFlashEnabled = Plugin.config.Bind<bool>("Remote", "RemoteScrapRBFlashEnabled", true, "Can Remote scrap trigger Radar Booster Flash?\n[One Client]");
            remoteScrapRBFlashRange = Plugin.config.Bind<int>("Remote", "RemoteScrapRBFlashRange", 16, "Radius around player with Remote in which all Radar Boosters Flash will be triggered.\nSuggested values between 1-20.");
        }
    }
}
