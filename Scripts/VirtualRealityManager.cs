using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VirtualRealityManager : MonoBehaviour {

	//VR Real World Dimension for MoCap Systems
	public float HighQualityZoneWidth = 8f;
	public float HighQualityZoneLength = 12f;
	public float BorderZoneWidth = 1f;
	
	[SerializeField]
	public List<EnvironmentController> AvailableEnvironments = new List<EnvironmentController>();

	[SerializeField]
	public List<EnvironmentController> ActiveEnvironments = new List<EnvironmentController>();
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKey(KeyCode.KeypadEnter))
		{
			Debug.Log("Enter hit");
		}
	}

   /// <summary>
   /// Change the whole world to exactly one environment
   /// </summary>
   /// <param name="worldName"></param>
	public EnvironmentController ChangeWorld(string worldName)
	{
		if (AvailableEnvironments.Any((i) => i.Title.Equals(worldName))) {

			if (this.ActiveEnvironments.Any())
				this.ActiveEnvironments.ForEach((i) => i.gameObject.SetActive(false));

			this.ActiveEnvironments.Clear();
			var enabledEnvironment = AvailableEnvironments.First((i) => i.Title.Equals(worldName));

			enabledEnvironment.gameObject.SetActive(true);
			
			this.ActiveEnvironments.Add(enabledEnvironment);
			
			return enabledEnvironment;
		}

		return null;
	}

	/// <summary>
	/// Combine multiple environments - TODO HalleV and Maze
	/// </summary>
	/// <param name="names"></param>
	public void CombineEnvironments(params string[] names)
	{  
		foreach (var item in names)
		{
			var environment = this.AvailableEnvironments.First((i) => i.Title.Equals(item));
			if(environment != null)
				environment.gameObject.SetActive(true);
		}
	}

	void OnDrawGizmos()
	{
		DrawRealWorldBorder();
	}

	private void DrawRealWorldBorder()
	{
		float halfWidth = HighQualityZoneWidth / 2;
		float halfLengt = HighQualityZoneLength / 2;

		float x0 = transform.position.x - halfWidth;
		float x1 = transform.position.x + halfWidth;
		float z0 = transform.position.z - halfLengt;
		float z1 = transform.position.z + halfLengt;

		Vector3 env00 = new Vector3(x0, transform.position.y, z0);
		Vector3 env11 = new Vector3(x1, transform.position.y, z1);
		Vector3 env01 = new Vector3(x0, transform.position.y, z1);
		Vector3 env10 = new Vector3(x1, transform.position.y, z0);

		float dotSize = 1f;

#if UNITY_EDITOR
		
		Handles.DrawDottedLine(env00, env01, dotSize);
		Handles.DrawDottedLine(env00, env10, dotSize);
		Handles.DrawDottedLine(env10, env11, dotSize);
		Handles.DrawDottedLine(env01, env11, dotSize);
#endif

		Vector3 bz_length_Size = new Vector3(BorderZoneWidth, 0, HighQualityZoneLength + 2 * BorderZoneWidth);
		Vector3 bz_Width_Size = new Vector3(HighQualityZoneWidth + 2 * BorderZoneWidth, 0, BorderZoneWidth);

		float halfBorderZone = BorderZoneWidth / 2;

		float bz_west = x0 - halfBorderZone;
		Vector3 bz_west_Origin = new Vector3(bz_west, transform.position.y, transform.position.z);

		float bz_east = x1 + halfBorderZone;
		Vector3 bz_east_Origin = new Vector3(bz_east, transform.position.y, transform.position.z);

		float bz_north = z1 + halfBorderZone;
		Vector3 bz_north_Origin = new Vector3(transform.position.x, transform.position.y, bz_north);

		float bz_south = z0 - halfBorderZone;
		Vector3 bz_south_Origin = new Vector3(transform.position.x, transform.position.y, bz_south);

		Gizmos.color = new Color(1, 0, 0, 0.1f);
		Gizmos.DrawCube(bz_west_Origin, bz_length_Size);
		Gizmos.DrawCube(bz_east_Origin, bz_length_Size);
		Gizmos.DrawCube(bz_north_Origin, bz_Width_Size);
		Gizmos.DrawCube(bz_south_Origin, bz_Width_Size);
	}
}
