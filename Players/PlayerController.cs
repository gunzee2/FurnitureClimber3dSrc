using System;
using System.Threading;
using Animations;
using Cysharp.Threading.Tasks;
using ECM2.Characters;
using ECM2.Common;
using ECM2.Components;
using JetBrains.Annotations;
using Managers;
using Sirenix.OdinInspector;
using StageObjects;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Players
{
    public class PlayerController : MonoBehaviour,IPlayerEventProvider
    {
        public IReadOnlyReactiveProperty<Vector3> MoveDirection => _moveDirectionReactiveProperty;
        private Vector3ReactiveProperty _moveDirectionReactiveProperty = new Vector3ReactiveProperty();
    
        public IReadOnlyReactiveProperty<bool> IsGrounded => _isGroundedReactiveProperty;
        private BoolReactiveProperty _isGroundedReactiveProperty = new BoolReactiveProperty();
    
        public IReadOnlyReactiveProperty<Vector3> Velocity => _velocity;
        [ShowInInspector] [ReadOnly] private Vector3ReactiveProperty _velocity = new Vector3ReactiveProperty(Vector3.zero);
        public IReadOnlyReactiveProperty<float> ChargeRatio => _chargeRatio;
        [ShowInInspector][ReadOnly] private FloatReactiveProperty _chargeRatio = new FloatReactiveProperty(0);
    
        public IObservable<Unit> OnJumpAttack => _onJumpAttack;
        private readonly Subject<Unit> _onJumpAttack = new Subject<Unit>();
    
        public IObservable<RaycastHit> OnJumpAttackHit => _onJumpAttackHit;
        private readonly Subject<RaycastHit> _onJumpAttackHit = new Subject<RaycastHit>();
    
        public IObservable<HitEventData> OnCollisionEnterObject => _onCollisionEnterObject;
        private readonly Subject<HitEventData> _onCollisionEnterObject = new Subject<HitEventData>();
    
        public IObservable<HitEventData> OnDown => _onDown;
        private readonly Subject<HitEventData> _onDown = new Subject<HitEventData>();
    
        public IObservable<Unit> OnLanded => _onLanded;
        private readonly Subject<Unit> _onLanded = new Subject<Unit>();
    
        public IObservable<float> OnJumpStart => _onJumpStart;
        private readonly Subject<float> _onJumpStart = new Subject<float>();

        [SerializeField] private float jumpRechargeTime = 0.5f;

        [SerializeField][Range(0, 20)] private int chargeSpeed;
    
        [SerializeField] private Collider attackCollider;
    
        [SerializeField] private float maxChargeEnergy = 60;
    
        private IPlayerInput _playerInput;
        private PlayerCharacter _character;
        private GameObject _mainCamera;
        private bool _isJumpRecharging = false;
        private IAnimatorStateProvider _animatorStateProvider;
        private CancellationTokenSource _cts;
        private CapsuleHitLocation _hitLocationWhenDown;
        private CheckPointData _checkPointData;
    
        private float ChargeEnergy
        {
            get => _chargeEnergy;
            set
            {
                _chargeEnergy = value;
                _chargeRatio.Value = (_chargeEnergy / (float)maxChargeEnergy); // float変換する(int同士だと結果もintになる)
            }
        }
        private float _chargeEnergy;

        public struct HitEventData
        {
            public CapsuleHitLocation hitLocation;
            public bool hitGround;
            public Vector3 point;
            public Vector3 normal;
            public Collider collider;
            public Rigidbody rigidbody => collider ? collider.attachedRigidbody : null;
        }
    
        public void Awake()
        {
            _character = GetComponent<PlayerCharacter>();
            _playerInput = GetComponent<IPlayerInput>();
            _animatorStateProvider = GetComponent<IAnimatorStateProvider>();

            _cts = InitializeCancellationTokenSource(_cts);

            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
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
        
            #region チェックポイント関連
            _checkPointData = new CheckPointData { Position = transform.position, Rotation = transform.rotation };
        
            MessageBroker.Default.Receive<CheckPointData>()
                .Subscribe(x =>
                {
                    _checkPointData = x;
                }).AddTo(this);
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.MoveToCheckPoint)
                .Subscribe(_ =>
                {
                    Debug.Log($"Check Point Data => {_checkPointData.Position}, {_checkPointData.Rotation}");
                    _character.TeleportPosition(_checkPointData.Position);
                    _character.SetYaw( _checkPointData.Rotation.eulerAngles.y );
                    _character.LaunchCharacter(Vector3.zero, true, true);
                }).AddTo(this);
            #endregion
        
            // 移動処理
            this.UpdateAsObservable()
                .Select(_ => _playerInput.Move.Value)
                .Where(_ => _chargeEnergy <= 0)
                .Where(_ => IsGrounded.Value)
                .Where(_ => _animatorStateProvider.State.Value == AnimatorState.Idle)
                .Where(_ => !_isJumpRecharging)
                .Subscribe(vec =>
                {
                    var moveDirection = new Vector3
                    {
                        x = vec.x,
                        y = 0.0f,
                        z = vec.y
                    };
                
                    moveDirection = moveDirection.relativeTo(_mainCamera.transform, _character.GetUpVector());
                
                    _character.SetMovementDirection(moveDirection);

                    _moveDirectionReactiveProperty.Value = moveDirection;
                }).AddTo(this);
        
            _character.ObserveEveryValueChanged(x => x.GetVelocity())
                .Subscribe(x =>
                {
                    _velocity.Value = x;
                }).AddTo(this);

            #region 着地
            // 着地を監視してReactivePropertyに変換
            // 毎フレーム監視したいためObserveEveryValueChangedを使わない
            this.UpdateAsObservable()
                .Select(_ => _character.IsOnGround())
                .Subscribe(x =>
                {
                    _isGroundedReactiveProperty.SetValueAndForceNotify(x);
                }).AddTo(this);
        
            // 着地イベント
            Observable.FromEvent<Character.LandedEventHandler>(
                    h => () => h(),
                    h => _character.Landed += h,
                    h => _character.Landed -= h)
                .Skip(1) // ゲーム開始、既に着地している状態から始まった時に着地イベントが実行されないように1フレームスキップする
                .Subscribe(_ =>
                {
                    _onLanded.OnNext(Unit.Default);
                })
                .AddTo(this);

            // 着地した瞬間に、しゃがみをやめる
            _isGroundedReactiveProperty
                .Pairwise()
                .Where(x => x.Previous != x.Current)
                .Select(x => x.Current)
                .Where(x => x)
                .Subscribe(x =>
                {
                    _character.StopCrouching();
                    ChargeEnergy = 0;
                
                    _character.SetVelocity(Vector3.zero);
                    if (_character.GetMovementDirection().magnitude > 0) _character.SetMovementDirection(Vector3.zero);
                
                    Debug.Log($"is Grounded => {x}");
                }).AddTo(this);
            #endregion

            #region 衝突
            // 衝突イベント
            // ジャンプ中とジャンプ攻撃中のみ衝突でダウンして吹き飛ぶ
            Observable.FromEvent<Character.MovementHitEventHandler, MovementHit>(
                    h => (ref MovementHit hit) => h(hit),
                    h => _character.MovementHit += h,
                    h => _character.MovementHit -= h)
                .Where(x => !x.hitWalkableGround)
                .Where(x => x.hitLocation != CapsuleHitLocation.Bottom)
                .Where(_ => _animatorStateProvider.State.Value == AnimatorState.Jumping || _animatorStateProvider.State.Value == AnimatorState.Attack)
                .Subscribe(x =>
                {
                    var hit = new HitEventData
                    {
                        collider = x.collider,
                        hitGround = x.hitGround,
                        hitLocation = x.hitLocation,
                        normal = x.normal,
                        point = x.point
                    };
                    _onDown.OnNext(hit);
                    Reflect(hit);
                })
                .AddTo(this);

            // トゲトゲなど、ダメージオブジェクトに衝突した時にも吹き飛ぶようにする
            this.OnCollisionEnterAsObservable()
                .Where(x => x.collider.CompareTag("DamageObject"))
                .Subscribe(x =>
                {
                    var hit = new HitEventData
                    {
                        collider = x.collider,
                        hitGround = false,
                        hitLocation = CapsuleHitLocation.Sides,
                        normal = x.contacts[0].normal,
                        point = x.contacts[0].point
                    };
                    CollisionDamageObject(hit, x.rigidbody.velocity);
                }).AddTo(this);
            #endregion

            #region キャラクターの向き
            // ジャンプ中は移動方向に向く
            this.UpdateAsObservable()
                .Where(_ => _animatorStateProvider.State.Value == AnimatorState.Jumping)
                .Subscribe(_ =>
                {
                    _character.RotateTo(_character.GetVelocity(), true);
                }).AddTo(this);
            // 障害物に衝突してダウンしたら移動方向と逆に向く(ぶつかった方向を向きながらノックバックしてダウン)
            this.UpdateAsObservable()
                .Where(_ => _animatorStateProvider.State.Value == AnimatorState.Down)
                .Where(_ => _hitLocationWhenDown == CapsuleHitLocation.Sides)
                .Subscribe(_ =>
                {
                    _character.RotateTo(-_character.GetVelocity(), true);
                }).AddTo(this);
            #endregion

            #region メニュー入力
            _playerInput.OnOpenMenuPressed.Subscribe(_ =>
            {
                MessageBroker.Default.Publish(GameManager.GlobalEvent.OpenMenu);
            }).AddTo(this);
            _playerInput.OnOpenPhotoModePressed.Subscribe(_ =>
            {
                MessageBroker.Default.Publish(GameManager.GlobalEvent.TogglePhotoMode);
            }).AddTo(this);
            #endregion

            #region ジャンプチャージ
            var chargeReleaseStream = _playerInput.JumpCharge
                .Pairwise()
                .Where(x => x.Previous == true && x.Current == false)
                .AsUnitObservable();

            var chargeCancelStream = _animatorStateProvider.State
                .Pairwise()
                .Where(x => x.Previous == AnimatorState.Charging && x.Current != AnimatorState.Charging)
                .AsUnitObservable();
            
            // チャージを終了してジャンプ開始
            chargeReleaseStream
                .Where(_ => _animatorStateProvider.State.Value == AnimatorState.Charging)
                .Where(_ => ChargeEnergy > 0)
                .Subscribe(_ =>
                {
                    _onJumpStart.OnNext(ChargeRatio.Value);
        
                    DoJump(ChargeRatio.Value);
            
                    _isJumpRecharging = true;
                    Observable.Timer(TimeSpan.FromSeconds(jumpRechargeTime)).Subscribe(_ =>
                    {
                        _isJumpRecharging = false;
                    }).AddTo(this);
                }).AddTo(this);

            chargeCancelStream
                .Subscribe(_ =>
                {
                    Debug.Log("Charge Cancel");
                    ChargeEnergy = 0;
                }).AddTo(this);

            _playerInput.JumpCharge
                .Where(_ => !_isJumpRecharging) // ジャンプ直後ではない時
                .Where(_ => _animatorStateProvider.State.Value == AnimatorState.Idle)
                .Where(x => x)
                .SelectMany(this.UpdateAsObservable())
                .TakeUntil(chargeReleaseStream.Amb(chargeCancelStream))
                .RepeatUntilDestroy(this)
                .Subscribe(_ =>
                {
                    if (_character.GetMovementDirection().magnitude > 0) _character.SetMovementDirection(Vector3.zero);
                
                    ChargeEnergy += (50 + 5 * chargeSpeed) * Time.deltaTime;
                
                    _character.RotateTo(_mainCamera.transform.forward, true);

                    if (ChargeEnergy > maxChargeEnergy) ChargeEnergy = maxChargeEnergy;
                }).AddTo(this);
            #endregion

            #region ジャンプ攻撃

            // ジャンプ中に攻撃ボタンとジャンプボタンの両方で攻撃が出るようにする
            var jumpAttackStream1 = _playerInput.JumpAttack.Where(x => x);
            var jumpAttackStream2 = _playerInput.JumpCharge.Where(x => x);
        
            Observable.Merge(jumpAttackStream1, jumpAttackStream2)
                .Where(_ => _animatorStateProvider.State.Value == AnimatorState.Jumping)
                .ThrottleFirst(TimeSpan.FromSeconds(0.3f))
                .Subscribe(_ =>
                {
                    _cts = InitializeCancellationTokenSource(_cts);
                    StartJumpAttack(_cts.Token).Forget();
                }).AddTo(this);

            // ジャンプ攻撃が障害物にヒットした
            attackCollider.OnTriggerEnterAsObservable()
                .Where(_ => _animatorStateProvider.State.Value == AnimatorState.Jumping ||
                            _animatorStateProvider.State.Value == AnimatorState.Falling ||
                            _animatorStateProvider.State.Value == AnimatorState.Attack
                )
                .Subscribe(_ =>
                {
                    DoJumpAttackReflect();
                }).AddTo(this);
            #endregion
        }

        private void CollisionDamageObject(HitEventData x, Vector3 velocity)
        {
            Debug.Log("collision damage object");
            velocity *= 2;
            velocity.y = 10f;
        
            _hitLocationWhenDown = x.hitLocation;
            _onDown.OnNext(x);
            KnockBack(velocity, x);
        }

        private void Reflect(HitEventData x)
        {
            Debug.Log("collision hit");

            var reflectVector = Vector3.Reflect(_character.GetForwardVector(), x.normal) * 5f;
            reflectVector.y = 10f;
        
            KnockBack(reflectVector, x);
        }
    
        private void KnockBack(Vector3 launchVector, HitEventData hitEventData)
        {
            _character.LaunchCharacter(launchVector , true, true);
            _character.PauseGroundConstraint(0.2f);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async UniTaskVoid StartJumpAttack(CancellationToken token)
        {
            _onJumpAttack.OnNext(Unit.Default);

            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: token);
        
            attackCollider.enabled = true;
        
            await UniTask.Delay(TimeSpan.FromSeconds(0.45f), cancellationToken: token);
        
            attackCollider.enabled = false;
        
        }
    
        private void DoJumpAttackReflect()
        {
            RaycastHit hit;
            var layermask = 1 << 0;
        
            if (!Physics.Raycast(attackCollider.transform.position, transform.forward, out hit, 2f, layermask)) return;
        
            _onJumpAttackHit.OnNext(hit);

            Debug.Log($"Wall Hit");

            var reflectVector = Vector3.Reflect(_character.GetVelocity(), hit.normal) * 0.5f;
            var reflectNormalized = Vector3.Reflect(_character.GetForwardVector(), hit.normal) * 9f;
            Debug.Log($"Reflect Vector => {reflectVector}");
            Debug.Log($"Reflect Normalized Vector => {reflectNormalized}");

            reflectVector.y = _character.GetVelocity().magnitude * 0.2f;
            reflectVector.y += 15f;

            reflectVector += reflectNormalized;

            _character.LaunchCharacter(reflectVector, true, true);
            _character.PauseGroundConstraint(0.2f);
        }

        private void DoJump(float ratio)
        {
            var force = (_mainCamera.transform.forward + Vector3.up * 0.75f).normalized *
                        (_character.jumpImpulse * ratio + 7.5f);
            _character.LaunchCharacter(force, true, false);
            _character.Crouch();
            _character.PauseGroundConstraint(0.25f);
            _isGroundedReactiveProperty.Value = false;
        }

        public void SetPlanet(Transform planetTransform)
        {
            _character.planet = planetTransform;
        }

        /// <summary>
        /// Animationから呼ばれる
        /// </summary>
        public void ShowAttackTrigger()
        {
            attackCollider.enabled = true;
        }

    }
}