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
            IUser user = User.GetActiveUser();
            rawImage.texture = user.UserProfilePicture;   
        }
    }
}
