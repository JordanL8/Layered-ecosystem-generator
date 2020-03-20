using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(VegetationLayer))]
public class VegetationLayerEditor : Editor
{
    private VegetationLayer m_targetLayer;
    
    public Editor m_selectedDescriptionEditor;
    private int m_selectedDescriptionIndex = -1;


    private void OnEnable()
    {
        m_targetLayer = (VegetationLayer)target;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        RenderProperties();
    }

    private void RenderProperties()
    {
        EditorGUILayout.LabelField(m_targetLayer.name, new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
        EditorGUI.BeginChangeCheck();
        RenderDescriptionList();
        EditorGUILayout.Space(15);
        RenderSelectedDescription();
        if(EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(m_targetLayer);
        }
    }

    private void RenderDescriptionList()
    {
        if(m_selectedDescriptionEditor != null)
        {
            return;
        }
        EditorGUILayout.LabelField("Vegetation In Layer", EditorStyles.boldLabel);
        if(m_targetLayer.m_vegetationInLayer == null)
        {
            m_targetLayer.m_vegetationInLayer = new List<VegetationDescription>();
        }
        for (int i = 0; i < m_targetLayer.m_vegetationInLayer.Count; i++)
        {
            VegetationDescription currentDescription = m_targetLayer.m_vegetationInLayer[i];

            GUILayout.BeginHorizontal();
            m_targetLayer.m_vegetationInLayer[i] = EditorGUILayout.ObjectField(currentDescription, typeof(VegetationDescription), false) as VegetationDescription;

            if(GUILayout.Button("Select"))
            {
                SelectNewDescription(i);
            }
            if (GUILayout.Button("Remove"))
            {
                RemoveDescriptionFromList(i);
            }
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Add Description"))
        {
            m_targetLayer.m_vegetationInLayer.Add(null);
        }
        if(m_targetLayer.m_vegetationInLayer.Count > 0 && GUILayout.Button("Remove Description"))
        {
            int lastIndex = m_targetLayer.m_vegetationInLayer.Count - 1;
            RemoveDescriptionFromList(lastIndex);
        }
        EditorGUILayout.EndHorizontal();
    }


    private void RenderSelectedDescription()
    {
        if (m_selectedDescriptionEditor != null)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Vegetation Description Editor", EditorStyles.centeredGreyMiniLabel);
            m_selectedDescriptionEditor.OnInspectorGUI();
            if (GUILayout.Button("Deselect"))
            {
                m_selectedDescriptionEditor = null;
                m_selectedDescriptionIndex = -1;
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
    }

    private void RemoveDescriptionFromList(int index)
    {
        m_targetLayer.m_vegetationInLayer.RemoveAt(index);
        if(index == m_selectedDescriptionIndex)
        {
            m_selectedDescriptionEditor = null;
            m_selectedDescriptionIndex = -1;
        }
    }

    private void SelectNewDescription(int index)
    {
        m_selectedDescriptionEditor = Editor.CreateEditor(m_targetLayer.m_vegetationInLayer[index]);
        m_selectedDescriptionIndex = index;
    }
}
