using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EcosystemGenerator))]
public class EcosystemGeneratorEditor : Editor
{
    private Texture2D m_biomesRepresentationTexture;
    private EcosystemGenerator m_targetComponent;


    // Graph
    [SerializeField]
    public int m_textureScale = 1;
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
        m_textureScale = EditorGUILayout.IntField("Scale", m_textureScale);

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
        BiomeTextureDimensions textureDimensions = GetTextureDimensions();
        m_biomeGraph = new Graph(textureDimensions.Width, textureDimensions.Height, textureDimensions.TemperatureOffset, textureDimensions.RainfallOffset, m_textureScale);

        m_biomesRepresentationTexture = new Texture2D(textureDimensions.Width * m_textureScale,
            textureDimensions.Height * m_textureScale, TextureFormat.RGBA32, false);
        m_biomesRepresentationTexture.wrapMode = TextureWrapMode.Clamp;
        m_biomesRepresentationTexture.filterMode = FilterMode.Point;

        m_biomeGraph.DrawBox(0, 0, m_biomesRepresentationTexture.width, m_biomesRepresentationTexture.height, Color.white);

        foreach (Biome biome in m_targetComponent.m_biomes)
        {
            m_biomeGraph.DrawPolygon(biome.m_temperatureAndRainfallSamplePoints, biome.m_colorOnGraph);
        }

        m_biomeGraph.DrawBox(new Vector2Int(m_targetComponent.m_averageAnnualTemperature, m_targetComponent.m_averageAnnualRainfall), 4, Color.black);

        m_biomesRepresentationTexture = m_biomeGraph.GetGraph();
    }

    private BiomeTextureDimensions GetTextureDimensions()
    {
        BiomeTextureDimensions textureDimensions = BiomeTextureDimensions.MinMax();
        if (m_targetComponent == null || m_targetComponent.m_biomes.Length == 0) { return textureDimensions; }
        
        for (int currentBiome = 0; currentBiome < m_targetComponent.m_biomes.Length; ++currentBiome)
        {
            Biome biome = m_targetComponent.m_biomes[currentBiome];
            for (int currentSamplePoint = 0; currentSamplePoint < biome.m_temperatureAndRainfallSamplePoints.Length; ++currentSamplePoint)
            {
                int temperatureValue = biome.m_temperatureAndRainfallSamplePoints[currentSamplePoint].x;
                if(temperatureValue < textureDimensions.m_NegativeX) { textureDimensions.m_NegativeX = temperatureValue; }
                else if(temperatureValue > textureDimensions.m_PositiveX) { textureDimensions.m_PositiveX = temperatureValue; }

                int rainfallValue = biome.m_temperatureAndRainfallSamplePoints[currentSamplePoint].y;
                if(rainfallValue < textureDimensions.m_NegativeY) { textureDimensions.m_NegativeY = rainfallValue; }
                else if(rainfallValue > textureDimensions.m_PositiveY) { textureDimensions.m_PositiveY = rainfallValue; }
            }
        }

        return textureDimensions;
    }
}
