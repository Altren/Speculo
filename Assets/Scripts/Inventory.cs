using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class Inventory : MonoBehaviour, IHasChanged
{
    public Transform slotsGrid;
    public Dictionary<Item.Type, Slot> slots = new Dictionary<Item.Type, Slot>();

    void Start()
    {
        for (int i = (int)Item.Type.Item_Mirror_BR_TL; i <= (int)Item.Type.Item_Warp; ++i)
        {
            int slotIdx = i - (int)Item.Type.Item_Mirror_BR_TL;
            slots[(Item.Type)i] = slotsGrid.GetChild(slotIdx).GetComponent<Slot>();
        }
    }
    
    public void HasChanged()
    {
    }
}
