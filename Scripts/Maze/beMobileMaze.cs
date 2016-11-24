using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.SNEED.Mazes
{
	[Serializable]
	public class beMobileMaze : MonoBehaviour, ISerializationCallbackReceiver
	{
		#region replace this with readonly creation model

		[SerializeField]
		public string UnitNamePattern = "Unit_{0}_{1}";
		[SerializeField]
		public float MazeWidthInMeter = 6f;
		[SerializeField]
		public float MazeLengthInMeter = 10f;
		[SerializeField]
		public Vector3 RoomDimension = new Vector3(1.3f, 2, 1.3f);

		public int Rows { get { return Grid != null ? Grid.GetLength(1) : 0; } }

		public int Columns { get { return Grid != null ? Grid.GetLength(0) : 0; } }
	
		#endregion

		public event Action<MazeUnitEvent> MazeUnitEventOccured;

		[SerializeField]
		public List<MazeUnit> Units = new List<MazeUnit>();
	
		[SerializeField]
		public MazeUnit[,] Grid;
	
		public MazeUnit this[int col, int row]
		{
			get { return Grid[col, row]; }
			set { Grid[col, row] = value; }
		}

		public void RecieveUnitEvent(MazeUnitEvent unitEvent)
		{
			if (MazeUnitEventOccured != null)
				MazeUnitEventOccured(unitEvent);
		}

		#region Serialization logic to rebuild the grid 
		public void OnBeforeSerialize()
		{ 
			// does nothing in this case
		}

		public void OnAfterDeserialize()
		{
			var gridSize = CalcGridSize(this);

			Grid = FillGridWith(Units, (int) gridSize.x, (int) gridSize.y);
		}

		public static Vector2 CalcGridSize(beMobileMaze maze)
		{
			int columns = Mathf.FloorToInt(maze.MazeWidthInMeter / maze.RoomDimension.x) + 1;
			int rows = Mathf.FloorToInt(maze.MazeLengthInMeter / maze.RoomDimension.z) + 1;

			return new Vector2(columns, rows);
		}

		public static MazeUnit[,] FillGridWith(IEnumerable<MazeUnit> existingUnits, int columns, int rows)
		{
			var grid = new MazeUnit[columns, rows];

			foreach (var unit in existingUnits)
			{
				var x = (int)unit.GridID.x;
				var y = (int)unit.GridID.y;
				grid[x, y] = unit;
			}

			return grid;
		}

		#endregion
	}

}
