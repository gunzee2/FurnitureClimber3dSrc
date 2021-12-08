using ECM2.Components;
using UniRx;
using UniRx.Triggers;
using UnityEditor;
using UnityEngine;

namespace Platforms
{
    /// <summary>
    /// プレイヤーがトリガーに入ったら動き始める足場
    /// </summary>
    public class TriggerMovingPlatform : PlatformMovement
    {
        public float speed;
        public Vector3 offset;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;

        private bool _isMoveStart = false;
        private float moveStartTime;

        [SerializeField] private GameObject triggerSoundEventGO;
    
    
        private static float EaseInOut(float time, float duration)
        {
            return -0.5f * (Mathf.Cos(Mathf.PI * time / duration) - 1.0f);
        }

        protected override void OnStart()
        {
            base.OnStart();
            _startPosition = transform.position;
            _targetPosition = _startPosition + offset;

            this.OnTriggerEnterAsObservable().Where(x => x.CompareTag("Player")).Where(_ => !_isMoveStart).Subscribe(_ =>
            {
                _isMoveStart = true;
                moveStartTime = Time.time;
                if(triggerSoundEventGO != null) triggerSoundEventGO.SetActive(true);
            }).AddTo(this);

        }
    

        protected override void OnMove()
        {
            if (!_isMoveStart) return;
        
            var moveTime = Vector3.Distance(_startPosition, _targetPosition) / Mathf.Max(speed, 0.0001f);

            var t = EaseInOut(Mathf.PingPong(Time.time - moveStartTime, moveTime), moveTime);

            position = Vector3.Lerp(_startPosition, _targetPosition, t);
        }
    
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            if (EditorApplication.isPlaying)
            {
                Gizmos.DrawLine(_startPosition, _targetPosition);
            }
            else
            {
                Gizmos.DrawLine(transform.position, transform.position + offset);
            }
        }
#endif
    }
}
