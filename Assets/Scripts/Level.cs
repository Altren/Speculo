using UnityEngine;
using System;
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

    CellItem[,] cellItems;
    LaserItem[,] lazerItems;

    int CellSize = 80;
    int LevelSize = 4;

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

                int rand = UnityEngine.Random.Range(0, 2);
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
            line = createLine(0);
        int counter = 0;
        Vector3 lastPosition = new Vector3(0, 0, 0);
        /*foreach (Slot slot in GetComponentsInChildren<Slot>())
        {
            if (slot.item != null)
            {
                builder.Append(slot.item.sprite.name);
                builder.Append(slot.GetComponent<RectTransform>().anchoredPosition3D);
                builder.Append(" - ");
            }
        }*/


        for (int i = 0; i < 1/*4*/; i++)
        {
            for (int j = 0; j < 1/*LevelSize*/; j++)
            {
                drawLazerPath(-1, 0, 1, 0, line);
            }
        }

        debugText.text = builder.ToString();
    }

    static void Swap<T>(ref T a, ref T b)
    {
        T c = a;
        a = b;
        b = c;
    }

    private int drawLazerPath(int x, int y, int dx, int dy, LineRenderer line)
    {
        int length = 0;
        Vector3 lastPosition = new Vector3(0, 0, 0);

        x += dx;
        y += dy;

        while (x >= 0 && y >= 0 && x < LevelSize && y < LevelSize)
        {
            CellItem item = cellItems[y, x];

            Vector3 position = Camera.main.ScreenToWorldPoint(item.transform.position);
            Vector3 prePosition = length == 0 ? position : Vector3.MoveTowards(position, lastPosition, 0.001f);
            line.SetVertexCount(2 * (length + 1));
            line.SetPosition(2 * length, prePosition);
            line.SetPosition(2 * length + 1, position);
            //line.SetPosition(3 * counter + 2, position);
            lastPosition = position;

            Item.Type mirrorType = Item.Type.None;
            if (item.GetComponent<Slot>().item != null)
            {
                mirrorType = (Item.Type)Enum.Parse(typeof(Item.Type), item.GetComponent<Slot>().item.sprite.name);
            }

            if (mirrorType == Item.Type.Item_Mirror_BR_TL)
            {
                Swap(ref dx, ref dy);
                dx = -dx;
                dy = -dy;
            }
            else if(mirrorType == Item.Type.Item_Mirror_TR_BL)
            {
                Swap(ref dx, ref dy);
            }
            else if (mirrorType == Item.Type.Item_Mirror_R_L)
            {
                dx = -dx;
            }
            else if (mirrorType == Item.Type.Item_Mirror_T_B)
            {
                dy = -dy;
            }
            else if (mirrorType == Item.Type.Item_Mirror_T_R_B_L)
            {
                dx = -dx;
                dy = -dy;
            }

            x += dx;
            y += dy;

            length++;
        }
        return length;
    }

    //method to create line
    private LineRenderer createLine(int index)
    {
        //create a new empty gameobject and line renderer component
        line = new GameObject("Line" + index).gameObject.AddComponent<LineRenderer>();
        //assign the material to the line
        line.material = lineMaterial;
        //set the width
        line.SetWidth(0.15f, 0.15f);
        //render line to the world origin and not to the object's position
        line.useWorldSpace = true;
        return line;
    }
}
