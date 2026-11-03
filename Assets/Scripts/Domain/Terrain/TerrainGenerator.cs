using UnityEngine;
using System;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Generator Settings")]
    public TerrainConfig.GeneratorConfig config;

    [Header("References")]
    public TerrainEditor terrainBrush;

    public event Action OnTerrainGenerated;
    public event Action OnTerrainRegenerated;
    public event Action OnTerrainCleared;

    private Mesh _terrainMesh;
    private Vector3[] _baseVertices;

    void Start()
    {
        if (terrainBrush == null)
            terrainBrush = FindObjectOfType<TerrainEditor>();
    }

    public void Generate(TerrainConfig.GeneratorConfig generatorConfig)
    {
        config = generatorConfig;
        Generate();
    }

    public void Generate()
    {
        if (terrainBrush == null)
        {
            Debug.LogError("[TerrainGenerator] TerrainEditor not found!");
            return;
        }

        config.cellSize = terrainBrush.terrainScale;

        MeshFilter meshFilter = terrainBrush.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("[TerrainGenerator] MeshFilter not found!");
            return;
        }

        CreateTerrainMesh(meshFilter);
        ApplyTerrainType();

        terrainBrush.SyncVerticesFromMesh();

        OnTerrainGenerated?.Invoke();
        Debug.Log($"[TerrainGenerator] Terrain generated: {config.width}x{config.depth}, Type: {config.terrainType}");
    }

    public void Regenerate()
    {
        if (config.seed == 0)
        {
            config.seed = UnityEngine.Random.Range(0, 100000);
        }
        else
        {
            config.seed = UnityEngine.Random.Range(0, 100000);
        }

        Generate();
        OnTerrainRegenerated?.Invoke();
    }

    public void Clear()
    {
        if (terrainBrush == null)
            return;

        MeshFilter meshFilter = terrainBrush.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
            return;

        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = 0;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        terrainBrush.SyncVerticesFromMesh();
        OnTerrainCleared?.Invoke();
    }

    private void CreateTerrainMesh(MeshFilter meshFilter)
    {
        int width = config.width;
        int depth = config.depth;
        float cellSize = config.cellSize;

        _terrainMesh = new Mesh();
        _terrainMesh.name = "Generated Terrain";

        int vertexCount = (width + 1) * (depth + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];

        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                int index = z * (width + 1) + x;
                vertices[index] = new Vector3(x * cellSize, 0, z * cellSize);
                uv[index] = new Vector2((float)x / width, (float)z / depth);
                normals[index] = Vector3.up;
            }
        }

        _baseVertices = (Vector3[])vertices.Clone();

        int triangleCount = width * depth * 6;
        int[] triangles = new int[triangleCount];

        int triangleIndex = 0;
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int topLeft = z * (width + 1) + x;
                int topRight = topLeft + 1;
                int bottomLeft = (z + 1) * (width + 1) + x;
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
        _terrainMesh.normals = normals;
        _terrainMesh.triangles = triangles;

        _terrainMesh.RecalculateBounds();
        _terrainMesh.RecalculateNormals();

        meshFilter.sharedMesh = _terrainMesh;

        MeshCollider meshCollider = terrainBrush.GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = _terrainMesh;
        }
    }

    private void ApplyTerrainType()
    {
        if (_terrainMesh == null || _baseVertices == null)
            return;

        Vector3[] vertices = (Vector3[])_baseVertices.Clone();
        float[,] heightMap = GenerateTerrainHeightMap();

        int width = config.width;
        int depth = config.depth;
        
        // 验证高度图尺寸
        if (heightMap.GetLength(0) != width || heightMap.GetLength(1) != depth)
        {
            Debug.LogError($"[TerrainGenerator] Heightmap size mismatch in ApplyTerrainType: expected {width}x{depth}, got {heightMap.GetLength(0)}x{heightMap.GetLength(1)}");
            return;
        }

        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                int index = z * (width + 1) + x;
                
                // 安全获取高度值，避免越界
                float height = 0f;
                if (x < width && z < depth)
                {
                    height = heightMap[x, z];
                }
                else if (x == width && z < depth)
                {
                    // 右边缘：使用左边的高度值
                    height = heightMap[x - 1, z];
                }
                else if (z == depth && x < width)
                {
                    // 下边缘：使用上边的高度值
                    height = heightMap[x, z - 1];
                }
                else if (x == width && z == depth)
                {
                    // 右下角：使用左上角的高度值
                    height = heightMap[x - 1, z - 1];
                }
                
                // 确保高度值有效
                height = float.IsNaN(height) ? 0f : height;
                height = float.IsInfinity(height) ? 0f : height;
                
                vertices[index].y = height * config.heightRange;
            }
        }

        _terrainMesh.vertices = vertices;
        _terrainMesh.RecalculateNormals();
        _terrainMesh.RecalculateBounds();
        
        ValidateTerrainType(heightMap);
    }

    private float[,] GenerateTerrainHeightMap()
    {
        int width = config.width;
        int depth = config.depth;
        float[,] heightMap = new float[width, depth];

        UnityEngine.Random.InitState(config.seed);

        switch (config.terrainType)
        {
            case TerrainConfig.TerrainType.Plains:
                GeneratePlains(heightMap, width, depth);
                break;
            case TerrainConfig.TerrainType.Hills:
                GenerateHills(heightMap, width, depth);
                break;
            case TerrainConfig.TerrainType.Mountains:
                GenerateMountains(heightMap, width, depth);
                break;
            case TerrainConfig.TerrainType.Valley:
                GenerateValley(heightMap, width, depth);
                break;
            case TerrainConfig.TerrainType.Plateau:
                GeneratePlateau(heightMap, width, depth);
                break;
            case TerrainConfig.TerrainType.Canyon:
                GenerateCanyon(heightMap, width, depth);
                break;
            default:
                GenerateDefault(heightMap, width, depth);
                break;
        }

        return heightMap;
    }

    #region Terrain Type Generators

    // Plains (平原): 生成绝对平坦的基准面地形
    private void GeneratePlains(float[,] heightMap, int width, int depth)
    {
        float baseHeight = config.plainsBaseHeight;
        
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                heightMap[x, z] = baseHeight;
            }
        }
    }

    // Hills (丘陵): 缓坡起伏的地形特征，坡度变化平滑自然
    private void GenerateHills(float[,] heightMap, int width, int depth)
    {
        float offsetX = UnityEngine.Random.Range(-1000f, 1000f);
        float offsetZ = UnityEngine.Random.Range(-1000f, 1000f);
        float amplitude = config.hillsAmplitude;
        float frequency = config.noiseFrequency * 0.5f;
        float smoothness = config.hillsSmoothness;

        // 预计算噪声值，避免直接使用可能产生异常的数值
        float[,] noiseMap = new float[width, depth];
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseValue = Mathf.PerlinNoise(
                    (x + offsetX) * frequency, 
                    (z + offsetZ) * frequency);
                
                // 确保噪声值在有效范围内
                noiseValue = Mathf.Clamp(noiseValue, 0.001f, 0.999f);
                
                noiseMap[x, z] = noiseValue;
            }
        }

        // 使用预计算的噪声生成高度，添加平滑过渡
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // 使用3x3邻域平滑，避免单点异常
                float smoothNoise = GetSmoothNoise(noiseMap, x, z, width, depth);
                
                // 应用平滑处理，确保坡度变化自然
                float height = Mathf.Pow(smoothNoise, smoothness) * amplitude;
                
                // 添加多层低频噪声增加层次感（使用安全的噪声值）
                float noise2 = Mathf.PerlinNoise(
                    (x + offsetX + 500) * frequency * 0.3f, 
                    (z + offsetZ + 500) * frequency * 0.3f);
                height += noise2 * amplitude * 0.3f;
                
                // 确保高度值有效，避免NaN或负值
                height = float.IsNaN(height) ? 0f : height;
                height = float.IsInfinity(height) ? 0f : height;
                
                heightMap[x, z] = Mathf.Clamp(height, 0.001f, 0.999f);
            }
        }
    }

    // 3x3邻域平滑处理，消除单点异常
    private float GetSmoothNoise(float[,] noiseMap, int x, int z, int width, int depth)
    {
        float sum = 0f;
        int count = 0;
        
        for (int dz = -1; dz <= 1; dz++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int nx = Mathf.Clamp(x + dx, 0, width - 1);
                int nz = Mathf.Clamp(z + dz, 0, depth - 1);
                sum += noiseMap[nx, nz];
                count++;
            }
        }
        
        return sum / count;
    }

    // Mountains (山脉): 高度不一、陡峭程度各异的地形
    private void GenerateMountains(float[,] heightMap, int width, int depth)
    {
        float offsetX = UnityEngine.Random.Range(-1000f, 1000f);
        float offsetZ = UnityEngine.Random.Range(-1000f, 1000f);
        float peakHeight = config.mountainsPeakHeight;
        float sharpness = config.mountainsSharpness;
        float roughness = config.mountainsRoughness;

        // 预计算基础噪声
        float[,] baseNoiseMap = new float[width, depth];
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float baseNoise = 0f;
                float amplitude = 1f;
                float frequency = config.noiseFrequency;

                // 使用多层噪声创建复杂地形
                for (int octave = 0; octave < config.noiseOctaves; octave++)
                {
                    float noiseValue = Mathf.PerlinNoise(
                        (x + offsetX + octave * 100) * frequency, 
                        (z + offsetZ + octave * 100) * frequency);
                    baseNoise += noiseValue * amplitude;
                    amplitude *= config.noisePersistence;
                    frequency *= 2f;
                }
                
                baseNoiseMap[x, z] = baseNoise / (2f - Mathf.Pow(config.noisePersistence, config.noiseOctaves));
            }
        }

        // 找到噪声最大值作为主峰区域
        float maxNoise = 0f;
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                maxNoise = Mathf.Max(maxNoise, baseNoiseMap[x, z]);
            }
        }

        // 生成最终高度图
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float baseNoise = baseNoiseMap[x, z];
                
                // 判断是否为主峰区域（噪声值高于80%最大值）
                bool isPeakRegion = baseNoise > maxNoise * 0.8f;
                
                // 主峰区域应用更高的峰值高度，增强视觉区分度
                float effectivePeakHeight = isPeakRegion ? peakHeight : peakHeight * 0.7f;
                
                // 修复陡峭程度逻辑：值越大越陡峭
                // 使用反函数，当sharpness增大时，高度变化更剧烈
                float height;
                if (baseNoise > 0.5f)
                {
                    // 高于平均值的区域（山峰）：sharpness越大，峰值越高越陡峭
                    float normalized = (baseNoise - 0.5f) / 0.5f;
                    height = 0.5f + Mathf.Pow(normalized, 1f / sharpness) * 0.5f * effectivePeakHeight;
                }
                else
                {
                    // 低于平均值的区域（山谷）：sharpness越大，下降越快
                    float normalized = baseNoise / 0.5f;
                    height = Mathf.Pow(normalized, sharpness) * 0.5f * effectivePeakHeight;
                }
                
                // 增强粗糙度效果
                float roughNoise1 = Mathf.PerlinNoise(
                    (x + offsetX + 1000) * config.noiseFrequency * roughness * 2f, 
                    (z + offsetZ + 1000) * config.noiseFrequency * roughness * 2f);
                float roughNoise2 = Mathf.PerlinNoise(
                    (x + offsetX + 2000) * config.noiseFrequency * roughness * 4f, 
                    (z + offsetZ + 2000) * config.noiseFrequency * roughness * 4f);
                
                // 组合粗糙度噪声，产生更明显的表面细节
                float combinedRoughness = (roughNoise1 * 0.7f + roughNoise2 * 0.3f - 0.5f);
                height += combinedRoughness * 0.15f * roughness;
                
                // 确保高度有效
                height = float.IsNaN(height) ? 0f : height;
                height = float.IsInfinity(height) ? 0f : height;
                
                heightMap[x, z] = Mathf.Clamp(height, 0.001f, 0.999f);
            }
        }
    }

    // Valley (山谷): 明显凹陷形态的地形
    private void GenerateValley(float[,] heightMap, int width, int depth)
    {
        float offsetX = UnityEngine.Random.Range(-1000f, 1000f);
        float offsetZ = UnityEngine.Random.Range(-1000f, 1000f);
        float valleyDepth = config.valleyDepth;
        float smoothness = config.valleySmoothness;

        // 生成基础噪声作为山谷形状
        float[,] baseNoise = new float[width, depth];
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float noise = Mathf.PerlinNoise(
                    (x + offsetX) * config.noiseFrequency * 0.5f, 
                    (z + offsetZ) * config.noiseFrequency * 0.5f);
                baseNoise[x, z] = noise;
            }
        }

        // 创建山谷凹陷效果
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // 找到局部最小值作为山谷底部
                float minNeighbor = 1f;
                for (int dz = -1; dz <= 1; dz++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = Mathf.Clamp(x + dx, 0, width - 1);
                        int nz = Mathf.Clamp(z + dz, 0, depth - 1);
                        minNeighbor = Mathf.Min(minNeighbor, baseNoise[nx, nz]);
                    }
                }

                // 基于距离中心的距离创建凹陷
                float distFromMin = Mathf.Abs(baseNoise[x, z] - minNeighbor);
                float valleyShape = Mathf.Exp(-distFromMin / smoothness);
                
                // 计算高度：中心最低，向外逐渐升高
                float height = (1f - valleyShape * valleyDepth) * (0.3f + baseNoise[x, z] * 0.4f);
                
                heightMap[x, z] = Mathf.Clamp(height, 0f, 1f);
            }
        }
    }

    // Plateau (高原): 部分区域地形整体抬升，表面平坦
    private void GeneratePlateau(float[,] heightMap, int width, int depth)
    {
        float offsetX = UnityEngine.Random.Range(-1000f, 1000f);
        float offsetZ = UnityEngine.Random.Range(-1000f, 1000f);
        float plateauHeight = config.plateauHeight;
        float areaRatio = config.plateauAreaRatio;
        float edgeSmooth = config.plateauEdgeSmooth;

        // 生成高原区域的遮罩
        float[,] plateauMask = new float[width, depth];
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float noise = Mathf.PerlinNoise(
                    (x + offsetX) * config.noiseFrequency * 0.3f, 
                    (z + offsetZ) * config.noiseFrequency * 0.3f);
                plateauMask[x, z] = noise;
            }
        }

        // 计算高原阈值
        float threshold = 1f - areaRatio;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float maskValue = plateauMask[x, z];
                
                if (maskValue > threshold)
                {
                    // 高原区域：保持平坦
                    float plateauNoise = Mathf.PerlinNoise(
                        (x + offsetX + 500) * config.noiseFrequency * 0.1f, 
                        (z + offsetZ + 500) * config.noiseFrequency * 0.1f);
                    heightMap[x, z] = plateauHeight + (plateauNoise - 0.5f) * 0.05f;
                }
                else
                {
                    // 非高原区域：较低的基础高度
                    float baseNoise = Mathf.PerlinNoise(
                        (x + offsetX) * config.noiseFrequency, 
                        (z + offsetZ) * config.noiseFrequency);
                    heightMap[x, z] = baseNoise * 0.3f;
                }

                // 边缘平滑过渡
                float edgeBlend = 0f;
                for (int dz = -2; dz <= 2; dz++)
                {
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        int nx = Mathf.Clamp(x + dx, 0, width - 1);
                        int nz = Mathf.Clamp(z + dz, 0, depth - 1);
                        float neighborMask = plateauMask[nx, nz];
                        if ((neighborMask > threshold) != (maskValue > threshold))
                        {
                            edgeBlend += 1f;
                        }
                    }
                }

                if (edgeBlend > 0)
                {
                    edgeBlend /= 25f;
                    float smoothFactor = Mathf.SmoothStep(0f, 1f, edgeBlend / edgeSmooth);
                    float lowHeight = Mathf.PerlinNoise(
                        (x + offsetX) * config.noiseFrequency, 
                        (z + offsetZ) * config.noiseFrequency) * 0.3f;
                    heightMap[x, z] = Mathf.Lerp(heightMap[x, z], lowHeight, smoothFactor);
                }
            }
        }
    }

    // Canyon (峡谷): 狭窄的山谷地形
    private void GenerateCanyon(float[,] heightMap, int width, int depth)
    {
        float offsetX = UnityEngine.Random.Range(-1000f, 1000f);
        float offsetZ = UnityEngine.Random.Range(-1000f, 1000f);
        float canyonDepth = config.canyonDepth;
        float canyonWidth = config.canyonWidth;
        float curvature = config.canyonCurvature;

        // 生成峡谷中心线
        float[] centerLine = new float[depth];
        for (int z = 0; z < depth; z++)
        {
            float noise1 = Mathf.PerlinNoise(z * 0.02f + offsetX, offsetZ);
            float noise2 = Mathf.PerlinNoise(z * 0.01f + offsetX + 500, offsetZ + 500);
            centerLine[z] = (width / 2f) + (noise1 - 0.5f) * width * curvature + (noise2 - 0.5f) * width * 0.1f;
        }

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float centerX = centerLine[z];
                float distanceToCenter = Mathf.Abs(x - centerX) / (width / 2f);

                // 创建狭窄的V形峡谷
                float canyonProfile = 0f;
                if (distanceToCenter < canyonWidth * 2f)
                {
                    // 使用二次曲线创建V形剖面
                    canyonProfile = 1f - Mathf.Pow(distanceToCenter / canyonWidth, 2f);
                    canyonProfile = Mathf.Clamp(canyonProfile, 0f, 1f);
                }

                // 计算高度：峡谷底部低，两侧高
                float baseHeight = 0.4f + Mathf.PerlinNoise(
                    (x + offsetX) * config.noiseFrequency * 0.5f, 
                    (z + offsetZ) * config.noiseFrequency * 0.5f) * 0.2f;
                float height = baseHeight + (1f - canyonProfile * canyonDepth) * 0.4f;

                // 添加岩壁粗糙度
                float wallNoise = Mathf.PerlinNoise(
                    (x + offsetX + 1000) * config.noiseFrequency * 3f, 
                    (z + offsetZ + 1000) * config.noiseFrequency * 0.5f);
                height += (wallNoise - 0.5f) * 0.05f;

                heightMap[x, z] = Mathf.Clamp(height, 0f, 1f);
            }
        }
    }

    // 默认地形生成
    private void GenerateDefault(float[,] heightMap, int width, int depth)
    {
        float offsetX = UnityEngine.Random.Range(-1000f, 1000f);
        float offsetZ = UnityEngine.Random.Range(-1000f, 1000f);

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseValue = Mathf.PerlinNoise(
                    (x + offsetX) * config.noiseFrequency, 
                    (z + offsetZ) * config.noiseFrequency);
                heightMap[x, z] = noiseValue;
            }
        }
    }

    #endregion

    #region Validation

    private void ValidateTerrainType(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int depth = heightMap.GetLength(1);
        
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;
        float avgHeight = 0f;
        int total = width * depth;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float h = heightMap[x, z];
                minHeight = Mathf.Min(minHeight, h);
                maxHeight = Mathf.Max(maxHeight, h);
                avgHeight += h;
            }
        }
        avgHeight /= total;

        switch (config.terrainType)
        {
            case TerrainConfig.TerrainType.Plains:
                float heightVariance = maxHeight - minHeight;
                if (heightVariance > 0.001f)
                {
                    Debug.LogWarning($"[TerrainGenerator] Plains validation: Height variance {heightVariance:F6} exceeds threshold");
                }
                break;
            
            case TerrainConfig.TerrainType.Valley:
                if (minHeight > avgHeight * 0.5f)
                {
                    Debug.LogWarning($"[TerrainGenerator] Valley validation: Minimum height {minHeight:F3} is not sufficiently lower than average {avgHeight:F3}");
                }
                break;
            
            case TerrainConfig.TerrainType.Canyon:
                // 检查峡谷宽度是否明显小于山谷
                int lowPoints = 0;
                for (int z = 0; z < depth; z++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (heightMap[x, z] < avgHeight * 0.6f)
                            lowPoints++;
                    }
                }
                float lowAreaRatio = (float)lowPoints / total;
                if (lowAreaRatio > 0.3f)
                {
                    Debug.LogWarning($"[TerrainGenerator] Canyon validation: Low area ratio {lowAreaRatio:F3} exceeds expected canyon narrowness");
                }
                break;
        }

        Debug.Log($"[TerrainGenerator] Generated terrain stats - Type: {config.terrainType}, Min: {minHeight:F3}, Max: {maxHeight:F3}, Avg: {avgHeight:F3}");
    }

    #endregion

    public void ApplyHeightMap(float[,] heightMap)
    {
        if (_terrainMesh == null)
            return;

        Vector3[] vertices = _terrainMesh.vertices;
        int width = config.width;
        int depth = config.depth;

        // 验证高度图尺寸
        if (heightMap.GetLength(0) != width || heightMap.GetLength(1) != depth)
        {
            Debug.LogError($"[TerrainGenerator] Heightmap size mismatch: expected {width}x{depth}, got {heightMap.GetLength(0)}x{heightMap.GetLength(1)}");
            return;
        }

        // 确保顶点数组足够大
        int expectedVertexCount = (width + 1) * (depth + 1);
        if (vertices.Length != expectedVertexCount)
        {
            Debug.LogError($"[TerrainGenerator] Vertex array size mismatch: expected {expectedVertexCount}, got {vertices.Length}");
            return;
        }

        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                int index = z * (width + 1) + x;
                
                // 获取高度值（边缘处理）
                float height = 0f;
                if (x < width && z < depth)
                {
                    height = heightMap[x, z] * config.heightRange;
                }
                else if (x == width && z < depth)
                {
                    height = heightMap[x - 1, z] * config.heightRange;
                }
                else if (z == depth && x < width)
                {
                    height = heightMap[x, z - 1] * config.heightRange;
                }
                else if (x == width && z == depth)
                {
                    height = heightMap[x - 1, z - 1] * config.heightRange;
                }
                
                vertices[index].y = height;
            }
        }

        _terrainMesh.vertices = vertices;
        _terrainMesh.RecalculateNormals();
        _terrainMesh.RecalculateBounds();

        terrainBrush.SyncVerticesFromMesh();
    }

    public Mesh GetTerrainMesh()
    {
        return _terrainMesh;
    }

    public Vector3[] GetBaseVertices()
    {
        return _baseVertices;
    }
}