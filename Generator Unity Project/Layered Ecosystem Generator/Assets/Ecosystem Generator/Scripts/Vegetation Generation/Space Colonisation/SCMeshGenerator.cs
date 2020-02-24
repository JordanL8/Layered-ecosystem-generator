using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SCMeshGenerator
{
    private class SCMeshGeneratorBranchList
    {
        public List<SCBranch> branches = new List<SCBranch>();
        public int m_recursionNumber;
    }

    public static void BuildTree(SCBranch root, Transform parent, GameObject leafPrefab)
    {
        BuildBranches(root, 0, leafPrefab, parent);
    }

    private static void BuildBranches(SCBranch branchStart, int recursionNumber, GameObject leafPrefab, Transform parent = null)
    {
        SCMeshGeneratorBranchList branchList = new SCMeshGeneratorBranchList();
        if(branchStart.m_parent != null)
        {
            branchList.branches.Add(branchStart.m_parent);
        }
        branchList.branches.Add(branchStart);
        branchList.m_recursionNumber = recursionNumber;

        SCBranch curBranch = branchStart;
        while (curBranch != null)
        {
            for (int i = 0; i < curBranch.ChildCount; i++)
            {
                if (i == 0)
                {
                    SCBranch firstChildBranch = curBranch.GetChild(0);
                    branchList.branches.Add(firstChildBranch);
                }
                else
                {
                    BuildBranches(curBranch.GetChild(i), recursionNumber + 1, leafPrefab, parent);
                }
            }
            curBranch = curBranch.GetChild(0);
        }

        // Build


        MeshBuilder meshBuilder = new MeshBuilder();
        Quaternion rotation = Quaternion.identity;
        float height = 0.0f;
        for (int j = 0; j < branchList.branches.Count - 1; j++)
        {
            height += Vector3.Magnitude(branchList.branches[j].Position - branchList.branches[j + 1].Position);
        }
        float circumference = branchList.branches[0].m_thickness * 2 * Mathf.PI;
        float ratio = height / circumference;

        for (int j = 0; j < branchList.branches.Count; j++)
        {
            SCBranch currentBranch = branchList.branches[j];
            if (j + 1 < branchList.branches.Count)
            {
                rotation = Quaternion.LookRotation(branchList.branches[j + 1].Position - currentBranch.Position);
            }
            float v = (float)j / branchList.branches.Count * ratio;
            meshBuilder.BuildRing(currentBranch.Position, rotation, 6, currentBranch.m_thickness, v, j > 0);

            if(j > 0 && recursionNumber > 0)
            {
                //CreateLeaf(leafPrefab,currentPosition, rotation, currentRadius, 0.5f);
            }

            if (j == branchList.branches.Count - 1)
            {
                meshBuilder.BuildCap(currentBranch.Position, rotation, 6, currentBranch.m_thickness);
            }
        }
        Mesh segmentMesh = meshBuilder.CreateMesh();
        GameObject obj = new GameObject();
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>().sharedMesh = segmentMesh;
        obj.transform.parent = parent;



    }

    private static void CreateLeaf(GameObject leafPrefab, Vector3 position, Quaternion rotation, float radius, float chance = 1.0f)
    {
        if (Random.value <= chance)
        {
            GameObject newLeaf = GameObject.Instantiate(leafPrefab);
            //newLeaf.transform.localScale = Vector3.one * Vector3.Distance(leaf.Position, branch.Position);
            
            Vector3 eularRotation = rotation.eulerAngles;
            //eularRotation = new Vector3(eularRotation.z, 0, -eularRotation.x);
            newLeaf.transform.rotation = rotation;// Quaternion.LookRotation(eularRotation - position);

            newLeaf.transform.position = position + rotation.eulerAngles.normalized * (radius + newLeaf.transform.localScale.x/2.0f);
        }
    }
}
