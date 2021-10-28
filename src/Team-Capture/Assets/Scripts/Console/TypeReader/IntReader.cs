// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Globalization;

namespace Team_Capture.Console.TypeReader
{
    /// <summary>
    ///     A default reader for <see cref="int" />
    /// </summary>
    internal sealed class IntReader : ITypeReader
    {
        public object ReadType(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            return int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int result) ? result : 0;
        }
    }
}