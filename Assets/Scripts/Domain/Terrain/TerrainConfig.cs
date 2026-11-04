using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainConfig", menuName = "Terrain Editor/Terrain Config")]
public class TerrainConfig : ScriptableObject
{
    public BrushConfig brush = new BrushConfig();
    public CameraConfig camera = new CameraConfig();
    public EditorConfig editor = new EditorConfig();
    public GeneratorConfig generator = new GeneratorConfig();
    public EditConfig edit = new EditConfig();

    [Serializable] public class BrushConfig
    {
        [Min(0.1f)] public float radius = 2f;
        [Min(0.01f)] public float raiseLowerStrength = 1f;
        [Range(0f, 1f)] public float roughenAmount = 0.5f;
        [Min(0.01f)] public float flattenStrength = 1f;
        [Min(0.01f)] public float smoothStrength = 1f;
        [Min(0.1f)] public float minRadius = 0.1f;
        [Min(0.1f)] public float maxRadius = 10f;
        [Min(0.01f)] public float minStrength = 0.1f;
        [Min(0.01f)] public float maxStrength = 5f;
        public BrushType brushType = BrushType.Circle;
    }

    [Serializable] public class CameraConfig
    {
        public float moveSpeed = 50f;
        public float rotationSpeed = 200f;
        public float verticalSpeed = 50f;
        public float minMoveSpeed = 5f;
        public float maxMoveSpeed = 200f;
        public float minRotationSpeed = 30f;
        public float maxRotationSpeed = 500f;
        public float minVerticalSpeed = 5f;
        public float maxVerticalSpeed = 200f;
        public bool enableSmoothing = true;
        [Range(0.01f, 0.5f)] public float smoothTime = 0.1f;
        public bool enableSpeedAdjustment = true;
        [Range(0.1f, 2f)] public float distanceCheckInterval = 0.5f;
    }

    [Serializable] public class EditorConfig
    {
        public TerrainEditor.EditMode currentMode = TerrainEditor.EditMode.RaiseLower;
        public bool showGrid;
        public bool showNormals;
    }

    [Serializable] public class GeneratorConfig
    {
        [Range(10, 256)] public int width = 64;
        [Range(10, 256)] public int depth = 64;
        [Min(0.01f)] public float cellSize = 1f;
        [Min(0f)] public float heightRange = 10f;
        public TerrainType terrainType = TerrainType.Plains;

        [Header("Plains")]
        [Range(0f, 1f)] public float plainsBaseHeight = 0.1f;
        [Range(0f, 1f)] public float plainsRelief = 0.05f;
        public AnimationCurve plainsProfile = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Hills")]
        [Range(0f, 1f)] public float hillsBaseHeight = 0.15f;
        [Range(0f, 1f)] public float hillsAmplitude = 0.4f;
        [Min(0.1f)] public float hillsFrequency = 1.5f;
        public AnimationCurve hillsProfile = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Mountains")]
        [Range(0f, 1f)] public float mountainsBaseHeight = 0.1f;
        [Range(0f, 1f)] public float mountainsPeakHeight = 0.9f;
        [Range(0.1f, 8f)] public float mountainsSharpness = 2f;
        public AnimationCurve mountainsProfile = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Valley")]
        [Range(0f, 1f)] public float valleyRimHeight = 0.7f;
        [Range(0f, 1f)] public float valleyFloorHeight = 0.15f;
        [Range(0.05f, 1f)] public float valleyWidth = 0.55f;
        public AnimationCurve valleyProfile = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Plateau")]
        [Range(0f, 1f)] public float plateauBaseHeight = 0.2f;
        [Range(0f, 1f)] public float plateauHeight = 0.75f;
        [Range(0.05f, 1f)] public float plateauWidth = 0.5f;
        public AnimationCurve plateauEdgeProfile = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Canyon")]
        [Range(0f, 1f)] public float canyonRimHeight = 0.65f;
        [Range(0f, 1f)] public float canyonFloorHeight = 0.08f;
        [Range(0.01f, 1f)] public float canyonWidth = 0.18f;
        [Range(0f, 1f)] public float canyonCurvature = 0.5f;
        public AnimationCurve canyonWallProfile = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }

    [Serializable] public class EditConfig
    {
        public float globalHeightScale = 1f;
        [Range(0f, 1f)] public float globalSmoothAmount = 0.5f;
        [Range(0f, 1f)] public float globalRoughness = 0.5f;
    }

    public enum BrushType { Circle, Square, Smooth, Noise }
    public enum TerrainType { Plains, Hills, Mountains, Valley, Plateau, Canyon }

    public float GetCurrentModeStrength(TerrainEditor.EditMode mode)
    {
        switch (mode)
        {
            case TerrainEditor.EditMode.Roughen: return brush.roughenAmount;
            case TerrainEditor.EditMode.Flatten: return brush.flattenStrength;
            case TerrainEditor.EditMode.Smooth: return brush.smoothStrength;
            default: return brush.raiseLowerStrength;
        }
    }

    public void SetCurrentModeStrength(TerrainEditor.EditMode mode, float value)
    {
        switch (mode)
        {
            case TerrainEditor.EditMode.Roughen: brush.roughenAmount = Mathf.Clamp01(value); break;
            case TerrainEditor.EditMode.Flatten: brush.flattenStrength = Mathf.Max(0.01f, value); break;
            case TerrainEditor.EditMode.Smooth: brush.smoothStrength = Mathf.Max(0.01f, value); break;
            default: brush.raiseLowerStrength = Mathf.Max(0.01f, value); break;
        }
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
    }
}
