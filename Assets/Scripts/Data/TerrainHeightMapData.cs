using System;

[Serializable]
public class TerrainHeightMapData
{
    public int width;
    public int height;
    public float scale = 1f;
    public float maxHeight;
    public float[] heights;
}
