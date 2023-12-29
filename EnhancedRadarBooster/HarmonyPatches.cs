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
            var enumeratorMethod = AccessTools.Method(typeof(ShipTeleporter), "beamOutPlayer");
            var moveNext = AccessTools.EnumeratorMoveNext(enumeratorMethod);
            val.Patch(moveNext, transpiler: new HarmonyMethod(patchType, "ST_beamOutPlayer_Transpiler", (Type[])null));
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

        public static IEnumerable<CodeInstruction> ST_beamOutPlayer_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.MLogS.LogInfo($"ST_beamOutPlayer_Transpiler");
            var foundMassUsageMethod = false;
            var startIndex = -1;
            var endIndex = -1;

            var codes = new List<CodeInstruction>(instructions);

            for (var i = 0; i < codes.Count; i++)
            {
                Plugin.MLogS.LogInfo($" - {i} - {codes[i]} | {codes[i].opcode} | {codes[i].operand} | {codes[i].IsStloc()} | {(codes[i].IsStloc() && codes[i].ToString().Contains("UnityEngine.Vector3"))}");
                if (codes[i].Calls(AccessTools.Method(typeof(RoundManager), "get_Instance")) && (i + 3 < codes.Count) && codes[i+3].Calls(AccessTools.Method(typeof(RoundManager), "get_Instance")))
                {
                    Plugin.MLogS.LogInfo($"START " + (i));
                    startIndex = i;
                    for (var j = startIndex + 1; j < codes.Count; j++)
                    {
                        Plugin.MLogS.LogInfo($" - {i} {j} - {codes[j]} | {codes[j].opcode} | {codes[j].operand} | {codes[j].IsStloc()} | {(codes[j].IsStloc() && codes[j].ToString().Contains("UnityEngine.Vector3"))}");
                        if (codes[j].IsStloc() && codes[j].ToString().Contains("UnityEngine.Vector3"))
                        {
                            Plugin.MLogS.LogInfo("foundMassUsageMethod");
                            foundMassUsageMethod = true; 
                            Plugin.MLogS.LogInfo($"END " + j);
                            endIndex = j;
                            break;
                        }
                        if (codes[j].opcode == OpCodes.Ret)
                            break;
                        //if (codes[i].Calls(AccessTools.Method(typeof(RoundManager), "get_Instance")))
                        //    break;
                    }
                    if (foundMassUsageMethod)
                    {
                        break;
                    }
                }
            }
            //endIndex = startIndex + 11;
            if (startIndex > -1 && endIndex > -1)
            {
                // we cannot remove the first code of our range since some jump actually jumps to
                // it, so we replace it with a no-op instead of fixing that jump (easier).
                //codes[startIndex].opcode = OpCodes.Nop;
                //codes.RemoveRange(startIndex + 1, endIndex - startIndex - 1);

                Plugin.MLogS.LogInfo(string.Join("\n", codes.GetRange(startIndex, endIndex - startIndex + 1).Select(x => x.ToString())));
                CodeInstruction c1 = new CodeInstruction(OpCodes.Ldloc_1);
                codes[endIndex - 3] = c1;
                CodeInstruction c2 = new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ShipTeleporter), "teleporterPosition"));
                codes[endIndex - 2] = c2;
                codes[startIndex].opcode = OpCodes.Nop;
                codes.RemoveRange(startIndex + 1, endIndex - startIndex - 4);
                Plugin.MLogS.LogInfo(string.Join("\n", codes.GetRange(startIndex, 4).Select(x => x.ToString())));
            }

            Plugin.MLogS.LogInfo($"ST_beamOutPlayer_Transpiler END");
            return codes.AsEnumerable();
#if DEBUG
            Plugin.MLogS.LogInfo($"ST_beamOutPlayer_Transpiler");
#endif
        }
    }
}
