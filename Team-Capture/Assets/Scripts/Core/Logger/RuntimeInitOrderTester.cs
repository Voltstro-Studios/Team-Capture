//﻿#region
//
//using System;
//using UnityEngine;
//using static Logger.Logger;
// using static Logger.LogVerbosity;
//
//#endregion
//
//namespace Logger
//{
//	public static class RuntimeInitOrderTester
//	{
//		/*
//		 * The order is
//		 * - Static constructor (When combined with Subsystems on a method, alone probably won't get called until later (if at all))
//		 * - Subsystem registration
//		 * - After assemblies loaded
//		 * - Before splash screen
//		 * - Before scene load
//		 *  - RuntimeInitialiseOnLoad with no args
//		 *  - After scene load
//		 */
//		static RuntimeInitOrderTester()
//		{
//			Log($"Static constructor {DateTime.Now.Ticks:n0} ticks", INFO);
//		}
//
//		[RuntimeInitializeOnLoadMethod]
//		private static void A()
//		{
//			Log($"RuntimeInitializeOnLoadMethod {DateTime.Now.Ticks:n0} ticks", INFO);
//		}
//
//		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
//		private static void RunsSecond()
//		{
//			Log($"SubsystemRegistration {DateTime.Now.Ticks:n0} ticks",INFO);
//		}
//
//		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
//		private static void Aaa()
//		{
//			Log($"AfterAssembliesLoaded {DateTime.Now.Ticks:n0} ticks",INFO);
//		}
//
//		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
//		private static void Aaaa()
//		{
//			Log($"AfterSceneLoad {DateTime.Now.Ticks:n0} ticks", INFO);
//		}
//
//		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//		private static void Aaaaa()
//		{
//			Log($"BeforeSceneLoad {DateTime.Now.Ticks:n0} ticks", INFO);
//		}
//
//		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
//		private static void Aaaaaa()
//		{
//			Log($"BeforeSplashScreen {DateTime.Now.Ticks:n0} ticks", INFO);
//		}
//	}
//}
//

