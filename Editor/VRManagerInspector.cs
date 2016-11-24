using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;
using Assets.SNEED.Scripts;

namespace Assets.SNEED.EditorExtensions.CustomInspectors
{
    [CustomEditor(typeof(VirtualRealityManager))]
    public class VRManagerInspector : Editor
    {
        private VirtualRealityManager vrcontroller;

        void OnEnable()
        {
            LookUpEnvironments();
        }

        private void LookUpEnvironments()
        {
            vrcontroller = (VirtualRealityManager)target;

            var environments = vrcontroller.transform.AllChildren().Where(
                (e) => e.GetComponent<EnvironmentController>() != null)
                .Select((e) => e.GetComponent<EnvironmentController>());

            vrcontroller.AvailableEnvironments.Clear();
            vrcontroller.AvailableEnvironments.AddRange(environments);
        }

        public override void OnInspectorGUI()
        {
            if (vrcontroller == null)
                vrcontroller = (VirtualRealityManager)target;

            base.OnInspectorGUI();
        }

        public void OnSceneGUI()
        {
            vrcontroller = (VirtualRealityManager)target;

            if (!vrcontroller.AvailableEnvironments.Any((c) => c != null))
                return;

            Handles.BeginGUI();

            GUILayout.Space(25);

            GUILayout.BeginVertical(GUILayout.MaxWidth(75));

            GUILayout.Label("Choose Environment", EditorStyles.whiteLabel);
            GUILayout.Space(10);


            foreach (var item in vrcontroller.AvailableEnvironments)
            {
                GUILayout.BeginHorizontal();

                var state = item.gameObject.activeSelf;

                state = GUILayout.Toggle(state, "");

                item.gameObject.SetActive(state);

                if (GUILayout.Button(item.Title, GUILayout.Width(75)))
                {
                    vrcontroller.ChangeWorld(item.Title);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();


            Handles.EndGUI();
        }


        [DrawGizmo(GizmoType.Active | GizmoType.NonSelected)]
        static void DrawRealWorldBorder(VirtualRealityManager manager, GizmoType gizmoType)
        {
            var tempGizmoMatrix = Gizmos.matrix;

            Gizmos.matrix = manager.transform.localToWorldMatrix;

            float halfWidth = manager.HighQualityZoneWidth / 2;
            float halfLengt = manager.HighQualityZoneLength / 2;

            float x0 = 0 - halfWidth;
            float x1 = 0 + halfWidth;
            float z0 = 0 - halfLengt;
            float z1 = 0 + halfLengt;

            Vector3 env00 = new Vector3(x0, 0, z0);
            Vector3 env11 = new Vector3(x1, 0, z1);
            Vector3 env01 = new Vector3(x0, 0, z1);
            Vector3 env10 = new Vector3(x1, 0, z0);

            Gizmos.DrawLine(env00, env01);
            Gizmos.DrawLine(env00, env10);
            Gizmos.DrawLine(env10, env11);
            Gizmos.DrawLine(env01, env11);

            Vector3 bz_length_Size = new Vector3(manager.BorderZoneWidth, 0, manager.HighQualityZoneLength + 2 * manager.BorderZoneWidth);
            Vector3 bz_Width_Size = new Vector3(manager.HighQualityZoneWidth + 2 * manager.BorderZoneWidth, 0, manager.BorderZoneWidth);

            float halfBorderZone = manager.BorderZoneWidth / 2;

            float bz_west = x0 - halfBorderZone;
            Vector3 bz_west_Origin = new Vector3(bz_west, 0, 0);

            float bz_east = x1 + halfBorderZone;
            Vector3 bz_east_Origin = new Vector3(bz_east, 0, 0);

            float bz_north = z1 + halfBorderZone;
            Vector3 bz_north_Origin = new Vector3(0, 0, bz_north);

            float bz_south = z0 - halfBorderZone;
            Vector3 bz_south_Origin = new Vector3(0, 0, bz_south);

            Gizmos.color = new Color(1, 0, 0, 0.1f);
            Gizmos.DrawCube(bz_west_Origin, bz_length_Size);
            Gizmos.DrawCube(bz_east_Origin, bz_length_Size);
            Gizmos.DrawCube(bz_north_Origin, bz_Width_Size);
            Gizmos.DrawCube(bz_south_Origin, bz_Width_Size);

            Gizmos.matrix = tempGizmoMatrix;
        }

    }

}