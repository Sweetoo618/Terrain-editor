
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGrowthManager))]
public class TerrainGrowthManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TerrainGrowthManager manager = (TerrainGrowthManager)target;

        if (GUILayout.Button("Update Terrain"))
        {
            if (Application.isPlaying)
            {
                manager.UpdateTerrainGrowth(Time.deltaTime);
            }
            else
            {
                // In editor mode, we can simulate a delta time
                manager.Initialize(); // Ensure initialization
                manager.UpdateTerrainGrowth(0.1f); // Simulate 0.1s of growth
            }
        }

        // Placeholder for the growth rate chart
        EditorGUILayout.LabelField("Growth Rate Chart", EditorStyles.boldLabel);
        Rect chartRect = EditorGUILayout.GetControlRect(false, 100);
        EditorGUI.DrawRect(chartRect, new Color(0.2f, 0.2f, 0.2f));
        if (GUILayout.Button("Reset Terrain"))
        {
            if (EditorUtility.DisplayDialog("Reset Terrain", "Are you sure you want to reset the terrain to its initial state?", "Yes", "No"))
            {
                manager.Initialize(); // Make sure we have the original vertices
                manager.ResetTerrain();
            }
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Export Preset"))
        {
            if (manager.growthPreset != null)
            {
                string path = EditorUtility.SaveFilePanel("Export Growth Preset", "", manager.growthPreset.name + ".json", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    string json = JsonUtility.ToJson(manager.growthPreset, true);
                    System.IO.File.WriteAllText(path, json);
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Export Error", "No growth preset assigned.", "OK");
            }
        }

        if (GUILayout.Button("Import Preset"))
        {
            string path = EditorUtility.OpenFilePanel("Import Growth Preset", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                string json = System.IO.File.ReadAllText(path);
                TerrainGrowthPreset newPreset = (TerrainGrowthPreset)ScriptableObject.CreateInstance<TerrainGrowthPreset>();
                JsonUtility.FromJsonOverwrite(json, newPreset);
                
                string savePath = EditorUtility.SaveFilePanelInProject("Save Imported Preset", "New Imported Preset", "asset", "Please enter a file name to save the preset to.");
                if (!string.IsNullOrEmpty(savePath))
                {
                    AssetDatabase.CreateAsset(newPreset, savePath);
                    manager.growthPreset = newPreset;
                    AssetDatabase.SaveAssets();
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = newPreset;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
