using Managers;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class MenuPanelUIPresenter : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button checkPointButton;
        [SerializeField] private Button settingsButton;
        [FormerlySerializedAs("titleButton")] [SerializeField] private Button restartButton;

        [SerializeField] private CanvasGroup canvasGroup;

        private IUIInput _uiInput;

        private bool _isLoadGameSelected = false;


        private bool _isSettingsMenuOpenning = false;
    
        // Start is called before the first frame update
        void Start()
        {
            _uiInput = GetComponent<IUIInput>();
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.OpenMenu)
                .Subscribe(_ =>
                {
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.PauseGame);
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    EventSystem.current.firstSelectedGameObject = backButton.gameObject;
                    EventSystem.current.SetSelectedGameObject(backButton.gameObject);
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.ResumeGame)
                .Subscribe(_ =>
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.OpenSettingsMenu)
                .Subscribe(_ =>
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    Debug.Log("is settings true");
                    _isSettingsMenuOpenning = true;
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.BackToMainMenu)
                .Subscribe(_ =>
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
                    Debug.Log("is settings false");
                    _isSettingsMenuOpenning = false;
                }).AddTo(this);

            _uiInput.OnCloseMenuPressed
                .Where(_ => !_isLoadGameSelected)
                .Where(_ => !_isSettingsMenuOpenning)
                .Subscribe(_ =>
                {
                    Debug.Log("on close menu input");
                    Cursor.lockState = CursorLockMode.Locked;
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.ResumeGame);
                }).AddTo(this);

            backButton
                .OnClickAsObservable()
                .Where(_ => !_isLoadGameSelected)
                .Subscribe(_ =>
                {
                    Debug.Log("on close menu");
                    Cursor.lockState = CursorLockMode.Locked;
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.ResumeGame);
                }).AddTo(this);

            settingsButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.OpenSettingsMenu);
                }).AddTo(this);
        
            checkPointButton
                .OnClickAsObservable()
                .Where(_ => !_isLoadGameSelected)
                .Subscribe(_ =>
                {
                    Debug.Log("press check point button");
                    Cursor.lockState = CursorLockMode.Locked;
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.ResumeGame);
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.MoveToCheckPoint);
                }).AddTo(this);
            restartButton 
                .OnClickAsObservable()
                .Where(_ => !_isLoadGameSelected)
                .Take(1)
                .Subscribe(_ =>
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.RestartGame);
                    _isLoadGameSelected = true;
                }).AddTo(this);
        
            // 変なところをクリックしたら戻るボタンにフォーカスを戻す(コントローラ対策)
            this.UpdateAsObservable().Where(_ => EventSystem.current.currentSelectedGameObject == null).Subscribe(_ =>
                {
                    EventSystem.current.SetSelectedGameObject(backButton.gameObject);
                })
                .AddTo(this);
        }


    }
}