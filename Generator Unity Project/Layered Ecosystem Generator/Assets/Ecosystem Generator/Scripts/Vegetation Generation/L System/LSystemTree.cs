using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LSystemTree : MonoBehaviour
{
    public LSystemGenerationRuleAsset m_rulesAsset;

    [Header("Rendering")]
    [Tooltip("Specifies the Material the trunk and branches use.")]
    public Material m_branchMaterial;

    [Tooltip("Specifies the Prefab the L-System uses for the tree leaves.")]
    public GameObject m_leafPrefab;

    [Tooltip("Specifies the Material the L-System uses for the tree leaves. If your Leaf Prefab already has a Material, assign that Material to this property.")]
    public Material m_leafMaterial;

    private string m_sentence;
    
    private Dictionary<char, string> m_rulesDictionary;

    private void Start()
    {
        SetInitialReferences();
        Generate(0);
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

    public void Generate(int lodLevel)
    {
        if(m_sentence == null)
        {
            SetInitialReferences();
        }


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

        Build(branches, lodLevel);
    }

    private void Build(List<LSystemBranch> branches, int lodLevel)
    {
        for (int l = 0; l <= lodLevel; l++)
        {
            Transform m_lodTransform = new GameObject($"LOD {l}").transform;
            m_lodTransform.parent = transform;
            m_lodTransform.localPosition = Vector3.zero;

            Transform m_branchTransform = new GameObject("Branches").transform;
            m_branchTransform.parent = m_lodTransform;
            m_branchTransform.localPosition = Vector3.zero;
            Transform m_leavesTransform = new GameObject("Leaves").transform;
            m_leavesTransform.parent = m_lodTransform;
            m_leavesTransform.localPosition = Vector3.zero;

            for (int i = 0; i < branches.Count; i++)
            {
                Mesh segmentMesh = LSystemBranchMeshGenerator.BuildBranch(branches[i], l, 0.0075f);
                GameObject obj = new GameObject();
                obj.AddComponent<MeshRenderer>().sharedMaterial = m_branchMaterial;
                obj.AddComponent<MeshFilter>().sharedMesh = segmentMesh;
                obj.transform.parent = m_branchTransform;

                for (int j = 0; j < branches[i].m_leafTransforms.Count; j++)
                {
                    LSystemLeafTransform curLeafTransform = branches[i].m_leafTransforms[j];
                    GameObject newLeaf = Instantiate(m_leafPrefab, curLeafTransform.m_position, Quaternion.identity);
                    newLeaf.transform.up = curLeafTransform.m_transformForward;
                    newLeaf.transform.position += (newLeaf.transform.up * (newLeaf.transform.localScale.x / 3.0f));
                    newLeaf.transform.parent = m_leavesTransform;
                    //newLeaf.transform.Rotate(newLeaf.transform.up, Random.value * 360.0f);
                }
            }


            // Combine Meshes
            CombineMeshes(m_branchTransform, m_branchMaterial);
            CombineMeshes(m_leavesTransform, m_leafMaterial);
        }
    }

    private void CombineMeshes(Transform parent, Material material)
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.GetChild(i).gameObject);
        }
        MeshFilter myMeshFilter = parent.gameObject.AddComponent<MeshFilter>();
        myMeshFilter.sharedMesh = new Mesh();
        myMeshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        myMeshFilter.sharedMesh.CombineMeshes(combine, true);
        parent.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
    }
}
