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

    [Header("General")]

    public int m_shape;                         // General tree shape ID.
    public float m_baseSize;                    // Fractional branchless area at tree base. The branchless area is length * this value.
    public int m_scale, m_scaleV,               // Size and scaling of the tree.   
        m_zScake, m_zScaleV;
    public int m_levels;                        // Levels of recursion.
    public float m_ratio, m_ratioPower;         // Radius/length ratio, reduction.
    public float m_lobes, m_lobeDepth;          // Sinusoidal cross-section variation.
    public float m_flare;                       // Exponential expansion at base of tree.

    [Header("Trunk")]

    public float m_0Scale;                      // Extra trunk scaling.
    public float m_0ScaleV;                     // Trunk scaling variation.
    public float m_0Length, m_0LengthV,         // Fractional trunk, cross section scaling.
        m_0Taper;
    public int m_0BaseSplits;                   // Stem splits at base of trunk.
    public float m_0SegSplits, m_0SplitAngle,   // Stems splits and angle per segment.
        m_0SplitAngleV;
    public int m_0CurveRes, m_0Curve,           // Curvature resolution and angles.
        m_0CurveBack, m_0CurveV;

    [Header("1st Recursion")]

    public int m_1DownAngle;                    // Main branch: angle from trunk.
    public int m_1DownAngleV;                   // Angle variation.
    public int m_1Rotate, m_1RotateV,           // Spiraling angles, number of branches.
        m_1Branches;
    public float m_1Length, m_1LengthV,         // Relative length, cross-section scaling.
        m_1Taper;
    public float m_1SegSplits, m_1SplitAngle,   //Stem splits per segment.
        m_1SplitAngleV;
    public int m_1CurveRes, m_1Curve,           // Curvature resolution and angles.
        m_1CurveBack, m_1CurveV;

    [Header("2nd Recursion")]

    public int m_2DownAngle;                    // Main branch: angle from parent.
    public int m_2DownAngleV;                   // Angle variation.
    public int m_2Rotate, m_2RotateV,           // Spiraling angles, number of branches.
        m_2Branches;
    public float m_2Length, m_2LengthV,         // Relative length, cross-section scaling.
        m_2Taper;
    public float m_2SegSplits, m_2SplitAngle,   //Stem splits per segment.
        m_2SplitAngleV;
    public int m_2CurveRes, m_2Curve,           // Curvature resolution and angles.
        m_2CurveBack, m_2CurveV;

    [Header("3rd Recursion")]

    public int m_3DownAngle;                    // Main branch: angle from parent.
    public int m_3DownAngleV;                   // Angle variation.
    public int m_3Rotate, m_3RotateV,           // Spiraling angles, number of branches.
        m_3Branches;
    public float m_3Length, m_3LengthV,         // Relative length, cross-section scaling.
        m_3Taper;
    public float m_3SegSplits, m_3SplitAngle,   //Stem splits per segment.
        m_3SplitAngleV;
    public int m_3CurveRes, m_3Curve,           // Curvature resolution and angles.
        m_3CurveBack, m_3CurveV;

    [Header("Leaves")]

    public int m_leaves;                        // Number of leaves per parent.
    public int m_leafShape;                     // Shape id.
    public float m_leafScale, m_leafScaleX;     // Leaf length, relative x scale.
    public float m_attractionUp;                // Upward growth tendency.
    public int m_pruneRation;                   // Fractional effect of pruning.
    public float m_pruneWidth,                  // Width, position of envelope peak.
        m_pruneWidthPeak;
    public float m_prunePowerLow,               // Curvature of envelope.
        m_prunePowerHigh;
}
