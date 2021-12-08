using Managers;
using UniRx;
using UnityEngine;

namespace UI
{
    public class PauseGameUIPresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

    
        // Start is called before the first frame update
        void Start()
        {
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PauseGame)
                .Subscribe(_ =>
                {
                    _canvasGroup.alpha = 0;
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.ResumeGame)
                .Subscribe(_ =>
                {
                    _canvasGroup.alpha = 1;
                }).AddTo(this);
        
        }
    }
}
