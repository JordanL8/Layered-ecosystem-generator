using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "L System Rule 1", menuName = "Ecosystem Generator/L System Rules/Rule 1", order = 0)]
public class LSystemGenerationRule1 : LSystemGenerationRuleAsset
{
    public float m_length;
    public float m_uTilt;
    public float m_vTilt;
    public float m_twist;

    public override void Build(string commandString)
    {

        Turtle turtle = new Turtle(Vector3.zero, Vector3.up, Vector3.right);
        for (int i = 0; i < commandString.Length; i++)
        {
            switch (commandString[i])
            {
                case 'F':
                {
                    // Draw from turtle position to next position
                    Vector3 startPos = turtle.Position;
                    Vector3 endPos = turtle.Move(m_length);
                    d_lines.Add(startPos);
                    d_lines.Add(endPos);
                } break;
                case '+':
                {
                    turtle.YawLeft(m_vTilt);
                } break;
                case '-':
                {
                    turtle.YawRight(m_vTilt);
                } break;
                case '}':
                {
                    turtle.PitchDown(m_uTilt);
                } break;
                case '{':
                {
                    turtle.PitchUp(m_uTilt);
                } break;
                case '>':
                {
                    turtle.RollLeft(m_twist);
                } break;
                case '<':
                {
                    turtle.RollRight(m_twist);
                } break;
                case '|':
                {
                    turtle.YawLeft(180);
                } break;
                case '[':
                {
                    turtle.Push();
                } break;
                case ']':
                {
                    turtle.Pop();
                } break;
            }
        }
    }
}
