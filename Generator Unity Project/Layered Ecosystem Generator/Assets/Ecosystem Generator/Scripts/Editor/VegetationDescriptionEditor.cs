using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(VegetationDescription))]
public class VegetationDescriptionEditor : Editor
{
    private VegetationDescription m_targetDescription;

    private void Awake()
    {
        m_targetDescription = (VegetationDescription)target;
        //ResizeRelevantArrays();
    }
    private void OnEnable()
    {
        m_targetDescription = (VegetationDescription)target;
        //ResizeRelevantArrays();
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        RenderPlacementProperties();
        RenderVegetationProperties();
        if(EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(m_targetDescription);
        }
        //RenderGeneralProperties();
        //RenderRecursiveProperties();
        //RenderLeafProperties();
        //RenderPruneProperties();        
    }

    private void RenderPlacementProperties()
    {
        EditorGUILayout.LabelField("Placement Model", EditorStyles.boldLabel);
        m_targetDescription.m_innerRadius = Mathf.Max(EditorGUILayout.FloatField("Inner Radius", m_targetDescription.m_innerRadius), 0.05f);
        m_targetDescription.m_outerRadius = Mathf.Max(EditorGUILayout.FloatField("Outer Radius", m_targetDescription.m_outerRadius), 0.05f);
    }

    private void RenderVegetationProperties()
    {
        EditorGUILayout.LabelField("Vegetation", EditorStyles.boldLabel);
        m_targetDescription.m_vegationType = (VegetationType)EditorGUILayout.EnumPopup("Vegetation Type", m_targetDescription.m_vegationType);
        if(m_targetDescription.m_vegationType == VegetationType.LSystem)
        {
            RenderLSystemProperties();
        }
        else
        {
            RenderSpaceColonisationProperties();
        }
    }

    private void RenderLSystemProperties()
    {
        EditorGUILayout.LabelField("L-System", EditorStyles.boldLabel);
        m_targetDescription.m_lSystemRulesAsset = EditorGUILayout.ObjectField("Rules Asset", m_targetDescription.m_lSystemRulesAsset, typeof(LSystemGenerationRuleAsset), false) as LSystemGenerationRuleAsset;
        m_targetDescription.m_branchMaterial = EditorGUILayout.ObjectField("Branch Material", m_targetDescription.m_branchMaterial, typeof(Material), false) as Material;
        m_targetDescription.m_leafPrefab = EditorGUILayout.ObjectField("Leaf Prefab", m_targetDescription.m_leafPrefab, typeof(GameObject), false) as GameObject;
        m_targetDescription.m_leafMaterial = EditorGUILayout.ObjectField("Leaf Material", m_targetDescription.m_leafMaterial, typeof(Material), false) as Material;
    }

    private void RenderSpaceColonisationProperties()
    {
        EditorGUILayout.LabelField("Space Colonisation", EditorStyles.boldLabel);
        m_targetDescription.m_spaceColonisationTreePrefab = EditorGUILayout.ObjectField("Tree Prefab", m_targetDescription.m_spaceColonisationTreePrefab, typeof(GameObject), false) as GameObject;
    }

    //private void RenderGeneralProperties()
    //{
    //    GUILayout.Space(20.0f);
    //    EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
    //    m_targetDescription.m_shape = EditorGUILayout.IntField("Shape", m_targetDescription.m_shape);
    //    m_targetDescription.m_0Scale = EditorGUILayout.IntField("Base Scale", m_targetDescription.m_0Scale);
    //    m_targetDescription.m_0ScaleV = EditorGUILayout.IntField("Base Scale V", m_targetDescription.m_0ScaleV);
    //    EditorGUI.BeginChangeCheck();
    //    m_targetDescription.m_levels = EditorGUILayout.IntSlider("Levels", m_targetDescription.m_levels, 0, 3);
    //    if(EditorGUI.EndChangeCheck())
    //    {
    //        ResizeRelevantArrays();
    //    }
    //    m_targetDescription.m_ratio = EditorGUILayout.FloatField("Ratio", m_targetDescription.m_ratio);
    //    m_targetDescription.m_ratioPower = EditorGUILayout.FloatField("Ratio Power", m_targetDescription.m_ratioPower);
    //    m_targetDescription.m_flare = EditorGUILayout.FloatField("Flare", m_targetDescription.m_flare);
    //    m_targetDescription.m_baseSplits = EditorGUILayout.IntField("Base Splits", m_targetDescription.m_baseSplits);
    //}

    //private void RenderRecursiveProperties()
    //{
    //    //GUILayout.Space(20.0f);
    //    //EditorGUILayout.LabelField("Branch", EditorStyles.boldLabel);
    //    //GUILayout.BeginHorizontal();
    //    //GUILayout.BeginVertical();
    //    //GUILayout.Label("Property", EditorStyles.boldLabel, GUILayout.Height(20.0f));
    //    //GUILayout.Label("Base Size", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Down Angle", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Down Angle V", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Rotate", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Rotate V", GUILayout.Height(20.0f));

    //    //GUILayout.Label("Branches", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Length", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Length V", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Taper", GUILayout.Height(20.0f));

