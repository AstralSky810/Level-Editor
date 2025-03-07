// ObjectPool.cs
using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    public GameObject blockPrefab;
    public int initialPoolSize = 100;

    private Queue<GameObject> availableBlocks = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject block = Instantiate(blockPrefab);
            block.SetActive(false);
            availableBlocks.Enqueue(block);
        }
    }

    public GameObject GetBlock()
    {
        if (availableBlocks.Count == 0)
            ExpandPool(10);

        GameObject block = availableBlocks.Dequeue();
        block.SetActive(true);
        return block;
    }

    public void ReturnBlock(GameObject block)
    {
        block.SetActive(false);
        availableBlocks.Enqueue(block);
    }

    private void ExpandPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject block = Instantiate(blockPrefab);
            block.SetActive(false);
            availableBlocks.Enqueue(block);
        }
    }
}