using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine;

public class TurtlePosition
{
    public Vector3 m_storedPosition;
    public Vector3 m_storedForward;
    public Vector3 m_storedRight;
}

public class Turtle
{
    private Vector3 m_position;
    public Vector3 Position
    {
        get { return m_position; }
    }

    private Vector3 m_forward;
    private Vector3 m_right;

    private Stack<TurtlePosition> m_positionStack;

    public Turtle(Vector3 position, Vector3 direction, Vector3 right)
    {
        m_position = position;
        m_forward = direction;
        m_right = right;
        m_positionStack = new Stack<TurtlePosition>();
    }

    public Vector3 Move(float distance)
    {
        m_position += m_forward * distance;
        return m_position;
    }

    public void PitchUp(float angle)
    {
        m_forward = (Quaternion.AngleAxis(angle, m_right) * m_forward).normalized;
    }

    public void PitchDown(float angle)
    {
        m_forward = (Quaternion.AngleAxis(-angle, m_right) * m_forward).normalized;
    }

    public void YawLeft(float angle)
    {
        Vector3 axis = Vector3.Cross(m_forward, m_right).normalized;
        Quaternion rotation = Quaternion.AngleAxis(-angle, axis);
        m_forward = (rotation * m_forward).normalized;
        m_right = (rotation * m_right).normalized;
    }

    public void YawRight(float angle)
    {
        Vector3 axis = Vector3.Cross(m_forward, m_right).normalized;
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        m_forward = (rotation * m_forward).normalized;
        m_right = (rotation * m_right).normalized;
    }

    public void RollLeft(float angle)
    {
        m_right = (Quaternion.AngleAxis(-angle, m_forward) * m_right).normalized;
    }

    public void RollRight(float angle)
    {
        m_right = (Quaternion.AngleAxis(angle, m_forward) * m_right).normalized;
    }

    public void Push()
    {
        m_positionStack.Push(new TurtlePosition()
        {
            m_storedPosition = m_position,
            m_storedForward = m_forward,
            m_storedRight = m_right
        });
    }

    public void Pop()
    {
        TurtlePosition position = m_positionStack.Pop();
        m_position = position.m_storedPosition;
        m_forward = position.m_storedForward;
        m_right = position.m_storedRight;
    }
}

public class LSystem : MonoBehaviour
{
    public LSystemGenerationRuleAsset m_rulesAsset;

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
        if (m_rulesAsset != null &&
            m_rulesAsset.d_lines != null)
        {
            for (int i = 0; i < m_rulesAsset.d_lines.Count; i += 2)
            {
                Debug.DrawLine(m_rulesAsset.d_lines[i], m_rulesAsset.d_lines[i + 1]);
            }
        }
    }

    private void OnApplicationQuit()
    {
        m_rulesAsset.d_lines.Clear();
    }
}
