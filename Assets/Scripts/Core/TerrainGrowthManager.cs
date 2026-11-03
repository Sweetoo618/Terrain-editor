using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGrowthManager : MonoBehaviour
{
    [Header("Terrain Dimensions")]
    public int terrainWidth = 64;
    public int terrainHeight = 64;
    public TerrainGrowthPreset growthPreset;

    [Header("Performance Settings")]
    [Tooltip("地形生长更新间隔（秒），值越大性能越好但生长越慢")]
    public float growthUpdateInterval = 0.5f;

    [Tooltip("启用平滑处理")]
    public bool enableSmoothing = false;

    [Tooltip("平滑核大小")]
    public int smoothingKernelSize = 2;

    private Mesh _terrainMesh;
    private Vector3[] _originalVertices;
    private Vector3[] _modifiedVertices;
    private bool _initialized = false;
    private int _randomSeed;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            _terrainMesh = meshFilter.sharedMesh;
            _originalVertices = (Vector3[])_terrainMesh.vertices.Clone();
            _modifiedVertices = (Vector3[])_terrainMesh.vertices.Clone();
            _initialized = true;

            if (growthPreset != null)
            {
                _randomSeed = growthPreset.randomSeed;
            }
        }
        else
        {
            Debug.LogError("MeshFilter or sharedMesh is missing on this GameObject.");
        }
    }

    private float _lastGrowthUpdateTime;

    void Update()
    {
        if (!_initialized || growthPreset == null) return;

        if (Time.time - _lastGrowthUpdateTime > growthUpdateInterval)
        {
            UpdateTerrainGrowth(growthUpdateInterval);
            _lastGrowthUpdateTime = Time.time;
        }
    }

    public void UpdateTerrainGrowth(float deltaTime)
    {
        if (growthPreset == null || _modifiedVertices == null)
        {
            return;
        }

        UnityEngine.Random.InitState(_randomSeed);

        int width = terrainWidth + 1;
        for (int z = 0; z <= terrainHeight; z++)
        {
            for (int x = 0; x <= terrainWidth; x++)
            {
                int i = z * width + x;
                Vector3 worldPos = transform.TransformPoint(_originalVertices[i]);

                if (worldPos.x >= growthPreset.growthRange.min.x && worldPos.x <= growthPreset.growthRange.max.x &&
                    worldPos.y >= growthPreset.growthRange.min.y && worldPos.y <= growthPreset.growthRange.max.y &&
                    worldPos.z >= growthPreset.growthRange.min.z && worldPos.z <= growthPreset.growthRange.max.z)
                {
                    float noise = Mathf.PerlinNoise(worldPos.x * growthPreset.noiseScale, worldPos.z * growthPreset.noiseScale) * 2 - 1;
                    float noiseFactor = 1 + (noise * growthPreset.noiseStrength);

                    float randomFactor = UnityEngine.Random.Range(0.9f, 1.1f);
                    float heightScale = growthPreset.heightScaleCurve.Evaluate(worldPos.y / growthPreset.growthRange.max.y);
                    float growthAmount = growthPreset.growthRate * heightScale * randomFactor * noiseFactor * deltaTime;
                    Vector3 growth = growthPreset.growthDirection * growthAmount;
                    _modifiedVertices[i] += growth;
                }
            }
        }

        if (enableSmoothing && growthPreset.smoothness > 0)
        {
            ApplySmoothing();
        }

        _terrainMesh.vertices = _modifiedVertices;
        _terrainMesh.RecalculateNormals();
    }

    void ApplySmoothing()
    {
        int width = terrainWidth + 1;
        int height = terrainHeight + 1;
        float[,] heightMap = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heightMap[x, y] = _modifiedVertices[y * width + x].y;
            }
        }

        int kernelSize = Mathf.Min(smoothingKernelSize, growthPreset.smoothness);
        float[,] smoothedHeightMap = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float avgHeight = 0;
                int count = 0;
                for (int ky = -kernelSize; ky <= kernelSize; ky++)
                {
                    for (int kx = -kernelSize; kx <= kernelSize; kx++)
                    {
                        int nx = x + kx;
                        int ny = y + ky;
                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            avgHeight += heightMap[nx, ny];
                            count++;
                        }
                    }
                }
                smoothedHeightMap[x, y] = avgHeight / count;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                _modifiedVertices[y * width + x].y = smoothedHeightMap[x, y];
            }
        }
    }

    public void ResetTerrain()
    {
        if (_terrainMesh != null && _originalVertices != null)
        {
            _modifiedVertices = (Vector3[])_originalVertices.Clone();
            _terrainMesh.vertices = _modifiedVertices;
            _terrainMesh.RecalculateNormals();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (growthPreset != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Vector3 center = (growthPreset.growthRange.min + growthPreset.growthRange.max) / 2;
            Vector3 size = growthPreset.growthRange.max - growthPreset.growthRange.min;
            Gizmos.DrawWireCube(center, size);
        }
    }
}