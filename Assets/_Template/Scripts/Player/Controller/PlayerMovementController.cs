using DG.Tweening;
using TemplateSystems.Keys;
using UnityEngine;

namespace TemplateSystems.Controllers.Player
{
    public class PlayerMovementController : MonoBehaviour
    {
        #region Self Variables
        [SerializeField] float speedFactor = 3f;
        [SerializeField] float movementRadius = 15f;
        
        [Header("Rotation Settings")]
        [SerializeField] private float movementRotationSpeed = 10f;  // Rotation speed during movement
        [SerializeField] private float combatRotationSpeed = 15f;    // Rotation speed during combat
        private Transform _target;                                    // Current target to face
        public Transform Target 
        { 
            get => _target;
            set 
            {
                _target = value;
                // Reset movement direction when getting a new target
                if (_target != null)
                {
                    _movementDirection = Vector3.zero;
                }
            }
        }

        #region Serialized Variables
        [SerializeField] private Rigidbody rigidBody;
        //[SerializeField] private GameStatesType gameStatesType;

        #endregion

        #region Private Variables

        [SerializeField] private PlayerMovementData _playerMovementData;
        private bool _isReadyToMove;
        public bool IsReadyToMove => _isReadyToMove;
        [SerializeField] private bool _isReadyToPlay;
        private Vector3 _movementDirection;
        private PlayerAttackController _attackController;
        private void OnPointerDown() => ActivateMovement();
        private void OnInputReleased() => DeactivateMovement();
        #endregion

        #endregion
        private void Start()
        {
            _attackController = GetComponent<PlayerAttackController>();
            SubscribeEvents();
        }
        private void SubscribeEvents()
        {
            InputSignals.Instance.onJoystickDragged += OnJoystickDragged;
            InputSignals.Instance.onInputTaken += OnPointerDown;
            InputSignals.Instance.onInputReleased += OnInputReleased;
        }
        private void UnsubscribeEvents()
        {
            if (InputSignals.Instance == null)
                return;
            InputSignals.Instance.onInputTaken -= OnPointerDown;
            InputSignals.Instance.onInputReleased -= OnInputReleased;
            InputSignals.Instance.onJoystickDragged -= OnJoystickDragged;

        }
        private void OnJoystickDragged(IdleInputParams arg0)
        {
            UpdateIdleInputValue(arg0);
        }

        public void ActivateMovement()
        {
            _isReadyToMove = true;
        }

        public void DeactivateMovement()
        {
            _isReadyToMove = false;
            Stop();
        }

        private void FixedUpdate()
        {
            if (!_isReadyToPlay)
            {
                Stop();
                return;
            }

            if (_isReadyToMove && _movementDirection != Vector3.zero)
            {
                // If we're moving, handle movement and movement-based rotation
                IdleMove();
            }
            else if (Target != null)
            {
                // If we're not moving but have a target, handle combat rotation
                HandleCombatRotation();
            }
            else
            {
                Stop();
            }
        }

        private void IdleMove()
        {
            // Handle movement
            Vector3 direction = new Vector3(_movementDirection.x, 0, _movementDirection.z);
            Vector3 movement = direction * speedFactor * Time.fixedDeltaTime;
            
            Vector3 newPosition = rigidBody.position + movement;
            
            if (newPosition.magnitude <= movementRadius)
            {
                rigidBody.MovePosition(newPosition);
            }
            else
            {
                Vector3 clampedPosition = newPosition.normalized * movementRadius;
                rigidBody.MovePosition(clampedPosition);
            }

            // Handle rotation
            if (_movementDirection != Vector3.zero && Target == null)
            {
                // When moving, rotate towards movement direction
                var toRotation = Quaternion.LookRotation(_movementDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * movementRotationSpeed);
            }
            if(Target != null)
            {
                HandleCombatRotation(); 
            }
        }

        private void HandleCombatRotation()
        {
            if (Target != null)
            {
                // Calculate direction to target
                Vector3 directionToTarget = (Target.position - transform.position).normalized;
                directionToTarget.y = 0; // Keep rotation in horizontal plane

                // Rotate towards target
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * combatRotationSpeed);
            }
        }

        private void Stop()
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }

        public void MovementReset()
        {
            Stop();
            _isReadyToPlay = false;
            _isReadyToMove = false;
            Target = null;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }

        public void OnReset()
        {
            DOTween.KillAll();
        }

        public void SetMovementData(PlayerMovementData movementData)
        {
            _playerMovementData = movementData;
        }
        public void UpdateIdleInputValue(IdleInputParams inputParam)
        {
            _movementDirection = inputParam.JoystickMovement;
        }
        public void IsReadyToPlay(bool state)
        {
            _isReadyToPlay = state;
        }
    }
}