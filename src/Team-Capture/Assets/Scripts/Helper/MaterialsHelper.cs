// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace Team_Capture.Helper
{
    /// <summary>
    ///     Provides helper for materials
    /// </summary>
    public static class MaterialsHelper
    {
        //Material surface type change code based off from here:
        //https://answers.unity.com/questions/1608815/change-surface-type-with-lwrp.html

        private static readonly int Surface = Shader.PropertyToID("_Surface");
        private static readonly int Blend = Shader.PropertyToID("_Blend");
        private static readonly int AlphaClip = Shader.PropertyToID("_AlphaClip");
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

        /// <summary>
        ///     Changes the <see cref="Material" />'s transparency
        /// </summary>
        /// <param name="material"></param>
        /// <param name="transparent"></param>
        public static void ChangeMaterialTransparency(this Material material, bool transparent)
        {
            if (transparent)
            {
                material.SetFloat(Surface, (float) SurfaceType.Transparent);
                material.SetFloat(Blend, (float) BlendMode.Alpha);
            }
            else
            {
                material.SetFloat(Surface, (float) SurfaceType.Opaque);
            }

            material.SetupMaterialBlend();
        }

        /// <summary>
        ///     Sets up <see cref="Material" />'s blend
        /// </summary>
        /// <param name="material"></param>
        public static void SetupMaterialBlend(this Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            bool alphaClip = material.GetFloat(AlphaClip) == 1;
            if (alphaClip)
                material.EnableKeyword("_ALPHATEST_ON");
            else
                material.DisableKeyword("_ALPHATEST_ON");

            SurfaceType surfaceType = (SurfaceType) material.GetFloat(Surface);
            if (surfaceType == 0)
            {
                material.SetOverrideTag("RenderType", "");
                material.SetInt(SrcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                material.SetInt(DstBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt(ZWrite, 1);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                material.SetShaderPassEnabled("ShadowCaster", true);
            }
            else
            {
                BlendMode blendMode = (BlendMode) material.GetFloat(Blend);
                switch (blendMode)
                {
                    case BlendMode.Alpha:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt(SrcBlend, (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt(DstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt(ZWrite, 0);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int) RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                    case BlendMode.Premultiply:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt(SrcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                        material.SetInt(DstBlend, (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt(ZWrite, 0);
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int) RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                    case BlendMode.Additive:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt(SrcBlend, (int) UnityEngine.Rendering.BlendMode.One);
                        material.SetInt(DstBlend, (int) UnityEngine.Rendering.BlendMode.One);
                        material.SetInt(ZWrite, 0);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int) RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                    case BlendMode.Multiply:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt(SrcBlend, (int) UnityEngine.Rendering.BlendMode.DstColor);
                        material.SetInt(DstBlend, (int) UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt(ZWrite, 0);

                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int) RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                }
            }
        }

        private enum SurfaceType
        {
            Opaque,
            Transparent
        }

        private enum BlendMode
        {
            Alpha,
            Premultiply,
            Additive,
            Multiply
        }
    }
}

// ReSharper restore StringLiteralTypo
// ReSharper restore IdentifierTypo