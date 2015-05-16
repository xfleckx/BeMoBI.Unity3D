using UnityEngine;
using UnityEditor;

public class MazeDesignerMenuItems : ScriptableObject
{
    [MenuItem("beMobile/MazeDesigner/Add new Maze")]
    static void CreateNewMazeInScene()
    {
        GameObject mazeHost = new GameObject("beMobilMaze");
        var maze = mazeHost.AddComponent<beMobileMaze>();
        var mazeModel = ScriptableObject.CreateInstance<MazeModel>();
    }

    [InitializeOnLoad]
    public class InitializeNecessaryResources
    {
        static InitializeNecessaryResources()
        {
            //Debug.Log("Copying Gizmos");
            //TODO copying gizmos in Gizmos folder
        }
    }

}