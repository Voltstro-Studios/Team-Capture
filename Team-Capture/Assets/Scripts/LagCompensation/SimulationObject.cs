using System;
using System.Collections.Generic;
using Core.Networking;
using UnityEngine;

namespace LagCompensation
{
	internal class SimulationObject : MonoBehaviour
	{
		/// <summary>
		///     A <see cref="Dictionary{TKey,TValue}" /> containing the stored <see cref="SimulationFrameData" />, accessible by
		///     frame index (stored in
		///     <see cref="StoredFrames" />).
		/// </summary>
		[NonSerialized] public readonly Dictionary<int, SimulationFrameData> FrameData = new Dictionary<int, SimulationFrameData>();

		private readonly SimulationFrameData savedFrameData = new SimulationFrameData();

		//TODO: Make this have a getter returning a IReadonlyCollection, and hide this one
		/// <summary>
		/// A list of the frames available in <see cref="FrameData"/>
		/// </summary>
		[NonSerialized] public readonly List<int> StoredFrames = new List<int>();

#if UNITY_EDITOR
		[SerializeField] private bool showPreviousPositionsGizmos;

		private void OnDrawGizmos()
		{
			if (!showPreviousPositionsGizmos) return;

			for (int i = 0; i < StoredFrames.Count; i++)
			{
				//Get the decimal part. Shorthand for f = f % 1
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(FrameData[StoredFrames[i]].Position, 0.5f);
			}
		}
#endif

		private void Start()
		{
			SimulationHelper.SimulationObjects.Add(this);
		}

		private void OnDestroy()
		{
			SimulationHelper.SimulationObjects.Remove(this);
		}

		/// <summary>
		/// Creates and stores a new <see cref="SimulationFrameData"/> using the current position and rotation data
		/// </summary>
		public void AddFrame()
		{
			//If we've got too many frames stored
			if (StoredFrames.Count >= TCNetworkManager.Instance.maxFrameCount)
			{
				int oldestFrameIndex = StoredFrames[0];
				StoredFrames.RemoveAt(0);
				FrameData.Remove(oldestFrameIndex);
			}

			Transform t = transform;
			FrameData.Add(SimulationHelper.CurrentFrame, new SimulationFrameData
			{
				Position = t.position,
				Rotation = t.rotation
			});
			StoredFrames.Add(SimulationHelper.CurrentFrame);
		}

		/// <summary>
		///     <para>
		///     Sets the <see cref="GameObject"/>s <see cref="Transform"/> position and rotation using the given frame index,
		///     and optional interpolation value.
		///     </para>
		///     <para>
		///     The <see cref="Transform"/> will not be changed back until a call to <see cref="ResetStateTransform"/>
		///     </para>
		/// </summary>
		/// <param name="frameId">The frame from which the <see cref="Transform"/> is set</param>
		/// <param name="nextFrameInterpolation">A value used to interpolate between the selected frame and the next one</param>
		public void SetStateTransform(int frameId, float nextFrameInterpolation)
		{
			//Saves us repeatedly calling the getter
			Transform t = transform;
			savedFrameData.Position = t.position;
			savedFrameData.Rotation = t.rotation;

			t.position = Vector3.Lerp(FrameData[frameId - 1].Position, FrameData[frameId].Position,
				nextFrameInterpolation);
			t.rotation = FrameData[frameId - 1].Rotation;
		}

		/// <summary>
		/// Resets the <see cref="Transform"/> to the position before the call to <see cref="SetStateTransform"/>
		/// </summary>
		public void ResetStateTransform()
		{
			Transform t = transform;
			t.position = savedFrameData.Position;
			t.rotation = savedFrameData.Rotation;
		}
	}
}