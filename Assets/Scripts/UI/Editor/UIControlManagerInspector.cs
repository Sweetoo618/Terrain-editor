using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIControlManager))]
public class UIControlManagerInspector : Editor
{
    private UIControlManager manager;

    private void OnEnable() => manager = (UIControlManager)target;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.HelpBox("Runtime parameter UI is disabled. Select TerrainGenerator or TerrainConfig to edit the terrain shape.", MessageType.Info);
        DrawPropertiesExcluding(serializedObject, "m_Script");
        bool settingsChanged = serializedObject.ApplyModifiedProperties();
        if (settingsChanged) manager.ApplySettings();

        if (manager.config == null)
        {
            EditorGUILayout.HelpBox("Assign a TerrainConfig asset before generating terrain.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Terrain Operations", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Generate")) Generate();
            if (GUILayout.Button("Regenerate")) Regenerate();
            if (GUILayout.Button("Clear")) Clear();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Import JSON")) Import();
            if (GUILayout.Button("Export OBJ")) Export();
        }
    }

    private void Generate()
    {
        manager.GenerateTerrain();
        MarkDirty();
    }

    private void Regenerate()
    {
        manager.RegenerateTerrain();
        MarkDirty();
    }

    private void Clear()
    {
        manager.ClearTerrain();
        MarkDirty();
    }

    private void Import()
    {
        string path = EditorUtility.OpenFilePanel("Import terrain JSON", "", "json");
        if (!string.IsNullOrEmpty(path) && manager.ImportTerrain(path)) MarkDirty();
    }

    private void Export()
    {
        string path = EditorUtility.SaveFilePanel("Export terrain OBJ", "", "terrain", "obj");
        if (!string.IsNullOrEmpty(path)) manager.ExportTerrainToPath(path);
    }

    private void MarkDirty()
    {
        EditorUtility.SetDirty(manager);
        if (manager.config != null) EditorUtility.SetDirty(manager.config);
        SceneView.RepaintAll();
    }
}
