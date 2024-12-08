using UnityEngine;
using FishNet.Object;
using FishNet.Managing.Scened;
using FishNet.Connection;

namespace Shadowfinder.Scene
{
    public class SceneLoader : NetworkBehaviour
    {
        [SerializeField] string _sceneName;

        void LoadConnectionScene(string name, GameObject player)
        {
            var slud = new SceneLookupData() { Handle = 0, Name = name };
            var nob = player.GetComponent<NetworkObject>();
            var sld = new SceneLoadData()
            {
                SceneLookupDatas = new SceneLookupData[] { slud },
                MovedNetworkObjects = new NetworkObject[] { nob },
                Options = new LoadOptions()
                {
                    AutomaticallyUnload = true,
                    AllowStacking = false,
                    Addressables = true
                }
            };
            base.SceneManager.LoadConnectionScenes(nob.Owner, sld);
        }

        void LoadGlobalScene(string name, GameObject player)
        {
            var slud = new SceneLookupData() { Handle = 0, Name = name };
            var nob = player.GetComponent<NetworkObject>();
            var sld = new SceneLoadData()
            {
                SceneLookupDatas = new SceneLookupData[] { slud },
                MovedNetworkObjects = new NetworkObject[] { nob },
                Options = new LoadOptions() { Addressables = true }
            };
            base.SceneManager.LoadGlobalScenes(sld);
        }

        [ServerRpc(RequireOwnership = false)]
        void ServerLoadScene(string name, GameObject player, NetworkConnection conn = null)
        {
            LoadGlobalScene(name, player);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                ServerLoadScene(_sceneName, other.gameObject);
            }
        }
    }
}
