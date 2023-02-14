using UnityEngine;
using GLShared.General.Interfaces;
using Zenject;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using GLShared.General.Models;
using GLShared.General.Utilities;

namespace Frontend.Scripts.Models
{
    public class PCSinglePlayerInput : MonoBehaviour, IPlayerInputProvider, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly PlayerEntity playerEntity;

        private PlayerInput currentRemoteInput = null;

        private bool lockPlayerInput = true;

        public float horizontal;
        public float vertical;
        public float rawVertical;

        private bool brake;
        private bool pressedSnipingKey;
        private bool pressedShootingKey;
        private bool pressedTurretLockKey = true;

        private float combinedInput = 0f;
        private float lastVerticalInput = 0f;

        public float Vertical => vertical;
        public float Horizontal => horizontal;

        public bool Brake => brake;
        public bool SnipingKey => pressedSnipingKey;
        public bool ShootingKey => pressedShootingKey;
        public bool TurretLockKey => pressedTurretLockKey;
        public bool LockPlayerInput => lockPlayerInput;

        public float CombinedInput => combinedInput;
        public float LastVerticalInput => lastVerticalInput;
        public float AbsoluteVertical => ReturnAbsoluteVertical(ref vertical);
        public float AbsoluteHorizontal => ReturnAbsoluteHorizontal(ref horizontal);
        public float RawVertical => rawVertical;
        public float SignedVertical => ReturnSignedInput(ref vertical);
        public float SignedHorizontal => ReturnSignedInput(ref horizontal);

        public void Initialize()
        {
            signalBus.Subscribe<PlayerSignals.OnAllPlayersInputLockUpdate>(OnAllPlayersInputLockUpdate);
        }

        public void SetInput(PlayerInput input)
        {
            if (!playerEntity.IsLocalPlayer)
            {
                if (currentRemoteInput != null)
                {
                    lastVerticalInput = currentRemoteInput.Vertical;
                }

                currentRemoteInput = lockPlayerInput ? currentRemoteInput.EmptyPlayerInput() : input;
            }
        }

        private float ReturnAbsoluteHorizontal (ref float input)
        {
            return Mathf.Abs(input);
        }

        private float ReturnAbsoluteVertical(ref float input)
        {
            return Mathf.Abs(input);
        }

        private float ReturnSignedInput(ref float input)
        {
            return input != 0f ? Mathf.Sign(input) : 0f;
        }

        private void Update()
        {
            if (playerEntity.IsLocalPlayer)
            {
                if (!lockPlayerInput)
                {
                    brake = Input.GetButton("Brake");

                    horizontal = !Brake ? Input.GetAxis("Horizontal") : 0f;
                    vertical = !Brake ? Input.GetAxis("Vertical") : 0f;

                    pressedSnipingKey = Input.GetKeyDown(KeyCode.LeftShift);  
                    rawVertical = !Brake ? Input.GetAxisRaw("Vertical") : 0f;

                    pressedShootingKey = Input.GetMouseButton(0);
                    pressedTurretLockKey = Input.GetMouseButton(1);

                    if (vertical != 0f)
                    {
                        lastVerticalInput = SignedVertical;
                    }

                    combinedInput = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
                    playerEntity.Input.UpdateControllerInputs(horizontal, vertical, rawVertical, brake, pressedTurretLockKey, pressedShootingKey);
                }
            }
            else
            {
                if (currentRemoteInput == null)
                {
                    return;
                }

                horizontal = currentRemoteInput.Horizontal;
                vertical = currentRemoteInput.Vertical;
                rawVertical = currentRemoteInput.RawVertical;

                combinedInput = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            }
        }

        private void OnAllPlayersInputLockUpdate(PlayerSignals.OnAllPlayersInputLockUpdate OnAllPlayersInputLockUpdate)
        {
            lockPlayerInput = OnAllPlayersInputLockUpdate.LockPlayersInput;
        }
    }
}
