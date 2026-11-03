using UnityEngine;
using System;

[Serializable]
public class TerrainConfig : ScriptableObject
{
    [Header("笔刷设置")]
    public BrushConfig brush = new BrushConfig();

    [Header("相机设置")]
    public CameraConfig camera = new CameraConfig();

    [Header("编辑器设置")]
    public EditorConfig editor = new EditorConfig();

    [Header("地形生成设置")]
    public GeneratorConfig generator = new GeneratorConfig();

    [Header("地形编辑设置")]
    public EditConfig edit = new EditConfig();

    [Serializable]
    public class BrushConfig
    {
        [Tooltip("笔刷半径")]
        public float radius = 2f;

        [Tooltip("升高/降低程度")]
        public float raiseLowerStrength = 1f;

        [Tooltip("粗糙化程度")]
        [Range(0f, 1f)]
        public float roughenAmount = 0.5f;

        [Tooltip("平整程度")]
        public float flattenStrength = 1f;

        [Tooltip("平滑程度")]
        public float smoothStrength = 1f;

        [Tooltip("笔刷半径最小值")]
        public float minRadius = 0.1f;

        [Tooltip("笔刷半径最大值")]
        public float maxRadius = 10f;

        [Tooltip("笔刷强度最小值")]
        public float minStrength = 0.1f;

        [Tooltip("笔刷强度最大值")]
        public float maxStrength = 5f;

        [Tooltip("笔刷类型")]
        public BrushType brushType = BrushType.Circle;
    }

    [Serializable]
    public class CameraConfig
    {
        [Tooltip("移动速度")]
        public float moveSpeed = 50f;

        [Tooltip("旋转速度")]
        public float rotationSpeed = 200f;

        [Tooltip("垂直速度")]
        public float verticalSpeed = 50f;

        [Tooltip("移动速度最小值")]
        public float minMoveSpeed = 5f;

        [Tooltip("移动速度最大值")]
        public float maxMoveSpeed = 200f;

        [Tooltip("旋转速度最小值")]
        public float minRotationSpeed = 30f;

        [Tooltip("旋转速度最大值")]
        public float maxRotationSpeed = 500f;

        [Tooltip("垂直速度最小值")]
        public float minVerticalSpeed = 5f;

        [Tooltip("垂直速度最大值")]
        public float maxVerticalSpeed = 200f;

        [Tooltip("启用平滑移动")]
        public bool enableSmoothing = true;

        [Tooltip("平滑时间系数")]
        [Range(0.01f, 0.5f)]
        public float smoothTime = 0.1f;

        [Tooltip("启用高度自适应速度")]
        public bool enableSpeedAdjustment = true;

        [Tooltip("距离检测更新间隔（秒）")]
        [Range(0.1f, 2f)]
        public float distanceCheckInterval = 0.5f;
    }

    [Serializable]
    public class EditorConfig
    {
        [Tooltip("当前编辑模式")]
        public TerrainEditor.EditMode currentMode = TerrainEditor.EditMode.RaiseLower;

        [Tooltip("显示网格")]
        public bool showGrid = false;

        [Tooltip("显示法线")]
        public bool showNormals = false;
    }

    [Serializable]
        public class GeneratorConfig
        {
            [Tooltip("地形宽度（顶点数）")]
            [Range(10, 256)]
            public int width = 64;

            [Tooltip("地形深度（顶点数）")]
            [Range(10, 256)]
            public int depth = 64;

            [Tooltip("地形单元格大小")]
            public float cellSize = 1f;

            [Tooltip("地形高度范围")]
            public float heightRange = 10f;

            [Tooltip("地形类型")]
            public TerrainType terrainType = TerrainType.Plains;

            [Tooltip("噪声种子")]
            public int seed = 0;

            [Tooltip("基础噪声频率")]
            public float noiseFrequency = 0.1f;

            [Tooltip("基础噪声振幅")]
            public float noiseAmplitude = 1f;

            [Tooltip("噪声层数")]
            [Range(1, 8)]
            public int noiseOctaves = 4;

            [Tooltip("噪声衰减系数")]
            public float noisePersistence = 0.5f;

            // Plains (平原) 参数
            [Tooltip("平原基准高度")]
            [Range(0f, 1f)]
            public float plainsBaseHeight = 0.1f;

            // Hills (丘陵) 参数
            [Tooltip("丘陵起伏幅度")]
            [Range(0.1f, 1f)]
            public float hillsAmplitude = 0.4f;

            [Tooltip("丘陵平滑程度")]
            [Range(1f, 5f)]
            public float hillsSmoothness = 2f;

            // Mountains (山脉) 参数
            [Tooltip("山脉主峰高度")]
            [Range(0.5f, 1f)]
            public float mountainsPeakHeight = 0.9f;

            [Tooltip("山脉陡峭程度")]
            [Range(0.5f, 2f)]
            public float mountainsSharpness = 1.2f;

