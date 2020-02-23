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

    private List<SCBranch> m_children;

    public int ChildCount
    {
        get { return m_children.Count; }
    }

    public SCBranch GetChild(int i)
    {
        if(i < m_children.Count)
        {
            return m_children[i];
        }
        return null;
    }

    public bool m_grownToParent = false;

    public SCBranch(SCBranch parent, Vector3 position, Vector3 direction)
    {
        m_parent = parent;
        m_position = position;
        m_direction = direction;
        m_weldDirection = direction;
        m_thickness = 0.05f;
        m_children = new List<SCBranch>();
    }

    private void AddChild(SCBranch child)
    {
        m_children.Add(child);
    }

    public SCBranch Next(float length)
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
            nextBranch = new SCBranch(this, m_position + nextDirection.normalized * length, nextDirection.normalized);
            
        }
        else
        {
            nextBranch = new SCBranch(this, m_position + m_direction.normalized * length, m_direction.normalized);
        }
        m_children.Add(nextBranch);
        return nextBranch;
    } 

    public void Reset()
    {
        m_leafPositions.Clear();
        m_direction = m_weldDirection;
        m_count = 0;
    }

    public void D_Draw()
    {
        if (m_parent != null)
        {
            Debug.DrawLine(m_position, m_parent.Position);
        }
    }
}

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

public class SCTree : MonoBehaviour
{
    public float m_leafKillDistance;
    private float m_sqrLeafKillDistance;

    public float m_interactionDistance;
    private float m_sqrInteractionDistance;

    public float m_branchLength;

    private List<SCLeaf> m_leaves = new List<SCLeaf>();
    private List<SCBranch> m_branches = new List<SCBranch>();

    [Header("Rendering")]
    public Material m_branchMaterial;


    private void Start()
    {
        //Generate();
    }


    private void Update()
    {
        //D_DrawBranches();
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Generate();
        }
    }

    private void Generate()
    {
        m_sqrLeafKillDistance = m_leafKillDistance * m_leafKillDistance;
        m_sqrInteractionDistance = m_interactionDistance * m_interactionDistance;
        InitialiseLeaves();
        InitialiseTree();
        GrowTree();
        BuildMesh();
        //D_DrawLeaves();
    }

    
    private void InitialiseLeaves()
    {
        for (int i = 0; i < 100; i++)
        {
            SCLeaf leaf = new SCLeaf(transform.position + Random.insideUnitSphere * 2.5f + Vector3.up * 7.5f);
            m_leaves.Add(leaf);
        }

        for (int i = 0; i < 100; i++)
        {
            SCLeaf leaf = new SCLeaf(transform.position + Random.insideUnitSphere * 2.5f + Vector3.up * 10.0f);
            m_leaves.Add(leaf);
        }
    }

    private void InitialiseTree()
    {
        SCBranch root = new SCBranch(null, transform.position, Vector3.up);
        m_branches.Add(root);

        int d_I = 0;
        bool isInRange = false;
        SCBranch currentBranch = root;

        while (!isInRange && d_I < 1000)
        {
            for (int i = 0; i < m_leaves.Count; i++)
            {
                if (Vector3.SqrMagnitude(m_leaves[i].Position - currentBranch.Position) < m_sqrInteractionDistance)
                {
                    isInRange = true;
                }
            }

            if(!isInRange)
            {
                SCBranch nextBranch = currentBranch.Next(m_branchLength);
                m_branches.Add(nextBranch);
                currentBranch = nextBranch;
            }
            d_I++;
        }
    }


    private void GrowTree()
    {
        int maxGrowthIterations = 40;
        while (GrowthStep() && maxGrowthIterations> 0)
        {
            maxGrowthIterations--;
        }
    }

    private bool GrowthStep()
    {
        for (int i = 0; i < m_leaves.Count; i++)
        {
            SCLeaf curLeaf = m_leaves[i];
            SCBranch closestBranchToLeaf = null;
            float closestDistance = float.MaxValue;
            for (int j = 0; j < m_branches.Count; j++)
            {
                SCBranch curBranch = m_branches[j];
                float sqrDistance = Vector3.SqrMagnitude(curLeaf.Position - curBranch.Position);
                if(sqrDistance < m_sqrLeafKillDistance)
                {
                    // Remove Leaf
                    m_leaves.RemoveAt(i);
                    i--;
                    closestBranchToLeaf = null;
                    break;
                }
                else if(sqrDistance < closestDistance)
                {
                    closestBranchToLeaf = curBranch;
                    closestDistance = sqrDistance;
                }
            }

            if(closestBranchToLeaf != null)
            {
                closestBranchToLeaf.m_leafPositions.Add(curLeaf.Position);
                Vector3 newDirection = (curLeaf.Position - closestBranchToLeaf.Position).normalized;
                closestBranchToLeaf.m_count++;
            }
        }

        bool grew = false;
        for (int i = m_branches.Count - 1; i >= 0; i--)
        {
            SCBranch curBranch = m_branches[i];
            if(curBranch.m_count > 0)
            {
                SCBranch newBranch = curBranch.Next(m_branchLength);
                m_branches.Add(newBranch);
                curBranch.Reset();
                grew = true;
            }
        }
        return grew;
    }

    private void BuildMesh()
    {
        Transform branchObjectTransform = new GameObject("Branches").transform;
        branchObjectTransform.parent = transform;
        branchObjectTransform.localPosition = Vector3.zero;
        SCMeshGenerator.BuildTree(m_branches[0], branchObjectTransform);

        // Combine Meshes
        MeshFilter[] meshFilters = branchObjectTransform.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            Destroy(meshFilters[i].gameObject);
        }
        gameObject.AddComponent<MeshFilter>().mesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true);
        gameObject.AddComponent<MeshRenderer>().sharedMaterial = m_branchMaterial;
    }

    private void D_DrawLeaves()
    {
        if (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
        Transform leafParent = new GameObject("D_Leaves").transform;
        leafParent.parent = transform;
        for (int i = 0; i < m_leaves.Count; i++)
        {
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaf.transform.localScale = Vector3.one * 0.05f;
            leaf.transform.position = m_leaves[i].Position;
            leaf.transform.parent = leafParent;
        }
    }

    private void D_DrawBranches()
    {
        for (int i = 0; i < m_branches.Count; i++)
        {
            //float thicknessMultiplier = i / m_branches.Count;
            m_branches[i].D_Draw();
        }
    }
}
