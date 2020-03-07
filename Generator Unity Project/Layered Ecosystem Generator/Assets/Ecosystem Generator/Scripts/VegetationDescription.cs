using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VegetationType
{
    LSystem,
    SpaceColonisation
}

[CreateAssetMenu(fileName = "Vegetation Description", menuName = "Ecosystem Generator/Vegetation Description", order = 2)]
public class VegetationDescription : ScriptableObject
{
    public float m_innerRadius = 0.5f;
    public float m_outerRadius = 5.0f;

    public float m_sparcity = 1.0f;
    
    public VegetationType m_vegationType = VegetationType.LSystem;

    public int m_variants = 1;
    // L-System properties
    public LSystemGenerationRuleAsset m_lSystemRulesAsset;
    public Material m_branchMaterial;
    public GameObject m_leafPrefab;
    public Material m_leafMaterial;

    // Space Colonisation properties
    public GameObject m_spaceColonisationTreePrefab;
}
