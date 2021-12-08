using Players;
using UnityEngine;

namespace Platforms
{
    public class PlanetController : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerController playerController))
            {
                playerController.SetPlanet(transform);
            }
        }
    }
}