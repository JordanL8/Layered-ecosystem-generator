using UnityEngine;

[System.Serializable]
public class TemperatureRainfallSamplePoints
{
    public int m_temperature;
    public int m_rainfall;

    public Vector2 ToVector2()
    {
        return new Vector2(m_temperature, m_rainfall);
    }
}

[CreateAssetMenu(fileName = "Biome", menuName = "Ecosystem Generator/Biome", order = 0)]
public class Biome : ScriptableObject
{
    public string m_biomeName = "New Biome";
    public Vector2Int[] m_temperatureAndRainfallSamplePoints;
    public Color m_colorOnGraph = Color.white;
    public Layer[] m_layers;   
}
