using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-2000)]
public class EasterEggs : MonoBehaviour
{
	private static readonly Dictionary<KeyCode, Action> Actions = new Dictionary<KeyCode, Action>()
	{
		[KeyCode.A] = DeleteTest
	};

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	public void Update()
	{
		if (Event.current != null)
			Debug.Log(JsonConvert.SerializeObject(Event.current));
		 
		//Intercept
		if (Event.current != null &&
		    Event.current.isKey &&
		    Event.current.type.Equals(EventType.KeyDown))
		{
			if (Actions.TryGetValue(Event.current.keyCode, out Action action))
			{
				action();
				GUIUtility.hotControl = 0;
				Event.current.Use();
			}
		}
	}

	private static void DeleteTest()
	{
		Debug.Log("Test!");
	}
}