using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationGenerator : MonoBehaviour
{













    private float GetStemRadius(float trunkLength, float ratio, float scale)
    {
        return trunkLength * ratio * scale;
    }

    private float GetStemRadius(float parentRadius, float parentLength, float childLength, float ratioPower)
    {
        return parentLength * Mathf.Pow((childLength / parentLength), ratioPower);
    }
}
