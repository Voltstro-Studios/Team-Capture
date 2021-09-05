// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using Steamworks.Data;
using UnityEngine;
using Color = Steamworks.Data.Color;

namespace Team_Capture.Integrations.Steamworks
{
    /// <summary>
    ///     Utils for Steamworks
    /// </summary>
    public static class SteamUtils
    {
        /// <summary>
        ///     Loads data from a <see cref="Image"/> into a <see cref="Texture2D"/>
        /// </summary>
        /// <param name="texture2D"></param>
        /// <param name="image"></param>
        public static void LoadSteamworksImageIntoTexture2D(this Texture2D texture2D, Image image)
        {
            for (int x = 0; x < image.Width; x++)
            for (int y = 0; y < image.Height; y++)
            {
                Color p = image.GetPixel(x, y);
                texture2D.SetPixel(x, (int)image.Height - y, 
                    new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }

            texture2D.Apply();
        }
    }
}