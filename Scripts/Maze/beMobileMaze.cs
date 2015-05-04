using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class beMobileMaze : MonoBehaviour
{

	#region replace this with readonly creation model
	public float MazeWidthInMeter = 6f;
	public float MazeLengthInMeter = 10f;
	public float RoomHigthInMeter = 2;
	
	public Vector3 RoomDimension = new Vector3(1.3f, 2, 1.3f);
	public Vector2 EdgeDimension = new Vector2(0.1f, 0.1f);

	public float WallThicknessInMeter = 0.1f;

	public int Rows;
	public int Columns;

	public List<GameObject> Walls;
	public List<GameObject> Edges;
	public List<GameObject> Waypoints;
	#endregion

	public event Action<MazeUnitEvent> MazeUnitEventOccured;

	[SerializeField]
	public List<MazeUnit> Units = new List<MazeUnit>();

	public List<PathInMaze> Paths = new List<PathInMaze>();
	
	public PathInMaze RequirePath(string targetPath)
	{  
		return Paths.SingleOrDefault((p) => p.PathName.Equals(targetPath));
	}
	
	public LinkedList<MazeUnit> CreatePathFromGridIDs(LinkedList<Vector2> gridIDs)
	{
		var enumerator = gridIDs.GetEnumerator();
		var units = new LinkedList<MazeUnit>();

		while (enumerator.MoveNext()) {

			var gridField = enumerator.Current;

			var correspondingUnitHost = transform.FindChild(string.Format("Unit_{0}_{1}", gridField.x, gridField.y));

			if (correspondingUnitHost == null)
				throw new MissingComponentException("It seems, that the path doesn't match the maze! Requested Unit is missing!");
		
			var unit = correspondingUnitHost.GetComponent<MazeUnit>();

			if (unit == null)
				throw new MissingComponentException("Expected Component on Maze Unit is missing!");

			units.AddLast(unit);
		}

		return units;

	}

	public void RecieveUnitEvent(MazeUnitEvent unitEvent)
	{
		if (MazeUnitEventOccured != null)
			MazeUnitEventOccured(unitEvent);
	}

	public IEnumerable<Waypoint> GetActiveWaypoints()
	{
		return GetComponentsInChildren<Waypoint>();
	}
	
	public IEnumerable<Waypoint> GetAllWaypoints()
	{
		return GetComponentsInChildren<Waypoint>(true);
	}

#if UNITY_EDITOR
	public Action EditorGizmoCallbacks;
#endif

	public void OnDrawGizmos()
	{
		drawFloorGrid();
#if UNITY_EDITOR
		if (EditorGizmoCallbacks != null)
			EditorGizmoCallbacks();
#endif
	}

	private void drawFloorGrid()
	{
		// store map width, height and position
		var mapWidth = MazeWidthInMeter;
		var mapHeight = MazeLengthInMeter;
		var position = this.transform.position;

		// draw layer border
		Gizmos.color = Color.white;
		Gizmos.DrawLine(position, position + new Vector3(mapWidth, 0, 0));
		Gizmos.DrawLine(position, position + new Vector3(0, 0, mapHeight));
		Gizmos.DrawLine(position + new Vector3(mapWidth, 0, 0), position + new Vector3(mapWidth, 0, mapHeight));
		Gizmos.DrawLine(position + new Vector3(0, 0, mapHeight), position + new Vector3(mapWidth, 0, mapHeight));

		Columns = Mathf.FloorToInt(MazeWidthInMeter / RoomDimension.x);
		Rows = Mathf.FloorToInt(MazeLengthInMeter / RoomDimension.z);

		// draw tile cells
		Gizmos.color = Color.grey;
		for (float i = 1; i < Columns + 1; i++)
		{
			Gizmos.DrawLine(position + new Vector3(i * RoomDimension.x, 0, 0), position + new Vector3(i * RoomDimension.x, 0, mapHeight));
		}

		for (float i = 1; i < Rows + 1; i++)
		{
			Gizmos.DrawLine(position + new Vector3(0, 0, i * RoomDimension.z), position + new Vector3(mapWidth, 0, i * RoomDimension.z));
		}

	}


}
