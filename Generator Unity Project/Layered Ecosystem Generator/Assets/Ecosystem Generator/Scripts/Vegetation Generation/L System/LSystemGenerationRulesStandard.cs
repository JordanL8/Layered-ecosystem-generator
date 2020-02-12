﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[CreateAssetMenu(fileName = "L System Rules", menuName = "Ecosystem Generator/L System Rules/Standard Rules", order = 0)]
public class LSystemGenerationRulesStandard : LSystemGenerationRuleAsset
{
    // This rule set uses the same turtle commands as Houdini's L-System turtle.

    [Header("Model Generation")]
    [Tooltip("Sets the default distance to move then turtle when you use the F command.")]
    public float m_stepSize;

    [Tooltip("Sets the default amount to multiply the current length by when you use the \" command.")]
    public float m_stepSizeScale;

    [Tooltip("Sets the default angle that all rotation commands use.")]
    public float m_angle;

    [Tooltip("Sets the default amount to multiply the current angle by when you use the ; command.")]
    public float m_angleScale;

    [Header("Rendering")]
    [Tooltip("Sets the default thickness of the branches.")]
    public float m_thickness;

    [Tooltip("Sets the default amount to multiply the current thickness by when you use the ; command.")]
    public float m_thicknessScale;

    public override void Build(string commandString)
    {
        LSystemTurtle turtle = new LSystemTurtle(Vector3.zero, Vector3.up, Vector3.right, m_stepSize, m_stepSizeScale, m_angle, m_angleScale, m_thickness, m_thicknessScale);
        for (int i = 0; i < commandString.Length; i++)
        {
            if (i + 1 < commandString.Length)
            {
                if (commandString[i + 1] == '(') // Move command has parameters
                {
                    StringBuilder parameters = new StringBuilder();
                    int j = i + 2;
                    for (; j < commandString.Length && commandString[j] != ')'; j++)
                    {
                        parameters.Append(commandString[j]);
                    }
                    string parametersString = parameters.ToString();
                    string[] parameterArray = parametersString.Split(',');
                    ParseCommand(commandString[i], turtle, parameterArray);
                    i = j;
                    continue;
                }
            }
            ParseCommand(commandString[i], turtle);
        }

        turtle.Commit();
        List<LSystemBranch> branches = turtle.GetBranches();

        for (int i = 0; i < branches.Count; i++)
        {
            for (int j = 0; j < branches[i].m_branchPositions.Count - 1; j++)
            {
                StemSegment seg = new StemSegment()
                {
                    m_startPos = branches[i].m_branchPositions[j].m_position,
                    m_endPos = branches[i].m_branchPositions[j + 1].m_position,
                    m_startRadius = branches[i].m_branchPositions[j].m_radius < 0 ? branches[i].m_branchPositions[j + 1].m_radius : branches[i].m_branchPositions[j].m_radius,
                    m_endRadius = branches[i].m_branchPositions[j + 1].m_radius
                };
                Mesh segmentMesh = StemFactory.CreateStemMesh(seg, 6, StemCapOption.BOTH_CAPS);
                GameObject obj = new GameObject();
                obj.AddComponent<MeshRenderer>();
                obj.AddComponent<MeshFilter>().sharedMesh = segmentMesh;
                obj.transform.position = seg.m_startPos;
                obj.transform.parent = GameObject.Find("LSystem Tree").transform;
            }
        }
    }

