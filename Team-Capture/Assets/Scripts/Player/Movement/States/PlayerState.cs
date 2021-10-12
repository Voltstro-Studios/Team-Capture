// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.Player.Movement.States
{
    internal struct PlayerState
    {
        public uint FixedFrame;
        public sbyte TimingStepChange;
        
        public Vector3 Position;
        public Vector2 Rotation;
        public Vector3 Velocity;
    }
}