using UnityEngine;
using System.Collections;

public static class Extensions
{
    public static void ApplyTarget(this Transform origin, Transform targetTransform)
    {
        origin.parent = targetTransform.parent;

        origin.localPosition = targetTransform.localPosition;

        origin.localRotation = targetTransform.localRotation;

        origin.transform.localScale = targetTransform.localScale;
    }

    public static string AsPartFileName(this Vector3 v){
       
        var fileNameCompatibleString = string.Format("{0}x{1}x{2}", v.x, v.y, v.z);

        return fileNameCompatibleString.Replace('.', '_');
    }
}
