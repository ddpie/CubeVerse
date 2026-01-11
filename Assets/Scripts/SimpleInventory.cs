using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 简单背包系统 - 存储收集的方块
/// </summary>
public class SimpleInventory : MonoBehaviour
{
    [Header("背包设置")]
    public int maxSlots = 9;                      // 最大槽位数
    public int maxStackSize = 64;                 // 每个槽位最大堆叠数
    public int selectedSlot = 0;                  // 当前选中的槽位
    
    [Header("初始物品")]
    public int initialBlockCount = 20;            // 初始方块数量
    public Color initialBlockColor = new Color(0.6f, 0.4f, 0.2f); // 初始方块颜色（泥土色）
    
    [Header("UI设置")]
    public bool showInventoryUI = true;
    public KeyCode[] slotKeys = new KeyCode[] 
    { 
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
        KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
    };
    
    // 背包槽位
    private List<InventorySlot> slots = new List<InventorySlot>();
    
    // UI相关
    private GUIStyle slotStyle;
    private GUIStyle selectedSlotStyle;
    private GUIStyle countStyle;
    private GUIStyle slotNumberStyle;
    private bool stylesInitialized = false;
    
    void Start()
    {
        InitializeInventory();
    }
    
    void InitializeInventory()
    {
        // 初始化槽位
        slots.Clear();
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot());
        }
        
        // 添加初始方块
        if (initialBlockCount > 0)
        {
            AddBlock(initialBlockColor, initialBlockCount);
        }
        
        // 添加一些不同颜色的方块作为初始物品
        AddBlock(new Color(0.4f, 0.7f, 0.2f), 10); // 草地色
        AddBlock(new Color(0.5f, 0.5f, 0.5f), 10); // 石头色
        AddBlock(new Color(0.9f, 0.8f, 0.5f), 5);  // 沙子色
        
        Debug.Log("SimpleInventory: 背包已初始化");
    }
    
    void Update()
    {
        HandleSlotSelection();
        HandleScrollWheel();
    }
    
    /// <summary>
    /// 处理数字键选择槽位
    /// </summary>
    void HandleSlotSelection()
    {
        for (int i = 0; i < slotKeys.Length && i < maxSlots; i++)
        {
            if (Input.GetKeyDown(slotKeys[i]))
            {
                selectedSlot = i;
            }
        }
    }
    
    /// <summary>
    /// 处理滚轮切换槽位
    /// </summary>
    void HandleScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                selectedSlot--;
                if (selectedSlot < 0) selectedSlot = maxSlots - 1;
            }
            else
            {
                selectedSlot++;
                if (selectedSlot >= maxSlots) selectedSlot = 0;
            }
        }
    }
    
    /// <summary>
    /// 添加方块到背包
    /// </summary>
    public bool AddBlock(Color color, int count = 1)
    {
        // 首先尝试堆叠到已有的相同颜色槽位
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].isEmpty && ColorEquals(slots[i].blockColor, color))
            {
                int spaceLeft = maxStackSize - slots[i].count;
                if (spaceLeft > 0)
                {
                    int toAdd = Mathf.Min(count, spaceLeft);
                    slots[i].count += toAdd;
                    count -= toAdd;
                    
                    if (count <= 0) return true;
                }
            }
        }
        
        // 如果还有剩余，放入空槽位
        while (count > 0)
        {
            int emptySlot = FindEmptySlot();
            if (emptySlot == -1)
            {
                Debug.Log("SimpleInventory: 背包已满");
                return false;
            }
            
            int toAdd = Mathf.Min(count, maxStackSize);
            slots[emptySlot].blockColor = color;
            slots[emptySlot].count = toAdd;
            slots[emptySlot].isEmpty = false;
            count -= toAdd;
        }
        
        return true;
    }
    
    /// <summary>
    /// 从当前选中槽位移除一个方块
    /// </summary>
    public Color RemoveBlock()
    {
        if (selectedSlot < 0 || selectedSlot >= slots.Count)
            return Color.white;
        
        InventorySlot slot = slots[selectedSlot];
        if (slot.isEmpty || slot.count <= 0)
            return Color.white;
        
        Color color = slot.blockColor;
        slot.count--;
        
        if (slot.count <= 0)
        {
            slot.isEmpty = true;
            slot.blockColor = Color.white;
        }
        
        return color;
    }
    
    /// <summary>
    /// 检查当前槽位是否有方块
    /// </summary>
    public bool HasBlocks()
    {
        if (selectedSlot < 0 || selectedSlot >= slots.Count)
            return false;
        
        return !slots[selectedSlot].isEmpty && slots[selectedSlot].count > 0;
    }
    
    /// <summary>
    /// 获取当前选中槽位的方块颜色
    /// </summary>
    public Color GetSelectedBlockColor()
    {
        if (selectedSlot < 0 || selectedSlot >= slots.Count)
            return Color.white;
        
        return slots[selectedSlot].blockColor;
    }
    
    /// <summary>
    /// 获取当前选中槽位的方块数量
    /// </summary>
    public int GetSelectedBlockCount()
    {
        if (selectedSlot < 0 || selectedSlot >= slots.Count)
            return 0;
        
        return slots[selectedSlot].count;
    }
    
    /// <summary>
    /// 查找空槽位
    /// </summary>
    int FindEmptySlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].isEmpty)
                return i;
        }
        return -1;
    }
    
    /// <summary>
    /// 比较两个颜色是否相等（允许小误差）
    /// </summary>
    bool ColorEquals(Color a, Color b)
    {
        float threshold = 0.01f;
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }
    
    void OnGUI()
    {
        if (!showInventoryUI) return;
        
        InitStyles();
        DrawHotbar();
    }
    
    void InitStyles()
    {
        if (stylesInitialized) return;
        
        slotStyle = new GUIStyle(GUI.skin.box);
        slotStyle.normal.background = MakeTexture(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.8f));
        slotStyle.alignment = TextAnchor.MiddleCenter;
        
        selectedSlotStyle = new GUIStyle(GUI.skin.box);
        selectedSlotStyle.normal.background = MakeTexture(2, 2, new Color(0.5f, 0.5f, 0.5f, 0.95f));
        selectedSlotStyle.alignment = TextAnchor.MiddleCenter;
        
        countStyle = new GUIStyle(GUI.skin.label);
        countStyle.fontSize = 14;
        countStyle.fontStyle = FontStyle.Bold;
        countStyle.normal.textColor = Color.white;
        countStyle.alignment = TextAnchor.LowerRight;
        
        slotNumberStyle = new GUIStyle(GUI.skin.label);
        slotNumberStyle.fontSize = 14;
        slotNumberStyle.fontStyle = FontStyle.Bold;
        slotNumberStyle.normal.textColor = Color.white;
        slotNumberStyle.alignment = TextAnchor.MiddleCenter;
        
        stylesInitialized = true;
    }
    
    void DrawHotbar()
    {
        float slotSize = 50f;
        float padding = 5f;
        float totalWidth = maxSlots * (slotSize + padding) - padding;
        float startX = (Screen.width - totalWidth) / 2;
        float startY = Screen.height - slotSize - 20f;
        
        for (int i = 0; i < maxSlots; i++)
        {
            float x = startX + i * (slotSize + padding);
            Rect slotRect = new Rect(x, startY, slotSize, slotSize);
            
            // 绘制槽位背景
            GUIStyle style = (i == selectedSlot) ? selectedSlotStyle : slotStyle;
            GUI.Box(slotRect, "", style);
            
            // 绘制方块颜色预览
            if (!slots[i].isEmpty && slots[i].count > 0)
            {
                Rect colorRect = new Rect(x + 5, startY + 5, slotSize - 10, slotSize - 10);
                Texture2D colorTex = MakeTexture(1, 1, slots[i].blockColor);
                GUI.DrawTexture(colorRect, colorTex);
                
                // 绘制数量（右下角，带阴影）
                Rect countShadowRect = new Rect(x + 1, startY + 1, slotSize - 3, slotSize - 3);
                GUI.color = Color.black;
                GUI.Label(countShadowRect, slots[i].count.ToString(), countStyle);
                GUI.color = Color.white;
                
                Rect countRect = new Rect(x, startY, slotSize - 3, slotSize - 3);
                GUI.Label(countRect, slots[i].count.ToString(), countStyle);
            }
            
            // 绘制槽位编号（槽位上方）
            Rect numRect = new Rect(x, startY - 18, slotSize, 16);
            GUI.color = (i == selectedSlot) ? Color.yellow : Color.white;
            GUI.Label(numRect, (i + 1).ToString(), slotNumberStyle);
            GUI.color = Color.white;
        }
        
        // 绘制准星
        DrawCrosshair();
    }
    
    void DrawCrosshair()
    {
        float size = 20f;
        float thickness = 2f;
        float centerX = Screen.width / 2;
        float centerY = Screen.height / 2;
        
        Color crosshairColor = new Color(1f, 1f, 1f, 0.8f);
        Texture2D tex = MakeTexture(1, 1, crosshairColor);
        
        // 水平线
        GUI.DrawTexture(new Rect(centerX - size / 2, centerY - thickness / 2, size, thickness), tex);
        // 垂直线
        GUI.DrawTexture(new Rect(centerX - thickness / 2, centerY - size / 2, thickness, size), tex);
    }
    
    Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
    
    /// <summary>
    /// 背包槽位数据
    /// </summary>
    [System.Serializable]
    public class InventorySlot
    {
        public Color blockColor = Color.white;
        public int count = 0;
        public bool isEmpty = true;
    }
}
