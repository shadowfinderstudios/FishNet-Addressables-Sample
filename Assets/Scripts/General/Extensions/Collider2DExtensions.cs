using System.Collections.Generic;
using UnityEngine;

namespace Shadowfinder.Extensions
{
    public static class Collider2DExtensions
    {
        public static void TryUpdateShapeToAttachedSprite(this PolygonCollider2D collider)
        {
            collider.UpdateShapeToSprite(collider.GetComponent<SpriteRenderer>().sprite);
        }

        public static void UpdateShapeToSprite(this PolygonCollider2D collider, Sprite sprite)
        {
            if (collider != null && sprite != null)
            {
                collider.pathCount = sprite.GetPhysicsShapeCount();
                var path = new List<Vector2>();
                for (int i = 0; i < collider.pathCount; i++)
                {
                    path.Clear();
                    sprite.GetPhysicsShape(i, path);
                    collider.SetPath(i, path.ToArray());
                }
            }
        }
    }
}
