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

        DragHelper.itemBeingDragged = startParent.item;
        startParent.item = null;

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
            startParent.item = DragHelper.itemBeingDragged;
            // TODO: item dropped to nowhere
            transform.position = startPosition;
            gameObject.transform.SetParent(startParent.transform);

            ExecuteEvents.ExecuteHierarchy<IHasChanged>(startParent.gameObject, null, (x, y) => x.HasChanged());
        }
        DragHelper.itemBeingDragged = null;
    }
}
