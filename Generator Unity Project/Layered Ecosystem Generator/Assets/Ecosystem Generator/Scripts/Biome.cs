using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CreateAssetMenu(fileName = "Biome", menuName = "Ecosystem Generator/Biome", order = 0)]
public class Biome : ScriptableObject
{
    [Header("General")]
    public string m_biomeName = "New Biome";

    [Header("Generation")]
    public GroundLayer m_groundLayer;
    public List<VegetationLayer> m_vegetationLayers;
    public float m_sparcity = 1.0f;

    [Header("Graph")]
    public Vector2Int[] m_temperatureAndRainfallSamplePoints;
    public Color m_colorOnGraph = Color.white;



    [HideInInspector] [SerializeField] public AnimBool m_hideGraph = new AnimBool(true);
}


