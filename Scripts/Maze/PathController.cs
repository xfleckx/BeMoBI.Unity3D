using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(beMobileMaze))]
public class PathController : MonoBehaviour {

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
