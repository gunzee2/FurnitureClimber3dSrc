using System;
using System.Text;
using Animations;
using Cysharp.Threading.Tasks;
using Managers;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Utilities;

namespace Replays
{
    public class ReplayDataPlayer : MonoBehaviour
    {

        [SerializeField] private DeveloperReplayContainer _developerReplayContainer;

        public double TotalTimeMilliSeconds => _replayData.totalTimeMilliSeconds;
        public bool UseDeveloperReplay => _developerReplayContainer.useDeveloperReplay;

        private ReplayData _replayData;
        private int _currentFrameCount = 0;
        private AnimatorState _previousAnimatorState = AnimatorState.None;
        private Animator _animator;
        private bool _canReplay = false;

        // Start is called before the first frame update
        void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
        
            _animator = GetComponent<Animator>();

            byte[] compress;
            if (_developerReplayContainer.useDeveloperReplay)
            {
                compress = _developerReplayContainer.DeveloperReplay;
            

            }else if (ES3.FileExists("replayDataCompress.raw"))
            {
            
                compress = ES3.LoadRawBytes("replayDataCompress.raw");
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        
            Debug.Log($"ReplayData File Size:{compress.Length} byte.");
        
            var json = Encoding.UTF8.GetString(DataOperation.Decompress(compress));
            _replayData = JsonUtility.FromJson<ReplayData>(json);
        
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PlayerStart)
                .Subscribe(_ =>
                {
                    _canReplay = true;
                }).AddTo(this);
        
            Debug.Log($"Replay Data Loaded. Total {_replayData.motionFrameDataList.Count} Frames.");
            Debug.Log($"Replay Data Loaded. TotalTime: {StopWatchManager.TimeSpanToStopwatchFormatString(TimeSpan.FromMilliseconds(_replayData.totalTimeMilliSeconds))} .");
        
            // 開発者リプレイとの対戦の場合のみ、勝利したか判定する
            MessageBroker.Default.Receive<TimeSpan>()
                .Where(_ => _developerReplayContainer.useDeveloperReplay)
                .Where(x => x.TotalMilliseconds < _replayData.totalTimeMilliSeconds)
                .Subscribe(_ =>
                {
                    MessageBroker.Default.Publish(GameManager.GlobalEvent.YouWin);
                }).AddTo(this);

            this.FixedUpdateAsObservable()
                .Where(_ => _canReplay)
                .Where(_ => _currentFrameCount < _replayData.motionFrameDataList.Count).Subscribe(
                    _ =>
                    {
                        var frameData = _replayData.motionFrameDataList[_currentFrameCount];
                        transform.position = frameData.position;
                        transform.rotation = frameData.rotation;

                        if (_previousAnimatorState != frameData.animatorState)
                        {
                            _animator.SetTriggerOneFrame(GetAnimatorTriggerString(frameData.animatorState));
                        }
                
                        _animator.SetBool("IsWalking", frameData.isWalking);
                        _animator.SetBool("IsCharging", frameData.isCharging);
                
                        //Debug.Log($"Frame {_currentFrameCount}, Pos => {frameData.position}, Rot => {frameData.rotation}");
                        _currentFrameCount += 1;

                        _previousAnimatorState = frameData.animatorState;

                    }).AddTo(this);
        }

        private static string GetAnimatorTriggerString(AnimatorState state)
        {
            var str = "";
            switch (state)
            {
                case AnimatorState.Idle:
                    str = "IdleTrigger";
                    break;
                case AnimatorState.Charging:
                    str = "ChargeTrigger";
                    break;
                case AnimatorState.Down:
                    str = "DownTrigger";
                    break;
                case AnimatorState.Falling:
                    str = "FallTrigger";
                    break;
                case AnimatorState.Jumping:
                    str = "JumpTrigger";
                    break;
                case AnimatorState.Attack:
                    str = "AttackTrigger";
                    break;
                case AnimatorState.Landing:
                    str = "LandTrigger";
                    break;
            }

            return str;
        }

    }
}