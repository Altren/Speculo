using UnityEngine;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using CellItem = Slot;

public class Level : MonoBehaviour, IHasChanged
{
    [SerializeField]
    Image slotPrefab = null;
    [SerializeField]
    Image laserPrefab = null;

    [SerializeField]
    Material lineMaterial = null;

    [SerializeField]
    Text debugText = null;

    [SerializeField]
    Image gridPanel = null;
    [SerializeField]
    Image leftLasers = null;
    [SerializeField]
    Image rightLasers = null;
    [SerializeField]
    Image topLasers = null;
    [SerializeField]
    Image bottomLasers = null;

    Image[] laserGrids;
    Image[] uiGrids;

    CellItem[,] cellItems;
    LaserItem[,] laserItems;

    LineDrawer[,] laserLines;

    int CellSize = 80;
    int LevelSize = 4;

    void Start()
    {
        uiGrids = new Image[] { gridPanel, leftLasers, rightLasers, topLasers, bottomLasers };
        laserGrids = new Image[] { topLasers, rightLasers, bottomLasers, leftLasers };
        // remove editor test childrens
        foreach (var item in uiGrids)
        {
            item.GetComponent<GridLayoutGroup>().cellSize = new Vector2(CellSize, CellSize);
            ClearTestChilds(item.transform);
        }

        cellItems = new CellItem[LevelSize, LevelSize];
        laserItems = new LaserItem[4, LevelSize];
        laserLines = new LineDrawer[4, LevelSize];

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                LaserItem laserItem = Instantiate(laserPrefab).GetComponent<LaserItem>();
                laserItem.transform.SetParent(laserGrids[i].transform, false);
                laserItem.GetComponent<GridLayoutGroup>().cellSize = new Vector2(CellSize, CellSize);

                laserItems[i, j] = laserItem;
                laserLines[i, j] = new LineDrawer(i * LevelSize + j, lineMaterial);

                int iCopy = i;
                int jCopy = j;
                EventTrigger trigger = laserItem.GetComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback.AddListener((eventData) => { OnLaserDown(iCopy, jCopy); });
                trigger.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerUp;
                entry.callback.AddListener((eventData) => { OnLaserUp(iCopy, jCopy); });
                trigger.triggers.Add(entry);
            }
        }

        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                CellItem cellItem = Instantiate(slotPrefab).GetComponent<Slot>();
                cellItem.transform.SetParent(gridPanel.transform, false);
                cellItem.GetComponent<GridLayoutGroup>().cellSize = new Vector2(CellSize, CellSize);

                cellItems[i, j] = cellItem;
            }
        }

        GenerateNewLevel();

        HasChanged();
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
                    cellItems[i, j].SetItem(newItem);
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                laserItems[i, j].requiredLength = DrawLaserPath(i, j, false);
            }
        }
    }

    private void OnLaserDown(int i, int j)
    {
        laserItems[i, j].drawCurrent = true;
        DrawLaserPath(i, j, true);
    }

    private void OnLaserUp(int i, int j)
    {
        laserItems[i, j].drawCurrent = false;
        ClearLaserPath(i, j);
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
        /*foreach (Slot slot in GetComponentsInChildren<Slot>())
        {
            if (slot.item != null)
            {
                builder.Append(slot.item.sprite.name);
                builder.Append(slot.GetComponent<RectTransform>().anchoredPosition3D);
                builder.Append(" - ");
            }
        }*/

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                var laserItem = laserItems[i, j].GetComponent<LaserItem>();
                laserItem.currentLength = DrawLaserPath(i, j, false);
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

    private int DrawLaserPath(int i, int j, bool drawLine)
    {
        LineDrawer line = null;
        if (drawLine)
            line = laserLines[i, j];
        int[,] directions = new int[4, 2] { { 0, 1 }, { -1, 0 }, { 0, -1 }, { 1, 0 } };
        int[,] positions = new int[4, 2] { { j, -1 }, { LevelSize, j }, { j, LevelSize }, { -1, j } };

        int x = positions[i, 0];
        int y = positions[i, 1];
        int dx = directions[i, 0];
        int dy = directions[i, 1];

        int length = 0;

        if (line != null)
        {
            line.Reset();
            line.AddPoint(GetLaserItem(x, y));
        }

        x += dx;
        y += dy;

        while (x >= 0 && y >= 0 && x < LevelSize && y < LevelSize)
        {
            CellItem slot = cellItems[y, x];

            if (line != null)
                line.AddPoint(slot);

            Item.Type mirrorType = Item.Type.None;
            if (slot.item != null)
            {
                mirrorType = (Item.Type)Enum.Parse(typeof(Item.Type), slot.item.sprite.name);
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
            line.AddPoint(GetLaserItem(x, y));

        return length;
    }

    private void ClearLaserPath(int i, int j)
    {
        LineDrawer line = laserLines[i, j];
        line.Reset();
    }

    LaserItem GetLaserItem(int x, int y)
    {
        if (y == -1)
        {
            return laserItems[0, x];
        }
        if (x == LevelSize)
        {
            return laserItems[1, y];
        }
        if (y == LevelSize)
        {
            return laserItems[2, x];
        }
        if (x == -1)
        {
            return laserItems[3, y];
        }
        return null;
    }
}
