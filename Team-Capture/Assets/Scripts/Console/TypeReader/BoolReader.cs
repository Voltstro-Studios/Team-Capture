// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Globalization;

namespace Team_Capture.Console.TypeReader
{
	/// <summary>
	///     A default reader for <see cref="bool" />
	/// </summary>
	internal sealed class BoolReader : ITypeReader
	{
		public object ReadType(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return true;

			//Check to see if it is just 'true' or 'false'
			input = input.ToLower();
			if (bool.TryParse(input, out bool result)) return result;

			//See if it is just '1' or '0'
			if (!int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int intResult)) return false;

			return intResult == 1;
		}
	}
}