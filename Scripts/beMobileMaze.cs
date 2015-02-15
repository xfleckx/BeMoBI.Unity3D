using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class beMobileMaze : MonoBehaviour {

    public float MazeWidthInMeter = 6f;
    public float MazeLengthInMeter = 10f;
    public float RoomHigthInMeter = 3;
    
    public Vector2 RoomDimension = new Vector2(1.3f, 1.3f);
    public Vector2 EdgeDimension = new Vector2(0.1f, 0.1f);

    public List<GameObject> Walls;
    public List<GameObject> Edges;
    public List<GameObject> Waypoints;

    public List<GameObject> Units = new List<GameObject>();

    public float WallThicknessInMeter = 0.1f;
     
    private Vector3 origin;

    void OnEnable() 
    {
        origin = gameObject.transform.position;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnDrawGizmos()
    { 
        origin = gameObject.transform.position;
        float yoffset = RoomHigthInMeter / 2;
        origin = new Vector3(origin.x, origin.y + yoffset, origin.z);
        Vector3 boundingBox = new Vector3(MazeWidthInMeter, RoomHigthInMeter, MazeLengthInMeter);
        Gizmos.DrawWireCube(origin, boundingBox);
        
        foreach (var unit in Units)
        {
            float unitOriginX = unit.transform.position.x + WallThicknessInMeter + RoomDimension.x / 2;
            float unitOriginZ = unit.transform.position.z + WallThicknessInMeter + RoomDimension.y / 2;
            Vector3 roomOrigin = new Vector3(unitOriginX, origin.y, unitOriginZ);
            Vector3 waypointLineEnd = new Vector3(unitOriginX, 0.9f, unitOriginZ);
            Gizmos.DrawLine(new Vector3(unitOriginX, origin.y - yoffset, unitOriginZ), waypointLineEnd);
            Gizmos.DrawIcon(waypointLineEnd, "WayPointIcon.png", true);
            Gizmos.DrawWireCube(roomOrigin, new Vector3(1.5f, RoomHigthInMeter, 1.5f));
        }
    }


    private void drawFloorGrid(Vector3 origin, int elementsCountInLength, int elementsCountInWidth) 
    {
        
    
    }
}
