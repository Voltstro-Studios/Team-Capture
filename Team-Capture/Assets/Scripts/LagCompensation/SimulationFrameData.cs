using UnityEngine;

//Todo: make this readonly and use a constructor
namespace LagCompensation
{
	public class SimulationFrameData
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
	}
}
