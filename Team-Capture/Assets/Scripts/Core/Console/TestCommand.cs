using UnityEngine;

namespace Core.Console
{
	public class TestCommand : MonoBehaviour
	{
		[ConCommand(Name = "test", Summary = "This is a test command!")]
		public static void Command(string[] args)
		{
			Logger.Logger.Log("Test!");
		}
	}
}
