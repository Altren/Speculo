﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Level : MonoBehaviour, IHasChanged
{
    [SerializeField]
    Transform slots;
    [SerializeField]
    Text inventoryText;

    public Image slotPrefab;
    public Image itemPrefab;
    public Image borderPrefab;
    
    void Start()
    {
        GridLayoutGroup grid = this.GetComponent<GridLayoutGroup>();
        int LevelSize = 4;
        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                Image slot = (Image)Instantiate(slotPrefab);
                slot.transform.SetParent(transform, false);

                int rand = Random.Range(0, 2);
                if (rand != 0)
                {
                    Image item = (Image)Instantiate(itemPrefab);
                    item.transform.SetParent(slot.transform, false);
                }
            }
        }

        HasChanged();
    }

    #region IHasChanged implementation
    public void HasChanged()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append(" - ");
        foreach (Transform slotTransform in slots)
        {
            GameObject item = slotTransform.GetComponent<Slot>().item;
            if (item)
            {
                builder.Append(item.name);
                builder.Append(" - ");
            }
        }
        inventoryText.text = builder.ToString();
    }
    #endregion
}
