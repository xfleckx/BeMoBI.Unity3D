using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathInMaze : ScriptableObject {

    [SerializeField]
    public LinkedList<Vector2> GridIDs;
    [SerializeField]
    public string PathName = string.Empty;

    public void Setup(LinkedList<MazeUnit> pathInSelection)
    {
        GridIDs = new LinkedList<Vector2>();

        foreach (var item in pathInSelection)
        {
            GridIDs.AddLast(item.GridID);
        }

    }
}
