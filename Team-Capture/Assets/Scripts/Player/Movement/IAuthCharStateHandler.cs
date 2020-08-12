namespace Player.Movement
{
	/// <summary>
	/// Handles player's state
	/// </summary>
	public interface IAuthCharStateHandler
	{
		void OnStateChange(CharacterState newState);
	}
}