using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(UIControlManager))]
public class UIControlManagerInspector : Editor
{
    private UIControlManager _target;
    
    private bool _showBrushSettings = true;
    private bool _showBrushBasic = true;
    private bool _showBrushAdvanced = false;
    
    private bool _showCameraSettings = true;
    private bool _showCameraBasic = true;
    private bool _showCameraAdvanced = false;
    
    private bool _showEditorSettings = true;
    
    private bool _showTerrainGenerator = true;
    private bool _showTerrainSize = true;
    private bool _showTerrainType = true;
    private bool _showTerrainNoise = true;
    
    private bool _showTerrainOperations = true;
    private bool _showDataIO = true;
    
    private bool _showExportDialog = false;
    private bool _showImportDialog = false;
    private bool _showConfigCreatedDialog = false;
    private bool _showCreateTerrainDialog = false;
    private bool _showClearTerrainDialog = false;

    void OnEnable()
    {
        _target = (UIControlManager)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawHeader();
        
        if (_target.config == null)
        {
            DrawConfigMissingWarning();
            serializedObject.ApplyModifiedProperties();
            return;
        }

        DrawBrushSettings();
        DrawEditorSettings();
        DrawCameraSettings();
        DrawTerrainGenerator();
        DrawTerrainOperations();
        DrawDataIO();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed && _target.enableRealTimeSync)
        {
            _target.ApplyAllSettings();
        }

