using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VegetationGenerator : MonoBehaviour
{
    public VegetationDescription m_vegDesc;

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
    }
}
