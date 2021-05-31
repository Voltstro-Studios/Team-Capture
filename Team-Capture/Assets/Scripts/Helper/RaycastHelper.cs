// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;

namespace Team_Capture.Helper
{
	/// <summary>
	///     Additional methods to help with ray-casting
	/// </summary>
	internal static class RaycastHelper
	{
		/// <summary>
		///     Does a Physics.RaycastAll, but sorted by distance
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="direction"></param>
		/// <param name="maxDirection"></param>
		/// <param name="layerMask"></param>
		/// <returns></returns>
		public static RaycastHit[] RaycastAllSorted(Vector3 origin, Vector3 direction, float maxDirection,
			int layerMask)
		{
			// ReSharper disable once Unity.PreferNonAllocApi
			RaycastHit[] rays = Physics.RaycastAll(origin, direction, maxDirection, layerMask);
			Array.Sort(rays, (hit, raycastHit) => hit.distance.CompareTo(raycastHit.distance));
			return rays;
		}
	}
}