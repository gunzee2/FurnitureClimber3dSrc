using ECM2.Components;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Platforms
{
    /// <summary>
    /// プレーヤーがトリガーに入ったら回り始める足場
    /// </summary>
    public class TriggerRotatingPlatform : PlatformMovement
    {
        [SerializeField] private float _rotationSpeedX = 0.0f;
        [SerializeField] private float _rotationSpeedY = 30.0f;
        [SerializeField] private float _rotationSpeedZ = 0.0f;

        private bool _isMoveStart = false;

        protected override void OnStart()
        {
            base.OnStart();

            this.OnTriggerEnterAsObservable().Where(x => x.CompareTag("Player")).Where(_ => !_isMoveStart).Subscribe(_ =>
            {
                _isMoveStart = true;
            }).AddTo(this);

        }
    

        protected override void OnMove()
        {
            if (!_isMoveStart) return;
        
            rotation *= Quaternion.Euler(_rotationSpeedX * Time.deltaTime, _rotationSpeedY * Time.deltaTime, _rotationSpeedZ * Time.deltaTime);
        }
    }
}
