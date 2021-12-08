using Managers;
using Settings;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SettingsPanelUIPresenter : SerializedMonoBehaviour 
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button backButton;
        [SerializeField] private Button reverseXButton;
        [SerializeField] private Button reverseYButton;
        [SerializeField] private Button postEffectButton;
        [SerializeField] private TMP_Text reverseXButtonText;
        [SerializeField] private TMP_Text reverseYButtonText;
        [SerializeField] private TMP_Text postEffectButtonText;
        [SerializeField] private IUIInput _uiInput;
    

        private BoolReactiveProperty isReverseX = new BoolReactiveProperty(false);
        private BoolReactiveProperty isReverseY = new BoolReactiveProperty(false);
        private BoolReactiveProperty isPostEffectOn = new BoolReactiveProperty(true);

        private bool isOpening = false;
    
        // Start is called before the first frame update
        void Start()
        {
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.OpenSettingsMenu)
                .Subscribe(_ =>
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    EventSystem.current.SetSelectedGameObject(backButton.gameObject);
                    isOpening = true;
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.BackToMainMenu)
                .Subscribe(_ =>
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    isOpening = false;
                }).AddTo(this);
            isReverseX.Subscribe(x =>
            {
                const string reverseXText = "リバースカメラ(左右)：";
                if (x)
                {
                    reverseXButtonText.text = reverseXText + "ON";
                }
                else
                {
                    reverseXButtonText.text = reverseXText + "OFF";
                }
            
            }).AddTo(this);
            isReverseY.Subscribe(x =>
            {
                const string reverseYText = "リバースカメラ(上下)：";
                if (x)
                {
                    reverseYButtonText.text = reverseYText + "ON";
                }
                else
                {
                    reverseYButtonText.text = reverseYText + "OFF";
                }
            }).AddTo(this);
            isPostEffectOn.Subscribe(x =>
            {
                const string postEffectText = "ポストエフェクト：";
                if (x)
                {
                    postEffectButtonText.text = postEffectText + "ON";
                }
                else
                {
                    postEffectButtonText.text = postEffectText + "OFF";
                }
            }).AddTo(this);
        
            MessageBroker.Default.Receive<SettingsData>()
                .Subscribe(x =>
                {
                    Debug.Log("Load Setting Data");
                    Debug.Log($"reverseX = {x.isReverseX}, reverseY = {x.isReverseY}, postEffect = {x.isApplyPostEffect}");
                    isReverseX.Value = x.isReverseX;
                    isReverseY.Value = x.isReverseY;
                    isPostEffectOn.Value = x.isApplyPostEffect;
                }).AddTo(this);
            reverseXButton.OnClickAsObservable().Subscribe(_ =>
            {
                isReverseX.Value = !isReverseX.Value;
            }).AddTo(this);
            reverseYButton.OnClickAsObservable().Subscribe(_ =>
            {
                isReverseY.Value = !isReverseY.Value;
            }).AddTo(this);
            postEffectButton.OnClickAsObservable().Subscribe(_ =>
            {
                isPostEffectOn.Value = !isPostEffectOn.Value;
            }).AddTo(this);
        
            backButton.OnClickAsObservable().Subscribe(_ => { BackToMainMenu(); }).AddTo(this);
            _uiInput.OnCloseMenuPressed
                .Where(_ => isOpening)
                .Subscribe(_ =>
                {
                    BackToMainMenu();
                }).AddTo(this);
        }

        private void BackToMainMenu()
        {
            MessageBroker.Default.Publish(new SettingsData(isReverseX.Value, isReverseY.Value, isPostEffectOn.Value));
            MessageBroker.Default.Publish(GameManager.GlobalEvent.BackToMainMenu);
        }
    }
}