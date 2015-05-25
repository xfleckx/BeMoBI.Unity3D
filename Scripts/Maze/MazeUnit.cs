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