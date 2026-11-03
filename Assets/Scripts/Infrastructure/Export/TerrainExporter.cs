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
        TerrainData data = new TerrainData();
        data.vertices = mesh.vertices;
        data.triangles = mesh.triangles;
        data.uv = mesh.uv;
        data.normals = mesh.normals;

        return JsonUtility.ToJson(data, true);
    }

    [Serializable]
    public class TerrainData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uv;
        public Vector3[] normals;
    }

    public void ImportFromPath(string path)
    {
        if (!File.Exists(path))
        {
            OnExportFailed?.Invoke($"文件不存在：{path}");
            return;
        }

        try
        {
            string content = File.ReadAllText(path);
            TerrainData data = JsonUtility.FromJson<TerrainData>(content);

            TerrainEditor terrainBrush = FindObjectOfType<TerrainEditor>();
            if (terrainBrush == null)
            {
                OnExportFailed?.Invoke("TerrainEditor组件未找到！");
                return;
            }

            Mesh mesh = terrainBrush.GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null)
            {
                OnExportFailed?.Invoke("地形网格数据未找到！");
                return;
            }

            mesh.vertices = data.vertices;
            mesh.triangles = data.triangles;
            mesh.uv = data.uv;
            mesh.normals = data.normals;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            terrainBrush.GetComponent<MeshCollider>().sharedMesh = mesh;
            terrainBrush.SyncVerticesFromMesh();

            OnExportSuccess?.Invoke($"地形数据已成功导入！\n文件路径：{path}");
        }
        catch (Exception e)
        {
            OnExportFailed?.Invoke($"导入失败：{e.Message}");
        }
    }
}