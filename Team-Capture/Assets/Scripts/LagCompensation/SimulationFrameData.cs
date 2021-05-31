// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

//Todo: make this readonly and use a constructor
namespace Team_Capture.LagCompensation
{
	//Todo: make this readonly and use a constructor
	//TODO: Also add documentation and a scale vector
	internal class SimulationFrameData
	{
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
	}
}