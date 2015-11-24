using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum UnitType { I, L, T, X }
public enum TurnType { STRAIGHT, LEFT, RIGHT }

[RequireComponent(typeof(PathController))]
public class PathInMaze : MonoBehaviour, ISerializationCallbackReceiver
{
	public bool Available = true;

	public Dictionary<Vector2, PathElement> PathElements;

    public LinkedList<PathElement> PathAsLinkedList;
    
	private bool inverse = false;

	public bool Inverse
	{
		get { return inverse; } 
	}

	public int ID = -1;

	public void OnEnable()
	{
		InitEmptys();
	}

	public void OnDisable()
	{
	}

	void Awake()
	{
		InitEmptys();
	}

	void InitEmptys()
	{
		if (Units == null)
		{
			Units = new List<MazeUnit>();
			Debug.Log("Creating empty Unit List in Path");
		}

		if (GridIDs == null)
		{
			GridIDs = new List<Vector2>();
			Debug.Log("Creating empty GridID list in Path");
		}

		if (Elements == null)
		{
			PathElements = new Dictionary<Vector2, PathElement>();
			Debug.Log("Creating empty Elements Dictionary in Path");
		}
        
        if (PathAsLinkedList == null)
        {
            PathAsLinkedList = new LinkedList<PathElement>();
            Debug.Log("Creating empty Linked List");
        }

    }

    public void InvertPath()
	{
		IEnumerable<KeyValuePair<Vector2, PathElement>> temp = PathElements.Reverse().ToList();
		PathElements = temp.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value);
		inverse = !inverse;
	}

	#region Serialization

	[HideInInspector]
	[SerializeField]
	private List<MazeUnit> Units;

	[HideInInspector]
	[SerializeField]
	private List<PathElement> Elements;

	[HideInInspector]
	[SerializeField]
	private List<Vector2> GridIDs;

	// save the dictionary to lists
	public void OnBeforeSerialize()
	{ 
		if (PathElements == null || Units == null || GridIDs == null ||Elements == null)
			return;

		GridIDs.Clear();
		Elements.Clear();
		Units.Clear();
        PathAsLinkedList.Clear();

		foreach (var item in PathElements)
		{
			GridIDs.Add(item.Key);
			Elements.Add(item.Value);
			Units.Add(item.Value.Unit);
		}
	}

	// load dictionary from lists
	public void OnAfterDeserialize()
	{
		PathElements = new Dictionary<Vector2, PathElement>();
        PathAsLinkedList = new LinkedList<PathElement>(Elements);

		for (int i = 0; i < GridIDs.Count; i++)
		{   
			var gid = GridIDs[i];
			PathElements.Add(gid, Elements[i]);
			PathElements[gid].Unit = Units[i];
		}
		 
	}

	#endregion 

#if UNITY_EDITOR
	public Action EditorGizmoCallbacks;
#endif

	public void OnDrawGizmos()
	{
#if UNITY_EDITOR
		if (EditorGizmoCallbacks != null)
			EditorGizmoCallbacks();
#endif
	}
}

[Serializable]
public class PathElement
{
	[SerializeField]
	public GameObject Landmark;

	[SerializeField]
	public MazeUnit Unit;

	[SerializeField]
	public UnitType Type;

	[SerializeField]
	public TurnType Turn;

	public PathElement()
	{

	}

	public PathElement(MazeUnit unit, UnitType type = UnitType.I, TurnType turn = TurnType.STRAIGHT) 
	{ 
		Type = type;
		Turn = turn;
		Unit = unit;
	}
}
