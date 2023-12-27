using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace EnhancedRadarBooster
{
    public class HarmonyPatches
    {
        private static readonly Type patchType;
        private const float defaultOrthographicSize = 19.7f;
        private const float defaultFarClipPlane = 7.52f;
        private const float defaultNearClipPlane = -2.47f;

        static HarmonyPatches()
        {
            patchType = typeof(HarmonyPatches);
            Harmony val = new Harmony("LethalCompany.MrHydralisk.EnhancedRadarBooster");
            val.Patch((MethodBase)AccessTools.Method(typeof(ManualCameraRenderer), "MapCameraFocusOnPosition", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "MCR_MapCameraFocusOnPosition_Postfix", (Type[])null));
            val.Patch((MethodBase)AccessTools.Method(typeof(ShipTeleporter), "beamUpPlayer", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "ST_beamUpPlayer_Postfix", (Type[])null));
#if DEBUG
            Plugin.MLogS.LogInfo($"HarmonyPatches is loaded!");
#endif
        }
		
        public static void MCR_MapCameraFocusOnPosition_Postfix(ManualCameraRenderer __instance)
        {
            if (__instance.targetedPlayer == null)
            {
                __instance.mapCamera.nearClipPlane = defaultNearClipPlane / 2f;
                __instance.mapCamera.farClipPlane = defaultFarClipPlane * 2;
                StartOfRound.Instance.radarCanvas.planeDistance = defaultNearClipPlane / 2f + 0.05f;
                __instance.mapCamera.orthographicSize = defaultOrthographicSize * 2;
            }
            else
            {
                __instance.mapCamera.farClipPlane = defaultFarClipPlane;
                __instance.mapCamera.orthographicSize = defaultOrthographicSize;
            }
#if DEBUG
            Plugin.MLogS.LogInfo($"MCR_MapCameraFocusOnPosition_Postfix");
#endif
        }

        public static void ST_beamUpPlayer_Postfix(ShipTeleporter __instance)
        {
            ManualCameraRenderer MCR = StartOfRound.Instance.mapScreen;
            if (MCR.targetTransformIndex < MCR.radarTargets.Count && MCR.radarTargets[MCR.targetTransformIndex].isNonPlayer)
            {
                RadarBoosterItem component = MCR.radarTargets[MCR.targetTransformIndex].transform.gameObject.GetComponent<RadarBoosterItem>();
                Plugin.MLogS.LogInfo($"RB " + component?.radarBoosterName ?? "null");
                if (component != null)
                {
                    component.transform.position = __instance.teleporterPosition.position + new Vector3(0, 1f, 0);
                    component.EnableRadarBooster(false);
                    component.FallToGround(true);
                }
            }
#if DEBUG
            Plugin.MLogS.LogInfo($"ST_beamUpPlayer_Postfix");
#endif
        }
    }
}
