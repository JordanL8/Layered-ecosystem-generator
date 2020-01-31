using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stem 
{
    public Vector3 m_startPos;
    public Vector3 m_endPos;

    public float m_startRadius;
    public float m_endRadius;

    public Stem(Vector3 startPos, Vector3 endPos)
    {
        m_startPos = startPos;
        m_endPos = endPos;
    }
}
