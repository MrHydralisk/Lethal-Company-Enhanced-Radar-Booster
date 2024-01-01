using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Unity.Netcode;
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
            val.Patch(AccessTools.Method(typeof(ManualCameraRenderer), "MapCameraFocusOnPosition", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "MCR_MapCameraFocusOnPosition_Postfix", (Type[])null));
            val.Patch(AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(ShipTeleporter), "beamUpPlayer")), transpiler: new HarmonyMethod(patchType, "ST_beamUpPlayer_Transpiler", (Type[])null));
            val.Patch(AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(ShipTeleporter), "beamOutPlayer")), transpiler: new HarmonyMethod(patchType, "ST_beamOutPlayer_Transpiler", (Type[])null));
            val.Patch(AccessTools.Method(typeof(GameNetworkManager), "Start", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "GNM_Start_Postfix", (Type[])null));
            val.Patch(AccessTools.Method(typeof(StartOfRound), "Start", (Type[])null, (Type[])null), postfix: new HarmonyMethod(patchType, "SOR_Start_Postfix", (Type[])null));
#if DEBUG
            Plugin.MLogS.LogInfo("HarmonyPatches is loaded!");
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
        }

        public static void beamUpRadarBooster(Transform teleporterPosition)
        {
            ManualCameraRenderer MCR = StartOfRound.Instance.mapScreen;
            RadarBoosterItem component;
            if (MCR.targetTransformIndex < MCR.radarTargets.Count && MCR.radarTargets[MCR.targetTransformIndex].isNonPlayer && (component = MCR.radarTargets[MCR.targetTransformIndex].transform.gameObject.GetComponent<RadarBoosterItem>()) != null)
            {
                Vector3 position3 = teleporterPosition.position;
                EnhancedRadarBoosterNetworkHandler.instance.TeleportRadarBoosterRpc(component.GetComponent<NetworkObject>(), position3, false);
            }
        }
        public static IEnumerable<CodeInstruction> ST_beamUpPlayer_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            int startIndex = -1;
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
#if DEBUG
                Plugin.MLogS.LogInfo($" - {i} - {codes[i]} | {codes[i].opcode} | {codes[i].operand} | {(codes[i].Is(OpCodes.Ldstr, "Targeted player is null"))}");
#endif
                if (codes[i].Is(OpCodes.Ldstr, "Targeted player is null"))
                {
#if DEBUG
                    Plugin.MLogS.LogInfo($"START {i}");
#endif
                    startIndex = i;
                }
            }
            if (startIndex > -1)
            {
#if DEBUG
                Plugin.MLogS.LogInfo(string.Join("\n", codes.GetRange(startIndex, 5).Select(x => x.ToString())));
#endif
                List<CodeInstruction> instructionsToInsert = new List<CodeInstruction>();
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_1));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ShipTeleporter), "teleporterPosition")));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "beamUpRadarBooster")));
                codes.InsertRange(startIndex + 2, instructionsToInsert);
#if DEBUG
                Plugin.MLogS.LogInfo(string.Join("\n", codes.GetRange(startIndex, 5 + instructionsToInsert.Count() + 1).Select(x => x.ToString())));
#endif
            }
            return codes.AsEnumerable();
#if DEBUG
            Plugin.MLogS.LogInfo(ST_beamUpPlayer_Transpiler");
#endif
        }

        public static void beamOutRadarBooster(Transform teleporterPosition)
        {
            Collider[] colliders = Physics.OverlapSphere(teleporterPosition.position, 2f);
            foreach (Collider collider in colliders)
            {
                RadarBoosterItem component = collider.gameObject.GetComponent<RadarBoosterItem>();
                if (component != null)
                {
                    Vector3 position3 = RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
                    position3 = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(position3);
                    EnhancedRadarBoosterNetworkHandler.instance.TeleportRadarBoosterRpc(component.GetComponent<NetworkObject>(), position3, true);
                }
            }
        }

        public static Vector3 VectorToRadarBooster(Vector3 current)
        {
            ManualCameraRenderer MCR = StartOfRound.Instance.mapScreen;
            RadarBoosterItem component;
            if (MCR.targetTransformIndex < MCR.radarTargets.Count && MCR.radarTargets[MCR.targetTransformIndex].isNonPlayer && (component = MCR.radarTargets[MCR.targetTransformIndex].transform.gameObject.GetComponent<RadarBoosterItem>()) != null)
            {
                current = component.transform.position;
                current = RoundManager.Instance.GetNavMeshPosition(current, sampleRadius: 0.5f);
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
                if (codes[i].Calls(AccessTools.Method(typeof(RoundManager), "get_Instance")) && (i + 3 < codes.Count) && codes[i+3].Calls(AccessTools.Method(typeof(RoundManager), "get_Instance")))
                {
                    startIndex = i;
                    for (int j = startIndex + 1; j < codes.Count; j++)
                    {
                        if (codes[j].IsStloc() && codes[j].ToString().Contains("UnityEngine.Vector3"))
                        {
                            foundMassUsageMethod = true;
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
                List<CodeInstruction> instructionsToInsert = new List<CodeInstruction>();
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 8));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "VectorToRadarBooster")));
                instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_S, 8));
                codes.InsertRange(endIndex + 1, instructionsToInsert);

                List<CodeInstruction> instructionsToInsert2 = new List<CodeInstruction>();
                instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldloc_1));
                instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ShipTeleporter), "teleporterPosition")));
                instructionsToInsert2.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "beamOutRadarBooster")));
                codes.InsertRange(codes.Count() - 2, instructionsToInsert2);
            }
            return codes.AsEnumerable();
        }

        public static void GNM_Start_Postfix(GameNetworkManager __instance)
        {
            __instance.GetComponent<NetworkManager>().AddNetworkPrefab(Plugin.instance.enhancedRadarBoosterNetworkManager); 
        }

        public static void SOR_Start_Postfix(StartOfRound __instance)
        {
            if (__instance.IsHost)
            {
                GameObject ERBNMObject = GameObject.Instantiate(Plugin.instance.enhancedRadarBoosterNetworkManager);
                ERBNMObject.GetComponent<NetworkObject>().Spawn(true);
            }
        }
    }
}
