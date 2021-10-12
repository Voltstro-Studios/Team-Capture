// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using Mirror;
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
        
        [SerializeField] private float moveRate = 18f;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private GameObject gfxObject;
        
        private CharacterController characterController;
        
        private readonly List<PlayerInputs> clientMotorStates = new List<PlayerInputs>();
        
        private readonly Queue<PlayerInputs> receivedClientMotorStates = new Queue<PlayerInputs>();
        
        private uint lastClientStateReceived;
        
        private PlayerState? receivedServerMotorState;

        private readonly InputData storedInput = new InputData();

        private Vector3 velocity;
        private float rotationX;
        private float rotationY;

        public override void OnStartServer()
        {
            Setup();
        }

        public override void OnStartClient()
        {
            Setup();
        }

        public override void OnStartAuthority()
        {
            gfxObject.AddComponent<SmoothToZero>();
        }

        private void OnEnable()
        {
            FixedUpdateManager.OnFixedUpdate += OnFixedUpdate;
        }

        private void OnDisable()
        {
            FixedUpdateManager.OnFixedUpdate -= OnFixedUpdate;
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
        
        private void Setup()
        {
            characterController = GetComponent<CharacterController>();
            if (!isServer && !hasAuthority)
                characterController.enabled = false;
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
            cameraTransform.rotation = Quaternion.Euler(serverState.Rotation.x, serverState.Rotation.y, 0);
            velocity = serverState.Velocity;
            rotationX = serverState.Rotation.x;
            rotationY = serverState.Rotation.y;
            
            Physics.SyncTransforms();

            foreach (PlayerInputs clientState in clientMotorStates)
                ProcessInputs(clientState);
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
                ProcessInputs(state);

                PlayerState responseState = new PlayerState
                {
                    FixedFrame = state.FixedFrame,
                    Position = transform.position,
                    Rotation = new Vector2(rotationX, rotationY),
                    Velocity = velocity,
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
        
        private void ProcessInputs(PlayerInputs motorState)
        {
            motorState.MovementDir.x = motorState.MovementDir.x.PreciseSign();
            motorState.MovementDir.y = motorState.MovementDir.y.PreciseSign();

            rotationX -= motorState.LookDir.x;
            rotationY += motorState.LookDir.y;

            rotationX = Mathf.Clamp(rotationX, -90, 90);

            //Add move direction.
            Vector3 moveDirection = new Vector3(motorState.MovementDir.x, 0f, motorState.MovementDir.y) * moveRate;
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= Time.fixedDeltaTime;
            
            //Move character.
            characterController.Move(moveDirection);
            transform.rotation = Quaternion.Euler(0, rotationY, 0);
            cameraTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        }

        public void SetInput(float horizontal, float vertical, float lookX, float lookY)
        {
            storedInput.MovementDir = new Vector2(horizontal, vertical);
            storedInput.LookDir = new Vector2(lookY, lookX);
        }
        
        [Client]
        private void SendInputs()
        {
            PlayerInputs state = new PlayerInputs
            {
                FixedFrame = FixedUpdateManager.FixedFrame,
                LookDir = storedInput.LookDir,
                MovementDir = storedInput.MovementDir
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

            ProcessInputs(state);
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
        
        private class InputData
        {
            public bool Jump;

            public Vector2 LookDir;
            public Vector2 MovementDir;
        }
    }
}