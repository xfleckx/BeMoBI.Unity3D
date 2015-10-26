using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.BeMoBI.Unity3D.Editor.Maze.UnitCreation
{
    public class HidingSpotCreator : CreatorState
    {
        public override void Initialize()
        { 
            // for this unit it does nothing...
        }

        public override Rect OnGUI()
        {
            var rect = EditorGUILayout.BeginVertical();

                dimension = EditorGUILayout.Vector2Field("Spot Door dimension (width x height)", dimension);
                pivotOrigin = EditorGUILayout.Vector3Field("Origin", pivotOrigin);

                if (GUILayout.Button("Create Hiding Spot"))
                {
                    Construct();
                }

                EditorGUILayout.Space();

                Render_SaveAsPrefab_Option();

            EditorGUILayout.EndVertical();

            return rect;
        }


        private void Construct()
        {
            var newUnit = new GameObject("HidingSpot");

            //var creationModel = new HidingSpotModificationModel(this.doorMovingDirection, );

            newUnit.AddComponent<MazeUnit>();

            var boxCollider = newUnit.AddComponent<BoxCollider>();

            boxCollider.center = new Vector3(0, Dimension.y / 2, 0);

            boxCollider.size = Dimension;

            var prototypeMaterial = GetPrototypeMaterial();


            DestroyImmediate(prototypeMaterial);
        }

    }


    public class HidingSpotModificationModel : UnitMeshModificationModel
    {
        private HidingSpot.Direction doorMovingDirection = HidingSpot.Direction.Horizontal;
        public HidingSpot.Direction DoorMovingDirection
        {
            get { return doorMovingDirection; }
        }
        public HidingSpotModificationModel(HidingSpot.Direction doorMoveDirection, Vector3 spotDimension, Vector3 origin, bool useCenterAsOrigin)
            : base(spotDimension, origin, useCenterAsOrigin)
        {
            this.doorMovingDirection = doorMoveDirection;
        }

    }


}
