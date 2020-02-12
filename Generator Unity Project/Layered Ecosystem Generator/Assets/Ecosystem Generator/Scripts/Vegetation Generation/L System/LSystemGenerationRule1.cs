using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[CreateAssetMenu(fileName = "L System Rule 1", menuName = "Ecosystem Generator/L System Rules/Rule 1", order = 0)]
public class LSystemGenerationRule1 : LSystemGenerationRuleAsset
{
    public float m_length;
    public float m_uTilt;
    public float m_vTilt;
    public float m_twist;

    public override void Build(string commandString)
    {
        ////LSystemTurtle turtle = new LSystemTurtle(Vector3.zero, Vector3.up, Vector3.right);
        ////for (int i = 0; i < commandString.Length; i++)
        ////{
        ////    switch (commandString[i])
        ////    {
        ////        case 'F':
        ////        {
        ////            if(i + 1 < commandString.Length)
        ////            {
        ////                if(commandString[i + 1] == '(') // Move command has parameters
        ////                {
        ////                    StringBuilder parameters = new StringBuilder();
        ////                    int j = i + 2;
        ////                    for (; j < commandString.Length && commandString[j] != ')'; j++)
        ////                    {
        ////                        parameters.Append(commandString[j]);
        ////                    }
        ////                    string parametersString = parameters.ToString();
        ////                    string[] parameterArray = parametersString.Split(',');
        ////                    float lengthParam;
        ////                    float radiusParam;
        ////                    if (float.TryParse(parameterArray[0], out lengthParam)
        ////                        && float.TryParse(parameterArray[1], out radiusParam))
        ////                    {
        ////                        turtle.Move(lengthParam, radiusParam);
        ////                    }
        ////                    i = j;
        ////                }
        ////            }
        ////            // Draw from turtle position to next position
        ////            //Vector3 startPos = turtle.Position;
        ////            //Vector3 endPos = turtle.Move(m_length);
        ////            //d_lines.Add(startPos);
        ////            //d_lines.Add(endPos);
        ////        } break;
        ////        case '+':
        ////        {
        ////            turtle.YawLeft(m_vTilt);
        ////        } break;
        ////        case '-':
        ////        {
        ////            turtle.YawRight(m_vTilt);
        ////        } break;
        ////        case '}':
        ////        {
        ////            turtle.PitchDown(m_uTilt);
        ////        } break;
        ////        case '{':
        ////        {
        ////            turtle.PitchUp(m_uTilt);
        ////        } break;
        ////        case '>':
        ////        {
        ////            turtle.RollLeft(m_twist);
        ////        } break;
        ////        case '<':
        ////        {
        ////            turtle.RollRight(m_twist);
        ////        } break;
        ////        case '|':
        ////        {
        ////            turtle.YawLeft(180);
        ////        } break;
        ////        case '[':
        ////        {
        ////            turtle.Push();
        ////        } break;
        ////        case ']':
        ////        {
        ////            turtle.Pop();
        ////        } break;
        ////    }
        ////}
        //turtle.Commit();
        //List<LSystemBranch> branches = turtle.GetBranches();

        //for (int i = 0; i < branches.Count; i++)
        //{
        //    for (int j = 0; j < branches[i].m_branchPositions.Count - 1; j++)
        //    {
        //        StemSegment seg = new StemSegment()
        //        {
        //            m_startPos = branches[i].m_branchPositions[j].m_position,
        //            m_endPos = branches[i].m_branchPositions[j + 1].m_position,
        //            m_startRadius = branches[i].m_branchPositions[j].m_radius < 0 ? branches[i].m_branchPositions[j + 1].m_radius : branches[i].m_branchPositions[j].m_radius,
        //            m_endRadius = branches[i].m_branchPositions[j + 1].m_radius
        //        };
        //        Mesh segmentMesh = StemFactory.CreateStemMesh(seg, 6, StemCapOption.BOTH_CAPS);
        //        GameObject obj = new GameObject();
        //        obj.AddComponent<MeshRenderer>();
        //        obj.AddComponent<MeshFilter>().sharedMesh = segmentMesh;
        //        obj.transform.position = seg.m_startPos;
        //        obj.transform.parent = GameObject.Find("LSystem Tree").transform;
        //    }
        //}
    }
}
