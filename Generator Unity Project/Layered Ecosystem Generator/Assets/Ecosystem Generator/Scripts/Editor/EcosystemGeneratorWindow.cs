using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

public class EcosystemGeneratorWindow : EditorWindow
{
    [SerializeField]
    private EcosystemGeneratorProperties m_properties;

    // Temp Window properties.
    private Texture2D m_biomesRepresentationTexture;

        // Graph
    private GUIStyle m_graphGuiStyle;
    private Graph m_biomeGraph;
    private Biome m_currentBiome;
    
    private LayeredPoissonDiscSampler m_sampler;
    private List<Transform> m_layerParents = new List<Transform>();

    private Vector2 m_currentScrollPosition;

    // UI
    private GUIContent m_guiContentAverageAnnualTemp = new GUIContent("Average Annual Temperature", "Controls the average annual temperature of your ecosystem.");
    private GUIContent m_guiContentAverageAnnualRain = new GUIContent("Average Annual Rainfall", "Controls the average annual rainfall of your ecosystem.");
    private GUIContent m_guiContentMaximumInclude = new GUIContent("Maximum Incline", "Controls the maximum incline to place vegetation on. If the angle from the ground's normal to Vector3.up is greater than this value, the generator does not place vegetation on that ground.");
    private GUIContent m_guiContentHeightCheckOffset = new GUIContent("Height Check Offset", "Controls the vertical offset that the generator adds to the height of the Collider attached to the Target GameObject. The generator casts a Ray downwards from this height to find the 3D world position of each sample.");
    private GUIContent m_guiContentEncroachment = new GUIContent("Check For Encroachment", "When enabled, the generator does not place vegetation if the vegetation would encroach on GameObjects in your Scene.");
    private GUIContent m_guiContentTargetGameObject = new GUIContent("Target GameObject", "Specifies the GameObject to generator the ecosystem on.");

    [MenuItem("Window/Ecosystem Generator")]
    private static void Init()
    {
        EcosystemGeneratorWindow window = (EcosystemGeneratorWindow)EditorWindow.GetWindow(typeof(EcosystemGeneratorWindow));
        window.name = "Ecosystem Generator";
        window.Show();
    }

