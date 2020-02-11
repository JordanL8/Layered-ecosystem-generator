using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stem
{
    public int m_depth;
    public float m_length;
    public float m_radius;
    public float m_offset;
    public float m_maxChildLength;
    public float m_radiusLimit;


    public Stem m_parent;
    public List<Stem> m_children;
    public List<SegmentPosition> m_segmentPositions;

    public Stem(int depth, Stem parent = null, float offset = 0.0f, float radiusLimit = -1.0f)
    {
        m_depth = depth;
        m_parent = parent;
        m_offset = offset;
        m_radiusLimit = radiusLimit;
        m_children = new List<Stem>();
        m_segmentPositions = new List<SegmentPosition>();
    }

    
}

public class SegmentPosition
{
    public Vector3 m_position;
    public Vector3 m_rotation;
    public float m_radius;
}

public class VegetationGeneratorRecursive : MonoBehaviour
{
    public VegetationDescription m_vegDesc; // TODO: Make private.
    private Transform m_rootTransform;

    private float m_treeScale;
    private float m_trunkLength;
    private float m_baseLength = 0.0f;

    private int[] m_splitError = { 0, 0, 0, 0, 0, 0, 0 };

    private Vector3 m_growDirection = new Vector3(0, 1, 0);
    private Vector3 m_currentPosition = new Vector3(0, 0, 0);

    private void Start()
    {
        GenerateTree();
    }

    public void Initialise(VegetationDescription vegetationDescription)
    {
        m_vegDesc = vegetationDescription;
        m_rootTransform = transform;
        m_treeScale = m_vegDesc.m_0Scale + GetVariation(m_vegDesc.m_0ScaleV);
    }

    public void GenerateTree()
    {
        GenerateBranches();
    }

    private void GenerateBranches()
    {
        GenerateStem(new Stem(0));
    }

