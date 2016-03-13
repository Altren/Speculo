using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IDropHandler
{
    [SerializeField]
    Text text = null;
    [SerializeField]
    Item.Type slotItemType = Item.Type.None;

    private Image item = null;
    private int _itemsCount = 0;
    public int itemsCount
    {
        get { return _itemsCount; }
        set
        {
            _itemsCount = value;
            if (text)
                text.text = _itemsCount.ToString();
            if (_itemsCount > 0 && item == null && slotItemType != Item.Type.None)
            {
                Image newItem = (Image)Instantiate(Resources.Load(slotItemType.ToString(), typeof(Image)));
                newItem.transform.SetParent(transform, false);
                newItem.rectTransform.SetAsFirstSibling();
                item = newItem;
            }
        }
    }

    public Item.Type itemType
    {
        get
        {
            return GetItemType(item);
        }
    }

    void Start()
    {
    }

    static Item.Type GetItemType(Image image)
    {
        Item.Type type = Item.Type.None;
        if (image != null)
        {
            type = (Item.Type)Enum.Parse(typeof(Item.Type), image.sprite.name);
        }
        return type;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!item || slotItemType != Item.Type.None && slotItemType == GetItemType(DragHelper.itemBeingDragged))
        {
            AddItem(DragHelper.itemBeingDragged);
            DragHelper.itemBeingDragged = null;
            ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged());
        }
    }

    public void AddItem(Image newItem)
    {
        if (item)
            Destroy(item.gameObject);
        newItem.transform.SetParent(transform, true);
        newItem.rectTransform.SetAsFirstSibling();
        item = newItem;
        itemsCount++;
    }

    public Image TakeItem()
    {
        var result = item;
        item = null;
        itemsCount--;

        return result;
    }

    public void DestroyItem()
    {
        itemsCount = 0;
        if (item)
            Destroy(item.gameObject);
        item = null;
    }
}

namespace UnityEngine.EventSystems
{
    public interface IHasChanged : IEventSystemHandler
    {
        void HasChanged();
    }
}
