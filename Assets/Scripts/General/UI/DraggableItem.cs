using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shadowfinder.UI
{
    [RequireComponent(typeof(Image))]
    public class DraggableItem : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Image image;

        [HideInInspector]
        public Transform parentAfterDrag;

        void Awake()
        {
            image = GetComponent<Image>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            image.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.SetParent(parentAfterDrag);
            image.raycastTarget = true;
        }
    }
}
