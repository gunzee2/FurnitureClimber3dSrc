using UniRx;
namespace Animations
{
    public enum AnimatorState
    {
        None,
        Idle,
        Charging,
        Jumping,
        Falling,
        Down,
        Landing,
        Attack
    }

    [System.Serializable]
    public class AnimatorStateReactiveProperty : ReactiveProperty<AnimatorState>
    {
        public AnimatorStateReactiveProperty(){}
        public AnimatorStateReactiveProperty(AnimatorState initialValue) : base (initialValue) {}

    }
}