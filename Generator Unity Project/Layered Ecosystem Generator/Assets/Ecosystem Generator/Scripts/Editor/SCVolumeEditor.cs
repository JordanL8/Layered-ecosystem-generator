using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Code based on the following tutorial series by Sebastian Lague: https://www.youtube.com/watch?v=bPO7_JNWNmI

[CustomEditor(typeof(SCTree))]
public class SCVolumeEditor : Editor
{
    public class SelectionInformation
    {
        public int m_shapeIndex;
        public int m_mouseOverShapeIndex;

        public int m_pointIndex = -1;
        public bool m_mouseOverPoint;
        public bool m_pointSelected;
        public Vector3 m_dragStartPosition;

        public int m_lineIndex = -1;
        public bool m_mouseOverLine;
    }


    private SCTree m_scTree;
    private SCVolume m_scVolume;
    private SelectionInformation m_selectionInformation;
    private SCVolumeShape SelectedShape { get { return m_scVolume?.m_volumeShapes[m_selectionInformation.m_shapeIndex]; } }

    private float m_gizmoDiscRadius = 0.15f;
    private float m_sqrGizmoDiscRadius;

    private bool m_needsRepaint;

    private void OnEnable()
    {
        SetInitialReferences();
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    }

    private void SetInitialReferences()
    {
        m_scTree = target as SCTree;
        m_scVolume = m_scTree.m_volume;
        m_selectionInformation = new SelectionInformation();
        m_sqrGizmoDiscRadius = m_gizmoDiscRadius * m_gizmoDiscRadius;
    }

    private void OnSceneGUI()
    {
        Event guiEvent = Event.current;
        HandleEvent(guiEvent);
    }

    private void HandleEvent(Event guiEvent)
    {
        if (guiEvent.type == EventType.Repaint)
        {
            DrawBounds();
        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            HandleInput(guiEvent);
            if (m_needsRepaint)
            {
                HandleUtility.Repaint();
                m_needsRepaint = false;
            }
        }
    }

    void OnUndoRedoPerformed()
    {
        if(m_selectionInformation.m_shapeIndex >= m_scVolume.m_volumeShapes.Count)
        {
            m_selectionInformation.m_shapeIndex = m_scVolume.m_volumeShapes.Count - 1;
        }
    }

    #region Input

    private void HandleInput(Event guiEvent)
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float drawPlaneDistance = m_scTree.transform.position.z;
        float distanceToDrawPlane = (drawPlaneDistance - mouseRay.origin.z) / mouseRay.direction.z;
        Vector3 mousePosition = mouseRay.GetPoint(distanceToDrawPlane);

        if (guiEvent.button != 0)
        {
            return;
        }

        if (guiEvent.modifiers == EventModifiers.None && m_scVolume != null)
        {
            if (guiEvent.type == EventType.MouseDown)
            {
                HandleLeftMouseDown(mousePosition);
            }

            if (guiEvent.type == EventType.MouseDrag)
            {
                HandleLeftMouseDrag(mousePosition);
            }
        }
        else if (guiEvent.modifiers == EventModifiers.Control)
        {
            if (guiEvent.type == EventType.MouseDown)
            {
                HandleControlMouseDown(mousePosition);
            }
        }

        if (guiEvent.type == EventType.MouseUp && m_scVolume != null)
        {
            HandleLeftMouseUp(mousePosition);
        }

