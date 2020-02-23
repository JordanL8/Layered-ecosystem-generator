using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder
{
    private List<Vector3> m_vertices = new List<Vector3>();
    public List<Vector3> Vertices { get { return m_vertices; } }

    private List<Vector3> m_normals = new List<Vector3>();
    public List<Vector3> Normals { get { return m_normals; } }

    private List<Vector2> m_UVs = new List<Vector2>();
    public List<Vector2> UVs { get { return m_UVs; } }

    private List<int> m_indices = new List<int>();

    public void AddTriangle(int index0, int index1, int index2)
    {
        m_indices.Add(index0);
        m_indices.Add(index1);
        m_indices.Add(index2);
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = m_vertices.ToArray();
        mesh.triangles = m_indices.ToArray();

        if (m_normals.Count == m_vertices.Count)
        {
            mesh.normals = m_normals.ToArray();
        }
        else
        {
            mesh.RecalculateNormals();
        }

        if (m_UVs.Count == m_vertices.Count)
        {
            mesh.uv = m_UVs.ToArray();
        }

        mesh.RecalculateBounds();

        return mesh;
    }

    // Cylinder generation based on https://gamasutra.com/blogs/JayelindaSuridge/20130905/199626/Modelling_by_numbers_Part_Two_A.php.

    public void BuildRing(Vector3 position, Quaternion rotation, int segments, float radius, float v, bool buildTriangles)
    {
        float angleInc = (Mathf.PI * 2.0f) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = angleInc * i;

            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.y = Mathf.Sin(-angle);

            unitPosition = rotation * unitPosition;

            Vertices.Add(position + unitPosition * radius);
            Normals.Add(unitPosition);
            UVs.Add(new Vector2((float)i / segments, v));

            if (i > 0 && buildTriangles)
            {
                int baseIndex = Vertices.Count - 1;

                int vertsPerRow = segments + 1;

                int index0 = baseIndex;
                int index1 = baseIndex - 1;
                int index2 = baseIndex - vertsPerRow;
                int index3 = baseIndex - vertsPerRow - 1;

                AddTriangle(index0, index2, index1);
                AddTriangle(index2, index3, index1);
            }
        }
    }

    public void BuildCap(Vector3 position, Quaternion rotation, int segments, float radius)
    {
        Vector3 normal = rotation.eulerAngles.normalized;

        //one vertex in the center:
        Vertices.Add(position);
        Normals.Add(normal);
        UVs.Add(new Vector2(0.5f, 0.5f));

        int centreVertexIndex = Vertices.Count - 1;

        //vertices around the edge:
        float angleInc = (Mathf.PI * 2.0f) / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = angleInc * i;

            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.y = Mathf.Sin(angle);

            unitPosition = rotation * unitPosition;

            Vertices.Add(position + unitPosition * radius);
            Normals.Add(normal);

            Vector2 uv = new Vector2(unitPosition.x + 1.0f, unitPosition.z + 1.0f) * 0.5f;
            UVs.Add(uv);

            //build a triangle:
            if (i > 0)
            {
                int baseIndex = Vertices.Count - 1;

                AddTriangle(centreVertexIndex, baseIndex - 1,
                    baseIndex);
            }
        }
    }
}

