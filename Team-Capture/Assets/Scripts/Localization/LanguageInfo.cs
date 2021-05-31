// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using UnityEngine;

namespace Team_Capture.Localization
{
	/// <summary>
	///     Something that is spoken, called language
	/// </summary>
	public enum LanguageInfo : byte
	{
		/// <summary>
		///     The language is not loaded
		/// </summary>
		Unavailable = 0,

		/// <summary>
		///     No language is selected
		/// </summary>
		None = 1,

		/// <summary>
		///     The English language
		/// </summary>
		English = 2,

		/// <summary>
		///     The LolCat language. <a href="https://en.wikipedia.org/wiki/Lolcat">Yes, it exists...</a>
		/// </summary>
		LolCat = 3
	}

	/// <summary>
	///     Provides some extension methods for language
	/// </summary>
	public static class LanguageExtensions
	{
		/// <summary>
		///     Converts Unity's <see cref="SystemLanguage" /> to our <see cref="LanguageInfo" />
		/// </summary>
		/// <param name="systemLanguage"></param>
		/// <returns></returns>
		public static LanguageInfo ToLanguageInfo(this SystemLanguage systemLanguage)
		{
			switch (systemLanguage)
			{
				case SystemLanguage.English:
					return LanguageInfo.English;
				default:
					return LanguageInfo.None;
			}
		}
	}
}