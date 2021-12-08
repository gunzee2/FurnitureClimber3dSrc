using UniRx;
using UnityEngine;
namespace Cameras
{
    public interface ICameraMovementInput 
    {
        public IReadOnlyReactiveProperty<Vector2> Look { get; }
    }
}
