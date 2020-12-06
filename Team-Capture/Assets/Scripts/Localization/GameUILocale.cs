using Core;

namespace Localization
{
	/// <summary>
	///		Handles localization for the GameUI
	/// </summary>
	public static class GameUILocale
	{
		private static readonly string GameUiLocaleLocation = $"{Game.GetGameExecutePath()}/Resources/GameUI-%LANG%.json";

		private static Locale gameUiLocale;

		/// <summary>
		///		Resolves a string
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static string ResolveString(string id)
		{
			//If the GameUI locale doesn't exist, create it
			if(gameUiLocale == null)
				gameUiLocale = new Locale(GameUiLocaleLocation);

			return gameUiLocale.ResolveString(id);
		}
	}
}