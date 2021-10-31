// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Team_Capture.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Logger = Team_Capture.Logging.Logger;

namespace Team_Capture.Console
{
    /// <summary>
    ///     Sets up what console to use (if it is in-game GUI, or a terminal)
    /// </summary>
    [CreateOnInit]
    internal partial class ConsoleSetup : MonoBehaviour
    {
        private const string ConsoleUiPrefabPath = "Assets/Prefabs/UI/ConsoleGUI.prefab";
        
        internal static IConsoleUI ConsoleUI;

        private void Start()
        {
            if (ConsoleUI != null)
            {
                Destroy(gameObject);
                Logger.Warn("You should only ever load this script on a bootloader scene!");
                return;
            }

            //If we are headless we need to create a console UI using the OS's terminal
            //I really which Unity would have this included...
            if (Game.IsHeadless)
            {
#if UNITY_STANDALONE_WIN
				ConsoleUI = new ConsoleWindows($"{Application.productName} Server");
#elif UNITY_STANDALONE_LINUX
                ConsoleUI = new ConsoleLinux($"{Application.productName} Server");
#elif UNITY_STANDALONE_OSX
				//TODO: Add console for OSX
#endif
            }
            else
            {
                GameObject consoleUiPrefab =
                    Addressables.LoadAssetAsync<GameObject>(ConsoleUiPrefabPath).WaitForCompletion();
                
                //Create in-game console GUI
                ConsoleUI = Instantiate(consoleUiPrefab, transform).GetComponent<ConsoleGUI>();
            }

            //Init the console
            ConsoleUI.Init();

            //Init the backend of the console
            ConsoleBackend.InitConsoleBackend();

            //Exec autoexec
            ConsoleBackend.ExecuteFileCommand(new[] {"autoexec"});
        }

        private void Update()
        {
            ConsoleUI.UpdateConsole();
        }
    }
}