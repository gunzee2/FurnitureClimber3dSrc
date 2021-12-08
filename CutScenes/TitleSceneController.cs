using System;
using System.Threading;
using Cinemachine;
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
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace CutScenes
{
    public class TitleSceneController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private Button _startButton;
        [SerializeField] private Button _vsDevButton;
        [SerializeField] private Button _deleteReplayButton;
        [SerializeField] private Button _showControlInfoButton;
        [SerializeField] private TMP_Text _infoText;

        [SerializeField] private PlayableDirector _sceneTransitionPlayableDirector;
        [SerializeField] private CanvasGroup _fadeCanvasGroup;
        [SerializeField] private CanvasGroup _titleCanvasGroup;
        [SerializeField] private CanvasGroup _infoPanelCanvasGroup;

        [SerializeField] private CinemachineVirtualCamera _lookAtCam;

        [SerializeField] private Button _backButton;

        private int infoTextClickNum = 0;


        private bool _isSceneLoaded = false;


    
        // Start is called before the first frame update
        void Start()
        {
            MasterAudio.StartPlaylistOnClip("titleScenePlayList", "artic_loop");
            MasterAudio.FadePlaylistToVolume(1, 1);
            LoadBestTime();
            this.UpdateAsObservable().Where(_ => EventSystem.current.currentSelectedGameObject == null).Subscribe(_ =>
                {
                    EventSystem.current.SetSelectedGameObject(_startButton.gameObject);
                })
                .AddTo(this);
        }

        public void ShowTitleUI()
        {
            ShowTitleUISequence(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void OnDisable()
        {
        }

        private void LoadBestTime()
        {
            if (!ES3.KeyExists("bestTimeMilliSeconds")) return;
            var bestTime = ES3.Load<double>("bestTimeMilliSeconds");

            _infoText.text = $"自己ベスト: {StopWatchManager.TimeSpanToStopwatchFormatString(TimeSpan.FromMilliseconds(bestTime))}";

        }

        private async UniTaskVoid ShowTitleUISequence(CancellationToken token)
        {
        
            _startButton.OnClickAsObservable()
                .Where(_ => !_isSceneLoaded)
                .Take(1)
                .Subscribe(_ => 
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    SceneTransitionSequence(token).Forget();
                }).AddTo(this);
            _vsDevButton.OnClickAsObservable()
                .Where(_ => !_isSceneLoaded)
                .Take(1)
                .Subscribe(_ => 
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    SceneManager.sceneLoaded += LoadDeveloperReplay;
                    SceneTransitionSequence(token).Forget();
                }).AddTo(this);
            _deleteReplayButton.OnClickAsObservable()
                .Where(_ => !_isSceneLoaded)
                .Subscribe(_ => { DeletePlayData(); }).AddTo(this);
            _showControlInfoButton.OnClickAsObservable()
                .Where(_ => !_isSceneLoaded)
                .ThrottleFirst(TimeSpan.FromSeconds(1f))
                .Subscribe(_ =>
                {
                    _infoPanelCanvasGroup.DOFade(1f, 0.5f);
                    _infoPanelCanvasGroup.interactable = true;
                    _infoPanelCanvasGroup.blocksRaycasts = true;
                    EventSystem.current.SetSelectedGameObject(_backButton.gameObject);
                }).AddTo(this);
            _backButton.OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(1f))
                .Subscribe(_ =>
                {
                    _infoPanelCanvasGroup.DOFade(0f, 0.5f);
                    _infoPanelCanvasGroup.interactable = false;
                    _infoPanelCanvasGroup.blocksRaycasts = false;
                    EventSystem.current.SetSelectedGameObject(_showControlInfoButton.gameObject);
                }).AddTo(this);

            _infoText.GetComponent<Button>().OnClickAsObservable()
                .Subscribe(_ =>
                {
                    infoTextClickNum += 1;
                    if (infoTextClickNum < 10) return;
                
                    DeletePlayData();
                    infoTextClickNum = 0;
                }).AddTo(this);
            await _canvasGroup.DOFade(1, 1f).WithCancellation(token);
        }

        private void DeletePlayData()
        {
            if (ES3.FileExists("replayDataCompress.raw"))
            {
                ES3.DeleteFile("replayDataCompress.raw");
                _infoText.text = "プレイデータを削除しました。";
            }
            else
            {
                _infoText.text = "プレイデータが存在しませんでした。";
            }
            if (ES3.KeyExists("bestTimeMilliSeconds")) return;
            ES3.DeleteKey("bestTimeMilliSeconds");
        }

        private async UniTaskVoid SceneTransitionSequence(CancellationToken token)
        {
            _isSceneLoaded = true;

            await _titleCanvasGroup.DOFade(0, 0.25f).WithCancellation(token);
        
            _sceneTransitionPlayableDirector.Play();
        
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);

            await _fadeCanvasGroup.DOFade(1, 1f).WithCancellation(token);
        
            await SceneManager.LoadSceneAsync (sceneBuildIndex: 1).WithCancellation(token);
        
        


        }

        private void LoadDeveloperReplay(Scene arg0, LoadSceneMode arg1)
        {
            GameObject.Find("ReplayGorilla").GetComponent<DeveloperReplayContainer>().LoadDeveloperReplay();

            SceneManager.sceneLoaded -= LoadDeveloperReplay;
        }


        public void DisableLookAtPlayer()
        {
            _lookAtCam.LookAt = null;
        }
    
    }
}