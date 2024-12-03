using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using FishNet.Component.Animating;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "TakeStashItem",
    story: "[Agent] takes an item from the [Stash]",
    category: "Action",
    id: "11fe7d6ae756d11dda3d61342e4d630d")]
public partial class FN_TakeStashItemAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<StashArea> Stash;

    NetworkAnimator _netAnimator;

    protected override Status OnStart()
    {
        _netAnimator = Agent.Value.GetComponentInChildren<NetworkAnimator>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Stash.Value == null) return Status.Failure;
        var item = Stash.Value.TakeStash();
        if (item)
        {
            _netAnimator.SetTrigger("Gather");
            return Status.Success;
        }
        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

