using DG.Tweening;
using Managers;
using UniRx;
using UnityEngine;

namespace UI
{
    public class InformationTextUIPresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
    
        // Start is called before the first frame update
        void Start()
        {
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.ReplayDataSaved)
                .Subscribe(_ =>
                {
                    var seq = DOTween.Sequence();
                    seq.Append(canvasGroup.DOFade(1, 5).SetEase(Ease.InOutFlash, 9, 0));
                    seq.Append(canvasGroup.DOFade(0, 1).SetEase(Ease.InCubic));
                    seq.Play();
                }).AddTo(this);
        }
    }
}
