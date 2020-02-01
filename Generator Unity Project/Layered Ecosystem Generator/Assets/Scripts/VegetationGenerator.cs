using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationGenerator : MonoBehaviour
{
    public VegetationDescription m_vegDesc;

    private Transform m_curStemParent;

    private Queue<Stem> vegetationStems = new Queue<Stem>();

    public void Generate(VegetationDescription description)
    {
        m_vegDesc = description;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Grow();
        }
    }

    public void Grow()
    {
        vegetationStems.Enqueue(new Stem(transform.position, Vector3.zero, -1));

        while(vegetationStems.Count > 0)
        {
            Stem curStem = vegetationStems.Dequeue();
            TraverseStem(curStem);
        }
        
    }

    private void TraverseStem(Stem stem)
    {
        int nCurveRes = 0;
        int nCurve = 0;
        int nCurveV = 0;
        int nCurveBack = 0;
        float stemLength = 0;
        float stemBaseRadius = 0;
        float stemTopRadius = 0;

        float baseLength = 0; 
        float segmentLength = 0;

        if (stem.m_recursionLevel >= 0)    // -1 indicates trunk
        {
            nCurveRes = m_vegDesc.m_recLevelStem[stem.m_recursionLevel].m_nCurveRes;
            nCurve = m_vegDesc.m_recLevelStem[stem.m_recursionLevel].m_nCurve;
            nCurveBack = m_vegDesc.m_recLevelStem[stem.m_recursionLevel].m_nCurveBack;



            
        }
        else
        {
            nCurveRes = m_vegDesc.m_0CurveRes;
            nCurve = m_vegDesc.m_0Curve;
            nCurveV = m_vegDesc.m_0CurveV;
            nCurveBack = m_vegDesc.m_0CurveBack;
            stemLength = VegetationMetrics.GetStemLength(m_vegDesc.m_scale, m_vegDesc.m_scaleV, m_vegDesc.m_0Length, m_vegDesc.m_0LengthV);
            stemBaseRadius = VegetationMetrics.GetTrunkBaseRadius(stemLength, m_vegDesc.m_ratio, m_vegDesc.m_0Scale, m_vegDesc.m_0ScaleV);

            baseLength = stemLength * m_vegDesc.m_baseSize;
            segmentLength = (stemLength - baseLength) / (float)nCurveRes;
        }
        stemTopRadius = VegetationMetrics.GetStemTipRadius(stemBaseRadius, m_vegDesc.m_0Taper);

        Vector3 growDirection = Quaternion.Euler(stem.m_startEular) * Vector3.up;
        Vector3 curPos = stem.m_startPoint;
        float curRadius = stemBaseRadius;
        float angle = nCurveBack == 0 ? (float)nCurve / (float)nCurveRes : (float)nCurve / ((float)nCurveRes / 2.0f);

        int halfwayIndex = nCurveRes / 2;
        bool shouldCurveBack = nCurveBack != 0;
        float curveBackAngle = (float)nCurveBack / ((float)nCurveRes / 2.0f);

        for (int i = 0; i < nCurveRes; i++)
        {
            StemSegment segment = new StemSegment();

            segment.m_startPos = curPos;
            if (stem.m_recursionLevel < 0 && i == 0)
            {
                curPos += (growDirection * baseLength);
            }
            else
            {
                curPos += (growDirection * segmentLength);
            }
            segment.m_endPos = curPos;

            float curAngle = (shouldCurveBack && i >= halfwayIndex) ? curveBackAngle : angle;
            growDirection = (Quaternion.Euler(0, 0, curAngle + (VegetationMetrics.GetVariation(nCurveV) / nCurveRes)) * growDirection).normalized;

            segment.m_startRadius = curRadius;
            curRadius = Mathf.Lerp(stemBaseRadius, stemTopRadius, ((float)i + 1.0f) / ((float)nCurveRes));
            segment.m_endRadius = curRadius;

            stem.AddStemSegment(segment);

            if(i == 0 && stem.m_recursionLevel < 0)
            {
            }
        }


        CreateStemMesh(stem);
    }

    

    private void CreateStemMesh(Stem stem)
    {
        List<StemSegment> stemSegments = stem.GetStemSegments();
        for (int i = 0; i < stemSegments.Count; ++i)
        {
            GameObject stemGO = new GameObject("Stem");
            stemGO.transform.position = stemSegments[i].m_startPos;
            stemGO.AddComponent<MeshRenderer>();
            stemGO.AddComponent<MeshFilter>().sharedMesh = StemFactory.CreateStemMesh(stemSegments[i], 12, StemCapOption.BOTH_CAPS);
        }
    }






    
}
