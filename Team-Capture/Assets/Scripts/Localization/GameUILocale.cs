using Core;

namespace Localization
{
	public static class GameUILocale
	{
		private static readonly string GameUiLocaleLocation = $"{Game.GetGameExecutePath()}/Resources/GameUI-%LANG%.json";

		private static Locale gameUiLocale;

		public static string ResolveString(string id)
		{
			if(gameUiLocale == null)
				gameUiLocale = new Locale(GameUiLocaleLocation);

			return gameUiLocale.ResolveString(id);
		}
	}
}