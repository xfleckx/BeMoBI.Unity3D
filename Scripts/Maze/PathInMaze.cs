using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class PathInMaze : ScriptableObject {

    [SerializeField]
    public LinkedList<MazeUnit> Units;
      
    public void OnEnable()
    {
        hideFlags = HideFlags.HideAndDontSave;

        Debug.Log(string.Format("Path {0} Enabled", name));

        if (Units == null) { 
            Units = new LinkedList<MazeUnit>();
            Debug.Log("Creating empty Unit List in Path");
        }
    } 


}
