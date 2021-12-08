using System;
using UniRx;
using UnityEngine;

namespace Players
{
    public class PlayerTimeScaler : MonoBehaviour
    {
        [SerializeField] private float timeScale;
        [SerializeField] private float duration;
    
        private IPlayerEventProvider _playerEventProvider;
        private IDisposable _disposable;

        void Start()
        {
            Time.timeScale = 1f;
        
            _playerEventProvider = GetComponent<IPlayerEventProvider>();

            _playerEventProvider.OnDown.Subscribe(x =>
            {
                _disposable?.Dispose();

                Time.timeScale = timeScale;
                _disposable = Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(_ =>
                {
                    Time.timeScale = 1f;
                }).AddTo(this);

            }).AddTo(this);
            _playerEventProvider.OnJumpAttackHit.Subscribe(x =>
            {
                _disposable?.Dispose();

                Time.timeScale = timeScale;
                _disposable = Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(_ =>
                {
                    Time.timeScale = 1f;
                }).AddTo(this);

            }).AddTo(this);
        }

    }
}
