using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Logger = Core.Logging.Logger;

namespace Localization
{
	public class Locale
	{
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

				Logger.Warn("No locale exists for system language {@Language}... defaulting to english!", Language.ToString());
			}
			
			//Now to load the tokens
			Tokens = JsonConvert.DeserializeObject<Dictionary<string, string>>(
				File.ReadAllText(systemLanguageLocaleLocation));
		}
		
		private int language = -1;

		/// <summary>
		///		Gets the language that this local is for
		/// </summary>
		public SystemLanguage Language
		{
			get
			{
				if (language == -1)
					language = (int) Application.systemLanguage;

				return (SystemLanguage) language;
			}
		}

		public Dictionary<string, string> Tokens { get; }

		public string ResolveString(string id)
		{
			if (!Tokens.ContainsKey(id))
			{
				Logger.Warn("No key for {@ID} exists in localization!", id);
				return id;
			}

			return Tokens[id];
		}
	}
}