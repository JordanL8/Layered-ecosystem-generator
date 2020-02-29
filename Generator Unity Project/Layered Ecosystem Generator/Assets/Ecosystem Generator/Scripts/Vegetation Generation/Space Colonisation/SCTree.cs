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

    public SCBranch(SCBranch parent, Vector3 position, Vector3 direction, float thickness)
    {
        m_parent = parent;
        m_position = position;
        m_direction = direction;
        m_weldDirection = direction;
        m_thickness = thickness;
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
    [Header("Branches")]
    public float m_branchLength;
    public float m_branchEndThickness = 0.01f;
    public float m_branchConnectionPower = 2.0f;
    public int m_maxGrowthIterations;

    [Header("Leaves")]
    public bool m_addLeaves = true;
    public float m_leafDensity = 10.0f;
    public float m_leafKillDistance;
    private float m_sqrLeafKillDistance;

    public float m_interactionDistance;
    private float m_sqrInteractionDistance;

    public float m_topCanopyWidth;
    public float m_middleCanopyWidth;
    public float m_bottomCanopyWidth;

    [Header("Rendering")]
    public Material m_branchMaterial;
    public GameObject m_leafPrefab;
    public Material m_leafMaterial;

    [Header("Leaf Volume")]
    public SCVolume m_volume;

    private List<SCLeaf> m_leaves = new List<SCLeaf>();
    private List<SCBranch> m_branches = new List<SCBranch>();

    private Transform m_branchObjectTransform;
    private Transform m_leafObjectTransform;

    private void Start()
    {
        SetInitialReferences();
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

    private void SetInitialReferences()
    {
        m_branchObjectTransform = new GameObject("Branches").transform;
        m_branchObjectTransform.parent = transform;
        m_branchObjectTransform.localPosition = Vector3.zero;
        if (m_addLeaves)
        {
            m_leafObjectTransform = new GameObject("Leaves").transform;
            m_leafObjectTransform.parent = transform;
            m_leafObjectTransform.localPosition = Vector3.zero;
        }
    }

    public void Generate()
    {
        if(m_volume == null)
        {
            Debug.LogError("SCTree has no SCVolume attached. Can not generate a tree.");
            return;
        }
        if(m_branchObjectTransform == null)
        {
            SetInitialReferences();
        }

        m_sqrLeafKillDistance = m_leafKillDistance * m_leafKillDistance;
        m_sqrInteractionDistance = m_interactionDistance * m_interactionDistance;
        InitialiseLeaves();
        InitialiseTree();
        GrowTree();
        OptimiseBranch(m_branches[0]);
        CalculateBranchThickness();
        BuildMeshes();
        D_DrawLeaves();
    }
    

    private void InitialiseLeaves()
    {
        if (m_volume != null)
        {
            m_leaves = m_volume.GetLeavesList(transform, m_leafDensity, m_bottomCanopyWidth, m_middleCanopyWidth, m_topCanopyWidth);
        }
        //for (int i = 0; i < 100; i++)
        //{
        //    SCLeaf leaf = new SCLeaf(transform.position + Random.insideUnitSphere * 2.0f + Vector3.up * 10.0f);
        //    m_leaves.Add(leaf);
        //}

        //for (int i = 0; i < 100; i++)
        //{
        //    SCLeaf leaf = new SCLeaf(transform.position + new Vector3(0, 0, -2.0f) + Random.insideUnitSphere * 2.0f + Vector3.up * 7.5f);
        //    m_leaves.Add(leaf);
        //}

        //for (int i = 0; i < 100; i++)
        //{
        //    SCLeaf leaf = new SCLeaf(transform.position + new Vector3(0, 0, 2.0f) + Random.insideUnitSphere * 2.0f + Vector3.up * 7.5f);
        //    m_leaves.Add(leaf);
        //}
    }

    private void InitialiseTree()
    {
        SCVolumeShape trunkShape = m_volume.m_volumeShapes[0];
        SCBranch root = new SCBranch(null, trunkShape.m_boundingPoints[0], Vector3.up, m_branchEndThickness);
        m_branches.Add(root);
        SCBranch currentBranch = root;

        for (int i = 1; i < trunkShape.m_boundingPoints.Count; i++)
        {
            Vector3 nextPoint = trunkShape.m_boundingPoints[i];
            float distance = Vector3.Distance(currentBranch.Position, nextPoint);
            int connectingBranchNumber = Mathf.Max(Mathf.FloorToInt(distance / m_branchLength), 1);
            for (int j = 0; j < connectingBranchNumber; j++)
            {
                Vector3 nextDirection = (nextPoint - currentBranch.Position).normalized;
                SCBranch nextBranch = new SCBranch(currentBranch, currentBranch.Position + nextDirection * m_branchLength, nextDirection, m_branchEndThickness);
                currentBranch.AddChild(nextBranch);
                m_branches.Add(nextBranch);
                currentBranch = nextBranch;
            }
        }


        //SCBranch root = new SCBranch(null, transform.position, Vector3.up);
        //m_branches.Add(root);
        //SCBranch currentBranch = root;
        //int d_I = 0;
        //bool isInRange = false;

        //while (!isInRange && d_I < 1000)
        //{
        //    for (int i = 0; i < m_leaves.Count; i++)
        //    {
        //        if (Vector3.SqrMagnitude(m_leaves[i].Position - currentBranch.Position) < m_sqrInteractionDistance)
        //        {
        //            isInRange = true;
        //        }
        //    }

        //    if (!isInRange)
        //    {
        //        SCBranch nextBranch = currentBranch.Next(m_branchLength);
        //        if (nextBranch != null)
        //        {
        //            m_branches.Add(nextBranch);
        //            currentBranch = nextBranch;
        //        }
        //    }
        //    d_I++;
        //}
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
        if (m_addLeaves)
        {
            GameObject newLeaf = Instantiate(m_leafPrefab);
            Vector3 leafUp = leaf.Position - branch.Position;
            //newLeaf.transform.localScale = Vector3.one * Vector3.Distance(leaf.Position, branch.Position);
            newLeaf.transform.position = branch.Position + (leafUp.normalized * newLeaf.transform.localScale.x / 2.0f);
            newLeaf.transform.up = leafUp;
            newLeaf.transform.parent = m_leafObjectTransform;
        }
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
                    float radius = parentBranch.GetPipeRadius(m_branchConnectionPower);
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

    private void BuildMeshes()
    {
        SCMeshGenerator.BuildTree(m_branches[0], m_branchObjectTransform, m_leafPrefab);
        CombineMeshes(m_branchObjectTransform, m_branchMaterial);
        if (m_addLeaves)
        {
            CombineMeshes(m_leafObjectTransform, m_leafMaterial);
        }
    }

    private void CombineMeshes(Transform parent, Material material)
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(parent.GetChild(i).gameObject);
            }
            else
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
        MeshFilter myMeshFilter = parent.gameObject.AddComponent<MeshFilter>();
        myMeshFilter.sharedMesh = new Mesh();
        myMeshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        myMeshFilter.sharedMesh.CombineMeshes(combine, true);
        parent.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
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
