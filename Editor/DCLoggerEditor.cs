using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DCLogger.Runtime;
using DCLogger.Runtime.Configs;
using UnityEditor;
using UnityEngine;

namespace DCLogger.Editor
{
    public class DCLoggerEditor : EditorWindow
    {
        private DCLoggerConfig loggerConfig;
        private bool hasEnumChanges = false;
        private bool hasNameCollision = false;
        private bool hasInvalidName = false;

        [MenuItem("Window/DCLogger/DC Logger Window %l")]
        public static void ShowWindow()
        {
            GetWindow(typeof(DCLoggerEditor));
        }

        private void OnEnable()
        {
            LoadOrCreateConfig();
            DetectModuleConfigs();
        }

        private void LoadOrCreateConfig()
        {
            loggerConfig = AssetDatabase.LoadAssetAtPath<DCLoggerConfig>("Assets/Resources/DCLoggerConfig.asset");
            if (loggerConfig == null)
            {
                if (!Directory.Exists("Assets/Resources"))
                {
                    Directory.CreateDirectory("Assets/Resources");
                }

                loggerConfig = CreateInstance<DCLoggerConfig>();
                AssetDatabase.CreateAsset(loggerConfig, "Assets/Resources/DCLoggerConfig.asset");
                AssetDatabase.SaveAssets();
            }
        }

        private void DetectModuleConfigs()
        {
            string[] guids = AssetDatabase.FindAssets("t:ModuleConfig", new[] { "Assets", "Packages" });
            loggerConfig.moduleConfigs.Clear();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ModuleConfig moduleConfig = AssetDatabase.LoadAssetAtPath<ModuleConfig>(path);
                if (moduleConfig != null && !loggerConfig.moduleConfigs.Contains(moduleConfig))
                {
                    loggerConfig.moduleConfigs.Add(moduleConfig);
                }
            }

            EditorUtility.SetDirty(loggerConfig);
            AssetDatabase.SaveAssets();
        }

