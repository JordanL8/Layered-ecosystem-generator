using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LSystemBranchMeshGenerator
{
    public static Mesh BuildBranch(LSystemBranch branch)
    {
        MeshBuilder meshBuilder = new MeshBuilder();
        Quaternion rotation = Quaternion.identity;
        float height = 0.0f;
        for (int i = 0; i < branch.m_branchPositions.Count - 1; i++)
        {
            height += Vector3.Magnitude(branch.m_branchPositions[i].m_position - branch.m_branchPositions[i + 1].m_position);
        }
        float circumference = branch.m_branchPositions[0].m_radius * 2 * Mathf.PI;
        float ratio = height / circumference;
        for (int i = 0; i < branch.m_branchPositions.Count; i++)
        {
            LSystemBranchPosition currentPosition = branch.m_branchPositions[i];
            if(i + 1 < branch.m_branchPositions.Count)
            {
                rotation = Quaternion.LookRotation(branch.m_branchPositions[i + 1].m_position - currentPosition.m_position);
            }
            float v = (float)i / branch.m_branchPositions.Count * ratio;
            meshBuilder.BuildRing(currentPosition.m_position, rotation, 6, currentPosition.m_radius, v, i > 0);
            if(i == branch.m_branchPositions.Count - 1)
            {
                meshBuilder.BuildCap(currentPosition.m_position, rotation, 6, currentPosition.m_radius);
            }
        }

        return meshBuilder.CreateMesh();
    }
}
