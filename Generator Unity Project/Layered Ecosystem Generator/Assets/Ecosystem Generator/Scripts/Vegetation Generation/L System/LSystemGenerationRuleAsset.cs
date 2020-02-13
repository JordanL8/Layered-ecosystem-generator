using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class LSystemGenerationRuleAsset : ScriptableObject
{
    [Header("Rules")]
    public string m_axiom;
    public string[] m_rules;
    public int m_iterations;

    protected LSystemTurtle m_turtle;

    public abstract List<LSystemBranch> Build(string commandString);
}
