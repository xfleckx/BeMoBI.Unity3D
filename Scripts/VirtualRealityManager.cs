using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

namespace Assets.SNEED.Scripts
{
    public class VirtualRealityManager : MonoBehaviour
    {

        //VR Real World Dimension for MoCap Systems
        public float HighQualityZoneWidth = 8f;
        public float HighQualityZoneLength = 12f;
        public float BorderZoneWidth = 1f;

        [SerializeField]
        public List<EnvironmentController> AvailableEnvironments = new List<EnvironmentController>();

        /// <summary>
        /// Change the whole world to exactly one environment
        /// </summary>
        /// <param name="worldName"></param>
        public EnvironmentController ChangeWorld(string worldName)
        {
            AvailableEnvironments.ForEach((e) => e.gameObject.SetActive(false));

            if (AvailableEnvironments.Any((i) => i.Title.Equals(worldName)))
            {

                var enabledEnvironment = AvailableEnvironments.First((i) => i.Title.Equals(worldName));

                if (enabledEnvironment != null)
                    enabledEnvironment.gameObject.SetActive(true);

                return enabledEnvironment;
            }

            return null;
        }

        /// <summary>
        /// Combine multiple environments
        /// </summary>
        /// <param name="names"></param>
        public void CombineEnvironments(params string[] names)
        {
            AvailableEnvironments.ForEach((e) => e.gameObject.SetActive(false));

            foreach (var item in names)
            {
                var environment = this.AvailableEnvironments.First((i) => i.Title.Equals(item));
                if (environment != null)
                    environment.gameObject.SetActive(true);
            }
        }

    }
}
