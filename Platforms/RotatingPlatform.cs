using ECM2.Components;
using UnityEngine;

namespace Platforms
{
    public class RotatingPlatform : PlatformMovement
    {
        [SerializeField] private float _rotationSpeedX = 0.0f;
        [SerializeField] private float _rotationSpeedY = 30.0f;
        [SerializeField] private float _rotationSpeedZ = 0.0f;

        protected override void OnMove()
        {
            rotation *= Quaternion.Euler(_rotationSpeedX * Time.deltaTime, _rotationSpeedY * Time.deltaTime, _rotationSpeedZ * Time.deltaTime);
        }
    }
}