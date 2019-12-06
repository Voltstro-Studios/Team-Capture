using Player;
using UnityEngine;

// ReSharper disable BuiltInTypeReferenceStyle

namespace LagCompensation
{
    internal partial class Simulator
    {
        /// <summary>
        ///     Keeps a record of what position a player was in at what time
        /// </summary>
        private class PlayerPositionRecord
        {
            internal PlayerManager PlayerManager;
            internal Vector3 PlayerPosition;
        }
    }
}