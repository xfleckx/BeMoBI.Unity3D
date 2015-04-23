using UnityEngine;
using System.Collections;

public delegate void TrialBeginn();

public enum ReasonForTrialEnd { Finished, Aborted }

public delegate void TrialEnd(ReasonForTrialEnd reason);

public interface ITrial
{
    HUDInstruction instructions {get;set;}
    VirtualRealityManager world { get; set; }
    ParadigmController paradigm { get; set; }

    ITrial Next { get; set; }

    event TrialBeginn CallOnTrialBegin;

    event TrialEnd CallOnTrialEnd;
}


public class TrialController : beMoBIBase, ITrial {

	public HUDInstruction instructions {get;set;}
    public VirtualRealityManager world { get; set; }
    public ParadigmController paradigm { get; set; }

	public event TrialBeginn CallOnTrialBegin;

	public event TrialEnd CallOnTrialEnd;

    public ITrial Next { get; set; }

	void Awake()
	{
		base.Initialize();
	
		if (!instructions)
		{
			instructions = HUDInstruction.Instance;
		}
	}

	// Use this for initialization
	void Start () {
	

	}
	
	// Update is called once per frame
	void Update () {
	
	}

}
