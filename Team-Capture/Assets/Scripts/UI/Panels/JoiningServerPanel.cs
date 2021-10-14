using System;
using Team_Capture.Localization;
using TMPro;

namespace Team_Capture.UI.Panels
{
    internal class JoiningServerPanel : PanelBase
    {
        public TMP_Text messageText;
        
        public string joiningServerLocale = "Menu_JoiningServer";
        public string failToJoinLocale = "Menu_JoiningFail";

        private void Start()
        {
            cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        public override void OnEnable()
        {
            base.OnEnable();
            cancelButton.interactable = false;
            messageText.text = GameUILocale.ResolveString(joiningServerLocale);
        }

        public void FailToJoin()
        {
            cancelButton.interactable = true;
            messageText.text = GameUILocale.ResolveString(failToJoinLocale);
        }
    }
}
