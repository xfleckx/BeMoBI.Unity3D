using UnityEngine;
using UnityEditor;

public class MazeDesigner : ScriptableObject
{
    [MenuItem("beMobile/MazeDesigner/Add new Maze")]
    static void CreateNewMazeInScene()
    {
        GameObject maze = new GameObject("beMobilMaze");
        maze.AddComponent(typeof(beMobileMaze));
    }

    [MenuItem("beMobile/Enable Debug HUD")]
    static void AddDebugHUD() 
    {
        GameObject debugHUD = new GameObject("DebugHUD");
        debugHUD.AddComponent(typeof(DebugHUD));
    }

    // Validate the menu item defined by the function above.
    // The menu item will be disabled if this function returns false.
    [MenuItem("beMobile/Enable Debug HUD", true)]
    static bool ValidateLogSelectedTransformName()
    {
        var existingHUD = GameObject.FindObjectOfType<DebugHUD>(); 
        return existingHUD == null;
    }


}