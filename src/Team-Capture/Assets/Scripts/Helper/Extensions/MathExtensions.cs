// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.Helper.Extensions
{
    /// <summary>
    ///     Includes additional maths methods
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        ///     Returns negative-one, zero, or postive-one of a value instead of just negative-one or positive-one.
        /// </summary>
        /// <param name="value">Value to sign.</param>
        /// <returns>Precise sign.</returns>
        public static float PreciseSign(this float value)
        {
            return value == 0f ? 0f : Mathf.Sign(value);
        }
    }
}