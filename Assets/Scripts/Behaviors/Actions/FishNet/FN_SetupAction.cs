using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FishNetSetup",
    story: "[Agent] sets up for FishNet",
    category: "Action",
    id: "ceeb14bc993d4e80c1c1258e501213dd")]
public partial class FN_SetupAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    FN_AgentBehaviourSetup _navmeshController;

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            LogFailure("No agent assigned.");
            return Status.Failure;
        }

        _navmeshController = Agent.Value.GetComponent<FN_AgentBehaviourSetup>();
        if (_navmeshController == null)
        {
            LogFailure("Navmesh Controller is not initialized.");
            return Status.Failure;
        }

        if (!_navmeshController.IsServerInitialized)
        {
            LogFailure("Navmesh Controller is server only.");
            return Status.Failure;
        }

        var navMeshAgent = Agent.Value.GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;
            navMeshAgent.autoRepath = true;
            navMeshAgent.enabled = true;
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

