using UnityEngine;

public class EcosystemGenerator : MonoBehaviour
{
    [Header("Available Biomes")]
    public Biome[] m_biomes;
    [Header("Ecosystem Properties")]
    [Range(-15, 30)]
    public int m_averageAnnualTemperature = 9;
    [Range(0, 450)]
    public int m_averageAnnualRainfall = 125;
}
