using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonSample
{
    public float m_innerRadius;
    public float m_outerRadius;
    public int m_layer;
    public int m_vegetationType;
    public Vector2 m_samplePosition;
    public Vector3 m_correctedWorldSpacePosition;

    public bool IsInBounds(PoissonSample sampleToCheck)
    {
        float minimumDistance = sampleToCheck.m_outerRadius;
        minimumDistance += sampleToCheck.m_layer == m_layer ? m_outerRadius : m_innerRadius;
        float sqrMinimumDistance = minimumDistance * minimumDistance;
        return Vector2.SqrMagnitude(m_samplePosition - sampleToCheck.m_samplePosition) < sqrMinimumDistance;
    }
}