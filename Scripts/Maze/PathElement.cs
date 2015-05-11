using UnityEngine;
using System.Collections;

public class PathElement : MonoBehaviour {

    public MazeUnit parentUnit;

    void Awake()
    {
        parentUnit = GetComponentInParent<MazeUnit>();

        if (parentUnit == null) {
            throw new MissingComponentException("Path element has no parent object which has an MazeUnit component!");
        }
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
