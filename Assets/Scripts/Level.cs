using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Level : MonoBehaviour, IHasChanged
{
    [SerializeField]
    Transform slots;
    [SerializeField]
    Text debugText;

    public Image slotPrefab;
    public Image itemPrefab;
    public Image borderPrefab;
    
    void Start()
    {
        GridLayoutGroup grid = this.GetComponent<GridLayoutGroup>();
        int LevelSize = 3;
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
                    slot.GetComponent<Slot>().item = item;
                    item.transform.SetParent(slot.transform, false);
                }
            }
        }

        HasChanged();
    }

    public void HasChanged()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append(" - ");
        foreach (Transform slotTransform in slots)
        {
            Image item = slotTransform.GetComponent<Slot>().item;
            if (item)
            {
                builder.Append(item.name);
                builder.Append(" - ");
            }
        }
        debugText.text = builder.ToString();
    }
}
