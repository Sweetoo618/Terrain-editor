using UnityEngine;
using System.Collections.Generic;

public class TerrainValidationTool : MonoBehaviour
{
    [Header("Settings")]
    public bool enableValidation = true;
    public bool showDebugGizmos = true;
    public float crackDetectionThreshold = 0.1f;
    
    private TerrainGenerator _terrainGenerator;
    private MeshFilter _meshFilter;
    
    void Start()
    {
        _terrainGenerator = FindObjectOfType<TerrainGenerator>();
        if (_terrainGenerator != null)
        {
            _terrainGenerator.OnTerrainGenerated += OnTerrainGenerated;
        }
        
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null)
        {
            _meshFilter = FindObjectOfType<TerrainEditor>()?.GetComponent<MeshFilter>();
        }
    }
    
    void OnTerrainGenerated()
    {
        if (enableValidation)
        {
            ValidateTerrain();
        }
    }
    
    [ContextMenu("Validate Terrain")]
    public void ValidateTerrain()
    {
        if (_meshFilter == null || _meshFilter.sharedMesh == null)
        {
            Debug.LogWarning("[TerrainValidationTool] No mesh found for validation");
            return;
        }
        
        Mesh mesh = _meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        
        Debug.Log($"[TerrainValidationTool] Validating terrain mesh: {vertices.Length} vertices, {triangles.Length / 3} triangles");
        
        // 验证顶点数量
        ValidateVertexCount(mesh);
        
        // 检测裂缝
        List<Vector3> crackPoints = DetectCracks(vertices);
        
        // 验证三角形索引
        ValidateTriangleIndices(vertices, triangles);
        
        // 验证边界
        ValidateBounds(mesh);
        
        if (crackPoints.Count > 0)
        {
            Debug.LogWarning($"[TerrainValidationTool] Found {crackPoints.Count} potential crack points");
        }
        else
        {
            Debug.Log("[TerrainValidationTool] No cracks detected");
        }
    }
    
    void ValidateVertexCount(Mesh mesh)
    {
        if (_terrainGenerator != null && _terrainGenerator.config != null)
        {
            int width = _terrainGenerator.config.width;
            int depth = _terrainGenerator.config.depth;
            int expectedVertices = (width + 1) * (depth + 1);
            
            if (mesh.vertices.Length != expectedVertices)
            {
                Debug.LogError($"[TerrainValidationTool] Vertex count mismatch: expected {expectedVertices}, got {mesh.vertices.Length}");
            }
            else
            {
                Debug.Log($"[TerrainValidationTool] Vertex count correct: {expectedVertices}");
            }
        }
    }
    
    List<Vector3> DetectCracks(Vector3[] vertices)
    {
        List<Vector3> crackPoints = new List<Vector3>();
        
        for (int i = 0; i < vertices.Length; i++)
        {
            // 检查NaN或Infinity值
            if (float.IsNaN(vertices[i].x) || float.IsNaN(vertices[i].y) || float.IsNaN(vertices[i].z))
            {
                crackPoints.Add(vertices[i]);
                Debug.LogError($"[TerrainValidationTool] Found NaN vertex at index {i}");
                continue;
            }
            
            if (float.IsInfinity(vertices[i].x) || float.IsInfinity(vertices[i].y) || float.IsInfinity(vertices[i].z))
            {
                crackPoints.Add(vertices[i]);
                Debug.LogError($"[TerrainValidationTool] Found Infinity vertex at index {i}");
                continue;
            }
            
            // 检查相邻顶点的高度差（裂缝检测）
            if (i > 0 && i < vertices.Length - 1)
            {
                float heightDiff = Mathf.Abs(vertices[i].y - vertices[i - 1].y);
                if (heightDiff > crackDetectionThreshold)
                {
                    crackPoints.Add(vertices[i]);
                }
            }
        }
        
        return crackPoints;
    }
    
    void ValidateTriangleIndices(Vector3[] vertices, int[] triangles)
    {
        int maxIndex = vertices.Length - 1;
        
        for (int i = 0; i < triangles.Length; i++)
        {
            if (triangles[i] < 0 || triangles[i] > maxIndex)
            {
                Debug.LogError($"[TerrainValidationTool] Invalid triangle index {triangles[i]} at position {i} (max valid: {maxIndex})");
            }
        }
        
        Debug.Log($"[TerrainValidationTool] Triangle indices validation completed");
    }
    
    void ValidateBounds(Mesh mesh)
    {
        Bounds bounds = mesh.bounds;
        Debug.Log($"[TerrainValidationTool] Mesh bounds: Center={bounds.center}, Size={bounds.size}");
        
        if (_terrainGenerator != null && _terrainGenerator.config != null)
        {
            float expectedWidth = _terrainGenerator.config.width * _terrainGenerator.config.cellSize;
            float expectedDepth = _terrainGenerator.config.depth * _terrainGenerator.config.cellSize;
            
            float widthDiff = Mathf.Abs(bounds.size.x - expectedWidth);
            float depthDiff = Mathf.Abs(bounds.size.z - expectedDepth);
            
            if (widthDiff > 0.01f)
            {
                Debug.LogWarning($"[TerrainValidationTool] Width mismatch: expected {expectedWidth}, got {bounds.size.x}");
            }
            
            if (depthDiff > 0.01f)
            {
                Debug.LogWarning($"[TerrainValidationTool] Depth mismatch: expected {expectedDepth}, got {bounds.size.z}");
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || _meshFilter == null || _meshFilter.sharedMesh == null)
            return;
        
        Vector3[] vertices = _meshFilter.sharedMesh.vertices;
        
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            
            // 标记异常顶点
            if (float.IsNaN(vertices[i].y) || float.IsInfinity(vertices[i].y))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(worldPos, 0.2f);
            }
            else if (vertices[i].y < -1000 || vertices[i].y > 1000)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(worldPos, 0.15f);
            }
        }
    }
}