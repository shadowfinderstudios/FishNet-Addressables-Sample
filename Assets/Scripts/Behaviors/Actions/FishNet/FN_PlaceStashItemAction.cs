using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using FishNet.Component.Animating;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "FishNetPlaceStashItem",
    story: "[Agent] places an item in the [Stash]",
    category: "Action",
    id: "e173811b8436aa4c045ca691d46a4a76")]
public partial class FN_PlaceStashItemAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<StashArea> Stash;

    NetworkAnimator _netAnimator;

    protected override Status OnStart()
    {
        _netAnimator = Agent.Value.GetComponent<NetworkAnimator>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Stash.Value == null) return Status.Failure;
        if (Stash.Value.HasFree())
        {
            _netAnimator.SetTrigger("Gather");
            if (Stash.Value.PlaceStash())
                return Status.Success;
        }
        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

