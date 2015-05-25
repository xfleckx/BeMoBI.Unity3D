using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class MazeUnit : MonoBehaviour {

	public const string NORTH = "North";
	public const string SOUTH = "South";
	public const string WEST = "West";
	public const string EAST = "East";

    [SerializeField]
	public Vector2 GridID;

	public void Initialize(Vector2 tilePos)
	{
		GridID = tilePos; 
	}


	public virtual void Open(string directionName)
	{
		transform.FindChild(directionName).gameObject.SetActive(false);
	}

	public virtual void Close(string directionName)
	{
		transform.FindChild(directionName).gameObject.SetActive(true);
	}


	void OnTriggerEnter(Collider c)
	{
		Debug.Log(string.Format("Entering {0} {1}", GridID.x, GridID.y));

		var evt = new MazeUnitEvent(MazeUnitEventType.Entering, c, this); 

		SendMessageUpwards("RecieveUnitEvent", evt, SendMessageOptions.DontRequireReceiver);
	}
    
	void OnTriggerExit(Collider c)
	{
		Debug.Log(string.Format("Leaving {0} {1}", GridID.x, GridID.y));

		var evt = new MazeUnitEvent(MazeUnitEventType.Exiting, c, this); 
		
		SendMessageUpwards("RecieveUnitEvent", evt, SendMessageOptions.DontRequireReceiver);
	} 


	#region deprecated
	public static void Join(IEnumerable<MazeUnit> units)
	{
		System.Diagnostics.Debug.Assert(units.Any(), "should never called with empty enumerable");

		var unitsOrderedByColum = units.OrderBy((u) => u.GridID.x);
		var unitsOrderedByRow = units.OrderBy((u) => u.GridID.y);

		var columnStack = new Stack<MazeUnit>(unitsOrderedByColum);
		var rowStack = new Stack<MazeUnit>(unitsOrderedByRow);

		MazeUnit current = null;

		while (rowStack.Any())
		{

			if (current == null) {
				current = rowStack.Pop();
			}

			var lookAhead = rowStack.Peek();

			Debug.Log(string.Format("Current Grid ID {0} {1} vs LookAhead Grid ID {2} {3}",
				current.GridID.x, current.GridID.y, lookAhead.GridID.x, lookAhead.GridID.y));

			if (lookAhead.GridID.y == current.GridID.y + 1)
			{
				current.Open(NORTH);
				lookAhead.Open(SOUTH);
			}
			else if (lookAhead.GridID.y == current.GridID.y - 1)
			{
				current.Open(SOUTH);
				lookAhead.Open(NORTH);
			}

			current = rowStack.Pop(); 
		}

		current = null;

		while (columnStack.Any())
		{
			if (current == null)
			{
				current = columnStack.Pop();
			}

			var lookAhead = columnStack.Peek();

			if (lookAhead.GridID.x == current.GridID.x + 1)
			{
				current.Open(EAST);
				lookAhead.Open(WEST);
			}
			else if (lookAhead.GridID.x == current.GridID.x - 1)
			{
				lookAhead.Open(EAST);
				current.Open(WEST);
			}

			current = columnStack.Pop();
		}
	}

	public static void Split(IEnumerable<MazeUnit> units)
	{
		System.Diagnostics.Debug.Assert(units.Any(), "should never called with empty enumerable");

		
		var unitsOrderedByColum = units.OrderBy((u) => u.GridID.x);
		var unitsOrderedByRow = units.OrderBy((u) => u.GridID.y);

		var columnStack = new Stack<MazeUnit>(unitsOrderedByColum);
		var rowStack = new Stack<MazeUnit>(unitsOrderedByRow);

		MazeUnit current = null;

		while (rowStack.Any())
		{

			if (current == null) {
				current = rowStack.Pop();
			}

			var lookAhead = rowStack.Peek();

			Debug.Log(string.Format("Current Grid ID {0} {1} vs LookAhead Grid ID {2} {3}",
				current.GridID.x, current.GridID.y, lookAhead.GridID.x, lookAhead.GridID.y));

			if (lookAhead.GridID.y == current.GridID.y + 1)
			{
				current.Close(NORTH);
				lookAhead.Close(SOUTH);
			}
			else if (lookAhead.GridID.y == current.GridID.y - 1)
			{
				current.Close(SOUTH);
				lookAhead.Close(NORTH);
			}

			current = rowStack.Pop(); 
		}

		current = null;

		while (columnStack.Any())
		{
			if (current == null)
			{
				current = columnStack.Pop();
			}

			var lookAhead = columnStack.Peek();

			if (lookAhead.GridID.x == current.GridID.x + 1)
			{
				current.Close(EAST);
				lookAhead.Close(WEST);
			}
			else if (lookAhead.GridID.x == current.GridID.x - 1)
			{
				lookAhead.Close(EAST);
				current.Close(WEST);
			}

			current = columnStack.Pop();
		}

	}
	#endregion
}

public enum MazeUnitEventType { Entering, Exiting }

public class MazeUnitEvent
{
	private MazeUnitEventType mazeUnitEventType;
	public MazeUnitEventType MazeUnitEventType
	{
		get { return mazeUnitEventType; } 
	}

	private Collider c;
	private MazeUnit mazeUnit;
	public Collider Collider
	{
		get { return c; } 
	}
	 
	public MazeUnitEvent(global::MazeUnitEventType mazeUnitEventType, UnityEngine.Collider c, MazeUnit mazeUnit)
	{
		// TODO: Complete member initialization
		this.mazeUnitEventType = mazeUnitEventType;
		this.c = c;
		this.mazeUnit = mazeUnit;
	}
	
}