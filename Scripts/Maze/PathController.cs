using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(beMobileMaze))]
public class PathController : MonoBehaviour {

    [SerializeField]
    public List<PathInMaze> Paths = new List<PathInMaze>();

    public PathInMaze RequirePath(string targetPath)
    {
        return Paths.SingleOrDefault((p) => p.PathName.Equals(targetPath));
    }
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
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
}
