using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "StopAgent", story: "[Agent] sets isStopped to [IsStopped]", category: "Action", id: "6b86fad4559224a3bfaaeab4330fd0ff")]
public partial class AgentSetIsStoppedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsStopped;

    NavMeshAgent _navMeshAgent;

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            LogFailure("No agent assigned.");
            return Status.Failure;
        }

        _navMeshAgent = Agent.Value.GetComponent<NavMeshAgent>();
        _navMeshAgent.isStopped = IsStopped;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

