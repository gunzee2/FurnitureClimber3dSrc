using Managers;
using UniRx;
using UnityEngine;

namespace UI
{
    public class ReticlePanelUIPresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
    
    
        // Start is called before the first frame update
        void Start()
        {
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PlayerStart)
                .Subscribe(_ =>
                {
                    _canvasGroup.alpha = 1f;
                }).AddTo(this);
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
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PlayerGoal)
                .Subscribe(_ =>
                {
                    _canvasGroup.alpha = 0f;
                }).AddTo(this);
        
        }
    }
}
