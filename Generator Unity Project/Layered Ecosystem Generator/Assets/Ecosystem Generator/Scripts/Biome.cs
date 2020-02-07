using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CreateAssetMenu(fileName = "Biome", menuName = "Ecosystem Generator/Biome", order = 0)]
public class Biome : ScriptableObject
{
    [Header("General")]
    public string m_biomeName = "New Biome";

    [Header("Generation")]
    public GroundLayer m_groundLayer;
    public VegetationLayer[] m_vegetationLayers;
    public float m_sparcity = 1.0f;

    [Header("Graph")]
    public Vector2Int[] m_temperatureAndRainfallSamplePoints;
    public Color m_colorOnGraph = Color.white;



    [HideInInspector] [SerializeField] public AnimBool m_hideGraph = new AnimBool(true);
}

[CustomEditor(typeof(Biome))]
public class BiomeEditor : Editor
{
    private Texture2D m_biomesRepresentationTexture;
    private Biome m_targetBiome;

    private Graph m_biomeGraph;
    private GUIStyle m_graphGuiStyle;

    private void OnEnable()
    {
        m_targetBiome = (Biome)target;
        m_graphGuiStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter };
        DrawBiomeGraph();
    }

    public override void OnInspectorGUI()
    {
        m_targetBiome = (Biome)target;

        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            DrawBiomeGraph();
        }
        if (GUILayout.Button(m_targetBiome.m_hideGraph.target ? "Hide Graph" : "Show Graph"))
        {
            m_targetBiome.m_hideGraph.target = !m_targetBiome.m_hideGraph.target;
        }
        if (EditorGUILayout.BeginFadeGroup(m_targetBiome.m_hideGraph.faded))
        {
            GUILayout.Label(m_biomesRepresentationTexture, m_graphGuiStyle);
            if (GUILayout.Button("Redraw Biome Graph"))
            {
                DrawBiomeGraph();
            }
        }
        EditorGUILayout.EndFadeGroup();
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
}
