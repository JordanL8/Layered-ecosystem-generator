using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBranch
{
    private Vector3 m_position;
    public Vector3 Position
    {
        get { return m_position; }
    }

    private Vector3 m_weldDirection;

    private Vector3 m_direction;
    public Vector3 Direction
    {
        get { return m_direction; }
    }

    public SCBranch m_parent;

    public int m_count = 0;

    public List<Vector3> m_leafPositions = new List<Vector3>();

    public float m_thickness;
    public bool m_hasHadThicknessVisit = false;

    private List<SCBranch> m_children;

    public int ChildCount
    {
        get { return m_children.Count; }
    }

    public bool m_canGrow = true;

    public SCBranch(SCBranch parent, Vector3 position, Vector3 direction, float thickness, bool canGrow = true)
    {
        m_parent = parent;
        m_position = position;
        m_direction = direction;
        m_weldDirection = direction;
        m_thickness = thickness;
        m_canGrow = canGrow;
        m_children = new List<SCBranch>();
    }

    public SCBranch Next(float length, bool checkForOverlap = true)
    {
        SCBranch nextBranch;
        if (m_leafPositions.Count > 0)
        {
            Vector3 closestLeaf = m_leafPositions[0];
            Vector3 nextDirection = (m_direction + (closestLeaf - m_position)).normalized;
            float closestLeafDistance = Vector3.SqrMagnitude(m_leafPositions[0] - m_position);
            for (int i = 1; i < m_leafPositions.Count; i++)
            {
                float distance = Vector3.SqrMagnitude(m_leafPositions[i] - m_position);
                if (distance < closestLeafDistance)
                {
                    closestLeaf = m_leafPositions[i];
                    closestLeafDistance = distance;
                }
                nextDirection += (m_leafPositions[i] - m_position).normalized;
            }
            nextDirection += (closestLeaf - m_position).normalized * 0.1f;
            nextBranch = new SCBranch(this, m_position + nextDirection.normalized * length, nextDirection.normalized, m_thickness);

        }
        else
        {
            nextBranch = new SCBranch(this, m_position + m_direction.normalized * length, m_direction.normalized, m_thickness);
        }
        // Check for duplicates
        if (checkForOverlap)
        {
            Vector3 position = nextBranch.Position;
            for (int i = 0; i < m_children.Count; i++)
            {
                if (Vector3.SqrMagnitude(m_children[i].Position - position) < 0.01f * 0.01f)
                {
                    return null;
                }
            }
        }

        m_children.Add(nextBranch);
        return nextBranch;
    }

    public void OverridePosition(Vector3 newPosition)
    {
        m_position = newPosition;
        //m_direction = m_parent != null ? (m_position - m_parent.Position).normalized : Vector3.up;
    }

    public void Reset()
    {
        m_leafPositions.Clear();
        m_direction = m_weldDirection;
        m_count = 0;
    }

    public bool BeenReachedByAllChildren()
    {
        for (int i = 0; i < ChildCount; i++)
        {
            if (!GetChild(i).m_hasHadThicknessVisit)
            {
                return false;
            }
        }
        return true;
    }

    public float GetPipeRadius(float power)
    {
        float radiusPower = 0.0f;
        for (int i = 0; i < ChildCount; i++)
        {
            radiusPower += Mathf.Pow(m_children[i].m_thickness, power);
        }
        return Mathf.Sqrt(radiusPower);
    }

    public void RemoveChild(SCBranch branch)
    {
        m_children.Remove(branch);
    }

    public void AddChild(SCBranch branch)
    {
        m_children.Add(branch);
        branch.m_parent = this;
    }

    public SCBranch GetChild(int i)
    {
        if (i < m_children.Count)
        {
            return m_children[i];
        }
        return null;
    }

    public void ClearChildren()
    {
        for (int i = 0; i < m_children.Count; i++)
        {
            m_children[i].m_parent = null;
        }
        m_children.Clear();
    }

    public SCBranch Copy()
    {
        SCBranch copy = new SCBranch(m_parent, m_position, m_direction, m_thickness);
        copy.ClearChildren();
        for (int i = 0; i < m_children.Count; i++)
        {
            copy.AddChild(m_children[i]);
        }
        return copy;
    }

    public void D_Draw()
    {
        if (m_parent != null)
        {
            Debug.DrawLine(m_position, m_parent.Position);
        }
    }
}