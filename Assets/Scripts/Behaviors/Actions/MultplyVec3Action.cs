using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "MultplyVec3",
    story: "Multiply [Vector3] by [Float]",
    category: "Action",
    id: "e6485ad288c5864c9cb1d9652dca5f94")]
public partial class MultplyVec3Action : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> Vector3;
    [SerializeReference] public BlackboardVariable<float> Float;

    protected override Status OnStart()
    {
        Vector3.Value *= Float.Value;
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

