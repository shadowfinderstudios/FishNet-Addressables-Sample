using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Lerp", story: "[Agent] lerps [Speed] from [A] a to [B] b", category: "Action", id: "3403a0f919b084ca2a59195772091b9c")]
public partial class LerpAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Speed;
    [SerializeReference] public BlackboardVariable<float> A;
    [SerializeReference] public BlackboardVariable<float> B;

    protected override Status OnStart()
    {
        Speed.Value = Mathf.Lerp(Speed.Value, A.Value, Time.deltaTime * B.Value);
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

