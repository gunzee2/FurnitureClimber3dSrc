using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
namespace CutScenes
{
    public class PlayableDirectorPlayer : MonoBehaviour
    {
        private enum PlayState
        {
            Stopped,
            Playing,
            Paused,
        }

        [SerializeField] private PlayableDirector _director;
        [SerializeField] private float _initialTime;

        public float initialTime
        {
            get => _initialTime;
            set
            {
                _initialTime = Mathf.Clamp(value, 0, float.MaxValue);
                _director.initialTime = this.initialTime;
            }
        }

        [SerializeField]
        private float _timeScale = 1;

        public float timeScale
        {
            get => _timeScale;
            set
            {
                _timeScale = Mathf.Clamp(value, 0, float.MaxValue);

                if (this.playableGraph.IsValid())
                {
                    this.playableGraph.GetRootPlayable(0).SetSpeed(this.timeScale);
                }
            }
        }

        private int _completedLoops;
        public int CompletedLoops() => _completedLoops;

        public double time
        {
            get => _director.time;
            set
            {
                if (this.extrapolationMode == DirectorWrapMode.Loop)
                {
                    _completedLoops = (int) (value / this.duration);
                    _director.time = value - (this.duration * _completedLoops);
                }
                else
                {
                    if (value < 0)
                    {
                        _director.time = 0;
                    }
                    else if (this.duration <= value)
                    {
                        _director.time = this.duration;
                    }
                    else
                    {
                        _director.time = value;
                    }
                }

                _director.Evaluate();

                if (_state == PlayState.Stopped)
                {
                    if (this.extrapolationMode == DirectorWrapMode.Hold)
                    {
                        var timeCache = this.time;
                        _director.Stop();
                        _director.time = timeCache;
                    }

                    _state = PlayState.Paused;
                }
            }
        }

        public double normalizedTime
        {
            get => this.time / this.duration;
            set => this.time = value * this.duration;
        }

        public double duration => _director.duration;

        private PlayState _state;
        public bool IsStopped => (_state == PlayState.Stopped);
        public bool IsPlaying => (_state == PlayState.Playing);
        public bool IsPaused => (_state == PlayState.Paused);

        public DirectorWrapMode extrapolationMode => _director.extrapolationMode;
        public PlayableGraph playableGraph => _director.playableGraph;

        private readonly Subject<Unit> _start = new Subject<Unit>();
        private readonly Subject<Unit> _play = new Subject<Unit>();
        private readonly Subject<Unit> _pause = new Subject<Unit>();
        private readonly Subject<Unit> _stepComplete = new Subject<Unit>();
        private readonly Subject<Unit> _complete = new Subject<Unit>();
        private readonly Subject<Unit> _kill = new Subject<Unit>();

        public IObservable<Unit> OnStartAsObservable() => _start;
        public IObservable<Unit> OnPlayAsObservable() => _play;
        public IObservable<Unit> OnPauseAsObservable() => _pause;
        public IObservable<Unit> OnStepCompleteAsObservable() => _stepComplete;
        public IObservable<Unit> OnCompleteAsObservable() => _complete;
        public IObservable<Unit> OnKillAsObservable() => _kill;

        void OnDestroy()
        {
            _start.OnCompleted();
            _start.Dispose();

            _play.OnCompleted();
            _play.Dispose();

            _pause.OnCompleted();
            _pause.Dispose();

            _stepComplete.OnCompleted();
            _stepComplete.Dispose();

            _complete.OnCompleted();
            _complete.Dispose();

            _kill.OnCompleted();
            _kill.Dispose();
        }

        void Start()
        {
            if (_director.state == UnityEngine.Playables.PlayState.Playing)
            {
                _state = PlayState.Playing;
                this.timeScale = this.timeScale;

                _start.OnNext(Unit.Default);
                _play.OnNext(Unit.Default);
            }

            // 再生完了のチェック
            {
                // Wrap Mode(extrapolationMode)がNoneの場合
                _director.stopped += _ =>
                {
                    if (_state != PlayState.Playing) return;

                    _state = PlayState.Stopped;
                    _completedLoops = 1;
                    _stepComplete.OnNext(Unit.Default);
                    _complete.OnNext(Unit.Default);
                    _kill.OnNext(Unit.Default);
                };

                // Hold/Loopの場合
                this.CheckCompleteTask(this.GetCancellationTokenOnDestroy()).Forget();
            }
        }

        private async UniTaskVoid CheckCompleteTask(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    var prevTime = this.time;

                    await UniTask.Yield(PlayerLoopTiming.LastUpdate, cancellationToken);
                    var currentTime = this.time;

                    if ((this.extrapolationMode == DirectorWrapMode.Loop) && currentTime < prevTime)
                    {
                        _completedLoops++;
                        _stepComplete.OnNext(Unit.Default);
                    }
                }

                if (this.playableGraph.IsValid() == false) continue;
                if (this.extrapolationMode != DirectorWrapMode.Hold) continue;
                if (this.time < this.duration) continue;
                if (_state == PlayState.Stopped) continue;

                _state = PlayState.Stopped;
                _completedLoops = 1;
                _stepComplete.OnNext(Unit.Default);
                _complete.OnNext(Unit.Default);
                _kill.OnNext(Unit.Default);
            }
        }

        public void Play()
        {
            if (_state == PlayState.Playing) return;

            switch (_state)
            {
                case PlayState.Stopped:
                    {
                        _completedLoops = 0;
                        _director.time = this.initialTime;
                        _director.Evaluate();
                        break;
                    }
                case PlayState.Playing: return;
                case PlayState.Paused:
                    {
                        _director.Evaluate();
                        break;
                    }
                default: throw new ArgumentOutOfRangeException();
            }

            var prevState = _state;
            _state = PlayState.Playing;

            _director.Play();
            this.timeScale = this.timeScale;

            if (prevState == PlayState.Stopped)
            {
                _start.OnNext(Unit.Default);
            }

            _play.OnNext(Unit.Default);
        }

        public void Pause()
        {
            if (_state != PlayState.Playing) return;
            _state = PlayState.Paused;

            var timeCache = this.time;
            _director.Stop();
            _director.time = timeCache;

            _pause.OnNext(Unit.Default);
        }

        public void Kill(bool complete = false)
        {
            if (_state == PlayState.Stopped) return;

            _state = PlayState.Stopped;

            if (complete && (this.extrapolationMode != DirectorWrapMode.Loop))
            {
                _director.Stop();
                _director.time = _director.duration;
                _director.Evaluate();

                _completedLoops = 1;
                _complete.OnNext(Unit.Default);
            }
            else
            {
                var timeCache = this.time;
                _director.Stop();
                _director.time = timeCache;
            }

            _kill.OnNext(Unit.Default);
        }

        public void Complete()
        {
            if (_director.extrapolationMode == DirectorWrapMode.Loop) return;
            if (_state == PlayState.Stopped) return;
            _state = PlayState.Stopped;

            _director.Stop();
            _director.time = _director.duration;
            _director.Evaluate();

            _completedLoops = 1;
            _complete.OnNext(Unit.Default);
            _kill.OnNext(Unit.Default);
        }
    }
}