// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.Player.Movement.States
{
    internal struct PlayerInputs
    {
        public uint FixedFrame;

        public Vector2 MovementDir;
        public Vector2 LookDir;

        public bool WishJump;
    }
}