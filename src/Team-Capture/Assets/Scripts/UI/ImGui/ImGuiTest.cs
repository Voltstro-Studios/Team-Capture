using UImGui;
using UnityEngine;

namespace Team_Capture.UI.ImGui
{
    public class ImGuiTest : MonoBehaviour
    {
        private void OnEnable()
        {
            UImGuiUtility.Layout += OnImGuiLayout;
        }

        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnImGuiLayout;
        }

        private void OnImGuiLayout(UImGui.UImGui obj)
        {
            ImGuiNET.ImGui.ShowDemoWindow();
        }
    }
}