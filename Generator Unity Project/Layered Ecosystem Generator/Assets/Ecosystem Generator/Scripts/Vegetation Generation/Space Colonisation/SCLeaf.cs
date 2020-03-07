using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCLeaf
{
    private Vector3 m_position;
    public Vector3 Position
    {
        get { return m_position; }
    }

    public SCLeaf(Vector3 position)
    {
        m_position = position;
    }
}
