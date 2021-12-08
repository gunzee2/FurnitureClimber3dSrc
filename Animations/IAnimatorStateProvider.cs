using UniRx;
namespace Animations
{
    public interface IAnimatorStateProvider
    {
        public IReadOnlyReactiveProperty<AnimatorState> State { get; }
    }
}
