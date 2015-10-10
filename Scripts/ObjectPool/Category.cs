using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Category : MonoBehaviour {
     
    public List<GameObject> AssociatedObjects = new List<GameObject>();

    public bool HasObjects { get { return AssociatedObjects.Count > 0; } }

    private Stack<GameObject> tempSampling;

    public GameObject Sample()
    {
        int max =  AssociatedObjects.Count;
        
        var randomIndex = UnityEngine.Random.Range(0,max);

        return AssociatedObjects[randomIndex];
    }

    /// <summary>
    /// Set will be sampled until its empty than it resets and starts again.
    /// </summary>
    /// <returns></returns>
    public GameObject SampleWithoutReplacement()
    {
        if (tempSampling == null || tempSampling.Count == 0) { 
            var shuffled = AssociatedObjects.OrderBy(a => Guid.NewGuid()).ToList();
            tempSampling = new Stack<GameObject>(shuffled);
        }

        return tempSampling.Pop();
    }

    public GameObject GetObjectBy(string requestedName)
    {
        var target = AssociatedObjects.Single((o) => o.name.Equals(requestedName));

        if(target == null) { 

           var targetTransform = this.transform.Find(name);

            if (targetTransform == null)
                throw new ArgumentException(string.Format("The request object with the \"{0}\" doesn't exist!", requestedName));

            var hasMesh = targetTransform.GetComponentInChildren<MeshRenderer>() != null;
            var hasSkinnedMesh = targetTransform.GetComponentInChildren<SkinnedMeshRenderer>() != null;


            if (hasMesh || hasSkinnedMesh) {
                AssociatedObjects.Add(targetTransform.gameObject);
                return targetTransform.gameObject;
            }
            else
                throw new ArgumentException(string.Format("The request object with the \"{0}\" exists but doesn't appear to be a visible object!"));
        }

        return target;
    }
}
