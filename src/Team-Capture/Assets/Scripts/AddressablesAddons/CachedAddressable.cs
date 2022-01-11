// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Team_Capture.AddressablesAddons
{
    [Serializable]
    public class CachedAddressable<T> where T : Object
    {
        public AssetReference assetReference;

        private T cachedItem;
        private bool isWaitingForLoad;

        public T Value
        {
            get
            {
                if (isWaitingForLoad)
                    return null;

                if (cachedItem == null)
                {
                    isWaitingForLoad = true;
                    cachedItem = assetReference.LoadAssetAsync<T>().WaitForCompletion();
                    isWaitingForLoad = false;
                }

                return cachedItem;
            }
        }
    }
}