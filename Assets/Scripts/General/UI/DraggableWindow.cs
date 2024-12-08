using UnityEngine;
using UnityEngine.EventSystems;

namespace Shadowfinder.UI
{
    public class DraggableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform dragRect;
        [SerializeField] private Canvas canvas;

        void Awake()
        {
            dragRect = transform.parent.GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragRect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            dragRect.SetAsLastSibling();
        }
    }
}
