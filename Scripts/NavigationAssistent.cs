using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class NavigationAssistent : beMoBIBase {

    public  List<GameObject> Waypoints = new  List<GameObject>();

    void Awake() {
        base.Initialize();
    }

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

   
}
