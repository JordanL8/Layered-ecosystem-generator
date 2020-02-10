using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StemSegment
{
    public Vector3 m_startPos;
    public Vector3 m_endPos;

    public float m_startRadius;
    public float m_endRadius;
}
/*
public class Stem 
{
    private List<StemSegment> m_segments;
    public Vector3 m_startPoint;
    public Vector3 m_startEular;
    public int m_recursionLevel;
    public int m_segmentsThrough;

    public List<StemSegment> GetStemSegments()
    {
        return m_segments;
    }

    public void AddStemSegment(StemSegment segment)
    {
        m_segments.Add(segment);
    }

    public Stem(Vector3 startPoint, Vector3 startEular, int recursionLevel, int segmentsThrough = 0)
    {
        m_segments = new List<StemSegment>();
        m_startPoint = startPoint;
        m_startEular = startEular;
        m_recursionLevel = recursionLevel;
        m_segmentsThrough = segmentsThrough;
    }

    
}
*/