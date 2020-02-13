using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LSystemBranchMeshGenerator
{
    public static Mesh Start()
    {
        float bendAngleRadians = 180 * Mathf.Deg2Rad;
        float bendRadius = 5 / bendAngleRadians;
        float angleInc = bendAngleRadians / 10;
        Vector3 startOffset = new Vector3(bendRadius, 0.0f, 0.0f);

        MeshBuilder meshBuilder = new MeshBuilder();
        for (int i = 0; i <= 10; i++)
        {
            Vector3 centrePos = Vector3.zero;
            centrePos.x = Mathf.Cos(angleInc * i);
            centrePos.y = Mathf.Sin(angleInc * i);

            float zAngleDegrees = angleInc * i * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, zAngleDegrees);

            centrePos *= bendRadius;
            centrePos -= startOffset;

            float v = (float)i / 10;

            BuildRing(meshBuilder, centrePos, rotation, 6, 1, v, i > 0);
        }

        //MeshBuilder meshBuilder = new MeshBuilder();
        //BuildRing(meshBuilder, 6, Vector3.zero, 1, 0.0f, false);
        //BuildRing(meshBuilder, 6, Vector3.up * m_height, 1, 1.0f, true);

        //BuildCap(meshBuilder, 6, Vector3.zero, 1, true);
        //BuildCap(meshBuilder, 6, Vector3.up * m_height, 1, false);

        return meshBuilder.CreateMesh();
    }

    public static Mesh BuildBranch(LSystemBranch branch)
    {
        MeshBuilder meshBuilder = new MeshBuilder();
        Quaternion rotation = Quaternion.identity;
        for (int i = 0; i < branch.m_branchPositions.Count; i++)
        {
            LSystemPosition currentPosition = branch.m_branchPositions[i];
            if(i + 1 < branch.m_branchPositions.Count)
            {
                rotation = Quaternion.LookRotation(branch.m_branchPositions[i + 1].m_position - currentPosition.m_position);
            }
            float v = (float)i / branch.m_branchPositions.Count;
            BuildRing(meshBuilder, currentPosition.m_position, rotation, 6, currentPosition.m_radius, v, i > 0);
        }

        return meshBuilder.CreateMesh();
    }





    // Cylinder generation based on https://gamasutra.com/blogs/JayelindaSuridge/20130905/199626/Modelling_by_numbers_Part_Two_A.php.

    private static void BuildRing(MeshBuilder meshBuilder, Vector3 position, Quaternion rotation, int segments, float radius, float v, bool buildTriangles) 
    {
        float angleInc = (Mathf.PI * 2.0f) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = angleInc * i;

            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.y = Mathf.Sin(-angle);

            unitPosition = rotation * unitPosition;

            meshBuilder.Vertices.Add(position + unitPosition * radius);
            meshBuilder.Normals.Add(unitPosition);
            meshBuilder.UVs.Add(new Vector2((float)i / segments, v));

            if (i > 0 && buildTriangles)
            {
                int baseIndex = meshBuilder.Vertices.Count - 1;

                int vertsPerRow = segments + 1;

                int index0 = baseIndex;
                int index1 = baseIndex - 1;
                int index2 = baseIndex - vertsPerRow;
                int index3 = baseIndex - vertsPerRow - 1;

                meshBuilder.AddTriangle(index0, index2, index1);
                meshBuilder.AddTriangle(index2, index3, index1);
            }
        }
    }

    private static void BuildCap(MeshBuilder meshBuilder, int segments, Vector3 position, float radius, bool reverseDirection)
    {
        Vector3 normal = reverseDirection ? Vector3.down : Vector3.up;

        //one vertex in the center:
        meshBuilder.Vertices.Add(position);
        meshBuilder.Normals.Add(normal);
        meshBuilder.UVs.Add(new Vector2(0.5f, 0.5f));

        int centreVertexIndex = meshBuilder.Vertices.Count - 1;

        //vertices around the edge:
        float angleInc = (Mathf.PI * 2.0f) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = angleInc * i;

            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.z = Mathf.Sin(angle);

            meshBuilder.Vertices.Add(position + unitPosition * radius);
            meshBuilder.Normals.Add(normal);

            Vector2 uv = new Vector2(unitPosition.x + 1.0f, unitPosition.z + 1.0f) * 0.5f;
            meshBuilder.UVs.Add(uv);

            //build a triangle:
            if (i > 0)
            {
                int baseIndex = meshBuilder.Vertices.Count - 1;

                if (reverseDirection)
                    meshBuilder.AddTriangle(centreVertexIndex, baseIndex - 1,
                        baseIndex);
                else
                    meshBuilder.AddTriangle(centreVertexIndex, baseIndex,
                        baseIndex - 1);
            }
        }
    }
}
