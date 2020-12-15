using UnityEngine;

namespace Localization
{
	/// <summary>
	///		Something that is spoken, called language
	/// </summary>
	public enum LanguageInfo : byte
	{
		Unable = 0,
		None = 1,
		English = 2,
		LolCat = 3
	}

	public static class LanguageExtensions
	{
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