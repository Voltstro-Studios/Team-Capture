using NUnit.Framework;
using Team_Capture.Core.Networking;
using Team_Capture.Core.Networking.MessagePack;
using UnityEngine;

namespace Team_Capture.Editor.Tests
{
	public class PacketCompressionTests
	{
		[Test]
		public void PacketSerializeAndDeserializeTest()
		{
			int targetsSize = 100;
			WeaponShootEffectsTargets testMessage = new WeaponShootEffectsTargets
			{
				Targets = new Vector3[targetsSize], TargetNormals = new Vector3[targetsSize]
			};

			for (int i = 0; i < targetsSize; i++)
			{
				testMessage.Targets[i] = new Vector3(1.0f, 1.0f, 1.0f);
				testMessage.TargetNormals[i] = new Vector3(2.0f, 2.0f, 2.0f);
			}

			byte[] bytes = PacketCompression.CompressMessage(testMessage);

			PacketCompression.ReadMessage<WeaponShootEffectsTargets>(bytes);
		}
	}
}