using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private GameObject levelButtonPrefab;

    void Awake()
    {
        scrollView = GetComponent<ScrollRect>();
    }

    void Start()
    {
        LoadLevelButtons();
    }

    private void LoadLevelButtons()
    {
        LevelData[] sources = Resources.LoadAll<LevelData>("Levels");
        // 使用自然排序对文件名进行排序
        List<LevelData> levels = sources.OrderBy(level => ExtractNumberFromName(level.name)).ToList();

        foreach (var level in levels)
        {
            GameObject buttonObj = Instantiate(levelButtonPrefab, scrollView.content);
            LevelButton button = buttonObj.GetComponent<LevelButton>();
            button.Initialize(level, () =>
            {
                LevelManager.Instance.LoadLevel(level);
            });
        }
    }

    // 从文件名中提取数字
    private int ExtractNumberFromName(string fileName)
    {
        // 匹配文件名中的数字部分（支持格式如 "Level 1", "Level1", "Stage3" 等）
        Match match = Regex.Match(fileName, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int number))
        {
            return number;
        }
        return int.MaxValue; // 未找到数字的文件排到最后
    }
}