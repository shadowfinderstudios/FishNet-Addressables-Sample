using UnityEngine;
using FishNet.Object;
using FishNet;

public class TurkeySpawner : NetworkBehaviour
{
    [SerializeField] GameObject _turkeyPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();
        var go = Instantiate(_turkeyPrefab, transform.position, transform.rotation);
        InstanceFinder.ServerManager.Spawn(go);
    }
}
