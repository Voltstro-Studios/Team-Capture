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
    public class CachedFile<T> where T : Object
    {
        private readonly string addressablePath;
        
        private T cachedItem;
        private bool isWaitingForLoad = false;

        public CachedFile(string addressablePath)
        {
            this.addressablePath = addressablePath ?? throw new ArgumentNullException(nameof(addressablePath));
        }

        public T Value
        {
            get
            {
                if (isWaitingForLoad)
                    return null;
                
                if (cachedItem == null)
                {
                    isWaitingForLoad = true;
                    cachedItem = Addressables.LoadAssetAsync<T>(addressablePath).WaitForCompletion();
                    isWaitingForLoad = false;
                }
                    
                return cachedItem;
            }
        }
    }
}