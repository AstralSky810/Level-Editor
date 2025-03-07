using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("References")]
    public GameObject blockPrefab; // 方块预制体
    public Transform levelContainer; // 关卡容器对象

    public LevelData SelectedLevel { get; set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // 通过ScriptableObject加载关卡
    public void LoadLevel(LevelData levelData)
    {
        ClearLevel();
        SelectedLevel = levelData;

        StartCoroutine(LoadLevelRoutine(levelData));
    }

    private IEnumerator LoadLevelRoutine(LevelData levelData)
    {
        // 清空旧关卡
        ClearCurrentLevel();

        // 异步生成新关卡
        int blocksPerFrame = 10; // 每帧生成数量
        int count = 0;

        foreach (var block in levelData.blocks)
        {
            Vector3 position = new Vector3(
                block.position.x,
                0,
                block.position.y);

            InstantiateBlock(block, levelContainer.transform.position + position);

            if (++count % blocksPerFrame == 0)
                yield return null; // 分帧生成
        }

        GameManager.Instance.ShowLevelSelect(false);
    }

    private void ClearCurrentLevel()
    {
        foreach (Transform child in GameManager.Instance.levelContainer)
        {
            ObjectPool.Instance.ReturnBlock(child.gameObject);
        }
    }

    private void InstantiateBlock(BlockEntry data, Vector3 position)
    {
        GameObject newBlock = Instantiate(blockPrefab, position, Quaternion.identity, levelContainer);
        Block block = newBlock.GetComponent<Block>();
        block.data = data;
        block.tag = data.type == BlockType.Normal ? "Collectible" : "Obstacle";
        if(data.type == BlockType.Obstacle) 
        {
            newBlock.GetComponent<BoxCollider>().isTrigger = false;
        }

        newBlock.GetComponent<Renderer>().material.color = data.color;
    }

    // 获取所有关卡资源
    public LevelData[] GetAllLevels()
    {
        return Resources.LoadAll<LevelData>("Levels");
    }

    private void ClearLevel()
    {
        foreach (Transform child in levelContainer)
            Destroy(child.gameObject);
    }

    // 放置方块方法（适配ScriptableObject）
    public void PlaceBlock(BlockEntry data, Vector3 position)
    {
        InstantiateBlock(data, position);

    }
}