    private void GenerateStem(Stem stem, int start = 0, float splitAngle = 0, float brancNumFactor = 1, int cloneProbability = 1)
    {
        int curDepth = stem.m_depth;
        int nextDepth = Mathf.Min(stem.m_depth + 1, 3);

        if (start == 0)
        {
            stem.m_maxChildLength = m_vegDesc.m_length[nextDepth] + GetVariation(m_vegDesc.m_lengthV[nextDepth]);
            stem.m_length = GetStemLength(stem);
            stem.m_radius = GetStemRadius(stem);
            if (curDepth == 0)
            {
                m_baseLength = stem.m_length * m_vegDesc.m_baseSize[0];
            }
        }

        // TODO: Add pruning.
        //
        //
        //

        int curCurveRes = m_vegDesc.m_curveRes[curDepth];
        int curSegmentSplits = m_vegDesc.m_segSplits[curDepth];
        float curSegmentLength = stem.m_length / (float)curSegmentSplits;

        int baseSegmentIndex = Mathf.CeilToInt(m_vegDesc.m_baseSize[0] * m_vegDesc.m_curveRes[0]);

        float leafNum = 0;
        float branchNum = 0;

        float fLeavesOnSegment = 0;
        float fBranchesOnSegment = 0;
        if (curDepth == m_vegDesc.m_levels - 1 &&
            curDepth > 0 &&
            m_vegDesc.m_leafNumber > 0)
        {
            leafNum = GetLeafNumber(stem);
            leafNum *= 1 - start / curCurveRes;
            fLeavesOnSegment = leafNum / curCurveRes;
        }
        else
        {
            branchNum = GetBranchNumber(stem);
            branchNum *= 1 - start / curCurveRes;
            branchNum *= brancNumFactor;
            fBranchesOnSegment = branchNum / curCurveRes;
        }

        float maximumSegmentPoints = Mathf.Ceil(Mathf.Max(1.0f, 100f / curCurveRes));

        float leafErrorNum = 0.0f;
        float branchErrorNum = 0.0f;

        // Rotation
        float previousRotationAngle = 0.0f;
        if (m_vegDesc.m_rotate[nextDepth] >= 0)
        {
            previousRotationAngle = Random.value * 360.0f;
        }
        else
        {
            previousRotationAngle = 1.0f;
        }

        //TODO: Helix properties.

        float segmentPoints = (curDepth == 0 || m_vegDesc.m_taper[curDepth] > 1) ? maximumSegmentPoints : 2.0f;

        for (int i = start; i < curCurveRes + 1; i++)
        {
            if (m_vegDesc.m_curveV[curDepth] < 0)
            {
                // Helix branch
            }
            else
            {
                // Normal branch
                SegmentPosition newPosition = new SegmentPosition();
                if (i == start)
                {
                    newPosition.m_position = m_currentPosition;
                    newPosition.m_rotation = m_growDirection;
                }
                else
                {
                    Move(curSegmentLength);
                    newPosition.m_position = m_currentPosition;
                    newPosition.m_rotation = m_growDirection;
                }
                newPosition.m_radius = GetStemSegmentRadius(stem, i / curCurveRes);

                if(i > start)
                {
                    if(m_vegDesc.m_curveV[curDepth] >= 0)
                    {
                        int splitNum = 0;
                        if(m_vegDesc.m_baseSplits > 0 &&
                            curDepth == 0 &&
                            i == baseSegmentIndex)
                        {
                            splitNum = m_vegDesc.m_baseSplits;
                        }
                        else if (curSegmentSplits > 0 &&
                            i < curCurveRes &&
                            (curDepth > 0 || i > baseSegmentIndex))
                        {
                            if(Random.value < cloneProbability)
                            {
                                splitNum = (int)(curSegmentSplits + m_splitError[curDepth]);
                                m_splitError[curDepth] -= splitNum - curSegmentSplits;

                                cloneProbability /= splitNum + 1;
                                brancNumFactor /= splitNum + 1;
                                brancNumFactor = Mathf.Max(0.8f, brancNumFactor);

                                branchNum *= brancNumFactor;
                                fBranchesOnSegment = branchNum / curCurveRes;
                            }
                        }
                    }

                    if(Mathf.Abs(branchNum) > 0 && curDepth < m_vegDesc.m_levels - 1)
                    {
                        float branchesOnSegment;
                        if(branchNum < 0)
                        {
                            if(i == curCurveRes)
                            {
                                branchesOnSegment = branchNum;
                            }
                            else
                            {
                                branchesOnSegment = 0;
                            }
                        }
                        else
                        {
                            branchesOnSegment = (int)(fBranchesOnSegment + branchErrorNum);
                            branchErrorNum -= branchesOnSegment - fBranchesOnSegment;
                        }
                        if(Mathf.Abs(branchesOnSegment) > 0)
                        {
                        }
                    }
                }
            }
        }
    }

    private float GetStemLength(Stem stem)
    {
        if (stem.m_depth == 0)
        {
            return m_treeScale * (m_vegDesc.m_length[0] + GetVariation(m_vegDesc.m_lengthV[0]));
        }
        else if (stem.m_depth == 1)
        {
            return stem.m_parent.m_length * stem.m_parent.m_maxChildLength *
                GetShapeRatio(m_vegDesc.m_shape,
                (stem.m_parent.m_length - stem.m_offset) / (stem.m_parent.m_length - m_baseLength));
        }
        return stem.m_parent.m_maxChildLength * (stem.m_parent.m_length - 0.7f * stem.m_offset);
    }

    private float GetStemRadius(Stem stem)
    {
        if (stem.m_depth == 0)
        {
            return stem.m_length * m_vegDesc.m_ratio * m_treeScale;
        }
        float radius = stem.m_parent.m_radius * Mathf.Pow(stem.m_length / stem.m_parent.m_length, m_vegDesc.m_ratioPower);
        return radius;
    }

