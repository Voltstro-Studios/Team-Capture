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

namespace Team_Capture.Player.Movement
{
    internal class PlayerMovementManager : NetworkBehaviour
    {
        private const int MAXIMUM_RECEIVED_CLIENT_MOTOR_STATES = 10;
        private const int PAST_STATES_TO_SEND = 3;
        
        [SerializeField] private Transform cameraHolderTransform;
        [SerializeField] private GameObject cameraGameObject;
        [SerializeField] private GameObject gfxObject;
        
        [Header("Movement Settings")] 
        [SerializeField] private float moveSpeed = 14.0f;
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
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.7f;
        [SerializeField] private LayerMask groundMask;
        
        private CharacterController characterController;
        private PlayerTransformSync transformSync;
        private PlayerCameraRoll cameraRoll;
        
        private readonly List<PlayerInputs> clientMotorStates = new List<PlayerInputs>();
        private readonly Queue<PlayerInputs> receivedClientMotorStates = new Queue<PlayerInputs>();
        
        private uint lastClientStateReceived;
        private PlayerState? receivedServerMotorState;

        private readonly InputData storedInput = new InputData();

        private Vector3 velocity;
        private float rotationX;
        private float rotationY;
        private bool wishJump;

        private bool isReady;

        public override void OnStartAuthority()
        {
            gfxObject.AddComponent<SmoothToZero>();
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            PlayerTransformSync sync = GetComponent<PlayerTransformSync>();
            if(!isLocalPlayer)
                transformSync = sync;
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
            
            characterController.enabled = true;

            if(isReady && !isLocalPlayer)
                transformSync.enabled = true;
        }

        private void OnDisable()
        {
            FixedUpdateManager.OnFixedUpdate -= OnFixedUpdate;

            if (isLocalPlayer) //Local client
            {
                clientMotorStates.Clear();
                receivedClientMotorStates.Clear();
                lastClientStateReceived = 0;
                receivedServerMotorState = null;
                
                storedInput.Jump = false;
                storedInput.LookMovements.Clear();
                storedInput.MovementDir = Vector2.zero;
            }
            else //Observer or server
            {
                transformSync.Reset();
                transformSync.enabled = false;
            }
            
            velocity = Vector3.zero;
            rotationX = 0f;
            rotationY = 0f;
            wishJump = false;
            
            characterController.enabled = false;
        }

        private void OnFixedUpdate()
        {
            if (hasAuthority)
            {
                ProcessReceivedServerMotorState();
                SendInputs();
            }

            if (isServer)
            {
                ProcessReceivedClientMotorState();
            }
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
            storedInput.LookMovements.Push(new Vector2(lookY, lookX));
        }
        
        [Client]
        private void ProcessReceivedServerMotorState()
        {
            if (receivedServerMotorState == null)
                return;

            PlayerState serverState = receivedServerMotorState.Value;
            FixedUpdateManager.AddTiming(serverState.TimingStepChange);
            receivedServerMotorState = null;

            //Remove entries which have been handled by the server.
            int index = clientMotorStates.FindIndex(x => x.FixedFrame == serverState.FixedFrame);
            if (index != -1)
                clientMotorStates.RemoveRange(0, index + 1);

            //Snap motor to server values.
            transform.position = serverState.Position;
            transform.rotation = Quaternion.Euler(0, serverState.Rotation.y, 0);
            cameraHolderTransform.rotation = Quaternion.Euler(serverState.Rotation.x, serverState.Rotation.y, 0);
            velocity = serverState.Velocity;
            rotationX = serverState.Rotation.x;
            rotationY = serverState.Rotation.y;
            wishJump = serverState.WishJump;
            
            Physics.SyncTransforms();

            foreach (PlayerInputs clientState in clientMotorStates)
                DoMovement(clientState);
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

            //If there is input to process.
            if (receivedClientMotorStates.Count > 0)
            {
                PlayerInputs state = receivedClientMotorStates.Dequeue();
                //Process input of last received motor state.
                DoMovement(state);

                PlayerState responseState = new PlayerState
                {
                    FixedFrame = state.FixedFrame,
                    Position = transform.position,
                    Rotation = new Vector2(rotationX, rotationY),
                    Velocity = velocity,
                    WishJump = wishJump,
                    TimingStepChange = timingStepChange
                };

                //Send results back to the owner.
                TargetServerStateUpdate(base.connectionToClient, responseState);
            }
            //If there is no input to process.
            else if (timingStepChange != 0)
            {
                //Send timing step change to owner.
                TargetChangeTimingStep(base.connectionToClient, timingStepChange);
            }
        }

        [Client]
        private void SendInputs()
        {
            //Calculate average of look movements
            Vector2 lookMoveAverage = Vector2.zero;
            foreach (Vector2 lookMovement in storedInput.LookMovements)
            {
                Vector2 movement = lookMovement;
                if(float.IsNaN(lookMovement.x) || float.IsNaN(lookMovement.y))
                    movement = Vector2.zero;

                lookMoveAverage += movement;
            }

            lookMoveAverage /= storedInput.LookMovements.Count;
            
            //Sometimes is can be NaN
            if (float.IsNaN(lookMoveAverage.x))
                lookMoveAverage.x = 0;
            if (float.IsNaN(lookMoveAverage.y))
                lookMoveAverage.y = 0;
            
            storedInput.LookMovements.Clear();
            
            PlayerInputs state = new PlayerInputs
            {
                FixedFrame = FixedUpdateManager.FixedFrame,
                LookDir = lookMoveAverage,
                MovementDir = storedInput.MovementDir,
                WishJump = storedInput.Jump
            };
            clientMotorStates.Add(state);

            //Only send at most up to client motor states count.
            int targetArraySize = Mathf.Min(clientMotorStates.Count, 1 + PAST_STATES_TO_SEND);
            //Resize array to accomodate 
            PlayerInputs[] statesToSend = new PlayerInputs[targetArraySize];
            
            // Start at the end of cached inputs, and add to the end of inputs to send. This will add the older inputs first.
            for (int i = 0; i < targetArraySize; i++)
            {
                //Add from the end of states first.
                statesToSend[targetArraySize - 1 - i] = clientMotorStates[clientMotorStates.Count - 1 - i];
            }

            DoMovement(state);
            CmdSendInputs(statesToSend);
        }
        
