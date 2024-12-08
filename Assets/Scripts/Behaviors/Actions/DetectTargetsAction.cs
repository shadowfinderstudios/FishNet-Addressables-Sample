using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Shadowfinder.Sensors;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DetectTargets", story: "[Agent] detects [Target]", category: "Action", id: "ecb4d0c2eaeb34dedf5a5bc2188baa46")]
public partial class DetectTargetsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<string> TargetTag;

    NavMeshAgent _agent;
    RangeSensor _sensor;

    protected override Status OnStart()
    {
        _agent = Agent.Value.GetComponent<NavMeshAgent>();
        _sensor = Agent.Value.GetComponent<RangeSensor>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var target = _sensor.GetClosestTarget(TargetTag);
        if (target == null) return Status.Running;
        Target.Value = target.gameObject;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

