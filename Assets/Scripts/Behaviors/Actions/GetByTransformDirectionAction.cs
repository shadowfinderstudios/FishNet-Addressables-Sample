using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "GetByTransformDirection",
    story: "The [Transform] is advanced by [Direction] to give [Vector]",
    category: "Action",
    id: "382c1e9004164cf54086a38f928720a9")]
public partial class GetByTransformDirectionAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Transform;
    [SerializeReference] public BlackboardVariable<Vector3> Direction;
    [SerializeReference] public BlackboardVariable<Vector3> Vector;

    protected override Status OnStart()
    {
        Vector.Value = Transform.Value.position + Direction.Value;
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

