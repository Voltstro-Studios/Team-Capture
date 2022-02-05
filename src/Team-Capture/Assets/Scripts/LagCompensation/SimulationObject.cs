// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using Mirror;
using Team_Capture.Collections;
using Team_Capture.Core.Networking;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.LagCompensation
{
    internal class SimulationObject : MonoBehaviour
    {
        private Dictionary<double, SimulationFrameData> frameData;
        private FixedQueue<double> frameKeys;

        private Vector3 lastPosition;
        private Quaternion lastRotation;

        private int maxFrames;

        private void Start()
        {
            maxFrames = TCNetworkManager.Instance.GetMaxFramePoints();
            frameData = new Dictionary<double, SimulationFrameData>();
            frameKeys = new FixedQueue<double>(maxFrames);
            SimulationHelper.SimulationObjects.Add(this);
        }

        private void OnDestroy()
        {
            SimulationHelper.SimulationObjects.Remove(this);
        }

        /// <summary>
        ///     Creates and stores a new <see cref="SimulationFrameData" /> using the current position and rotation data
        /// </summary>
        public void AddFrame()
        {
            if (frameKeys.Count == maxFrames)
                frameData.Remove(frameKeys.Dequeue());

            double time = NetworkTime.time;
            Transform objTransform = transform;
            frameData.Add(time, new SimulationFrameData(objTransform.position, objTransform.rotation));
            frameKeys.Enqueue(time);
        }

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="GameObject" />s <see cref="Transform" /> position and rotation using the given frame index,
        ///         and optional interpolation value.
        ///     </para>
        ///     <para>
        ///         The <see cref="Transform" /> will not be changed back until a call to <see cref="ResetStateTransform" />
        ///     </para>
        /// </summary>
        /// <param name="secondsAgo"></param>
        public void SetStateTransform(double secondsAgo)
        {
            Transform objTransform = transform;
            
            //Save last state
            lastPosition = objTransform.position;
            lastRotation = objTransform.rotation;

            double currentTime = NetworkTime.time;
            double targetTime = currentTime - secondsAgo;

            double previousTime = 0f;
            double nextTime = 0f;
            for (int i = 0; i < frameKeys.Count; i++)
            {
                if (previousTime <= targetTime && frameKeys.ElementAt(i) >= targetTime)
                {
                    nextTime = frameKeys.ElementAt(i);
                    break;
                }
                else
                    previousTime = frameKeys.ElementAt(i);
            }

            if (nextTime == 0)
                    nextTime = frameKeys.GetMostRecentElement();

            double timeBetweenFrames = nextTime - previousTime;
            double timeAwayFromPrevious = currentTime - previousTime;
            
            //We loose some accuracy here, but Unity's transforms are floats
            float lerpProgress = (float)(timeAwayFromPrevious / timeBetweenFrames);
            
            Logger.Debug("TimeAgo: {TimeAgo}, previousTime: {PreviousTime}, nextTime: {NextTime}, lerp: {Lerp}", secondsAgo, previousTime, nextTime, lerpProgress);

            objTransform.position = Vector3.Lerp(frameData[previousTime].Position, frameData[nextTime].Position, lerpProgress);
            objTransform.rotation = Quaternion.Slerp(frameData[previousTime].Rotation, frameData[nextTime].Rotation,
                lerpProgress);
        }

        /// <summary>
        ///     Resets the <see cref="Transform" /> to the position before the call to <see cref="SetStateTransform" />
        /// </summary>
        public void ResetStateTransform()
        {
            Transform t = transform;
            t.position = lastPosition;
            t.rotation = lastRotation;
        }

#if UNITY_EDITOR
        [SerializeField] private bool showPreviousPositionsGizmos;

        private void OnDrawGizmos()
        {
            if (!showPreviousPositionsGizmos) return;

            for (int i = 0; i < frameKeys.Count; i++)
            {
                //Get the decimal part. Shorthand for f = f % 1
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(frameData[frameKeys[i]].Position, 0.5f);
            }
        }
#endif
    }
}