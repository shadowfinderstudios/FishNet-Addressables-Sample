using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "GetDirection",
    story: "[Direction] from [A] a to [B] b transforms",
    category: "Action",
    id: "b04b7b608ca902d3c99e1b935e68d343")]
public partial class GetDirectionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> Direction;
    [SerializeReference] public BlackboardVariable<Transform> A;
    [SerializeReference] public BlackboardVariable<Transform> B;

    protected override Status OnStart()
    {
        Direction.Value = (B.Value.position - A.Value.position).normalized;
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

