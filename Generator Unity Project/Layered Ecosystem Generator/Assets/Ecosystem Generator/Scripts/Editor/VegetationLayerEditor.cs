using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(VegetationLayer))]
public class VegetationLayerEditor : Editor
{
    private VegetationLayer m_targetLayer;

    private List<AnimBool> m_layerDescriptionActivated = new List<AnimBool>();

    private void OnEnable()
    {
        m_targetLayer = (VegetationLayer)target;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
    }

    private void RenderProperties()
    {
        EditorGUI.BeginChangeCheck();
        CheckDescriptionActivatedList();
        RenderDescriptionList();
        EditorGUILayout.Space(15);
        RenderVegetationDescriptions();
        if(EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(m_targetLayer);
        }
    }

    private void CheckDescriptionActivatedList()
    {
        if (m_layerDescriptionActivated == null || m_layerDescriptionActivated.Count == 0)
        {
            m_layerDescriptionActivated = new List<AnimBool>();
            for (int i = 0; i < m_targetLayer.m_vegetationInLayer.Count; i++)
            {
                m_layerDescriptionActivated.Add(new AnimBool(false));
            }
        }
    }

    private void RenderDescriptionList()
    {
        EditorGUILayout.LabelField("Vegetation In Layer", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        for (int i = 0; i < m_targetLayer.m_vegetationInLayer.Count; i++)
        {
            VegetationDescription currentDescription = m_targetLayer.m_vegetationInLayer[i];

            GUILayout.BeginHorizontal();
            m_targetLayer.m_vegetationInLayer[i] = EditorGUILayout.ObjectField(currentDescription, typeof(VegetationDescription), false) as VegetationDescription;
            if (GUILayout.Button("Remove"))
            {
                RemoveDescriptionFromList(i);
            }
            GUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Add Layer"))
        {
            m_targetLayer.m_vegetationInLayer.Add(null);
            m_layerDescriptionActivated.Add(new AnimBool(false));
        }
        if(m_targetLayer.m_vegetationInLayer.Count > 0 && GUILayout.Button("Remove Layer"))
        {
            int lastIndex = m_targetLayer.m_vegetationInLayer.Count - 1;
            RemoveDescriptionFromList(lastIndex);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void RenderVegetationDescriptions()
    {
        for (int i = 0; i < m_targetLayer.m_vegetationInLayer.Count; i++)
        {
            if (m_targetLayer.m_vegetationInLayer[i] != null)
            {
                EditorGUILayout.LabelField(m_targetLayer.m_vegetationInLayer[i].name, EditorStyles.boldLabel);
                if (GUILayout.Button(m_layerDescriptionActivated[i].value ? "Hide Description" : "Show Description"))
                {
                    m_layerDescriptionActivated[i].value = !m_layerDescriptionActivated[i].value;
                }
                if (EditorGUILayout.BeginFadeGroup(m_layerDescriptionActivated[i].faded))
                {
                    EditorGUI.indentLevel++;
                    VegetationDescriptionEditor.DisplayInspectorForCustomTarget(m_targetLayer.m_vegetationInLayer[i]);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();
            }
        }
    }

    private void RemoveDescriptionFromList(int index)
    {
        m_targetLayer.m_vegetationInLayer.RemoveAt(index);
        m_layerDescriptionActivated.RemoveAt(index);
    }
}
