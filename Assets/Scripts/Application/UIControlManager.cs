using UnityEngine;

public class UIControlManager : MonoBehaviour
{
    public static UIControlManager Instance { get; private set; }

    [Header("Configuration")]
    public TerrainConfig config;

    [Header("Services")]
    public TerrainEditor terrainBrush;
    public TerrainGenerator terrainGenerator;
    public TerrainExporter terrainExporter;
    public CameraController cameraController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeReferences();
        ApplySettings();
    }

    private void Start()
    {
        if (terrainGenerator != null && terrainBrush != null)
        {
            terrainGenerator.terrainBrush = terrainBrush;
            terrainGenerator.Generate();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void InitializeReferences()
    {
        if (terrainBrush == null) terrainBrush = FindObjectOfType<TerrainEditor>();
        if (terrainGenerator == null) terrainGenerator = FindObjectOfType<TerrainGenerator>();
        if (cameraController == null) cameraController = FindObjectOfType<CameraController>();
        if (terrainExporter == null) terrainExporter = FindObjectOfType<TerrainExporter>();
        if (terrainExporter == null) terrainExporter = gameObject.AddComponent<TerrainExporter>();
        if (config == null) config = ScriptableObject.CreateInstance<TerrainConfig>();
        if (terrainGenerator != null)
        {
            terrainGenerator.terrainConfig = config;
            if (terrainBrush != null) terrainGenerator.terrainBrush = terrainBrush;
        }
    }

    public void ApplySettings()
    {
        if (config == null) return;
        if (terrainBrush != null)
        {
            terrainBrush.terrainScale = config.generator.cellSize;
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

    public void GenerateTerrain() => terrainGenerator?.Generate();
    public void RegenerateTerrain() => terrainGenerator?.Regenerate();
    public void ClearTerrain() => terrainGenerator?.Clear();
    public void ApplyGlobalScale(float factor) => terrainBrush?.ApplyGlobalScale(factor);
    public void ApplyGlobalSmooth(float amount) => terrainBrush?.ApplyGlobalSmooth(amount);
    public void ApplyGlobalRoughness(float amount) => terrainBrush?.ApplyGlobalRoughness(amount);

    public void ExportTerrain(string filename = "terrain") => terrainExporter?.ExportToDesktop(filename);
    public void ExportTerrainToPath(string path) => terrainExporter?.ExportToPath(path);
    public bool ImportTerrain(string path) => terrainExporter != null && terrainExporter.ImportFromPath(path);

    public void ResetToDefaults()
    {
        if (config == null) return;
        config.ResetToDefaults();
        ApplySettings();
    }
}