    //    //GUILayout.Label("Segment Splits", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Split Angle", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Split Angle V", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Curve Resolution", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Curve", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Curve V", GUILayout.Height(20.0f));
    //    //GUILayout.Label("Curve Back", GUILayout.Height(20.0f));
    //    //GUILayout.EndVertical();
    //    //for (int i = 0; i < m_targetDescription.m_levels + 1; i++)
    //    //{
    //    //    GUILayout.BeginVertical();
    //    //    string levelHeading = i == 0 ? "Base" : i.ToString();
    //    //    GUILayout.Label(levelHeading, EditorStyles.boldLabel, GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_baseSize[i] = EditorGUILayout.FloatField(m_targetDescription.m_baseSize[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_downAngle[i] = EditorGUILayout.IntField(m_targetDescription.m_downAngle[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_downAngleV[i] = EditorGUILayout.IntField(m_targetDescription.m_downAngleV[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_rotate[i] = EditorGUILayout.IntField(m_targetDescription.m_rotate[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_rotateV[i] = EditorGUILayout.IntField(m_targetDescription.m_rotateV[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_branches[i] = EditorGUILayout.IntField(m_targetDescription.m_branches[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_length[i] = EditorGUILayout.FloatField(m_targetDescription.m_length[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_lengthV[i] = EditorGUILayout.FloatField(m_targetDescription.m_lengthV[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_taper[i] = EditorGUILayout.IntField(m_targetDescription.m_taper[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_segSplits[i] = EditorGUILayout.IntField(m_targetDescription.m_segSplits[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_splitAngle[i] = EditorGUILayout.IntField(m_targetDescription.m_splitAngle[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_splitAngleV[i] = EditorGUILayout.IntField(m_targetDescription.m_splitAngleV[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_curveRes[i] = EditorGUILayout.IntField(m_targetDescription.m_curveRes[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_curve[i] = EditorGUILayout.IntField(m_targetDescription.m_curve[i], GUILayout.Height(20.0f)); ;
    //    //    m_targetDescription.m_curveV[i] = EditorGUILayout.IntField(m_targetDescription.m_curveV[i], GUILayout.Height(20.0f));
    //    //    m_targetDescription.m_curveBack[i] = EditorGUILayout.IntField(m_targetDescription.m_curveBack[i], GUILayout.Height(20.0f));
    //    //    GUILayout.EndVertical();
    //    //}
    //    //GUILayout.EndHorizontal();

    //}

    //private void RenderLeafProperties()
    //{
    //    GUILayout.Space(20.0f);
    //    EditorGUILayout.LabelField("Leaf", EditorStyles.boldLabel);
    //    m_targetDescription.m_leafNumber = EditorGUILayout.IntField("Number", m_targetDescription.m_leafNumber);
    //    m_targetDescription.m_leafShape = EditorGUILayout.IntField("Shape", m_targetDescription.m_leafShape);
    //    m_targetDescription.m_leafScale = EditorGUILayout.FloatField("Scale", m_targetDescription.m_leafScale);
    //    m_targetDescription.m_leafScaleX = EditorGUILayout.FloatField("Scale X", m_targetDescription.m_leafScaleX);
    //    m_targetDescription.m_attractionUp = EditorGUILayout.FloatField("Attraction Up", m_targetDescription.m_attractionUp);
    //}

    //private void RenderPruneProperties()
    //{
    //    GUILayout.Space(20.0f);
    //    EditorGUILayout.LabelField("Prune", EditorStyles.boldLabel);
    //    m_targetDescription.m_pruneRatio = EditorGUILayout.IntField("Ratio", m_targetDescription.m_pruneRatio);
    //    m_targetDescription.m_pruneWidth = EditorGUILayout.FloatField("Wdith", m_targetDescription.m_pruneWidth);
    //    m_targetDescription.m_pruneWidthPeak = EditorGUILayout.FloatField("Width Peak", m_targetDescription.m_pruneWidthPeak);
    //    m_targetDescription.prunePowerLow = EditorGUILayout.FloatField("Power Low", m_targetDescription.prunePowerLow);
    //    m_targetDescription.prunePowerHigh = EditorGUILayout.FloatField("Power High", m_targetDescription.prunePowerHigh);
    //}

    //private void ResizeRelevantArrays()
    //{
    //    int newSize = m_targetDescription.m_levels + 1;
    //    int oldSize = 0;
    //    bool initialisesArray = true;
    //    if (m_targetDescription.m_baseSize != null)
    //    {
    //        oldSize = m_targetDescription.m_baseSize.Length;
    //        initialisesArray = false;
    //    }
    //    if (newSize == oldSize) { return; }
    //    else
    //    {
    //        ResizeArray(ref m_targetDescription.m_baseSize, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_downAngle, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_downAngleV, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_rotate, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_rotateV, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_branches, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_length, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_lengthV, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_taper, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_segSplits, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_splitAngle, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_splitAngleV, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_curveRes, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_curve, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_curveV, newSize, initialisesArray);
    //        ResizeArray(ref m_targetDescription.m_curveBack, newSize, initialisesArray);
    //    }
    //}


    //private void ResizeArray<T>(ref T[] array, int newSize, bool initialisesArray)
    //{
    //    T[] newArray = new T[newSize];
    //    int smallestSize = initialisesArray ? 0 : newSize < array.Length ? newSize : array.Length;
    //    for (int i = 0; i < smallestSize; i++)
    //    {
    //        newArray[i] = array[i];
    //    }
    //    array = newArray;
    //}
}
