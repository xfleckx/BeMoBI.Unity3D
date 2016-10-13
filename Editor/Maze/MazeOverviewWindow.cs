using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
using Assets.SNEED.EditorExtensions.Maze.UnitCreation;

namespace Assets.SNEED.EditorExtensions.Mazes
{
	public class MazeOverviewWindow : EditorWindow
	{
		private IEnumerable<beMobileMaze> allSelectedMazes;

		private bool mazesSelected = false;

		private GUIContent noMazesSelected = new GUIContent("NO MAZES SELECTED");
		private GUIContent windowHeading = new GUIContent("Maze Preview");

		private GUIGrid grid;

		private bool isDev = true;
		private Mesh devMesh;
		private Material devMat;

		[MenuItem("SNEED/Show Maze Preview")]
		public static void Init()
		{
			var current = GetWindow<MazeOverviewWindow>();

			current.SetUpDev();
			
			current.Show(true);
		}
		private Mesh CreateNorthMesh()
		{
			var mesh = new Mesh();

			var vertices = new List<Vector3>()
		{
			new Vector3(1,     0,           0),
			new Vector3(0,     1,      0),
			new Vector3(0,       0,     0),
			new Vector3(1,    1,           0)
		};



			mesh.SetVertices(vertices);

			var triangles = new List<int>()
			{
				   0, // First triangle - count the 
				   2,
				   1,
				   0, // Second triangle
				   1,
				   3
			};

			mesh.SetTriangles(triangles, 0);

			var normals = new List<Vector3>()
		{
			Vector3.back,
			Vector3.back,
			Vector3.back,
			Vector3.back

		};

			mesh.SetNormals(normals);

			var uvs = MeshUtilities.GetPlaneUVs();

			mesh.SetUVs(0, uvs);

			mesh.RecalculateBounds();
			mesh.Optimize();

			return mesh;
		}

		private void SetUpDev()
		{

			devMesh = CreateNorthMesh();

			var test = GameObject.CreatePrimitive(PrimitiveType.Capsule);

			devMat = test.GetComponent<MeshRenderer>().sharedMaterial;

			DestroyImmediate(test);

			grid = new GUIGrid();
			grid.cols = 1;
			grid.rows = 1;

			grid.Rects = new List<Rect>();

			var testSize = new Vector2(555, 555);

			for (int i = 0; i < grid.cols; i++)
			{
				for (int j = 0; j < grid.rows; j++)
				{
					grid.Rects.Add(new Rect(new Vector2(i * testSize.x + (i * 5), j * testSize.y + (j * 5)), testSize));
				}
			}
		}

		void OnInspectorUpdate()
		{

		}

		void OnSelectionChange()
		{
			if (Selection.gameObjects.Any())
			{
				var allCurrentSelectedMazes = Selection.gameObjects.Where(
					 (go) => go.GetComponent<beMobileMaze>() != null).Select(
					 go => go.GetComponent<beMobileMaze>());

				allSelectedMazes = allCurrentSelectedMazes;
			}

			mazesSelected = allSelectedMazes.Any();

		}

		void OnGUI()
		{
			this.titleContent = windowHeading;

			if (!mazesSelected && !isDev)
			{
				this.ShowNotification(noMazesSelected);
				return;
			}

			var windowSize = position.size;

			if (grid == null)
				SetUpDev();

			if(Event.current.type == EventType.Repaint) { 

				var preview = new PreviewRenderUtility(true);

				foreach (var rect in grid.Rects)
				{
					preview.BeginStaticPreview(rect);
					preview.m_Camera = SceneView.currentDrawingSceneView.camera;
					preview.m_Camera.rect = rect;
					preview.m_Camera.transform.position = Vector3.zero + new Vector3(0, -3, 0);



					Handles.CubeCap(0, Vector3.zero, Quaternion.identity, 1);

					preview.DrawMesh(devMesh, Vector3.zero, Quaternion.identity, devMat, 0);
					
					preview.m_Camera.Render();

					var resultTexture = preview.EndStaticPreview();

					EditorGUI.DrawPreviewTexture(rect, resultTexture);
				
				}
			}
		}

	}


	internal class GUIGrid
	{
		public int rows;
		public int cols;

		public List<Rect> Rects;
	}
}
