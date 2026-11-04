using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorInspector : Editor
{
    private SerializedProperty terrainConfig;
    private bool showGrid = true;
    private bool showShape = true;

    private void OnEnable() => terrainConfig = serializedObject.FindProperty("terrainConfig");

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("Terrain Generator", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(terrainConfig, new GUIContent("Configuration Asset"));
        TerrainGenerator generator = (TerrainGenerator)target;
        TerrainConfig.GeneratorConfig data = generator.terrainConfig != null ? generator.terrainConfig.generator : null;
        if (data == null)
        {
            EditorGUILayout.HelpBox("Assign a TerrainConfig asset to edit and preview terrain shape.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.Space();
        showGrid = EditorGUILayout.Foldout(showGrid, "Grid", true, EditorStyles.foldoutHeader);
        if (showGrid)
        {
            EditorGUI.indentLevel++;
            Draw("width", "Width");
            Draw("depth", "Depth");
            Draw("cellSize", "Cell Size");
            Draw("heightRange", "Height Range");
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Terrain Type", EditorStyles.boldLabel);
        Draw("terrainType", "Type");
        showShape = EditorGUILayout.Foldout(showShape, "Shape Parameters", true, EditorStyles.foldoutHeader);
        if (showShape)
        {
            EditorGUI.indentLevel++;
            switch (data.terrainType)
            {
                case TerrainConfig.TerrainType.Plains:
                    Draw("plainsBaseHeight", "Base Height"); Draw("plainsRelief", "Relief"); Draw("plainsProfile", "Relief Profile"); break;
                case TerrainConfig.TerrainType.Hills:
                    Draw("hillsBaseHeight", "Base Height"); Draw("hillsAmplitude", "Amplitude"); Draw("hillsFrequency", "Frequency"); Draw("hillsProfile", "Slope Profile"); break;
                case TerrainConfig.TerrainType.Mountains:
                    Draw("mountainsBaseHeight", "Base Height"); Draw("mountainsPeakHeight", "Peak Height"); Draw("mountainsSharpness", "Sharpness"); Draw("mountainsProfile", "Peak Profile"); break;
                case TerrainConfig.TerrainType.Valley:
                    Draw("valleyRimHeight", "Rim Height"); Draw("valleyFloorHeight", "Floor Height"); Draw("valleyWidth", "Valley Width"); Draw("valleyProfile", "Side Profile"); break;
                case TerrainConfig.TerrainType.Plateau:
                    Draw("plateauBaseHeight", "Base Height"); Draw("plateauHeight", "Plateau Height"); Draw("plateauWidth", "Plateau Width"); Draw("plateauEdgeProfile", "Edge Profile"); break;
                case TerrainConfig.TerrainType.Canyon:
                    Draw("canyonRimHeight", "Rim Height"); Draw("canyonFloorHeight", "Floor Height"); Draw("canyonWidth", "Canyon Width"); Draw("canyonCurvature", "Curvature"); Draw("canyonWallProfile", "Wall Profile"); break;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Generate / Preview")) { generator.Generate(); EditorUtility.SetDirty(generator); SceneView.RepaintAll(); }
            if (GUILayout.Button("Clear")) { generator.Clear(); EditorUtility.SetDirty(generator); }
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void Draw(string name, string label)
    {
        TerrainGenerator generator = (TerrainGenerator)target;
        SerializedObject asset = new SerializedObject(generator.terrainConfig);
        SerializedProperty property = asset.FindProperty("generator").FindPropertyRelative(name);
        if (property != null) EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        if (asset.ApplyModifiedProperties()) EditorUtility.SetDirty(generator.terrainConfig);
    }
}
