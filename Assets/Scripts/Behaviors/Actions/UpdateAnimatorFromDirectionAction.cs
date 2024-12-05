using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "UpdateAnimatorFromDirection",
    story: "Updates the [Animator] from the [Direction]",
    category: "Action",
    id: "d436f0ac88ec0bc374580562137ac220")]
public partial class UpdateAnimatorFromDirectionAction : Action
{
    [SerializeReference] public BlackboardVariable<Animator> Animator;
    [SerializeReference] public BlackboardVariable<Vector3> Direction;

    protected override Status OnStart()
    {
        if (Animator.Value != null)
        {
            var dir = Direction.Value.normalized;
            Animator.Value.SetInteger("Motion", 0);
            Animator.Value.SetFloat("DX", dir.x);
            Animator.Value.SetFloat("DY", dir.y);
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

