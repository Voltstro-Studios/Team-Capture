using UnityEngine;

namespace UI.Elements.Settings
{
	public class SettingsMenuPanel : MonoBehaviour
	{
		[SerializeField] private Transform scrollingArea;

		public Transform GetScrollingArea => scrollingArea;
	}
}
