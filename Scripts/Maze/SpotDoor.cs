using UnityEngine;
using System.Collections;

namespace Assets.SNEED.Mazes
{
    public class SpotDoor : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            SendMessageUpwards("DoorStepEntered", other, SendMessageOptions.RequireReceiver);
        }

    }
}