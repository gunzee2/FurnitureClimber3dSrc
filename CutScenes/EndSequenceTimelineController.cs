using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using DG.Tweening;
using Managers;
using Replays;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace CutScenes
{
    public class EndSequenceTimelineController : MonoBehaviour
    {
        private PlayableDirector _director;
    
        [FormerlySerializedAs("readyCanvasGroup")] [SerializeField] private CanvasGroup resultCanvasGroup;
        [SerializeField] private CanvasGroup timeCanvasGroup;
        [FormerlySerializedAs("readyText")] [SerializeField] private TMP_Text goalText;
        [SerializeField] private TMP_Text newRecordText;
        [SerializeField] private TMP_Text youWinText;
        [SerializeField] private TMP_Text goalTimeText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button titleButton;
        [SerializeField] private Button tweetButton;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private DeveloperReplayContainer _developerReplayContainer;
    

        private bool _isNewRecord = false;
        private bool _isYouWin = false;
        private bool _isSceneLoaded = false;


        [SerializeField]private Volume volume;
        private VolumeProfile _volumeProfile;
    
        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
            _volumeProfile = volume.profile;
        }

        private void Start()
        {
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PlayerGoal)
                .Subscribe(_ =>
                {
                    DepthOfField dof;
                    if (_volumeProfile.TryGet(out dof))
                    {
                        DOVirtual.Float(10f, 3f, 1f, value =>
                        {
                            dof.focusDistance.Override(value);
                        });
                    }
                    _director.Play();
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.GotNewRecord)
                .Subscribe(_ =>
                {
                    _isNewRecord = true;
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.YouWin)
                .Subscribe(_ =>
                {
                    _isYouWin = true;
                }).AddTo(this);
            MessageBroker.Default.Receive<TimeSpan>()
                .Subscribe(x =>
                {
                    goalTimeText.text = StopWatchManager.TimeSpanToStopwatchFormatString(x);
                }).AddTo(this);
        }

        public void EndSequence()
        {
            PlayEndSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid PlayEndSequence(CancellationToken token)
        {
            resultCanvasGroup.alpha = 1;
            resultCanvasGroup.interactable = true;
            resultCanvasGroup.blocksRaycasts = true;
            goalText.transform.localScale = Vector3.zero;
            retryButton.transform.localScale = Vector3.zero;
            titleButton.transform.localScale = Vector3.zero;
            tweetButton.transform.localScale = Vector3.zero;
        
            MessageBroker.Default.Publish(GameManager.GlobalEvent.SaveReplayData);
        
            timeCanvasGroup.DOFade(0, 0.5f);
            await goalText.transform.DOScale(new Vector3(2,2,2), 0.5f).SetEase(Ease.OutElastic).WithCancellation(token);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
            MasterAudio.PlaySound("clapHands");
            await goalTimeText.transform.DOScale(new Vector3(1.5f,1.5f,1.5f), 0.5f).SetEase(Ease.OutElastic).WithCancellation(token);
            if (_isNewRecord)
            {
                MasterAudio.PlaySound("quickTransition");
                await newRecordText.transform.DOScale(new Vector3(1f,1f,1f), 0.25f).SetEase(Ease.OutElastic).WithCancellation(token);
            }
            if (_isYouWin)
            {
                MasterAudio.PlaySound("quickTransition");
                await youWinText.transform.DOScale(new Vector3(1f,1f,1f), 0.25f).SetEase(Ease.OutElastic).WithCancellation(token);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        
        
            retryButton.OnClickAsObservable()
                .Where(_ => !_isSceneLoaded)
                .Take(1)
                .Subscribe(_ => 
                {
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.RestartGame);
                }).AddTo(this);
            titleButton.OnClickAsObservable()
                .Where(_ => !_isSceneLoaded)
                .Take(1)
                .Subscribe(_ => 
                {
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.LoadTitleScene);
                }).AddTo(this);
            tweetButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ => 
                {
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.TakeScreenShotResult);
                }).AddTo(this);
        
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        
            MasterAudio.PlaySound("quickTransition");
            await retryButton.transform.DOScale(new Vector3(1f,1f,1f), 0.25f).SetEase(Ease.OutElastic).WithCancellation(token);
            MasterAudio.PlaySound("quickTransition");
            await titleButton.transform.DOScale(new Vector3(1f,1f,1f), 0.25f).SetEase(Ease.OutElastic).WithCancellation(token);
            MasterAudio.PlaySound("quickTransition");
            await tweetButton.transform.DOScale(new Vector3(1f,1f,1f), 0.25f).SetEase(Ease.OutElastic).WithCancellation(token);
        
            // 変なところをクリックしたらコントローラのUIフォーカスが外れるための措置
            this.UpdateAsObservable().Where(_ => EventSystem.current.currentSelectedGameObject == null).Subscribe(_ =>
                {
                    EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
                })
                .AddTo(this);
        
        }
    }
}