using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemPosition
{
    public Vector3 m_position;
    public float m_radius;
}

public class LSystemBranch
{
    public List<LSystemPosition> m_branchPositions;

    public LSystemBranch()
    {
        m_branchPositions = new List<LSystemPosition>();
    }
}



public class TurtleProperties
{
    public Vector3 m_position;
    public Vector3 m_forward;
    public Vector3 m_right;

    public float m_stepSize;
    public float m_stepSizeScale;
    public float m_angle;
    public float m_angleScale;
    public float m_thickness;
    public float m_thicknessScale;

    public LSystemBranch m_branch;
}

public class LSystemTurtle
{

    public Vector3 Position
    {
        get { return m_currentTurtleProperties.m_position; }
    }

    private Stack<TurtleProperties> m_positionStack;

    private List<LSystemBranch> m_branches;
    private TurtleProperties m_currentTurtleProperties;

    

    public LSystemTurtle(Vector3 position, Vector3 direction, Vector3 right, float stepSize, float stepSizeScale, float angle,
        float angleScale, float thickness, float thicknessScale)
    {
        m_positionStack = new Stack<TurtleProperties>();
        m_branches = new List<LSystemBranch>();
        m_currentTurtleProperties = new TurtleProperties()
        {
            m_position = position,
            m_forward = direction,
            m_right = right,
            m_stepSize = stepSize,
            m_stepSizeScale = stepSizeScale,
            m_angle = angle,
            m_angleScale = angleScale,
            m_thickness = thickness,
            m_thicknessScale = thicknessScale,
            m_branch = new LSystemBranch(),
        };
        m_currentTurtleProperties.m_branch.m_branchPositions.Add(new LSystemPosition()
        {
            m_position = position,
            m_radius = -1
        });
        Push();
    }

    public Vector3 Move(float distance = -1, float radius = -1)
    {
        distance = distance < 0 ? m_currentTurtleProperties.m_stepSize : distance;
        radius = radius < 0 ? m_currentTurtleProperties.m_thickness : radius;

        m_currentTurtleProperties.m_position += m_currentTurtleProperties.m_forward * distance;
        m_currentTurtleProperties.m_branch.m_branchPositions.Add(new LSystemPosition()
        {
            m_position = m_currentTurtleProperties.m_position,
            m_radius = radius
        });
        return m_currentTurtleProperties.m_position;
    }

    #region Rotation
    public void PitchUp(float angle = -1)
    {
        angle = angle < 0 ? m_currentTurtleProperties.m_angle : angle;
        m_currentTurtleProperties.m_forward = (Quaternion.AngleAxis(angle, m_currentTurtleProperties.m_right) * m_currentTurtleProperties.m_forward).normalized;
    }

    public void PitchDown(float angle = -1)
    {
        angle = angle < 0 ? m_currentTurtleProperties.m_angle : angle;
        m_currentTurtleProperties.m_forward = (Quaternion.AngleAxis(-angle, m_currentTurtleProperties.m_right) * m_currentTurtleProperties.m_forward).normalized;
    }

    public void YawLeft(float angle = -1)
    {
        angle = angle < 0 ? m_currentTurtleProperties.m_angle : angle;
        Vector3 axis = Vector3.Cross(m_currentTurtleProperties.m_forward, m_currentTurtleProperties.m_right).normalized;
        Quaternion rotation = Quaternion.AngleAxis(-angle, axis);
        m_currentTurtleProperties.m_forward = (rotation * m_currentTurtleProperties.m_forward).normalized;
        m_currentTurtleProperties.m_right = (rotation * m_currentTurtleProperties.m_right).normalized;
    }

    public void YawRight(float angle = -1)
    {
        angle = angle < 0 ? m_currentTurtleProperties.m_angle : angle;
        Vector3 axis = Vector3.Cross(m_currentTurtleProperties.m_forward, m_currentTurtleProperties.m_right).normalized;
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        m_currentTurtleProperties.m_forward = (rotation * m_currentTurtleProperties.m_forward).normalized;
        m_currentTurtleProperties.m_right = (rotation * m_currentTurtleProperties.m_right).normalized;
    }

    public void RollLeft(float angle = -1)
    {
        angle = angle < 0 ? m_currentTurtleProperties.m_angle : angle;
        m_currentTurtleProperties.m_right = (Quaternion.AngleAxis(-angle, m_currentTurtleProperties.m_forward) * m_currentTurtleProperties.m_right).normalized;
    }

