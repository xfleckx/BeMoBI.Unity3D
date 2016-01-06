using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace Assets.SNEED.Unity3D.Editor.Maze
{
    public class MazeBaker
    {
        internal void Bake(beMobileMaze maze)
        {
            Debug.Assert(maze.GetComponent<MeshFilter>() == null, "Component has already a MeshFilter");
             
            var meshFilter = maze.gameObject.AddComponent<MeshFilter>();

            Debug.Assert(maze.GetComponent<MeshRenderer>() == null, "Component has already a MeshRenderer");

            var meshRenderer = maze.gameObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = new Mesh();
            meshFilter.sharedMesh.name = maze.name;

            var combineInstances = new List<CombineInstance>();

            foreach (var unit in maze.Units)
            {
                var allMeshFilter = unit.GetComponentsInChildren<MeshFilter>();
                
                var selectedMeshFilter = allMeshFilter.Where(
                    mf => mf.name.Equals("North") ||
                     mf.name.Equals("West") ||
                      mf.name.Equals("East") ||
                       mf.name.Equals("South") ||
                        mf.name.Equals("Top") ||
                         mf.name.Equals("Floor")
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

            meshFilter.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        }
    }
}
