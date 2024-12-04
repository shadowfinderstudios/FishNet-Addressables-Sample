using FishNet.Object;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BehaviorGraphAgent), typeof(NavMeshAgent))]
public class FN_AgentBehaviourSetup : NetworkBehaviour
{
    public override void OnStartServer()
    {
        var agent = GetComponent<BehaviorGraphAgent>();
        agent.enabled = true;
    }

    public override void OnStartClient()
    {
        if (base.IsClientOnlyStarted)
        {
            var agent = GetComponent<BehaviorGraphAgent>();
            agent.enabled = false;

            var navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.enabled = false;
        }
    }
}
