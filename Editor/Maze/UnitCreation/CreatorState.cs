using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.BeMoBI.Unity3D.Editor.Maze.UnitCreation
{
    public abstract class CreatorState : ScriptableObject
    {
        protected Vector3 dimension = Vector3.one;

        public Vector3 Dimension
        {
            get { return dimension; }
            set { dimension = value; }
        }

        protected Vector3 pivotOrigin = Vector3.zero;
        public Vector3 PivotOrigin
        {
            get { return pivotOrigin; }
            set { pivotOrigin = value; }
        }

        protected string prefabName = String.Empty;
        public string PrefabName
        {
            get { return prefabName; }
            set { prefabName = value; }
        }

        protected GameObject constructedUnit;
        public GameObject ConstructedUnit
        {
            get { return constructedUnit; }
            set { constructedUnit = value; }
        }

        protected GameObject prefabReference;
        public GameObject PrefabReference
        {
            get { return prefabReference; }
            set { prefabReference = value; }
        }

        public abstract void Initialize();

        public abstract Rect OnGUI();

        protected void Render_SaveAsPrefab_Option()
        {
            prefabName = EditorGUILayout.TextField("Prefab Name:", prefabName);

            if (constructedUnit != null && prefabReference == null && GUILayout.Button("Save as Prefab"))
            {
                var targetPath = EditorEnvironmentConstants.Get_PREFAB_DIR_PATH();

                Debug.Assert(AssetDatabase.IsValidFolder(targetPath), string.Format("Expected prefab folder at \"{0}\"", targetPath));

                var targetFilePath = targetPath + Path.AltDirectorySeparatorChar +
                    string.Format("{0}{1}", prefabName + dimension.AsPartFileName(), EditorEnvironmentConstants.PREFAB_EXTENSION);

                PrefabUtility.CreatePrefab(targetFilePath, constructedUnit);
            }

        }


        #region Helper

        protected Material GetPrototypeMaterial()
        {
            var prototype = GameObject.CreatePrimitive(PrimitiveType.Plane);
            prototype.hideFlags = HideFlags.HideAndDontSave;
            var prototypeMeshRenderer = prototype.GetComponent<MeshRenderer>();
            var material = prototypeMeshRenderer.sharedMaterial;

            return material;
        }

        protected Vector3 V(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }
        protected Vector2 V(float x, float y)
        {
            return new Vector2(x, y);
        }

        #endregion

    }
}
