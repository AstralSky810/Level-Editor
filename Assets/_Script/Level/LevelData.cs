using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Level/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public Vector2Int gridSize;
    public List<BlockEntry> blocks = new List<BlockEntry>();

    [HideInInspector]
    public int sortOrder; // 用于保存排序顺序
}

[System.Serializable]
public class BlockEntry
{
    public Vector2 position;
    public Color color; // 示例: "#FF0000FF"
    public BlockType type;
}
public enum BlockType { Normal, Obstacle }

[System.Serializable]
public class RuntimeLevelData
{
    public Vector2Int gridSize;
    public List<BlockEntry> blocks = new List<BlockEntry>();

    // 深拷贝构造函数
    public RuntimeLevelData(LevelData source)
    {
        gridSize = source.gridSize;
        blocks = new List<BlockEntry>();
        foreach (var block in source.blocks)
        {
            blocks.Add(new BlockEntry
            {
                position = block.position,
                color = block.color,
                type = block.type
            });
        }
    }
}

