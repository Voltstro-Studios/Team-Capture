// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Team_Capture.Weapons.Jobs
{
    [BurstCompile]
    public struct CreateDirectionsJob : IJobParallelFor
    {
        [ReadOnly] public float SpreadFactor;
        [ReadOnly] public Random Random;

        [WriteOnly] public NativeArray<Vector3> Directions;

        public void Execute(int index)
        {
            Directions[index] = new Vector3(
                Random.NextFloat(-SpreadFactor, SpreadFactor),
                Random.NextFloat(-SpreadFactor, SpreadFactor),
                Random.NextFloat(-SpreadFactor, SpreadFactor));
        }
    }
}