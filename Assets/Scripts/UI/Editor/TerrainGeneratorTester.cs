using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class TerrainGeneratorTester : EditorWindow
{
    private int _testWidth = 64;
    private int _testDepth = 64;
    private bool _runComprehensiveTest = false;
    private List<string> _testResults = new List<string>();
    
    [MenuItem("Tools/Terrain Generator Tester")]
    public static void ShowWindow()
    {
        GetWindow<TerrainGeneratorTester>("Terrain Generator Tester");
    }

    void OnGUI()
    {
        GUILayout.Label("Terrain Generator Comprehensive Test", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        GUILayout.Label("Single Test Parameters:", EditorStyles.label);
        _testWidth = EditorGUILayout.IntField("Width", _testWidth);
        _testDepth = EditorGUILayout.IntField("Depth", _testDepth);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Run Single Test", GUILayout.Height(30)))
        {
            RunSingleTest(_testWidth, _testDepth);
        }
        
        EditorGUILayout.Space();
        
        _runComprehensiveTest = EditorGUILayout.Toggle("Run Comprehensive Test", _runComprehensiveTest);
        if (_runComprehensiveTest && GUILayout.Button("Run All Tests", GUILayout.Height(30)))
        {
            RunComprehensiveTests();
        }
        
        EditorGUILayout.Space();
        
        GUILayout.Label("Test Results:", EditorStyles.boldLabel);
        using (var scroll = new EditorGUILayout.ScrollViewScope(Vector2.zero, GUILayout.Height(300)))
        {
            foreach (string result in _testResults)
            {
                GUILayout.Label(result);
            }
        }
        
        if (GUILayout.Button("Clear Results"))
        {
            _testResults.Clear();
        }
    }

    void RunSingleTest(int width, int depth)
    {
        _testResults.Clear();
        _testResults.Add($"=== Testing terrain size: {width} x {depth} ===");
        
        // 验证参数有效性
        if (width <= 0 || depth <= 0)
        {
            _testResults.Add("❌ Error: Width and depth must be positive");
            return;
        }
        
        // 检查顶点数量限制
        int vertexCount = (width + 1) * (depth + 1);
        _testResults.Add($"Vertex count: {vertexCount:N0}");
        
        if (vertexCount > 65000)
        {
            _testResults.Add("⚠️ Warning: Vertex count exceeds recommended limit (65,000)");
        }
        
        // 检查三角形数量
        int triangleCount = width * depth * 6;
        _testResults.Add($"Triangle count: {triangleCount:N0}");
        
        // 计算理论尺寸
        float cellSize = 1f; // 默认单元大小
        float actualWidth = width * cellSize;
        float actualDepth = depth * cellSize;
        _testResults.Add($"Theoretical terrain size: {actualWidth:F2} x {actualDepth:F2} units");
        
        // 检查索引计算
        int lastVertexIndex = (depth) * (width + 1) + width;
        _testResults.Add($"Last vertex index: {lastVertexIndex:N0}");
        
        // 验证三角形索引范围
        int maxTriangleIndex = triangleCount / 3;
        _testResults.Add($"Max triangle index range: 0 to {maxTriangleIndex:N0}");
        
        if (maxTriangleIndex > vertexCount)
        {
            _testResults.Add("❌ ERROR: Triangle indices exceed vertex count!");
        }
        else
        {
            _testResults.Add("✅ Triangle indices are valid");
        }
        
        // 检查内存估算
        long vertexMemory = (long)vertexCount * 3 * 4; // Vector3 = 3 floats = 12 bytes
        long triangleMemory = (long)triangleCount * 4; // int = 4 bytes
        _testResults.Add($"Estimated memory: Vertices={vertexMemory / 1024:N0} KB, Triangles={triangleMemory / 1024:N0} KB");
        
        _testResults.Add("=== Test completed ===");
        Repaint();
    }

    void RunComprehensiveTests()
    {
        _testResults.Clear();
        _testResults.Add("=== Comprehensive Terrain Generator Test Suite ===");
        
        // 测试用例定义
        int[] testSizes = { 32, 64, 128, 256, 512, 1024 };
        string[] testLabels = { "Small", "Medium", "Large", "XL", "XXL", "XXXL" };
        
        for (int i = 0; i < testSizes.Length; i++)
        {
            int size = testSizes[i];
            _testResults.Add($"");
            _testResults.Add($"--- {testLabels[i]} Size Test: {size}x{size} ---");
            
            int vertexCount = (size + 1) * (size + 1);
            int triangleCount = size * size * 6;
            
            _testResults.Add($"Vertex count: {vertexCount:N0}");
            _testResults.Add($"Triangle count: {triangleCount:N0}");
            
            // 检查限制
            bool vertexWarning = vertexCount > 65000;
            bool triangleWarning = triangleCount > 200000;
            
            if (vertexWarning)
                _testResults.Add("⚠️ Vertex count exceeds Unity's recommended limit");
            if (triangleWarning)
                _testResults.Add("⚠️ Triangle count may cause performance issues");
            
            // 检查索引溢出
            bool indexOverflow = triangleCount / 3 > vertexCount;
            if (indexOverflow)
                _testResults.Add("❌ INDEX OVERFLOW: Triangle indices exceed vertex range!");
            else
                _testResults.Add("✅ Index validation passed");
            
            // 噪声参数验证
            float noiseCoord = size * 0.1f; // typical frequency
            if (noiseCoord > 10000f)
                _testResults.Add("⚠️ Perlin noise coordinates may exceed optimal range");
            
            _testResults.Add($"--- {testLabels[i]} Test {(indexOverflow ? "FAILED" : "PASSED")} ---");
        }
        
        // 边界条件测试
        _testResults.Add("");
        _testResults.Add("--- Edge Case Tests ---");
        
        // 非正方形测试
        int[] widthTests = { 64, 128, 256 };
        int[] depthTests = { 32, 64, 128 };
        
        foreach (int w in widthTests)
        {
            foreach (int d in depthTests)
            {
                if (w != d)
                {
                    int vertexCount = (w + 1) * (d + 1);
                    int triangleCount = w * d * 6;
                    bool valid = triangleCount / 3 <= vertexCount;
                    _testResults.Add($"{w}x{d}: Vertices={vertexCount:N0}, Triangles={triangleCount:N0} - {(valid ? "OK" : "INVALID")}");
                }
            }
        }
        
        _testResults.Add("");
        _testResults.Add("=== Test Suite Completed ===");
        Repaint();
    }
}