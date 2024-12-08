using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Shadowfinder.Spawnables;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "VehicleChooseTurn",
    story: "[Agent] chooses a [Turn] from [Intersection] and is [Turning] turning",
    category: "Action",
    id: "a6a426d2b3441749ae44bfe6c60b53bc")]
public partial class VehicleChooseTurnAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<RoadIntersection> Turn;
    [SerializeReference] public BlackboardVariable<RoadIntersection> Intersection;
    [SerializeReference] public BlackboardVariable<bool> Turning;

    protected override Status OnStart()
    {
        GameObject nextTarget = null;

        Turning.Value = true;

        int rand = UnityEngine.Random.Range(0, 3);
        if (Intersection.Value.Left != null && rand == 0)
            nextTarget = Intersection.Value.Left;
        else if (Intersection.Value.Right != null && rand == 1)
            nextTarget = Intersection.Value.Right;
        else if (Intersection.Value.Ahead != null)
        {
            Turning.Value = false;
            nextTarget = Intersection.Value.Ahead;
        }
        if (nextTarget == null) return Status.Failure;

        Turn.Value = nextTarget.GetComponent<RoadIntersection>();

        if (Turn.Value == null) return Status.Failure;

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

