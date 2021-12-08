using System;
using System.Diagnostics;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Managers
{
    public class StopWatchManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text stopWatchText;

        public TimeSpan stopWatchTime => _stopwatch.Elapsed;
    
        private Stopwatch _stopwatch;

        private void Awake()
        {
            _stopwatch = new Stopwatch();
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PlayerStart)
                .Subscribe(_ =>
                {
                    StartStopWatch();
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PlayerGoal)
                .Subscribe(_ =>
                {
                    StopStopWatch();
                    Debug.Log($"Player Goal Time => {_stopwatch.Elapsed}");
                    MessageBroker.Default.Publish(_stopwatch.Elapsed);
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PauseGame)
                .Subscribe(_ =>
                {
                    Debug.Log("pause");
                    StopStopWatch();
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.ResumeGame)
                .Subscribe(_ =>
                {
                    StartStopWatch();
                    Debug.Log("resume");
                }).AddTo(this);
            this.UpdateAsObservable().Where(_ => stopWatchTime.TotalMinutes >= 3f).Take(1).Subscribe(_ =>
            {
                MessageBroker.Default.Publish(GameManager.GlobalEvent.TimeOver);
            }).AddTo(this);

            _stopwatch.ObserveEveryValueChanged(x => x.Elapsed).Where(_ => stopWatchText).Subscribe(x =>
            {
                stopWatchText.text = x.ToString(@"hh\:mm\:ss\.fff");
            }).AddTo(this);
        }

        public static string TimeSpanToStopwatchFormatString(TimeSpan timeSpan)
        {
            return timeSpan.ToString(@"hh\:mm\:ss\.fff");
        }

        private void StartStopWatch()
        {
            _stopwatch.Start();
        }

        private void StopStopWatch()
        {
            _stopwatch.Stop();
        }
    }
}