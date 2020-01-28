using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayeredPoissonDiscSampler
{
    private List<PoissonSample> m_samples;
    private Vector2 m_bounds;
    private int m_rejectionNumber = 30;

    public LayeredPoissonDiscSampler()
    {
        m_samples = new List<PoissonSample>();
        m_bounds.x = 10.0f;
        m_bounds.y = 10.0f;
    }

    public void SampleForLayer(VegetationLayer layer, int layerNumber)
    {
        List<PoissonSample> spawnPoints;
        if (m_samples.Count == 0)
        {
            spawnPoints = new List<PoissonSample>();
            PoissonSample newSample = GetSample(layer, layerNumber);
            newSample.m_position = new Vector2(m_bounds.x / 2, m_bounds.y / 2);
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
                newSample.m_position = GetRandomPosition(spawnPoint, newSample);
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
        return !(sample.m_position.x + sample.m_outerRadius > m_bounds.x ||
            sample.m_position.x - sample.m_outerRadius < 0 ||
            sample.m_position.y + sample.m_outerRadius > m_bounds.y ||
            sample.m_position.y - sample.m_outerRadius < 0);
    }

    private PoissonSample GetSample(VegetationLayer layer, int layerNumber)
    {
        int vegetationIndex = Random.Range(0, layer.m_vegetationInLayer.Length);
        VegetationDescription currentVegetation = layer.m_vegetationInLayer[vegetationIndex];

        PoissonSample newSample = new PoissonSample();
        newSample.m_innerRadius = currentVegetation.m_innerRadius;
        newSample.m_outerRadius = currentVegetation.m_outerRadius;
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
        return spawnPoint.m_position + (direction * Random.Range(minimumDistance, minimumDistance * 2.0f));   // This naive range results in a distribution biased towards the smaller radius. This gives a more packed result.
    }
}
