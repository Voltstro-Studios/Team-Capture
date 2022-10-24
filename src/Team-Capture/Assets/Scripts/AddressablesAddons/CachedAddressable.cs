// Team-Capture
// Copyright (C) 2019-2021 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Team_Capture.AddressablesAddons
{
    [Serializable]
    public class CachedAddressable<T> where T : Object
    {
        [SerializeField] private AssetReference assetReference;

        private bool haveSetValue;
        private T cachedItem;

        public T Value
        {
            get
            {
                if (haveSetValue) 
                    return cachedItem;
                
                cachedItem = assetReference.LoadAssetAsync<T>().WaitForCompletion();
                haveSetValue = true;
                return cachedItem;
            }
        }

        public static implicit operator T(CachedAddressable<T> cachedAddressable) => cachedAddressable.Value;

        public override bool Equals(object obj)
        {
            if (obj is CachedAddressable<T> cachedAddressable)
            {
                if (cachedAddressable.assetReference.AssetGUID == assetReference.AssetGUID)
                    return true;
            }

            return false;
        }
    }
}