    public void RollRight(float angle = -1)
    {
        angle = angle < 0 ? m_currentTurtleProperties.m_angle : angle;
        m_currentTurtleProperties.m_right = (Quaternion.AngleAxis(angle, m_currentTurtleProperties.m_forward) * m_currentTurtleProperties.m_right).normalized;
    }

    // ~
    public void RandomRotation(float angle = -1)
    {
        angle = angle < 0 ? 180 : angle;
        if(Random.value < 0.5f) { PitchUp(Random.value * angle); } else { PitchDown(Random.value * angle); }
        if(Random.value < 0.5f) { RollLeft(Random.value * angle); } else { RollRight(Random.value * angle); }
        if(Random.value < 0.5f) { YawLeft(Random.value * angle); } else { YawRight(Random.value * angle); }
    }
    #endregion

    #region Scaling
    // "
    public void MultplyStepSize(float scale = -1)
    {
        scale = scale < 0 ? m_currentTurtleProperties.m_stepSizeScale : scale;
        m_currentTurtleProperties.m_stepSize *= scale;
    }

    // !
    public void MultplyThickness(float scale = -1)
    {
        scale = scale < 0 ? m_currentTurtleProperties.m_thicknessScale : scale;
        m_currentTurtleProperties.m_thickness *= scale;
    }

    // ;
    public void MultplyAngle(float scale = -1)
    {
        scale = scale < 0 ? m_currentTurtleProperties.m_angleScale : scale;
        m_currentTurtleProperties.m_angle *= scale;
    }

    // _
    public void DivideStepSize(float scale = -1)
    {
        scale = scale < 0 ? m_currentTurtleProperties.m_angleScale : scale;
        m_currentTurtleProperties.m_stepSize /= scale;
    }

    // ?
    public void DivideThickness(float scale = -1)
    {
        scale = scale < 0 ? m_currentTurtleProperties.m_angleScale : scale;
        m_currentTurtleProperties.m_thickness /= scale;
    }

    // @
    public void DivideAngle(float scale = -1)
    {
        scale = scale < 0 ? m_currentTurtleProperties.m_angleScale : scale;
        m_currentTurtleProperties.m_angle /= scale;
    }
    #endregion

    #region Stack Management
    // [
    public void Push()
    {
        TurtleProperties newPosition = new TurtleProperties()
        {
            m_position = m_currentTurtleProperties.m_position,
            m_forward = m_currentTurtleProperties.m_forward,
            m_right = m_currentTurtleProperties.m_right,
            m_stepSize = m_currentTurtleProperties.m_stepSize,
            m_stepSizeScale = m_currentTurtleProperties.m_stepSizeScale,
            m_angle = m_currentTurtleProperties.m_angle,
            m_angleScale = m_currentTurtleProperties.m_angleScale,
            m_thickness = m_currentTurtleProperties.m_thickness,
            m_thicknessScale = m_currentTurtleProperties.m_thicknessScale,
            m_branch = new LSystemBranch()
        };

        newPosition.m_branch.m_branchPositions.Add(new LSystemPosition()
        {
            m_position = m_currentTurtleProperties.m_position,
            m_radius = -1
        });

        m_positionStack.Push(newPosition);
    }

    // ]
    public void Pop()
    {
        m_branches.Add(m_currentTurtleProperties.m_branch);

        TurtleProperties position = m_positionStack.Pop();
        m_currentTurtleProperties.m_position = position.m_position;
        m_currentTurtleProperties.m_forward = position.m_forward;
        m_currentTurtleProperties.m_right = position.m_right;
        m_currentTurtleProperties.m_stepSize = position.m_stepSize;
        m_currentTurtleProperties.m_stepSizeScale = position.m_stepSizeScale;
        m_currentTurtleProperties.m_angle = position.m_angle;
        m_currentTurtleProperties.m_angleScale = position.m_angleScale;
        m_currentTurtleProperties.m_thickness = position.m_thickness;
        m_currentTurtleProperties.m_thicknessScale = position.m_thicknessScale;
        m_currentTurtleProperties.m_branch = position.m_branch;
    }

    public void Commit()
    {
        if (m_positionStack.Count > 0)
        {
            Pop();
        }
    }
    #endregion

    public List<LSystemBranch> GetBranches()
    {
        return m_branches;
    }
}