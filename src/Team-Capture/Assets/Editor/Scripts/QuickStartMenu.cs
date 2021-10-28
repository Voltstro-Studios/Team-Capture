using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Team_Capture.SceneManagement;
using Team_Capture.UserManagement;
using UnityEditor;
using UnityEngine;
using UnityVoltBuilder.Build;
using UnityVoltBuilder.GUI;
using Debug = UnityEngine.Debug;

namespace Team_Capture.Editor
{
    [InitializeOnLoad]
    public static class QuickStartMenu
    {
        private const string QuickStartDataKey = "TC-QuickStartData";
        private static readonly QuickstartData quickstartData;

        private static readonly List<Process> StartedProcesses = new List<Process>();

        static QuickStartMenu()
        {
            AddMenu();
            string json = EditorPrefs.GetString(QuickStartDataKey, null);
            quickstartData = string.IsNullOrEmpty(json) ? new QuickstartData() : JsonUtility.FromJson<QuickstartData>(json);
        }

        private static void AddMenu()
        {
            BuildToolWindow.AddOnDraw(OnDraw);
        }

        private static void OnDraw()
        {
            EditorGUILayout.BeginVertical(GUIStyles.DropdownContentStyle);
            Color defaultGUIBackgroundColor = GUI.backgroundColor;
            
            GUILayout.Label("Quick Start", GUIStyles.DropdownHeaderStyle);

            string tcFolder = $"{GameBuilder.GetBuildDirectory()}/Team-Capture-Quick/";
#if UNITY_EDITOR_WIN
            const string appName = "Team-Capture.exe";
#else
            const string appName = "Team-Capture";
#endif
            string tcFullPath = $"{tcFolder}{appName}";
            
            if (!File.Exists(tcFullPath))
            {
                EditorGUILayout.HelpBox("You need to build Team-Capture first before being able to use quick start!", MessageType.Error);
            }
            else
            {
                //Get all TC scenes
                List<TCScene> scenes = TCScenesManager.GetAllOnlineScenes();
                
                 EditorGUILayout.BeginHorizontal();
                 {
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Start"))
                    {
                        //Start each process
                        foreach (QuickStartEntry entry in quickstartData.entries)
                        {
                            //Build arguments
                            string arguments = $"-scene {scenes[quickstartData.selectedSceneIndex].SceneFileName} -auth-method {quickstartData.authMode.ToString()} ";
                            if (entry.server)
                                arguments += "-batchmode -nographics ";
                            arguments += entry.additionalArguments;
                            
                            //Setup and start the process
                            Process newProcess = new Process
                            {
                                StartInfo = new ProcessStartInfo(tcFullPath, arguments)
                            };
                            newProcess.Start();
                            StartedProcesses.Add(newProcess);
                        }

                        Debug.Log(quickstartData.entries.Count > 1
                            ? $"Started {quickstartData.entries.Count} processes..."
                            : "Started 1 process...");
                    }
                    GUI.backgroundColor = defaultGUIBackgroundColor;

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Stop All"))
                    {
                        //Kill each started process
                        foreach (Process process in StartedProcesses)
                        {
                            if(process.HasExited)
                                continue;
                            
                            process.Kill();
                        }
                        StartedProcesses.Clear();
                        
                        Debug.Log("Stopped processes.");
                    }
                    GUI.backgroundColor = defaultGUIBackgroundColor;
                 }
                 EditorGUILayout.EndHorizontal();

                 //Make sure there are online scenes
                 if (scenes.Count == 0)
                     EditorGUILayout.HelpBox("There are no available scenes!", MessageType.Error);
                 else
                 {
                     //Check that all scenes still exist
                     if (quickstartData.selectedSceneIndex > scenes.Count)
                         quickstartData.selectedSceneIndex = 0;

                     //Display the scenes in a popup (aka a dropdown)
                     string[] names = scenes.Select(scene => scene.SceneFileName).ToArray();
                     quickstartData.selectedSceneIndex =
                         EditorGUILayout.Popup("Scene", quickstartData.selectedSceneIndex, names);
                 }

                 quickstartData.authMode = (UserProvider) EditorGUILayout.EnumPopup("Auth-Mode", quickstartData.authMode);

                 GUILayout.Space(10f);
                 GUILayout.Label("Processes", GUIStyles.DropdownHeaderStyle);

                 if (quickstartData.entries.Count == 0)
                     EditorGUILayout.HelpBox("There are no processes!", MessageType.Error);
                 else
                 {
                     for (int i = 0; i < quickstartData.entries.Count; i++)
                     {
                         QuickStartEntry entry = quickstartData.entries[i];
                    
                         GUILayout.BeginVertical();
                         {
                             GUILayout.BeginHorizontal();
                             {
                                 GUILayout.Space(10);
                                 entry.server = EditorGUILayout.Toggle("Server?", entry.server);
                             }
                             GUILayout.EndHorizontal();
                        
                             GUILayout.BeginHorizontal();
                             {
                                 GUILayout.Space(10);
                                 entry.additionalArguments = EditorGUILayout.TextField("Additional Arguments", entry.additionalArguments);
                             }
                             GUILayout.EndHorizontal();
                        
                             GUILayout.BeginHorizontal();
                             {
                                 GUILayout.Space(10);
                                 if (GUILayout.Button("Remove", new GUIStyle(EditorStyles.miniButtonMid)))
                                 {
                                     quickstartData.entries.RemoveAt(i);
                                 }
                             }
                             GUILayout.EndHorizontal();
                        
                             GUILayout.Space(8f);
                         }
                         GUILayout.EndVertical();
                     }
                 }

                 if (GUILayout.Button("Add Process"))
                 {
                     quickstartData.entries.Add(new QuickStartEntry
                     {
                         additionalArguments = string.Empty
                     });
                 }
                 if (EditorGUI.EndChangeCheck())
                 {
                     string json = JsonUtility.ToJson(quickstartData);
                     EditorPrefs.SetString(QuickStartDataKey, json);
                 }
            }

            EditorGUILayout.EndVertical();
        }
        
        [Serializable]
        private class QuickstartData
        {
            public int selectedSceneIndex;
            public UserProvider authMode;
            
            public List<QuickStartEntry> entries = new List<QuickStartEntry>
            {
                new QuickStartEntry
                {
                    server = true,
                    additionalArguments = "-high"
                },
                new QuickStartEntry
                {
                    server = false,
                    additionalArguments = "-novid -high -connect localhost"
                }
            };
        }

        [Serializable]
        private class QuickStartEntry
        {
            public bool server;
            public string additionalArguments;
        }
    }
}
