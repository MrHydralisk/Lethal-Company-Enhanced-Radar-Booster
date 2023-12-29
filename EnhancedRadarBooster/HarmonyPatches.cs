using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            val.Patch((MethodBase)AccessTools.Method(typeof(ShipTeleporter), "beamOutPlayer", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "ST_beamOutPlayer_Postfix", (Type[])null)); 
            val.Patch(AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(ShipTeleporter), "beamOutPlayer")), transpiler: new HarmonyMethod(patchType, "ST_beamOutPlayer_Transpiler", (Type[])null));
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
            RadarBoosterItem component;
            if (MCR.targetTransformIndex < MCR.radarTargets.Count && MCR.radarTargets[MCR.targetTransformIndex].isNonPlayer && (component = MCR.radarTargets[MCR.targetTransformIndex].transform.gameObject.GetComponent<RadarBoosterItem>()) != null)
            {
                component.transform.position = __instance.teleporterPosition.position + new Vector3(0, 1f, 0);
                component.EnableRadarBooster(false);
                component.FallToGround(true);
            }
#if DEBUG
            Plugin.MLogS.LogInfo($"ST_beamUpPlayer_Postfix");
#endif
        }

        public static void ST_beamOutPlayer_Postfix(ShipTeleporter __instance)
        {
            Collider[] colliders = Physics.OverlapSphere(__instance.teleporterPosition.position, 2f);
            foreach (Collider collider in colliders)
            {
                RadarBoosterItem component = collider.gameObject.GetComponent<RadarBoosterItem>();
                if (component != null)
                {
                    Vector3 position3 = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
                    position3 = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(position3);
                    component.transform.position = position3 + new Vector3(0, 1f, 0);
                    component.EnableRadarBooster(true);
                    component.FallToGround(false);
                }
            }
#if DEBUG
            Plugin.MLogS.LogInfo($"ST_beamOutPlayer_Postfix");
#endif
        }

        public static Vector3 VectorToRadarBooster(Vector3 current)
        {
            ManualCameraRenderer MCR = StartOfRound.Instance.mapScreen;
            RadarBoosterItem component;
            if (MCR.targetTransformIndex < MCR.radarTargets.Count && MCR.radarTargets[MCR.targetTransformIndex].isNonPlayer && (component = MCR.radarTargets[MCR.targetTransformIndex].transform.gameObject.GetComponent<RadarBoosterItem>()) != null)
            {
#if DEBUG
                Plugin.MLogS.LogInfo($"From {component.transform.position} To {current}");
#endif
                current = component.transform.position;
                current = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(current, 0.5f);
            }
            return current;
        }

        public static IEnumerable<CodeInstruction> ST_beamOutPlayer_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            bool foundMassUsageMethod = false;
            int startIndex = -1;
            int endIndex = -1;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
#if DEBUG
                Plugin.MLogS.LogInfo($" - {i} - {codes[i]} | {codes[i].opcode} | {codes[i].operand} | {codes[i].IsStloc()} | {(codes[i].IsStloc() && codes[i].ToString().Contains("UnityEngine.Vector3"))}");
#endif
                if (codes[i].Calls(AccessTools.Method(typeof(RoundManager), "get_Instance")) && (i + 3 < codes.Count) && codes[i+3].Calls(AccessTools.Method(typeof(RoundManager), "get_Instance")))
                {
#if DEBUG
                    Plugin.MLogS.LogInfo($"START " + (i));
#endif
                    startIndex = i;
                    for (int j = startIndex + 1; j < codes.Count; j++)
                    {
#if DEBUG
                        Plugin.MLogS.LogInfo($" - {i} {j} - {codes[j]} | {codes[j].opcode} | {codes[j].operand} | {codes[j].IsStloc()} | {(codes[j].IsStloc() && codes[j].ToString().Contains("UnityEngine.Vector3"))}");
#endif
                        if (codes[j].IsStloc() && codes[j].ToString().Contains("UnityEngine.Vector3"))
                        {
                            foundMassUsageMethod = true;
#if DEBUG
                            Plugin.MLogS.LogInfo($"END " + j);
#endif
                            endIndex = j;
                            break;
                        }
                        if (codes[j].opcode == OpCodes.Ret)
                            break;
                    }
                    if (foundMassUsageMethod)
                    {
                        break;
                    }
                }
            }
            if (startIndex > -1 && endIndex > -1)
            {
#if DEBUG
                Plugin.MLogS.LogInfo(string.Join("\n", codes.GetRange(startIndex, endIndex - startIndex + 1).Select(x => x.ToString())));
#endif
                List<CodeInstruction> instructionsToInsert = new List<CodeInstruction>();

                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 8));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "VectorToRadarBooster")));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_S, 8));
                codes.InsertRange(endIndex + 1, instructionsToInsert);
#if DEBUG
                Plugin.MLogS.LogInfo(string.Join("\n", codes.GetRange(startIndex, endIndex - startIndex + instructionsToInsert.Count() + 1).Select(x => x.ToString())));
#endif
            }
            return codes.AsEnumerable();
#if DEBUG
            Plugin.MLogS.LogInfo($"ST_beamOutPlayer_Transpiler");
#endif
        }
    }
}
