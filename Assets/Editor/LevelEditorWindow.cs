using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class LevelEditorWindow : EditorWindow
{
    // 窗口布局参数
    private const float LIST_WIDTH = 200f;
    private const float PRESET_WIDTH = 150f;
    private Vector2 listScrollPos;
    private Vector2 editorScrollPos;

    // 关卡数据
    private LevelData currentLevel;
    private List<LevelData> allLevels = new List<LevelData>();

    // 编辑器状态
    private Color selectedColor = Color.white;
    private BlockType selectedType;
    private bool isDragging;

    private const int UNDO_STACK_SIZE = 50;
    private Stack<LevelSnapshot> undoStack = new Stack<LevelSnapshot>();
    private Stack<LevelSnapshot> redoStack = new Stack<LevelSnapshot>();

    [MenuItem("Tools/Level Editor")]
    static void Init() => GetWindow<LevelEditorWindow>("Level Editor");

    void OnEnable()
    {
        InitializePresets();
        RefreshLevelList();
        if (allLevels.Count > 0) currentLevel = allLevels[0];
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
    }

    void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    }

    private void OnUndoRedoPerformed()
    {
        Repaint();
        EditorUtility.SetDirty(currentLevel);
    }

    void OnGUI()
    {
        //HandleInput();
        GUILayout.BeginHorizontal();
        DrawLevelList();
        DrawMainEditor();
        DrawPresetPanel();
        GUILayout.EndHorizontal();

        HandleDragAndDrop();
        HandleGridInteraction();
    }

    #region 关卡列表区域
    private void DrawLevelList()
    {
        GUILayout.BeginVertical(GUILayout.Width(LIST_WIDTH));

        // 列表标题
        GUILayout.Label("关卡列表", EditorStyles.boldLabel);

        // 刷新按钮
        if (GUILayout.Button("刷新列表", EditorStyles.miniButton))
            RefreshLevelList();

        // 滚动视图
        listScrollPos = GUILayout.BeginScrollView(listScrollPos);
        foreach (var level in allLevels)
        {
            GUILayout.BeginHorizontal();

            // 关卡按钮
            bool isSelected = currentLevel == level;
            if (GUILayout.Toggle(isSelected, level.name, "Button",
                GUILayout.ExpandWidth(true)))
            {
                if (!isSelected) LoadLevel(level);
            }

            // 删除按钮
            if (GUILayout.Button("×", GUILayout.Width(20)))
                DeleteLevel(level);

            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        // 新建按钮
        if (GUILayout.Button("新建关卡"))
            CreateNewLevel();

        GUILayout.EndVertical();
    }

    private void RefreshLevelList()
    {
        allLevels = AssetDatabase.FindAssets("t:LevelData")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<LevelData>(path))
            .OrderBy(level => level.name)
            .ToList();
    }
    #endregion

    #region 撤销/重做系统
    private class LevelSnapshot
    {
        public List<BlockEntry> blocks;
        public Vector2Int gridSize;
    }

    private void RecordUndoState(string actionName)
    {
        // 创建深度拷贝
        var snapshot = new LevelSnapshot
        {
            blocks = currentLevel.blocks.Select(b => new BlockEntry
            {
                position = b.position,
                color = b.color,
                type = b.type
            }).ToList(),
            gridSize = currentLevel.gridSize
        };

        undoStack.Push(snapshot);
        redoStack.Clear();

        // 限制历史记录数量
        while (undoStack.Count > UNDO_STACK_SIZE)
        {
            undoStack = new Stack<LevelSnapshot>(undoStack.Take(UNDO_STACK_SIZE));
        }
    }
    private void PerformUndo()
    {
        if (undoStack.Count == 0) return;

        // 当前状态存入重做堆栈
        redoStack.Push(CreateSnapshot(currentLevel));

        // 恢复上一个状态
        var snapshot = undoStack.Pop();
        ApplySnapshot(snapshot);

    }

    private void PerformRedo()
    {
        if (redoStack.Count == 0) return;

        // 当前状态存入撤销堆栈
        undoStack.Push(CreateSnapshot(currentLevel));

        // 恢复下一个状态
        var snapshot = redoStack.Pop();
        ApplySnapshot(snapshot);

    }

    private LevelSnapshot CreateSnapshot(LevelData level)
    {
        return new LevelSnapshot
        {
            blocks = level.blocks.ToList(),
            gridSize = level.gridSize
        };
    }

    private void ApplySnapshot(LevelSnapshot snapshot)
    {
        currentLevel.blocks.Clear();
        currentLevel.blocks.AddRange(snapshot.blocks);
        currentLevel.gridSize = snapshot.gridSize;
        EditorUtility.SetDirty(currentLevel);
        Repaint();
    }


    #endregion

    #region 主编辑区域
    private void DrawMainEditor()
    {
        if (currentLevel == null) return;

        GUILayout.BeginVertical();

        // 工具栏
        DrawToolbar();

        // 网格区域
        editorScrollPos = GUILayout.BeginScrollView(editorScrollPos);
        DrawGrid();
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        // 撤销按钮
        GUI.enabled = undoStack.Count > 0;
        if (GUILayout.Button("← Undo", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            PerformUndo();
        }

        // 重做按钮
        GUI.enabled = redoStack.Count > 0;
        if (GUILayout.Button("Redo →", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            PerformRedo();
        }
        GUI.enabled = true;

        // 关卡名称编辑
        string newName = EditorGUILayout.DelayedTextField(
            currentLevel.name,
            EditorStyles.toolbarTextField);

        if (newName != currentLevel.name)
            RenameLevel(currentLevel, newName);

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
    }
    #endregion

    #region 核心功能
    private void CreateNewLevel()
    {
        RecordUndoState("Create Level");  // 新建时记录初始状态

        LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
        newLevel.name = "New Level";
        newLevel.gridSize = new Vector2Int(20, 20);

        string path = EditorUtility.SaveFilePanelInProject(
            "新建关卡",
            "NewLevel.asset",
            "asset",
            "选择保存位置");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newLevel, path);
            RefreshLevelList();
            LoadLevel(newLevel);
        }

    }

    private void LoadLevel(LevelData level)
    {
        currentLevel = level;
        Repaint();
    }

    private void SaveLevel(LevelData level)
    {
        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();
        Debug.Log($"已保存关卡: {level.name}");
    }

    private void RenameLevel(LevelData level, string newName)
    {
        RecordUndoState("Rename Level");

        string path = AssetDatabase.GetAssetPath(level);
        AssetDatabase.RenameAsset(path, newName);
        RefreshLevelList();
    }

    private void DeleteLevel(LevelData level)
    {
        if (EditorUtility.DisplayDialog("删除关卡",
            $"确定要永久删除关卡 {level.name} 吗？",
            "删除", "取消"))
        {
            string path = AssetDatabase.GetAssetPath(level);
            AssetDatabase.DeleteAsset(path);
            RefreshLevelList();
            if (currentLevel == level) currentLevel = null;
        }
    }
    #endregion

    #region 网格绘制
    private const float GRID_OFFSET_X = 50f;
    private const float GRID_OFFSET_Y = 80f;
    private const float CELL_SIZE = 25f;
    private const float TOOLBAR_HEIGHT = 30f;

    private void DrawGrid()
    {
        if (currentLevel == null) return;

        // 计算网格总尺寸
        float gridWidth = currentLevel.gridSize.x * CELL_SIZE;
        float gridHeight = currentLevel.gridSize.y * CELL_SIZE;

        // 绘制背景
        Handles.BeginGUI();
        Handles.DrawSolidRectangleWithOutline(
            new Rect(GRID_OFFSET_X, GRID_OFFSET_Y, gridWidth, gridHeight),
            new Color(0.15f, 0.15f, 0.15f, 1f),
            new Color(0.3f, 0.3f, 0.3f, 1f)
        );

        // 添加滚动区域边界
        GUILayout.BeginVertical(GUILayout.Width(position.width - LIST_WIDTH - PRESET_WIDTH));
        editorScrollPos = GUILayout.BeginScrollView(editorScrollPos,
            GUILayout.Width(position.width - LIST_WIDTH - PRESET_WIDTH),
            GUILayout.Height(position.height - TOOLBAR_HEIGHT));

        // 绘制网格线
        for (int x = 0; x <= currentLevel.gridSize.x; x++)
        {
            Vector3 start = new Vector3(
                GRID_OFFSET_X + x * CELL_SIZE,
                GRID_OFFSET_Y
            );
            Vector3 end = new Vector3(
                GRID_OFFSET_X + x * CELL_SIZE,
                GRID_OFFSET_Y + gridHeight
            );
            Handles.DrawLine(start, end);
        }

        for (int y = 0; y <= currentLevel.gridSize.y; y++)
        {
            Vector3 start = new Vector3(
                GRID_OFFSET_X,
                GRID_OFFSET_Y + y * CELL_SIZE
            );
            Vector3 end = new Vector3(
                GRID_OFFSET_X + gridWidth,
                GRID_OFFSET_Y + y * CELL_SIZE
            );
            Handles.DrawLine(start, end);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        Handles.EndGUI();

        // 绘制方块
        foreach (var block in currentLevel.blocks)
        {
            Rect rect = new Rect(
                GRID_OFFSET_X + block.position.x * CELL_SIZE + 2,
                GRID_OFFSET_Y + block.position.y * CELL_SIZE + 2,
                CELL_SIZE - 4,
                CELL_SIZE - 4
            );
            EditorGUI.DrawRect(rect, block.color);
        }
    }

    #endregion

    // 预设数据
    [System.Serializable]
    private class PresetBlock
    {
        public Color color = Color.white;
        public BlockType type = BlockType.Normal;
        public Texture2D preview;
    }

    private List<PresetBlock> presetBlocks = new List<PresetBlock>();
    private PresetBlock draggingPreset;
    private Vector2 dragOffset;

    // 编辑器状态
    private Vector2Int? lastGridPos;

    #region 预设面板
    private void InitializePresets()
    {
        presetBlocks = new List<PresetBlock>
        {
            CreatePreset(Color.red, BlockType.Obstacle),
            CreatePreset(Color.green),
            CreatePreset(Color.blue),
            CreatePreset(Color.yellow)
        };
    }

    private PresetBlock CreatePreset(Color color, BlockType type = BlockType.Normal)
    {
        var preset = new PresetBlock
        {
            color = color,
            type = type,
            preview = CreatePreviewTexture(color)
        };
        return preset;
    }

    private Texture2D CreatePreviewTexture(Color color)
    {
        var tex = new Texture2D(64, 64);
        var pixels = Enumerable.Repeat(color, 64 * 64).ToArray();
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private void DrawPresetPanel()
    {
        GUILayout.BeginVertical(GUILayout.Width(PRESET_WIDTH));
        GUILayout.Label("预设方块", EditorStyles.boldLabel);

        foreach (var preset in presetBlocks)
        {
            GUILayout.BeginHorizontal();
            var rect = GUILayoutUtility.GetRect(50, 50);
            GUI.DrawTexture(rect, preset.preview);

            // 拖拽开始区域
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                draggingPreset = preset;
                dragOffset = Event.current.mousePosition - rect.position;
                GUI.changed = true;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
    #endregion

    #region 拖拽逻辑
    private void HandleDragAndDrop()
    {
        if (draggingPreset == null) return;

        Event e = Event.current;

        switch (e.type)
        {
            case EventType.Repaint:
                // 仅绘制预览，不处理事件
                var mousePos = e.mousePosition;
                var previewRect = new Rect(mousePos.x - 25, mousePos.y - 25, 50, 50);
                GUI.DrawTexture(previewRect, draggingPreset.preview);
                break;

            case EventType.MouseUp:
                HandleDrop(e.mousePosition);
                draggingPreset = null;
                e.Use(); // 只在处理输入事件时调用Use
                break;

            case EventType.MouseDrag:
                // 强制重绘以更新预览位置
                Repaint();
                break;
        }
    }

    private void HandleDrop(Vector2 mousePos)
    {
        Rect editorRect = new Rect(
            LIST_WIDTH,
            TOOLBAR_HEIGHT,
            position.width - LIST_WIDTH - PRESET_WIDTH,
            position.height - TOOLBAR_HEIGHT
        );

        if (editorRect.Contains(mousePos))
        {
            Vector2 gridSpacePos = mousePos 
                - editorRect.position 
                - new Vector2(GRID_OFFSET_X, GRID_OFFSET_Y) 
                + editorScrollPos;

            Vector2Int gridPos = new Vector2Int(
                Mathf.FloorToInt(gridSpacePos.x / CELL_SIZE),
                Mathf.FloorToInt(gridSpacePos.y / CELL_SIZE));

            if (IsPositionValid(gridPos))
            {
                Undo.RecordObject(currentLevel, "Place Block");
                AddBlock(gridPos, draggingPreset.color, draggingPreset.type);
                EditorUtility.SetDirty(currentLevel);
            }
        }
    }
    #endregion

    #region 网格交互

    private void HandleGridInteraction()
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            var mousePos = e.mousePosition;
            Vector2Int gridPos = ConvertToGridPos(mousePos);

            if (IsPositionValid(gridPos))
            {
                Undo.RecordObject(currentLevel, "Remove Block");
                RemoveBlock(gridPos);
                EditorUtility.SetDirty(currentLevel);
                Repaint();
                e.Use(); // 只在处理右键点击时调用
            }
        }
    }

    private Vector2Int ConvertToGridPos(Vector2 mousePos)
    {
        Rect editorRect = new Rect(
            LIST_WIDTH,
            TOOLBAR_HEIGHT,
            position.width - LIST_WIDTH - PRESET_WIDTH,
            position.height - TOOLBAR_HEIGHT
        );

        Vector2 gridSpacePos = mousePos 
            - editorRect.position 
            - new Vector2(GRID_OFFSET_X, GRID_OFFSET_Y) 
            + editorScrollPos;

        return new Vector2Int(
            Mathf.FloorToInt(gridSpacePos.x / CELL_SIZE),
            Mathf.FloorToInt(gridSpacePos.y / CELL_SIZE));
    }

    private bool IsPositionValid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < currentLevel.gridSize.x &&
               gridPos.y >= 0 && gridPos.y < currentLevel.gridSize.y;
    }
    #endregion

    #region 方块操作
    private void AddBlock(Vector2Int pos, Color color, BlockType type)
    {
        if (currentLevel.blocks.Any(b => b.position == pos)) return;

        RecordUndoState("Edit Block");  // 在修改前记录状态

        var existing = currentLevel.blocks.Find(b =>
            b.position == pos);

        if (existing != null)
        {
            currentLevel.blocks.Remove(existing);
        }

        currentLevel.blocks.Add(new BlockEntry
        {
            position = pos,
            color = color,
            type = type
        });

        EditorUtility.SetDirty(currentLevel);
        Repaint();
    }

    private void RemoveBlock(Vector2Int pos)
    {
        currentLevel.blocks.RemoveAll(b => b.position == pos);
    }
    #endregion
}
