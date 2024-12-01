using UnityEngine;
using FishNet.Object;
using FishNet;

public class NpcSpawner : NetworkBehaviour
{
    [SerializeField] GameObject _prefab;

    public override void OnStartServer()
    {
        base.OnStartServer();
        var go = Instantiate(_prefab, transform.position, transform.rotation);
        InstanceFinder.ServerManager.Spawn(go);
    }
}
