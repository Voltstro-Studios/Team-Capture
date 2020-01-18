using System.Collections.Generic;
using Global;

namespace LagCompensation
{
	public class SimulationHelper
	{
		public static readonly List<SimulationObject> SimulationObjects = new List<SimulationObject>();
		public static int CurrentFrame { get; private set; }

		//Every time this is called, tell all our simulation objects to add a frame
		public static void UpdateSimulationObjectData()
		{
			CurrentFrame++;
			for (int i = 0; i < SimulationObjects.Count; i++) SimulationObjects[i].AddFrame();
		}
	}
}
