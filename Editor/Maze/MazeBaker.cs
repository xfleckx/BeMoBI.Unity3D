using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace Assets.SNEED.Unity3D.Editor.Maze
{
    public class MazeBaker
    {
        public bool replaceOriginalMaze = false;

        public bool ignoreFloor = false;

        private const bool MERGE_SUBMESHES = true;
        private const bool USE_MATRICES = true;

        public beMobileMaze Bake(beMobileMaze originalMaze)
        {
            beMobileMaze mazeToUse = null;

            if (!replaceOriginalMaze)
            {
                mazeToUse = GameObject.Instantiate(originalMaze);
                originalMaze.gameObject.SetActive(false);
            }

            Debug.Assert(mazeToUse.GetComponent<MeshFilter>() == null, "Component has already a MeshFilter");
             
            var meshFilter = mazeToUse.gameObject.AddComponent<MeshFilter>();

            Debug.Assert(mazeToUse.GetComponent<MeshRenderer>() == null, "Component has already a MeshRenderer");
            
            meshFilter.mesh = new Mesh();
            meshFilter.sharedMesh.name = mazeToUse.name;

            var meshRenderer = mazeToUse.gameObject.AddComponent<MeshRenderer>();

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

            meshRenderer.material = plane.GetComponent<MeshRenderer>().sharedMaterial;

            GameObject.DestroyImmediate(plane);

            var combineInstances = new List<CombineInstance>();

            foreach (var unit in mazeToUse.Units)
            {
                var allMeshFilter = unit.GetComponentsInChildren<MeshFilter>();
                
                var selectedMeshFilter = allMeshFilter.Where(
                    mf => mf.name.Equals("North") ||
                     mf.name.Equals("West") ||
                      mf.name.Equals("East") ||
                       mf.name.Equals("South") ||
                        mf.name.Equals("Top") ||
                         (mf.name.Equals("Floor") && !ignoreFloor)
                    );
                
                foreach (var filter in selectedMeshFilter)
                {
                    var combined = new CombineInstance();
                    combined.mesh = filter.sharedMesh;
                    combined.transform = filter.transform.localToWorldMatrix;
                    combineInstances.Add(combined);

                    Component.DestroyImmediate(filter);

                }

                var allRenderer = unit.GetComponentsInChildren<MeshRenderer>();

                var selectedRenderer = allRenderer.Where(
                    mf => mf.name.Equals("North") ||
                     mf.name.Equals("West") ||
                      mf.name.Equals("East") ||
                       mf.name.Equals("South") ||
                        mf.name.Equals("Top") ||
                         mf.name.Equals("Floor")
                    );

                foreach (var renderer in selectedRenderer)
                {
                    Component.DestroyImmediate(renderer);
                }

            }

            meshFilter.sharedMesh.CombineMeshes(combineInstances.ToArray(), MERGE_SUBMESHES, USE_MATRICES);

            AssetDatabase.CreateAsset(meshFilter.sharedMesh, EditorEnvironmentConstants.Get_PACKAGE_MODEL_SUBFOLDER() + "/" + mazeToUse.name + "_combinedMesh.asset");

            return mazeToUse;
        }
    }
}
