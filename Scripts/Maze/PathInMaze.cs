using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class PathInMaze : MonoBehaviour {
    
    public List<MazeUnit> Units;

    [SerializeField]
    public List<Vector2> GridIDs;

    void Awake()
    {
        if (Units == null)
        {
            Units = new List<MazeUnit>();
            Debug.Log("Creating empty Unit List in Path");
        }
    }

    public void OnEnable()
    {
       // hideFlags = HideFlags.HideInInspector |HideFlags.HideInHierarchy;

        Debug.Log(string.Format("Path {0} Enabled", name));


        if (GridIDs == null)
        {
            GridIDs = new List<Vector2>();
        }
    }


#if UNITY_EDITOR
    public Action EditorGizmoCallbacks;
#endif

    public void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (EditorGizmoCallbacks != null)
            EditorGizmoCallbacks();
#endif
    }

    public void OnDestroy()
    {
        Debug.Log(string.Format("{0} destroyed!", name));
    }
}
