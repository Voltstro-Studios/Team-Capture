// Team-Capture
// Copyright (c) 2019-2022 Voltstro-Studios
// 
// This project is governed by the AGPLv3 License.
// For more details see the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Team_Capture.Helper.Extensions;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Team_Capture.Editor.CustomEditors
{
    [CustomEditor(typeof(AddressableAssetGroup)), CanEditMultipleObjects]
    public class AddressableGroupInspector : UnityEditor.Editor
    {
        [CanBeNull] private UnityEditor.Editor defaultEditor;
        private AddressableAssetGroup assetGroup;
        
        private void OnEnable()
        {
            assetGroup = target as AddressableAssetGroup;
            
            //We doing this bullshit as all I want to do is extend the existing AddressableAssetGroup editor
            Type type = Type.GetType("UnityEditor.AddressableAssets.GUI.AddressableAssetGroupInspector, Unity.Addressables.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (type == null)
            {
                Debug.Log("Did not get AddressableAssetGroupInspector!");
                return;
            }
            
            defaultEditor = CreateEditor(targets, type);
            InvokeMethodOnDefault("OnEnable");
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Order"))
            {
                //We about to do some naughty stuff, but have to no thanks to Unity
                FieldInfo fieldInfo = target.GetType().GetField("m_SerializeEntries",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (fieldInfo == null)
                {
                    Debug.LogError("Failed to get m_SerializeEntries field!");
                    return;
                }

                List<AddressableAssetEntry> entries = fieldInfo.GetValue<List<AddressableAssetEntry>>(target);
                entries.Sort((x, y) => string.Compare(x.address, y.address, StringComparison.Ordinal));
                
                fieldInfo.SetValue(target, entries);
                EditorUtility.SetDirty(target);
                Debug.Log("Asset group sorted!");
            }
            
            if(defaultEditor == null)
                return;
            
            defaultEditor.OnInspectorGUI();
        }
        

        private void OnDisable()
        {
            InvokeMethodOnDefault("OnDisable");
            DestroyImmediate(defaultEditor);
        }

        private void InvokeMethodOnDefault(string methodName)
        {
            if(defaultEditor == null)
                return;
            
            MethodInfo method = defaultEditor.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                Debug.Log($"Failed to get method {methodName}!");
                return;
            }

            method.Invoke(defaultEditor, null);
        }
    }
}
