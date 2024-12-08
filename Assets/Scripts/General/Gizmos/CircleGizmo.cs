using UnityEngine;

namespace Shadowfinder.Gizmo
{
    public class CircleGizmo : MonoBehaviour
    {
        [SerializeField] Color _color = Color.white;
        [SerializeField] float _radius = 1f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _color;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}