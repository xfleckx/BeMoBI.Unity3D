using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

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

    public static string AsPartFileName(this Vector2 v)
    {
        var fileNameCompatibleString = string.Format("{0}x{1}", v.x, v.y);

        return fileNameCompatibleString.Replace('.', '_');
    }

    public static List<GameObject> AllChildren(this Transform transform)
    {
        var childCount = transform.childCount;

        List<GameObject> result = new List<GameObject>();

        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;
            result.Add(child);
        }

        return result;
    }

}
