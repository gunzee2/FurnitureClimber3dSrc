using System;
using UniRx;
using UnityEngine;

namespace Players
{
    public interface IPlayerEventProvider
    {
        public IReadOnlyReactiveProperty<Vector3> MoveDirection { get; }
        public IReadOnlyReactiveProperty<bool> IsGrounded { get; }
    
        public IReadOnlyReactiveProperty<Vector3> Velocity { get; }
        public IReadOnlyReactiveProperty<float> ChargeRatio { get; }
    
        public IObservable<float> OnJumpStart { get; }
        public IObservable<Unit> OnJumpAttack { get; }
        public IObservable<RaycastHit> OnJumpAttackHit { get; }
    
        public IObservable<PlayerController.HitEventData> OnCollisionEnterObject { get; }
    
        public IObservable<PlayerController.HitEventData> OnDown { get; }
        public IObservable<Unit> OnLanded { get; }
    }
}
