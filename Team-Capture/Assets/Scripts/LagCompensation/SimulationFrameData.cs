using UnityEngine;

//Todo: make this readonly and use a constructor
namespace LagCompensation
{
	//Todo: make this readonly and use a constructor
	//TODO: Also add documentation and a scale vector
	internal class SimulationFrameData
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
	}
}