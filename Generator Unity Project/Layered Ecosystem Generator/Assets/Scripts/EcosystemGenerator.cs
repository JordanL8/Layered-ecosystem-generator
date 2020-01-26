using UnityEngine;

public class EcosystemGenerator : MonoBehaviour
{
    public Biome[] m_biomes;
    [Range(-10, 30)]
    public int m_averageAnnualTemperature;
    [Range(0, 45)]
    public int m_averageAnnualRainfall;
}
