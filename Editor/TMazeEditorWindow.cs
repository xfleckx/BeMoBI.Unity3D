using UnityEngine;
using UnityEditor;

public class TMazeEditorWindow : EditorWindow
{
    [MenuItem("beMobile/MazeDesigner/Add new T-Maze")]
    static void CreateNewMazeInScene()
    {
        var window = EditorWindow.GetWindow<TMazeEditorWindow>();
        window.Init();
        window.Show();
    }

    private static TMazeEditorWindow _instance = null;
    private Camera _editorCamera = null;

    private void Init()
    {
        _instance = GetWindow<TMazeEditorWindow>("Custom Scene Editor");
        // These next two parameters are optional
        _instance.wantsMouseMove = true;
        _instance.autoRepaintOnSceneChange = true;
 
        GameObject cameraObject = new GameObject("_Custom Editor Camera", typeof(Camera));
        cameraObject.GetComponent<Camera>().orthographic = true;
        cameraObject.GetComponent<Camera>().orthographicSize = 14;
        cameraObject.GetComponent<Camera>().farClipPlane = 100f;
        cameraObject.GetComponent<Camera>().nearClipPlane = 0.3f;
        cameraObject.GetComponent<Camera>().depth = -1;
        cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.Color;
        cameraObject.GetComponent<Camera>().backgroundColor = Color.black;
        cameraObject.hideFlags = HideFlags.HideAndDontSave;
        _instance._editorCamera = cameraObject.GetComponent<Camera>();
    
    }

    private Vector2 UnitDimension;

    void OnEnable()
    {
        this.title = "T-Maze Editor";
    }

    void OnGUI()
    {
        
        if (_editorCamera != null)
        {
            // NOTE: This is not a perfect rectangle for the window.  Adjust the size to get the desired unit
            Rect cameraRect = new Rect(0f, 0f, position.width, position.height);
            Handles.DrawCamera(cameraRect, _editorCamera, DrawCameraMode.TexturedWire);
        }

    }


    void OnDestroy()
    {
        // Destroy the camera when the editor is closed
        DestroyImmediate(_editorCamera.gameObject);
    }
 
}