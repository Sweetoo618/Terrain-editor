using UnityEngine;
using System;
using System.IO;
using System.Text;

public class TerrainExporter : MonoBehaviour
{
    [Header("Export Settings")]
    [Tooltip("默认导出文件名")]
    public string defaultFileName = "terrain";

    [Tooltip("导出格式")]
    public ExportFormat format = ExportFormat.OBJ;

    public enum ExportFormat
    {
        OBJ,
        JSON
    }

    public event Action<string> OnExportSuccess;
    public event Action<string> OnExportFailed;

    public void ExportToDesktop(string filename = null)
    {
        if (filename == null)
            filename = defaultFileName;

        TerrainEditor terrainBrush = FindObjectOfType<TerrainEditor>();
        if (terrainBrush == null)
        {
            OnExportFailed?.Invoke("TerrainEditor组件未找到！");
            Debug.LogError("[TerrainExporter] TerrainEditor not found!");
            return;
        }

        Mesh mesh = terrainBrush.GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null)
        {
            OnExportFailed?.Invoke("地形网格数据未找到！");
            Debug.LogError("[TerrainExporter] Mesh not found!");
            return;
        }

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string extension = format == ExportFormat.OBJ ? ".obj" : ".json";
        string fullPath = Path.Combine(desktopPath, filename + extension);

