using UnityEngine;
using System;

public class UIControlManager : MonoBehaviour
{
    public static UIControlManager Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("地形编辑器统一配置")]
    public TerrainConfig config;

    [Header("Service References")]
    [Tooltip("地形笔刷组件")]
    public TerrainEditor terrainBrush;

    [Tooltip("地形生成器组件")]
    public TerrainGenerator terrainGenerator;

    [Tooltip("地形导出器组件")]
    public TerrainExporter terrainExporter;

    [Tooltip("相机控制器组件")]
    public CameraController cameraController;

    [Header("Runtime Settings")]
    [Tooltip("启用实时同步")]
    public bool enableRealTimeSync = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeReferences();
        ApplyInitialSettings();
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    void InitializeReferences()
    {
        if (terrainBrush == null)
            terrainBrush = FindObjectOfType<TerrainEditor>();
        if (terrainGenerator == null)
            terrainGenerator = FindObjectOfType<TerrainGenerator>();
        if (terrainExporter == null)
        {
            terrainExporter = FindObjectOfType<TerrainExporter>();
            if (terrainExporter == null)
            {
                GameObject exporterObj = new GameObject("TerrainExporter");
                terrainExporter = exporterObj.AddComponent<TerrainExporter>();
                Debug.Log("[UIControlManager] TerrainExporter component created automatically.");
            }
        }
        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();

        if (config == null)
        {
            config = ScriptableObject.CreateInstance<TerrainConfig>();
        }
    }

    void ApplyInitialSettings()
    {
        if (terrainBrush != null)
        {
            terrainBrush.brushRadius = config.brush.radius;
            terrainBrush.brushStrength = config.GetCurrentModeStrength(config.editor.currentMode);
            terrainBrush.roughenAmount = config.brush.roughenAmount;
            terrainBrush.currentMode = config.editor.currentMode;
        }

        if (cameraController != null)
        {
            cameraController.SetMoveSpeed(config.camera.moveSpeed);
            cameraController.SetRotationSpeed(config.camera.rotationSpeed);
            cameraController.SetVerticalSpeed(config.camera.verticalSpeed);
        }
    }

    void SubscribeToEvents()
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.OnTerrainGenerated += () => TerrainEvents.InvokeTerrainGenerated();
            terrainGenerator.OnTerrainRegenerated += () => TerrainEvents.InvokeTerrainRegenerated();
            terrainGenerator.OnTerrainCleared += () => TerrainEvents.InvokeTerrainCleared();
        }

        if (terrainExporter != null)
        {
            terrainExporter.OnExportSuccess += (msg) => TerrainEvents.InvokeExportSuccess(msg);
            terrainExporter.OnExportFailed += (msg) => TerrainEvents.InvokeExportFailed(msg);
        }
    }

    void UnsubscribeFromEvents()
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.OnTerrainGenerated -= () => TerrainEvents.InvokeTerrainGenerated();
            terrainGenerator.OnTerrainRegenerated -= () => TerrainEvents.InvokeTerrainRegenerated();
            terrainGenerator.OnTerrainCleared -= () => TerrainEvents.InvokeTerrainCleared();
        }

        if (terrainExporter != null)
        {
            terrainExporter.OnExportSuccess -= (msg) => TerrainEvents.InvokeExportSuccess(msg);
            terrainExporter.OnExportFailed -= (msg) => TerrainEvents.InvokeExportFailed(msg);
        }
    }

    #region Brush API
    public void SetBrushRadius(float value)
    {
        config.brush.radius = Mathf.Clamp(value, config.brush.minRadius, config.brush.maxRadius);

        if (enableRealTimeSync && terrainBrush != null)
        {
            terrainBrush.brushRadius = config.brush.radius;
        }

        TerrainEvents.InvokeBrushRadiusChanged(config.brush.radius);
    }

    public float GetBrushRadius()
    {
        return config.brush.radius;
    }

    public void SetBrushStrength(float value, TerrainEditor.EditMode mode)
    {
        config.SetCurrentModeStrength(mode, value);

        if (enableRealTimeSync && terrainBrush != null)
        {
            terrainBrush.brushStrength = config.GetCurrentModeStrength(mode);
            if (mode == TerrainEditor.EditMode.Roughen)
            {
                terrainBrush.roughenAmount = config.brush.roughenAmount;
            }
        }

        TerrainEvents.InvokeBrushStrengthChanged(config.GetCurrentModeStrength(mode), mode);
    }

    public float GetBrushStrength(TerrainEditor.EditMode mode)
    {
        return config.GetCurrentModeStrength(mode);
    }

    public void SetRoughenAmount(float value)
    {
        config.brush.roughenAmount = Mathf.Clamp01(value);

        if (enableRealTimeSync && terrainBrush != null)
        {
            terrainBrush.roughenAmount = config.brush.roughenAmount;
        }

        TerrainEvents.InvokeRoughenAmountChanged(config.brush.roughenAmount);
    }

    public float GetRoughenAmount()
    {
        return config.brush.roughenAmount;
    }
    #endregion

    #region Camera API
    public void SetMoveSpeed(float value)
    {
        config.camera.moveSpeed = Mathf.Clamp(value, config.camera.minMoveSpeed, config.camera.maxMoveSpeed);

        if (enableRealTimeSync && cameraController != null)
        {
            cameraController.SetMoveSpeed(config.camera.moveSpeed);
        }

        TerrainEvents.InvokeCameraMoveSpeedChanged(config.camera.moveSpeed);
    }

    public float GetMoveSpeed()
    {
        return config.camera.moveSpeed;
    }

    public void SetRotationSpeed(float value)
    {
        config.camera.rotationSpeed = Mathf.Clamp(value, config.camera.minRotationSpeed, config.camera.maxRotationSpeed);

        if (enableRealTimeSync && cameraController != null)
        {
            cameraController.SetRotationSpeed(config.camera.rotationSpeed);
        }

        TerrainEvents.InvokeCameraRotationSpeedChanged(config.camera.rotationSpeed);
    }

    public float GetRotationSpeed()
    {
        return config.camera.rotationSpeed;
    }

    public void SetVerticalSpeed(float value)
    {
        config.camera.verticalSpeed = Mathf.Clamp(value, config.camera.minVerticalSpeed, config.camera.maxVerticalSpeed);

        if (enableRealTimeSync && cameraController != null)
        {
            cameraController.SetVerticalSpeed(config.camera.verticalSpeed);
        }

        TerrainEvents.InvokeCameraVerticalSpeedChanged(config.camera.verticalSpeed);
    }

    public float GetVerticalSpeed()
    {
        return config.camera.verticalSpeed;
    }
    #endregion

    #region Editor API
    public void SetEditMode(TerrainEditor.EditMode mode)
    {
        config.editor.currentMode = mode;

        if (enableRealTimeSync && terrainBrush != null)
        {
            terrainBrush.currentMode = mode;
            terrainBrush.brushStrength = config.GetCurrentModeStrength(mode);
            if (mode == TerrainEditor.EditMode.Roughen)
            {
                terrainBrush.roughenAmount = config.brush.roughenAmount;
            }
        }

        TerrainEvents.InvokeEditModeChanged(mode);
    }

    public TerrainEditor.EditMode GetEditMode()
    {
        return config.editor.currentMode;
    }

    public void ToggleGrid()
    {
        config.editor.showGrid = !config.editor.showGrid;
    }

    public void ToggleNormals()
    {
        config.editor.showNormals = !config.editor.showNormals;
    }
    #endregion

    #region Terrain Generation API
    public void GenerateTerrain()
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.Generate(config.generator);
        }
    }

    public void RegenerateTerrain()
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.Regenerate();
        }
    }

    public void ClearTerrain()
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.Clear();
        }
    }
    #endregion

    #region Terrain Edit API
    public void ApplyGlobalScale(float scaleFactor)
    {
        if (terrainBrush != null)
        {
            terrainBrush.ApplyGlobalScale(scaleFactor);
        }
    }

    public void ApplyGlobalSmooth(float smoothAmount)
    {
        if (terrainBrush != null)
        {
            terrainBrush.ApplyGlobalSmooth(smoothAmount);
        }
    }

    public void ApplyGlobalRoughness(float roughness)
    {
        if (terrainBrush != null)
        {
            terrainBrush.ApplyGlobalRoughness(roughness);
        }
    }
    #endregion

    #region Export/Import API
    public void ExportTerrain(string filename = "terrain")
    {
        if (terrainExporter != null)
        {
            terrainExporter.ExportToDesktop(filename);
        }
    }

    public void ExportTerrainToPath(string fullPath)
    {
        Debug.Log($"[UIControlManager] ExportTerrainToPath called: {fullPath}");
        Debug.Log($"[UIControlManager] terrainExporter is null: {terrainExporter == null}");
        
        if (terrainExporter != null)
        {
            terrainExporter.ExportToPath(fullPath);
        }
        else
        {
            Debug.LogError("[UIControlManager] terrainExporter is null! Cannot export terrain.");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("导出失败", "TerrainExporter组件未找到！\n请确保场景中有TerrainExporter组件。", "确定");
#endif
        }
    }

    public void ImportTerrain(string path)
    {
        if (terrainExporter != null)
        {
            terrainExporter.ImportFromPath(path);
        }
    }
    #endregion

    #region Batch Operations
    public void ResetToDefaults()
    {
        config.ResetToDefaults();
        ApplyInitialSettings();
        TerrainEvents.InvokeSettingsReset();
    }

    public void ApplyAllSettings()
    {
        ApplyInitialSettings();
    }
    #endregion

    #region Slider Value API (Compatibility)
    public void SetSliderValue(string controlName, float value)
    {
        switch (controlName.ToLower())
        {
            case "brushradius":
            case "radius":
                SetBrushRadius(value);
                break;
            case "brushstrength":
            case "strength":
                SetBrushStrength(value, config.editor.currentMode);
                break;
            case "roughenamount":
            case "roughen":
                SetRoughenAmount(value);
                break;
            case "movespeed":
                SetMoveSpeed(value);
                break;
            case "rotationspeed":
                SetRotationSpeed(value);
                break;
            case "verticalspeed":
                SetVerticalSpeed(value);
                break;
        }
    }

    public float GetSliderValue(string controlName)
    {
        switch (controlName.ToLower())
        {
            case "brushradius":
            case "radius":
                return GetBrushRadius();
            case "brushstrength":
            case "strength":
                return GetBrushStrength(config.editor.currentMode);
            case "roughenamount":
            case "roughen":
                return GetRoughenAmount();
            case "movespeed":
                return GetMoveSpeed();
            case "rotationspeed":
                return GetRotationSpeed();
            case "verticalspeed":
                return GetVerticalSpeed();
            default:
                return 0f;
        }
    }
    #endregion

    [Serializable]
    public class TerrainData
    {
        public int width;
        public int height;
        public float[] heights;
    }
}