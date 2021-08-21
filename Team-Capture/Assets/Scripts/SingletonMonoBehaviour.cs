// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using JetBrains.Annotations;
using UnityEngine;
using Logger = Team_Capture.Logging.Logger;

//From https://github.com/EternalClickbait/LibEternal/blob/Dev/LibEternal.Unity/SingletonMonoBehaviour.cs

// ReSharper disable once CommentTypo
namespace Team_Capture
{
	/// <summary>
	///     This is a generic Singleton implementation for <see cref="MonoBehaviour" />s.
	///     Create a derived class where the type <typeparamref name="T" /> is the script you want to "Singletonize"
	///     Upon loading it will call <see cref="UnityEngine.Object.DontDestroyOnLoad" /> on the <see cref="GameObject" />
	///     where this
	///     script is contained
	///     so it persists upon <see cref="UnityEngine.SceneManagement.Scene" /> changes.
	/// </summary>
	/// <remarks>
	///     DO NOT REDEFINE <see cref="Awake" />, <see cref="Start" /> or <see cref="OnDestroy" /> in derived classes. EVER.
	///     Instead, use protected abstract methods:
	///     <see cref="SingletonAwakened" />
	///     <see cref="SingletonStarted" />
	///     <see cref="SingletonDestroyed" />
	///     to perform the initialization/cleanup: those methods are guaranteed to only be called once in the
	///     entire lifetime of the <see cref="MonoBehaviour" />
	/// </remarks>
	[PublicAPI]
	[DisallowMultipleComponent]
	public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
	{
		#region Singleton Implementation

		/// <summary>
		///     Backing field that holds the unique instance for this class
		/// </summary>
		private static T instance;

		#endregion

		/// <summary>
		///     Global access point to the unique instance of this class.
		/// </summary>
		public static T Instance => instance;

		// ReSharper disable StaticMemberInGenericType
		/// <summary>
		///     <c>true</c> if this Singleton <see cref="Awake" /> method has already been called by Unity; otherwise, <c>false</c>
		///     .
		/// </summary>
		public static bool IsAwakened { get; private set; }

		/// <summary>
		///     <c>true</c> if this Singleton <see cref="Start" /> method has already been called by Unity; otherwise, <c>false</c>
		///     .
		/// </summary>
		public static bool IsStarted { get; private set; }

		/// <summary>
		///     <c>true</c> if this Singleton <see cref="OnDestroy" /> method has already been called by Unity; otherwise,
		///     <c>false</c>.
		/// </summary>
		public static bool IsDestroyed { get; private set; }
		// ReSharper restore StaticMemberInGenericType

		#region Singleton Life-time Management

		/// <summary>
		///		Override and set to true to ensure that this object will still be destroyed on load
		/// </summary>
		protected virtual bool DoDestroyOnLoad { get; }

		/// <summary>
		///     Unity3D Awake method.
		/// </summary>
		/// <remarks>
		///     This method will only be called once even if multiple instances of the
		///     singleton <see cref="MonoBehaviour" /> exist in the scene.
		///     You can override this method in derived classes to customize the initialization of your
		///     <see cref="MonoBehaviour" />
		/// </remarks>
		protected virtual void SingletonAwakened()
		{
		}

		/// <summary>
		///     Unity3D Start method.
		/// </summary>
		/// <remarks>
		///     This method will only be called once even if multiple instances of the
		///     singleton <see cref="MonoBehaviour" /> exist in the scene.
		///     You can override this method in derived classes to customize the initialization of your
		///     <see cref="MonoBehaviour" />
		/// </remarks>
		protected virtual void SingletonStarted()
		{
		}

		/// <summary>
		///     Unity3D OnDestroy method.
		/// </summary>
		/// <remarks>
		///     This method will only be called once even if multiple instances of the
		///     singleton <see cref="MonoBehaviour" /> exist in the scene.
		///     You can override this method in derived classes to customize the initialization of your
		///     <see cref="MonoBehaviour" />
		/// </remarks>
		protected virtual void SingletonDestroyed()
		{
		}

		/// <summary>
		///     If a duplicated instance of a Singleton <see cref="MonoBehaviour" /> is loaded into the scene
		///     this method will be called instead of <see cref="SingletonAwakened" />(). That way you can customize
		///     what to do with repeated instances.
		/// </summary>
		/// <remarks>
		///     The default approach is delete the duplicated <see cref="MonoBehaviour" />
		/// </remarks>
		protected virtual void NotifyInstanceRepeated()
		{
			Destroy(GetComponent<T>());
		}

		#endregion

		#region Unity3d Messages - DO NOT OVERRRIDE / IMPLEMENT THESE METHODS in child classes!

		private void Awake()
		{
			T thisInstance = GetComponent<T>();

			//Initialize the singleton if the script is already in the scene in a GameObject
			if (instance == null)
			{
				instance = thisInstance;
				if(!DoDestroyOnLoad)
					DontDestroyOnLoad(instance.gameObject);
			}

			else if (thisInstance != instance)
			{
				NotifyInstanceRepeated();

				return;
			}

			if (IsAwakened) return;

			SingletonAwakened();
			IsAwakened = true;
			IsDestroyed = false;
		}

		private void Start()
		{
			//Do not start it twice
			if (IsStarted) return;

			SingletonStarted();
			IsStarted = true;
		}

		private void OnDestroy()
		{
			//Here we are dealing with a duplicate so we don't need to shut the singleton down
			if (this != instance) return;

			//Flag set when Unity sends the message OnDestroy to this Component.
			//This is needed because there is a chance that the GO holding this singleton
			//is destroyed before some other object that also access this singleton when is being destroyed.
			//As the singleton instance is null, that would create both a new instance of this
			//MonoBehaviourSingleton and a brand new GO to which the singleton instance is attached to..
			//
			//However as this is happening during the Unity app shutdown for some reason the newly created GO
			//is kept in the scene instead of being discarded after the game exists play mode.
			//(Unity bug?)
			IsDestroyed = true;
			IsAwakened = false;
			IsStarted = false;

			SingletonDestroyed();
		}

		#endregion
	}
}