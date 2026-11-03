
using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrainGrowthPreset", menuName = "Terrain/Growth Preset", order = 1)]
public class TerrainGrowthPreset : ScriptableObject
{
    [Header("Core Growth Parameters")]
    [Tooltip("The percentage of height change per unit of time. 0.1% to 100%")]
    [Range(0.001f, 1.0f)]
    public float growthRate = 0.1f;

    [Tooltip("The 3D bounding box for the growth area.")]
    public GrowthRange growthRange = new GrowthRange();

    [Tooltip("The smoothness level of the growth, from 1 (sharp) to 10 (very smooth).")]
    [Range(1, 10)]
    public int smoothness = 5;

    [Header("Noise Settings")]
    [Tooltip("The scale of the Perlin noise. Smaller values create larger features.")]
    public float noiseScale = 0.1f;

    [Tooltip("The strength of the Perlin noise. Determines how much the noise affects the growth.")]
    public float noiseStrength = 0.5f;

    [Header("Additional Parameters")]
    [Tooltip("Seed for random number generation to ensure reproducible results.")]
    public int randomSeed = 12345;

    [Tooltip("The normalized vector indicating the primary direction of growth.")]
    public Vector3 growthDirection = Vector3.up;

    [Tooltip("A curve to scale the height of the terrain growth.")]
    public AnimationCurve heightScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Tooltip("Enable or disable Level of Detail attenuation for growth.")]
    public bool enableLOD = false;

    [System.Serializable]
    public struct GrowthRange
    {
        public Vector3 min;
        public Vector3 max;

        public void Validate()
        {
            min.x = Mathf.Min(min.x, max.x);
            min.y = Mathf.Min(min.y, max.y);
            min.z = Mathf.Min(min.z, max.z);

            max.x = Mathf.Max(min.x, max.x);
            max.y = Mathf.Max(min.y, max.y);
            max.z = Mathf.Max(min.z, max.z);
        }
    }

    private void OnValidate()
    {
        growthRange.Validate();
        growthDirection.Normalize();
    }
}