    private float GetStemSegmentRadius(Stem stem, float lerp)
    {
        float radius = 0;
        int taperType = m_vegDesc.m_taper[stem.m_depth];
        int taperUnit;
        if (taperType < 1)
        {
            taperUnit = taperType;
        }
        else if (taperType < 2)
        {
            taperUnit = 2 - taperType;
        }
        else
        {
            taperUnit = 0;
        }
        float taper = stem.m_radius * (1 - taperUnit * lerp);

        if(taperType < 1)
        {
            radius = taper;
        }
        else
        {
            float depth;
            float lerp2 = (1 - lerp) * stem.m_length;
            float lerp3;
            if(taperType < 2 || lerp2 < taper)
            {
                depth = 1;
            }
            else
            {
                depth = taperType - 2;
            }
            if(taperType < 2)
            {
                lerp3 = lerp2;
            }
            else
            {
                lerp3 = Mathf.Abs(lerp2 - 2 * taper * (int)(lerp2 / (2 * taper) + 0.5f));
            }
        }
        if(stem.m_depth == 0)
        {
            float y = Mathf.Max(0, 1 - 8 * lerp);
            float flare = m_vegDesc.m_flare * ((Mathf.Pow(100, y) - 1) / 100) + 1;
            radius *= flare;
        }
        return radius;
    }

    private float GetShapeRatio(int shape, float ratio)
    {
        float shapeRatio = 0.0f;
        switch(shape)
        {
            case 0:     // Conical
            {
                shapeRatio = 0.2f + 0.8f * ratio;
            } break;
            case 1:     // Spherical
            {
                shapeRatio = 0.2f + 0.8f * Mathf.Sin(Mathf.PI * ratio);
            } break;
            case 2:     // Hemi-spherical
            {
                shapeRatio = 0.2f + 0.8f * Mathf.Sin(0.5f * Mathf.PI * ratio);
            } break;
            case 3:     // Cylindrical
            {
                shapeRatio = 1.0f;
            } break;
            case 4:     // Tapered Cylindrical
            {
                shapeRatio = 0.5f + 0.5f * ratio;
            } break;
            case 5:     // Flame
            {
                shapeRatio = ratio <= 0.7f ?
                    ratio / 0.7f :
                    (1.0f - ratio) / 0.3f;
            } break;
            case 6:     // Inverse Conical
            {
                shapeRatio = 1.0f - 0.8f * ratio;
            } break;
            case 7:     // Tend Flame
            {
                shapeRatio = ratio <= 0.7f ?
                    0.5f + 0.5f * ratio / 0.7f :
                    0.5f + 0.5f * (1.0f - ratio) / 0.3f;
            } break;
            case 8:     // Envelope
            {
                // TODO: Add prune envelope.
            } break;
        }
        return shapeRatio;
    }

    private float GetLeafNumber(Stem stem)
    {
        if(m_vegDesc.m_leafNumber >= 0)
        {
            float leafNum = m_vegDesc.m_leafNumber * m_treeScale / m_vegDesc.m_0Scale;
            return leafNum * (stem.m_length / (stem.m_parent.m_maxChildLength / stem.m_parent.m_length));
        }
        return m_vegDesc.m_leafNumber;
    }

    private float GetBranchNumber(Stem stem)
    {
        int nextDepth = Mathf.Min(stem.m_depth + 1, 3);
        float branchNumber;
        if(stem.m_depth == 1)
        {
            branchNumber = m_vegDesc.m_branches[nextDepth] * (Random.value * 0.2f + 0.9f);
        }
        else
        {
            if(m_vegDesc.m_branches[nextDepth] < 0)
            {
                branchNumber = m_vegDesc.m_branches[nextDepth];
            }
            else if(stem.m_depth == 1)
            {
                branchNumber = m_vegDesc.m_branches[nextDepth] * (0.2f + 0.8f * (stem.m_length / stem.m_parent.m_length / stem.m_parent.m_maxChildLength));
            }
            else
            {
                branchNumber = m_vegDesc.m_branches[nextDepth] * (1.0f - 0.5f * stem.m_offset / stem.m_parent.m_length);
            }
        }
        return branchNumber / (1 - m_vegDesc.m_baseSize[stem.m_depth]);
    }

    private float GetVariation(float variation)
    {
        return Random.Range(-variation, variation);
    }

    private void AddBranchesToStem(Stem stem, int segmentIndex, int branchesOnSegment, float rotationAngle, bool leaves = false)
    {
        SegmentPosition start = stem.m_segmentPositions[stem.m_segmentPositions.Count - 2];
        SegmentPosition end = stem.m_segmentPositions[stem.m_segmentPositions.Count - 1];

    }

    private void Move(float distance)
    {
        m_currentPosition += m_growDirection * distance;
    }
}
