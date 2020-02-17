using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// 1. Properties prefixed with a number denote similar properties at different levels of recursion.
// 2. Properties suffixed with a V denote a vatiation property. They describe the magnitude of variation. A negative value acts as a flag.
// 3. All angular properties are expressed in degrees.


[CreateAssetMenu(fileName = "Vegetation Description", menuName = "Ecosystem Generator/Vegetation Description", order = 2)]
public class VegetationDescription : ScriptableObject
{
    [Header("Placement Model")]
    public float m_innerRadius;
    public float m_outerRadius;

    [Header("General Properties")]
    public int m_shape;
    public int m_0Scale;
    public int m_0ScaleV;
    public int m_levels;
    public float m_ratio;
    public float m_ratioPower;
    public float m_flare;
    public int m_baseSplits;

    [Header("Recursive Properties")]
    public float[]  m_baseSize;
    public int[]    m_downAngle;
    public int[]    m_downAngleV;
    public int[]    m_rotate;
    public int[]    m_rotateV;

    public int[]    m_branches;
    public float[]  m_length;
    public float[]  m_lengthV;
    public int[]    m_taper;

    public int[]    m_segSplits;
    public int[]    m_splitAngle;
    public int[]    m_splitAngleV;
    public int[]    m_curveRes;
    public int[]    m_curve;
    public int[]    m_curveV;
    public int[]    m_curveBack;

    [Header("Leaf Properties")]
    public int      m_leafNumber;
    public int      m_leafShape;
    public float    m_leafScale;
    public float    m_leafScaleX;
    public float    m_attractionUp;

    [Header("Prune Properties")]
    public int      m_pruneRatio;
    public float    m_pruneWidth;
    public float    m_pruneWidthPeak;
    public float    prunePowerLow;
    public float    prunePowerHigh;
}
