using UnityEngine;
using System.Collections;
using System;

public class TopLighting : MonoBehaviour {

    private Light centerLight;
    
	// Use this for initialization
	void Start () {
        centerLight = GetComponentInChildren<Light>();
	}

    public void SwitchOn()
    {
        this.centerLight.enabled = true;
    }

    public void SwitchOff()
    {
        this.centerLight.enabled = false;
    }

    public void ToggleLightDirection(OpenDirections direction)
    {
        string directionName = Enum.GetName(typeof(OpenDirections), direction);

        var target = this.transform.FindChild(directionName);

        if(target == null){
            Debug.Assert(target != null, string.Format("This component should have an child with the name: {0}", target));
            return;
            }

        bool state = target.gameObject.activeSelf;
        target.gameObject.SetActive(!state);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, -transform.up / 2);
    }
     
}
