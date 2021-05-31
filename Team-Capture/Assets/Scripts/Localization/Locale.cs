// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityCommandLineParser;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Localization
{
	/// <summary>
	///     A locale, handles loading locale files for different language based off user preference or by the system language.
	/// </summary>
	public class Locale
	{
		[CommandLineArgument("language")] internal static LanguageInfo OverrideLanguage = LanguageInfo.Unavailable;

		private LanguageInfo language = LanguageInfo.Unavailable;

		/// <summary>
		///     Creates a new <see cref="Locale" /> instance
		/// </summary>
		/// <param name="fileLocation"></param>
		public Locale(string fileLocation)
		{
			//Try and load the locale for the native system language
			string systemLanguageLocaleLocation = fileLocation.Replace("%LANG%", Language.ToString());
			if (!File.Exists(systemLanguageLocaleLocation)) //The locale doesn't exist for the system language
			{
				//Try and default to english (Should generally always exist, as Voltstro Studios is english...)
				systemLanguageLocaleLocation = fileLocation.Replace("%LANG%", "English");
				if (!File.Exists(systemLanguageLocaleLocation))
				{
					Logger.Error("No locale exists at {@LocalLocation}! The locale will not be loaded!", fileLocation);
					Tokens = new Dictionary<string, string>();
					return;
				}

				Logger.Warn("No locale exists for system language {@Language}... defaulting to english!",
					Language.ToString());
			}

			//Now to load the tokens
			Tokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(
				File.ReadAllText(systemLanguageLocaleLocation));
		}

		/// <summary>
		///     Gets the language that this local is for
		/// </summary>
		public LanguageInfo Language
		{
			get
			{
				if (language == LanguageInfo.Unavailable)
					language = OverrideLanguage == LanguageInfo.Unavailable
						? Application.systemLanguage.ToLanguageInfo()
						: OverrideLanguage;

				return language;
			}
		}

		/// <summary>
		///     All the available tokens
		/// </summary>
		public Dictionary<string, string> Tokens { get; }

		/// <summary>
		///     Resolves the string
		///     <para>
		///         If the <see cref="id" /> doesn't exist in the <see cref="Tokens" /> dictionary then the <see cref="id" />
		///         will be returned.
		///     </para>
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public string ResolveString(string id)
		{
			return Tokens.ContainsKey(id) ? Tokens[id] : id;
		}
	}
}