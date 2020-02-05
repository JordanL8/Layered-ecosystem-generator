using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StemCapOption
{
    NO_CAPS,
    TOP_CAP,
    BOTTOM_CAP,
    BOTH_CAPS
}


public static class StemFactory // Cylinder creation based on https://github.com/doukasd/Unity-Components/blob/master/ProceduralCylinder/Assets/Scripts/Procedural/ProceduralCylinder.cs created by Dimitris Doukas.
{
    public static Mesh CreateStemMesh(StemSegment stem, int radialSegments, StemCapOption stemCapOption)  // Infers there are only two height segments. One for the top and one for the bottom.
    {
        Mesh stemMesh = new Mesh();
        
            
        int vertColumnCount = radialSegments + 1;
        int vertRowCount = 2;

        int vertexCount = vertColumnCount * vertRowCount;
        int uVCount = vertexCount;
        int sideTriCount = radialSegments * 2;
        int capTriCount = stemCapOption == StemCapOption.NO_CAPS ? 0 : radialSegments - 2;
        int capMultiplier = stemCapOption == StemCapOption.BOTH_CAPS ? 2 : 1;
        int trisArrayLength = (sideTriCount + capTriCount * capMultiplier) * 3;

        Vector3[] stemVertices = new Vector3[vertexCount];
        Vector2[] stemUVs = new Vector2[uVCount];
        int[] stemTris = new int[trisArrayLength];

        float angleStep = 2 * Mathf.PI / radialSegments;
        float uvStepH = 1.0f / radialSegments;
        float uvStepV = 1.0f;   // TODO calculate this from radius and height.

        float height = Vector3.Distance(stem.m_startPos, stem.m_endPos); 
        float currentRadius = stem.m_startRadius;
        Vector3 currentCentre = Vector3.zero;
        Quaternion stemAngle = Quaternion.FromToRotation(stem.m_startPos, stem.m_endPos);

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < vertColumnCount; i++)
            {
                float angle = i * angleStep;
                angle = i == vertColumnCount - 1 ? 0 : angle;

                stemVertices[j * vertColumnCount + i] = currentCentre + (stemAngle * new Vector3(currentRadius * Mathf.Cos(angle), 0, currentRadius * Mathf.Sin(angle)));
                stemUVs[j * vertColumnCount + i] = new Vector2(i * uvStepH, j * uvStepV);

                if (j == 0 || i >= vertColumnCount - 1)
                {
                    continue;
                }
                else
                {
                    int baseIndex = capTriCount * 3 + (j - 1) * radialSegments * 6 + i * 6;
                    stemTris[baseIndex + 0] = j * vertColumnCount + i;
                    stemTris[baseIndex + 1] = j * vertColumnCount + i + 1;
                    stemTris[baseIndex + 2] = (j - 1) * vertColumnCount + i;
                   
                    stemTris[baseIndex + 3] = (j - 1) * vertColumnCount + i;
                    stemTris[baseIndex + 4] = j * vertColumnCount + i + 1;
                    stemTris[baseIndex + 5] = (j - 1) * vertColumnCount + i + 1;
                    
                }
            }
            currentRadius = stem.m_endRadius;
            currentCentre = stem.m_endPos + (-stem.m_startPos);
        }

        bool leftSided = true;
        int leftIndex = 0;
        int rightIndex = 0;
        int middleIndex = 0;
        int topCapVertexOffset = vertexCount - vertColumnCount;
        for (int i = 0; i < capTriCount; i++)
        {
            int bottomCapBaseIndex = i * 3;
            int topCapBaseIndex = (capTriCount + sideTriCount) * 3 + i * 3;

            if (i == 0)
            {
                middleIndex = 0;
                leftIndex = 1;
                rightIndex = vertColumnCount - 2;
                leftSided = true;
            }
            else if (leftSided)
            {
                middleIndex = rightIndex;
                rightIndex--;
            }
            else
            {
                middleIndex = leftIndex;
                leftIndex++;
            }
            leftSided = !leftSided;

            if (stemCapOption == StemCapOption.BOTH_CAPS)
            {
                stemTris[bottomCapBaseIndex + 0] = rightIndex;
                stemTris[bottomCapBaseIndex + 1] = middleIndex;
                stemTris[bottomCapBaseIndex + 2] = leftIndex;
                stemTris[topCapBaseIndex + 0] = topCapVertexOffset + leftIndex;
                stemTris[topCapBaseIndex + 1] = topCapVertexOffset + middleIndex;
                stemTris[topCapBaseIndex + 2] = topCapVertexOffset + rightIndex;
            }
            else if(stemCapOption == StemCapOption.BOTTOM_CAP)
            {
                stemTris[bottomCapBaseIndex + 0] = rightIndex;
                stemTris[bottomCapBaseIndex + 1] = middleIndex;
                stemTris[bottomCapBaseIndex + 2] = leftIndex;
            }
            else if (stemCapOption == StemCapOption.TOP_CAP)
            {
                stemTris[bottomCapBaseIndex + 0] = topCapVertexOffset + leftIndex;
                stemTris[bottomCapBaseIndex + 1] = topCapVertexOffset + middleIndex;
                stemTris[bottomCapBaseIndex + 2] = topCapVertexOffset + rightIndex;
            }
        }
        stemMesh.vertices = stemVertices;
        stemMesh.uv = stemUVs;
        stemMesh.triangles = stemTris;

        stemMesh.RecalculateNormals();
        stemMesh.RecalculateBounds();

        return stemMesh;
    }
}
