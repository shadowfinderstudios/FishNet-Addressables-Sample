using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Managing;
using FishNet.Connection;
using UnityEngine.UIElements;

public class AddrPrefabSpawner : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;

    void Start()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            NetworkManagerExtensions.LogWarning($"Spawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }
    }

    public void Spawn(NetworkConnection conn, NetworkObject obj, Vector3 spawnPosition)
    {
        var go = Instantiate(obj, spawnPosition, Quaternion.identity);
        InstanceFinder.ServerManager.Spawn(go, conn);
    }
}
