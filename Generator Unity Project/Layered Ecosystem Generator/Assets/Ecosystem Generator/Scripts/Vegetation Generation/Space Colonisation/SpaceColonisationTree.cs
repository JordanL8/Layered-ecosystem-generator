using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TreeEnvelope
{
    public Vector3 m_position;
    public List<Vector3> m_attractionPoints = new List<Vector3>();

    public abstract void Fill(float numberOfPoints);
    public abstract bool IsInBounds(Vector3 position);
    public abstract float GetTrunkRestingHeight();
}

public class SphereTreeEnvelope : TreeEnvelope
{
    private float m_radius;
    private float m_sqrRadius;

    public SphereTreeEnvelope(Vector3 position, float radius)
    {
        m_position = position;
        m_radius = radius;
        m_sqrRadius = radius * radius;
    }

    public override void Fill(float numberOfPoints)
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            m_attractionPoints.Add(m_position + (Random.insideUnitSphere * m_radius));
        }
    }

    public override bool IsInBounds(Vector3 position)
    {
        return Vector3.SqrMagnitude(position - m_position) <= m_sqrRadius;
    }

    public override float GetTrunkRestingHeight()
    {
        return m_position.y - m_radius / 2;
    }
}

[System.Serializable]
public class SpaceColonisationNode
{
    public SpaceColonisationNode m_parent;
    public List<SpaceColonisationNode> m_children;

    public Vector3 m_position;
    public Vector3 m_direction;
    public float m_crossSection;

    public List<Vector3> m_validAttractionPoints;

    public SpaceColonisationNode(Vector3 position, Vector3 direction, SpaceColonisationNode parent)
    {
        m_position = position;
        m_direction = direction;
        m_parent = parent;
        if (parent != null)
        {
            parent.AddChild(this);
        }
        m_validAttractionPoints = new List<Vector3>();
    }

    public void AddChild(SpaceColonisationNode childNode)
    {
        if(m_children == null)
        {
            m_children = new List<SpaceColonisationNode>();
        }
        m_children.Add(childNode);
    }

    public void ClearAttractionPoints()
    {
        m_validAttractionPoints.Clear();
    }
}

public class SpaceColonisationTree : MonoBehaviour
{
    public float m_stemLength;

    public int m_maxIterations;

    public float m_radiusOfInfluence;
    private float m_sqrRadiusOfInfluence;

    public float m_attractionPointKillDistance;
    private float m_sqrAttractionPointKillDistance;
   

    private List<SpaceColonisationNode> m_nodes = new List<SpaceColonisationNode>();
    private TreeEnvelope m_treeEnvelope;

    private List<SpaceColonisationNode> m_activeNodes = new List<SpaceColonisationNode>();

    private List<SpaceColonisationNode> m_roots = new List<SpaceColonisationNode>();

    private void Start()
    {
        SetInitialReferences();

        m_treeEnvelope.Fill(1000);
        
        D_VisualiseAttractionPoints();
    }

    private void Update()
    {
        for (int i = 0; i < m_roots.Count; i++)
        {
            D_DrawNodeChildren(m_roots[i]);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            SpaceColonisationNode rootNode = new SpaceColonisationNode(Vector3.zero, Vector3.up, null);
            AddNode(rootNode, true);
            CreateTrunk(rootNode);
            StartCoroutine(D_StepThroughGrowth());
        }
    }

    private void D_DrawNodeChildren(SpaceColonisationNode node)
    {
        if (node.m_children != null)
        {
            for (int i = 0; i < node.m_children.Count; i++)
            {
                D_DrawNodeChildren(node.m_children[i]);
                Debug.DrawLine(node.m_position, node.m_children[i].m_position);
            }
        }
    }

