using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace EnhancedRadarBooster
{
    public class EnhancedRadarBoosterNetworkHandler : NetworkBehaviour
    {
        public static EnhancedRadarBoosterNetworkHandler instance;

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (instance != null && instance.gameObject != null)
                {
                    NetworkObject networkObject = instance.gameObject.GetComponent<NetworkObject>();

                    if (networkObject != null)
                    {
                        networkObject.Despawn();
                        Plugin.MLogS.LogInfo("EnhancedRadarBoosterNetworkHandler Despawned");
                    }
                }
            }

            instance = this;
            base.OnNetworkSpawn();
            Plugin.MLogS.LogInfo("EnhancedRadarBoosterNetworkHandler Spawned");
        }

        public void TeleportRadarBoosterRpc(NetworkObjectReference item, Vector3 position, bool isEnable = false)
        {
            if (IsHost || IsServer)
            {
                EnhancedRadarBoosterNetworkHandler.instance.TeleportRadarBoosterClientRpc(item, position, isEnable);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportRadarBoosterServerRpc(NetworkObjectReference item, Vector3 position, bool isEnable = false)
        {
            TeleportRadarBoosterClientRpc(item, position, isEnable);
        }

        [ClientRpc]
        public void TeleportRadarBoosterClientRpc(NetworkObjectReference item, Vector3 position, bool isEnable = false)
        {
            StartCoroutine(TeleportRadarBooster(item, position, isEnable));
        }

        public IEnumerator TeleportRadarBooster(NetworkObjectReference item, Vector3 position, bool isEnable = false)
        {
            NetworkObject netObject = null;
            item.TryGet(out netObject);
            RadarBoosterItem radarBooster = netObject.GetComponent<RadarBoosterItem>();
#if DEBUG
            Plugin.MLogS.LogInfo($"TeleportRadarBooster A {radarBooster.startFallingPosition.ToString()} | {radarBooster.transform.position.ToString()} | {radarBooster.transform.localPosition.ToString()} | {radarBooster.targetFloorPosition.ToString()} || {radarBooster.transform?.parent?.ToString() ?? "---"} | {radarBooster.parentObject?.ToString() ?? "---"}");
#endif
            Vector3 hitPoint = position;
            radarBooster.transform.position = position;
            if (isEnable)
            {
                radarBooster.isInElevator = false;
                radarBooster.isInShipRoom = false;
                radarBooster.isInFactory = true;
                radarBooster.transform.SetParent(null, worldPositionStays: true);
            }
            else
            {
                if (StartOfRound.Instance.shipInnerRoomBounds.bounds.Contains(hitPoint))
                {
                    radarBooster.isInElevator = true;
                    radarBooster.isInShipRoom = true;
                    radarBooster.transform.SetParent(StartOfRound.Instance.elevatorTransform, worldPositionStays: true);
                }
                else if (StartOfRound.Instance.shipBounds.bounds.Contains(hitPoint))
                {
                    radarBooster.isInElevator = true;
                    radarBooster.transform.SetParent(StartOfRound.Instance.elevatorTransform, worldPositionStays: true);
                }
                else
                {
                    radarBooster.transform.SetParent(null, worldPositionStays: true);
                }
                radarBooster.isInFactory = false;
                hitPoint += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            }
#if DEBUG
            Plugin.MLogS.LogInfo($"TeleportRadarBooster B {hitPoint} = {radarBooster.startFallingPosition.ToString()} | {radarBooster.transform.position.ToString()} | {radarBooster.transform.localPosition.ToString()} | {radarBooster.targetFloorPosition.ToString()} || {radarBooster.transform?.parent?.ToString() ?? "---"} | {radarBooster.parentObject?.ToString() ?? "---"}");
#endif		
            if (radarBooster.transform.parent != null)
            {
                hitPoint = radarBooster.transform.parent.InverseTransformPoint(position);
            }
            radarBooster.fallTime = 0f;
            radarBooster.hasHitGround = false;
            GameNetworkManager.Instance.localPlayerController.SetItemInElevator(radarBooster.isInElevator, radarBooster.isInShipRoom, radarBooster);
            radarBooster.startFallingPosition = hitPoint + Vector3.up * 0.07f;
            radarBooster.targetFloorPosition = hitPoint;
            radarBooster.EnablePhysics(enable: true);
            if (radarBooster.isBeingUsed != isEnable)
            {
                radarBooster.UseItemOnClient(isEnable);
            }
#if DEBUG
            Plugin.MLogS.LogInfo($"TeleportRadarBooster C {radarBooster.startFallingPosition.ToString()} | {radarBooster.transform.position.ToString()} | {radarBooster.transform.localPosition.ToString()} | {radarBooster.targetFloorPosition.ToString()} || {radarBooster.transform?.parent?.ToString() ?? "---"} | {radarBooster.parentObject?.ToString() ?? "---"}");
#endif
            yield return null;
        }
    }
}
