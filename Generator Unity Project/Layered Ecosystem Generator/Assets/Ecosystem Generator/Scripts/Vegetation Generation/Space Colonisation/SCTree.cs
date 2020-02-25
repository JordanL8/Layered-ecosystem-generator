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

    public bool m_grownToParent = false;

    public SCBranch(SCBranch parent, Vector3 position, Vector3 direction)
    {
        m_parent = parent;
        m_position = position;
        m_direction = direction;
        m_weldDirection = direction;
        m_thickness = 0.02f;
        m_children = new List<SCBranch>();
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
        // Check for duplicates
        Vector3 position = nextBranch.Position;
        for (int i = 0; i < m_children.Count; i++)
        {
            if (Vector3.SqrMagnitude(m_children[i].Position - position) < 0.01f * 0.01f)
            {
                return null;
            }
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

    public float GetPipeRadius()
    {
        float radiusPower = 0.0f;
        for (int i = 0; i < ChildCount; i++)
        {
            radiusPower += Mathf.Pow(m_children[i].m_thickness, 2.0f);
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
        SCBranch copy = new SCBranch(m_parent, m_position, m_direction);
        copy.m_thickness = m_thickness;
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

    public int m_maxGrowthIterations;

    private List<SCLeaf> m_leaves = new List<SCLeaf>();
    private List<SCBranch> m_branches = new List<SCBranch>();

    [Header("Rendering")]
    public Material m_branchMaterial;
    public GameObject m_leafPrefab;

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
        OptimiseBranch(m_branches[0]);
        CalculateBranchThickness();
        BuildMesh();
        //D_DrawLeaves();
    }

    
    private void InitialiseLeaves()
    {
        for (int i = 0; i < 100; i++)
        {
            SCLeaf leaf = new SCLeaf(transform.position + Random.insideUnitSphere * 2.0f + Vector3.up * 6.0f);
            m_leaves.Add(leaf);
        }

        for (int i = 0; i < 100; i++)
        {
            SCLeaf leaf = new SCLeaf(transform.position + Random.insideUnitSphere * 2.0f + Vector3.up * 10.0f);
            m_leaves.Add(leaf);
        }

        for (int i = 0; i < 100; i++)
        {
            SCLeaf leaf = new SCLeaf(transform.position + new Vector3(0, 0, -2.0f) + Random.insideUnitSphere * 2.0f + Vector3.up * 7.5f);
            m_leaves.Add(leaf);
        }

        for (int i = 0; i < 100; i++)
        {
            SCLeaf leaf = new SCLeaf(transform.position + new Vector3(0, 0, 2.0f) + Random.insideUnitSphere * 2.0f + Vector3.up * 7.5f);
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
                if (nextBranch != null)
                {
                    m_branches.Add(nextBranch);
                    currentBranch = nextBranch;
                }
            }
            d_I++;
        }
    }

    private void GrowTree()
    {
        int maxGrowthIterations = m_maxGrowthIterations;
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
                    RemoveLeaf(i, curLeaf, curBranch);
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
                if (newBranch != null)
                {
                    m_branches.Add(newBranch);
                    curBranch.Reset();
                    grew = true;
                }
            }
        }
        return grew;
    }

    private void RemoveLeaf(int index, SCLeaf leaf, SCBranch branch)
    {
        m_leaves.RemoveAt(index);
        GameObject newLeaf = Instantiate(m_leafPrefab);
        Vector3 leafUp = leaf.Position - branch.Position;
        //newLeaf.transform.localScale = Vector3.one * Vector3.Distance(leaf.Position, branch.Position);
        newLeaf.transform.position = branch.Position + (leafUp.normalized * newLeaf.transform.localScale.x / 2.0f);
        newLeaf.transform.up = leafUp;
    }

    private void OptimiseBranch(SCBranch branch)
    {
        SCBranch curBranch = branch;
        while(curBranch != null)
        {
            int childCount = curBranch.ChildCount;
            if (childCount == 0) { break; }

            if (childCount == 1)
            {
                SCBranch childBranch = curBranch.GetChild(0);
                if (Vector3.Dot(curBranch.Direction, childBranch.Direction) > 0.98f)
                {
                    if (curBranch.m_parent != null)
                    {
                        curBranch.m_parent.RemoveChild(curBranch);
                        curBranch.m_parent.AddChild(childBranch);
                        m_branches.Remove(curBranch);
                    }
                }
                curBranch = childBranch;
            }
            else
            {
                for (int i = 1; i < childCount; i++)
                {
                    OptimiseBranch(curBranch.GetChild(i));
                }
                curBranch = curBranch.GetChild(0);
            }
        }
    }

    private void PruneBranch(SCBranch branch)
    {

    }

    private void CalculateBranchThickness()
    {
        List<SCBranch> branchEnds = new List<SCBranch>();
        SCBranch root = m_branches[0];
        GetBranchEnds(root, ref branchEnds);

        for (int i = 0; i < branchEnds.Count; i++)
        {
            SCBranch curBranch = branchEnds[i];

            while(curBranch != null)
            {
                curBranch.m_hasHadThicknessVisit = true;

                SCBranch parentBranch = curBranch.m_parent;
                if (parentBranch == null)
                {
                    break;
                }
                
                if (parentBranch.ChildCount > 1)
                {
                    if (!parentBranch.BeenReachedByAllChildren())
                    {
                        break;
                    }
                    float radius = parentBranch.GetPipeRadius();
                    parentBranch.m_thickness = radius;
                }
                else
                {
                    parentBranch.m_thickness = curBranch.m_thickness;
                }
                curBranch = parentBranch;
            }
        }
    }

    private void GetBranchEnds(SCBranch curBranch, ref List<SCBranch> branchEnds)
    {
        if(curBranch.ChildCount == 0)
        {
            branchEnds.Add(curBranch);
        }
        for (int i = 0; i < curBranch.ChildCount; i++)
        {
            GetBranchEnds(curBranch.GetChild(i), ref branchEnds);
        }
    }

    private void BuildMesh()
    {
        Transform branchObjectTransform = new GameObject("Branches").transform;
        branchObjectTransform.parent = transform;
        branchObjectTransform.localPosition = Vector3.zero;
        SCMeshGenerator.BuildTree(m_branches[0], branchObjectTransform, m_leafPrefab);

        // Combine Meshes
        MeshFilter[] meshFilters = branchObjectTransform.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            Destroy(meshFilters[i].gameObject);
        }
        MeshFilter myMeshFilter = gameObject.AddComponent<MeshFilter>();
        myMeshFilter.mesh = new Mesh();
        myMeshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        myMeshFilter.mesh.CombineMeshes(combine, true);
        gameObject.AddComponent<MeshRenderer>().sharedMaterial = m_branchMaterial;
    }

    private void D_DrawLeaves()
    {
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