    private IEnumerator D_StepThroughGrowth()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            Step();
        }
    }

    private void D_VisualiseAttractionPoints()
    {
        Transform parent;
        if(transform.childCount > 0)
        {
            GameObject.Destroy(transform.GetChild(0).gameObject);
        }
        parent = new GameObject("Attraction Points").transform;
        parent.parent = transform;

        for (int i = 0; i < m_treeEnvelope.m_attractionPoints.Count; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.position = m_treeEnvelope.m_attractionPoints[i];
            obj.transform.localScale = Vector3.one * 0.05f;
            obj.transform.parent = parent;
        }
    }

    private void SetInitialReferences()
    {
        m_treeEnvelope = new SphereTreeEnvelope(Vector3.up * 10, 4.0f);
        m_sqrRadiusOfInfluence = m_radiusOfInfluence * m_radiusOfInfluence;
        m_sqrAttractionPointKillDistance = m_attractionPointKillDistance * m_attractionPointKillDistance;
    }
    

    private void Step()
    {
        SetValidAttractionPoints();
        Grow();
        PruneInfluencePoints();
    }

    private void SetValidAttractionPoints()
    {
        for (int i = 0; i < m_treeEnvelope.m_attractionPoints.Count; i++)
        {
            Vector3 curAttractionPoint = m_treeEnvelope.m_attractionPoints[i];
            float closestSqrDistance = float.MaxValue;
            int closestIndex = -1;
            for (int j = 0; j < m_activeNodes.Count; j++)
            {
                float sqrDistance = Vector3.SqrMagnitude(curAttractionPoint - m_activeNodes[j].m_position);
                if(sqrDistance < m_sqrRadiusOfInfluence)
                {
                    if(sqrDistance < closestSqrDistance)
                    {
                        closestIndex = j;
                    }
                }
            }
            if(closestIndex >= 0)
            {
                m_activeNodes[closestIndex].m_validAttractionPoints.Add(curAttractionPoint);
            }
        }
    }

    private void CreateTrunk(SpaceColonisationNode root)
    {
        int terminationNum = 100;
        SpaceColonisationNode current = m_roots[0];
        Vector3 direction = GetDirectionToAttractionPointCenterOfMass(root.m_position);
        direction = direction.normalized * m_treeEnvelope.GetTrunkRestingHeight();
        while (current.m_position.y < direction.y && terminationNum > 0)
        {
            SpaceColonisationNode newNode = new SpaceColonisationNode(current.m_position + direction.normalized * m_stemLength, direction, current);
            current = newNode;
            m_activeNodes.Add(current);
            terminationNum--;
        }
    }

    private void Grow()
    {
        List<SpaceColonisationNode> newNodes = new List<SpaceColonisationNode>();
        for (int i = 0; i < m_activeNodes.Count; i++)
        {
            if(m_activeNodes[i].m_validAttractionPoints.Count == 0)
            {
                m_activeNodes[i] = m_activeNodes[m_activeNodes.Count - 1];
                m_activeNodes.RemoveAt(m_activeNodes.Count - 1);
                i--;
                continue;
            }
            SpaceColonisationNode currentNode = m_activeNodes[i];
            Vector3 growDirection;
            if(CheckForInfluence(currentNode, out growDirection))
            { 
                Vector3 newNodePosition = currentNode.m_position + (growDirection * m_stemLength);
                SpaceColonisationNode newNode = new SpaceColonisationNode(newNodePosition, growDirection, currentNode);
                newNodes.Add(newNode);
            }
        }
        m_activeNodes.AddRange(newNodes);
    }

    private void PruneInfluencePoints()
    {
        bool shouldVisualise = false;
        for (int i = 0; i < m_treeEnvelope.m_attractionPoints.Count; i++)
        {
            for (int j = 0; j < m_activeNodes.Count; j++)
            {
                if(Vector3.SqrMagnitude(m_treeEnvelope.m_attractionPoints[i] - m_activeNodes[j].m_position) <= m_sqrAttractionPointKillDistance)
                {
                    m_treeEnvelope.m_attractionPoints.RemoveAt(i);
                    --i;
                    shouldVisualise = true;
                    break;
                }
            }
        }
        if(shouldVisualise)
        {
            D_VisualiseAttractionPoints();
        }
    }

    private Vector3 GetDirectionToAttractionPointCenterOfMass(Vector3 position)
    {
        Vector3 averageDirection = Vector3.zero;
        for (int i = 0; i < m_treeEnvelope.m_attractionPoints.Count; i++)
        {
            averageDirection += (m_treeEnvelope.m_attractionPoints[i] - position).normalized;
        }
        return averageDirection.normalized;
    }

    private bool CheckForInfluence(SpaceColonisationNode node, out Vector3 influenceDirection)
    {
        int count = 0;
        Vector3 averageDirection = Vector3.zero;
        for (int i = 0; i < node.m_validAttractionPoints.Count; i++)
        {
            Vector3 direction = node.m_validAttractionPoints[i] - node.m_position;
            if(Vector3.SqrMagnitude(direction) <= m_sqrRadiusOfInfluence)
            {
                averageDirection += direction.normalized;
                ++count;
            }
        }
        node.m_validAttractionPoints.Clear();
        influenceDirection = averageDirection.normalized;
        return count == 0 ? false : true;
    }

    private void AddNode(Vector3 position, Vector3 direction, SpaceColonisationNode parent, bool isRoot = false)
    {
        SpaceColonisationNode newNode = new SpaceColonisationNode(position, direction, parent);
        AddNode(newNode, isRoot);
    }

    private void AddNode(SpaceColonisationNode newNode, bool isRoot = false)
    {
        m_nodes.Add(newNode);
        m_activeNodes.Add(newNode);
        if (isRoot)
        {
            m_roots.Add(newNode);
        }
    }
}
