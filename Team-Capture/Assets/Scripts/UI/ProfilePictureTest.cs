using Team_Capture.UserManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Team_Capture.UI
{
    [RequireComponent(typeof(RawImage))]
    public class ProfilePictureTest : MonoBehaviour
    {
        private RawImage rawImage;
        
        private void Start()
        {
            rawImage = GetComponent<RawImage>();
            SetTexture();
        }

        private void OnEnable()
        {
            SetTexture();
        }

        private void SetTexture()
        {
            rawImage.texture = User.GetActiveUser().UserProfilePicture;   
        }
    }
}