            [Tooltip("山脉粗糙度")]
            [Range(1f, 4f)]
            public float mountainsRoughness = 2f;

            // Valley (山谷) 参数
            [Tooltip("山谷凹陷深度")]
            [Range(0.3f, 0.9f)]
            public float valleyDepth = 0.6f;

            [Tooltip("山谷平滑过渡")]
            [Range(0.1f, 0.5f)]
            public float valleySmoothness = 0.2f;

            // Plateau (高原) 参数
            [Tooltip("高原抬升高度")]
            [Range(0.3f, 0.9f)]
            public float plateauHeight = 0.7f;

            [Tooltip("高原区域比例")]
            [Range(0.2f, 0.8f)]
            public float plateauAreaRatio = 0.5f;

            [Tooltip("高原边缘过渡")]
            [Range(0.05f, 0.3f)]
            public float plateauEdgeSmooth = 0.15f;

            // Canyon (峡谷) 参数
            [Tooltip("峡谷深度")]
            [Range(0.4f, 0.9f)]
            public float canyonDepth = 0.7f;

            [Tooltip("峡谷宽度")]
            [Range(0.05f, 0.3f)]
            public float canyonWidth = 0.15f;

            [Tooltip("峡谷弯曲程度")]
            [Range(0f, 1f)]
            public float canyonCurvature = 0.5f;
        }

    [Serializable]
    public class EditConfig
    {
        [Tooltip("全局高度缩放")]
        public float globalHeightScale = 1f;

        [Tooltip("全局平滑程度")]
        [Range(0f, 1f)]
        public float globalSmoothAmount = 0.5f;

        [Tooltip("全局粗糙程度")]
        [Range(0f, 1f)]
        public float globalRoughness = 0.5f;
    }

    public enum BrushType
    {
        Circle,
        Square,
        Smooth,
        Noise
    }

    public enum TerrainType
    {
        Plains,
        Hills,
        Mountains,
        Valley,
        Plateau,
        Canyon
    }

    public void ResetToDefaults()
    {
        brush = new BrushConfig();
        camera = new CameraConfig();
        editor = new EditorConfig();
        generator = new GeneratorConfig();
        edit = new EditConfig();
    }

    public void ClampAllValues()
    {
        brush.radius = Mathf.Clamp(brush.radius, brush.minRadius, brush.maxRadius);
        brush.raiseLowerStrength = Mathf.Clamp(brush.raiseLowerStrength, brush.minStrength, brush.maxStrength);
        brush.flattenStrength = Mathf.Clamp(brush.flattenStrength, brush.minStrength, brush.maxStrength);
        brush.smoothStrength = Mathf.Clamp(brush.smoothStrength, brush.minStrength, brush.maxStrength);
        brush.roughenAmount = Mathf.Clamp01(brush.roughenAmount);

        camera.moveSpeed = Mathf.Clamp(camera.moveSpeed, camera.minMoveSpeed, camera.maxMoveSpeed);
        camera.rotationSpeed = Mathf.Clamp(camera.rotationSpeed, camera.minRotationSpeed, camera.maxRotationSpeed);
        camera.verticalSpeed = Mathf.Clamp(camera.verticalSpeed, camera.minVerticalSpeed, camera.maxVerticalSpeed);
    }

    public float GetCurrentModeStrength(TerrainEditor.EditMode mode)
    {
        switch (mode)
        {
            case TerrainEditor.EditMode.RaiseLower:
                return brush.raiseLowerStrength;
            case TerrainEditor.EditMode.Roughen:
                return brush.roughenAmount;
            case TerrainEditor.EditMode.Flatten:
                return brush.flattenStrength;
            case TerrainEditor.EditMode.Smooth:
                return brush.smoothStrength;
            default:
                return brush.raiseLowerStrength;
        }
    }

    public void SetCurrentModeStrength(TerrainEditor.EditMode mode, float value)
    {
        switch (mode)
        {
            case TerrainEditor.EditMode.RaiseLower:
                brush.raiseLowerStrength = Mathf.Clamp(value, brush.minStrength, brush.maxStrength);
                break;
            case TerrainEditor.EditMode.Roughen:
                brush.roughenAmount = Mathf.Clamp01(value);
                break;
            case TerrainEditor.EditMode.Flatten:
                brush.flattenStrength = Mathf.Clamp(value, brush.minStrength, brush.maxStrength);
                break;
            case TerrainEditor.EditMode.Smooth:
                brush.smoothStrength = Mathf.Clamp(value, brush.minStrength, brush.maxStrength);
                break;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/Terrain Editor/Terrain Config")]
    public static void CreateTerrainConfig()
    {
        TerrainConfig config = CreateInstance<TerrainConfig>();
        UnityEditor.AssetDatabase.CreateAsset(config, "Assets/TerrainConfig.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.EditorUtility.FocusProjectWindow();
        UnityEditor.Selection.activeObject = config;
    }
#endif
}