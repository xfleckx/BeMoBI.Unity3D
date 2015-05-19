using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class PathInMaze : ScriptableObject {

    [SerializeField]
    public List<MazeUnit> Units;
      
    public void OnEnable()
    {
       // hideFlags = HideFlags.HideInInspector |HideFlags.HideInHierarchy;

        Debug.Log(string.Format("Path {0} Enabled", name));

        if (Units == null) { 
            Units = new List<MazeUnit>();
            Debug.Log("Creating empty Unit List in Path");
        }
    }

    public void OnDestroy()
    {
        Debug.Log(string.Format("{0} destroyed!", name));
    }
}
