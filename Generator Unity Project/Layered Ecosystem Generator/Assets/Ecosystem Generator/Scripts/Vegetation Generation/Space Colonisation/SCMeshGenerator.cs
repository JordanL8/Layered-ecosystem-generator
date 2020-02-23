using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SCMeshGenerator
{
    private class SCMeshGeneratorBranchList
    {
        public List<Vector3> m_positions = new List<Vector3>();
        public int m_recursionNumber;
    }

    public static void BuildTree(SCBranch root, Transform parent)
    {
        int maxRecursionDepth = FindMaxRecursionDepth(root, 0);
        BuildBranches(root, 0, 0.075f, 0.001f, maxRecursionDepth, parent);
    }

    private static int FindMaxRecursionDepth(SCBranch branch, int currentDepth)
    {
        int maxFoundDepth = 0;
        while (branch != null)
        {
            for (int i = 0; i < branch.ChildCount; i++)
            {
                if (i > 0)
                {
                    int depth = FindMaxRecursionDepth(branch.GetChild(i), currentDepth + 1);
                    if(depth > maxFoundDepth) { maxFoundDepth = depth; }
                }
            }
            branch = branch.GetChild(0);
        }
        return maxFoundDepth > currentDepth ? maxFoundDepth : currentDepth;
    }

    private static void BuildBranches(SCBranch branchStart, int recursionNumber, float maxBranchThickness, float minBranchThickness, int maximumRecursionDepth, Transform parent = null)
    {
        SCMeshGeneratorBranchList branchList = new SCMeshGeneratorBranchList();
        if(branchStart.m_parent != null)
        {
            branchList.m_positions.Add(branchStart.m_parent.Position);
        }
        branchList.m_positions.Add(branchStart.Position);
        branchList.m_recursionNumber = recursionNumber;

        SCBranch curBranch = branchStart;
        while (curBranch != null)
        {
            for (int i = 0; i < curBranch.ChildCount; i++)
            {
                if (i == 0) { branchList.m_positions.Add(curBranch.GetChild(0).Position); }
                else
                {
                    BuildBranches(curBranch.GetChild(i), recursionNumber + 1, maxBranchThickness, minBranchThickness, maximumRecursionDepth, parent);
                }
            }
            curBranch = curBranch.GetChild(0);
        }

        // Build

        float radiusStep = (float)(recursionNumber) / (float)maximumRecursionDepth;
        if (radiusStep > 0.05f)
        {
            radiusStep += 0.3f;
        }
        float radius = Mathf.Lerp(maxBranchThickness, minBranchThickness, radiusStep);

        MeshBuilder meshBuilder = new MeshBuilder();
        Quaternion rotation = Quaternion.identity;
        float height = 0.0f;
        for (int j = 0; j < branchList.m_positions.Count - 1; j++)
        {
            height += Vector3.Magnitude(branchList.m_positions[j] - branchList.m_positions[j + 1]);
        }
        float circumference = radius * 2 * Mathf.PI;
        float ratio = height / circumference;

        for (int j = 0; j < branchList.m_positions.Count; j++)
        {
            Vector3 currentPosition = branchList.m_positions[j];
            if (j + 1 < branchList.m_positions.Count)
            {
                rotation = Quaternion.LookRotation(branchList.m_positions[j + 1] - currentPosition);
            }
            float v = (float)j / branchList.m_positions.Count * ratio;
            meshBuilder.BuildRing(currentPosition, rotation, 6, radius, v, j > 0);
            if (j == branchList.m_positions.Count - 1)
            {
                meshBuilder.BuildCap(currentPosition, rotation, 6, radius);
            }
        }
        Mesh segmentMesh = meshBuilder.CreateMesh();
        GameObject obj = new GameObject();
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>().sharedMesh = segmentMesh;
        obj.transform.parent = parent;
    }
}
