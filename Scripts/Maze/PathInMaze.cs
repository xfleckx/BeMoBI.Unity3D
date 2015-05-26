using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum UnitType { I, L, T, X }
public enum TurnType { STRAIGHT, LEFT, RIGHT }

[RequireComponent(typeof(PathController))]
public class PathInMaze : MonoBehaviour, ISerializationCallbackReceiver
{
    public Dictionary<Vector2, PathElement> PathElements;

    public ObjectHideOut HideOut;
    
    public MazeUnit HideOutReplacement;

    public int ID = -1;

    public void OnEnable()
    {
        InitEmptys();

        EnableHideOut();
    }

    public void OnDisable()
    {
        DisableHideOut();
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
    }

    public void EnableHideOut()
    {
        if (HideOut != null) { 
            HideOut.enabled = true;
            HideOut.gameObject.SetActive(true);
            HideOutReplacement.gameObject.SetActive(false);
        }
    }
    public void DisableHideOut()
    {
        if (HideOut != null) { 
            HideOut.enabled = false;
            HideOut.gameObject.SetActive(false);
            HideOutReplacement.gameObject.SetActive(true);
        }
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
        Debug.Log("Serialization starts");

        if (PathElements == null || Units == null || GridIDs == null ||Elements == null)
            return;

        GridIDs.Clear();
        Elements.Clear();
        Units.Clear();

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

        for (int i = 0; i < GridIDs.Count; i++)
		{   
            var gid = GridIDs[i];
            PathElements.Add(gid, Elements[i]);
            PathElements[gid].Unit = Units[i];
        }

        Debug.Log("Derialization ends");
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

    public void OnDestroy()
    {
        Debug.Log(string.Format("{0} destroyed!", name));
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
