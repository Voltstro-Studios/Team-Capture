using System.Collections;
using UnityEngine;

namespace LagCompensation
{
    internal partial class Simulator
    {
        public class SimulatorBehaviour : MonoBehaviour
        {
            private void Awake() => StartCoroutine(UpdateSimulatorPositions());

            private static IEnumerator UpdateSimulatorPositions()
            {
                while (true)
                {
                    RecordPlayerPositions();
                    yield return new WaitForSecondsRealtime(1f / TicksPerSecond);
                }

                // ReSharper disable once IteratorNeverReturns
            }
        }
    }
}