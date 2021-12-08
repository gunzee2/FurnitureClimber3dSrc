using UniRx;
using UnityEngine;

namespace Managers
{
    public class TimeScaleManager : MonoBehaviour
    {
        private float _originalTimeScale = 1;
        void Start()
        {
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PauseGame)
                .Subscribe(_ =>
                {
                    _originalTimeScale = Time.deltaTime;

                    Time.timeScale = 0;
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.ResumeGame)
                .Subscribe(_ =>
                {
                    Time.timeScale = 1;
                }).AddTo(this);
        
        }
    }
}