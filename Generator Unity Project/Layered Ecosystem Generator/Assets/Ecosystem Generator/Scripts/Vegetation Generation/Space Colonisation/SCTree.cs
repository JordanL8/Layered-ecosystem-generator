using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SCTree : MonoBehaviour
{
    [Header("Branches")]
    public float m_branchLength = 0.3f;
    public float m_branchEndThickness = 0.01f;
    public float m_branchConnectionPower = 2.0f;
    public int m_maxGrowthIterations = 80;
    [Range(0.8f,1.0f)]
    public float m_optimsationAngle = 0.98f;
    public bool m_overrideRotationSegments = false;
    [Min(3)]
    public int m_overrideRotationSegmentNum = 8;

    [Header("Leaves")]
    public bool m_addLeaves = true;
    public float m_leafDensity = 20.0f;
    public float m_leafKillDistance = 0.4f;
    private float m_sqrLeafKillDistance;

    public float m_interactionDistance = 0.8f;
    private float m_sqrInteractionDistance;

    [Range(0,1)]
    public float m_topCanopyWidth = 0.1f;
    [Range(0, 1)]
    public float m_middleCanopyWidth = 1.0f;
    [Range(0, 1)]
    public float m_bottomCanopyWidth = 0.1f;

    [Header("Rendering")]
    public Material m_branchMaterial;
    public GameObject m_leafPrefab;
    public Material m_leafMaterial;

    [Header("Leaf Volume")]
    public SCVolume m_volume;

    private List<SCLeaf> m_leaves = new List<SCLeaf>();
    private List<SCBranch> m_branches = new List<SCBranch>();

    private List<Vector3> m_leafPositions = new List<Vector3>();
    private List<Vector3> m_leafOrientations = new List<Vector3>();

    public void Generate(int lodLevel)
    {
        if(m_volume == null)
        {
            Debug.LogError("SCTree has no SCVolume attached. Can not generate a tree.");
            return;
        }
        m_leaves = new List<SCLeaf>();
        m_branches = new List<SCBranch>();

        m_sqrLeafKillDistance = m_leafKillDistance * m_leafKillDistance;
        m_sqrInteractionDistance = m_interactionDistance * m_interactionDistance;
        InitialiseLeaves();
        InitialiseTree();
        GrowTree();
        OptimiseBranch(m_branches[0]);
        CalculateBranchThickness();

        //D_DrawLeaves();
        BuildMeshes(lodLevel, m_overrideRotationSegments, m_overrideRotationSegmentNum);
    }
    

    private void InitialiseLeaves()
    {
        if (m_volume != null)
        {
            m_leaves = m_volume.GetLeavesList(transform, m_leafDensity, m_bottomCanopyWidth, m_middleCanopyWidth, m_topCanopyWidth);
        }
    }

    private void InitialiseTree()
    {
        m_leafPositions = new List<Vector3>();
        m_leafOrientations = new List<Vector3>();

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
                //Vector3 nextDirection = i + 1 < trunkShape.m_boundingPoints.Count ? Vector3.Lerp((nextPoint - currentBranch.Position).normalized, (trunkShape.m_boundingPoints[i + 1] - nextPoint).normalized, (float)j / (float)(connectingBranchNumber - 1)).normalized : (nextPoint - currentBranch.Position).normalized;
                Vector3 nextDirection = i + 1 < trunkShape.m_boundingPoints.Count ? ((trunkShape.m_boundingPoints[i + 1] - currentBranch.Position).normalized * 0.5f + (nextPoint - currentBranch.Position).normalized).normalized : (nextPoint - currentBranch.Position).normalized;

                bool canGrow = (j == connectingBranchNumber - 1); //|| (i == trunkShape.m_boundingPoints.Count - 1);
                SCBranch nextBranch = new SCBranch(currentBranch, currentBranch.Position + nextDirection * m_branchLength, nextDirection, m_branchEndThickness, canGrow);
                currentBranch.AddChild(nextBranch);
                m_branches.Add(nextBranch);
                currentBranch = nextBranch;
            }
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
            if(curBranch.m_count > 0 && curBranch.m_canGrow)
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
            Vector3 leafUp = leaf.Position - branch.Position;
            m_leafPositions.Add(branch.Position + (leafUp.normalized * m_leafPrefab.transform.localScale.x / 2.0f));
            m_leafOrientations.Add(leafUp);


            //newLeaf.transform.position = branch.Position + (leafUp.normalized * newLeaf.transform.localScale.x / 2.0f);
            //newLeaf.transform.up = leafUp;
            //newLeaf.transform.parent = transform;
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
                if (Vector3.Dot(curBranch.Direction, childBranch.Direction) > m_optimsationAngle)
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

    public void BuildMeshes(int LodLevel, bool overrideRotationSegments = false, int overrideRotationSegmentsNum = 8)
    {
        if (overrideRotationSegments)
        {
            Transform m_lodTransform = new GameObject($"Tree").transform;
            m_lodTransform.parent = transform;
            m_lodTransform.localPosition = Vector3.zero;

            Transform m_branchTransform = new GameObject("Branches").transform;
            m_branchTransform.parent = m_lodTransform;
            m_branchTransform.localPosition = Vector3.zero;
            Transform m_leavesTransform = new GameObject("Leaves").transform;
            m_leavesTransform.parent = m_lodTransform;
            m_leavesTransform.localPosition = Vector3.zero;

            SCMeshGenerator.BuildTree(m_branches[0], m_branchTransform, m_leafPrefab, 0, overrideRotationSegmentsNum);
            CombineMeshes(m_branchTransform, m_branchMaterial);

            InstantiateLeaves(m_leavesTransform);
            CombineMeshes(m_leavesTransform, m_leafMaterial);
        }
        else
        {
            for (int i = 0; i <= LodLevel; i++)
            {
                Transform m_lodTransform = new GameObject($"LOD {i}").transform;
                m_lodTransform.parent = transform;
                m_lodTransform.localPosition = Vector3.zero;

                Transform m_branchTransform = new GameObject("Branches").transform;
                m_branchTransform.parent = m_lodTransform;
                m_branchTransform.localPosition = Vector3.zero;
                Transform m_leavesTransform = new GameObject("Leaves").transform;
                m_leavesTransform.parent = m_lodTransform;
                m_leavesTransform.localPosition = Vector3.zero;

                SCMeshGenerator.BuildTree(m_branches[0], m_branchTransform, m_leafPrefab, i);
                CombineMeshes(m_branchTransform, m_branchMaterial);

                InstantiateLeaves(m_leavesTransform);
                CombineMeshes(m_leavesTransform, m_leafMaterial);
            }
        }
    }

    private void InstantiateLeaves(Transform parent)
    {
        for (int i = 0; i < m_leafPositions.Count; i++)
        {
            GameObject newLeaf = Instantiate(m_leafPrefab);
            newLeaf.transform.parent = parent;
            newLeaf.transform.position = m_leafPositions[i];
            newLeaf.transform.up = m_leafOrientations[i];
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
        myMeshFilter.sharedMesh.RecalculateNormals();
        myMeshFilter.sharedMesh.RecalculateTangents();
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
