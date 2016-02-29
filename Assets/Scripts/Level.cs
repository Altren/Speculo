using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using CellItem = UnityEngine.UI.Image;
using LaserItem = UnityEngine.UI.Image;

public class Level : MonoBehaviour, IHasChanged
{
    public Image slotPrefab;
    public Image itemPrefab;
    public Image lazerPrefab;

    public Text debugText;
    
    public Image gridPanel;
    public Image leftLasers;
    public Image rightLasers;
    public Image topLasers;
    public Image bottomLasers;

    Image[] grids;

    public Material lineMaterial;
    //number of lines drawn
    private int currLines = 0;

    void Start()
    {
        grids = new Image[] {gridPanel, leftLasers, rightLasers, topLasers, bottomLasers};
        // remove editor test childrens
        foreach (var item in grids)
        {
            ClearTestChilds(item.transform);
        }

        int LevelSize = 2;
        for (int i = 0; i < LevelSize; i++)
        {
            Image slot;
            slot = (Image)Instantiate(lazerPrefab);
            slot.transform.SetParent(leftLasers.transform, false);
            slot = (Image)Instantiate(lazerPrefab);
            slot.transform.SetParent(rightLasers.transform, false);
            slot = (Image)Instantiate(lazerPrefab);
            slot.transform.SetParent(topLasers.transform, false);
            slot = (Image)Instantiate(lazerPrefab);
            slot.transform.SetParent(bottomLasers.transform, false);

            for (int j = 0; j < LevelSize; j++)
            {
                slot = (Image)Instantiate(slotPrefab);
                slot.transform.SetParent(gridPanel.transform, false);

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

    private void ClearTestChilds(Transform panel)
    {
        foreach (Transform t in panel)
        {
            Destroy(t.gameObject);
        }
    }

    public void HasChanged()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append(" - ");
        var line = createLine();
        int counter = 0;
        foreach (Slot slot in GetComponentsInChildren<Slot>())
        {
            if (counter < 3)
            {
                line.SetPosition(counter, slot.GetComponent<RectTransform>().anchoredPosition3D/40);
                counter++;
            }
            builder.Append(slot.name);
            builder.Append(slot.GetComponent<RectTransform>().anchoredPosition3D);
            builder.Append(" - ");
        }
        
        debugText.text = builder.ToString();
    }

    //method to create line
    private LineRenderer createLine()
    {
        //create a new empty gameobject and line renderer component
        LineRenderer line = new GameObject("Line" + currLines).AddComponent<LineRenderer>();
        //assign the material to the line
        line.material = lineMaterial;
        //set the number of points to the line
        line.SetVertexCount(3);
        //set the width
        line.SetWidth(0.15f, 0.45f);
        //render line to the world origin and not to the object's position
        line.useWorldSpace = true;
        return line;
    }
}
