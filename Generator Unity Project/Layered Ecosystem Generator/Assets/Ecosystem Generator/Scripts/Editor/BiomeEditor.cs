using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(Biome))]
public class BiomeEditor : Editor
{

    private VegetationLayerEditor m_selectedLayerEditor;
    private int m_selectedLayerIndex = -1;

    private Texture2D m_biomesRepresentationTexture;
    private Biome m_targetBiome;

    private Graph m_biomeGraph;

    private void OnEnable()
    {
        m_targetBiome = (Biome)target;
        DrawBiomeGraph();
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        
        RenderGeneralProperties();
        RenderGenerationProperties();
        RenderSelectedLayer();

        EditorGUI.BeginChangeCheck();
        RenderGraphProperties();
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(m_targetBiome);
            DrawBiomeGraph();
        }
        RenderGraphVisualisation();
    }


    private void DrawBiomeGraph()
    {
        if (m_targetBiome == null) { return; }
        m_biomeGraph = new Graph(450, 450, 15, 0);

        m_biomesRepresentationTexture = new Texture2D(450, 450, TextureFormat.RGBA32, false);
        m_biomesRepresentationTexture.wrapMode = TextureWrapMode.Clamp;
        m_biomesRepresentationTexture.filterMode = FilterMode.Point;

        m_biomeGraph.Fill(Color.white);
        m_biomeGraph.DrawPolygon(m_targetBiome.m_temperatureAndRainfallSamplePoints, m_targetBiome.m_colorOnGraph);
        m_biomeGraph.Apply();

        m_biomesRepresentationTexture = m_biomeGraph.GetGraph();
    }

    private void RenderGeneralProperties()
    {
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
        m_targetBiome.m_biomeName = EditorGUILayout.TextField("Biome Name", m_targetBiome.m_biomeName);
    }

    private void RenderGenerationProperties()
    {
        EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
        m_targetBiome.m_groundLayer = EditorGUILayout.ObjectField("Ground Layer", m_targetBiome.m_groundLayer, typeof(GroundLayer), false) as GroundLayer;

        if (m_selectedLayerEditor != null)
        {
            return;
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Layers In Biome", EditorStyles.boldLabel);
        for (int i = 0; i < m_targetBiome.m_vegetationLayers.Count; i++)
        {
            VegetationLayer currentLayer = m_targetBiome.m_vegetationLayers[i];

            GUILayout.BeginHorizontal();
            m_targetBiome.m_vegetationLayers[i] = EditorGUILayout.ObjectField(currentLayer, typeof(VegetationLayer), false) as VegetationLayer;
            if (GUILayout.Button("Select"))
            {
                SelectNewLayer(i);
            }
            if (GUILayout.Button("Remove"))
            {
                RemoveLayerFromList(i);
            }
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Layer"))
        {
            m_targetBiome.m_vegetationLayers.Add(null);
        }
        if (m_targetBiome.m_vegetationLayers.Count > 0 && GUILayout.Button("Remove Layer"))
        {
            int lastIndex = m_targetBiome.m_vegetationLayers.Count - 1;
            RemoveLayerFromList(lastIndex);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void RenderSelectedLayer()
    {
        if (m_selectedLayerEditor != null)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Vegetation Layer Editor", EditorStyles.centeredGreyMiniLabel);
            m_selectedLayerEditor.OnInspectorGUI();
            if (m_selectedLayerEditor.m_selectedDescriptionEditor == null)
            {
                if (GUILayout.Button("Deselect"))
                {
                    m_selectedLayerEditor.m_selectedDescriptionEditor = null;
                    m_selectedLayerEditor = null;
                    m_selectedLayerIndex = -1;
                }
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
        }
    }

    private void RenderGraphProperties()
    {
        EditorGUILayout.Space(20);
    }

    private void RenderGraphVisualisation()
    {
        if (GUILayout.Button(m_targetBiome.m_hideGraph.target ? "Hide Graph" : "Show Graph"))
        {
            m_targetBiome.m_hideGraph.target = !m_targetBiome.m_hideGraph.target;
        }
        if (EditorGUILayout.BeginFadeGroup(m_targetBiome.m_hideGraph.faded))
        {
            GUILayout.Label(m_biomesRepresentationTexture, new GUIStyle() { alignment = TextAnchor.MiddleCenter });
            if (GUILayout.Button("Redraw Biome Graph"))
            {
                DrawBiomeGraph();
            }
        }
        EditorGUILayout.EndFadeGroup();
    }

    private void RemoveLayerFromList(int index)
    {
        m_targetBiome.m_vegetationLayers.RemoveAt(index);
        if(index == m_selectedLayerIndex)
        {
            m_selectedLayerEditor = null;
            m_selectedLayerIndex = -1;
        }
    }
    
    private void SelectNewLayer(int index)
    {
        m_selectedLayerEditor = Editor.CreateEditor(m_targetBiome.m_vegetationLayers[index]) as VegetationLayerEditor;
        m_selectedLayerIndex = index;
    }
}