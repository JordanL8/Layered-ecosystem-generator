using UnityEngine;
using System.Collections.Generic;
using UnityEditor;


public class EcosystemGenerator : MonoBehaviour
{
    [Header("Available Biomes")]
    public Biome[] m_biomes;
    [Header("Ecosystem Properties")]
    [Range(-15, 30)]
    public int m_averageAnnualTemperature = 9;
    [Range(0, 450)]
    public int m_averageAnnualRainfall = 125;

    private LayeredPoissonDiscSampler m_sampler = new LayeredPoissonDiscSampler();

    public Biome biome;

    private void Start()
    {
        GenerateEcosystem(biome);
    }

    private void ClearHierarchy()
    {
        for (int currentChild = 0; currentChild < transform.childCount; currentChild++)
        {
            Destroy(transform.GetChild(currentChild).gameObject);
        }
    }


    public void GenerateEcosystem(Biome biome)
    {
        ClearHierarchy();
        for (int i = 0; i < biome.m_vegetationLayers.Length; i++)
        {
            m_sampler.SampleForLayer(biome.m_vegetationLayers[i], i);
        }
        List<PoissonSample> samples = m_sampler.GetSamples();
        for (int i = 0; i < samples.Count; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.transform.position = new Vector3(samples[i].m_position.x, 0, samples[i].m_position.y);
            obj.transform.localScale = new Vector3(samples[i].m_outerRadius * 2, 0.0f, samples[i].m_outerRadius * 2);
            GameObject obj2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj2.transform.position = new Vector3(samples[i].m_position.x, 0, samples[i].m_position.y);
            obj2.transform.localScale = new Vector3(samples[i].m_innerRadius * 2, 0.0f, samples[i].m_innerRadius * 2);
        }
    }
}
