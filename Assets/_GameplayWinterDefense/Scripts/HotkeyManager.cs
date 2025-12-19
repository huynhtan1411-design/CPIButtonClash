using UnityEngine;
using TemplateSystems;
using TemplateSystems.Keys;
using CLHoma.Combat;

namespace WD
{
    public class HotkeyManager : MonoBehaviour
    {
        [Header("Movement Hotkeys")]
        [SerializeField] private bool enableMovementHotkeys = true;
        [SerializeField] private KeyCode moveUpKey = KeyCode.W;
        [SerializeField] private KeyCode moveDownKey = KeyCode.S;
        [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
        [SerializeField] private KeyCode moveRightKey = KeyCode.D;
        [SerializeField] private KeyCode moveUpAltKey = KeyCode.UpArrow;
        [SerializeField] private KeyCode moveDownAltKey = KeyCode.DownArrow;
        [SerializeField] private KeyCode moveLeftAltKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode moveRightAltKey = KeyCode.RightArrow;
        [SerializeField] private bool useInputSignals = true;
        [SerializeField] private Transform directMoveTarget;
        [SerializeField] private float directMoveSpeed = 5f;

        [Header("Wave Hotkey")]
        [SerializeField] private bool enableWaveHotkey = true;
        [SerializeField] private KeyCode startWaveKey = KeyCode.F;

        [Header("Path Spawn Hotkeys")]
        [SerializeField] private bool enablePathSpawnHotkeys = false;
        [SerializeField] private PathSpawnController pathSpawnController;
        [SerializeField] private PathSpawnHotkey[] pathSpawnHotkeys;

        [Header("Path Spawn Mode")]
        [Tooltip("Bật: giữ phím để spawn, nhả phím dừng. Tắt: bấm 1 lần để bật/tắt spawn.")]
        [SerializeField] private bool holdToSpawn = false;

        [Header("Elite Spawn Hotkey")]
        [SerializeField] private bool enableEliteSpawnHotkey = false;
        [SerializeField] private KeyCode eliteSpawnKey = KeyCode.E;

        [Header("Drop Spawn Hotkeys")]
        [SerializeField] private bool enableDropSpawnHotkeys = false;
        [SerializeField] private DropSpawnHotkey[] dropSpawnHotkeys;

        private bool isMovementActive;

        private void Awake()
        {
            if (directMoveTarget == null && WDPlayerController.Instance != null)
                directMoveTarget = WDPlayerController.Instance.transform;
        }

        private void Update()
        {
            if (GameManager.IsPaused())
            {
                if (isMovementActive)
                {
                    isMovementActive = false;
                    SendInputReleased();
                }
                return;
            }

            HandleMovementInput();
            HandleWaveInput();
            HandlePathSpawnHotkeys();
            HandleEliteSpawnHotkey();
            HandleDropSpawnHotkeys();
        }

        private void HandleMovementInput()
        {
            if (!enableMovementHotkeys)
                return;

            Vector3 movement = GetMovementVector();
            bool hasInput = movement.sqrMagnitude > 0.0001f;

            if (hasInput)
            {
                if (!isMovementActive)
                {
                    isMovementActive = true;
                    SendInputTaken();
                }

                if (!SendMovementToInputSignals(movement))
                    DirectMove(movement);
            }
            else if (isMovementActive)
            {
                isMovementActive = false;
                SendInputReleased();
            }
        }

        private Vector3 GetMovementVector()
        {
            bool up = Input.GetKey(moveUpKey) || Input.GetKey(moveUpAltKey);
            bool down = Input.GetKey(moveDownKey) || Input.GetKey(moveDownAltKey);
            bool left = Input.GetKey(moveLeftKey) || Input.GetKey(moveLeftAltKey);
            bool right = Input.GetKey(moveRightKey) || Input.GetKey(moveRightAltKey);

            float x = (right ? 1f : 0f) - (left ? 1f : 0f);
            float z = (up ? 1f : 0f) - (down ? 1f : 0f);

            Vector3 movement = new Vector3(x, 0f, z);
            if (movement.sqrMagnitude > 1f)
                movement.Normalize();

            return movement;
        }

        private bool SendMovementToInputSignals(Vector3 movement)
        {
            if (!useInputSignals || InputSignals.Instance == null)
                return false;

            InputSignals.Instance.onJoystickDragged?.Invoke(new IdleInputParams
            {
                JoystickMovement = movement
            });
            return true;
        }

        private void SendInputTaken()
        {
            if (!useInputSignals || InputSignals.Instance == null)
                return;

            InputSignals.Instance.onInputTaken?.Invoke();
        }

        private void SendInputReleased()
        {
            if (!useInputSignals || InputSignals.Instance == null)
                return;

            InputSignals.Instance.onInputReleased?.Invoke();
        }

        private void DirectMove(Vector3 movement)
        {
            if (directMoveTarget == null)
                return;

            directMoveTarget.position += movement * directMoveSpeed * Time.deltaTime;
        }

        private void HandleWaveInput()
        {
            if (!enableWaveHotkey || !Input.GetKeyDown(startWaveKey))
                return;

            if (GameManager.Instance == null)
                return;

            if (GameManager.Instance.CurrentPhase == GamePhase.BuildPhase)
                GameManager.Instance.StartCombatPhase();
            else if (GameManager.Instance.CurrentPhase == GamePhase.CombatPhase)
                GameManager.Instance.StartNextWave();
        }

        private void HandlePathSpawnHotkeys()
        {
            if (!enablePathSpawnHotkeys || pathSpawnController == null || pathSpawnHotkeys == null)
                return;

            for (int i = 0; i < pathSpawnHotkeys.Length; i++)
            {
                var mapping = pathSpawnHotkeys[i];

                if (!holdToSpawn)
                {
                    if (Input.GetKeyDown(mapping.key))
                    {
                        // Debug.Log($"Spawn toggle: {mapping.key} path={mapping.pathIndex} frame={Time.frameCount}", this);
                        pathSpawnController.ToggleSpawnAtIndex(mapping.pathIndex);
                    }
                }
                else
                {
                    if (Input.GetKey(mapping.key))
                        pathSpawnController.StartSpawnAtIndex(mapping.pathIndex);

                    if (Input.GetKeyUp(mapping.key))
                        pathSpawnController.StopSpawnAtIndex(mapping.pathIndex);
                }
            }
        }

        private void HandleEliteSpawnHotkey()
        {
            if (!enableEliteSpawnHotkey || pathSpawnController == null)
                return;

            if (Input.GetKeyDown(eliteSpawnKey))
                pathSpawnController.SpawnEliteOnce();
        }

        private void HandleDropSpawnHotkeys()
        {
            if (!enableDropSpawnHotkeys || pathSpawnController == null || dropSpawnHotkeys == null)
                return;

            for (int i = 0; i < dropSpawnHotkeys.Length; i++)
            {
                var mapping = dropSpawnHotkeys[i];
                if (Input.GetKeyDown(mapping.key))
                {
                    pathSpawnController.SpawnDropAtIndex(mapping.pointIndex);
                }
            }
        }
    }

    [System.Serializable]
    public struct PathSpawnHotkey
    {
        public KeyCode key;
        public int pathIndex;
    }

    [System.Serializable]
    public struct DropSpawnHotkey
    {
        public KeyCode key;
        public int pointIndex;
    }
}
