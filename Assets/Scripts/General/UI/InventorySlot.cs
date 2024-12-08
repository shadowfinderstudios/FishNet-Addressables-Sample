using UnityEngine;
using UnityEngine.EventSystems;

namespace Shadowfinder.UI
{
    public class InventorySlot : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            var draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggableItem == null) return;
            if (transform.childCount != 0)
            {
                var current = transform.GetChild(0).gameObject;
                var currentDraggable = current.GetComponent<DraggableItem>();
                if (currentDraggable != null)
                    currentDraggable.transform.SetParent(draggableItem.parentAfterDrag);
            }
            draggableItem.parentAfterDrag = transform;
        }
    }
}
