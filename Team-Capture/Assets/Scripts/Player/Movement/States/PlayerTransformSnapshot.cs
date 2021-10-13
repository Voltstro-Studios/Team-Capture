// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using UnityEngine;

namespace Team_Capture.Player.Movement.States
{
    internal struct PlayerTransformSnapshot : Snapshot
    {
        public double remoteTimestamp { get; set; }
        public double localTimestamp { get; set; }

        public Vector3 position;
        public Vector2 rotation;
        
        public PlayerTransformSnapshot(double remoteTimestamp, double localTimestamp, Vector3 position, Vector2 rotation)
        {
            this.remoteTimestamp = remoteTimestamp;
            this.localTimestamp = localTimestamp;
            this.position = position;
            this.rotation = rotation;
        }

        public static PlayerTransformSnapshot Interpolate(PlayerTransformSnapshot from, PlayerTransformSnapshot to, double t)
        {
            return new PlayerTransformSnapshot(0, 0,
                Vector3.LerpUnclamped(from.position, to.position, (float)t),
                Vector2.LerpUnclamped(from.rotation, to.rotation, (float)t));
        }
    }
}