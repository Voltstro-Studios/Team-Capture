using System;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Helper;
using Team_Capture.Input;
using Team_Capture.Player.Movement;
using UnityEngine;
using Voltstro.CommandLineParser;

namespace Team_Capture.UI
{
	/// <summary>
	///     A UI used for debugging purposes
	/// </summary>
	internal class DebugMenu : SingletonMonoBehaviour<DebugMenu>
	{
		/// <summary>
		///     Is the debug menu open?
		/// </summary>
		[CommandLineArgument("debugmenu")] [ConVar("cl_debugmenu", "Shows the debug menu", true)]
		public static bool DebugMenuOpen;

		/// <summary>
		///     Reads input
		/// </summary>
		public InputReader inputReader;

		/// <summary>
		///     How often to refresh the fps counter
		/// </summary>
		public float refreshRate = 1f;

		private float fps;

		private float timer;

		private void OnGUI()
		{
			if (!DebugMenuOpen)
				return;

			const string spacer = "===================";

			GUI.skin.label.fontSize = 20;

			float yOffset = 10;

			if (NetworkManager.singleton != null && NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly)
				if (PlayerMovementManager.ShowPos)
					yOffset = 120;

			GUI.Box(new Rect(8, yOffset, 475, 310), "");
			GUI.Label(new Rect(10, yOffset, 1000, 40), version);
			GUI.Label(new Rect(10, yOffset += 20, 1000, 40), spacer);

			if (Time.unscaledTime > timer)
			{
				fps = (int) (1f / Time.unscaledDeltaTime);
				timer = Time.unscaledTime + refreshRate;
			}

			GUI.Label(new Rect(10, yOffset += 30, 1000, 40), $"FPS: {fps}");
			GUI.Label(new Rect(10, yOffset += 30, 1000, 40), "Device Info");
			GUI.Label(new Rect(10, yOffset += 20, 1000, 40), spacer);
			GUI.Label(new Rect(10, yOffset += 20, 1000, 40), cpu);
			GUI.Label(new Rect(10, yOffset += 20, 1000, 40), gpu);
			GUI.Label(new Rect(10, yOffset += 20, 1000, 40), ram);
			GUI.Label(new Rect(10, yOffset += 20, 1000, 40), renderingApi);
			GUI.Label(new Rect(10, yOffset += 30, 1000, 40), "Network");
			GUI.Label(new Rect(10, yOffset += 20, 1000, 40), spacer);
			GUI.Label(new Rect(10, yOffset += 20, 1000, 40), ipAddress);
			GUI.Label(new Rect(10, yOffset + 20, 1000, 40), $"Status: {GetNetworkingStatus()}");
		}

		protected override void SingletonAwakened()
		{
		}

		protected override void SingletonStarted()
		{
			version = $"Team-Capture {Application.version}";
			cpu = $"CPU: {SystemInfo.processorType}";
			gpu = $"GPU: {SystemInfo.graphicsDeviceName}";
			ram = $"RAM: {SystemInfo.systemMemorySize / 1000} GB";
			renderingApi = $"Rendering API: {SystemInfo.graphicsDeviceType}";
			ipAddress = $"IP: {NetHelper.LocalIpAddress()}";

			inputReader.DebugMenuToggle += () => DebugMenuOpen = !DebugMenuOpen;
			inputReader.EnableDebugMenuInput();
		}

		protected override void SingletonDestroyed()
		{
		}

		private string GetNetworkingStatus()
		{
			if (NetworkManager.singleton == null)
				return "Networking not active!";

			switch (NetworkManager.singleton.mode)
			{
				case NetworkManagerMode.Offline:
					return "Not Connected";
				case NetworkManagerMode.ServerOnly:
					return "Server active";
				case NetworkManagerMode.ClientOnly:
					return $"Connected ({NetworkManager.singleton.networkAddress})";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Info

		private string version;
		private string cpu;
		private string gpu;
		private string ram;
		private string renderingApi;
		private string ipAddress;

		#endregion
	}
}