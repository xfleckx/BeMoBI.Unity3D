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
        maze.Grid = MazeEditorUtil.ReconfigureGrid(maze, maze.MazeLengthInMeter, maze.MazeWidthInMeter);
    }

    [MenuItem("beMobile/MazeDesigner/Open Editor on new Maze")]
    static void OpenEditorWithNewMazeInScene()
    {
        var window = EditorWindow.GetWindow<MazeEditorWindow>();
        window.Init();
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