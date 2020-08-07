using Core;
using SceneManagement;
using UI;
using UnityEngine;
using UnityEngine.Rendering;
using Logger = Core.Logging.Logger;

namespace Console
{
	/// <summary>
	/// Sets up what console to use (if it is in-game GUI, or a terminal)
	/// </summary>
	internal class ConsoleSetup : MonoBehaviour
	{
		[SerializeField] private GameObject consoleUiPrefab;

		[SerializeField] private string sceneToLoadToNext = "StartVideo";
		[SerializeField] private string sceneToLoadToNextHeadless = "MainMenu";

		internal static IConsoleUI ConsoleUI;

		private void Awake()
		{
			if (ConsoleUI != null)
			{
				Destroy(gameObject);
				Logger.Warn("You should only ever load this script on a bootloader scene!");
				return;
			}

			DontDestroyOnLoad(gameObject);

			//If we are headless we need to create a console UI using the OS's terminal
			//I really which Unity would have this included...
			if (Game.IsHeadless)
			{
#if UNITY_STANDALONE_WIN
				ConsoleUI = new ConsoleWindows($"{Application.productName} Server");
#elif UNITY_STANDALONE_LINUX
				//TODO: Add console for Linux
#elif UNITY_STANDALONE_OSX
				//TODO: Add console for OSX
#endif
			}
			else
			{
				//Create in-game console GUI
				ConsoleUI = Instantiate(consoleUiPrefab, transform).GetComponent<ConsoleGUI>();
			}

			//Init the console
			ConsoleUI.Init();

			//Register commands
			ConsoleBackend.RegisterCommands();

			TCScenesManager.LoadScene(Game.IsHeadless
				? TCScenesManager.FindSceneInfo(sceneToLoadToNextHeadless)
				: TCScenesManager.FindSceneInfo(sceneToLoadToNext));
		}

		private void Update()
		{
			ConsoleUI.UpdateConsole();
		}
	}
}