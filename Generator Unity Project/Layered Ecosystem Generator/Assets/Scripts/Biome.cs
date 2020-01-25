using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome", menuName = "Ecosystem Generator/Biome", order = 0)]
public class Biome : ScriptableObject
{
    public string m_BiomeName = "New Biome";
    public Vector2Int m_TemperatureRange;
    [Min(0)]
    public Vector2Int m_RainfallRange;
    public Color m_ColorOnGraph = Color.white;
    public Layer[] m_Layers;   
}
