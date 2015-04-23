using UnityEngine;
using System.Collections;

public class ParadigmController : beMoBIBase {

	HUDInstruction instructions;

	/// <summary>
	/// The world exists and gets manipulated in the trials under specific conditions
	/// </summary>
	VirtualRealityManager world;

	TrialController entryTrial;

	void Awake()
	{
		base.Initialize();
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	    


	}

	void BeginParadigm()
	{
		if (!entryTrial)
		{
			throw new MissingReferenceException("No Trial to start defined!");
		}


	}

	void OnCurrentTrialStart()
	{

	}

	void OnCurrentTrialEnd(ReasonForTrialEnd reason)
	{
        GoFromTo(entryTrial, entryTrial.Next);
	}

	public void GoFromTo(ITrial currentTrlal, ITrial nextTrial)
	{
		currentTrlal.CallOnTrialBegin -= OnCurrentTrialStart;
		currentTrlal.CallOnTrialEnd -= OnCurrentTrialEnd;

		nextTrial.CallOnTrialBegin += OnCurrentTrialStart;
		nextTrial.CallOnTrialEnd += OnCurrentTrialEnd;
	}

    public void RequestPause()
    {

    }

}