        // 在布局外处理对话框
        if (_showExportDialog)
        {
            _showExportDialog = false;
            string filename = EditorUtility.SaveFilePanel("导出地形数据", 
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop), 
                "terrain", "obj");
            if (!string.IsNullOrEmpty(filename))
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(filename);
                string extension = System.IO.Path.GetExtension(filename);
                Debug.Log($"[UIControlManagerInspector] Exporting terrain: {filename}, name: {name}, extension: {extension}");
                _target.ExportTerrainToPath(filename);
            }
        }

        if (_showImportDialog)
        {
            _showImportDialog = false;
            string path = EditorUtility.OpenFilePanel("选择地形数据文件", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                _target.ImportTerrain(filename);
                EditorUtility.DisplayDialog("导入成功", "地形数据已成功导入", "确定");
            }
        }

        if (_showConfigCreatedDialog)
        {
            _showConfigCreatedDialog = false;
            EditorUtility.DisplayDialog("成功", "TerrainConfig 配置文件已创建！", "确定");
        }

        if (_showCreateTerrainDialog)
        {
            _showCreateTerrainDialog = false;
            var generator = _target.config.generator;
            if (EditorUtility.DisplayDialog("确认创建地形", 
                $"将创建尺寸为 {generator.width}×{generator.depth} 的地形，是否继续？", 
                "确定", "取消"))
            {
                _target.GenerateTerrain();
                MarkDirty();
            }
        }

        if (_showClearTerrainDialog)
        {
            _showClearTerrainDialog = false;
            if (EditorUtility.DisplayDialog("确认清空地形", "确定要将地形恢复为平面吗？", "确定", "取消"))
            {
                _target.ClearTerrain();
                MarkDirty();
            }
        }
    }
    
    void DrawConfigMissingWarning()
    {
        EditorGUILayout.HelpBox("请先创建并分配 TerrainConfig 配置文件！", MessageType.Warning);
        if (GUILayout.Button("创建 TerrainConfig"))
        {
            TerrainConfig config = ScriptableObject.CreateInstance<TerrainConfig>();
            string directory = "Assets/Scripts/Domain/Terrain";
            if (!AssetDatabase.IsValidFolder(directory))
            {
                directory = "Assets";
            }
            string path = AssetDatabase.GenerateUniqueAssetPath($"{directory}/TerrainConfig.asset");
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            _target.config = config;
            EditorUtility.SetDirty(_target);
            EditorUtility.SetDirty(config);
            
            serializedObject.Update();
            SceneView.RepaintAll();
            _showConfigCreatedDialog = true;
        }
    }

    void DrawHeader()
    {
        
        EditorGUILayout.Space();
    }

    void DrawBrushSettings()
    {
        _showBrushSettings = EditorGUILayout.Foldout(_showBrushSettings, "🖌️ 笔刷设置", true, EditorStyles.foldoutHeader);
        if (_showBrushSettings)
        {
            EditorGUILayout.BeginVertical("Box");
            
            var config = _target.config;
            var brush = config.brush;
            var editor = config.editor;
            TerrainEditor.EditMode currentMode = editor.currentMode;

            _showBrushBasic = EditorGUILayout.Foldout(_showBrushBasic, "基础参数", true);
            if (_showBrushBasic)
            {
                EditorGUILayout.BeginVertical("GroupBox");
                
                float newRadius = EditorGUILayout.Slider(
                    new GUIContent("笔刷半径", "控制地形编辑时笔刷的影响范围"), 
                    brush.radius, brush.minRadius, brush.maxRadius);
                if (newRadius != brush.radius)
                {
                    brush.radius = newRadius;
                    MarkDirty();
                }
                
                string strengthLabel = GetStrengthLabel(currentMode);
                string strengthTooltip = GetStrengthTooltip(currentMode);
                
                switch (currentMode)
                {
                    case TerrainEditor.EditMode.RaiseLower:
                        float newRaiseLowerStrength = EditorGUILayout.Slider(
                            new GUIContent(strengthLabel, strengthTooltip), 
                            brush.raiseLowerStrength, brush.minStrength, brush.maxStrength);
                        if (newRaiseLowerStrength != brush.raiseLowerStrength)
                        {
                            brush.raiseLowerStrength = newRaiseLowerStrength;
                            MarkDirty();
                        }
                        break;
                    case TerrainEditor.EditMode.Roughen:
                        float newRoughenAmount = EditorGUILayout.Slider(
                            new GUIContent(strengthLabel, strengthTooltip), 
                            brush.roughenAmount, 0f, 1f);
                        if (newRoughenAmount != brush.roughenAmount)
                        {
                            brush.roughenAmount = newRoughenAmount;
                            MarkDirty();
                        }
                        break;
                    case TerrainEditor.EditMode.Flatten:
                        float newFlattenStrength = EditorGUILayout.Slider(
                            new GUIContent(strengthLabel, strengthTooltip), 
                            brush.flattenStrength, brush.minStrength, brush.maxStrength);
                        if (newFlattenStrength != brush.flattenStrength)
                        {
                            brush.flattenStrength = newFlattenStrength;
                            MarkDirty();
                        }
                        break;
                    case TerrainEditor.EditMode.Smooth:
                        float newSmoothStrength = EditorGUILayout.Slider(
                            new GUIContent(strengthLabel, strengthTooltip), 
                            brush.smoothStrength, brush.minStrength, brush.maxStrength);
                        if (newSmoothStrength != brush.smoothStrength)
                        {
                            brush.smoothStrength = newSmoothStrength;
                            MarkDirty();
                        }
                        break;
                }
                
                EditorGUILayout.EndVertical();
            }

            _showBrushAdvanced = EditorGUILayout.Foldout(_showBrushAdvanced, "模式参数", true);
            if (_showBrushAdvanced)
            {
                EditorGUILayout.BeginVertical("GroupBox");
                
                using (new EditorGUI.DisabledScope(currentMode != TerrainEditor.EditMode.RaiseLower))
                {
                    float newRaiseLowerStrength = EditorGUILayout.Slider(
                        new GUIContent("升高/降低程度", "控制地形升高或降低的幅度"), 
                        brush.raiseLowerStrength, brush.minStrength, brush.maxStrength);
                    if (newRaiseLowerStrength != brush.raiseLowerStrength)
                    {
                        brush.raiseLowerStrength = newRaiseLowerStrength;
                        MarkDirty();
                    }
                }
                
                using (new EditorGUI.DisabledScope(currentMode != TerrainEditor.EditMode.Roughen))
                {
                    float newRoughenAmount = EditorGUILayout.Slider(
                        new GUIContent("粗糙化程度", "控制地形表面的粗糙程度"), 
                        brush.roughenAmount, 0f, 1f);
                    if (newRoughenAmount != brush.roughenAmount)
                    {
                        brush.roughenAmount = newRoughenAmount;
                        MarkDirty();
                    }
                }
                
                using (new EditorGUI.DisabledScope(currentMode != TerrainEditor.EditMode.Flatten))
                {
                    float newFlattenStrength = EditorGUILayout.Slider(
                        new GUIContent("平整程度", "控制地形平整的强度"), 
                        brush.flattenStrength, brush.minStrength, brush.maxStrength);
                    if (newFlattenStrength != brush.flattenStrength)
                    {
                        brush.flattenStrength = newFlattenStrength;
                        MarkDirty();
                    }
                }
                
                using (new EditorGUI.DisabledScope(currentMode != TerrainEditor.EditMode.Smooth))
                {
                    float newSmoothStrength = EditorGUILayout.Slider(
                        new GUIContent("平滑程度", "控制地形平滑的强度"), 
                        brush.smoothStrength, brush.minStrength, brush.maxStrength);
                    if (newSmoothStrength != brush.smoothStrength)
                    {
                        brush.smoothStrength = newSmoothStrength;
                        MarkDirty();
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("参数范围限制", EditorStyles.boldLabel);
                
                float newMinRadius = EditorGUILayout.FloatField(new GUIContent("最小半径"), brush.minRadius);
                if (newMinRadius != brush.minRadius)
                {
                    brush.minRadius = newMinRadius;
                    MarkDirty();
                }
                
                float newMaxRadius = EditorGUILayout.FloatField(new GUIContent("最大半径"), brush.maxRadius);
                if (newMaxRadius != brush.maxRadius)
                {
                    brush.maxRadius = newMaxRadius;
                    MarkDirty();
                }
                
                float newMinStrength = EditorGUILayout.FloatField(new GUIContent("最小强度"), brush.minStrength);
                if (newMinStrength != brush.minStrength)
                {
                    brush.minStrength = newMinStrength;
                    MarkDirty();
                }
                
                float newMaxStrength = EditorGUILayout.FloatField(new GUIContent("最大强度"), brush.maxStrength);
                if (newMaxStrength != brush.maxStrength)
                {
                    brush.maxStrength = newMaxStrength;
                    MarkDirty();
                }
                
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    string GetStrengthLabel(TerrainEditor.EditMode mode)
    {
        switch (mode)
        {
            case TerrainEditor.EditMode.RaiseLower:
                return "升高/降低程度";
            case TerrainEditor.EditMode.Roughen:
                return "粗糙化程度";
            case TerrainEditor.EditMode.Flatten:
                return "平整程度";
            case TerrainEditor.EditMode.Smooth:
                return "平滑程度";
            default:
                return "笔刷强度";
        }
    }

    string GetStrengthTooltip(TerrainEditor.EditMode mode)
    {
        switch (mode)
        {
            case TerrainEditor.EditMode.RaiseLower:
                return "控制地形升高或降低的幅度";
            case TerrainEditor.EditMode.Roughen:
                return "控制地形表面的粗糙程度";
            case TerrainEditor.EditMode.Flatten:
                return "控制地形平整的强度";
            case TerrainEditor.EditMode.Smooth:
                return "控制地形平滑的强度";
            default:
                return "控制每次编辑操作对地形的影响程度";
        }
    }

    void DrawCameraSettings()
    {
        _showCameraSettings = EditorGUILayout.Foldout(_showCameraSettings, "🎥 相机控制", true, EditorStyles.foldoutHeader);
        if (_showCameraSettings)
        {
            EditorGUILayout.BeginVertical("Box");
            
            var config = _target.config;
            var camera = config.camera;

            _showCameraBasic = EditorGUILayout.Foldout(_showCameraBasic, "速度参数", true);
            if (_showCameraBasic)
            {
                EditorGUILayout.BeginVertical("GroupBox");
                
                float newMoveSpeed = EditorGUILayout.Slider(
                    new GUIContent("移动速度", "相机水平移动的速度"), 
                    camera.moveSpeed, camera.minMoveSpeed, camera.maxMoveSpeed);
                if (newMoveSpeed != camera.moveSpeed)
                {
                    camera.moveSpeed = newMoveSpeed;
                    MarkDirty();
                }
                
                float newRotationSpeed = EditorGUILayout.Slider(
                    new GUIContent("旋转速度", "相机旋转的灵敏度"), 
                    camera.rotationSpeed, camera.minRotationSpeed, camera.maxRotationSpeed);
                if (newRotationSpeed != camera.rotationSpeed)
                {
                    camera.rotationSpeed = newRotationSpeed;
                    MarkDirty();
                }
                
                float newVerticalSpeed = EditorGUILayout.Slider(
                    new GUIContent("垂直速度", "相机上下移动的速度"), 
                    camera.verticalSpeed, camera.minVerticalSpeed, camera.maxVerticalSpeed);
                if (newVerticalSpeed != camera.verticalSpeed)
                {
                    camera.verticalSpeed = newVerticalSpeed;
                    MarkDirty();
                }
                
                EditorGUILayout.EndVertical();
            }

            _showCameraAdvanced = EditorGUILayout.Foldout(_showCameraAdvanced, "高级设置", true);
            if (_showCameraAdvanced)
            {
                EditorGUILayout.BeginVertical("GroupBox");
                
                EditorGUILayout.LabelField("速度范围限制", EditorStyles.boldLabel);
                
                float newMinMoveSpeed = EditorGUILayout.FloatField(new GUIContent("最小移动速度"), camera.minMoveSpeed);
                if (newMinMoveSpeed != camera.minMoveSpeed)
                {
                    camera.minMoveSpeed = newMinMoveSpeed;
                    MarkDirty();
                }
                
                float newMaxMoveSpeed = EditorGUILayout.FloatField(new GUIContent("最大移动速度"), camera.maxMoveSpeed);
                if (newMaxMoveSpeed != camera.maxMoveSpeed)
                {
                    camera.maxMoveSpeed = newMaxMoveSpeed;
                    MarkDirty();
                }
                
                float newMinRotationSpeed = EditorGUILayout.FloatField(new GUIContent("最小旋转速度"), camera.minRotationSpeed);
                if (newMinRotationSpeed != camera.minRotationSpeed)
                {
                    camera.minRotationSpeed = newMinRotationSpeed;
                    MarkDirty();
                }
                
                float newMaxRotationSpeed = EditorGUILayout.FloatField(new GUIContent("最大旋转速度"), camera.maxRotationSpeed);
                if (newMaxRotationSpeed != camera.maxRotationSpeed)
                {
                    camera.maxRotationSpeed = newMaxRotationSpeed;
                    MarkDirty();
                }
                
                float newMinVerticalSpeed = EditorGUILayout.FloatField(new GUIContent("最小垂直速度"), camera.minVerticalSpeed);
                if (newMinVerticalSpeed != camera.minVerticalSpeed)
                {
                    camera.minVerticalSpeed = newMinVerticalSpeed;
                    MarkDirty();
                }
                
                float newMaxVerticalSpeed = EditorGUILayout.FloatField(new GUIContent("最大垂直速度"), camera.maxVerticalSpeed);
                if (newMaxVerticalSpeed != camera.maxVerticalSpeed)
                {
                    camera.maxVerticalSpeed = newMaxVerticalSpeed;
                    MarkDirty();
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("平滑设置", EditorStyles.boldLabel);
                
                bool newEnableSmoothing = EditorGUILayout.Toggle(
                    new GUIContent("启用平滑", "启用相机移动和旋转的平滑过渡效果"), 
                    camera.enableSmoothing);
                if (newEnableSmoothing != camera.enableSmoothing)
                {
                    camera.enableSmoothing = newEnableSmoothing;
                    MarkDirty();
                }
                
                if (camera.enableSmoothing)
                {
                    float newSmoothTime = EditorGUILayout.Slider(
                        new GUIContent("平滑时间", "值越小响应越快，值越大越平滑"), 
                        camera.smoothTime, 0.01f, 0.5f);
                    if (newSmoothTime != camera.smoothTime)
                    {
                        camera.smoothTime = newSmoothTime;
                        MarkDirty();
                    }
                }
                
                bool newEnableSpeedAdjustment = EditorGUILayout.Toggle(
                    new GUIContent("高度自适应", "根据相机高度自动调整移动速度"), 
                    camera.enableSpeedAdjustment);
                if (newEnableSpeedAdjustment != camera.enableSpeedAdjustment)
                {
                    camera.enableSpeedAdjustment = newEnableSpeedAdjustment;
                    MarkDirty();
                }
                
                if (camera.enableSpeedAdjustment)
                {
                    float newDistanceCheckInterval = EditorGUILayout.Slider(
                        new GUIContent("检测间隔", "距离地面检测的更新频率"), 
                        camera.distanceCheckInterval, 0.1f, 2f);
                    if (newDistanceCheckInterval != camera.distanceCheckInterval)
                    {
                        camera.distanceCheckInterval = newDistanceCheckInterval;
                        MarkDirty();
                    }
                }
                
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    void DrawEditorSettings()
    {
        _showEditorSettings = EditorGUILayout.Foldout(_showEditorSettings, "⚙️ 编辑器设置", true, EditorStyles.foldoutHeader);
        if (_showEditorSettings)
        {
            EditorGUILayout.BeginVertical("Box");
            
            EditorGUILayout.LabelField("当前编辑模式", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            {
                DrawModeButton(TerrainEditor.EditMode.RaiseLower, "升高/降低");
                DrawModeButton(TerrainEditor.EditMode.Roughen, "粗糙化");
                DrawModeButton(TerrainEditor.EditMode.Flatten, "平整");
                DrawModeButton(TerrainEditor.EditMode.Smooth, "平滑");
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    void DrawTerrainGenerator()
    {
        _showTerrainGenerator = EditorGUILayout.Foldout(_showTerrainGenerator, "🌍 地形生成器", true, EditorStyles.foldoutHeader);
        if (_showTerrainGenerator)
        {
            EditorGUILayout.BeginVertical("Box");
            
            var config = _target.config;
            var generator = config.generator;

            _showTerrainSize = EditorGUILayout.Foldout(_showTerrainSize, "地形尺寸", true);
            if (_showTerrainSize)
            {
                EditorGUILayout.BeginVertical("GroupBox");
                
                int newLength = Mathf.RoundToInt(EditorGUILayout.Slider(
                    new GUIContent("长度", "地形长度（顶点数）"), 
                    (float)generator.width, 10f, 256f));
                if (newLength != generator.width)
                {
                    generator.width = newLength;
                    MarkDirty();
                }
                
                int newWidth = Mathf.RoundToInt(EditorGUILayout.Slider(
                    new GUIContent("宽度", "地形宽度（顶点数）"), 
                    (float)generator.depth, 10f, 256f));
                if (newWidth != generator.depth)
                {
                    generator.depth = newWidth;
                    MarkDirty();
                }
                
                float newCellSize = EditorGUILayout.Slider(
                    new GUIContent("单元格大小", "每个网格单元的物理大小"), 
                    generator.cellSize, 0.1f, 5f);
                if (newCellSize != generator.cellSize)
                {
                    generator.cellSize = newCellSize;
                    MarkDirty();
                }
                
                float newHeightRange = EditorGUILayout.Slider(
                    new GUIContent("最大高度", "地形的最大高度值"), 
                    generator.heightRange, 1f, 50f);
                if (newHeightRange != generator.heightRange)
                {
                    generator.heightRange = newHeightRange;
                    MarkDirty();
                }
                
                EditorGUILayout.EndVertical();
            }

            _showTerrainType = EditorGUILayout.Foldout(_showTerrainType, "地形类型", true);
            if (_showTerrainType)
            {
                EditorGUILayout.BeginVertical("GroupBox");
                
                TerrainConfig.TerrainType newTerrainType = (TerrainConfig.TerrainType)EditorGUILayout.EnumPopup(
                    new GUIContent("预设类型", "选择预设的地形样式"), 
                    generator.terrainType);
                if (newTerrainType != generator.terrainType)
                {
                    generator.terrainType = newTerrainType;
                    MarkDirty();
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "地形类型说明:\n" +
                    "• Plains(平原): 生成绝对平坦的基准面\n" +
                    "• Hills(丘陵): 缓坡起伏、坡度平滑自然\n" +
                    "• Mountains(山脉): 高度不一、陡峭复杂\n" +
                    "• Valley(山谷): 明显凹陷、碗状地形\n" +
                    "• Plateau(高原): 部分区域抬升、表面平坦\n" +
                    "• Canyon(峡谷): 狭窄山谷、V形剖面",
                    MessageType.Info);

                // 地形类型专属参数
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("专属参数", EditorStyles.boldLabel);
                
                switch (generator.terrainType)
                {
                    case TerrainConfig.TerrainType.Plains:
                        DrawPlainsParams(generator);
                        break;
                    case TerrainConfig.TerrainType.Hills:
                        DrawHillsParams(generator);
                        break;
                    case TerrainConfig.TerrainType.Mountains:
                        DrawMountainsParams(generator);
                        break;
                    case TerrainConfig.TerrainType.Valley:
                        DrawValleyParams(generator);
                        break;
                    case TerrainConfig.TerrainType.Plateau:
                        DrawPlateauParams(generator);
                        break;
                    case TerrainConfig.TerrainType.Canyon:
                        DrawCanyonParams(generator);
                        break;
                }
                
                EditorGUILayout.EndVertical();
            }

            // 噪声参数 - 仅在非平原预设下显示
            bool noiseParamsEnabled = generator.terrainType != TerrainConfig.TerrainType.Plains;
            bool showAdvancedNoiseParams = generator.terrainType != TerrainConfig.TerrainType.Plains && 
                                           generator.terrainType != TerrainConfig.TerrainType.Hills;
            
            if (noiseParamsEnabled)
            {
                _showTerrainNoise = EditorGUILayout.Foldout(_showTerrainNoise, "噪声参数", true);
                if (_showTerrainNoise)
                {
                    EditorGUILayout.BeginVertical("GroupBox");
                    
                    float newNoiseFrequency = EditorGUILayout.Slider(
                        new GUIContent("噪声频率", "值越小地形越平滑，值越大细节越丰富"), 
                        generator.noiseFrequency, 0.01f, 0.5f);
                    if (newNoiseFrequency != generator.noiseFrequency)
                    {
                        generator.noiseFrequency = newNoiseFrequency;
                        MarkDirty();
                    }
                    
                    int newSeed = EditorGUILayout.IntField(
                        new GUIContent("随机种子", "地形生成的随机种子，不同种子生成不同地形"), 
                        generator.seed);
                    if (newSeed != generator.seed)
                    {
                        generator.seed = newSeed;
                        MarkDirty();
                    }
                    
                    // Hills预设不使用以下参数，仅在其他预设下显示
                    if (showAdvancedNoiseParams)
                    {
                        float newNoiseAmplitude = EditorGUILayout.Slider(
                            new GUIContent("噪声振幅", "控制地形起伏幅度"), 
                            generator.noiseAmplitude, 0.1f, 2f);
                        if (newNoiseAmplitude != generator.noiseAmplitude)
                        {
                            generator.noiseAmplitude = newNoiseAmplitude;
                            MarkDirty();
                        }
                        
                        int newNoiseOctaves = EditorGUILayout.IntSlider(
                            new GUIContent("噪声层数", "多层噪声叠加增加地形细节丰富度"), 
                            generator.noiseOctaves, 1, 8);
                        if (newNoiseOctaves != generator.noiseOctaves)
                        {
                            generator.noiseOctaves = newNoiseOctaves;
                            MarkDirty();
                        }
                        
                        float newNoisePersistence = EditorGUILayout.Slider(
                            new GUIContent("噪声衰减", "每层噪声振幅的衰减系数，影响细节层次"), 
                            generator.noisePersistence, 0.01f, 1f);
                        if (newNoisePersistence != generator.noisePersistence)
                        {
                            generator.noisePersistence = newNoisePersistence;
                            MarkDirty();
                        }
                    }
                    
                    EditorGUILayout.EndVertical();
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    void DrawTerrainOperations()
    {
        _showTerrainOperations = EditorGUILayout.Foldout(_showTerrainOperations, "⛏️ 地形操作", true, EditorStyles.foldoutHeader);
        if (_showTerrainOperations)
        {
            EditorGUILayout.BeginVertical("Box");
            
            var generator = _target.config.generator;
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("创建地形", GUILayout.Height(30)))
                {
                    _showCreateTerrainDialog = true;
                }
                
                if (GUILayout.Button("重新生成", GUILayout.Height(30)))
                {
                    _target.RegenerateTerrain();
                    MarkDirty();
                }
                
                if (GUILayout.Button("清空地形", GUILayout.Height(30)))
                {
                    _showClearTerrainDialog = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    void DrawDataIO()
    {
        _showDataIO = EditorGUILayout.Foldout(_showDataIO, "📤 数据导入导出", true, EditorStyles.foldoutHeader);
        if (_showDataIO)
        {
            EditorGUILayout.BeginVertical("Box");
            
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("导出到桌面", GUILayout.Height(28)))
                {
                    _showExportDialog = true;
                }
                
                if (GUILayout.Button("导入地形数据", GUILayout.Height(28)))
                {
                    _showImportDialog = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }

    void DrawModeButton(TerrainEditor.EditMode mode, string label)
    {
        bool isActive = _target.config.editor.currentMode == mode;
        Color originalColor = GUI.backgroundColor;
        
        if (isActive)
        {
            GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
        }
        
        if (GUILayout.Button(label, GUILayout.Height(28)))
        {
            _target.SetEditMode(mode);
            MarkDirty();
        }
        
        GUI.backgroundColor = originalColor;
    }

    void DrawSliderWithInput(SerializedProperty property, GUIContent label, float min, float max, float step)
    {
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            
            float currentValue = property.floatValue;
            float sliderValue = GUILayout.HorizontalSlider(currentValue, min, max, GUILayout.MinWidth(150));
            
            if (Mathf.Abs(sliderValue - currentValue) > step * 0.1f)
            {
                property.floatValue = Mathf.Round(sliderValue / step) * step;
            }
            
            string inputValue = EditorGUILayout.DelayedTextField(property.floatValue.ToString("F2"), GUILayout.Width(60));
            if (float.TryParse(inputValue, out float parsedValue))
            {
                property.floatValue = Mathf.Clamp(parsedValue, min, max);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    void MarkDirty()
    {
        EditorUtility.SetDirty(_target);
        serializedObject.Update();
    }

    #region Terrain Type Parameter Drawers

    void DrawPlainsParams(TerrainConfig.GeneratorConfig generator)
    {
        EditorGUILayout.BeginVertical("Box");
        
        float newBaseHeight = EditorGUILayout.Slider(
            new GUIContent("基准高度", "平原的基础高度值"), 
            generator.plainsBaseHeight, 0f, 1f);
        if (newBaseHeight != generator.plainsBaseHeight)
        {
            generator.plainsBaseHeight = newBaseHeight;
            MarkDirty();
        }
        
        EditorGUILayout.EndVertical();
    }

    void DrawHillsParams(TerrainConfig.GeneratorConfig generator)
    {
        EditorGUILayout.BeginVertical("Box");
        
        float newAmplitude = EditorGUILayout.Slider(
            new GUIContent("起伏幅度", "丘陵的高度变化范围"), 
            generator.hillsAmplitude, 0.1f, 1f);
        if (newAmplitude != generator.hillsAmplitude)
        {
            generator.hillsAmplitude = newAmplitude;
            MarkDirty();
        }
        
        float newSmoothness = EditorGUILayout.Slider(
            new GUIContent("平滑程度", "控制坡度变化的平滑度，值越大越平滑"), 
            generator.hillsSmoothness, 1f, 5f);
        if (newSmoothness != generator.hillsSmoothness)
        {
            generator.hillsSmoothness = newSmoothness;
            MarkDirty();
        }
        
        EditorGUILayout.EndVertical();
    }

    void DrawMountainsParams(TerrainConfig.GeneratorConfig generator)
    {
        EditorGUILayout.BeginVertical("Box");
        
        float newPeakHeight = EditorGUILayout.Slider(
            new GUIContent("主峰高度", "主峰区域的最高高度，值越大山峰越高"), 
            generator.mountainsPeakHeight, 0.5f, 1f);
        if (newPeakHeight != generator.mountainsPeakHeight)
        {
            generator.mountainsPeakHeight = newPeakHeight;
            MarkDirty();
        }
        
        float newSharpness = EditorGUILayout.Slider(
            new GUIContent("陡峭程度", "控制山坡陡峭度，值越大山坡越陡峭"), 
            generator.mountainsSharpness, 0.5f, 2f);
        if (newSharpness != generator.mountainsSharpness)
        {
            generator.mountainsSharpness = newSharpness;
            MarkDirty();
        }
        
        float newRoughness = EditorGUILayout.Slider(
            new GUIContent("粗糙度", "控制山脉表面细节，值越大表面越粗糙"), 
            generator.mountainsRoughness, 1f, 4f);
        if (newRoughness != generator.mountainsRoughness)
        {
            generator.mountainsRoughness = newRoughness;
            MarkDirty();
        }
        
        EditorGUILayout.EndVertical();
    }

    void DrawValleyParams(TerrainConfig.GeneratorConfig generator)
    {
        EditorGUILayout.BeginVertical("Box");
        
        float newDepth = EditorGUILayout.Slider(
            new GUIContent("凹陷深度", "山谷底部的凹陷程度"), 
            generator.valleyDepth, 0.3f, 0.9f);
        if (newDepth != generator.valleyDepth)
        {
            generator.valleyDepth = newDepth;
            MarkDirty();
        }
        
        float newSmoothness = EditorGUILayout.Slider(
            new GUIContent("平滑过渡", "山谷边缘的平滑过渡程度"), 
            generator.valleySmoothness, 0.1f, 0.5f);
        if (newSmoothness != generator.valleySmoothness)
        {
            generator.valleySmoothness = newSmoothness;
            MarkDirty();
        }
        
        EditorGUILayout.EndVertical();
    }

    void DrawPlateauParams(TerrainConfig.GeneratorConfig generator)
    {
        EditorGUILayout.BeginVertical("Box");
        
        float newHeight = EditorGUILayout.Slider(
            new GUIContent("抬升高度", "高原区域的高度"), 
            generator.plateauHeight, 0.3f, 0.9f);
        if (newHeight != generator.plateauHeight)
        {
            generator.plateauHeight = newHeight;
            MarkDirty();
        }
        
        float newAreaRatio = EditorGUILayout.Slider(
            new GUIContent("区域比例", "高原区域占整个地形的比例"), 
            generator.plateauAreaRatio, 0.2f, 0.8f);
        if (newAreaRatio != generator.plateauAreaRatio)
        {
            generator.plateauAreaRatio = newAreaRatio;
            MarkDirty();
        }
        
        float newEdgeSmooth = EditorGUILayout.Slider(
            new GUIContent("边缘过渡", "高原边缘的平滑过渡宽度"), 
            generator.plateauEdgeSmooth, 0.05f, 0.3f);
        if (newEdgeSmooth != generator.plateauEdgeSmooth)
        {
            generator.plateauEdgeSmooth = newEdgeSmooth;
            MarkDirty();
        }
        
        EditorGUILayout.EndVertical();
    }

    void DrawCanyonParams(TerrainConfig.GeneratorConfig generator)
    {
        EditorGUILayout.BeginVertical("Box");
        
        float newDepth = EditorGUILayout.Slider(
            new GUIContent("峡谷深度", "峡谷的凹陷深度"), 
            generator.canyonDepth, 0.4f, 0.9f);
        if (newDepth != generator.canyonDepth)
        {
            generator.canyonDepth = newDepth;
            MarkDirty();
        }
        
        float newWidth = EditorGUILayout.Slider(
            new GUIContent("峡谷宽度", "峡谷的狭窄程度，值越小越窄"), 
            generator.canyonWidth, 0.05f, 0.3f);
        if (newWidth != generator.canyonWidth)
        {
            generator.canyonWidth = newWidth;
            MarkDirty();
        }
        
        float newCurvature = EditorGUILayout.Slider(
            new GUIContent("弯曲程度", "峡谷中心线的弯曲程度"), 
            generator.canyonCurvature, 0f, 1f);
        if (newCurvature != generator.canyonCurvature)
        {
            generator.canyonCurvature = newCurvature;
            MarkDirty();
        }
        
        EditorGUILayout.EndVertical();
    }

    #endregion
}