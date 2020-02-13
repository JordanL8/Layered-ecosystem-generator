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
}

