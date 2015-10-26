using System;
using System.Collections.Generic;
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

        #region Helper

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
