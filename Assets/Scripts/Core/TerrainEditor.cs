using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainEditor : MonoBehaviour
{
    public enum EditMode { RaiseLower, Roughen, Flatten, Smooth }
    public enum TerrainState { NotGenerated, Generating, Editable }

    [Header("Undo/Redo Settings")]
    public int maxUndoSteps = 50;

    private List<Vector3[]> _undoStack = new List<Vector3[]>();
    private List<Vector3[]> _redoStack = new List<Vector3[]>();

    [Header("Terrain Settings")]
    public float terrainScale = 1.0f;

    [Header("Brush Settings")]
    public EditMode currentMode = EditMode.RaiseLower;
    public float brushRadius = 5.0f;
    public float brushStrength = 1.0f;
    [Range(0f, 1f)]
    public float roughenAmount = 0.5f;

    [Header("Visuals")]
    public LineRenderer brushVisualizer;

    [Header("Performance Settings")]
    [Tooltip("网格更新节流间隔（秒），值越大性能越好但响应越慢")]
    public float meshUpdateThrottle = 0.033f;

    [Tooltip("启用顶点插值使小半径下地形更平滑")]
    public bool enableVertexInterpolation = true;

    [Header("State Management")]
    public TerrainState terrainState = TerrainState.NotGenerated;
    private int _terrainWidth = 0;
    private int _terrainHeight = 0;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private MeshCollider _meshCollider;
    private Vector3[] _smoothedVertices;
    private TerrainGenerator _terrainGenerator;
    private bool _strokeUndoSaved;

    public int TerrainWidth => _terrainWidth;
    public int TerrainHeight => _terrainHeight;

    void Start()
    {
        _terrainGenerator = FindObjectOfType<TerrainGenerator>();
        if (_terrainGenerator != null)
        {
            _terrainGenerator.terrainBrush = this;
            if (UIControlManager.Instance == null)
                _terrainGenerator.Generate();
        }
        else
        {
            GenerateTerrain();
        }

        InitializeBrushVisualizer();
    }

    void InitializeBrushVisualizer()
    {
        if (brushVisualizer == null)
        {
            GameObject visualizerObj = new GameObject("BrushVisualizer");
            visualizerObj.transform.SetParent(this.transform);
            brushVisualizer = visualizerObj.AddComponent<LineRenderer>();

            brushVisualizer.startWidth = 0.05f;
            brushVisualizer.endWidth = 0.05f;
            brushVisualizer.positionCount = 21;
            brushVisualizer.useWorldSpace = true;
            brushVisualizer.loop = true;

            Shader unlitShader = Shader.Find("Unlit/Color");
            if (unlitShader == null)
            {
                unlitShader = Shader.Find("Sprites/Default");
            }
            brushVisualizer.material = new Material(unlitShader);

            brushVisualizer.startColor = new Color(1, 0, 0, 0.7f);
            brushVisualizer.endColor = new Color(1, 0, 0, 0.7f);
            brushVisualizer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            brushVisualizer.receiveShadows = false;
            brushVisualizer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            brushVisualizer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }
    }

    private float _lastUpdateTime;
    private const float UPDATE_INTERVAL = 0.05f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            _strokeUndoSaved = false;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Z)) Undo();
            else if (Input.GetKeyDown(KeyCode.Y)) Redo();
        }

        if (Time.time - _lastUpdateTime > UPDATE_INTERVAL)
        {
            UpdateBrushVisualizer();
            _lastUpdateTime = Time.time;
        }

        if (Input.GetMouseButton(0) && terrainState == TerrainState.Editable)
        {
            HandleMouseInput();
        }

        if (Input.GetMouseButtonUp(0))
            _strokeUndoSaved = false;
    }

    void UpdateBrushVisualizer()
    {
        if (brushVisualizer == null) return;

        if (Camera.main == null || terrainState != TerrainState.Editable)
        {
            brushVisualizer.enabled = false;
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            brushVisualizer.enabled = true;
            DrawCircle(hit.point + Vector3.up * 0.1f, brushRadius);
        }
        else
        {
            brushVisualizer.enabled = false;
        }
    }

    void DrawCircle(Vector3 center, float radius)
    {
        int segments = brushVisualizer.positionCount - 1;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (360f / segments);
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            brushVisualizer.SetPosition(i, new Vector3(center.x + x, center.y, center.z + z));
        }
    }

    public void GenerateTerrain()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();

        _mesh = new Mesh();
        meshFilter.mesh = _mesh;
        _mesh.name = "Procedural Terrain";

        _terrainWidth = 64;
        _terrainHeight = 64;

        _vertices = new Vector3[(_terrainWidth + 1) * (_terrainHeight + 1)];
        _smoothedVertices = new Vector3[_vertices.Length];
        Vector2[] uv = new Vector2[_vertices.Length];
        int[] triangles = new int[_terrainWidth * _terrainHeight * 6];

        for (int i = 0, y = 0; y <= _terrainHeight; y++)
        {
            for (int x = 0; x <= _terrainWidth; x++, i++)
            {
                _vertices[i] = new Vector3(x * terrainScale, 0, y * terrainScale);
                _smoothedVertices[i] = _vertices[i];
                uv[i] = new Vector2((float)x / _terrainWidth, (float)y / _terrainHeight);
            }
        }

        for (int ti = 0, vi = 0, y = 0; y < _terrainHeight; y++, vi++)
        {
            for (int x = 0; x < _terrainWidth; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + _terrainWidth + 1;
                triangles[ti + 5] = vi + _terrainWidth + 2;
            }
        }

        _mesh.vertices = _vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();

        _meshCollider.sharedMesh = _mesh;
        terrainState = TerrainState.Editable;
        ClearHistory();
    }

    public void SyncVerticesFromMesh()
    {
        SyncVerticesFromMesh(0, 0);
    }

    public void SyncVerticesFromMesh(int width, int height)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        _mesh = meshFilter.sharedMesh;
        _vertices = _mesh.vertices;
        _smoothedVertices = (Vector3[])_vertices.Clone();

        if (width > 0 && height > 0)
        {
            _terrainWidth = width;
            _terrainHeight = height;
        }
        else if (_terrainGenerator != null && _terrainGenerator.config != null)
        {
            _terrainWidth = _terrainGenerator.config.width;
            _terrainHeight = _terrainGenerator.config.depth;
        }
        else
        {
            CalculateTerrainDimensions();
        }

        _meshCollider = GetComponent<MeshCollider>();
        ClearHistory();
        terrainState = TerrainState.Editable;
        Debug.Log($"[TerrainEditor] Terrain ready for editing: {_terrainWidth} x {_terrainHeight}");
    }

    public void SetTerrainMesh(Mesh mesh, int width, int height, float cellSize = 0f)
    {
        if (mesh == null) return;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        meshFilter.sharedMesh = mesh;
        if (cellSize > 0f) terrainScale = cellSize;

        if (_meshCollider != null)
        {
            _meshCollider.sharedMesh = null;
            _meshCollider.sharedMesh = mesh;
        }

        SyncVerticesFromMesh(width, height);
    }

    private void CalculateTerrainDimensions()
    {
        if (_mesh == null) return;

        int vertexCount = _mesh.vertexCount;
        int estimatedSize = Mathf.RoundToInt(Mathf.Sqrt(vertexCount)) - 1;
        _terrainWidth = Mathf.Max(estimatedSize, 1);
        _terrainHeight = Mathf.Max(estimatedSize, 1);
    }

    void HandleMouseInput()
    {
        if (Camera.main == null)
        {
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == _meshCollider)
        {
            ModifyTerrain(hit.point);
        }
    }

    private float _lastMeshUpdateTime;
    private bool _needsMeshUpdate = false;

    void ModifyTerrain(Vector3 hitPoint)
    {
        if (_vertices == null || _vertices.Length == 0) return;

        int width = _terrainWidth + 1;

        Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);

        int centerX = Mathf.RoundToInt(localHitPoint.x / terrainScale);
        int centerZ = Mathf.RoundToInt(localHitPoint.z / terrainScale);
        int radiusInVertices = Mathf.CeilToInt(brushRadius / terrainScale);

        float deltaTime = Time.deltaTime;
        float currentStrength = Input.GetKey(KeyCode.LeftShift) ? -brushStrength : brushStrength;

        if (!_strokeUndoSaved)
        {
            SaveUndoState();
            _strokeUndoSaved = true;
        }

        float targetHeight = 0f;
        if (currentMode == EditMode.Flatten)
        {
            int centerIndex = centerZ * width + centerX;
            if (centerIndex >= 0 && centerIndex < _vertices.Length)
            {
                targetHeight = _vertices[centerIndex].y;
            }
        }

        for (int z = Mathf.Max(0, centerZ - radiusInVertices); z <= Mathf.Min(_terrainHeight, centerZ + radiusInVertices); z++)
        {
            for (int x = Mathf.Max(0, centerX - radiusInVertices); x <= Mathf.Min(_terrainWidth, centerX + radiusInVertices); x++)
            {
                int i = z * width + x;
                if (i >= 0 && i < _vertices.Length)
                {
                    float distance = Vector3.Distance(_vertices[i], localHitPoint);
                    if (distance < brushRadius)
                    {
                        float falloff = 1 - (distance / brushRadius);
                        falloff = falloff * falloff * (3 - 2 * falloff);

                        if (currentMode == EditMode.RaiseLower)
                        {
                            _vertices[i].y += currentStrength * falloff * deltaTime * 10f;
                        }
                        else if (currentMode == EditMode.Roughen)
                        {
                            float noise = (Mathf.PerlinNoise(_vertices[i].x * 0.3f, _vertices[i].z * 0.3f) * 2f - 1f);
                            float displacement = noise * brushStrength * roughenAmount * falloff * deltaTime * 8f;
                            _vertices[i].y += displacement;
                        }
                        else if (currentMode == EditMode.Flatten)
        {
                            float heightDifference = targetHeight - _vertices[i].y;
                            _vertices[i].y += heightDifference * brushStrength * falloff * deltaTime * 15f;
                        }
                        else if (currentMode == EditMode.Smooth)
                        {
                            float sumHeight = 0f;
                            int count = 0;

                            for (int dz = -1; dz <= 1; dz++)
                            {
                                for (int dx = -1; dx <= 1; dx++)
                                {
                                    if (dx == 0 && dz == 0) continue;

                                    int neighborX = x + dx;
                                    int neighborZ = z + dz;

                                    if (neighborX >= 0 && neighborX <= _terrainWidth &&
                                        neighborZ >= 0 && neighborZ <= _terrainHeight)
                                    {
                                        int neighborIndex = neighborZ * width + neighborX;
                                        sumHeight += _vertices[neighborIndex].y;
                                        count++;
                                    }
                                }
                            }

                            if (count > 0)
                            {
                                float averageHeight = sumHeight / count;
                                float heightDifference = averageHeight - _vertices[i].y;
                                _vertices[i].y += heightDifference * brushStrength * falloff * deltaTime * 12f;
                            }
                        }
                    }
                }
            }
        }

        _needsMeshUpdate = true;
    }

    void LateUpdate()
    {
        if (_needsMeshUpdate && Time.time - _lastMeshUpdateTime > meshUpdateThrottle)
        {
            UpdateMesh();
            _lastMeshUpdateTime = Time.time;
            _needsMeshUpdate = false;
        }
    }

    void UpdateMesh()
    {
        UpdateMesh(true);
    }

    void UpdateMesh(bool interpolate)
    {
        if (_mesh == null) return;

        if (interpolate && enableVertexInterpolation && _smoothedVertices != null)
        {
            ApplyVertexInterpolation();
            _mesh.vertices = _smoothedVertices;
        }
        else
        {
            _mesh.vertices = _vertices;
        }

        _mesh.RecalculateNormals();

        if (_meshCollider != null && _meshCollider.enabled)
        {
            _meshCollider.sharedMesh = _mesh;
        }
    }

    void ApplyVertexInterpolation()
    {
        if (_vertices == null || _smoothedVertices == null) return;

        System.Array.Copy(_vertices, _smoothedVertices, _vertices.Length);

        int width = _terrainWidth + 1;
        if (width <= 0) return;

        for (int i = 0; i < _smoothedVertices.Length; i++)
        {
            int x = i % width;
            int z = i / width;

            if (x > 0 && x < _terrainWidth && z > 0 && z < _terrainHeight)
            {
                float avgHeight = (
                    _vertices[i - 1].y +
                    _vertices[i + 1].y +
                    _vertices[i - width].y +
                    _vertices[i + width].y
                ) * 0.25f;

                _smoothedVertices[i].y = Mathf.Lerp(_vertices[i].y, avgHeight, 0.5f);
            }
        }
    }

    private void SaveUndoState()
    {
        maxUndoSteps = Mathf.Max(1, maxUndoSteps);
        if (_undoStack.Count >= maxUndoSteps)
        {
            _undoStack.RemoveAt(0);
        }

        _undoStack.Add((Vector3[])_vertices.Clone());
        _redoStack.Clear();
    }

    public void Undo()
    {
        if (_undoStack.Count > 0)
        {
            _redoStack.Add((Vector3[])_vertices.Clone());
            _vertices = _undoStack[_undoStack.Count - 1];
            _undoStack.RemoveAt(_undoStack.Count - 1);
            UpdateMesh(false);
        }
    }

    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
            _undoStack.Add((Vector3[])_vertices.Clone());
            _vertices = _redoStack[_redoStack.Count - 1];
            _redoStack.RemoveAt(_redoStack.Count - 1);
            UpdateMesh(false);
        }
    }

    public void ApplyGlobalScale(float scaleFactor)
    {
        if (_vertices == null || _vertices.Length == 0) return;
        SaveUndoState();
        
        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i].y *= scaleFactor;
        }
        
        UpdateMesh();
    }

    public void ApplyGlobalSmooth(float smoothAmount)
    {
        if (_vertices == null || _vertices.Length == 0) return;
        SaveUndoState();
        
        int width = _terrainWidth + 1;
        int height = _terrainHeight + 1;
        
        Vector3[] tempVertices = (Vector3[])_vertices.Clone();
        
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int index = y * width + x;
                float sum = 0f;
                int count = 0;
                
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int neighborIndex = (y + dy) * width + (x + dx);
                        sum += tempVertices[neighborIndex].y;
                        count++;
                    }
                }
                
                float avgHeight = sum / count;
                _vertices[index].y = Mathf.Lerp(_vertices[index].y, avgHeight, smoothAmount);
            }
        }
        
        UpdateMesh();
    }

    public void ApplyGlobalRoughness(float roughnessAmount)
    {
        if (_vertices == null || _vertices.Length == 0) return;
        SaveUndoState();
        
        for (int i = 0; i < _vertices.Length; i++)
        {
            float noise = (Mathf.PerlinNoise(_vertices[i].x * 0.5f, _vertices[i].z * 0.5f) * 2f - 1f);
            _vertices[i].y += noise * roughnessAmount * 2f;
        }
        
        UpdateMesh();
    }

    public void ImportHeightData(TerrainHeightMapData terrainData)
    {
        int width = terrainData.width;
        int depth = terrainData.height;
        Vector3[] vertices = new Vector3[terrainData.heights.Length];
        Vector2[] uv = new Vector2[vertices.Length];

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = z * width + x;
                float height = terrainData.heights[index];
                vertices[index] = new Vector3(x * terrainData.scale, height, z * terrainData.scale);
                uv[index] = new Vector2((float)x / (width - 1), (float)z / (depth - 1));
            }
        }

        int terrainWidth = width - 1;
        int terrainHeight = depth - 1;
        int[] triangles = new int[terrainWidth * terrainHeight * 6];
        for (int ti = 0, vi = 0, z = 0; z < terrainHeight; z++, vi++)
        {
            for (int x = 0; x < terrainWidth; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + width;
                triangles[ti + 5] = vi + width + 1;
            }
        }

        Mesh mesh = new Mesh { name = "Imported Heightmap Terrain" };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        SetTerrainMesh(mesh, terrainWidth, terrainHeight, terrainData.scale);
    }

    public void ClearToFlat()
    {
        if (_vertices == null || _vertices.Length == 0) return;
        SaveUndoState();
        for (int i = 0; i < _vertices.Length; i++)
            _vertices[i].y = 0f;
        UpdateMesh();
    }

    private void ClearHistory()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        _strokeUndoSaved = false;
    }
}
