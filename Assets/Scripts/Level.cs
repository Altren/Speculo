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
    public Image[] lazerGrids;

    Image[] uiGrids;

    public Material lineMaterial;
    //number of lines drawn
    private int currLines = 0;

    CellItem[,] cellItems;
    LaserItem[,] lazerItems;

    int CellSize = 80;

    void Start()
    {
        uiGrids = new Image[] {gridPanel, leftLasers, rightLasers, topLasers, bottomLasers};
        lazerGrids = new Image[] { topLasers, rightLasers, bottomLasers, leftLasers };
        // remove editor test childrens
        foreach (var item in uiGrids)
        {
            item.GetComponent<GridLayoutGroup>().cellSize = new Vector2(CellSize, CellSize);
            ClearTestChilds(item.transform);
        }

        int LevelSize = 4;
        cellItems = new CellItem[LevelSize, LevelSize];
        lazerItems = new LaserItem[4, LevelSize];
        
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                LaserItem lazerItem = (LaserItem)Instantiate(lazerPrefab);
                lazerItem.transform.SetParent(lazerGrids[i].transform, false);
                lazerItem.GetComponent<GridLayoutGroup>().cellSize = new Vector2(CellSize, CellSize);

                lazerItems[i, j] = lazerItem;
            }
        }

        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                CellItem cellItem = (Image)Instantiate(slotPrefab);
                cellItem.transform.SetParent(gridPanel.transform, false);
                cellItem.GetComponent<GridLayoutGroup>().cellSize = new Vector2(CellSize, CellSize);

                cellItems[i, j] = cellItem;

                int rand = Random.Range(0, 2);
                if (rand != 0)
                {
                    Image item = (Image)Instantiate(itemPrefab);
                    cellItem.GetComponent<Slot>().item = item;
                    item.transform.SetParent(cellItem.transform, false);
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

    LineRenderer line = null;

    public void HasChanged()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append(" - ");
        if (!line)
            line = createLine();
        int counter = 0;
        Vector3 lastPosition = new Vector3(0, 0, 0);
        foreach (Slot slot in GetComponentsInChildren<Slot>())
        {
            if (slot.item != null)
            {
                if (counter < 5)
                {
                    Vector3 position = Camera.main.ScreenToWorldPoint(slot.transform.position);
                    Vector3 prePosition = counter == 0 ? position : Vector3.MoveTowards(position, lastPosition, 0.001f);

                    line.SetPosition(2 * counter, prePosition);
                    line.SetPosition(2 * counter + 1, position);
                    //line.SetPosition(3 * counter + 2, position);
                    lastPosition = position;
                    counter++;

                    builder.Append(position);
                    builder.Append(" - ");
                }
                //builder.Append(slot.name);
                //builder.Append(slot.GetComponent<RectTransform>().anchoredPosition3D);
                //builder.Append(" - ");
            }
        }
        
        debugText.text = builder.ToString();
    }

    //method to create line
    private LineRenderer createLine()
    {
        //create a new empty gameobject and line renderer component
        line = new GameObject("Line" + currLines).gameObject.AddComponent<LineRenderer>();
        //assign the material to the line
        line.material = lineMaterial;
        //set the number of points to the line
        line.SetVertexCount(10);
        //set the width
        line.SetWidth(0.15f, 0.15f);
        //render line to the world origin and not to the object's position
        line.useWorldSpace = true;
        return line;
    }
}
