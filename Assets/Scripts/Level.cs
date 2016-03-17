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
    GameObject gridPanel = null;
    [SerializeField]
    GameObject leftLasers = null;
    [SerializeField]
    GameObject rightLasers = null;
    [SerializeField]
    GameObject topLasers = null;
    [SerializeField]
    GameObject bottomLasers = null;
    [SerializeField]
    Inventory inventory = null;
    
    [SerializeField]
    Text sizeText = null;
    [SerializeField]
    Text difficultyText = null;

    GameObject[] laserGrids;
    GameObject[] uiGrids;

    CellItem[,] cellItems;
    LaserItem[,] laserItems;

    LineDrawer[,] laserLines;

    private int[,] warppoint;
    
    int LevelSize = 4;
    int Difficulty = 30;

    void Start()
    {
        uiGrids = new GameObject[] { gridPanel, leftLasers, rightLasers, topLasers, bottomLasers };
        laserGrids = new GameObject[] { topLasers, rightLasers, bottomLasers, leftLasers };
        
        GenerateNewLevel();

        HasChanged();
    }

    private void CreateLevelUiGrid()
    {
        foreach (var item in uiGrids)
        {
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

                laserItems[i, j] = laserItem;
                laserLines[i, j] = new LineDrawer(i * LevelSize + j, lineMaterial, laserItem.gameObject);

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

                cellItems[i, j] = cellItem;
            }
        }

        FitGridToScreen();
    }

    int lastWidth = 0;
    int lastHeight = 0;
    void Update()
    {
        if (lastWidth != Screen.width || lastHeight != Screen.height)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            FitGridToScreen();
        }
    }

    private void FitGridToScreen()
    {
        int inventoryCellSize = 80;
        if (Screen.height > Screen.width)
            inventoryCellSize = Screen.width / 6;

        inventory.GetComponent<GridLayoutGroup>().cellSize = new Vector2(inventoryCellSize, inventoryCellSize);
        foreach (var slot in inventory.slots)
        {
            slot.Value.GetComponent<GridLayoutGroup>().cellSize = new Vector2(inventoryCellSize, inventoryCellSize);
            slot.Value.GetComponent<GridLayoutGroup>().spacing = new Vector2(0, -inventoryCellSize);
        }

        int freeWidth = Screen.width;
        int freeHeight = Screen.height - 2 * inventoryCellSize;
        int cellSize = Math.Min(freeWidth, freeHeight) / (LevelSize + 2);
        // remove previous childrens
        foreach (var item in uiGrids)
        {
            item.GetComponent<GridLayoutGroup>().cellSize = new Vector2(cellSize, cellSize);
        }

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                laserItems[i, j].GetComponent<GridLayoutGroup>().cellSize = new Vector2(cellSize, cellSize);
            }
        }
        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                cellItems[i, j].GetComponent<GridLayoutGroup>().cellSize = new Vector2(cellSize, cellSize);
            }
        }
    }

    static int counter = 0;
    private void GenerateNewLevel()
    {
        CreateLevelUiGrid();
        ClearInventory();
        warppoint = new int[2, 2] { { -1, -1 }, { -1, -1 } };
        counter++;
        UnityEngine.Random.seed = counter;
        int difficulty = Difficulty; // original game had 10, 25, 40 for easy, medium, hard
        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                cellItems[i, j].DestroyItem();

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
                    cellItems[i, j].AddItem(newItem);
                }
            }
        }

        // add two warps
        int nwarps = 2;
        if (difficulty <= 30 || LevelSize < 5)
            nwarps = 0;
        else if (LevelSize < 10 )
            nwarps = 2 * UnityEngine.Random.Range(0, 2); // 50% for 0 and 50% for 2

        int tryCounter = 0;
        while (nwarps > 0 && tryCounter < 50)
        {
            int x = UnityEngine.Random.Range(0, LevelSize);
            int y = UnityEngine.Random.Range(0, LevelSize);
            if (cellItems[y, x].itemType == Item.Type.None)
            {
                Image newItem = (Image)Instantiate(Resources.Load(Item.Type.Item_Warp.ToString(), typeof(Image)));
                cellItems[y, x].AddItem(newItem);
                nwarps--;
                warppoint[nwarps, 0] = x;
                warppoint[nwarps, 1] = y;
            }
            tryCounter++;
        }
        if (tryCounter == 50)
            Debug.LogError("Too many warp tries.");
        if (nwarps == 1)
        {
            cellItems[warppoint[0, 1], warppoint[0, 0]].DestroyItem();
        }

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                laserItems[i, j].requiredLength = DrawLaserPath(i, j, false, out laserItems[i, j].returnToSelf);
            }
        }

        MoveAllItemsToInventory();

        UpdateLevelSizeText();
        UpdateDifficultyText();
    }

    private void MoveAllItemsToInventory()
    {
        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                if (cellItems[i, j].itemType != Item.Type.None)
                {
                    inventory.slots[cellItems[i, j].itemType].itemsCount++;
                }
                cellItems[i, j].DestroyItem();
            }
        }
        HasChanged();
    }

    private void ClearInventory()
    {
        foreach(var slot in inventory.slots)
        {
            slot.Value.DestroyItem();
        }
                
        HasChanged();
    }

    private void OnLaserDown(int i, int j)
    {
        laserItems[i, j].drawCurrent = true;
        bool dummy;
        DrawLaserPath(i, j, true, out dummy);
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
        // update warp points
        warppoint = new int[2, 2] { { -1, -1 }, { -1, -1 } };
        int warpIndex = 0;
        for (int x = 0; x < LevelSize; x++)
        {
            for (int y = 0; y < LevelSize; y++)
            {
                if (cellItems[y, x].itemType == Item.Type.Item_Warp)
                {
                    warppoint[warpIndex, 0] = x;
                    warppoint[warpIndex, 1] = y;
                    warpIndex++;
                }
            }
        }

        // check for level completion
        if (IsLevelCompleted())
            GenerateNewLevel();
    }

    private bool IsLevelCompleted()
    {
        bool lasersDone = true;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                var laserItem = laserItems[i, j].GetComponent<LaserItem>();
                bool returnToSelf;
                int currentLength;
                currentLength = DrawLaserPath(i, j, false, out returnToSelf);

                laserItem.currentReturnToSelf = returnToSelf;
                laserItem.currentLength = currentLength;

                if (laserItem.currentLength != laserItem.requiredLength || laserItem.returnToSelf != returnToSelf)
                    lasersDone = false;
            }
        }

        bool itemsDone = true;
        foreach (var slot in inventory.slots)
        {
            if (slot.Value.itemsCount != 0)
            {
                itemsDone = false;
                break;
            }
        }

        return itemsDone && DragHelper.itemBeingDragged == null && lasersDone;
    }

    static void Swap<T>(ref T a, ref T b)
    {
        T c = a;
        a = b;
        b = c;
    }

    private int DrawLaserPath(int i, int j, bool drawLine, out bool returnToSelf)
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

        int startX = x;
        int startY = y;

        int length = 0;

        if (line != null)
        {
            line.Reset();
            line.AddPoint(GetLaserItem(x, y));
        }

        x += dx;
        y += dy;

        while (x >= 0 && y >= 0 && x < LevelSize && y < LevelSize && length < 1000)
        {
            CellItem slot = cellItems[y, x];

            if (line != null)
                line.AddPoint(slot);

            Item.Type itemType = slot.itemType;

            if (itemType == Item.Type.Item_Mirror_BR_TL)
            {
                Swap(ref dx, ref dy);
                dx = -dx;
                dy = -dy;
            }
            else if (itemType == Item.Type.Item_Mirror_TR_BL)
            {
                Swap(ref dx, ref dy);
            }
            else if (itemType == Item.Type.Item_Mirror_R_L)
            {
                dx = -dx;
            }
            else if (itemType == Item.Type.Item_Mirror_T_B)
            {
                dy = -dy;
            }
            else if (itemType == Item.Type.Item_Mirror_T_R_B_L)
            {
                dx = -dx;
                dy = -dy;
            }
            else if (itemType == Item.Type.Item_Warp)
            {
                int warpPointIndex = 0;
                if (x == warppoint[0, 0] && y == warppoint[0, 1])
                    warpPointIndex = 0;
                else if (x == warppoint[1, 0] && y == warppoint[1, 1])
                    warpPointIndex = 1;

                if ((warppoint[1 - warpPointIndex,0] == -1) && (warppoint[1 - warpPointIndex,1] == -1))
                {
                    x = -1;
                    y = -1;
                    dx = 0;
                    dy = 0;
                }
                else
                {
                    x = warppoint[1 - warpPointIndex,0];
                    y = warppoint[1 - warpPointIndex,1];
                    if (line != null)
                        line.AddPoint(cellItems[y, x]);
                }
            }

            x += dx;
            y += dy;

            length++;
        }

        var item = GetLaserItem(x, y);
        if (line != null && item != null)
            line.AddPoint(item);

        returnToSelf = (x == startX && y == startY);

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

    public void IncreaseLevelSize()
    {
        if (LevelSize < 12)
            LevelSize++;
        
        GenerateNewLevel();
    }

    public void DecreaseLevelSize()
    {
        if (LevelSize > 3)
            LevelSize--;
        
        GenerateNewLevel();
    }

    public void IncreaseDifficulty()
    {
        if (Difficulty < 100)
            Difficulty += 10;
        
        GenerateNewLevel();
    }

    public void DecreaseDifficulty()
    {
        if (Difficulty > 10)
            Difficulty -= 10;
        
        GenerateNewLevel();
    }

    private void UpdateLevelSizeText()
    {
        sizeText.text = "Size: " + LevelSize.ToString();
        if (LevelSize == 3)
            sizeText.text += " (Min)";
        if (LevelSize == 12)
            sizeText.text += " (Max)";
    }

    private void UpdateDifficultyText()
    {
        if (Difficulty < 40)
            difficultyText.text = "Easy";
        else if (Difficulty > 60)
            difficultyText.text = "Hard";
        else
            difficultyText.text = "Medium";
        difficultyText.text += " (" + (Difficulty / 10).ToString() + ")";
    }
}
