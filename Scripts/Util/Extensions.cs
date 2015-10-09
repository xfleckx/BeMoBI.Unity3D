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
}
