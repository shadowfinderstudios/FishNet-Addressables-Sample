using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "GetStoplightColor",
    story: "[StoplightColor] from [Intersection]",
    category: "Action",
    id: "0d9703d447ffebca71598dda644dd099")]
public partial class GetStoplightColorAction : Action
{
    [SerializeReference] public BlackboardVariable<StoplightColor> StoplightColor;
    [SerializeReference] public BlackboardVariable<RoadIntersection> Intersection;

    protected override Status OnStart()
    {
        StoplightColor.Value = Intersection.Value.State;
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

