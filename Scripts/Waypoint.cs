using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.EventSystems;

namespace Assets.SNEED.Scripts
{
    [RequireComponent(typeof(Collider))]
    public class Waypoint : MonoBehaviour
    {
        [SerializeField]
        public UnityEvent m_OnWaypointReached = new UnityEvent();

        public bool Active = false;
        
        public void OnTriggerEnter(Collider other)
        {
            if (!Active)
                return;


            m_OnWaypointReached.Invoke();
        }

        public void OnTriggerExit(Collider other)
        {
            if (!Active)
                return;

            SendMessageUpwards("RecieveWaypointEvent", name);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
    }
}