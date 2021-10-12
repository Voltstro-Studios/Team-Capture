// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Team_Capture.Core
{
    [Preserve]
    public class FixedUpdateManager : MonoBehaviour
    {
        /// <summary>
        /// Maximum percentage timing may vary from the FixedDeltaTime.
        /// </summary>
        private const float MAXIMUM_OFFSET_PERCENT = 0.35f;
        
        /// <summary>
        /// How quickly timing can recover to it's default value.
        /// </summary>
        private const float TIMING_RECOVER_RATE = 0.0025f;
        
        /// <summary>
        /// Percentage of FixedDeltaTime to modify timing by when a step must occur.
        /// </summary>
        public const float TimingStepPercent = 0.015f;
        
        /// <summary>
        /// Dispatched before a simulated fixed update occurs.
        /// </summary>
        public static event Action OnPreFixedUpdate;
        
        /// <summary>
        /// Dispatched when a simulated fixed update occurs.
        /// </summary>
        public static event Action OnFixedUpdate;
        
        /// <summary>
        /// Dispatched after a simulated fixed update occurs. Physics would have simulated prior to this event.
        /// </summary>
        public static event Action OnPostFixedUpdate;
        
        /// <summary>
        /// Current fixed frame. Applied before any events are invoked.
        /// </summary>
        public static uint FixedFrame { get; private set; } = 0;
        
        /// <summary>
        /// Ticks applied from updates.
        /// </summary>
        private float updateTicks = 0f;
        
        /// <summary>
        /// Range which the timing may reside within.
        /// </summary>
        private static float[] timingRange;
        
        /// <summary>
        /// Value to change timing per step.
        /// </summary>
        private static float timingPerStep;
        
        /// <summary>
        /// Current FixedUpdate timing.
        /// </summary>
        private static float adjustedFixedUpdate;

        private void Update()
        {
            UpdateTicks(Time.deltaTime);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            GameObject go = new GameObject("Fixed Update Manager");
            go.AddComponent<FixedUpdateManager>();
            DontDestroyOnLoad(go);

            Physics.autoSimulation = false;
            Physics2D.simulationMode = SimulationMode2D.Script;

            adjustedFixedUpdate = Time.fixedDeltaTime;
            timingPerStep = Time.fixedDeltaTime * TimingStepPercent;
            timingRange = new []
            {
                Time.fixedDeltaTime * (1f - MAXIMUM_OFFSET_PERCENT),
                Time.fixedDeltaTime * (1f + MAXIMUM_OFFSET_PERCENT)
            };
        }

        /// <summary>
        ///     Adds onto AdjustedFixedDeltaTime.
        /// </summary>
        /// <param name="steps"></param>
        public static void AddTiming(sbyte steps)
        {
            if (steps == 0)
                return;

            adjustedFixedUpdate = Mathf.Clamp(adjustedFixedUpdate + (steps * timingPerStep), timingRange[0], timingRange[1]);
        }

        /// <summary>
        ///     Adds the current deltaTime to update ticks and processes simulated fixed update.
        /// </summary>
        private void UpdateTicks(float deltaTime)
        {
            updateTicks += deltaTime;
            while (updateTicks >= adjustedFixedUpdate)
            {
                updateTicks -= adjustedFixedUpdate;
                
                //If at maximum value then reset fixed frame.
                //This would probably break the game but even at 128t/s
                //it would take over a year of the server running straight to ever reach this value!
                if (FixedFrame == uint.MaxValue)
                    FixedFrame = 0;
                
                FixedFrame++;

                OnPreFixedUpdate?.Invoke();
                OnFixedUpdate?.Invoke();

                Physics2D.Simulate(Time.fixedDeltaTime);
                Physics.Simulate(Time.fixedDeltaTime);

                OnPostFixedUpdate?.Invoke();
            }

            //Recover timing towards default fixedDeltaTime.
            adjustedFixedUpdate = Mathf.MoveTowards(adjustedFixedUpdate, Time.fixedDeltaTime, TIMING_RECOVER_RATE * deltaTime);
        }
    }
}