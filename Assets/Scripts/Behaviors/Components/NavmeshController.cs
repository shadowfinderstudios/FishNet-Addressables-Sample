using FishNet.Object;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class NavmeshController : NetworkBehaviour
{
    NavMeshAgent _navMeshAgent;
    BehaviorGraphAgent _agent;

    [ServerRpc]
    public void ServerSetDest(Vector3 target)
    {
        _navMeshAgent.SetDestination(target);
        ObserversSetDest(target);
    }

    [ObserversRpc]
    public void ObserversSetDest(Vector3 target)
    {
        _navMeshAgent.SetDestination(target);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsServerInitialized)
        {
            _agent = GetComponent<BehaviorGraphAgent>();
            _agent.enabled = false;
        }

        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.updateRotation = false;
            _navMeshAgent.updateUpAxis = false;
            _navMeshAgent.autoRepath = true;
        }
    }
}
