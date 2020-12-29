using Mirror;
using UnityEngine;

namespace Team_Capture.Core.Networking.MessagePack
{
	public struct WeaponShootEffectsTargets : NetworkMessage
	{
		public Vector3[] Targets;

		public Vector3[] TargetNormals;
	}
}