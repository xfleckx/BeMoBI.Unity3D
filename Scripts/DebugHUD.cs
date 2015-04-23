using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugHUD : MonoBehaviour {

	public KeyCode toggleHud;

	public GameObject hudRenderPanel;
 
	public Text position;

	private bool visible = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKey(toggleHud))
		{
			ShowHudFor(float.PositiveInfinity);
		}

	}

	/// <summary>
	/// Display the HUD for Error Messages only a few seconds...
	/// 
	/// </summary>
	/// <param name="timeInSeconds">If Positive Infinity toggle through Input</param>
	private void ShowHudFor(float timeInSeconds)
	{
		if (!visible)
		{

		}
	}
}
