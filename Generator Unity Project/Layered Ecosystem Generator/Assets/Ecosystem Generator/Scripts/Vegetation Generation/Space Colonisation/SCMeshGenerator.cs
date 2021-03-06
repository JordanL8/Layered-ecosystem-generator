﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SCMeshGenerator
{
    private class SCMeshGeneratorBranchList
    {
        public List<SCBranch> branches = new List<SCBranch>();
        public int m_recursionNumber;
    }

    public static void BuildTree(SCBranch root, Transform parent, GameObject leafPrefab, int lodLevel, int overrideSegments = -1)
    {
        BuildBranches(root, 0, leafPrefab, lodLevel, overrideSegments, parent);
    }

    private static void BuildBranches(SCBranch branchStart, int recursionNumber, GameObject leafPrefab, int lodLevel, int overrideSegments = -1, Transform parent = null)
    {
        SCMeshGeneratorBranchList branchList = new SCMeshGeneratorBranchList();
        if(branchStart.m_parent != null)
        {
            branchList.branches.Add(branchStart.m_parent);
        }
        branchList.branches.Add(branchStart);
        branchList.m_recursionNumber = recursionNumber;

        SCBranch curBranch = branchStart;
        while (curBranch != null)
        {
            // Implementation 1. UV bug
            //if(curBranch.ChildCount == 0)
            //{
            //    break;
            //}
            //
            //if (curBranch.ChildCount == 1)
            //{
            //    SCBranch firstChildBranch = curBranch.GetChild(0);
            //    branchList.branches.Add(firstChildBranch);
            //    curBranch = firstChildBranch;
            //}
            //else
            //{
            //    for (int i = 0; i < curBranch.ChildCount; i++)
            //    {
            //        BuildBranches(curBranch.GetChild(i), recursionNumber + 1, leafPrefab, parent);
            //    }
            //    break;
            //}

            // Implementation 2. Curve bug
            int nextBranch = 0;
            float closestDirection = -1;
            for (int i = 0; i < curBranch.ChildCount; i++)
            {
                float dot = Vector3.Dot(curBranch.Direction, curBranch.GetChild(i).Direction);
                if (dot > closestDirection)
                {
                    nextBranch = i;
                    closestDirection = dot;
                }
            }
            for (int i = 0; i < curBranch.ChildCount; i++)
            {
                if (i == nextBranch)
                {
                    SCBranch firstChildBranch = curBranch.GetChild(i);
                    branchList.branches.Add(firstChildBranch);
                }
                else
                {
                    BuildBranches(curBranch.GetChild(i), recursionNumber + 1, leafPrefab, lodLevel, overrideSegments, parent);
                }
            }
            curBranch = curBranch.GetChild(nextBranch);
            //
        }

        // Build


        MeshBuilder meshBuilder = new MeshBuilder();
        Quaternion rotation = Quaternion.identity;
        int segments = overrideSegments;
        if (segments < 3)
        {
            switch (lodLevel)
            {
                case 0:
                {
                    segments = recursionNumber < 2 ? 6 : 4;
                }
                break;
                case 1:
                {
                    segments = recursionNumber == 0 ? 4 : 3;
                }
                break;
                default:
                {
                    segments = 3;
                }
                break;
            }
        }

        float height = 0.0f;
        for (int j = 0; j < branchList.branches.Count - 1; j++)
        {
            height += Vector3.Magnitude(branchList.branches[j].Position - branchList.branches[j + 1].Position);
        }
        float circumference = (branchList.branches[0].m_thickness + branchList.branches[branchList.branches.Count-1].m_thickness) / 2 * 2 * Mathf.PI;
        float ratio = height / circumference;

        for (int j = 0; j < branchList.branches.Count; j++)
        {
            SCBranch currentBranch = branchList.branches[j];
            if (j + 1 < branchList.branches.Count)
            {
                if (rotation == Quaternion.identity)
                {
                    rotation = Quaternion.LookRotation(branchList.branches[j + 1].Position - currentBranch.Position);
                }
                else
                {
                    Quaternion lastRotation = rotation;
                    Quaternion nextRotation = j + 2 < branchList.branches.Count ? Quaternion.LookRotation(branchList.branches[j + 2].Position - currentBranch.Position) : Quaternion.LookRotation(branchList.branches[j + 1].Position - currentBranch.Position);
                    rotation = Quaternion.Lerp(lastRotation, nextRotation, 0.5f);
                }
            }
            float v = (float)j / branchList.branches.Count * ratio;
            meshBuilder.BuildRing(currentBranch.Position, rotation, segments, currentBranch.m_thickness, v, j > 0);

            if(j > 0 && recursionNumber > 0)
            {
                //CreateLeaf(leafPrefab,currentPosition, rotation, currentRadius, 0.5f);
            }

            if (j == branchList.branches.Count - 1 && lodLevel < 1)
            {
                meshBuilder.BuildCap(currentBranch.Position, rotation, segments, currentBranch.m_thickness);
            }
        }
        Mesh segmentMesh = meshBuilder.CreateMesh();
        GameObject obj = new GameObject();
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<MeshFilter>().sharedMesh = segmentMesh;
        obj.transform.parent = parent;



    }

    private static void CreateLeaf(GameObject leafPrefab, Vector3 position, Quaternion rotation, float radius, float chance = 1.0f)
    {
        if (Random.value <= chance)
        {
            GameObject newLeaf = GameObject.Instantiate(leafPrefab);
            //newLeaf.transform.localScale = Vector3.one * Vector3.Distance(leaf.Position, branch.Position);
            
            Vector3 eularRotation = rotation.eulerAngles;
            //eularRotation = new Vector3(eularRotation.z, 0, -eularRotation.x);
            newLeaf.transform.rotation = rotation;// Quaternion.LookRotation(eularRotation - position);

            newLeaf.transform.position = position + rotation.eulerAngles.normalized * (radius + newLeaf.transform.localScale.x/2.0f);
        }
    }
}
