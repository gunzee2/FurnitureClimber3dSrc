using ECM2.Characters;
using ECM2.Common;
using ECM2.Components;
using UniRx;
using UnityEngine;

namespace Players
{
    public class PlayerCharacter : Character
    {
        [Space(15f)]
        [SerializeField]public Transform planet;
    
        private Vector3 _initialGravity;
    
        public IReadOnlyReactiveProperty<Vector3> Displacement => _displacement;
        private Vector3ReactiveProperty _displacement = new Vector3ReactiveProperty();

        protected override void OnAwake()
        {
            base.OnAwake();
            _initialGravity = gravity;
        }

        public override bool CanCrouch()
        {
            return IsFalling();
        }

        public void RotateTo(Vector3 vec, bool isLerp)
        {
            if(isLerp)
                RotateTowards(vec);
            else
            {
                RotateTowardsInstant(vec);
            }
        }
        private void RotateTowardsInstant(Vector3 worldDirection, bool isPlanar = true)
        {
            Vector3 characterUp = GetUpVector();

            if (isPlanar)
                worldDirection = worldDirection.projectedOnPlane(characterUp);

            if (worldDirection.isZero())
                return;

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection, characterUp);

            characterMovement.rotation = targetRotation;
        }

        protected override void OnMovementHit(ref MovementHit movementHit)
        {
            base.OnMovementHit(ref movementHit);

            if (movementHit.hitGround) return;
        
            _displacement.Value = movementHit.displacement;
        }

    
        protected override void UpdateRotation()
        {
            // Call base method (eg: rotate towards movement direction)

            base.UpdateRotation();

            if (planet == null)
            {
                gravity = _initialGravity;

                characterMovement.rotation = Quaternion.FromToRotation(GetUpVector(), -gravity) * GetRotation();
            
            }
            else
            {
                // Update's gravity direction and orient Character's Up to -gravity direction
            
                gravity = (transform.position - planet.position).normalized * -gravity.magnitude;

                Quaternion targetRotation = Quaternion.FromToRotation(GetUpVector(), -gravity) * characterMovement.rotation;
                characterMovement.rotation = Quaternion.RotateTowards(characterMovement.rotation, targetRotation, rotationRate * Time.deltaTime);
            
            }
        
        }

        protected override void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
        {
            base.OnMovementModeChanged(prevMovementMode, prevCustomMode);
            Debug.Log($"Movement Mode => {_movementMode}");
        }

        public CapsuleHitLocation ComputeCapsuleHitLocation(Vector3 inNormal)
        {
            return characterMovement.ComputeCapsuleHitLocation(transform.position, transform.rotation, inNormal);
        }
    
    }
}