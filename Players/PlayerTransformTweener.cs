using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ECM2.Components;
using UnityEngine;

namespace Players
{
    public class PlayerTransformTweener : MonoBehaviour
    {
        [SerializeField] private Transform tweenTransform;
        [SerializeField] private Vector3 tweenScaleSides = Vector3.one;
        [SerializeField] private Vector3 tweenScaleTop = Vector3.one;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private int vibrato;


        private Vector3 initialScale;
    
        private IPlayerEventProvider _playerEventProvider;

        private CancellationTokenSource _downTweenCts;
    
        private void Awake()
        {
            _playerEventProvider = GetComponent<IPlayerEventProvider>();
            initialScale = transform.localScale;
        }

        private void Start()
        {
            _downTweenCts = new CancellationTokenSource();

            DownTweenSequence(_downTweenCts.Token).Forget();

        }

        private void OnDisable()
        {
            _downTweenCts.Cancel();
            _downTweenCts.Dispose();
        }

        private async UniTaskVoid DownTweenSequence(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    var movementHit = await _playerEventProvider.OnDown.ToUniTask(true, token);

                    switch (movementHit.hitLocation)
                    {
                        case CapsuleHitLocation.Top:
                            await tweenTransform.DOPunchScale(tweenScaleTop, duration, vibrato).WithCancellation(token);
                            break;
                        case CapsuleHitLocation.Sides:
                            await tweenTransform.DOPunchScale(tweenScaleSides, duration, vibrato).WithCancellation(token);
                            break;
                        case CapsuleHitLocation.Bottom:
                            break;
                    }
                
                    tweenTransform.localScale = initialScale;
                }
            }
            catch (Exception)
            {
                Debug.Log("Cancel Down Tween");
            }
        }

    }
}