using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace CrashKonijn.Goap.GenTest
{
    public class IdleTargetSensor : LocalTargetSensorBase
    {
        Vector2 _spawnPoint = Vector2.zero;

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            if (_spawnPoint == Vector2.zero) _spawnPoint = agent.Transform.position;

            var targetPosition = _spawnPoint + Random.insideUnitCircle * 7f;
            var position = GetClosestPointOnNavMesh(targetPosition, 2f);
            return new PositionTarget(new Vector3(position.x, position.y, 0f));
        }

        public override void Created() {}

        public override void Update() { }

        Vector2 GetClosestPointOnNavMesh(Vector3 targetPosition, float maxDistance)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, maxDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                // Handle the case where no point is found on the NavMesh
                return targetPosition; // Or return a default position
            }
        }

        Vector2 GetRandomPointOnNavMesh(IActionReceiver agent, Vector3 targetPosition)
        {
            var path = new NavMeshPath();
            NavMesh.CalculatePath(agent.Transform.position, targetPosition, NavMesh.AllAreas, path);

            if (path.corners.Length == 0)
            {
                // Handle the case where no path is found
                return agent.Transform.position; // Or return a default position
            }

            var navMeshBounds = new Bounds(path.corners[0], Vector3.zero);
            foreach (Vector3 corner in path.corners)
            {
                navMeshBounds.Encapsulate(corner);
            }

            Vector3 randomPoint = new Vector3(Random.Range(navMeshBounds.min.x, navMeshBounds.max.x),
                                              Random.Range(navMeshBounds.min.y, navMeshBounds.max.y), 0);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                // Retry if the sample failed
                return GetRandomPointOnNavMesh(agent, targetPosition);
            }
        }
    }
}