        try
        {
            string content = format == ExportFormat.OBJ ? MeshToObj(mesh) : MeshToJson(mesh);
            File.WriteAllText(fullPath, content);

            string successMessage = $"地形数据已成功导出到桌面！\n文件路径：{fullPath}";
            OnExportSuccess?.Invoke(successMessage);
            Debug.Log($"[TerrainExporter] Terrain exported to desktop: {fullPath}");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("导出成功", successMessage, "确定");
#endif
        }
        catch (Exception e)
        {
            string errorMessage = $"无法导出地形数据！\n错误原因：{e.Message}\n请检查文件权限或尝试其他位置。";
            OnExportFailed?.Invoke(errorMessage);
            Debug.LogError($"[TerrainExporter] Failed to export terrain: {e.Message}");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("导出失败", errorMessage, "确定");
#endif
        }
    }

    public void ExportToPath(string path)
    {
        Debug.Log($"[TerrainExporter] ExportToPath called with path: {path}");
        
        TerrainEditor terrainBrush = FindObjectOfType<TerrainEditor>();
        if (terrainBrush == null)
        {
            OnExportFailed?.Invoke("TerrainEditor组件未找到！");
            Debug.LogError("[TerrainExporter] TerrainEditor not found!");
            return;
        }

        Mesh mesh = terrainBrush.GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null)
        {
            OnExportFailed?.Invoke("地形网格数据未找到！");
            Debug.LogError("[TerrainExporter] Mesh not found!");
            return;
        }

        Debug.Log($"[TerrainExporter] Mesh found: {mesh.vertexCount} vertices, {mesh.triangles.Length / 3} triangles");

        try
        {
            string extension = format == ExportFormat.OBJ ? ".obj" : ".json";
            string fullPath = Path.ChangeExtension(path, extension);
            
            Debug.Log($"[TerrainExporter] Writing to: {fullPath}");

            string content = format == ExportFormat.OBJ ? MeshToObj(mesh) : MeshToJson(mesh);
            File.WriteAllText(fullPath, content);

            string successMessage = $"地形数据已成功导出！\n文件路径：{fullPath}";
            OnExportSuccess?.Invoke(successMessage);
            Debug.Log($"[TerrainExporter] Export successful: {fullPath}");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("导出成功", successMessage, "确定");
#endif
        }
        catch (Exception e)
        {
            string errorMessage = $"导出失败：{e.Message}";
            OnExportFailed?.Invoke(errorMessage);
            Debug.LogError($"[TerrainExporter] Export failed: {e.Message}");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("导出失败", errorMessage, "确定");
#endif
        }
    }

    private string MeshToObj(Mesh mesh)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("# Terrain Editor Export");
        sb.AppendLine($"# Export Date: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        sb.AppendLine($"# Vertices: {mesh.vertexCount}");
        sb.AppendLine($"# Triangles: {mesh.triangles.Length / 3}");
        sb.AppendLine();

        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            sb.AppendLine($"v {vertices[i].x:F6} {vertices[i].y:F6} {vertices[i].z:F6}");
        }

        sb.AppendLine();

        Vector2[] uvs = mesh.uv;
        if (uvs != null && uvs.Length > 0)
        {
            for (int i = 0; i < uvs.Length; i++)
            {
                sb.AppendLine($"vt {uvs[i].x:F6} {uvs[i].y:F6}");
            }
        }

        sb.AppendLine();

        Vector3[] normals = mesh.normals;
        if (normals != null && normals.Length > 0)
        {
            for (int i = 0; i < normals.Length; i++)
            {
                sb.AppendLine($"vn {normals[i].x:F6} {normals[i].y:F6} {normals[i].z:F6}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("o Terrain");
        sb.AppendLine("g Terrain");

        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int idx1 = triangles[i] + 1;
            int idx2 = triangles[i + 1] + 1;
            int idx3 = triangles[i + 2] + 1;

            if (uvs != null && uvs.Length > 0 && normals != null && normals.Length > 0)
            {
                sb.AppendLine($"f {idx1}/{idx1}/{idx1} {idx2}/{idx2}/{idx2} {idx3}/{idx3}/{idx3}");
            }
            else
            {
                sb.AppendLine($"f {idx1} {idx2} {idx3}");
            }
        }

        return sb.ToString();
    }

    private string MeshToJson(Mesh mesh)
    {
        TerrainMeshData data = new TerrainMeshData();
        data.vertices = mesh.vertices;
        data.triangles = mesh.triangles;
        data.uv = mesh.uv;
        data.normals = mesh.normals;
        TerrainEditor terrainEditor = FindObjectOfType<TerrainEditor>();
        if (terrainEditor != null)
        {
            data.width = terrainEditor.TerrainWidth;
            data.depth = terrainEditor.TerrainHeight;
            data.cellSize = terrainEditor.terrainScale;
        }

        return JsonUtility.ToJson(data, true);
    }

    [Serializable]
    public class TerrainMeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uv;
        public Vector3[] normals;
        public int width;
        public int depth;
        public float cellSize = 1f;
    }

    public bool ImportFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            OnExportFailed?.Invoke($"文件不存在：{path}");
            return false;
        }

        try
        {
            string content = File.ReadAllText(path);
            TerrainEditor terrainBrush = FindObjectOfType<TerrainEditor>();
            if (terrainBrush == null)
            {
                OnExportFailed?.Invoke("TerrainEditor组件未找到！");
                return false;
            }

            TerrainMeshData meshData = JsonUtility.FromJson<TerrainMeshData>(content);
            if (meshData != null && meshData.vertices != null && meshData.vertices.Length > 0)
            {
                if (!TryGetGridSize(meshData, out int width, out int depth) ||
                    !IsValidMeshData(meshData) ||
                    !TryGetGridSpacing(meshData.vertices, width, depth, out float cellSize))
                {
                    OnExportFailed?.Invoke("网格 JSON 格式无效，顶点尺寸或三角形索引不正确。");
                    return false;
                }

                Mesh importedMesh = new Mesh { name = "Imported Terrain" };
                importedMesh.vertices = meshData.vertices;
                importedMesh.triangles = meshData.triangles;
                if (meshData.uv != null && meshData.uv.Length == meshData.vertices.Length)
                    importedMesh.uv = meshData.uv;
                importedMesh.RecalculateNormals();
                importedMesh.RecalculateBounds();
                terrainBrush.SetTerrainMesh(importedMesh, width, depth, cellSize);
            }
            else
            {
                TerrainHeightMapData heightData = JsonUtility.FromJson<TerrainHeightMapData>(content);
                if (!IsValidHeightMap(heightData))
                {
                    OnExportFailed?.Invoke("JSON 不包含有效的地形网格或高度图数据。");
                    return false;
                }

                if (heightData.scale <= 0f)
                    heightData.scale = 1f;
                terrainBrush.ImportHeightData(heightData);
            }

            OnExportSuccess?.Invoke($"地形数据已成功导入！\n文件路径：{path}");
            Debug.Log($"[TerrainExporter] Terrain imported from: {path}");
            return true;
        }
        catch (Exception e)
        {
            OnExportFailed?.Invoke($"导入失败：{e.Message}");
            Debug.LogError($"[TerrainExporter] Failed to import terrain: {e.Message}");
            return false;
        }
    }

    private static bool IsValidMeshData(TerrainMeshData data)
    {
        if (data.triangles == null || data.triangles.Length == 0 || data.triangles.Length % 3 != 0)
            return false;
        if (data.uv != null && data.uv.Length > 0 && data.uv.Length != data.vertices.Length)
            return false;

        for (int i = 0; i < data.vertices.Length; i++)
        {
            Vector3 vertex = data.vertices[i];
            if (float.IsNaN(vertex.x) || float.IsNaN(vertex.y) || float.IsNaN(vertex.z) ||
                float.IsInfinity(vertex.x) || float.IsInfinity(vertex.y) || float.IsInfinity(vertex.z))
                return false;
        }

        for (int i = 0; i < data.triangles.Length; i++)
        {
            if (data.triangles[i] < 0 || data.triangles[i] >= data.vertices.Length)
                return false;
        }

        return true;
    }

    private static bool TryGetGridSize(TerrainMeshData data, out int width, out int depth)
    {
        if (data.width > 0 && data.depth > 0 && (data.width + 1) * (data.depth + 1) == data.vertices.Length)
        {
            width = data.width;
            depth = data.depth;
            return true;
        }

        int side = Mathf.RoundToInt(Mathf.Sqrt(data.vertices.Length));
        if (side * side == data.vertices.Length)
        {
            width = side - 1;
            depth = side - 1;
            return width > 0;
        }

        width = 0;
        depth = 0;
        return false;
    }

    private static bool TryGetGridSpacing(Vector3[] vertices, int width, int depth, out float cellSize)
    {
        cellSize = 0f;
        if (vertices.Length != (width + 1) * (depth + 1))
            return false;

        Vector3 origin = vertices[0];
        float spacingX = vertices[1].x - origin.x;
        float spacingZ = vertices[width + 1].z - origin.z;
        float tolerance = Mathf.Max(0.001f, Mathf.Max(spacingX, spacingZ) * 0.001f);
        if (spacingX <= 0f || spacingZ <= 0f || Mathf.Abs(spacingX - spacingZ) > tolerance)
            return false;

        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                int index = z * (width + 1) + x;
                if (Mathf.Abs(vertices[index].x - (origin.x + x * spacingX)) > tolerance ||
                    Mathf.Abs(vertices[index].z - (origin.z + z * spacingZ)) > tolerance)
                    return false;

                vertices[index].x -= origin.x;
                vertices[index].z -= origin.z;
            }
        }

        cellSize = (spacingX + spacingZ) * 0.5f;
        return true;
    }

    private static bool IsValidHeightMap(TerrainHeightMapData data)
    {
        if (data == null || data.width < 2 || data.height < 2 || data.heights == null ||
            data.heights.Length != data.width * data.height)
            return false;

        for (int i = 0; i < data.heights.Length; i++)
        {
            if (float.IsNaN(data.heights[i]) || float.IsInfinity(data.heights[i]))
                return false;
        }

        return true;
    }
}
