// LevelButton.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI levelNameText;

    void Awake()
    {
        thumbnail = GetComponent<Image>();
        levelNameText = GetComponentInChildren<TextMeshProUGUI>();

    }

    public void Initialize(LevelData data, System.Action onClick)
    {
        levelNameText.text = data.name;
        GetComponent<Button>().onClick.AddListener(() => onClick());
    }

}