using UnityEngine;

[CreateAssetMenu(fileName = "Biome", menuName = "Ecosystem Generator/Biome", order = 0)]
public class Biome : ScriptableObject
{
    [Header("Graph")]
    public string m_biomeName = "New Biome";
    public Vector2Int[] m_temperatureAndRainfallSamplePoints;
    public Color m_colorOnGraph = Color.white;

    [Header("Generation")]
    public GroundLayer m_groundLayer;
    public VegetationLayer[] m_vegetationLayers;
    public float m_sparcity = 1.0f;
}
