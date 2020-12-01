using System;
using JetBrains.Annotations;
using UnityEngine;
using Logger = Core.Logging.Logger;

//From https://github.com/EternalClickbait/LibEternal/blob/Dev/LibEternal.Unity/SingletonMonoBehaviour.cs

// ReSharper disable once CommentTypo
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
	public static T Instance
	{
		get
		{
			if (instance) return instance;

			if (IsDestroyed)
			{
				Logger.Warn("Attempt to access destroyed {@Type} singleton", typeof(T));
				return null;
			}

			Logger.Info("Singleton Instance of {@Type} not set", typeof(T));

			//Finding an existing instance
			{
				T[] instances = FindObjectsOfType<T>();

				// ReSharper disable once ConvertIfStatementToSwitchStatement
				if (instances.Length == 0)
				{
					Logger.Warn(
						"Singleton of type {@SingletonName} not found. Ensure that it is placed on a valid GameObject",
						typeof(T).Name);

					//Create a new instance
					GameObject go = new GameObject(typeof(T).Name)
					{
						transform = {position = Vector3.zero, rotation = Quaternion.identity, parent = null}
					};
					return instance = go.AddComponent<T>();
				}

				//Only found one, all's fine
				if (instances.Length == 1)
					return instance = instances[0];

				if (instances.Length > 1)
				{
					Logger.Warn(
						"More than one singleton of type {@SingletonType} was found ({@InstancesCount}): {@AllInstances}",
						typeof(T), instances.Length, instances);

					//All but the first instance are duplicates
					for (int i = 1; i < instances.Length; i++) instances[i].NotifyInstanceRepeated();
					return instance = instances[0];
				}
			}

			//Should never get here, but all code paths need to either return or throw
			const string errorMessage = "Instance count checks";
			Logger.Error(errorMessage);
			throw new Exception(errorMessage);
		}
	}

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
	///     Unity3D Awake method.
	/// </summary>
	/// <remarks>
	///     This method will only be called once even if multiple instances of the
	///     singleton <see cref="MonoBehaviour" /> exist in the scene.
	///     You can override this method in derived classes to customize the initialization of your
	///     <see cref="MonoBehaviour" />
	/// </remarks>
	protected abstract void SingletonAwakened();

	/// <summary>
	///     Unity3D Start method.
	/// </summary>
	/// <remarks>
	///     This method will only be called once even if multiple instances of the
	///     singleton <see cref="MonoBehaviour" /> exist in the scene.
	///     You can override this method in derived classes to customize the initialization of your
	///     <see cref="MonoBehaviour" />
	/// </remarks>
	protected abstract void SingletonStarted();

	/// <summary>
	///     Unity3D OnDestroy method.
	/// </summary>
	/// <remarks>
	///     This method will only be called once even if multiple instances of the
	///     singleton <see cref="MonoBehaviour" /> exist in the scene.
	///     You can override this method in derived classes to customize the initialization of your
	///     <see cref="MonoBehaviour" />
	/// </remarks>
	protected abstract void SingletonDestroyed();

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
			DontDestroyOnLoad(instance.gameObject);
		}

		else if (thisInstance != instance)
		{
			Logger.Warn("Duplicate {Type} Singleton created", typeof(T));
			NotifyInstanceRepeated();

			return;
		}

		if (IsAwakened) return;

		SingletonAwakened();
		IsAwakened = true;
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

		SingletonDestroyed();
	}

	#endregion
}