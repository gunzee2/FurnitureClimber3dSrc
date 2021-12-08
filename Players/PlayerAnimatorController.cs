using Animations;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Utilities;

namespace Players
{
    public class PlayerAnimatorController : MonoBehaviour, IAnimatorStateProvider, IAnimatorDataProvider
    {
        [SerializeField] private Animator animator;

        private IPlayerEventProvider _playerEventProvider;

        public IReadOnlyReactiveProperty<AnimatorState> State => _state;
        [SerializeField] private AnimatorStateReactiveProperty _state = new AnimatorStateReactiveProperty(AnimatorState.Idle);

        public float Speed { get; private set; }
        public float ChargeRatio { get; private set; }

        void Awake()
        {
            _playerEventProvider = GetComponent<IPlayerEventProvider>();
        }

        private void Start()
        {
            _playerEventProvider.MoveDirection.Subscribe(x =>
            {
                if (_playerEventProvider.IsGrounded.Value)
                {
                    animator.SetFloat("Speed", x.sqrMagnitude);
                    Speed = x.sqrMagnitude;
                }
                else
                {
                    animator.SetFloat("Speed", 0);
                    Speed = 0;
                }
            }).AddTo(this);

            _playerEventProvider.Velocity.Subscribe(x =>
            {
                animator.SetFloat("VelocityY", x.y);
            }).AddTo(this);

            _playerEventProvider.IsGrounded.Subscribe(x =>
            {
                animator.SetBool("Grounded", x);
            
            }).AddTo(this);

            _playerEventProvider.ChargeRatio.Subscribe(x =>
            {
                animator.SetFloat("ChargeRatio", x);
                ChargeRatio = x;
            
                animator.SetBool("Charging", x > 0);
            }).AddTo(this);

            _playerEventProvider.OnJumpStart.Subscribe(x =>
            {
                animator.SetTriggerOneFrame("JumpTrigger");
            }).AddTo(this);


            _playerEventProvider.OnJumpAttack.Subscribe(_ =>
            {
                animator.SetTriggerOneFrame("JumpAttackTrigger");
            }).AddTo(this);

            _playerEventProvider.OnDown.Subscribe(_ =>
            {
                animator.SetTriggerOneFrame("DownTrigger");
            }).AddTo(this);
        
            #region ObservableStateMachineTrigger

            var stateMachineTrigger = animator.GetBehaviour<ObservableStateMachineTrigger>();

            stateMachineTrigger.OnStateEnterAsObservable()
                .Where(x => x.StateInfo.IsName("Idle Walk Run Blend"))
                .Subscribe(_ =>
                {
                    _state.Value = AnimatorState.Idle;
                }).AddTo(this);
            stateMachineTrigger.OnStateEnterAsObservable()
                .Where(x => x.StateInfo.IsName("ChargeBlendTree"))
                .Subscribe(_ =>
                {
                    _state.Value = AnimatorState.Charging;
                }).AddTo(this);
            stateMachineTrigger.OnStateEnterAsObservable()
                .Where(x => x.StateInfo.IsName("Jumping"))
                .Subscribe(_ =>
                {
                    _state.Value = AnimatorState.Jumping;
                }).AddTo(this);
            stateMachineTrigger.OnStateEnterAsObservable()
                .Where(x => x.StateInfo.IsName("Down"))
                .Subscribe(_ =>
                {
                    _state.Value = AnimatorState.Down;
                }).AddTo(this);
            stateMachineTrigger.OnStateEnterAsObservable()
                .Where(x => x.StateInfo.IsName("Falling"))
                .Subscribe(_ =>
                {
                    _state.Value = AnimatorState.Falling;
                }).AddTo(this);
            stateMachineTrigger.OnStateEnterAsObservable()
                .Where(x => x.StateInfo.IsName("JumpLand"))
                .Subscribe(_ =>
                {
                    _state.Value = AnimatorState.Landing;
                }).AddTo(this);
            stateMachineTrigger.OnStateEnterAsObservable()
                .Where(x => x.StateInfo.IsName("JumpingAttack"))
                .Subscribe(_ =>
                {
                    _state.Value = AnimatorState.Attack;
                }).AddTo(this);


            #endregion
        }
    }
}