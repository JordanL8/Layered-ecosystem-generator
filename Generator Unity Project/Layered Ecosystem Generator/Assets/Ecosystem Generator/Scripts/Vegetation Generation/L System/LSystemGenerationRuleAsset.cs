using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LSystemRule
{
    public char m_key;
    public string m_value;
}

public abstract class LSystemGenerationRuleAsset : ScriptableObject
{
    public LSystemRule[] m_rules;
    public int m_iterations;
    public string m_axiom;

    public List<Vector3> d_lines = new List<Vector3>();

    public abstract void Build(string commandString);
}
