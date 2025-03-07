using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("UI References")]
    public Transform inventoryGrid;
    public GameObject slotPrefab;


    private List<BlockEntry> collectedBlocks = new List<BlockEntry>();
    private List<InventorySlot> slots = new List<InventorySlot>();

    [Header("Pagination")]
    public Button prevPageButton;
    public Button nextPageButton;
    public TextMeshProUGUI pageText;

    public const int PAGE_SIZE = 10; // 2x10
    public const int totalPages = 2;
    private int currentPage;
    private int inventorySize = PAGE_SIZE * totalPages; // 最大容量

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeSlots();
        UpdatePageUI();
        prevPageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.AddListener(NextPage);
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < PAGE_SIZE; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, inventoryGrid);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slot.ClearSlot(); // 初始隐藏
            slots.Add(slot);
        }
        UpdatePageUI();
    }

    public void AddItem(BlockEntry block)
    {
        collectedBlocks.Add(block);

        UpdatePageUI();
    }

    public void RemoveItem(int globalIndex)
    {
        if (globalIndex < collectedBlocks.Count)
        {
            collectedBlocks.RemoveAt(globalIndex);
            UpdatePageUI();
        }
    }

    public bool IsFull()
    {
        return collectedBlocks.Count >= inventorySize;
    }

    private void NextPage()
    {
        currentPage = Mathf.Min(currentPage + 1, totalPages - 1);
        UpdatePageUI();
    }

    private void PrevPage()
    {
        currentPage = Mathf.Max(currentPage - 1, 0);
        UpdatePageUI();
    }

    private void UpdatePageUI()
    {
        // 计算有效物品总数
        int totalItems = slots.Count(s => !s.IsEmpty);

        int startIndex = currentPage * PAGE_SIZE;
        int endIndex = (currentPage + 1) * PAGE_SIZE;

        for (int i = 0; i < PAGE_SIZE; i++)
        {
            int globalIndex = startIndex + i;

            if (globalIndex < collectedBlocks.Count)
            {
                slots[i].SetBlock(collectedBlocks[globalIndex], globalIndex);
                slots[i].gameObject.SetActive(true);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }

        pageText.text = $"Page: {currentPage + 1}/{totalPages}";

        // 按钮状态
        prevPageButton.interactable = currentPage > 0;
        nextPageButton.interactable = currentPage < totalPages - 1;

    }
}