namespace Delegates
{
	/// <summary>
	/// A delegate for when a player is killed
	/// </summary>
	/// <param name="playerKilledId">The ID of the player who was killed</param>
	/// <param name="playerKillerId">The ID of the player who killed the player</param>
	public delegate void PlayerKilledDelegate(string playerKilledId, string playerKillerId);
}