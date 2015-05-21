using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(PathController))]
public class PathInMaze : MonoBehaviour {
    
    public List<MazeUnit> Units;

    [SerializeField]
    public List<Vector2> GridIDs;

    public int ID = -1;

    void Awake()
    {
        if (Units == null)
        {
            Units = new List<MazeUnit>();
            Debug.Log("Creating empty Unit List in Path");
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
