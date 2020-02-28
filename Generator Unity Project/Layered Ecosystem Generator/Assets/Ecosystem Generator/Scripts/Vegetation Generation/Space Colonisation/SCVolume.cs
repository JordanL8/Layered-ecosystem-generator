using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AnimatedValues;

[System.Serializable]
public class SCVolumeShape
{
    public List<Vector3> m_boundingPoints = new List<Vector3>();
}

[CreateAssetMenu(fileName = "SCVolume", menuName = "Ecosystem Generator/Space Colonisation Volume", order = 2)]
public class SCVolume : ScriptableObject
{
    [HideInInspector] public List<SCVolumeShape> m_volumeShapes = new List<SCVolumeShape>();  // Index 0 is the trunk.
    [HideInInspector] public AnimBool m_showVolumeShapeList = new AnimBool(false);
    [HideInInspector] public bool m_isEditable = false;

    public List<SCLeaf> GetLeavesList(Transform basePosition, float density, float bottomCanopyWidth, float middleCanopyWidth, float topCanopyWidth)
    {
        List<SCLeaf> leavesList = new List<SCLeaf>();
        for (int i = 1; i < m_volumeShapes.Count; i++)
        {
            SCVolumeShape curShape = m_volumeShapes[i];
            if (curShape.m_boundingPoints.Count == 0) { continue; }
            // Create Bounding Box.
            Vector3 minPosition = curShape.m_boundingPoints[0];
            Vector3 maxPosition = curShape.m_boundingPoints[0];
            for (int j = 0; j < curShape.m_boundingPoints.Count; j++)
            {
                Vector3 position = curShape.m_boundingPoints[j];// - basePosition.position;
                if (position.x > maxPosition.x) { maxPosition.x = position.x; }
                else if (position.x < minPosition.x) { minPosition.x = position.x; }

                if (position.y > maxPosition.y) { maxPosition.y = position.y; }
                else if (position.y < minPosition.y) { minPosition.y = position.y; }
            }


            // Populate shape with leaves
            float area = (maxPosition.x - minPosition.x) * (maxPosition.y - minPosition.y);
            int leafNumber = Mathf.RoundToInt(density * area);
            for (int j = 0; j < leafNumber; j++)
            {
                Vector3 position = new Vector3(Random.Range(minPosition.x, maxPosition.x), Random.Range(minPosition.y, maxPosition.y), 0.0f);
                if (IsInVolume(position, curShape.m_boundingPoints))
                {
                    // TODO, set the width smarter.
                    //float width = maxPosition.x - minPosition.x;
                    float height = maxPosition.y - minPosition.y;
                    float normalisedHeight = (position.y - minPosition.y) / height;
                    float width = Lerp3(bottomCanopyWidth, middleCanopyWidth, topCanopyWidth, normalisedHeight);
                    //float normalisedDiff = (0 - 1) / (maxPosition.x - centreX) * (compareValue - centreX) + centreX;
                    position.z += Random.Range(-width / 2, width / 2); //* Mathf.Pow(normalisedDiff, 2);
                    
                    SCLeaf leaf = new SCLeaf(basePosition.position + position);
                    leavesList.Add(leaf);
                }
            }


            // Remove those too close to one another.
            float squareClosestDistance = 0.10f * 0.10f;
            List<int> removalIndices = new List<int>();
            for (int j = 0; j < leavesList.Count - 1; j++)
            {
                for (int k = j + 1; k < leavesList.Count; k++)
                {
                    if (Vector3.SqrMagnitude(leavesList[k].Position - leavesList[j].Position) < squareClosestDistance)
                    {
                        removalIndices.Add(j);
                        break;
                    }
                }
            }

            for (int j = removalIndices.Count - 1; j > 0; j--)
            {
                leavesList.RemoveAt(removalIndices[j]);
            }
        }
        return leavesList;
    }

    public bool IsInVolume(Vector3 point, List<Vector3> polyPoints)
    {
        var j = polyPoints.Count - 1;
        var inside = false;
        for (int i = 0; i < polyPoints.Count; j = i++)
        {
            var pi = polyPoints[i];
            var pj = polyPoints[j];
            if (((pi.y <= point.y && point.y < pj.y) || (pj.y <= point.y && point.y < pi.y)) &&
                (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x))
                inside = !inside;
        }
        return inside;
    }

    private float Lerp3(float a, float b, float c, float t)
    {
      
        if (t <= 0.5f)
        {
            float normalisedCorrection = t / 0.5f;
            return Mathf.Lerp(a, b, -Mathf.Pow(normalisedCorrection - 1, 2.0f) + 1); 
        }
        else
        {
            float normalisedCorrection = (t - 0.5f) / (1.0f - 0.5f);
            return Mathf.Lerp(b, c, Mathf.Pow(normalisedCorrection , 2.0f));
        }
    }
}
