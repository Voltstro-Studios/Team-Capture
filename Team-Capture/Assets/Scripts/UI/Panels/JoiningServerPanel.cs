using Team_Capture.AddressablesAddons;
using TMPro;

namespace Team_Capture.UI.Panels
{
    internal class JoiningServerPanel : PanelBase
    {
        public TMP_Text messageText;

        public CachedLocalizedString joiningServerLocale;
        public CachedLocalizedString failToJoinLocale;

        private void Start()
        {
            cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        public override void OnEnable()
        {
            base.OnEnable();
            cancelButton.interactable = false;
            messageText.text = joiningServerLocale.Value;
        }

        public void FailToJoin()
        {
            cancelButton.interactable = true;
            messageText.text = failToJoinLocale.Value;
        }
    }
}