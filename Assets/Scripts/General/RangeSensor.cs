using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;

[RequireComponent(typeof(CircleCollider2D))]
public class RangeSensor : NetworkBehaviour
{
    public float detectionRadius = 10f;
    public List<string> targetTags = new();

    readonly List<Transform> detectedObjects = new(10);
    CircleCollider2D circleCollider;

    private void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = detectionRadius;

        var colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var collider in colliders)
            if (targetTags.Contains(collider.tag))
                detectedObjects.Add(collider.transform);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Untagged")) return;

        if (targetTags.Contains(other.tag))
            detectedObjects.Add(other.transform);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Untagged")) return;

        if (targetTags.Contains(other.tag))
            detectedObjects.Remove(other.transform);
    }

    public Transform GetClosestTarget(string tag)
    {
        Transform closestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (var detectedObject in detectedObjects)
        {
            var distanceSqr = (detectedObject.position - transform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr && detectedObject.CompareTag(tag))
            {
                closestTarget = detectedObject;
                closestDistanceSqr = distanceSqr;
            }
        }
        return closestTarget;
    }
}
