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
        uiGrids = new Image[] { gridPanel, leftLasers, rightLasers, topLasers, bottomLasers };
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
            }
        }

        //HasChanged();

        GenerateNewLevel();
    }

    private void GenerateNewLevel()
    {
        int difficulty = 40; // original game had 10, 25, 40 for easy, medium, hard
        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                Item.Type type = Item.Type.None;
                int rnd = UnityEngine.Random.Range(0, 100);
                if (rnd < 100 - difficulty) type = Item.Type.None;                             //(100-difficulty)% empty
                else if (rnd < 100 - difficulty * 3 / 4) type = Item.Type.Item_Mirror_BR_TL;   //(difficulty/4)% upright
                else if (rnd < 100 - difficulty * 1 / 2) type = Item.Type.Item_Mirror_TR_BL;   //(difficulty/4)% upleft
                else if (rnd < 100 - difficulty * 2 / 6) type = Item.Type.Item_Mirror_R_L;     //(difficulty/6)% vertical
                else if (rnd < 100 - difficulty * 1 / 6) type = Item.Type.Item_Mirror_T_B;     //(difficulty/6)% horizontal
                else/*if (rnd < 100 - difficulty * 0/6)*/type = Item.Type.Item_Mirror_T_R_B_L; //(difficulty/6)% block

                if (type != Item.Type.None)
                {
                    Image newItem = (Image)Instantiate(Resources.Load(type.ToString(), typeof(Image)));
                    cellItems[i, j].GetComponent<Slot>().SetItem(newItem);
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                int[,] directions = new int[4, 2] { { 0, 1 }, { -1, 0 }, { 0, -1 }, { 1, 0 } };
                int[,] positions = new int[4, 2] { { j, -1 }, { LevelSize, j }, { j, LevelSize }, { -1, j } };

                var countText = lazerItems[i, j].GetComponentInChildren<Text>();
                int length = drawLazerPath(positions[i,0], positions[i, 1], directions[i,0], directions[i, 1], null);
                countText.text = length.ToString();
                Debug.Log(length.ToString());
            }
        }
    }

    private void ClearTestChilds(Transform panel)
    {
        foreach (Transform t in panel)
        {
            Destroy(t.gameObject);
        }
    }

    LineDrawer line = null;

    public void HasChanged()
    {

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        builder.Append(" - ");
        /*foreach (Slot slot in GetComponentsInChildren<Slot>())
        {
            if (slot.item != null)
            {
                builder.Append(slot.item.sprite.name);
                builder.Append(slot.GetComponent<RectTransform>().anchoredPosition3D);
                builder.Append(" - ");
            }
        }*/

        if (line == null)
            line = new LineDrawer(0, lineMaterial);

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

    private int drawLazerPath(int x, int y, int dx, int dy, LineDrawer line)
    {
        int length = 0;

        if (line != null)
        {
            line.Reset();
            line.AddPoint(GetLazerItem(x, y));
        }

        x += dx;
        y += dy;

        while (x >= 0 && y >= 0 && x < LevelSize && y < LevelSize)
        {
            CellItem item = cellItems[y, x];

            if (line != null)
                line.AddPoint(item);

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

        if (line != null)
            line.AddPoint(GetLazerItem(x, y));

        return length;
    }

    LaserItem GetLazerItem(int x, int y)
    {
        if (y == -1)
        {
            return lazerItems[0, x];
        }
        if (x == LevelSize)
        {
            return lazerItems[1, y];
        }
        if (y == LevelSize)
        {
            return lazerItems[2, x];
        }
        if (x == -1)
        {
            return lazerItems[3, y];
        }
        return null;
    }
}
