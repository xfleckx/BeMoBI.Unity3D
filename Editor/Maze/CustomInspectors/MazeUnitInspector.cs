using UnityEngine;
using UnityEditor;
using Assets.SNEED.Mazes;

namespace Assets.SNEED.EditorExtensions.CustomInspectors
{
    [CustomEditor(typeof(MazeUnit))]
    public class MazeUnitInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
        
        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmos(MazeUnit unit, GizmoType gizmoType)
        {
            Vector3 position = unit.transform.position;

            var tempHandleColor = Handles.color;

            Handles.color = Color.green;

            if (Vector3.Distance(position, Camera.current.transform.position) < 10f)
            {
                foreach (var item in unit.transform.AllChildren())
                {
                    var customForward = Vector3.zero;

                    if (item.name == MazeUnit.TOP)
                    {
                        customForward = Vector3.down;
                    }

                    if (item.name == MazeUnit.FLOOR)
                        customForward = Vector3.up;

                    if (item.name == MazeUnit.WEST)
                        customForward = Vector3.left;

                    if (item.name == MazeUnit.EAST)
                        customForward = Vector3.right;

                    if (item.name == MazeUnit.SOUTH)
                        customForward = Vector3.back;

                    if (item.name == MazeUnit.NORTH)
                        customForward = Vector3.forward;

                    var pos = item.transform.position;
                    Handles.DrawWireDisc(pos, customForward, 0.1f);
                    Handles.Label(pos, new GUIContent(item.name));
                }
            }

            Handles.color = tempHandleColor;
        }
    }
}