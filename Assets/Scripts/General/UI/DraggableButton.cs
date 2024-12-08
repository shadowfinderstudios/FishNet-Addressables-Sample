using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Shadowfinder.UI
{
    public class DraggableButton : Button, IDragHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform dragRect;
        [SerializeField] private Canvas canvas;

        protected override void Awake()
        {
            dragRect = transform.parent.GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            base.Awake();
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
