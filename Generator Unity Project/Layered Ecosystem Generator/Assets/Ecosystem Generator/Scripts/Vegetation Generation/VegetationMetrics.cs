using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VegetationMetrics 
{
    public static float GetVariation(float variation)
    {
        return Random.Range(-variation, variation);
    }

    public static float GetTrunkBaseRadius(float trunkLength, float ratio, float trunkScale, float trunkScaleV)  // Trunk
    {
        return trunkLength * ratio * (trunkScale + GetVariation(trunkScaleV));
    }

    public static float GetStemBaseRadius(float parentRadius, float parentLength, float childLength, float ratioPower)
    {
        return parentLength * Mathf.Pow((childLength / parentLength), ratioPower);
    }

    public static float GetStemTipRadius(float baseRadius, float taper)
    {
        return baseRadius * (1 - taper);
    }

    public static float GetStemLength(float trunkScale, float trunkScaleV, float trunkLength, float trunkLengthV)
    {
        return (trunkScale + GetVariation(trunkScaleV)) * (trunkLength + GetVariation(trunkLengthV));
    }
    
}