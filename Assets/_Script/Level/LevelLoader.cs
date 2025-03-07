using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    public void LoadLevel(LevelData levelData)
    {
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

            InstantiateBlock(block, position);

            if (++count % blocksPerFrame == 0)
                yield return null; // 分帧生成
        }

        GameManager.Instance.ShowLevelSelect(false);
    }

    private void InstantiateBlock(BlockEntry data, Vector3 position)
    {
        GameObject block = ObjectPool.Instance.GetBlock();
        block.transform.position = position;
        block.GetComponent<Block>().data = data;
        block.tag = data.type == BlockType.Normal ? "Collectible" : "Obstacle";

        block.GetComponent<Renderer>().material.color = data.color;
        block.transform.SetParent(GameManager.Instance.levelContainer);
    }

    private void ClearCurrentLevel()
    {
        foreach (Transform child in GameManager.Instance.levelContainer)
        {
            ObjectPool.Instance.ReturnBlock(child.gameObject);
        }
    }
}