// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Console.TypeReader
{
	/// <summary>
	///     The interface for a type reader
	/// </summary>
	internal interface ITypeReader
	{
		/// <summary>
		///     Read the type and return it
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		object ReadType(string input);
	}
}