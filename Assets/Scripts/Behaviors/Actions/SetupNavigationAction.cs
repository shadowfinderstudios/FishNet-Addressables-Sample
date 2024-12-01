using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetupNavigation", story: "[Agent] initializes its navigation.", category: "Action/Navigation", id: "261581889c2c006db086e5458eefcd94")]
public partial class SetupNavigationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    NavMeshAgent _navMeshAgent;

    protected override Status OnStart()
    {
        _navMeshAgent = Agent.Value.GetComponentInChildren<NavMeshAgent>();
        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.updateRotation = false;
            _navMeshAgent.updateUpAxis = false;
            _navMeshAgent.autoRepath = true;
        }

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