        [Command(channel = Channels.Unreliable)]
        private void CmdSendInputs(PlayerInputs[] states)
        {
            //No states to process.
            if (states == null || states.Length == 0)
                return;
            
            //Only for client host.
            if (isClient && hasAuthority)
                return;

            /* Go through every new state and if the fixed frame
             * for that state is newer than the last received
             * fixed frame then add it to motor states. */
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].FixedFrame > lastClientStateReceived)
                {
                    receivedClientMotorStates.Enqueue(states[i]);
                    lastClientStateReceived = states[i].FixedFrame;
                }
            }

            while (receivedClientMotorStates.Count > MAXIMUM_RECEIVED_CLIENT_MOTOR_STATES)
                receivedClientMotorStates.Dequeue();
        }
        
        [TargetRpc(channel = Channels.Unreliable)]
        private void TargetServerStateUpdate(NetworkConnection conn, PlayerState state)
        {
            //Exit if received state is older than most current.
            if (receivedServerMotorState != null && state.FixedFrame < receivedServerMotorState.Value.FixedFrame)
                return;

            receivedServerMotorState = state;
        }
        
        [TargetRpc(channel = Channels.Unreliable)]
        private void TargetChangeTimingStep(NetworkConnection conn, sbyte steps)
        {
            FixedUpdateManager.AddTiming(steps);
        }

        #region Movement

        private void DoMovement(PlayerInputs input)
        {
            input.MovementDir.x = input.MovementDir.x.PreciseSign();
            input.MovementDir.y = input.MovementDir.y.PreciseSign();
            
            bool isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask);
            wishJump = input.WishJump;
            
            if(isGrounded)
                GroundMove(input);
            else
                AirMove(input);

            //Mouse movement
            rotationX -= input.LookDir.x;
            rotationY += input.LookDir.y;

            rotationX = Mathf.Clamp(rotationX, -90, 90);

            //Move character
            characterController.Move(velocity * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0, rotationY, 0);
            cameraHolderTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
            
            if(isLocalPlayer && cameraRoll != null)
                cameraRoll.SetVelocity(velocity);
        }
        
        private void GroundMove(PlayerInputs input)
        {
            //Do not apply friction if the player is queueing up the next jump
            if (!wishJump)
                ApplyFriction(true, 1.0f);
            else
                ApplyFriction(true, 0f);

            Vector3 wishDir = new Vector3(input.MovementDir.x, 0f, input.MovementDir.y);
            wishDir = transform.TransformDirection(wishDir);
            wishDir.Normalize();

            float wishSpeed = wishDir.magnitude;
            wishSpeed *= moveSpeed;

            Accelerate(wishDir, wishSpeed, runAcceleration);

            //Reset the gravity velocity
            velocity.y = -gravityAmount * Time.deltaTime;

            if(wishJump)
            {
                velocity.y = jumpHeight;
                wishJump = false;
            }
        }
        
        private void AirMove(PlayerInputs input)
        {
            Vector3 wishDir = new Vector3(input.MovementDir.x, 0f, input.MovementDir.y);
            wishDir = transform.TransformDirection(wishDir);

            float wishSpeed = wishDir.magnitude;
            wishSpeed *= moveSpeed;

            wishDir.Normalize();

            float wishSpeed2 = wishSpeed;
            float accel = Vector3.Dot(velocity, wishDir) < 0 ? airDecceleration : airAcceleration;
			
            //If the player is ONLY strafing left or right
            if(input.MovementDir.y == 0 && input.MovementDir.x != 0)
            {
                if(wishSpeed > sideStrafeSpeed)
                    wishSpeed = sideStrafeSpeed;
				
                accel = sideStrafeAcceleration;
            }

            Accelerate(wishDir, wishSpeed, accel);
            if(airControl > 0)
                AirControl(input, wishDir, wishSpeed2);
			
            velocity.y -= gravityAmount * Time.deltaTime;
        }
        
        private void AirControl(PlayerInputs input, Vector3 wishDir, float wishSpeed)
        {
            //Can't control movement if not moving forward or backward
            if(Mathf.Abs(input.MovementDir.y) < 0.001 || Mathf.Abs(wishSpeed) < 0.001)
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
			
            if(isGrounded)
            {
                float control = speed < runDeacceleration ? runDeacceleration : speed;
                drop = control * friction * Time.deltaTime * t;
            }

            float newSpeed = speed - drop;
            if(newSpeed < 0)
                newSpeed = 0;
            if(speed > 0)
                newSpeed /= speed;

            velocity.x *= newSpeed;
            velocity.z *= newSpeed;
        }
        
        private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if(addSpeed <= 0)
                return;
			
            float accelSpeed = accel * Time.fixedDeltaTime * wishSpeed;
            if(accelSpeed > addSpeed)
                accelSpeed = addSpeed;

            velocity.x += accelSpeed * wishDir.x;
            velocity.z += accelSpeed * wishDir.z;
        }

        #endregion
        
        private class InputData
        {
            public bool Jump;
            
            public Vector2 MovementDir;

            public SamplingStack<Vector2> LookMovements = new SamplingStack<Vector2>(64);
        }
    }
}