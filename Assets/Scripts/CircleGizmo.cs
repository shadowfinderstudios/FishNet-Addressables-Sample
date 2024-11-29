using UnityEngine;

public class CircleGizmo : MonoBehaviour
{
    [SerializeField] Color _color = Color.white;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _color;
        float radius = transform.localScale.magnitude;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
