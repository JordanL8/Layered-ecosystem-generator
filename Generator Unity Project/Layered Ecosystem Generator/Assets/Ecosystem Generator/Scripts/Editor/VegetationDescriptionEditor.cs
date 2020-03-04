using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(VegetationDescription))]
public class VegetationDescriptionEditor : Editor
{
    private VegetationDescription m_targetDescription;

    private void Awake()
    {
        m_targetDescription = (VegetationDescription)target;
    }
    private void OnEnable()
    {
        m_targetDescription = (VegetationDescription)target;
    }
    

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        RenderPlacementProperties();
        RenderVegetationProperties();
        if(EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(m_targetDescription);
        }   
    }

    private void RenderPlacementProperties()
    {
        EditorGUILayout.LabelField("Placement Model", EditorStyles.boldLabel);
        m_targetDescription.m_innerRadius = Mathf.Max(EditorGUILayout.FloatField("Inner Radius", m_targetDescription.m_innerRadius), 0.05f);
        m_targetDescription.m_outerRadius = Mathf.Max(EditorGUILayout.FloatField("Outer Radius", m_targetDescription.m_outerRadius), 0.05f);
    }

    private void RenderVegetationProperties()
    {
        EditorGUILayout.LabelField("Vegetation", EditorStyles.boldLabel);
        m_targetDescription.m_vegationType = (VegetationType)EditorGUILayout.EnumPopup("Vegetation Type", m_targetDescription.m_vegationType);
        m_targetDescription.m_variants = EditorGUILayout.IntSlider("Variants", m_targetDescription.m_variants, 1, 10);
        if(m_targetDescription.m_vegationType == VegetationType.LSystem)
        {
            RenderLSystemProperties();
        }
        else
        {
            RenderSpaceColonisationProperties();
        }
    }
    private void RenderLSystemProperties()
    {
        EditorGUILayout.LabelField("L-System", EditorStyles.boldLabel);
        m_targetDescription.m_lSystemRulesAsset = EditorGUILayout.ObjectField("Rules Asset", m_targetDescription.m_lSystemRulesAsset, typeof(LSystemGenerationRuleAsset), false) as LSystemGenerationRuleAsset;
        m_targetDescription.m_branchMaterial = EditorGUILayout.ObjectField("Branch Material", m_targetDescription.m_branchMaterial, typeof(Material), false) as Material;
        m_targetDescription.m_leafPrefab = EditorGUILayout.ObjectField("Leaf Prefab", m_targetDescription.m_leafPrefab, typeof(GameObject), false) as GameObject;
        m_targetDescription.m_leafMaterial = EditorGUILayout.ObjectField("Leaf Material", m_targetDescription.m_leafMaterial, typeof(Material), false) as Material;
    }

    private void RenderSpaceColonisationProperties()
    {
        EditorGUILayout.LabelField("Space Colonisation", EditorStyles.boldLabel);
        m_targetDescription.m_spaceColonisationTreePrefab = EditorGUILayout.ObjectField("Tree Prefab", m_targetDescription.m_spaceColonisationTreePrefab, typeof(GameObject), false) as GameObject;
    }
}
