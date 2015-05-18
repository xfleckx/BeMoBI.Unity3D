using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class beMobileMaze : MonoBehaviour
{
	#region replace this with readonly creation model

    [SerializeField]
    public string UnitNamePattern = "Unit_{0}_{1}";
    [SerializeField]
	public float MazeWidthInMeter = 6f;
    [SerializeField]
	public float MazeLengthInMeter = 10f;
    [SerializeField]
	public float RoomHigthInMeter = 2;
    [SerializeField]
	public Vector3 RoomDimension = new Vector3(1.3f, 2, 1.3f);
    [SerializeField]
    public Vector2 EdgeDimension = new Vector2(0.1f, 0.1f);
    [SerializeField]
    public float WallThicknessInMeter = 0.1f;

    public int Rows { get { return Grid != null ? Grid.GetLength(1) : 0; } }

    public int Columns { get { return Grid != null ? Grid.GetLength(0) : 0; } }
    
	#endregion

	public event Action<MazeUnitEvent> MazeUnitEventOccured;

	[SerializeField]
	public HashSet<MazeUnit> Units = new HashSet<MazeUnit>();
    
    [SerializeField]
    public MazeUnit[,] Grid;

    public void Awake()
    {

    }

	public void RecieveUnitEvent(MazeUnitEvent unitEvent)
	{
		if (MazeUnitEventOccured != null)
			MazeUnitEventOccured(unitEvent);
	}

#if UNITY_EDITOR
	public Action EditorGizmoCallbacks;
#endif

	public void OnDrawGizmos()
	{ 
#if UNITY_EDITOR
		if (EditorGizmoCallbacks != null)
			EditorGizmoCallbacks();
#endif
	}
     
    void Reset()
    {
        Debug.Log("Reset on Maze"); 
    }
}
