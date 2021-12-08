using Players;
using UnityEngine;

namespace StageObjects
{
    public class PowerUpItem : MonoBehaviour
    {
        [SerializeField] private float powerUpValue;
    
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerCharacter playerCharacter)) return;
        
            playerCharacter.jumpImpulse += powerUpValue;
            
            Destroy(gameObject);
        }
    }
}
