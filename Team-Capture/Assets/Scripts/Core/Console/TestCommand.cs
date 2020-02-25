using UnityEngine;

namespace Core.Console
{
	public class TestCommand : MonoBehaviour
	{
		[ConCommand("test", "This is a test command!")]
		public static void Command(string[] args)
		{
			Logger.Logger.Log("Test!");
		}
	}
}
