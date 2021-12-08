using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Platforms
{
    /// <summary>
    /// 一定時間待機してから回転する足場
    /// </summary>
    public class RotatingPlatformSequential : MonoBehaviour 
    {
        [SerializeField] private float startDelay;
        [SerializeField] private float rotateDuration;
        [SerializeField] private float waitDuration;
        [SerializeField] private Ease easeType = Ease.InOutCubic;

        [SerializeField] private Vector3 rotateVector;
    
        private CancellationTokenSource _DownTweenCTS;

        private Rigidbody _rb;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _DownTweenCTS = new CancellationTokenSource();

            RotateSequence(_DownTweenCTS.Token).Forget();
        
        }

        private async UniTaskVoid RotateSequence(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(startDelay), cancellationToken: token);
                while (true)
                {
                    await _rb.DORotate(rotateVector, rotateDuration).SetUpdate(UpdateType.Fixed).SetRelative().SetEase(easeType).WithCancellation(token);
                    await UniTask.Delay(TimeSpan.FromSeconds(waitDuration), cancellationToken: token);
                }

            }
            catch (Exception)
            {
                Debug.Log("Rotate Cancel");
            }
        }
    
        private void OnDisable()
        {
            _DownTweenCTS.Cancel();
            _DownTweenCTS.Dispose();
        }
    }
}