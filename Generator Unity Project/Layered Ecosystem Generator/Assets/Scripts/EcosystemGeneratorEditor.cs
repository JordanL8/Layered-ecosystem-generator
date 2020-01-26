using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EcosystemGenerator))]
public class EcosystemGeneratorEditor : Editor
{
    private Texture2D m_biomesRepresentationTexture;
    private EcosystemGenerator m_targetComponent;


    // Graph
    private GUIStyle m_graphGuiStyle;
    private Graph m_biomeGraph;

    private void OnEnable()
    {
        m_targetComponent = (EcosystemGenerator)target;
        m_graphGuiStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter };
        DrawBiomeGraph();
    }

    public override void OnInspectorGUI()
    {
        m_targetComponent = (EcosystemGenerator)target;
        base.OnInspectorGUI();

        // Ecosystem properties


        // Graph
        GUILayout.Space(20.0f);
        GUILayout.Label("Biome Graph", EditorStyles.boldLabel);

        GUILayout.Label(m_biomesRepresentationTexture, m_graphGuiStyle);
        if (GUILayout.Button("Redraw Biome Graph"))
        {
            DrawBiomeGraph();
        }
    }

    // UI




    // Graph

    private void DrawBiomeGraph()
    {
        if (m_targetComponent == null || m_targetComponent.m_biomes.Length == 0) { return; }
        m_biomeGraph = new Graph(450, 450, 15, 0);

        m_biomesRepresentationTexture = new Texture2D(450, 450, TextureFormat.RGBA32, false);
        m_biomesRepresentationTexture.wrapMode = TextureWrapMode.Clamp;
        m_biomesRepresentationTexture.filterMode = FilterMode.Point;

        m_biomeGraph.Fill(Color.white);

        foreach (Biome biome in m_targetComponent.m_biomes)
        {
            m_biomeGraph.DrawPolygon(biome.m_temperatureAndRainfallSamplePoints, biome.m_colorOnGraph);
        }

        m_biomesRepresentationTexture = m_biomeGraph.GetGraph();
    }
}
