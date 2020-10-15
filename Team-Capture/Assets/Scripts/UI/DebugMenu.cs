using System;
using BootManagement;
using Console;
using Helper;
using Mirror;
using Player.Movement;
using UnityEngine;
using Voltstro.CommandLineParser;

namespace UI
{
	/// <summary>
	/// A UI used for debugging purposes
	/// </summary>
	public class DebugMenu : SingletonMonoBehaviour<DebugMenu>, IStartOnBoot
	{
		/// <summary>
		/// Is the debug menu open?
		/// </summary>
		[CommandLineArgument("debugmenu")]
		[ConVar("cl_debugmenu", "Shows the debug menu")]
		public static bool DebugMenuOpen;

		/// <summary>
		/// The key used to open and close the debug menu
		/// </summary>
		public KeyCode openDebugMenuKey = KeyCode.F3;

		/// <summary>
		/// How often to refresh the fps counter
		/// </summary>
		public float refreshRate = 1f;

		private float timer;
		private float fps;

		#region Info

		private string version;
		private string cpu;
		private string gpu;
		private string ram;
		private string renderingApi;
		private string ipAddress;
		
		#endregion

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
		}

		protected override void SingletonDestroyed()
		{
			
		}

		public void Init()
		{
		}

		private void OnGUI()
		{
			if(!DebugMenuOpen)
				return;

			const string spacer = "===================";

			GUI.skin.label.fontSize = 20;

			float yOffset = 10;

			if (NetworkManager.singleton != null && NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly)
			{
				if (PlayerMovementManager.ShowPos)
					yOffset = 120;
			}

			GUI.Box(new Rect(8, yOffset, 475, 310), "");

			GUI.Label(new Rect(10, yOffset, 1000, 40), version);
			yOffset += 20;

			GUI.Label(new Rect(10, yOffset, 1000, 40), spacer);

			if (Time.unscaledTime > timer)
			{
				fps = (int) (1f / Time.unscaledDeltaTime);
				timer = Time.unscaledTime + refreshRate;
			}

			yOffset += 30;
			GUI.Label(new Rect(10, yOffset, 1000, 40), $"FPS: {fps}");

			yOffset += 30;
			GUI.Label(new Rect(10, yOffset, 1000, 40), "Device Info");

			yOffset += 20;
			GUI.Label(new Rect(10, yOffset, 1000, 40), spacer);

			yOffset += 20;
			GUI.Label(new Rect(10, yOffset, 1000, 40), cpu);

			yOffset += 20;
			GUI.Label(new Rect(10, yOffset, 1000, 40), gpu);

			yOffset += 20;
			GUI.Label(new Rect(10, yOffset, 1000, 40), ram);

			yOffset += 20;
			GUI.Label(new Rect(10, yOffset, 1000, 40), renderingApi);

			yOffset += 30;
			GUI.Label(new Rect(10, yOffset, 1000, 40), "Network");

			yOffset += 20;
			GUI.Label(new Rect(10, yOffset, 1000, 40), spacer);

			yOffset += 20;
			GUI.Label(new Rect(10, yOffset, 1000, 40), ipAddress);

			yOffset += 20;
			GUI.Label(new Rect(10, yOffset, 1000, 40), $"Status: {GetNetworkingStatus()}");
		}

		private void Update()
		{
			if (Input.GetKeyDown(openDebugMenuKey))
				DebugMenuOpen = !DebugMenuOpen;
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
	}
}