using System;
using BootManagement;
using Console;
using Helper;
using Mirror;
using UnityEngine;
using Voltstro.CommandLineParser;

namespace UI
{
	public class DebugMenu : SingletonMonoBehaviour<DebugMenu>, IStartOnBoot
	{
		[CommandLineArgument("debugmenu")]
		[ConVar("cl_debugmenu", "Shows the debug menu")]
		public static bool DebugMenuOpen;

		public KeyCode openDebugMenuKey = KeyCode.F3;
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

			GUI.Label(new Rect(10, 10, 1000, 40), version);
			GUI.Label(new Rect(10, 30, 1000, 40), spacer);

			if (Time.unscaledTime > timer)
			{
				fps = (int) (1f / Time.unscaledDeltaTime);
				timer = Time.unscaledTime + refreshRate;
			}

			GUI.Label(new Rect(10, 60, 1000, 40), $"FPS: {fps}");

			GUI.Label(new Rect(10, 100, 1000, 40), "Device Info");
			GUI.Label(new Rect(10, 120, 1000, 40), spacer);
			GUI.Label(new Rect(10, 140, 1000, 40), cpu);
			GUI.Label(new Rect(10, 160, 1000, 40), gpu);
			GUI.Label(new Rect(10, 180, 1000, 40), ram);
			GUI.Label(new Rect(10, 200, 1000, 40), renderingApi);

			GUI.Label(new Rect(10, 230, 1000, 40), "Network");
			GUI.Label(new Rect(10, 250, 1000, 40), spacer);
			GUI.Label(new Rect(10, 270, 1000, 40), ipAddress);
			GUI.Label(new Rect(10, 290, 1000, 40), $"Status: {GetNetworkingStatus()}");
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