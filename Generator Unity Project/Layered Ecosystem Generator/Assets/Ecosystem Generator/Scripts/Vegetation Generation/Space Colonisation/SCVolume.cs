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

    public List<SCLeaf> GetLeavesList(Transform basePosition, float density)
    {
        List<SCLeaf> leavesList = new List<SCLeaf>();
        for (int i = 1; i < m_volumeShapes.Count; i++)
        {
            SCVolumeShape curShape = m_volumeShapes[i];
            if (curShape.m_boundingPoints.Count == 0) { continue; }
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
            float area = (maxPosition.x - minPosition.x) * (maxPosition.y - minPosition.y);

            for (int j = 0; j < 200; j++)
            {
                Vector3 position = new Vector3(Random.Range(minPosition.x, maxPosition.x), Random.Range(minPosition.y, maxPosition.y), 0.0f);
                if (IsInVolume(position, curShape.m_boundingPoints))
                {
                    SCLeaf leaf = new SCLeaf(basePosition.position + position);
                    leavesList.Add(leaf);
                }
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
}
