// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using Mirror;
using Team_Capture.Collections;
using Team_Capture.Core;
using Team_Capture.Helper.Extensions;
using Team_Capture.Player.Movement.States;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Player.Movement
{
    internal class PlayerMovementManager : NetworkBehaviour
    {
        private const int MaximumReceivedClientMotorStates = 10;
        private const int PastStatesToSend = 3;

        [SerializeField] private Transform cameraHolderTransform;
        [SerializeField] private GameObject cameraGameObject;
        [SerializeField] private GameObject gfxObject;

        [Header("Movement Settings")] [SerializeField]
        private float moveSpeed = 14.0f;

        [SerializeField] private float friction = 6;
        [SerializeField] private float runAcceleration = 20.0f;
        [SerializeField] private float runDeacceleration = 16.0f;
        [SerializeField] private float airAcceleration = 3.0f;
        [SerializeField] private float airDecceleration = 3.0f;
        [SerializeField] private float airControl = 0.6f;
        [SerializeField] private float sideStrafeAcceleration = 50.0f;
        [SerializeField] private float sideStrafeSpeed = 1.5f;
        [SerializeField] private float jumpHeight = 8.0f;
        [SerializeField] private float gravityAmount = 24.0f;

        [Header("Ground Check")] [SerializeField]
        private Transform groundCheck;

        [SerializeField] private float groundDistance = 0.7f;
        [SerializeField] private LayerMask groundMask;

        private readonly List<PlayerInputs> clientMotorStates = new();
        private readonly Queue<PlayerInputs> receivedClientMotorStates = new();

        private readonly InputData storedInput = new();
        private PlayerCameraRoll cameraRoll;

        /// <summary>
        ///     <see cref="CharacterController"/> of this client
        /// </summary>
        internal CharacterController CharacterController { get; private set; }

        private bool isReady;

        private uint lastClientStateReceived;
        private PlayerState? receivedServerMotorState;
        private float rotationX;
        private float rotationY;
        private PlayerTransformSync transformSync;

        private Vector3 velocity;
        private bool wishJump;

        private void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            PlayerTransformSync sync = GetComponent<PlayerTransformSync>();
            if (!isLocalPlayer)
            {
                transformSync = sync;
            }
            else
            {
                Destroy(sync);

                cameraRoll = cameraGameObject.AddComponent<PlayerCameraRoll>();
                cameraRoll.SetBaseTransform(transform);
            }

            isReady = true;
        }

        private void OnEnable()
        {
            FixedUpdateManager.OnFixedUpdate += OnFixedUpdate;

            CharacterController.enabled = true;

            if (isReady && !isLocalPlayer)
                transformSync.enabled = true;
        }

        private void OnDisable()
        {
            FixedUpdateManager.OnFixedUpdate -= OnFixedUpdate;

            if (isLocalPlayer)
            {
                clientMotorStates.Clear();
                receivedClientMotorStates.Clear();
                lastClientStateReceived = 0;
                receivedServerMotorState = null;

                storedInput.Jump = false;
                storedInput.LookMovements.Clear();
                storedInput.MovementDir = Vector2.zero;
            }
            else
            {
                transformSync.Reset();
                transformSync.enabled = false;
            }

            velocity = Vector3.zero;
            rotationX = 0f;
            rotationY = 0f;
            wishJump = false;

            CharacterController.enabled = false;
        }

        public override void OnStartAuthority()
        {
            gfxObject.AddComponent<PlayerGfxMover>();
        }

        private void OnFixedUpdate()
        {
            if (hasAuthority)
            {
                ProcessReceivedServerMotorState();
                SendInputs();
            }

            if (isServer) ProcessReceivedClientMotorState();
        }

        [Server]
        internal void SetLocation(Vector3 location)
        {
            transform.position = location;
            Physics.SyncTransforms();

            RpcSetLocation(location);
        }

        [ClientRpc]
        private void RpcSetLocation(Vector3 location)
        {
            if (isLocalPlayer)
            {
                clientMotorStates.Clear();
                receivedClientMotorStates.Clear();
                lastClientStateReceived = 0;
                receivedServerMotorState = null;
            }
            else
            {
                transformSync.Reset();
            }

            transform.position = location;
            Physics.SyncTransforms();
        }

        /// <summary>
        ///     Set inputs
        /// </summary>
        /// <param name="horizontal"></param>
        /// <param name="vertical"></param>
        /// <param name="lookX"></param>
        /// <param name="lookY"></param>
        /// <param name="jump"></param>
        [Client]
        internal void SetInput(float horizontal, float vertical, float lookX, float lookY, bool jump)
        {
            storedInput.MovementDir = new Vector2(horizontal, vertical);
            storedInput.Jump = jump;
            storedInput.LookMovements.PushFront(new Vector2(lookY, lookX));
        }

        [Client]
        private void ProcessReceivedServerMotorState()
        {
            if (receivedServerMotorState == null)
                return;

            PlayerState serverState = receivedServerMotorState.Value;
            FixedUpdateManager.AddTiming(serverState.TimingStepChange);
            receivedServerMotorState = null;
            
            int index = clientMotorStates.FindIndex(x => x.FixedFrame == serverState.FixedFrame);
            if (index != -1)
                clientMotorStates.RemoveRange(0, index + 1);
            
            transform.position = serverState.Position;
            transform.rotation = Quaternion.Euler(0, serverState.Rotation.y, 0);
            cameraHolderTransform.rotation = Quaternion.Euler(serverState.Rotation.x, serverState.Rotation.y, 0);
            velocity = serverState.Velocity;
            rotationX = serverState.Rotation.x;
            rotationY = serverState.Rotation.y;
            wishJump = serverState.WishJump;

            Physics.SyncTransforms();

            for (int i = 0; i < clientMotorStates.Count; i++)
            {
                DoMovement(clientMotorStates[i]);
            }
        }

        [Server]
        private void ProcessReceivedClientMotorState()
        {
            if (isClient && hasAuthority)
                return;

            sbyte timingStepChange = 0;

            if (receivedClientMotorStates.Count == 0)
                timingStepChange = -1;
            else if (receivedClientMotorStates.Count > 1)
                timingStepChange = 1;
            
            if (receivedClientMotorStates.Count > 0)
            {
                PlayerInputs state = receivedClientMotorStates.Dequeue();
                DoMovement(state);

                PlayerState responseState = new()
                {
                    FixedFrame = state.FixedFrame,
                    Position = transform.position,
                    Rotation = new Vector2(rotationX, rotationY),
                    Velocity = velocity,
                    WishJump = wishJump,
                    TimingStepChange = timingStepChange
                };
                
                TargetServerStateUpdate(connectionToClient, responseState);
            }
            else if (timingStepChange != 0)
            {
                TargetChangeTimingStep(connectionToClient, timingStepChange);
            }
        }

        [Client]
        private void SendInputs()
        {
            Vector2 lookMoveAverage = Vector2.zero;
            foreach (Vector2 lookMovement in storedInput.LookMovements)
            {
                lookMoveAverage += lookMovement;
            }

            lookMoveAverage /= storedInput.LookMovements.Size;

            //Sometimes it can be NaN
            if (float.IsNaN(lookMoveAverage.x))
                lookMoveAverage.x = 0;
            if (float.IsNaN(lookMoveAverage.y))
                lookMoveAverage.y = 0;

            storedInput.LookMovements.Clear();

            PlayerInputs state = new()
            {
                FixedFrame = FixedUpdateManager.FixedFrame,
                LookDir = lookMoveAverage,
                MovementDir = storedInput.MovementDir,
                WishJump = storedInput.Jump
            };
            clientMotorStates.Add(state);
            
            int targetArraySize = Mathf.Min(clientMotorStates.Count, 1 + PastStatesToSend);
            var statesToSend = new PlayerInputs[targetArraySize];
            
            for (int i = 0; i < targetArraySize; i++)
                statesToSend[targetArraySize - 1 - i] = clientMotorStates[clientMotorStates.Count - 1 - i];

            DoMovement(state);
            CmdSendInputs(statesToSend);
        }

        [Command(channel = Channels.Unreliable)]
        private void CmdSendInputs(PlayerInputs[] states)
        {
            if (states == null || states.Length == 0)
                return;
            
            if (isClient && hasAuthority)
                return;
            
            for (int i = 0; i < states.Length; i++)
                if (states[i].FixedFrame > lastClientStateReceived)
                {
                    receivedClientMotorStates.Enqueue(states[i]);
                    lastClientStateReceived = states[i].FixedFrame;
                }

            while (receivedClientMotorStates.Count > MaximumReceivedClientMotorStates)
            {
                receivedClientMotorStates.Dequeue();
            }
        }

        [TargetRpc(channel = Channels.Unreliable)]
        private void TargetServerStateUpdate(NetworkConnection conn, PlayerState state)
        {
            if (receivedServerMotorState != null && state.FixedFrame < receivedServerMotorState.Value.FixedFrame)
                return;

            receivedServerMotorState = state;
        }

        [TargetRpc(channel = Channels.Unreliable)]
        private void TargetChangeTimingStep(NetworkConnection conn, sbyte steps)
        {
            FixedUpdateManager.AddTiming(steps);
        }

        private class InputData
        {
            public bool Jump;

            public readonly CircularBuffer<Vector2> LookMovements = new(64);

            public Vector2 MovementDir;
        }

        #region Movement

        public void KnockBack(Vector3 direction, float force)
        {
            if (isServer || isLocalPlayer)
            {
                float playerMass = 5f;
                velocity += force * direction / playerMass;
            }
        }

        private void DoMovement(PlayerInputs input)
        {
            input.MovementDir.x = input.MovementDir.x.PreciseSign();
            input.MovementDir.y = input.MovementDir.y.PreciseSign();

            bool isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask);
            wishJump = input.WishJump;

            if (isGrounded)
                GroundMove(input);
            else
                AirMove(input);

            //Mouse movement
            rotationX -= input.LookDir.x;
            rotationY += input.LookDir.y;

            rotationX = Mathf.Clamp(rotationX, -90, 90);

            //Move character
            CharacterController.Move(velocity * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0, rotationY, 0);
            cameraHolderTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0);

            if (isLocalPlayer && cameraRoll != null)
                cameraRoll.SetVelocity(velocity);
        }

        private void GroundMove(PlayerInputs input)
        {
            //Do not apply friction if the player is queueing up the next jump
            if (!wishJump)
                ApplyFriction(true, 1.0f);
            else
                ApplyFriction(true, 0f);

            Vector3 wishDir = new(input.MovementDir.x, 0f, input.MovementDir.y);
            wishDir = transform.TransformDirection(wishDir);
            wishDir.Normalize();

            float wishSpeed = wishDir.magnitude;
            wishSpeed *= moveSpeed;

            Accelerate(wishDir, wishSpeed, runAcceleration);

            //Reset the gravity velocity
            velocity.y = -gravityAmount * Time.deltaTime;

            if (wishJump)
            {
                velocity.y = jumpHeight;
                wishJump = false;
            }
        }

        private void AirMove(PlayerInputs input)
        {
            Vector3 wishDir = new(input.MovementDir.x, 0f, input.MovementDir.y);
            wishDir = transform.TransformDirection(wishDir);

            float wishSpeed = wishDir.magnitude;
            wishSpeed *= moveSpeed;

            wishDir.Normalize();

            float wishSpeed2 = wishSpeed;
            float accel = Vector3.Dot(velocity, wishDir) < 0 ? airDecceleration : airAcceleration;

            //If the player is ONLY strafing left or right
            if (input.MovementDir.y == 0 && input.MovementDir.x != 0)
            {
                if (wishSpeed > sideStrafeSpeed)
                    wishSpeed = sideStrafeSpeed;

                accel = sideStrafeAcceleration;
            }

            Accelerate(wishDir, wishSpeed, accel);
            if (airControl > 0)
                AirControl(input, wishDir, wishSpeed2);

            velocity.y -= gravityAmount * Time.deltaTime;
        }

        private void AirControl(PlayerInputs input, Vector3 wishDir, float wishSpeed)
        {
            //Can't control movement if not moving forward or backward
            if (Mathf.Abs(input.MovementDir.y) < 0.001 || Mathf.Abs(wishSpeed) < 0.001)
                return;

            float zSpeed = velocity.y;
            velocity.y = 0;

            float speed = velocity.magnitude;
            velocity.Normalize();

            float dot = Vector3.Dot(velocity, wishDir);
            float k = 32;
            k *= airControl * dot * dot * Time.deltaTime;

            //Change direction while slowing down
            if (dot > 0)
            {
                velocity.x = velocity.x * speed + wishDir.x * k;
                velocity.y = velocity.y * speed + wishDir.y * k;
                velocity.z = velocity.z * speed + wishDir.z * k;

                velocity.Normalize();
            }

            velocity.x *= speed;
            velocity.y = zSpeed;
            velocity.z *= speed;
        }

        private void ApplyFriction(bool isGrounded, float t)
        {
            Vector3 vec = velocity;
            vec.y = 0.0f;

            float speed = vec.magnitude;
            float drop = 0.0f;

            if (isGrounded)
            {
                float control = speed < runDeacceleration ? runDeacceleration : speed;
                drop = control * friction * Time.deltaTime * t;
            }

            float newSpeed = speed - drop;
            if (newSpeed < 0)
                newSpeed = 0;
            if (speed > 0)
                newSpeed /= speed;

            velocity.x *= newSpeed;
            velocity.z *= newSpeed;
        }

        private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0)
                return;

            float accelSpeed = accel * Time.fixedDeltaTime * wishSpeed;
            if (accelSpeed > addSpeed)
                accelSpeed = addSpeed;

            velocity.x += accelSpeed * wishDir.x;
            velocity.z += accelSpeed * wishDir.z;
        }

        #endregion
    }
}