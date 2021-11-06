// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using UnityEngine;

namespace Team_Capture.Weapons.Effects
{
    public struct DefaultEffectsMessage : IEffectsMessage
    {
        public DefaultEffectsMessage(NetworkReader reader)
        {
            Targets = reader.ReadArray<Vector3>();
            TargetsNormals = reader.ReadArray<Vector3>();
        }

        public DefaultEffectsMessage(Vector3[] targets, Vector3[] targetsNormals)
        {
            Targets = targets;
            TargetsNormals = targetsNormals;
        }
        
        public EffectsType EffectsType => EffectsType.Default;
        
        public Vector3[] Targets;
        public Vector3[] TargetsNormals;
        
        public void Serialize(NetworkWriter writer)
        {
            writer.WriteArray(Targets);
            writer.WriteArray(TargetsNormals);
        }
    }
}