// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.LagCompensation
{
    /// <summary>
    ///     Contains data about the position and rotation of an object
    /// </summary>
    internal readonly struct LagCompensationFrameData
    {
        public LagCompensationFrameData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
    }
}