using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(beMobileMaze))]
public class MazeEditor : Editor
{
    private beMobileMaze focusedMaze;

    public void OnEnable()
    {
        focusedMaze = (beMobileMaze)target;
    }

    protected override void OnHeaderGUI()
    {
        //base.OnHeaderGUI();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Length of Maze ");
        focusedMaze.MazeLengthInMeter = EditorGUILayout.FloatField(focusedMaze.MazeLengthInMeter, GUILayout.Width(50));
        GUILayout.Label("m");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Width of Maze ");
        focusedMaze.MazeWidthInMeter = EditorGUILayout.FloatField(focusedMaze.MazeWidthInMeter, GUILayout.Width(50));
        GUILayout.Label("m");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(" Heigth of Rooms ");
        focusedMaze.RoomHigthInMeter = EditorGUILayout.FloatField(focusedMaze.RoomHigthInMeter, GUILayout.Width(50));
        GUILayout.Label("m");
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        if (GUILayout.Button("Open Maze Editor", GUILayout.Width(255)))
        {
            MazeEditorWindow window = (MazeEditorWindow)EditorWindow.GetWindow(typeof(MazeEditorWindow));
            window.Init();
        }

        if (GUILayout.Button("Clone Maze", GUILayout.Width(255)))
        {
            GameObject.Instantiate(focusedMaze);
        }
         
        GUILayout.EndVertical();
    }


}