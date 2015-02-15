using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class beMoBIBase : MonoBehaviour {
    
    public void Reset()
    {
        Initialize();
    }

    protected IMarkerStream markerStreamInstance;
    private List<Component> markerStreams;

    protected void Initialize()
    {
        markerStreams = new List<Component>(GetComponents(typeof(IMarkerStream)));

        if (markerStreams.Count > 0)
        {
            markerStreamInstance = markerStreams[0] as IMarkerStream;
        }
        else
        {
            Debug.LogWarning("No instance implementating IMarkerStream found! \n creating Debug.Log MarkerStream instance");
            GameObject DebugMarkerStreamHost = new GameObject();
            DebugMarkerStreamHost.AddComponent(typeof(DebugMarkerStream));
            DebugMarkerStreamHost.name = DebugMarkerStream.Instance.StreamName;
        }
    }

    protected void WriteMarker(string name) 
    {
        markerStreamInstance.Write(name);
    }

    protected void WriteMarker(string name, float customTimeStamp) 
    {
        markerStreamInstance.Write(name, customTimeStamp);
    }
}
 
public interface IMarkerStream
{
    string StreamName { get; } 

    void Write(string name, float customTimeStamp);

    void Write(string name);
}

public class DebugMarkerStream : Singleton<DebugMarkerStream>, IMarkerStream
{ 
    private const string streamName = "DebugMarkerStream";
    public string StreamName
    {
        get { return streamName; }
    }

    private const string logWithTimeStampPattern = "Marker {0} at {1}"; 

    public void Write(string name, float customTimeStamp)
    {
        Debug.Log(string.Format(logWithTimeStampPattern, name, customTimeStamp));
    }

    public void Write(string name)
    {
        Debug.Log(string.Format(logWithTimeStampPattern, name, Time.realtimeSinceStartup));
    }
}