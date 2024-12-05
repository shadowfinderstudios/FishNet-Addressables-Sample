using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChangeAgentSpeed", story: "[Agent] changes speed to [Speed]", category: "Action", id: "962650d19886e68dd4711651478f6dbb")]
public partial class ChangeAgentSpeedAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Speed;

    NavMeshAgent _navMeshAgent;

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            LogFailure("No agent assigned.");
            return Status.Failure;
        }

        _navMeshAgent = Agent.Value.GetComponent<NavMeshAgent>();
        _navMeshAgent.speed = Speed.Value;

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

