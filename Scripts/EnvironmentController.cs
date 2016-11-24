using UnityEngine;

namespace Assets.SNEED.Scripts
{
    /// <summary>
    /// A semantic component to mark a toplevel game object as a
    /// environment.
    /// </summary>
    public class EnvironmentController : MonoBehaviour
    {
        public string Title = "Name of the Environment";

        void Awake()
        {
            Title = gameObject.name;
        }
    }
}