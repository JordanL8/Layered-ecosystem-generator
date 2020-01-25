using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TemperatureRainfallSamplePoints
{
    public int m_Temperature;
    public int m_Rainfall;
}

[CreateAssetMenu(fileName = "Biome", menuName = "Ecosystem Generator/Biome", order = 0)]
public class Biome : ScriptableObject
{
    public string m_BiomeName = "New Biome";
    public TemperatureRainfallSamplePoints[] m_TemperatureAndRainfallSamplePoints;
    public Color m_ColorOnGraph = Color.white;
    public Layer[] m_Layers;   
}
