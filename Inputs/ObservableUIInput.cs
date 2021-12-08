using System;
using Managers;
using UI;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inputs
{
    public class ObservableUIInput : MonoBehaviour, StarterAssetsInputAction.IUIActions,IUIInput
    {
        private StarterAssetsInputAction.UIActions _input;

        public IObservable<Unit> OnCloseMenuPressed => _onCloseMenuPressed;
        private readonly Subject<Unit> _onCloseMenuPressed = new Subject<Unit>();
    
        private void Awake()
        {
            _input = new StarterAssetsInputAction.UIActions(new StarterAssetsInputAction());
            _input.SetCallbacks(this);
        }

        // Start is called before the first frame update
        void Start()
        {
            _input.Disable();
        
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.OpenMenu)
                .Subscribe(_ =>
                {
                    _input.Enable();
                }).AddTo(this);
            MessageBroker.Default.Receive<GameManager.GlobalEvent>()
                .Where(x => x == GameManager.GlobalEvent.ResumeGame)
                .Subscribe(_ =>
                {
                    _input.Disable();
                }).AddTo(this);
        
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
        }

        public void OnClick(InputAction.CallbackContext context)
        {
        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
        }

        public void OnCloseMenu(InputAction.CallbackContext context)
        {
            if(context.performed)
                _onCloseMenuPressed.OnNext(Unit.Default);
        }
        private void OnDisable()
        {
            _input.Disable();
        }

        private void OnDestroy()
        {
            _input.Disable();
        }

    }
}