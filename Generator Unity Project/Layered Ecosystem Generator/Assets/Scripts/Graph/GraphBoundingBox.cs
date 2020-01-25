using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphBoundingBox
{
    private Vector2Int m_Min;
    private Vector2Int m_Max;

    public int MinX => m_Min.x;
    public int MinY => m_Min.y;

    public int MaxX => m_Max.x;
    public int MaxY => m_Max.y;

    public GraphBoundingBox(Vector2Int[] vertices)
    {
        if (vertices.Length == 0) { return; }

        m_Min = m_Max = vertices[0];
        for (int curVertex = 1; curVertex < vertices.Length; ++curVertex)
        {
            Extend(vertices[curVertex]);
        }
    }

    public void Extend(Vector2Int newVertex)
    {
        if (newVertex.x < m_Min.x) m_Min.x = newVertex.x;
        if (newVertex.y < m_Min.y) m_Min.y = newVertex.y;

        if (newVertex.x > m_Max.x) m_Max.x = newVertex.x;
        if (newVertex.y > m_Max.y) m_Max.y = newVertex.y;
    }
}
