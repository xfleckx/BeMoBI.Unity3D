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
        }

        public override Rect OnGUI()
        {
            var rect = EditorGUILayout.BeginVertical();

                dimension = EditorGUILayout.Vector2Field("Spot Door dimension (width x height)", dimension);
                pivotOrigin = EditorGUILayout.Vector3Field("Origin", pivotOrigin);

                if (GUILayout.Button("Create Hiding Spot"))
                {
                    var newUnit = new GameObject("HidingSpot");

                    //var creationModel = new HidingSpotModificationModel(this.doorMovingDirection, );

                    newUnit.AddComponent<MazeUnit>();

                    var boxCollider = newUnit.AddComponent<BoxCollider>();

                    boxCollider.center = new Vector3(0, Dimension.y / 2, 0);

                    boxCollider.size = Dimension;

                    //var prototype = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    //prototype.hideFlags = HideFlags.HideAndDontSave;
                    //var prototypeMeshRenderer = prototype.GetComponent<MeshRenderer>();
                    //var material = prototypeMeshRenderer.sharedMaterial;



                    //DestroyImmediate(prototype);
                }

                EditorGUILayout.Space();

                prefabName = EditorGUILayout.TextField("Prefab Name:", prefabName);

                if (constructedUnit != null && prefabReference == null && GUILayout.Button("Save as Prefab"))
                {
                    var targetPath = EditorEnvironmentConstants.Get_PREFAB_DIR_PATH();

                    Debug.Assert(AssetDatabase.IsValidFolder(targetPath), string.Format("Expected prefab folder at \"{0}\"", targetPath));

                    var targetFilePath = targetPath + Path.AltDirectorySeparatorChar +
                        string.Format("{0}{1}", prefabName + dimension.AsPartFileName(), EditorEnvironmentConstants.PREFAB_EXTENSION);

                    PrefabUtility.CreatePrefab(targetFilePath, constructedUnit);
                }

            EditorGUILayout.EndVertical();

            return rect;
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