        private void OnGUI()
        {
            if (loggerConfig == null)
            {
                DrawMissingConfigWarning();
                return;
            }

            // Show the config asset field as read-only
            EditorGUILayout.LabelField(
                new GUIContent("Logger Config:", "This is the configuration file for the logger"),
                EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(new GUIContent("Config Asset", "The Logger configuration asset"), loggerConfig,
                typeof(DCLoggerConfig), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginVertical();
            GUILayout.Label(new GUIContent("DC LOGGER"),new GUIStyle()
                {
                    fontSize = 80,
                    alignment = TextAnchor.MiddleCenter,
                    border = new RectOffset(10,10,10,10),
                    fontStyle = FontStyle.Bold,
                }, GUILayout.ExpandWidth(true), GUILayout.MinHeight(50),
                GUILayout.MaxHeight(100));
            EditorGUILayout.EndVertical();

            GUILayout.Space(20);

            // Button to create a new module
            if (GUILayout.Button("Create New Module"))
            {
                CreateNewModule();
            }

            GUILayout.Space(10);

            GUILayout.Label(new GUIContent("Modules", "Manage logging modules"), EditorStyles.boldLabel);

            hasNameCollision = false;
            hasInvalidName = false;
            HashSet<string> channelNames = new HashSet<string>(); // To track unique channel names

            // Display modules with separation
            foreach (var moduleConfig in loggerConfig.moduleConfigs)
            {
                DrawModuleConfigUI(moduleConfig);
            }

            GUILayout.Space(20);

            // Folder selection for class generation
            EditorGUILayout.LabelField(
                new GUIContent("Class File Path:",
                    "The classes will be generated in the same directory as the ModuleConfig files."),
                EditorStyles.boldLabel);

            // Re-evaluate whether the Generate button should be enabled
            bool canGenerate = hasEnumChanges && !hasNameCollision && !hasInvalidName;

            GUI.enabled = canGenerate;

            if (GUILayout.Button(new GUIContent("GENERATE", "Generate the static classes for each module"),new GUILayoutOption[]
                {
                    GUILayout.Height(50)
                }))
            {
                GenerateStaticClassesForAllModules();
            }

            GUI.enabled = true;

            GUILayout.Space(20);

            // Preprocessor section for enabling/disabling logging
            GUILayout.Label("Logging Configuration", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            bool isLoggingEnabled = IsLoggingEnabled();
            GUI.enabled = !isLoggingEnabled;
            if (GUILayout.Button(new GUIContent("Enable Logging",
                    "Enable logging by defining the DC_LOGGING preprocessor symbol")))
            {
                SetLoggingEnabled(true);
            }

            GUI.enabled = isLoggingEnabled;
            if (GUILayout.Button(new GUIContent("Disable Logging",
                    "Disable logging by removing the DC_LOGGING preprocessor symbol")))
            {
                SetLoggingEnabled(false);
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            // Clear all and Select all section at the bottom
            GUILayout.Space(10);
            GUILayout.Label(new GUIContent("Bulk Actions", "Actions that apply to all channels"),
                EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Deselect all", "Disable all channels")))
            {
                Undo.RecordObject(loggerConfig, "Deselect All Channels");
                SetAllChannels(false);
            }

            if (GUILayout.Button(new GUIContent("Select all", "Enable all channels")))
            {
                Undo.RecordObject(loggerConfig, "Select All Channels");
                SetAllChannels(true);
            }

            EditorGUILayout.EndHorizontal();

            // Save the config if changes were made
            if (GUI.changed)
            {
                EditorUtility.SetDirty(loggerConfig);
                AssetDatabase.SaveAssets();
            }
        }

        private bool IsModuleReadOnly(ModuleConfig moduleConfig)
        {
            string modulePath = AssetDatabase.GetAssetPath(moduleConfig);
            string moduleFullPath = Path.GetFullPath(modulePath);
            return moduleFullPath.Contains("PackageCache");
        }

        private void DrawModuleConfigUI(ModuleConfig moduleConfig)
        {
            bool isReadOnly = IsModuleReadOnly(moduleConfig);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Module: {moduleConfig.ModuleName}", EditorStyles.boldLabel);

            if (isReadOnly)
            {
                EditorGUILayout.HelpBox("This module is read-only because it is located in the PackageCache folder.",
                    MessageType.Info);
            }

            EditorGUI.BeginDisabledGroup(isReadOnly);

            foreach (var channel in moduleConfig.Channels)
            {
                DrawChannelUI(channel, moduleConfig, isReadOnly);
            }

            if (!isReadOnly && GUILayout.Button("Add Channel"))
            {
                Undo.RecordObject(moduleConfig, "Add Channel");

                moduleConfig.AddChannel("New Channel", Color.white); // Use the new method to add a channel
                hasEnumChanges = true; // Set this flag since a new channel has been added
                EditorUtility.SetDirty(moduleConfig);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void DrawChannelUI(Channel channel, ModuleConfig moduleConfig, bool isReadOnly)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(isReadOnly);

            channel.Enabled = EditorGUILayout.Toggle(channel.Enabled, GUILayout.Width(20));

            if (channel.IsEditing && !isReadOnly)
            {
                string oldName = channel.Name;
                channel.Name = EditorGUILayout.TextField(channel.Name);
                channel.ChannelColor = EditorGUILayout.ColorField(channel.ChannelColor, GUILayout.Width(50));

                if (oldName != channel.Name)
                {
                    hasEnumChanges = true; // Set this flag since the name has changed
                }

                if (GUILayout.Button("Save", GUILayout.Width(50)))
                {
                    channel.IsEditing = false;
                    channel.OriginalName = channel.Name;
                }
            }
            else
            {
                EditorGUILayout.LabelField(channel.Name, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(50, 18), channel.ChannelColor);

                if (!isReadOnly && GUILayout.Button("Edit", GUILayout.Width(50)))
                {
                    channel.IsEditing = true;
                }
            }

            if (!isReadOnly && GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                moduleConfig.Channels.Remove(channel);
                hasEnumChanges = true; // Set this flag since a channel has been removed
                return; // Exit loop since we're modifying the collection
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private string GenerateValidConstFieldName(string name)
        {
            // Replace spaces and invalid characters with underscores
            return Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
        }

        private void GenerateStaticClassForModule(ModuleConfig moduleConfig)
        {
            if (IsModuleReadOnly(moduleConfig))
            {
                Debug.LogWarning($"Skipping static class generation for read-only module: {moduleConfig.ModuleName}");
                return;
            }

            string moduleDirectoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(moduleConfig));
            if (string.IsNullOrEmpty(moduleDirectoryPath))
            {
                Debug.LogError($"Could not determine the path for the ModuleConfig: {moduleConfig.ModuleName}");
                return;
            }

            string classFullPath = Path.Combine(moduleDirectoryPath, $"{moduleConfig.ModuleName}Channels.cs");

            using (StreamWriter file = new StreamWriter(classFullPath))
            {
                file.WriteLine($"public static class {moduleConfig.ModuleName}Channels");
                file.WriteLine("{");

                foreach (var channel in moduleConfig.Channels)
                {
                    string constFieldName = GenerateValidConstFieldName(channel.Name);
                    file.WriteLine(
                        $"    public const string {constFieldName} = \"{channel.Name}\";");
                }

                file.WriteLine("}");
            }

            AssetDatabase.Refresh();
        }


        private void GenerateStaticClassesForAllModules()
        {
            foreach (var moduleConfig in loggerConfig.moduleConfigs)
            {
                GenerateStaticClassForModule(moduleConfig);
            }

            hasEnumChanges = false;
        }

        private void SetAllChannels(bool enabled)
        {
            foreach (var moduleConfig in loggerConfig.moduleConfigs)
            {
                foreach (var channel in moduleConfig.Channels)
                {
                    channel.Enabled = enabled;
                }
            }
        }

        private bool IsLoggingEnabled()
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)
                .Contains("DC_LOGGING");
        }

        private void SetLoggingEnabled(bool enabled)
        {
            string defines =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (enabled)
            {
                if (!defines.Contains("DC_LOGGING"))
                {
                    defines += ";DC_LOGGING";
                }
            }
            else
            {
                defines = defines.Replace("DC_LOGGING", "");
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
        }

        private void DrawMissingConfigWarning()
        {
            GUILayout.FlexibleSpace(); // Push content to the center
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Center content horizontally

            // Warning icon and message
            GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
            GUILayout.Label(warningIcon, GUILayout.Width(40), GUILayout.Height(40));

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Unable to find the configuration file for logger", EditorStyles.boldLabel);
            GUILayout.Label("Please reload to create a new configuration file.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace(); // Center content horizontally
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace(); // Push content to the center

            // Reload button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reload", GUILayout.Width(100), GUILayout.Height(30)))
            {
                LoadOrCreateConfig();
                DetectModuleConfigs(); // Ensure we detect module configs again
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void CreateNewModule()
        {
            string path = EditorUtility.SaveFilePanel("Create New Module", Application.dataPath, "NewModuleConfig",
                "asset");
            if (!string.IsNullOrEmpty(path))
            {
                ModuleConfig newModule = CreateInstance<ModuleConfig>();
                newModule.ModuleName = Path.GetFileNameWithoutExtension(path);
                AssetDatabase.CreateAsset(newModule, path);
                AssetDatabase.SaveAssets();

                // Add the new module to the logger config
                loggerConfig.moduleConfigs.Add(newModule);
                EditorUtility.SetDirty(loggerConfig);
                AssetDatabase.SaveAssets();
            }
        }
    }
}