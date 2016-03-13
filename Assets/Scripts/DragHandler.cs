using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Vector3 startPosition;
    Slot startParent;

    public void OnBeginDrag(PointerEventData eventData)
    {
        startParent = transform.parent.GetComponent<Slot>();
        startPosition = transform.position;

        DragHelper.itemBeingDragged = startParent.TakeItem();

        GetComponent<CanvasGroup>().blocksRaycasts = false;
        DragHelper.itemBeingDragged.transform.SetParent(DragHelper.topRenderTransform);

        ExecuteEvents.ExecuteHierarchy<IHasChanged>(startParent.gameObject, null, (x, y) => x.HasChanged());
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        if (transform.parent == DragHelper.topRenderTransform)
        {
            startParent.AddItem(DragHelper.itemBeingDragged);
            // TODO: item dropped to nowhere

            ExecuteEvents.ExecuteHierarchy<IHasChanged>(startParent.gameObject, null, (x, y) => x.HasChanged());
        }
        DragHelper.itemBeingDragged = null;
    }
}
