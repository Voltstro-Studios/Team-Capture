using UnityEngine;

/// <summary>
/// A panel for a main menu
/// </summary>
[CreateAssetMenu(fileName = "New TC Main Menu Event", menuName = "Team Capture/MainMenuPanel")]
public class MainMenuPanel : ScriptableObject
{
	/// <summary>
	/// The base prefab of the panel
	/// </summary>
	[Tooltip("The base prefab of the panel")]
	public GameObject panelPrefab;

	/// <summary>
	/// Whether or not to darken the background
	/// </summary>
	[Tooltip("Whether or not to darken the background")]
	public bool darkenScreen;

	/// <summary>
	/// Whether or not to show a black bar on top
	/// </summary>
	[Tooltip("Whether or not to show a black bar on top")]
	public bool showTopBlackBar;

	/// <summary>
	/// Is this panel currently open
	/// </summary>
	[HideInInspector] public bool isOpen;

	/// <summary>
	/// The active in-game panel
	/// </summary>
	[HideInInspector] public GameObject activePanel;
}