using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.EventSystems;

namespace Assets.SNEED.Scripts
{
    [RequireComponent(typeof(Collider))]
    public class Waypoint : beMoBIBase, IEventSystemHandler
    {

        [SerializeField]
        public UnityEvent m_OnWaypointReached = new UnityEvent();

        public bool Active = false;
        
        
        void Start()
        {
            base.Initialize();
        }
        

        public void OnTriggerEnter(Collider other)
        {
            if (!Active)
                return;

            WriteMarker("Way point entered");

            m_OnWaypointReached.Invoke();
        }

        public void OnTriggerExit(Collider other)
        {
            if (!Active)
                return;

            WriteMarker("Way point exit");

            SendMessageUpwards("RecieveWaypointEvent", name);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
    }
}