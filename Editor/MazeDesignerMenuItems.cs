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

    [MenuItem("beMobile/MazeDesigner/Add new Maze as Prefab")]
    static void CreateNewMazeAsPrefab()
    {
        var filePath = EditorUtility.SaveFilePanelInProject("Save the prefab", "aBeMoBIMaze", "prefab","Message");
        var prefab = PrefabUtility.CreateEmptyPrefab(filePath);


        GameObject mazeHost = new GameObject("beMoBIMaze");
        PrefabUtility.ReplacePrefab(mazeHost, prefab, ReplacePrefabOptions.ConnectToPrefab);
        var maze = mazeHost.AddComponent<beMobileMaze>(); 
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