using Structs;

namespace Interfaces
{
	public interface IAuthCharStateHandler
	{
		void OnStateChange(CharacterState newState);
	}
}