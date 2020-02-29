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
    private Biome m_currentBiome;

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

        // Graph
        EditorGUILayout.Space(20.0f);
        EditorGUILayout.LabelField("Biome", EditorStyles.boldLabel);
        
        string displayBiomeText = m_currentBiome != null ? $"Current Biome: { m_currentBiome.m_biomeName }." : "Invalid Selection!";
        EditorGUILayout.LabelField(displayBiomeText);

        EditorGUILayout.Space(20.0f);
        GUILayout.Label(m_biomesRepresentationTexture, m_graphGuiStyle);
        if (GUILayout.Button("Redraw Biome Graph"))
        {
            DrawBiomeGraph();
        }
        if(GUILayout.Button("Generate Ecosystem"))
        {
            m_targetComponent.GenerateEcosystem(m_targetComponent.biome, m_targetComponent.gameObject);
        }
    }
    
    // Graph

    private void DrawBiomeGraph()
    {
        if (m_targetComponent == null || m_targetComponent.m_biomes.Length == 0) { return; }
        m_biomeGraph = new Graph(450, 450, 15, 0);

        m_biomesRepresentationTexture = new Texture2D(450, 450, TextureFormat.RGBA32, false);
        m_biomesRepresentationTexture.wrapMode = TextureWrapMode.Clamp;
        m_biomesRepresentationTexture.filterMode = FilterMode.Point;

        m_biomeGraph.Fill(Color.white);

        bool validSample = false;
        foreach (Biome biome in m_targetComponent.m_biomes)
        {
            m_biomeGraph.DrawPolygon(biome.m_temperatureAndRainfallSamplePoints, biome.m_colorOnGraph);
            if (m_biomeGraph.IsInPolygon((m_targetComponent.m_averageAnnualTemperature + 15) * 10, m_targetComponent.m_averageAnnualRainfall, biome.m_temperatureAndRainfallSamplePoints))
            {
                validSample = true;
                m_currentBiome = biome;
            }
        }
        if(!validSample) { m_currentBiome = null; }

        m_biomeGraph.DrawCross(new Vector2Int(m_targetComponent.m_averageAnnualTemperature, m_targetComponent.m_averageAnnualRainfall), 7, 1, Color.black);
        m_biomeGraph.Apply();

        m_biomesRepresentationTexture = m_biomeGraph.GetGraph();
    }
}
