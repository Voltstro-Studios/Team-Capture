// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Console.TypeReader
{
	/// <summary>
	///     A default reader for <see cref="string" />
	/// </summary>
	internal sealed class StringReader : ITypeReader
	{
		public object ReadType(string input)
		{
			return input;
		}
	}
}