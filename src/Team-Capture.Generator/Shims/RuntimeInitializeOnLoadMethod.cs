// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

namespace Team_Capture.Generator.Shims
{
    /// <summary>
    ///   <para>Set RuntimeInitializeOnLoadMethod type.</para>
    /// </summary>
    /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=RuntimeInitializeLoadType">`RuntimeInitializeLoadType` on docs.unity3d.com</a></footer>
    public enum RuntimeInitializeLoadType
    {
      /// <summary>
      ///   <para>After Scene is loaded.</para>
      /// </summary>
      /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=RuntimeInitializeLoadType.AfterSceneLoad">`RuntimeInitializeLoadType.AfterSceneLoad` on docs.unity3d.com</a></footer>
      AfterSceneLoad,
      
      /// <summary>
      ///   <para>Before Scene is loaded.</para>
      /// </summary>
      /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=RuntimeInitializeLoadType.BeforeSceneLoad">`RuntimeInitializeLoadType.BeforeSceneLoad` on docs.unity3d.com</a></footer>
      BeforeSceneLoad,
      
      /// <summary>
      ///   <para>Callback when all assemblies are loaded and preloaded assets are initialized.</para>
      /// </summary>
      /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=RuntimeInitializeLoadType.AfterAssembliesLoaded">`RuntimeInitializeLoadType.AfterAssembliesLoaded` on docs.unity3d.com</a></footer>
      AfterAssembliesLoaded,
      
      /// <summary>
      ///   <para>Immediately before the splash screen is shown.</para>
      /// </summary>
      /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=RuntimeInitializeLoadType.BeforeSplashScreen">`RuntimeInitializeLoadType.BeforeSplashScreen` on docs.unity3d.com</a></footer>
      BeforeSplashScreen,
      
      /// <summary>
      ///   <para>Callback used for registration of subsystems</para>
      /// </summary>
      /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=RuntimeInitializeLoadType.SubsystemRegistration">`RuntimeInitializeLoadType.SubsystemRegistration` on docs.unity3d.com</a></footer>
      SubsystemRegistration,
    }
}