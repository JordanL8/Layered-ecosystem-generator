using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


public class LSystem : MonoBehaviour
{
    public LSystemGenerationRuleAsset m_rulesAsset;

    private string m_sentence;
    
    private Dictionary<char, string> m_rulesDictionary;

    private void Start()
    {
        SetInitialReferences();
        //Generate();
        // Get branches from turtle then build
    }

    private void SetInitialReferences()
    {
        m_sentence = m_rulesAsset.m_axiom.ToString();
        PopulateDictionary();
    }

    private void PopulateDictionary()
    {
        m_rulesDictionary = new Dictionary<char, string>();
        for (int i = 0; i < m_rulesAsset.m_rules.Length; i++)
        {
            m_rulesDictionary.Add(m_rulesAsset.m_rules[i].m_key, m_rulesAsset.m_rules[i].m_value);
        }
    }

    private void Generate()
    {
        for (int i = 0; i < m_rulesAsset.m_iterations; i++)
        {
            StringBuilder stringBuilder = new StringBuilder();
            char[] charSentence = m_sentence.ToCharArray();
            for (int j = 0; j < charSentence.Length; j++)
            {
                char rule = charSentence[j];
                if (m_rulesDictionary.ContainsKey(rule))
                {
                    stringBuilder.Append(m_rulesDictionary[rule]);
                }
                else
                {
                    stringBuilder.Append(rule);
                }
            }
            m_sentence = stringBuilder.ToString();
        }
        m_rulesAsset.Build(m_sentence);
        m_sentence = "";
    }
    

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Generate();
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (m_rulesAsset != null &&
                m_rulesAsset.d_lines != null)
            {
                for (int i = 0; i < m_rulesAsset.d_lines.Count; i += 2)
                {
                    Debug.DrawLine(m_rulesAsset.d_lines[i], m_rulesAsset.d_lines[i + 1]);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        m_rulesAsset.d_lines.Clear();
    }
}
