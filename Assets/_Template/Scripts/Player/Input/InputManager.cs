using TemplateSystems.Keys;
using System;
using UnityEngine;
using UISystems;

namespace TemplateSystems.Managers
{
    [Serializable]
    public enum GameStatesType
    {
        Idle
    }
    public class InputManager : MonoSingleton<InputManager>
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private bool isReadyForTouch, isFirstTimeTouchTaken;
        [SerializeField] private GameStatesType currentGameState;
        private FloatingJoystick floatingJoystick;

        #endregion

        #region Private Variables

        private bool _isTouching;
        public bool IsTouching => _isTouching;
        private Vector2? _mousePosition;
        private Vector3 _moveVector;
        private Vector3 _joystickPosition;

        #endregion

        #endregion

        private void Start()
        {
            floatingJoystick = UIManager.instance.UIGameplayCtr.FloatingJoystick;
        }
        #region Event Subscription

        private void OnEnable()
        {
            SubscribeEvents();
        }
        private void OnDisable()
        {
            UnsubscribeEvents();
        }
        private void SubscribeEvents()
        {

        }

        private void UnsubscribeEvents()
        {

        }

        #endregion

        private void Update()
        {
            if (!isReadyForTouch) return;

            if (Input.GetMouseButtonUp(0))
            {
                _isTouching = false;
                InputSignals.Instance.onInputReleased?.Invoke();
            }

            if (Input.GetMouseButtonDown(0))
            {
                _isTouching = true;
                InputSignals.Instance.onInputTaken?.Invoke();
                if (!isFirstTimeTouchTaken)
                {
                    isFirstTimeTouchTaken = true;
                    InputSignals.Instance.onFirstTimeTouchTaken?.Invoke();
                }

                _mousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                if (_isTouching)
                    if (currentGameState == GameStatesType.Idle)
                    {
                        _joystickPosition = new Vector3(floatingJoystick.Horizontal, 0, floatingJoystick.Vertical);
                        _moveVector = _joystickPosition;
                        InputSignals.Instance.onJoystickDragged?.Invoke(new IdleInputParams
                        {
                            JoystickMovement = _moveVector
                        });
                    }
            }
        }
    }
}