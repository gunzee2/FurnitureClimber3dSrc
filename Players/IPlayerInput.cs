using System;
using UniRx;
using UnityEngine;

namespace Players
{
    public interface IPlayerInput 
    {
        public IReadOnlyReactiveProperty<Vector2> Move { get; }
        public IReadOnlyReactiveProperty<bool> JumpCharge { get; }
        public IReadOnlyReactiveProperty<bool> JumpAttack { get; }
        public IReadOnlyReactiveProperty<bool> Sprint { get; }

        public IReadOnlyReactiveProperty<bool> AnalogMovement { get; }
		
        public IObservable<Unit> OnOpenMenuPressed { get; }
        public IObservable<Unit> OnOpenPhotoModePressed { get; }

    }
}