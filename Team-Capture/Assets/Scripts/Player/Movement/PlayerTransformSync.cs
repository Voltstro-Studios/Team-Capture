// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Mirror;
using UnityEngine;

namespace Team_Capture.Player.Movement
{
    public class PlayerTransformSync : NetworkTransformBase
    {
        protected override Transform targetComponent => transform;

        protected override void OnServerToClientSync(Vector3? position, Quaternion? rotation, Vector3? scale)
        {
            //TODO: We should modify Mirror to not send to owner
            if(hasAuthority)
                return;
            
            base.OnServerToClientSync(position, rotation, scale);
        }
    }
}
