using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IDropHandler
{
    public Image item;
    
    void Start()
    {
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (item)
                    break;
                item = transform.GetChild(0).gameObject.GetComponent<Image>();
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!item)
        {
            SetItem(DragHelper.itemBeingDragged);
            ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged());
        }
    }

    public void SetItem(Image newItem)
    {
        if (item && !newItem)
        {
            item = null;
            Destroy(item);
        }
        if (!item && newItem)
        {
            newItem.transform.SetParent(transform, false);
            item = newItem;
        }
    }
}

namespace UnityEngine.EventSystems
{
    public interface IHasChanged : IEventSystemHandler
    {
        void HasChanged();
    }
}
