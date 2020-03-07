using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayeredPoissonDiscSampler
{
    private List<PoissonSample> m_samples;
    private Bounds m_bounds;
    private int m_rejectionNumber = 25;
    private float m_sparcity;

    public LayeredPoissonDiscSampler(Bounds bounds, float sparcity = 1.0f)
    {
        m_samples = new List<PoissonSample>();
        m_bounds = bounds;
        m_sparcity = sparcity;
    }

    public void SampleForLayer(VegetationLayer layer, int layerNumber)
    {
        List<PoissonSample> spawnPoints;
        if (m_samples.Count == 0)
        {
            spawnPoints = new List<PoissonSample>();
            PoissonSample newSample = GetSample(layer, layerNumber);
            newSample.m_samplePosition = new Vector2(m_bounds.min.x + Random.value * (m_bounds.max.x - m_bounds.min.x), m_bounds.min.z + Random.value * (m_bounds.max.z - m_bounds.min.z));
            m_samples.Add(newSample);
            spawnPoints.Add(newSample);
        }
        else
        {
            spawnPoints = new List<PoissonSample>(m_samples);
        }

        while (spawnPoints.Count > 0)
        {
            PoissonSample newSample = GetSample(layer, layerNumber);
            PoissonSample spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            int numRejcetions = 0;
            bool successfulSample = true;
            while (numRejcetions <= m_rejectionNumber)
            {
                successfulSample = true;
                newSample.m_samplePosition = GetRandomPosition(spawnPoint, newSample);
                if (!IsInPlacementRegion(newSample))
                {
                    successfulSample = false;
                    ++numRejcetions;
                }
                else
                {
                    for (int i = 0; i < m_samples.Count; i++)
                    {
                        if (m_samples[i].IsInBounds(newSample))
                        {
                            successfulSample = false;
                            ++numRejcetions;
                            break;
                        }
                    }
                }
                if (successfulSample)
                {
                    m_samples.Add(newSample);
                    spawnPoints.Add(newSample);
                    break;
                }
            }

            if(!successfulSample)
            {
                spawnPoints.Remove(spawnPoint);
            }
        }
    }

    public List<PoissonSample> GetSamples()
    {
        return m_samples;
    }

    private bool IsInPlacementRegion(PoissonSample sample)
    {
        return !(sample.m_samplePosition.x + sample.m_outerRadius > m_bounds.max.x ||
            sample.m_samplePosition.x - sample.m_outerRadius < m_bounds.min.x ||
            sample.m_samplePosition.y + sample.m_outerRadius > m_bounds.max.z ||
            sample.m_samplePosition.y - sample.m_outerRadius < m_bounds.min.z);
    }

    private PoissonSample GetSample(VegetationLayer layer, int layerNumber)
    {
        int vegetationIndex = Random.Range(0, layer.m_vegetationInLayer.Count);
        VegetationDescription currentVegetation = layer.m_vegetationInLayer[vegetationIndex];

        PoissonSample newSample = new PoissonSample();
        newSample.m_innerRadius = currentVegetation.m_innerRadius * currentVegetation.m_sparcity * m_sparcity;
        newSample.m_outerRadius = currentVegetation.m_outerRadius * currentVegetation.m_sparcity * m_sparcity;
        newSample.m_layer = layerNumber;
        newSample.m_vegetationType = vegetationIndex;
        return newSample;
    }

    private Vector2 GetRandomPosition(PoissonSample spawnPoint, PoissonSample sample)
    {
        float angle = Random.value * Mathf.PI * 2;
        Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)).normalized;

        float minimumDistance = sample.m_outerRadius;
        minimumDistance += sample.m_layer == spawnPoint.m_layer ? spawnPoint.m_outerRadius : spawnPoint.m_innerRadius;
        return spawnPoint.m_samplePosition + (direction * Random.Range(minimumDistance, minimumDistance * 2.0f));   // This naive range results in a distribution biased towards the smaller radius. This gives a more packed result.
    }
}
