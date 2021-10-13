// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using Mirror;
using Team_Capture.Player.Movement.States;
using UnityEngine;

namespace Team_Capture.Player.Movement
{
    //Code built on Mirror's built-in NetworkTransformBase
    
    internal class PlayerTransformSync : NetworkBehaviour
    {
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform cameraTransform;

        [Header("Synchronization")]
        [Range(0, 1)] public float sendInterval = 0.050f;

        [Header("Interpolation")]
        public bool interpolatePosition = true;
        public bool interpolateRotation = true;

        [Header("Buffering")]
        [Tooltip("Snapshots are buffered for sendInterval * multiplier seconds. If your expected client base is to run at non-ideal connection quality (2-5% packet loss), 3x supposedly works best.")]
        public int bufferTimeMultiplier = 1;
        
        [Tooltip("Buffer size limit to avoid ever growing list memory consumption attacks.")]
        public int bufferSizeLimit = 64;

        [Tooltip("Start to accelerate interpolation if buffer size is >= threshold. Needs to be larger than bufferTimeMultiplier.")]
        public int catchupThreshold = 4;

        [Tooltip("Once buffer is larger catchupThreshold, accelerate by multiplier % per excess entry.")]
        [Range(0, 1)] public float catchupMultiplier = 0.10f;
        
        private readonly SortedList<double, PlayerTransformSnapshot> clientBuffer = new SortedList<double, PlayerTransformSnapshot>();
        
        private readonly Func<PlayerTransformSnapshot, PlayerTransformSnapshot, double, PlayerTransformSnapshot> interpolate 
            = PlayerTransformSnapshot.Interpolate;

        private float BufferTime => sendInterval * bufferTimeMultiplier;
        
        private double clientInterpolationTime;
        
        private double lastClientSendTime;
        private double lastServerSendTime;
        
        private PlayerTransformSnapshot ConstructSnapshot()
        {
            return new PlayerTransformSnapshot(
                NetworkTime.localTime,
                0,
                rootTransform.localPosition,
                new Vector2(cameraTransform.localRotation.eulerAngles.x, rootTransform.localRotation.eulerAngles.y)
            );
        }
        
        private void ApplySnapshot(PlayerTransformSnapshot start, PlayerTransformSnapshot goal, PlayerTransformSnapshot interpolated)
        {
            rootTransform.localPosition = interpolatePosition ? interpolated.position : goal.position;
            
            rootTransform.localRotation = Quaternion.Euler(0, interpolateRotation ? interpolated.rotation.y : goal.rotation.y, 0);
            cameraTransform.localRotation = Quaternion.Euler(interpolateRotation ? interpolated.rotation.x : goal.rotation.x, 0, 0);
        }

        [ClientRpc(channel = Channels.Unreliable, includeOwner = false)]
        private void RpcServerToClientSync(Vector3 position, Vector2 rotation) => OnServerToClientSync(position, rotation);
        
        private void OnServerToClientSync(Vector3 position, Vector2 rotation)
        {
            if (isServer) 
                return;

            //Protect against ever growing buffer size attacks
            if (clientBuffer.Count >= bufferSizeLimit) 
                return;
            
            double timestamp = NetworkClient.connection.remoteTimeStamp;
            
            //Construct snapshot with batch timestamp to save bandwidth
            PlayerTransformSnapshot snapshot = new PlayerTransformSnapshot(
                timestamp,
                NetworkTime.localTime,
                position, rotation);

            //Add to buffer (or drop if older than first element)
            SnapshotInterpolation.InsertIfNewEnough(snapshot, clientBuffer);
        }
        
        private void UpdateServer()
        {
            if (NetworkTime.localTime >= lastServerSendTime + sendInterval)
            {
                //Send snapshot without timestamp.
                //Receiver gets it from batch timestamp to save bandwidth.
                PlayerTransformSnapshot snapshot = ConstructSnapshot();
                RpcServerToClientSync(
                    // only sync what the user wants to sync
                    snapshot.position,
                    snapshot.rotation);

                lastServerSendTime = NetworkTime.localTime;
            }
        }

        private void UpdateClient()
        {
            //Compute snapshot interpolation & apply if any was spit out
            //TODO: We don't have Time.deltaTime double yet. float is fine.
            if (SnapshotInterpolation.Compute(
                NetworkTime.localTime, Time.deltaTime,
                ref clientInterpolationTime,
                BufferTime, clientBuffer,
                catchupThreshold, catchupMultiplier,
                interpolate,
                out PlayerTransformSnapshot computed))
            {
                PlayerTransformSnapshot start = clientBuffer.Values[0];
                PlayerTransformSnapshot goal = clientBuffer.Values[1];
                ApplySnapshot(start, goal, computed);
            }
        }

        private void Update()
        {
            //If server then always sync to others.
            if (isServer) 
                UpdateServer();
            //'else if' because host mode shouldn't send anything to server.
            //it is the server. don't overwrite anything there.
            else if (isClient) 
                UpdateClient();
        }
        
        private void Reset()
        {
            clientBuffer.Clear();
            clientInterpolationTime = 0;
        }

        internal void SetLocation()
        {
            Reset();
        }

        private void OnDisable() => Reset();
        private void OnEnable() => Reset();

        private void OnValidate()
        {
            //Wake sure that catchup threshold is > buffer multiplier.
            //For a buffer multiplier of '3', we usually have at _least_ 3
            //buffered snapshots. often 4-5 even.
            //
            //catchUpThreshold should be a minimum of bufferTimeMultiplier + 3,
            //to prevent clashes with SnapshotInterpolation looking for at least
            //3 old enough buffers, else catch up will be implemented while there
            //is not enough old buffers, and will result in jitter.
            //(validated with several real world tests by ninja & imer)
            catchupThreshold = Mathf.Max(bufferTimeMultiplier + 3, catchupThreshold);

            //Buffer limit should be at least multiplier to have enough in there
            bufferSizeLimit = Mathf.Max(bufferTimeMultiplier, bufferSizeLimit);
        }
    }
}
