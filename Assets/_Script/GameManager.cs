// GameManager.cs
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public Transform levelContainer;  // 关卡容器
    public GameObject blockPrefab;    // 方块预制体
    public GameObject levelSelectPanel; // 关卡选择界面

    private LevelData currentLevel;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 初始化显示关卡选择界面
    void Start()
    {
        ShowLevelSelect(true);
    }

    public void ShowLevelSelect(bool show)
    {
        levelSelectPanel.SetActive(show);

        // 暂停/恢复游戏逻辑
        Time.timeScale = show ? 0 : 1;
    }
}