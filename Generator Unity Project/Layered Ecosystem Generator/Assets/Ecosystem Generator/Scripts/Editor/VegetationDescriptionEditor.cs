using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(VegetationDescription))]
public class VegetationDescriptionEditor : Editor
{
    private VegetationDescription m_targetDescription;
    
    private void OnEnable()
    {
        if (target)
        {
            m_targetDescription = (VegetationDescription)target;
        }
    }
    
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        RenderProperties(m_targetDescription);
        
    }

    public void RenderProperties(VegetationDescription customTarget)
    {
        if(customTarget == null)
        {
            return;
        }
        EditorGUILayout.LabelField(m_targetDescription.name, new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
        EditorGUI.BeginChangeCheck();
        RenderPlacementProperties(customTarget);
        RenderVegetationProperties(customTarget);
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(customTarget);
        }
    }

    private void RenderPlacementProperties(VegetationDescription customTarget)
    {
        EditorGUILayout.LabelField("Placement Model", EditorStyles.boldLabel);
        customTarget.m_innerRadius = Mathf.Max(EditorGUILayout.FloatField("Inner Radius", customTarget.m_innerRadius), 0.05f);
        customTarget.m_outerRadius = Mathf.Max(EditorGUILayout.FloatField("Outer Radius", customTarget.m_outerRadius), 0.05f);
    }

    private void RenderVegetationProperties(VegetationDescription customTarget)
    {
        EditorGUILayout.LabelField("Vegetation", EditorStyles.boldLabel);
        customTarget.m_vegationType = (VegetationType)EditorGUILayout.EnumPopup("Vegetation Type", customTarget.m_vegationType);
        customTarget.m_variants = EditorGUILayout.IntSlider("Variants", customTarget.m_variants, 1, 10);
        if(customTarget.m_vegationType == VegetationType.LSystem)
        {
            RenderLSystemProperties(customTarget);
        }
        else
        {
            RenderSpaceColonisationProperties(customTarget);
        }
    }
    private void RenderLSystemProperties(VegetationDescription customTarget)
    {
        EditorGUILayout.LabelField("L-System", EditorStyles.boldLabel);
        customTarget.m_lSystemRulesAsset = EditorGUILayout.ObjectField("Rules Asset", customTarget.m_lSystemRulesAsset, typeof(LSystemGenerationRuleAsset), false) as LSystemGenerationRuleAsset;
        customTarget.m_branchMaterial = EditorGUILayout.ObjectField("Branch Material", customTarget.m_branchMaterial, typeof(Material), false) as Material;
        customTarget.m_leafPrefab = EditorGUILayout.ObjectField("Leaf Prefab", customTarget.m_leafPrefab, typeof(GameObject), false) as GameObject;
        customTarget.m_leafMaterial = EditorGUILayout.ObjectField("Leaf Material", customTarget.m_leafMaterial, typeof(Material), false) as Material;
    }

    private void RenderSpaceColonisationProperties(VegetationDescription customTarget)
    {
        EditorGUILayout.LabelField("Space Colonisation", EditorStyles.boldLabel);
        customTarget.m_spaceColonisationTreePrefab = EditorGUILayout.ObjectField("Tree Prefab", customTarget.m_spaceColonisationTreePrefab, typeof(GameObject), false) as GameObject;
    }
}
