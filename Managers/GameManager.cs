using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DarkTonic.MasterAudio;
using DG.Tweening;
using JetBrains.Annotations;
using Replays;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public enum GlobalEvent{
            None,
            PlayerStart,
            PlayerGoal,
            PauseGame,
            ResumeGame,
            SaveReplayData,
            TakeScreenShotPhotoMode,
            TakeScreenShotResult,
            SetupGame,
            MoveToCheckPoint,
            TimeOver,
            GotNewRecord,
            YouWin,
            ReplayDataSaved,
            OpenMenu,
            RestartGame,
            LoadTitleScene,
            TogglePhotoMode,
            OpenSettingsMenu,
            BackToMainMenu
        }
    
        private CancellationTokenSource _cts;

        [SerializeField] private CanvasGroup readyCanvasGroup;
        [SerializeField] private TMP_Text readyText;

        [SerializeField] private bool isPlayInstant = false;

        [SerializeField] private DeveloperReplayContainer _developerReplayContainer;
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        private bool _isSceneLoaded = false;
    
    
        private void Awake()
        {
            Application.targetFrameRate = 60;

            _cts = InitializeCancellationTokenSource(_cts);
        }
        private CancellationTokenSource InitializeCancellationTokenSource([CanBeNull] CancellationTokenSource cts)
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            return cts;
        }

        private void Start()
        {
            MessageBroker.Default.Publish(GlobalEvent.SetupGame);
        
            if (isPlayInstant)
            {
                Observable.TimerFrame(1).Subscribe(_ =>
                {
                    MessageBroker.Default.Publish(GlobalEvent.PlayerStart);
                }).AddTo(this);
            }
            MessageBroker.Default.Receive<TimeSpan>()
                .Subscribe(x =>
                {
                    if (ES3.KeyExists("bestTimeMilliSeconds"))
                    {
                        var bestTime = ES3.Load<double>("bestTimeMilliSeconds");
                    
                        Debug.Log($"compare best time {x.TotalMilliseconds} >= {bestTime}");
                        if (x.TotalMilliseconds >= bestTime) return;
                    
                        SaveBestTime(x);
                    }
                    else
                    {
                        SaveBestTime(x);
                    }
                }).AddTo(this);

            MessageBroker.Default.Receive<GlobalEvent>()
                .Where(x => x == GlobalEvent.RestartGame)
                .Subscribe(_ =>
                {
                    if (_developerReplayContainer)
                    {
                        if (_developerReplayContainer.useDeveloperReplay)
                        {
                            SceneManager.sceneLoaded += LoadDeveloperReplay;
                        }
                    }
                    SceneTransitionSequence(1, this.GetCancellationTokenOnDestroy()).Forget();
                
                }).AddTo(this);
            MessageBroker.Default.Receive<GlobalEvent>()
                .Where(x => x == GlobalEvent.LoadTitleScene)
                .Subscribe(_ =>
                {
                    SceneTransitionSequence(0, this.GetCancellationTokenOnDestroy()).Forget();
                
                }).AddTo(this);
                
        }

    
        private async UniTaskVoid SceneTransitionSequence(int sceneNum, CancellationToken token)
        {
            _isSceneLoaded = true;
        
            if(fadeCanvasGroup)
                await fadeCanvasGroup.DOFade(1, 1f).SetUpdate(true).WithCancellation(token);
        
            await SceneManager.LoadSceneAsync (sceneBuildIndex: sceneNum).WithCancellation(token);

        }
        private void SaveBestTime(TimeSpan bestTime)
        {
            MessageBroker.Default.Publish(GameManager.GlobalEvent.GotNewRecord);
            ES3.Save("bestTimeMilliSeconds", bestTime.TotalMilliseconds);
        }

        /// <summary>
        /// Timeline Signalから呼び出し
        /// </summary>
        public void PlayerStart()
        {
            PlayerStartSequence(_cts.Token).Forget();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    
        private void LoadDeveloperReplay(Scene arg0, LoadSceneMode arg1)
        {
            GameObject.Find("ReplayGorilla").GetComponent<DeveloperReplayContainer>().LoadDeveloperReplay();

            SceneManager.sceneLoaded -= LoadDeveloperReplay;
        }

        private async UniTaskVoid PlayerStartSequence(CancellationToken token)
        {
            readyText.transform.localScale = Vector3.zero;
            readyCanvasGroup.alpha = 1;
        
            MasterAudio.PlaySound("ready");
            await readyText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutElastic).WithCancellation(token);
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: token);
            await readyText.transform.DOScale(Vector3.zero, 0.2f).WithCancellation(token);
            await UniTask.Delay(TimeSpan.FromSeconds(0.6f), cancellationToken: token);
        
            readyText.text = "GO!!";
            MessageBroker.Default.Publish(GlobalEvent.PlayerStart);
        
            MasterAudio.PlaySound("go");
            await readyText.transform.DOScale(new Vector3(2,2,2), 0.25f).SetEase(Ease.OutQuint).WithCancellation(token);
            readyCanvasGroup.DOFade(0, 0.5f);

        }
    }
}