// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Team_Capture.Collections
{
    //From:
    //https://github.com/upscalebaby/generic-serializable-dictionary/blob/70133ea96240b5626711dc821039a84f05ca85e6/Assets/Scripts/GenericDictionary.cs
    
    /// <summary>
    /// Generic Serializable Dictionary for Unity 2020.1 and above.
    /// Simply declare your key/value types and you're good to go - zero boilerplate.
    /// </summary>
    [Serializable]
    public class GenericDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        // Internal
        [SerializeField]
        private List<KeyValuePair> list = new();
        
        private Dictionary<TKey, int> indexByKey = new();
        private Dictionary<TKey, TValue> dict = new();

#pragma warning disable 0414
        [SerializeField, HideInInspector]
        private bool keyCollision;
#pragma warning restore 0414

        [Serializable]
        private struct KeyValuePair
        {
            public TKey key;
            public TValue value;
            
            public KeyValuePair(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }

        //Lists are serialized natively by Unity, no custom implementation needed.
        public void OnBeforeSerialize() { }

        //Populate dictionary with pairs from list and flag key-collisions.
        public void OnAfterDeserialize()
        {
            dict.Clear();
            indexByKey.Clear();
            keyCollision = false;
            
            for (int i = 0; i < list.Count; i++)
            {
                TKey key = list[i].key;
                if (key != null && !ContainsKey(key))
                {
                    dict.Add(key, list[i].value);
                    indexByKey.Add(key, i);
                }
                else
                {
                    keyCollision = true;
                }
            }
        }

        //IDictionary
        public TValue this[TKey key]
        {
            get => dict[key];
            set
            {
                dict[key] = value;
                if (indexByKey.ContainsKey(key))
                {
                    int index = indexByKey[key];
                    list[index] = new KeyValuePair(key, value);
                }
                else
                {
                    list.Add(new KeyValuePair(key, value));
                    indexByKey.Add(key, list.Count - 1);
                }
            }
        }

        public ICollection<TKey> Keys => dict.Keys;
        public ICollection<TValue> Values => dict.Values;

        public void Add(TKey key, TValue value)
        {
            dict.Add(key, value);
            list.Add(new KeyValuePair(key, value));
            indexByKey.Add(key, list.Count - 1);
        }

        public bool ContainsKey(TKey key) => dict.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (!dict.Remove(key))
                return false;
            
            int index = indexByKey[key];
            list.RemoveAt(index);
            UpdateIndexLookup(index);
            indexByKey.Remove(key);
            return true;

        }

        private void UpdateIndexLookup(int removedIndex) {
            for (int i = removedIndex; i < list.Count; i++)
            {
                TKey key = list[i].key;
                indexByKey[key]--;
            }
        }

        public bool TryGetValue(TKey key, out TValue value) => dict.TryGetValue(key, out value);

        // ICollection
        public int Count => dict.Count;
        public bool IsReadOnly { get; set; }

        public void Add(KeyValuePair<TKey, TValue> pair)
        {
            Add(pair.Key, pair.Value);
        }

        public void Clear()
        {
            dict.Clear();
            list.Clear();
            indexByKey.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            return dict.TryGetValue(pair.Key, out TValue value) && EqualityComparer<TValue>.Default.Equals(value, pair.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The starting array index cannot be negative.");
            if (array.Length - arrayIndex < dict.Count)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            foreach (KeyValuePair<TKey, TValue> pair in dict)
            {
                array[arrayIndex] = pair;
                arrayIndex++;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> pair)
        {
            if (!dict.TryGetValue(pair.Key, out TValue value))
                return false;
            
            bool valueMatch = EqualityComparer<TValue>.Default.Equals(value, pair.Value);
            return valueMatch && Remove(pair.Key);
        }

        // IEnumerable
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();
    }
}
