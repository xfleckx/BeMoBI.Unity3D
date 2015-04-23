using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.EventSystems;

public class Waypoint : beMoBIBase, IEventSystemHandler {

    [SerializeField]
    public UnityEvent m_OnWaypointReached = new UnityEvent();

	void Awake()
	{

	}

	// Use this for initialization
	void Start () {
		base.Initialize();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTriggerEnter(Collider other)
	{
		WriteMarker("Way point entered");

        m_OnWaypointReached.Invoke();
	}

	public void OnTriggerExit(Collider other)
	{ 
		WriteMarker("Way point exit");

        SendMessageUpwards("RecieveWaypointEvent", name);
	}


	private Vector3 gizmoSize = new Vector3(0.01f, 0.01f, 0.01f);

	void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(transform.position, 0.1f);
		Gizmos.DrawCube(transform.position, gizmoSize);
	}
}
 