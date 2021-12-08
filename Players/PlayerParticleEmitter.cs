using UniRx;
using UnityEngine;

namespace Players
{
    public class PlayerParticleEmitter : MonoBehaviour
    {
        private IPlayerEventProvider _playerEventProvider;

        [SerializeField] private ParticleSystem smokeParticleSystem;
        [SerializeField] private ParticleSystem attackHitParticleSystem;
        [SerializeField] private ParticleSystem groundedSmokeParticleSystem;
    

        private void Start()
        {
            _playerEventProvider = GetComponent<IPlayerEventProvider>();

            _playerEventProvider.OnDown.Subscribe(_ =>
            {
                smokeParticleSystem.Emit(10);
            }).AddTo(this);

            _playerEventProvider.OnJumpAttackHit.Subscribe(x =>
            {
                //attackHitParticleSystem.transform.position = x.point;
                attackHitParticleSystem.Play();
            }).AddTo(this);

            _playerEventProvider.OnLanded.Subscribe(_ =>
            {
                groundedSmokeParticleSystem.Emit(20);
            }).AddTo(this);
        }
    }
}
