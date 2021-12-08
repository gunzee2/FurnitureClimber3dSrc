using UniRx;
using UniRx.Triggers;
using UnityEngine;
namespace Cameras
{
    public class CameraMover : MonoBehaviour
    {
        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 89.0f;
        public float BottomClamp = 0.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;
    
        public bool IsReverseY = false;
        public bool IsReverseX = false;
    
        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
    
        private const float _threshold = 0.01f;
    
        private ICameraMovementInput _cameraMovementInput;

        private void Start()
        {
            _cinemachineTargetPitch = CinemachineCameraTarget.transform.eulerAngles.x;
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.eulerAngles.y;
        
            _cameraMovementInput = GetComponent<ICameraMovementInput>();
        
            this.LateUpdateAsObservable()
                .Select(_ => _cameraMovementInput.Look.Value)
                .Subscribe(CameraRotation).AddTo(this);
        }
    
        private void CameraRotation(Vector2 lookValue)
        {
            if (IsReverseY) lookValue.y = -lookValue.y;
            if (IsReverseX) lookValue.x = -lookValue.x;
        
            // if there is an input and camera position is not fixed
            if (lookValue.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                _cinemachineTargetYaw += lookValue.x * Time.deltaTime;
                _cinemachineTargetPitch += lookValue.y * Time.deltaTime;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }
    
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}
