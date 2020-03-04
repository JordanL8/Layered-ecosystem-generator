using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LSystemBranchMeshGenerator
{
    public static Mesh BuildBranch(LSystemBranch branch, int lodLevel, float minRadiusThreshold)
    {
        int segments = 0;
        switch (lodLevel)
        {
            case 0:
                {
                    segments = 6;
                }
                break;
            case 1:
                {
                    segments = 3;
                }
                break;
            default:
                {
                    segments = 2;
                }
                break;
        }

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
            if (lodLevel >= 1)
            {
                if (currentPosition.m_radius / lodLevel <= minRadiusThreshold)
                {
                    continue;
                }
            }
            else
            {
                if(currentPosition.m_radius <= minRadiusThreshold * 2)
                {
                    segments = 3;
                }
            }
            if(i + 1 < branch.m_branchPositions.Count)
            {
                rotation = Quaternion.LookRotation(branch.m_branchPositions[i + 1].m_position - currentPosition.m_position);
            }
            float v = (float)i / branch.m_branchPositions.Count * ratio;
            meshBuilder.BuildRing(currentPosition.m_position, rotation, segments, currentPosition.m_radius, v, i > 0);
            if (i == branch.m_branchPositions.Count - 1 && (lodLevel < 1 && currentPosition.m_radius > 0.05f)) 
            {
                meshBuilder.BuildCap(currentPosition.m_position, rotation, segments, currentPosition.m_radius);
            }
        }

        return meshBuilder.CreateMesh();
    }
}
