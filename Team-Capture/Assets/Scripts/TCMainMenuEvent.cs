using UnityEngine;

[CreateAssetMenu(fileName = "New TC Main Menu Event", menuName = "Team Capture/TCMainMenuEvent")]
public class TCMainMenuEvent : ScriptableObject
{
	public GameObject panelPrefab;

	public bool darkenScreen;
	public bool showTopBlackBar;

	[HideInInspector] public bool isOpen;
	[HideInInspector] public GameObject activePanel;
}