using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SCVolumeShape
{
    public List<Vector3> m_boundingPoints = new List<Vector3>();
}

public class SCVolume : MonoBehaviour
{
    public List<SCVolumeShape> m_volumeShapes = new List<SCVolumeShape>();
}
