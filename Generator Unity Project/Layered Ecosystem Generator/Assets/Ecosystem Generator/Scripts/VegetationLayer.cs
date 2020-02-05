using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Vegetation Layer", menuName = "Ecosystem Generator/Vegetation Layer", order = 1)]
public class VegetationLayer : ScriptableObject
{
    public VegetationDescription[] m_vegetationInLayer;
    public GameObject[] TEMPPREFABS;
}
