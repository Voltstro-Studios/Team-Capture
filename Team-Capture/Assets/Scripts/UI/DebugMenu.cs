﻿// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Team_Capture.Console;
using Team_Capture.Helper;
using Team_Capture.Input;
using UImGui;
using Unity.Profiling;
using UnityEngine;

namespace Team_Capture.UI
{
	/// <summary>
	///     A UI used for debugging purposes
	/// </summary>
	internal class DebugMenu : SingletonMonoBehaviour<DebugMenu>
	{
		/// <summary>
		///     Reads input
		/// </summary>
		public InputReader inputReader;

		/// <summary>
		///		Icon texture
		/// </summary>
		public Texture iconTexture;

		/// <summary>
		///		Icon size
		/// </summary>
		public Vector2 iconSize = new Vector2(48, 48);

		/// <summary>
		///		How often to refresh the fps counter
		/// </summary>
		public float refreshRate = 1f;

		/// <summary>
		///     Is the debug menu open?
		/// </summary>
		[ConVar("cl_debugmenu", "Shows the debug menu", true)]
		public static bool DebugMenuOpen;

		private const string Spacer = "===================";

		private ProfilerRecorder mainThreadRecorder;
		private ProfilerRecorder totalMemoryUsedRecorder;
		private ProfilerRecorder gcReservedMemoryRecorder;
		private ProfilerRecorder totalDrawCallsRecorder;

		private float timer;

		private double frameTime;
		private int fps;
		private int totalMemoryUsed;
		private int gcReserved;
		private int drawCalls;
		
		private int inMessageCountFrame;
		private int outMessageCountFrame;
		private int inMessageBytesFrame;
		private int outMessageBytesFrame;
		
		private int inMessageCount;
		private int outMessageCount;
		private int inMessageBytes;
		private int outMessageBytes;

		private void Update()
		{
			if (!(Time.unscaledTime > timer)) return;

			frameTime = GetRecorderFrameTimeAverage(mainThreadRecorder) * 1e-6f;
			fps = (int) (1f / Time.unscaledDeltaTime);

			totalMemoryUsed = (int) totalMemoryUsedRecorder.LastValue / (1024 * 1024);
			gcReserved = (int) gcReservedMemoryRecorder.LastValue / (1024 * 1024);
			drawCalls = (int) totalDrawCallsRecorder.LastValue;

			inMessageCountFrame = inMessageCount;
			outMessageCountFrame = outMessageCount;
			inMessageBytesFrame = inMessageBytes;
			outMessageBytesFrame = outMessageBytes;

			inMessageCount = 0;
			inMessageBytes = 0;
			outMessageCount = 0;
			outMessageBytes = 0;
			
			timer = Time.unscaledTime + refreshRate;
		}

		private void OnEnable()
		{
			timer = Time.unscaledTime;

			mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
			totalMemoryUsedRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
			gcReservedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
			totalDrawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");

			NetworkDiagnostics.InMessageEvent += AddInMessage;
			NetworkDiagnostics.OutMessageEvent += AddOutMessage;
			UImGuiUtility.Layout += OnLayout;
		}

		private void OnLayout(UImGui.UImGui obj)
		{
			if(!DebugMenuOpen)
				return;
			
			ImGuiNET.ImGui.Begin("Stats");
			{
				ImGuiNET.ImGui.BeginGroup();
				{
					ImGuiNET.ImGui.Image(UImGuiUtility.GetTextureId(iconTexture), iconSize);
					ImGuiNET.ImGui.SameLine();
					ImGuiNET.ImGui.Text(version);
				}
				ImGuiNET.ImGui.EndGroup();
				
				ImGuiNET.ImGui.Spacing();
				ImGuiNET.ImGui.Text($"Frame Time: {frameTime:F1}ms");
				ImGuiNET.ImGui.Text($"FPS: {fps}");
				ImGuiNET.ImGui.Text($"Total Memory: {totalMemoryUsed} MB");
				ImGuiNET.ImGui.Text($"GC Reserved: {gcReserved} MB");
				ImGuiNET.ImGui.Text($"Draw Calls: {drawCalls}");
				
				ImGuiNET.ImGui.Spacing();
				ImGuiNET.ImGui.Text("Device Info");
				ImGuiNET.ImGui.Text(cpu);
				ImGuiNET.ImGui.Text(gpu);
				ImGuiNET.ImGui.Text(ram);
				ImGuiNET.ImGui.Text(renderingApi);
				
				ImGuiNET.ImGui.Spacing();
				ImGuiNET.ImGui.Text("Network");
				ImGuiNET.ImGui.Text(ipAddress);
				ImGuiNET.ImGui.Text($"Status: {GetNetworkingStatus()}");
				ImGuiNET.ImGui.Text($"In Messages {inMessageCountFrame} ({inMessageBytesFrame / 1000} kb)");
				ImGuiNET.ImGui.Text($"Out Message {outMessageCountFrame} ({outMessageBytesFrame / 1000} kb)");
			}
			ImGuiNET.ImGui.End();
		}

		private void OnDisable()
		{
			mainThreadRecorder.Dispose();
			totalMemoryUsedRecorder.Dispose();
			gcReservedMemoryRecorder.Dispose();
			totalDrawCallsRecorder.Dispose();
			
			NetworkDiagnostics.InMessageEvent -= AddInMessage;
			NetworkDiagnostics.OutMessageEvent -= AddOutMessage;
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

		private double GetRecorderFrameTimeAverage(ProfilerRecorder recorder)
		{
			int samplesCount = recorder.Capacity;
			if (samplesCount == 0)
				return 0;

			List<ProfilerRecorderSample> samples = new List<ProfilerRecorderSample>(samplesCount);
			recorder.CopyTo(samples);
			double r = samples.Aggregate<ProfilerRecorderSample, double>(0, (current, sample) => current + sample.Value);
			r /= samplesCount;

			return r;
		}

		private void AddInMessage(NetworkDiagnostics.MessageInfo info)
		{
			inMessageCount++;
			inMessageBytes += info.bytes;
		}

		private void AddOutMessage(NetworkDiagnostics.MessageInfo info)
		{
			outMessageCount++;
			outMessageBytes += info.bytes;
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