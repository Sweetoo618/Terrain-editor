
using UnityEngine;

[ExecuteInEditMode]
public class CameraInitializer : MonoBehaviour
{
    [Header("Target References")]
    public TerrainGenerator terrainGenerator;

    [Header("Camera Settings")]
    [Tooltip("高度乘数，用于根据地形宽度确定相机高度")]
    public float heightMultiplier = 1.0f;
    
    [Tooltip("相机绕X轴的旋转角度（度）")]
    public float angleX = 45.0f;

    [ContextMenu("初始化相机位置")]
    public void InitializeCamera()
    {
        if (terrainGenerator == null)
            terrainGenerator = FindObjectOfType<TerrainGenerator>();

        if (terrainGenerator == null || terrainGenerator.config == null)
            return;

        float terrainWidth = terrainGenerator.config.width * terrainGenerator.config.cellSize;
        float terrainHeight = terrainGenerator.config.depth * terrainGenerator.config.cellSize;

        Vector3 terrainCenter = new Vector3(terrainWidth / 2f, 0, terrainHeight / 2f);
        float height = terrainWidth * heightMultiplier;
        float zOffset = height / Mathf.Tan(angleX * Mathf.Deg2Rad);
        
        Vector3 cameraPos = new Vector3(terrainCenter.x, height, terrainCenter.z - zOffset);

        transform.position = cameraPos;
        transform.rotation = Quaternion.Euler(angleX, 0, 0);
    }

    void Start()
    {
        InitializeCamera();
    }
}
