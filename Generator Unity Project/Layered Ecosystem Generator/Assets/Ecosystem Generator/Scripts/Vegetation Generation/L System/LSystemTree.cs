using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LSystemTree : MonoBehaviour
{
    public LSystemGenerationRuleAsset m_rulesAsset;

    [Header("Rendering")]
    [Tooltip("Specifies the Material that the trunk and branches use.")]
    public Material m_branchMaterial;

    [Tooltip("Specifies the Prefab that the L-System uses for the tree leaves.")]
    public GameObject m_leafPrefab;

    private string m_sentence;
    
    private Dictionary<char, string> m_rulesDictionary;

    private void Start()
    {
        SetInitialReferences();
        //Generate();
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
            string[] keyValue = m_rulesAsset.m_rules[i].Split('=');
            if (keyValue.Length < 2)
            {
                Debug.LogError($"{m_rulesAsset.m_rules[i]} is not a valid rule");
            }
            else
            {
                m_rulesDictionary.Add(keyValue[0].ToCharArray()[0], keyValue[1]);
            }
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
        List<LSystemBranch> branches = m_rulesAsset.Build(m_sentence);
        m_sentence = "";

        Build(branches);
    }

    private void Build(List<LSystemBranch> branches)
    {
        for (int i = 0; i < branches.Count; i++)
        {
            Mesh segmentMesh = LSystemBranchMeshGenerator.BuildBranch(branches[i]);
            GameObject obj = new GameObject();
            obj.AddComponent<MeshRenderer>().sharedMaterial = m_branchMaterial;
            obj.AddComponent<MeshFilter>().sharedMesh = segmentMesh;
            //obj.transform.position = branches[i].m_branchPositions[0].m_position;
            obj.transform.parent = transform;
        }
    }
    

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Generate();
        }
    }
}
