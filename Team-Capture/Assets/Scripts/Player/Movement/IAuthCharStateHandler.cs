namespace Player.Movement
{
	public interface IAuthCharStateHandler
	{
		void OnStateChange(CharacterState newState);
	}
}