using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Threading;

public class EcosystemGenerator : MonoBehaviour
{
    [Header("Available Biomes")]
    [Tooltip("Specifies the biomes available for this generator.")]
    public Biome[] m_biomes;

    [Header("Ecosystem Properties")]
    [Range(-15, 30), Tooltip("Controls the average annual temperature of your ecosystem.")]
    public int m_averageAnnualTemperature = 9;

    [Range(0, 450), Tooltip("Controls the average annual rainfall of your ecosystem.")]
    public int m_averageAnnualRainfall = 125;

    [Header("Generation Properties")]
    [Range(0,90), Tooltip("Controls the maximum incline that the generator places vegetation. If the angle from the ground's normal to Vector3.up is greater than this value, the generator does not place vegetation.")]
    public float m_maximumIncline = 10.0f;

    [Min(0), Tooltip("Sets the height above bounds of the target the generator uses to validate the position of each sample point. The generator copies the position of each sample point, increases the height to the height of the target GameObject's bounds plus this value, then casts a Raycast straight down to check the validity of the sample point.")]
    public float m_checkHeightOffset = 5.0f;
    [Tooltip("When enabled, the generator checks whether the extents of a piece of vegetation encroach on GameObjects in your Scene.")]
    public bool m_checkForEncroachment = false;

    [Space(50)]
    public Biome biome;     //Temp
    private LayeredPoissonDiscSampler m_sampler;

    private List<Transform> m_layerParents = new List<Transform>();
    public void ClearHierarchy()
    {
        if (m_layerParents == null)
        {
            return;
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
        ClearHierarchy();

        for (int i = 0; i < biome.m_vegetationLayers.Length; i++)
        {
            m_sampler.SampleForLayer(biome.m_vegetationLayers[i], i);
            m_layerParents.Add(new GameObject($"Layer: {biome.m_vegetationLayers[i].name}").transform);
            m_layerParents[i].parent = targetGameObject.transform;
            m_layerParents[i].localPosition = Vector3.zero;
        }
        List<PoissonSample> samples = GetValidSamples(m_sampler.GetSamples(), gameObject, targetBounds.max.y + m_checkHeightOffset);
        if(samples == null)
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
                if(m_checkForEncroachment)
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
            if(hit.transform.gameObject == target)
            {
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                if(angle < m_maximumIncline)
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
        for(int i = 0; i < cornerOffsets.Length; i++)
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
            GameObject veg = Instantiate(biome.m_vegetationLayers[samples[i].m_layer].TEMPPREFABS[0]);
            veg.transform.position = samples[i].m_correctedWorldSpacePosition;
            veg.transform.rotation = Quaternion.Euler(0, Random.value * 360, 0);
            veg.transform.parent = m_layerParents[samples[i].m_layer];

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.transform.position = new Vector3(samples[i].m_correctedWorldSpacePosition.x, 0, samples[i].m_correctedWorldSpacePosition.y);
            obj.transform.localScale = new Vector3(samples[i].m_outerRadius * 2, 0.0f, samples[i].m_outerRadius * 2);
            obj.transform.parent = m_layerParents[samples[i].m_layer];
            GameObject obj2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj2.transform.position = new Vector3(samples[i].m_correctedWorldSpacePosition.x, 0, samples[i].m_correctedWorldSpacePosition.y);
            obj2.transform.localScale = new Vector3(samples[i].m_innerRadius * 2, 0.0f, samples[i].m_innerRadius * 2);
            obj2.transform.parent = m_layerParents[samples[i].m_layer];
        }
    }
}
