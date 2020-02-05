using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AnimatedValues;

public class EcosystemGeneratorProperties : ScriptableObject
{
    // User properties.
    [HideInInspector] [SerializeField] public List<Biome> m_biomes;

    // Ecosystem properties.
    [Range(-15, 30), Tooltip("Controls the average annual temperature of your ecosystem.")]
    [HideInInspector] [SerializeField] public int m_averageAnnualTemperature = 9;

    [Range(0, 450), Tooltip("Controls the average annual rainfall of your ecosystem.")]
    [HideInInspector] [SerializeField] public int m_averageAnnualRainfall = 125;

    // Generation Properties.
    [Range(0, 90), Tooltip("Controls the maximum incline that the generator places vegetation. If the angle from the ground's normal to Vector3.up is greater than this value, the generator does not place vegetation.")]
    [HideInInspector] [SerializeField] public float m_maximumIncline = 10.0f;
    [HideInInspector] [SerializeField] public float m_checkHeightOffset = 5.0f;
    [HideInInspector] [SerializeField] public bool m_checkForEncroachment = false;
    [HideInInspector] [SerializeField] public GameObject m_targetGameObject;

    // Window Properties.
    [HideInInspector] [SerializeField] public AnimBool m_hideBiomeSection = new AnimBool(true);
}
