using Team_Capture.Localization;
using TMPro;

namespace Team_Capture.UI.Panels
{
	internal class CreatingServerPanel : PanelBase
	{
		public string startingMessageLocale = "Menu_StartingServer";
		public string failedToStartMessageLocale = "Menu_StartingServerFail";

		public TMP_Text messageText;

		private void Start()
		{
			cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
		}

		public override void OnEnable()
		{
			base.OnEnable();
			cancelButton.interactable = false;
			messageText.text = GameUILocale.ResolveString(startingMessageLocale);
		}

		public void FailedToStartMessage()
		{
			cancelButton.interactable = true;
			messageText.text = GameUILocale.ResolveString(failedToStartMessageLocale);
		}
	}
}