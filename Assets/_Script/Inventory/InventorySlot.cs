using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public Image icon;
    public int globalIndex;

    public BlockEntry CurrentBlock { get; private set; }
    public bool IsEmpty => CurrentBlock == null;

    void Awake()
    {
        ClearSlot(); // 初始化时清空
    }

    // 修改SetBlock方法
    public void SetBlock(BlockEntry block, int index)
    {
        CurrentBlock = block;
        icon.color = block.color;
        icon.gameObject.SetActive(true);

        globalIndex = index;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsEmpty) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 放置逻辑
            LevelManager.Instance.PlaceBlock(CurrentBlock, hit.point);
            InventorySystem.Instance.RemoveItem(globalIndex);
        }

    }

    public void ClearSlot()
    {
        CurrentBlock = null;
        icon.gameObject.SetActive(false);
    }
}