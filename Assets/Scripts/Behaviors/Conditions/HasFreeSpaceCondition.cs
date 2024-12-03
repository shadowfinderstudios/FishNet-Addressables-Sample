using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
    name: "HasFreeSpace",
    story: "[Stash] has free space",
    category: "Conditions",
    id: "d1a957a9375db51ca82d6a0f96de6e88")]
public partial class HasFreeSpaceCondition : Condition
{
    [SerializeReference] public BlackboardVariable<StashArea> Stash;

    public override bool IsTrue()
    {
        if (Stash.Value == null) return false;
        return Stash.Value.HasFree();
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
