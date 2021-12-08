using System;
using Cameras;
using Managers;
using Players;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inputs
{
    public class ObservablePlayerInput : MonoBehaviour, StarterAssetsInputAction.IPlayerActions, IPlayerInput, ICameraMovementInput
    {

        public IReadOnlyReactiveProperty<Vector2> Move => _move;
        private Vector2ReactiveProperty _move = new Vector2ReactiveProperty(Vector2.zero);

        public IReadOnlyReactiveProperty<Vector2> Look => _look;
        private Vector2ReactiveProperty _look = new Vector2ReactiveProperty(Vector2.zero);

        public IReadOnlyReactiveProperty<bool> JumpCharge => _jumpCharge;
        private BoolReactiveProperty _jumpCharge = new BoolReactiveProperty(false);
        public IReadOnlyReactiveProperty<bool> JumpAttack => _jumpAttack;
        private BoolReactiveProperty _jumpAttack = new BoolReactiveProperty(false);
        public IReadOnlyReactiveProperty<bool> Sprint => _sprint;
        private BoolReactiveProperty _sprint = new BoolReactiveProperty(false);
        public IReadOnlyReactiveProperty<bool> AnalogMovement => _analogMovement;
        private BoolReactiveProperty _analogMovement = new BoolReactiveProperty(false);

        public BoolReactiveProperty CursorLocked => _cursorLocked;
        private BoolReactiveProperty _cursorLocked = new BoolReactiveProperty(true);
        public BoolReactiveProperty CursorInputForLook => _cursorInputForLook;
        private BoolReactiveProperty _cursorInputForLook = new BoolReactiveProperty(true);

        public IObservable<Unit> OnOpenMenuPressed => _onOpenMenuPressed;
        private readonly Subject<Unit> _onOpenMenuPressed = new Subject<Unit>();

        public IObservable<Unit> OnOpenPhotoModePressed => _onOpenPhotoModePressed;
        private readonly Subject<Unit> _onOpenPhotoModePressed = new Subject<Unit>();
    
        private StarterAssetsInputAction.PlayerActions _input;
        private bool _canCursorLock = true;

        #region Unity Events
        void Awake()
        {
            _input = new StarterAssetsInputAction.PlayerActions(new StarterAssetsInputAction());
        
            _input.SetCallbacks(this);

        }

        private void Start()
        {
            SetCursorState(_cursorLocked.Value);
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.SetupGame)
                .Subscribe(_ =>
                {
                    Debug.Log("setup");
                    _canCursorLock = true;
                    _cursorLocked.Value = true;
                    SetCursorState(_cursorLocked.Value);
                    _input.Disable();
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PlayerStart)
                .Subscribe(_ =>
                {
                    Debug.Log("player start");
                    _input.Enable();
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PauseGame)
                .Subscribe(_ =>
                {
                    _canCursorLock = false;
                    _cursorLocked.Value = false;
                    SetCursorState(_cursorLocked.Value);
                    _input.Disable();
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.ResumeGame)
                .Subscribe(_ =>
                {
                    _canCursorLock = true;
                    _cursorLocked.Value = true;
                    SetCursorState(_cursorLocked.Value);
                    _input.Enable();
                }).AddTo(this);
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.PlayerGoal)
                .Subscribe(_ =>
                {
                    _canCursorLock = false;
                    _cursorLocked.Value = false;
                    SetCursorState(_cursorLocked.Value);
                    _input.Disable();
                }).AddTo(this);
        }

        private void OnDisable()
        {
            _input.Disable();
        }

        private void OnDestroy()
        {
            _input.Disable();
        }
        #endregion
    
        #region Input System Events
        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MoveInput(context.ReadValue<Vector2>());
            }
            else
            {
                MoveInput(context.ReadValue<Vector2>());
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if(CursorInputForLook.Value)
            {
                LookInput(context.ReadValue<Vector2>());
            }
        }

        public void OnJumpCharge(InputAction.CallbackContext context)
        {
            if (context.started) JumpChargeInput(true);
            if (context.canceled) JumpChargeInput(false);
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started) SprintInput(true);
            if (context.canceled) SprintInput(false);
        }

        public void OnOpenMenu(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OpenMenu();
            }
        }

        public void OnStartPhotoMode(InputAction.CallbackContext context)
        {
            if(context.performed)
                OpenPhotoMode();
        }

        public void OnJumpAttack(InputAction.CallbackContext context)
        {
            if (context.started) JumpAttackInput(true);
            if (context.canceled) JumpAttackInput(false);
        }

        #endregion
    
        #region public Method
        public void MoveInput(Vector2 newMoveDirection)
        {
            _move.Value = newMoveDirection;
        } 

        public void LookInput(Vector2 newLookDirection)
        {
            _look.Value = newLookDirection;
        }

        public void JumpChargeInput(bool newJumpState)
        {
            _jumpCharge.Value = newJumpState;
        }
        public void JumpAttackInput(bool newJumpState)
        {
            _jumpAttack.Value = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            Debug.Log(newSprintState);
            _sprint.Value = newSprintState;
        }

        public void OpenMenu()
        {
            _onOpenMenuPressed.OnNext(Unit.Default);
        }
        public void OpenPhotoMode()
        {
            _onOpenPhotoModePressed.OnNext(Unit.Default);
        }

        #endregion

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!_canCursorLock) return;
        
            SetCursorState(_cursorLocked.Value);
        }

        private void SetCursorState(bool newState)
        {
            Debug.Log($"cursor state => {newState}");
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

    }
}