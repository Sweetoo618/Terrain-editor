using System;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Configuration")]
    public TerrainConfig terrainConfig;

    [Header("References")]
    public TerrainEditor terrainBrush;

    public event Action OnTerrainGenerated;
    public event Action OnTerrainRegenerated;
    public event Action OnTerrainCleared;

    private Mesh _terrainMesh;
    private Vector3[] _baseVertices;
    public TerrainConfig.GeneratorConfig config => terrainConfig != null ? terrainConfig.generator : null;

    private void Start()
    {
        if (terrainBrush == null) terrainBrush = FindObjectOfType<TerrainEditor>();
    }

    public bool Generate()
    {
        if (config == null || terrainBrush == null)
        {
            if (terrainBrush == null) terrainBrush = FindObjectOfType<TerrainEditor>();
            if (config == null || terrainBrush == null)
            {
                Debug.LogError("[TerrainGenerator] Configuration or TerrainEditor is missing.");
                return false;
            }
        }

        if (config.width < 10 || config.depth < 10 || config.cellSize <= 0f)
        {
            Debug.LogError("[TerrainGenerator] Grid dimensions must be at least 10 and cell size must be positive.");
            return false;
        }

        MeshFilter meshFilter = terrainBrush.GetComponent<MeshFilter>();
        if (meshFilter == null) return false;
        CreateTerrainMesh(meshFilter);
        ApplyHeightMap(GenerateHeightMap());
        terrainBrush.SetTerrainMesh(_terrainMesh, config.width, config.depth, config.cellSize);
        OnTerrainGenerated?.Invoke();
        return true;
    }

    public void Regenerate()
    {
        if (Generate()) OnTerrainRegenerated?.Invoke();
    }

    public void Clear()
    {
        if (terrainBrush == null) return;
        terrainBrush.ClearToFlat();
        OnTerrainCleared?.Invoke();
    }

    public float[,] GenerateHeightMap()
    {
        if (config == null) return new float[0, 0];
        int width = config.width + 1;
        int depth = config.depth + 1;
        float[,] heights = new float[width, depth];
        for (int z = 0; z < depth; z++)
        {
            float v = depth == 1 ? 0f : (float)z / (depth - 1);
            for (int x = 0; x < width; x++)
            {
                float u = width == 1 ? 0f : (float)x / (width - 1);
                heights[x, z] = Mathf.Clamp01(EvaluateHeight(u, v));
            }
        }
        return heights;
    }

    private float EvaluateHeight(float u, float v)
    {
        float x = u * 2f - 1f;
        float z = v * 2f - 1f;
        switch (config.terrainType)
        {
            case TerrainConfig.TerrainType.Hills: return EvaluateHills(u, v);
            case TerrainConfig.TerrainType.Mountains: return EvaluateMountains(x, z);
            case TerrainConfig.TerrainType.Valley: return EvaluateValley(z);
            case TerrainConfig.TerrainType.Plateau: return EvaluatePlateau(x, z);
            case TerrainConfig.TerrainType.Canyon: return EvaluateCanyon(u, v, z);
            default: return EvaluatePlains(x, z);
        }
    }

    private float EvaluatePlains(float x, float z)
    {
        float edge = Mathf.Clamp01(Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)));
        float relief = EvaluateCurve(config.plainsProfile, 1f - edge);
        return config.plainsBaseHeight + config.plainsRelief * relief;
    }

    private float EvaluateHills(float u, float v)
    {
        float wave = (Mathf.Sin(u * Mathf.PI * config.hillsFrequency) * Mathf.Sin(v * Mathf.PI * config.hillsFrequency) + 1f) * 0.5f;
        float profile = EvaluateCurve(config.hillsProfile, wave);
        return config.hillsBaseHeight + config.hillsAmplitude * profile;
    }

    private float EvaluateMountains(float x, float z)
    {
        float radius = Mathf.Clamp01(Mathf.Sqrt(x * x + z * z) / 1.41421356f);
        float peak = EvaluateCurve(config.mountainsProfile, 1f - radius);
        peak = Mathf.Pow(Mathf.Clamp01(peak), config.mountainsSharpness);
        return Mathf.Lerp(config.mountainsBaseHeight, config.mountainsPeakHeight, peak);
    }

    private float EvaluateValley(float z)
    {
        float distance = Mathf.Clamp01(Mathf.Abs(z) / Mathf.Max(0.001f, config.valleyWidth));
        float slope = EvaluateCurve(config.valleyProfile, distance);
        return Mathf.Lerp(config.valleyFloorHeight, config.valleyRimHeight, slope);
    }

    private float EvaluatePlateau(float x, float z)
    {
        float distance = Mathf.Clamp01(Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) / Mathf.Max(0.001f, config.plateauWidth));
        float edge = EvaluateCurve(config.plateauEdgeProfile, distance);
        return Mathf.Lerp(config.plateauHeight, config.plateauBaseHeight, edge);
    }

    private float EvaluateCanyon(float u, float v, float z)
    {
        float centerLine = Mathf.Sin((u - 0.5f) * Mathf.PI * 2f) * config.canyonCurvature * 0.35f;
        float distance = Mathf.Clamp01(Mathf.Abs(z - centerLine) / Mathf.Max(0.001f, config.canyonWidth));
        float wall = EvaluateCurve(config.canyonWallProfile, distance);
        return Mathf.Lerp(config.canyonFloorHeight, config.canyonRimHeight, wall);
    }

    private static float EvaluateCurve(AnimationCurve curve, float value)
    {
        return curve == null ? value : Mathf.Clamp01(curve.Evaluate(Mathf.Clamp01(value)));
    }

    private void CreateTerrainMesh(MeshFilter meshFilter)
    {
        int width = config.width;
        int depth = config.depth;
        int vertexWidth = width + 1;
        int vertexCount = vertexWidth * (depth + 1);
        if (_terrainMesh == null) _terrainMesh = new Mesh();
        else _terrainMesh.Clear();
        _terrainMesh.name = "Generated Terrain";

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                int index = z * vertexWidth + x;
                vertices[index] = new Vector3(x * config.cellSize, 0f, z * config.cellSize);
                uv[index] = new Vector2((float)x / width, (float)z / depth);
            }
        }

        int[] triangles = new int[width * depth * 6];
        int triangleIndex = 0;
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int topLeft = z * vertexWidth + x;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + vertexWidth;
                int bottomRight = bottomLeft + 1;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topRight;
                triangles[triangleIndex++] = topRight;
                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = bottomRight;
            }
        }
        _terrainMesh.vertices = vertices;
        _terrainMesh.uv = uv;
        _terrainMesh.triangles = triangles;
        _terrainMesh.RecalculateNormals();
        meshFilter.sharedMesh = _terrainMesh;
        MeshCollider collider = terrainBrush.GetComponent<MeshCollider>();
        if (collider != null) collider.sharedMesh = _terrainMesh;
        _baseVertices = (Vector3[])vertices.Clone();
    }

    private void ApplyHeightMap(float[,] heights)
    {
        if (_terrainMesh == null || heights == null) return;
        Vector3[] vertices = (Vector3[])_baseVertices.Clone();
        int vertexWidth = config.width + 1;
        for (int z = 0; z <= config.depth; z++)
        {
            for (int x = 0; x <= config.width; x++)
            {
                int index = z * vertexWidth + x;
                vertices[index].y = heights[x, z] * config.heightRange;
            }
        }
        _terrainMesh.vertices = vertices;
        _terrainMesh.RecalculateNormals();
        _terrainMesh.RecalculateBounds();
    }
}