    private void ParseCommand(char command, LSystemTurtle turtle, params string[] commandParameters)
    {
        switch (command)
        {
            case 'F':
            {
                if(commandParameters.Length > 0)
                {
                    float lengthParam;
                    float widthParam;
                    if (float.TryParse(commandParameters[0], out lengthParam))
                    {
                        if (commandParameters.Length > 1)
                        {
                            if (float.TryParse(commandParameters[1], out widthParam))
                            {
                                turtle.Move(lengthParam, widthParam);
                                break;
                            }
                            else
                            {
                                turtle.Move(lengthParam);
                                break;
                            }
                        }
                        else
                        {
                            turtle.Move(lengthParam);
                            break;
                        }
                    }
                }
                turtle.Move();
            } break;

            case '-':
            {
                if (commandParameters.Length > 0)
                {
                    float angleParam;
                    if(float.TryParse(commandParameters[0], out angleParam))
                    {
                        turtle.YawLeft(angleParam);
                        break;
                    }
                }
                turtle.YawLeft();
            } break;

            case '+':
            {
                if (commandParameters.Length > 0)
                {
                    float angleParam;
                    if (float.TryParse(commandParameters[0], out angleParam))
                    {
                        turtle.YawRight(angleParam);
                        break;
                    }
                }
                turtle.YawRight();
            } break;

            case '&':
            {
                if (commandParameters.Length > 0)
                {
                    float angleParam;
                    if (float.TryParse(commandParameters[0], out angleParam))
                    {
                        turtle.PitchDown(angleParam);
                        break;
                    }
                }
                turtle.PitchDown();
            } break;

            case '^':
            {
                if (commandParameters.Length > 0)
                {
                    float angleParam;
                    if (float.TryParse(commandParameters[0], out angleParam))
                    {
                        turtle.PitchUp(angleParam);
                        break;
                    }
                }
                turtle.PitchUp();
            } break;

            case '/':
            {
                if (commandParameters.Length > 0)
                {
                    float angleParam;
                    if (float.TryParse(commandParameters[0], out angleParam))
                    {
                        turtle.RollLeft(angleParam);
                        break;
                    }
                }
                turtle.RollLeft();
            } break;

            case '\\':
            {
                if (commandParameters.Length > 0)
                {
                    float angleParam;
                    if (float.TryParse(commandParameters[0], out angleParam))
                    {
                        turtle.RollRight(angleParam);
                        break;
                    }
                }
                turtle.RollRight();
            } break;

            case '|':
            {
                turtle.YawLeft(180);
            } break;

            case '*':
            {
                turtle.RollLeft(180);
            } break;

            case '~':
            {
                if (commandParameters.Length > 0)
                {
                    float angleLimitParam;
                    if (float.TryParse(commandParameters[0], out angleLimitParam))
                    {
                        turtle.RandomRotation(angleLimitParam);
                        break;
                    }
                }
                turtle.RandomRotation();
            } break; 

            case '[':
            {
                turtle.Push();
            } break;

            case ']':
            {
                turtle.Pop();
            } break;
            
            case '"':
            {
                if(commandParameters.Length > 0)
                {
                    float scaleParam;
                    if (float.TryParse(commandParameters[0], out scaleParam))
                    {
                        turtle.MultplyStepSize(scaleParam);
                        break;
                    }
                }
                turtle.MultplyStepSize();
            } break;

            case '!':
            {
                if (commandParameters.Length > 0)
                {
                    float scaleParam;
                    if (float.TryParse(commandParameters[0], out scaleParam))
                    {
                        turtle.MultplyThickness(scaleParam);
                        break;
                    }
                }
                turtle.MultplyThickness();
            } break;

            case ';':
            {
                if (commandParameters.Length > 0)
                {
                    float scaleParam;
                    if (float.TryParse(commandParameters[0], out scaleParam))
                    {
                        turtle.MultplyAngle(scaleParam);
                        break;
                    }
                }
                turtle.MultplyAngle();
            } break;

            case '_':
            {
                if (commandParameters.Length > 0)
                {
                    float scaleParam;
                    if (float.TryParse(commandParameters[0], out scaleParam))
                    {
                        turtle.DivideStepSize(scaleParam);
                        break;
                    }
                }
                turtle.DivideStepSize();
            } break;

            case '?':
            {
                if (commandParameters.Length > 0)
                {
                    float scaleParam;
                    if (float.TryParse(commandParameters[0], out scaleParam))
                    {
                        turtle.DivideThickness(scaleParam);
                        break;
                    }
                }
                turtle.DivideThickness();
            } break;

            case '@':
            {
                if (commandParameters.Length > 0)
                {
                    float scaleParam;
                    if (float.TryParse(commandParameters[0], out scaleParam))
                    {
                        turtle.DivideAngle(scaleParam);
                        break;
                    }
                }
                turtle.DivideAngle();
            } break;
        }
    }
}