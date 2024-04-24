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
            radarBooster.startFallingPosition = position;
            radarBooster.transform.position = position;
            radarBooster.targetFloorPosition = position;
            radarBooster.FallToGround(!isEnable);
            radarBooster.isInFactory = isEnable;
            radarBooster.isInShipRoom = !isEnable;
            radarBooster.UseItemOnClient(isEnable);
            yield return null;
        }
    }
}
