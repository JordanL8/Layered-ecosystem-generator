using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;


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
    private SelectionInformation m_selectionInformation;
    private SCVolumeShape SelectedShape { get { return m_scTree.m_volume?.m_volumeShapes[m_selectionInformation.m_shapeIndex]; } }

    private float m_gizmoDiscRadius = 0.15f;
    private float m_sqrGizmoDiscRadius;

    private bool m_needsRepaint;
    
    private void OnEnable()
    {
        SetInitialReferences();
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
        PrefabStage.prefabStageClosing += DisableEditor;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        PrefabStage.prefabStageClosing -= DisableEditor;
    }

    private void SetInitialReferences()
    {
        m_scTree = target as SCTree;
        m_selectionInformation = new SelectionInformation();
        m_sqrGizmoDiscRadius = m_gizmoDiscRadius * m_gizmoDiscRadius;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        int shapeDeleteIndex = -1;
        if (m_scTree.m_volume == null)
        {
            if (GUILayout.Button("Create New Volume"))
            {
                CreateAndAssignNewSCVolume();
            }
        }
        else
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                if (GUILayout.Button("Edit"))
                {
                    EnableEditor();
                }
                if(GUILayout.Button("Generate"))
                {
                    DestroyTreeChildren();
                    (target as SCTree).Generate(0);
                }
                if(GUILayout.Button("Destroy Tree"))
                {
                    DestroyTreeChildren();
                }
            }
            else
            {
                if (GUILayout.Button("Stop Editing"))
                {
                    CloseEditor();
                }
                if (GUILayout.Button(m_scTree.m_volume.m_showVolumeShapeList.value ? "Hide Volume Shape List" : "Show Volume Shape List"))
                {
                    m_scTree.m_volume.m_showVolumeShapeList.value = !m_scTree.m_volume.m_showVolumeShapeList.value;
                }
            }

            
            if(EditorGUILayout.BeginFadeGroup(m_scTree.m_volume.m_showVolumeShapeList.faded))
            {
                for (int i = 0; i < m_scTree.m_volume.m_volumeShapes.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(i == 0 ? "Trunk Path" : "Leaf Volume " + i);
                    GUI.enabled = i != m_selectionInformation.m_shapeIndex;
                    if (GUILayout.Button("Select"))
                    {
                        m_selectionInformation.m_shapeIndex = i;
                    }
                    GUI.enabled = i > 0;
                    if (GUILayout.Button("Delete"))
                    {
                        shapeDeleteIndex = i;
                    }
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndFadeGroup();
            if (shapeDeleteIndex != -1)
            {
                Undo.RecordObject(m_scTree.m_volume, "Delete Volume Shape");
                m_scTree.m_volume.m_volumeShapes.RemoveAt(shapeDeleteIndex);
                m_selectionInformation.m_shapeIndex = Mathf.Clamp(m_selectionInformation.m_shapeIndex, 0, m_scTree.m_volume.m_volumeShapes.Count - 1);
            }
        }

        if(GUI.changed)
        {
            m_needsRepaint = true;
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        if(m_scTree.m_volume == null)
        {
            return;
        }
        Event guiEvent = Event.current;
        HandleEvent(guiEvent);
    }

    private void HandleEvent(Event guiEvent)
    {
        if (guiEvent.type == EventType.Repaint)
        {
            if (m_scTree.m_volume.m_isEditable)
            {
                DrawBounds();
            }
        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            if (m_scTree.m_volume.m_isEditable && Tools.current != Tool.View)
            {
                HandleInput(guiEvent);
            }
            if (m_needsRepaint)
            {
                HandleUtility.Repaint();
                m_needsRepaint = false;
            }
        }
    }

    void OnUndoRedoPerformed()
    {
        if(m_selectionInformation.m_shapeIndex >= m_scTree.m_volume.m_volumeShapes.Count || m_selectionInformation.m_shapeIndex == -1)
        {
            m_selectionInformation.m_shapeIndex = m_scTree.m_volume.m_volumeShapes.Count - 1;
        }
    }

    #region Input

    private void HandleInput(Event guiEvent)
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float drawPlaneDistance = 0;// m_scTree.transform.position.z;
        float distanceToDrawPlane = (drawPlaneDistance - mouseRay.origin.z) / mouseRay.direction.z;
        Vector3 mousePosition = mouseRay.GetPoint(distanceToDrawPlane);

        if (guiEvent.button != 0)
        {
            return;
        }

        if (guiEvent.modifiers == EventModifiers.None)
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

        if (guiEvent.type == EventType.MouseUp)
        {
            HandleLeftMouseUp(mousePosition);
        }

        if (!m_selectionInformation.m_pointSelected)
        {
            UpdateMouseOverInformation(mousePosition);
        }
    }

    private void HandleControlMouseDown(Vector3 mousePosition)
    {
        if(m_selectionInformation.m_mouseOverPoint)
        {
            SelectShapeUnderMouse();
            DeletePointUnderMouse();
        }

        else
        {
            CreateNewVolumeShape();
            CreateNewVolumeShapePoint(mousePosition);
        }
    }

    private void HandleLeftMouseDown(Vector3 mousePosition)
    {
        if(m_scTree.m_volume.m_volumeShapes.Count == 0)
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
            Undo.RecordObject(m_scTree.m_volume, "Move Point");
            SelectedShape.m_boundingPoints[m_selectionInformation.m_pointIndex] = mousePosition;
            m_selectionInformation.m_pointSelected = false;
            m_selectionInformation.m_pointIndex = -1;
            m_needsRepaint = true;

            EditorUtility.SetDirty(m_scTree.m_volume);
        }
    }

    private void HandleLeftMouseDrag(Vector3 mousePosition)
    {
        if (m_selectionInformation.m_pointSelected)
        {
            SelectedShape.m_boundingPoints[m_selectionInformation.m_pointIndex] = mousePosition;
            m_needsRepaint = true;

            EditorUtility.SetDirty(m_scTree.m_volume);
        }
    }

    private void UpdateMouseOverInformation(Vector3 mousePosition)
    {
        // Check to see if the mouse is above any shape point.
        int mouseOverPointIndex = -1;
        int mouseOverShapeIndex = -1;
        for (int i = 0; i < m_scTree.m_volume.m_volumeShapes.Count; i++)
        {
            SCVolumeShape curShape = m_scTree.m_volume.m_volumeShapes[i];
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
            for (int i = 0; i < m_scTree.m_volume.m_volumeShapes.Count; i++)
            {
                SCVolumeShape curShape = m_scTree.m_volume.m_volumeShapes[i];
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
        Undo.RecordObject(m_scTree.m_volume, "Create new VolumeShape");
        m_scTree.m_volume.m_volumeShapes.Add(new SCVolumeShape());
        m_selectionInformation.m_shapeIndex = m_scTree.m_volume.m_volumeShapes.Count - 1;

        EditorUtility.SetDirty(m_scTree.m_volume);
    }

    private void CreateNewVolumeShapePoint(Vector3 position)
    {
        bool mouseIsOverSelectedShape = m_selectionInformation.m_mouseOverShapeIndex == m_selectionInformation.m_shapeIndex;
        int newPointIndex = m_selectionInformation.m_mouseOverLine && mouseIsOverSelectedShape ? m_selectionInformation.m_lineIndex + 1 : SelectedShape.m_boundingPoints.Count;
        Undo.RecordObject(m_scTree.m_volume, "Add New Point");
        SelectedShape.m_boundingPoints.Insert(newPointIndex, position);
        m_selectionInformation.m_pointIndex = newPointIndex;
        m_selectionInformation.m_mouseOverShapeIndex = m_selectionInformation.m_shapeIndex;
        m_needsRepaint = true;

        SelectPointUnderMouse();

        EditorUtility.SetDirty(m_scTree.m_volume);
    }

    private void DeletePointUnderMouse()
    {
        if(m_selectionInformation.m_shapeIndex == 0 && SelectedShape.m_boundingPoints.Count == 1)
        {
            return;
        }
        Undo.RecordObject(m_scTree.m_volume, "Delete Point");
        SelectedShape.m_boundingPoints.RemoveAt(m_selectionInformation.m_pointIndex);
        m_selectionInformation.m_pointSelected = false;
        m_selectionInformation.m_mouseOverPoint = false;
        m_needsRepaint = true;

        if(SelectedShape.m_boundingPoints.Count == 0)
        {
            m_scTree.m_volume.m_volumeShapes.RemoveAt(m_selectionInformation.m_shapeIndex);
            m_selectionInformation.m_shapeIndex = Mathf.Clamp(m_selectionInformation.m_shapeIndex, 0, m_scTree.m_volume.m_volumeShapes.Count - 1);
        }

        EditorUtility.SetDirty(m_scTree.m_volume);
    }

    #endregion


    #region Rendering

    private void DrawBounds()
    {
        Color trunkColour = new Color(0.4f, 0.25f, 0.13f);
        Color leafColour = new Color(0.1f, 0.9f, 0.1f);
        for (int i = 0; i < m_scTree.m_volume.m_volumeShapes.Count; i++)
        {
            SCVolumeShape volumeShape = m_scTree.m_volume.m_volumeShapes[i];
            bool shapeIsSelected = i == m_selectionInformation.m_shapeIndex;
            bool mouseIsOverShape = i == m_selectionInformation.m_mouseOverShapeIndex;
            Color deselectedShapeColour = Color.gray;

            for (int j = 0; j < volumeShape.m_boundingPoints.Count; j++)
            {
                Vector3 curPoint = volumeShape.m_boundingPoints[j];
                Vector3 nextPoint = volumeShape.m_boundingPoints[(j + 1) % volumeShape.m_boundingPoints.Count];

                if (j == m_selectionInformation.m_lineIndex && mouseIsOverShape && (i > 0 || j != volumeShape.m_boundingPoints.Count - 1))
                {
                    Handles.color = Color.cyan;
                    Handles.DrawLine(curPoint, nextPoint);
                }
                else
                {
                    if (!shapeIsSelected)
                    {
                        Handles.color = deselectedShapeColour;
                    }
                    else
                    {
                        Handles.color = i > 0 ? leafColour : trunkColour;
                    }
                    if (i > 0 || j != volumeShape.m_boundingPoints.Count - 1)
                    {
                        Handles.DrawDottedLine(curPoint, nextPoint, 4);
                    }
                }


                if (j == m_selectionInformation.m_pointIndex && mouseIsOverShape)
                {
                    Handles.color = m_selectionInformation.m_pointSelected ? Color.blue : Color.cyan;
                }
                else
                {
                    if (!shapeIsSelected)
                    {
                        Handles.color = deselectedShapeColour;
                    }
                    else
                    {
                        Handles.color = i > 0 ? leafColour : trunkColour;
                    }
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
        string saveDirectory = EditorUtility.SaveFilePanel("Save New SCVolume Asset", AssetDatabase.GetAssetPath(Selection.activeObject), "New SCVolume", "asset");
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
        m_scTree.m_volume.m_volumeShapes.Add(new SCVolumeShape());
        m_scTree.m_volume.m_volumeShapes[0].m_boundingPoints.Add(m_scTree.transform.position);
        m_scTree.m_volume.m_volumeShapes[0].m_boundingPoints.Add(m_scTree.transform.position + Vector3.up * 5.0f);
        EditorUtility.SetDirty(m_scTree.m_volume);

        if (!m_scTree.m_volume.m_isEditable)
        {
            EnableEditor();
        }
    }

    private void EnableEditor()
    {
        m_scTree.m_volume.m_isEditable = true;
        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target), typeof(GameObject)));
        Tools.hidden = true;
    }

    private void CloseEditor()
    {
        StageUtility.GoToMainStage();
    }

    private void DisableEditor(PrefabStage closingPrefabStage)
    {
        m_scTree.m_volume.m_isEditable = false;
        m_scTree.m_volume.m_showVolumeShapeList.value = false;
        Tools.hidden = false;
        EditorUtility.SetDirty(m_scTree.m_volume);
    }

    private void DestroyTreeChildren()
    {
        Transform targetTransform = (target as SCTree).transform;
        for (int i = targetTransform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(targetTransform.GetChild(i).gameObject);
        }
    }
}
