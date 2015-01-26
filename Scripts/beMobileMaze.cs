using UnityEngine;
using System.Collections;

public class beMobileMaze : MonoBehaviour {

    public float MazeWidthInMeter = 4f;
    public float MazeLengthInMeter = 8f;
    public float RoomHigthInMeter = 3;

    public Vector2 RoomDimension = new Vector2(1.5f, 1.5f);
     
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


        //int roomsInLength = Mathf.FloorToInt(MazeLengthInMeter / (RoomDimension.y + WallThicknessInMeter)); 
        //int roomsInWidth = Mathf.FloorToInt(MazeWidthInMeter / (RoomDimension.x + WallThicknessInMeter)); 
        //// elements are rooms and walls
        //int elementsCountInLength = roomsInLength + wallsInLength;

        //int elementsCountInWidth = 
        
        //for (int i = 0; i < elementsCountInLength; i++)
        //{
        //    for (int j = 0; j < elementsCountInWidth; j++)
        //    {

        //    }
        //}
    }

    private void drawFloorGrid(Vector3 origin, int elementsCountInLength, int elementsCountInWidth) 
    {
        
    
    }
}
