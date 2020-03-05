using UnityEngine;

public class SettingsMenuPanel : MonoBehaviour
{
	[SerializeField] private Transform scrollingArea;

	public Transform GetScrollingArea => scrollingArea;
}
