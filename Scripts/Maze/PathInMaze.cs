using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class PathInMaze : ScriptableObject {

    [SerializeField]
    public LinkedList<MazeUnit> Units;
    [SerializeField]
    public string PathName = string.Empty;

    public void OnEnable()
    {
        Debug.Log("Path Enabled");

        if (Units == null) { 
            Units = new LinkedList<MazeUnit>();
            Debug.Log("Creating empty Unit List in Path");
        }
    } 


}