    private void Awake()
    {
        string path = "Assets/Ecosystem Generator/Resources/Ecosystem Properties.asset";
        m_properties = Resources.Load<EcosystemGeneratorProperties>("Ecosystem Properties");
        if (m_properties == null)
        {
            m_properties = CreateInstance<EcosystemGeneratorProperties>();
            AssetDatabase.CreateAsset(m_properties, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        m_properties.m_hideBiomeSection.valueChanged.AddListener(Repaint);
        m_graphGuiStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter };
        m_currentScrollPosition = new Vector2();
        DrawBiomeGraph();
    }

    private void OnGUI()
    {
        if(m_properties == null) { return; }
        EditorGUI.BeginChangeCheck();
        // TODO: Fix vertical scroll view appearing over the UI.
        m_currentScrollPosition = GUILayout.BeginScrollView(m_currentScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
        RenderBiomeSection();
        RenderEcosystemPropertiesSection();
        RenderGenerationPropertiesSection();
        RenderGraphSection();  
        GUILayout.EndScrollView();
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(m_properties);
        }
    }

    private void RenderBiomeSection()
    {
        EditorGUILayout.Space(20.0f);
        EditorGUILayout.LabelField("Biomes", EditorStyles.boldLabel);
        if(GUILayout.Button(m_properties.m_hideBiomeSection.target ? "Hide Biomes" : "Show Biomes"))
        {
            m_properties.m_hideBiomeSection.target = !m_properties.m_hideBiomeSection.target;
        }
        if (EditorGUILayout.BeginFadeGroup(m_properties.m_hideBiomeSection.faded))
        {
            EditorGUI.indentLevel++;
            if (m_properties.m_biomes == null)
            {
                m_properties.m_biomes = new List<Biome>();
            }
            for (int i = 0; i < m_properties.m_biomes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                m_properties.m_biomes[i] = (Biome)EditorGUILayout.ObjectField(m_properties.m_biomes[i], typeof(Biome), false, GUILayout.MinWidth(60f));
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(75.0f)))
                {
                    m_properties.m_biomes.RemoveAt(i);
                    this.Repaint();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            if (m_properties.m_biomes.Count == 0)
            {
                if (GUILayout.Button("Add"))
                {
                    m_properties.m_biomes.Add(null);
                }
            }
            else
            {
                float halfWindowWidth = position.width / 2;
                if (GUILayout.Button("Add", GUILayout.Width(halfWindowWidth)))
                {
                    m_properties.m_biomes.Add(null);
                }
                if (GUILayout.Button("Remove Last", GUILayout.Width(halfWindowWidth)))
                {
                    m_properties.m_biomes.RemoveAt(m_properties.m_biomes.Count - 1);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFadeGroup();
    }

    private void RenderEcosystemPropertiesSection()
    {
        EditorGUILayout.Space(20.0f);
        EditorGUILayout.LabelField("Ecosystem Properties", EditorStyles.boldLabel);
        
        float halfWindowWidth = position.width / 2;
        EditorGUI.BeginChangeCheck();
        m_properties.m_averageAnnualTemperature = EditorGUILayout.IntSlider(m_guiContentAverageAnnualTemp, m_properties.m_averageAnnualTemperature, -15, 30);
        m_properties.m_averageAnnualRainfall = EditorGUILayout.IntSlider(m_guiContentAverageAnnualRain, m_properties.m_averageAnnualRainfall, 0, 450);
        if(EditorGUI.EndChangeCheck())
        {
            DrawBiomeGraph();
        }
    }

    private void RenderGenerationPropertiesSection()
    {
        EditorGUILayout.Space(20.0f);
        EditorGUILayout.LabelField("Placement Properties", EditorStyles.boldLabel);
        m_properties.m_maximumIncline = EditorGUILayout.Slider(m_guiContentMaximumInclude, m_properties.m_maximumIncline, 0.0f, 90.0f);
        m_properties.m_checkHeightOffset = EditorGUILayout.FloatField(m_guiContentHeightCheckOffset, m_properties.m_checkHeightOffset);
        m_properties.m_checkForEncroachment = EditorGUILayout.Toggle(m_guiContentEncroachment, m_properties.m_checkForEncroachment);
        m_properties.m_targetGameObject = EditorGUILayout.ObjectField(m_guiContentTargetGameObject, m_properties.m_targetGameObject, typeof(GameObject), true) as GameObject;
}

    private void RenderGraphSection()
    {
        // Graph
        EditorGUILayout.Space(20.0f);
        EditorGUILayout.LabelField("Biome Graph", EditorStyles.boldLabel);
   
        string displayBiomeText = m_currentBiome != null ? $"Current Biome: { m_currentBiome.m_biomeName }." : "Invalid Selection!";
        EditorGUILayout.LabelField(displayBiomeText);

        if (GUILayout.Button(m_properties.m_hideGraphSection.target ? "Hide Graph" : "Show Graph"))
        {
            m_properties.m_hideGraphSection.target = !m_properties.m_hideGraphSection.target;
        }
        if (EditorGUILayout.BeginFadeGroup(m_properties.m_hideGraphSection.faded))
        {
            if (m_biomesRepresentationTexture != null)
            {
                EditorGUILayout.Space(20.0f);
                GUILayout.Label(m_biomesRepresentationTexture, m_graphGuiStyle);
            }
            if (GUILayout.Button("Redraw Biome Graph"))
            {
                DrawBiomeGraph();
            }
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUI.BeginDisabledGroup(m_currentBiome == null ||
            m_properties.m_targetGameObject == null);
        if (GUILayout.Button($"Generate {(m_currentBiome != null ? (m_currentBiome.m_biomeName) : "Ecosystem")}"))
        {
            GenerateEcosystem(m_currentBiome, m_properties.m_targetGameObject);
        }
        EditorGUI.EndDisabledGroup();
    }

    private void DrawBiomeGraph()
    {
        if (m_properties.m_biomes == null || m_properties.m_biomes.Count == 0) { return; }
        m_biomeGraph = new Graph(450, 450, 15, 0);

        m_biomesRepresentationTexture = new Texture2D(450, 450, TextureFormat.RGBA32, false);
        m_biomesRepresentationTexture.wrapMode = TextureWrapMode.Clamp;
        m_biomesRepresentationTexture.filterMode = FilterMode.Point;

        m_biomeGraph.Fill(Color.white);

        bool validSample = false;
        foreach (Biome biome in m_properties.m_biomes)
        {
            m_biomeGraph.DrawPolygon(biome.m_temperatureAndRainfallSamplePoints, biome.m_colorOnGraph);
            if (m_biomeGraph.IsInPolygon((m_properties.m_averageAnnualTemperature + 15) * 10, m_properties.m_averageAnnualRainfall, biome.m_temperatureAndRainfallSamplePoints))
            {
                validSample = true;
                m_currentBiome = biome;
            }
        }
        if (!validSample) { m_currentBiome = null; }

        m_biomeGraph.DrawCross(new Vector2Int(m_properties.m_averageAnnualTemperature, m_properties.m_averageAnnualRainfall), 7, 1, Color.black);
        m_biomeGraph.Apply();

        m_biomesRepresentationTexture = m_biomeGraph.GetGraph();
    }










    // Generator.


    public void ClearHierarchy(GameObject targetGameObject)
    {
        if (m_layerParents == null)
        {
            return;
        }
        else if(m_layerParents.Count > 0)
        {
            if (m_layerParents[0] && m_layerParents[0].parent != targetGameObject.transform)
            {
                m_layerParents.Clear();
            }
        }
        for (int i = 0; i < m_layerParents.Count; i++)
        {
            if (m_layerParents[i])
            {
                DestroyImmediate(m_layerParents[i].gameObject);
            }
        }
        m_layerParents.Clear();
    }

    public void GenerateEcosystem(Biome biome, GameObject targetGameObject)
    {
        Collider targetCollider = targetGameObject.GetComponent<Collider>();
        if (targetCollider == null)
        {
            Debug.LogError("There is no Collider on the target GameObject. The generator Raycasts against the Collider to find the 3D position of vegetation.");
            return;
        }
        Bounds targetBounds = targetCollider.bounds;

        m_sampler = new LayeredPoissonDiscSampler(targetBounds, biome.m_sparcity);
        if (m_layerParents == null) { m_layerParents = new List<Transform>(); }
        ClearHierarchy(targetGameObject);

        for (int i = 0; i < biome.m_vegetationLayers.Length; i++)
        {
            m_sampler.SampleForLayer(biome.m_vegetationLayers[i], i);
            m_layerParents.Add(new GameObject($"Layer: {biome.m_vegetationLayers[i].name}").transform);
            m_layerParents[i].parent = targetGameObject.transform;
            m_layerParents[i].localPosition = Vector3.zero;
        }
        List<PoissonSample> samples = GetValidSamples(m_sampler.GetSamples(), targetGameObject, targetBounds.max.y + m_properties.m_checkHeightOffset);
        if (samples == null)
        {
            return;
        }

        PlaceVegetation(samples);
    }

    private List<PoissonSample> GetValidSamples(List<PoissonSample> allSamples, GameObject targetGameObject, float checkHeight)
    {
        List<PoissonSample> finalSamples = new List<PoissonSample>();
        for (int i = 0; i < allSamples.Count; i++)
        {
            PoissonSample curSample = allSamples[i];
            if (IsValidSample(curSample, out curSample.m_correctedWorldSpacePosition, targetGameObject, checkHeight))
            {
                if (m_properties.m_checkForEncroachment)
                {
                    if (!DoesSampleEncroachOnGameObjects(curSample, targetGameObject, checkHeight))
                    {
                        finalSamples.Add(curSample);
                    }
                }
                else
                {
                    finalSamples.Add(curSample);
                }
            }
        }
        return finalSamples;
    }

    private bool IsValidSample(PoissonSample sample, out Vector3 hitPosition, GameObject target, float checkHeight)
    {
        Vector3 checkOrigin = new Vector3(sample.m_samplePosition.x, checkHeight, sample.m_samplePosition.y);

        RaycastHit hit;
        if (Physics.Raycast(new Ray(checkOrigin, Vector3.down), out hit))
        {
            if (hit.transform.gameObject == target)
            {
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                if (angle < m_properties.m_maximumIncline)
                {
                    hitPosition = hit.point;
                    return true;
                }
            }
        }
        hitPosition = Vector3.zero;
        return false;
    }

    private bool DoesSampleEncroachOnGameObjects(PoissonSample sample, GameObject target, float checkHeight)    // Change to SphereCast.
    {
        Vector3[] cornerOffsets = { new Vector3(0, 0, sample.m_outerRadius), new Vector3(0, 0, -sample.m_outerRadius), new Vector3(sample.m_outerRadius, 0, 0), new Vector3(-sample.m_outerRadius, 0, 0) };
        for (int i = 0; i < cornerOffsets.Length; i++)
        {
            Vector3 checkOrigin = new Vector3(sample.m_samplePosition.x, checkHeight, sample.m_samplePosition.y) + cornerOffsets[i];
            RaycastHit hit;
            if (Physics.Raycast(new Ray(checkOrigin, Vector3.down), out hit))
            {
                if (hit.transform.gameObject != target)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void PlaceVegetation(List<PoissonSample> samples)
    {
        for (int i = 0; i < samples.Count; i++)
        {
            VegetationLayer sampleLayer = m_currentBiome.m_vegetationLayers[samples[i].m_layer];
            VegetationDescription sampleVegetation = sampleLayer.m_vegetationInLayer[samples[i].m_vegetationType];
            if(sampleVegetation.m_vegationType == VegetationType.SpaceColonisation)
            {
                GameObject veg = Instantiate(sampleVegetation.m_spaceColonisationTreePrefab);
                SCTree tree = veg.GetComponent<SCTree>();
                tree.Generate();
                veg.transform.position = samples[i].m_correctedWorldSpacePosition;
                veg.transform.rotation = Quaternion.Euler(0, Random.value * 360, 0);
                veg.transform.parent = m_layerParents[samples[i].m_layer];
            }

            //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            //obj.transform.position = new Vector3(samples[i].m_correctedWorldSpacePosition.x, 0, samples[i].m_correctedWorldSpacePosition.z);
            //obj.transform.localScale = new Vector3(samples[i].m_outerRadius * 2, 0.0f, samples[i].m_outerRadius * 2);
            //obj.transform.parent = m_layerParents[samples[i].m_layer];
            //GameObject obj2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            //obj2.transform.position = new Vector3(samples[i].m_correctedWorldSpacePosition.x, 0, samples[i].m_correctedWorldSpacePosition.z);
            //obj2.transform.localScale = new Vector3(samples[i].m_innerRadius * 2, 0.0f, samples[i].m_innerRadius * 2);
            //obj2.transform.parent = m_layerParents[samples[i].m_layer];
        }
    }
}
