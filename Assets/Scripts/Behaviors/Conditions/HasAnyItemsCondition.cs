using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
    name: "HasAnyItems",
    story: "[Stash] has any items",
    category: "Conditions",
    id: "f13806c6b89c35509b5101ba3ce0ed83")]
public partial class HasAnyItemsCondition : Condition
{
    [SerializeReference] public BlackboardVariable<StashArea> Stash;

    public override bool IsTrue()
    {
        if (Stash.Value == null) return false;
        return Stash.Value.GetStashedCount() > 0;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