        if (!m_selectionInformation.m_pointSelected && m_scVolume != null)
        {
            UpdateMouseOverInformation(mousePosition);
        }
    }

    private void HandleControlMouseDown(Vector3 mousePosition)
    {
        if (m_scVolume == null)
        {
            CreateAndAssignNewSCVolume();
        }
        else
        {
            CreateNewVolumeShape();
            CreateNewVolumeShapePoint(mousePosition);
        }
    }

    private void HandleLeftMouseDown(Vector3 mousePosition)
    {
        if(m_scVolume.m_volumeShapes.Count == 0)
        {
            CreateNewVolumeShape();
        }

        SelectShapeUnderMouse();

        if (m_selectionInformation.m_mouseOverPoint)
        {
            SelectPointUnderMouse();
        }
        else
        {
            CreateNewVolumeShapePoint(mousePosition);
        }
    }

    private void HandleLeftMouseUp(Vector3 mousePosition)
    {
        if(m_selectionInformation.m_pointSelected)
        {
            SelectedShape.m_boundingPoints[m_selectionInformation.m_pointIndex] = m_selectionInformation.m_dragStartPosition;
            Undo.RecordObject(m_scVolume, "Move Point");
            SelectedShape.m_boundingPoints[m_selectionInformation.m_pointIndex] = mousePosition;
            m_selectionInformation.m_pointSelected = false;
            m_selectionInformation.m_pointIndex = -1;
            m_needsRepaint = true;
        }
    }

    private void HandleLeftMouseDrag(Vector3 mousePosition)
    {
        if (m_selectionInformation.m_pointSelected)
        {
            SelectedShape.m_boundingPoints[m_selectionInformation.m_pointIndex] = mousePosition;
            m_needsRepaint = true;
        }
    }

    private void UpdateMouseOverInformation(Vector3 mousePosition)
    {
        // Check to see if the mouse is above any shape point.
        int mouseOverPointIndex = -1;
        int mouseOverShapeIndex = -1;
        for (int i = 0; i < m_scVolume.m_volumeShapes.Count; i++)
        {
            SCVolumeShape curShape = m_scVolume.m_volumeShapes[i];
            for (int j = 0; j < curShape.m_boundingPoints.Count; j++)
            {
                if (Vector3.SqrMagnitude(curShape.m_boundingPoints[j] - mousePosition) < m_sqrGizmoDiscRadius)
                {
                    mouseOverPointIndex = j;
                    mouseOverShapeIndex = i;
                    break;
                }
            }
        }

        // Update selected point information.
        if (mouseOverPointIndex != m_selectionInformation.m_pointIndex || mouseOverShapeIndex != m_selectionInformation.m_mouseOverShapeIndex)
        {
            m_selectionInformation.m_mouseOverShapeIndex = mouseOverShapeIndex;

            m_selectionInformation.m_pointIndex = mouseOverPointIndex;
            m_selectionInformation.m_mouseOverPoint = mouseOverPointIndex != -1;

            m_needsRepaint = true;
        }

        // Update selected line information based on the above point information. Points take preference over lines.
        if (m_selectionInformation.m_mouseOverPoint)
        {
            m_selectionInformation.m_mouseOverLine = false;
            m_selectionInformation.m_lineIndex = -1;
        }
        else
        {
            // Check to see if the mouse is near any shape line.
            int mouseOverLineIndex = -1;
            float closestLineDistance = m_gizmoDiscRadius;
            for (int i = 0; i < m_scVolume.m_volumeShapes.Count; i++)
            {
                SCVolumeShape curShape = m_scVolume.m_volumeShapes[i];
                for (int j = 0; j < curShape.m_boundingPoints.Count; j++)
                {
                    Vector3 curPoint = curShape.m_boundingPoints[j];
                    Vector3 nextPont = curShape.m_boundingPoints[(j + 1) % curShape.m_boundingPoints.Count];
                    float distanceFromMouseToLine = HandleUtility.DistancePointToLineSegment(mousePosition, curPoint, nextPont);
                    if (distanceFromMouseToLine < closestLineDistance)
                    {
                        closestLineDistance = distanceFromMouseToLine;
                        mouseOverLineIndex = j;
                        mouseOverShapeIndex = i;
                    }
                }
            }

            // Update selected line information.
            if(m_selectionInformation.m_lineIndex != mouseOverLineIndex || mouseOverShapeIndex != m_selectionInformation.m_mouseOverShapeIndex)
            {
                m_selectionInformation.m_mouseOverShapeIndex = mouseOverShapeIndex;
                m_selectionInformation.m_lineIndex = mouseOverLineIndex;
                m_selectionInformation.m_mouseOverLine = mouseOverLineIndex != -1;
                m_needsRepaint = true;
            }
        }
    }

    private void SelectPointUnderMouse()
    {
        m_selectionInformation.m_pointSelected = true;
        m_selectionInformation.m_mouseOverPoint = true;
        m_selectionInformation.m_mouseOverLine = false;
        m_selectionInformation.m_lineIndex = -1;

        m_selectionInformation.m_dragStartPosition = SelectedShape.m_boundingPoints[m_selectionInformation.m_pointIndex];
        m_needsRepaint = true;
    }

    private void SelectShapeUnderMouse()
    {
        if(m_selectionInformation.m_mouseOverShapeIndex != -1)
        {
            m_selectionInformation.m_shapeIndex = m_selectionInformation.m_mouseOverShapeIndex;
            m_needsRepaint = true;
        }
    }

    #endregion


    #region Volume Management

    private void CreateNewVolumeShape()
    {
        Undo.RecordObject(m_scVolume, "Create new VolumeShape");
        m_scVolume.m_volumeShapes.Add(new SCVolumeShape());
        m_selectionInformation.m_shapeIndex = m_scVolume.m_volumeShapes.Count - 1;
    }

    private void CreateNewVolumeShapePoint(Vector3 position)
    {
        bool mouseIsOverSelectedShape = m_selectionInformation.m_mouseOverShapeIndex == m_selectionInformation.m_shapeIndex;
        int newPointIndex = m_selectionInformation.m_mouseOverLine && mouseIsOverSelectedShape ? m_selectionInformation.m_lineIndex + 1 : SelectedShape.m_boundingPoints.Count;
        Undo.RecordObject(m_scVolume, "Add New Point");
        SelectedShape.m_boundingPoints.Insert(newPointIndex, position);
        m_selectionInformation.m_pointIndex = newPointIndex;
        m_selectionInformation.m_mouseOverShapeIndex = m_selectionInformation.m_shapeIndex;
        m_needsRepaint = true;

        SelectPointUnderMouse();
    }

    #endregion


    #region Rendering

    private void DrawBounds()
    {
        if(m_scVolume == null)
        {
            return;
        }
        if (m_scTree.transform.hasChanged)
        {
            m_scTree.transform.hasChanged = false;
        }
        for (int i = 0; i < m_scVolume.m_volumeShapes.Count; i++)
        {
            SCVolumeShape volumeShape = m_scVolume.m_volumeShapes[i];
            bool shapeIsSelected = i == m_selectionInformation.m_shapeIndex;
            bool mouseIsOverShape = i == m_selectionInformation.m_mouseOverShapeIndex;
            Color deselectedShapeColour = Color.gray;

            for (int j = 0; j < volumeShape.m_boundingPoints.Count; j++)
            {
                Vector3 curPoint = volumeShape.m_boundingPoints[j];
                Vector3 nextPont = volumeShape.m_boundingPoints[(j + 1) % volumeShape.m_boundingPoints.Count];

                if (j == m_selectionInformation.m_lineIndex && mouseIsOverShape)
                {
                    Handles.color = Color.cyan;
                    Handles.DrawLine(curPoint, nextPont);
                }
                else
                {
                    Handles.color = shapeIsSelected ? Color.green : deselectedShapeColour;
                    Handles.DrawDottedLine(curPoint, nextPont, 4);
                }


                if (j == m_selectionInformation.m_pointIndex && mouseIsOverShape)
                {
                    Handles.color = m_selectionInformation.m_pointSelected ? Color.blue : Color.cyan;
                }
                else
                {
                    Handles.color = shapeIsSelected ? Color.green : deselectedShapeColour;
                }
                Handles.DrawSolidDisc(curPoint, Vector3.back, m_gizmoDiscRadius);

            }
        }
        m_needsRepaint = false;
    }

    #endregion
    
    private void CreateAndAssignNewSCVolume()
    {
        SCVolume newVolume = ScriptableObject.CreateInstance<SCVolume>();

        string saveDirectory = EditorUtility.SaveFilePanel("Save New SCVolume Asset", Application.dataPath, "New SCVolume", "asset");
        if (saveDirectory == "")
        {
            return;
        }
        string relativeSavePath = saveDirectory;
        if (saveDirectory.StartsWith(Application.dataPath))
        {
            relativeSavePath = "Assets" + saveDirectory.Substring(Application.dataPath.Length);
        }
        AssetDatabase.CreateAsset(newVolume, relativeSavePath);
        AssetDatabase.SaveAssets();

        m_scTree.m_volume = newVolume;
        m_scVolume = newVolume;
    }
}
