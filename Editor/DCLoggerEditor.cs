using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DCLogger.Runtime;
using UnityEditor;
using UnityEngine;

namespace DCLogger.Editor
{
    public class DCLoggerEditor : EditorWindow
    {
        private const int MaxChannels = 32; // Maximum number of channels for a 32-bit integer enum
        private DCLoggerConfig loggerConfig;
        private bool isEnumGenerated = false;
        private bool hasEnumChanges = false; // Separate flag for enum changes
        private bool hasNameCollision = false; // Flag to track name collisions
        private bool hasInvalidName = false; // Flag to track invalid channel names
        private bool showEnumPreview = false; // Flag to toggle enum preview
        private string enumFilePath = "Assets/Scripts/";
        private string loggerFolderName = "DCLogger";
        private string channelScriptName = "Channel"; // Default script name

        [MenuItem("Window/DCLogger/DC Logger Window %l")]
        public static void ShowWindow()
        {
            GetWindow(typeof(DCLoggerEditor));
        }

        private void OnEnable()
        {
            LoadOrCreateConfig();
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

            GUILayout.Space(10);

            // Script name definition
            EditorGUILayout.LabelField(
                new GUIContent("Channel Script Name:", "Define the script name for the Channel enum"),
                EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            channelScriptName = EditorGUILayout.TextField(channelScriptName);
            if (EditorGUI.EndChangeCheck())
            {
                if (!IsValidClassName(channelScriptName))
                {
                    EditorGUILayout.HelpBox("Invalid script name. The script name must be a valid C# class name.",
                        MessageType.Error);
                }
            }

            GUILayout.Space(10);

            GUILayout.Label(new GUIContent("Click to toggle logging channels", "Toggle the channels on or off"),
                EditorStyles.boldLabel);

            hasNameCollision = false;
            hasInvalidName = false;
            HashSet<string> channelNames = new HashSet<string>(); // To track unique channel names
            List<int> channelsToRemove = new List<int>(); // Collect indices of channels to remove

            // Display channels
            for (int i = 0; i < loggerConfig.channels.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                loggerConfig.channels[i].Enabled = EditorGUILayout.Toggle(
                    new GUIContent("", "Enable or disable this channel"), loggerConfig.channels[i].Enabled,
                    GUILayout.Width(20));

                // Show editable text field if editing, otherwise show a label
                if (loggerConfig.channels[i].IsEditing)
                {
                    EditorGUI.BeginChangeCheck();

                    // Remove the "Name" label and make the TextField expand to full width
                    string newName =
                        EditorGUILayout.TextField(loggerConfig.channels[i].Name, GUILayout.ExpandWidth(true));

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(loggerConfig, "Edit Channel Name");
                        if (newName != loggerConfig.channels[i].OriginalName)
                        {
                            hasEnumChanges = true;
                        }

                        loggerConfig.channels[i].Name = newName;
                    }

                    // Allow color selection when editing
                    loggerConfig.channels[i].ChannelColor = EditorGUILayout.ColorField(
                        new GUIContent("", "The color associated with this channel"),
                        loggerConfig.channels[i].ChannelColor, GUILayout.Width(50));

                    // Allow log type selection when editing
                    loggerConfig.channels[i].LogType = (DCLoggerConfig.LoggingType) EditorGUILayout.EnumPopup(
                        new GUIContent("", "The type of log this channel represents"), loggerConfig.channels[i].LogType,
                        GUILayout.Width(80));
                }
                else
                {
                    // Display the channel name, color, and log type icon (non-interactive)
                    EditorGUILayout.LabelField(loggerConfig.channels[i].Name, GUILayout.ExpandWidth(true));
                    EditorGUI.DrawRect(GUILayoutUtility.GetRect(50, 18), loggerConfig.channels[i].ChannelColor);

                    // Display the appropriate icon for the logging type
                    GUIContent iconContent = GetLogTypeIcon(loggerConfig.channels[i].LogType);
                    GUILayout.Label(iconContent, GUILayout.Width(20), GUILayout.Height(20));
                }

                if (GUILayout.Button(loggerConfig.channels[i].IsEditing ? "Save" : "Edit", GUILayout.Width(50)))
                {
                    Undo.RecordObject(loggerConfig,
                        loggerConfig.channels[i].IsEditing ? "Save Channel" : "Edit Channel");
                    if (loggerConfig.channels[i].IsEditing)
                    {
                        if (loggerConfig.channels[i].Name != loggerConfig.channels[i].OriginalName)
                        {
                            loggerConfig.channels[i].OriginalName =
                                loggerConfig.channels[i].Name; // Update the original name
                            hasEnumChanges = true;
                        }
                    }

                    loggerConfig.channels[i].IsEditing = !loggerConfig.channels[i].IsEditing;
                }

                if (GUILayout.Button(new GUIContent("Clone", "Clone this channel"), GUILayout.Width(60)))
                {
                    Undo.RecordObject(loggerConfig, "Clone Channel");
                    CloneChannel(i);
                    hasEnumChanges = true;
                }

                if (GUILayout.Button(new GUIContent("Remove", "Remove this channel"), GUILayout.Width(60)))
                {
                    Undo.RecordObject(loggerConfig, "Remove Channel");
                    channelsToRemove.Add(i); // Defer removal until after the loop
                }

                EditorGUILayout.EndHorizontal();

                // Validate the channel name
                if (channelNames.Contains(loggerConfig.channels[i].Name))
                {
                    hasNameCollision = true;
                    EditorGUILayout.HelpBox("Channel name must be unique.", MessageType.Error);
                }
                else
                {
                    channelNames.Add(loggerConfig.channels[i].Name);
                }

                if (!IsValidChannelName(loggerConfig.channels[i].Name))
                {
                    hasInvalidName = true;
                    EditorGUILayout.HelpBox(
                        "Channel name cannot start with a number and should only contain letters, digits, or underscores.",
                        MessageType.Error);
                }
            }

            // Remove channels after the loop to avoid modifying the list during iteration
            foreach (int index in channelsToRemove)
            {
                loggerConfig.channels.RemoveAt(index);
            }

            GUILayout.Space(10);

            // Display message if max channels are reached
            if (loggerConfig.channels.Count >= MaxChannels)
            {
                EditorGUILayout.HelpBox(
                    $"Maximum number of channels ({MaxChannels}) reached. Cannot add more channels.",
                    MessageType.Warning);
            }

            // Add new channel button (disabled if max channels are reached)
            GUI.enabled = loggerConfig.channels.Count < MaxChannels;
            if (GUILayout.Button(new GUIContent("Add Channel", "Add a new logging channel")))
            {
                Undo.RecordObject(loggerConfig, "Add Channel");
                loggerConfig.channels.Add(new DCLoggerConfig.Channel("New Channel", Color.white,
                    DCLoggerConfig.LoggingType.Log));
                hasEnumChanges = true;
            }

            GUI.enabled = true;

            GUILayout.Space(10);

            // Folder selection for enum generation
            EditorGUILayout.LabelField(
                new GUIContent("Enum File Path:", "Select the folder where the enum file will be generated"),
                EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            enumFilePath = EditorGUILayout.TextField(enumFilePath);
            if (GUILayout.Button(new GUIContent("Select Folder", "Choose the folder for the enum file")))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Folder for Enum", "Assets/", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    enumFilePath = "Assets" + selectedPath.Replace(Application.dataPath, "");
                }
            }

            EditorGUILayout.EndHorizontal();

            // Enum preview toggle
            showEnumPreview = EditorGUILayout.Foldout(showEnumPreview,
                new GUIContent("Enum Preview", "Preview the generated enum code"));
            if (showEnumPreview)
            {
                EditorGUILayout.HelpBox(GenerateEnumPreview(), MessageType.None);
            }

            // Generate enum code only if enum changes exist, no name collisions, and no invalid names
            GUI.enabled = hasEnumChanges && !hasNameCollision && !hasInvalidName && IsValidClassName(channelScriptName);
            
            if (GUILayout.Button(new GUIContent("Generate", "Generate the enum based on the current channels")))
            {
                GenerateEnumAndLoggerClasses();
                hasEnumChanges = false;
            }

            GUI.enabled = true;

            GUILayout.Space(20);

            // Preprocessor section for enabling/disabling logging
            GUILayout.Label("Logging Configuration", EditorStyles.boldLabel);
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

            GUI.enabled = true;

            // Clear all and Select all section at the bottom
            GUILayout.Space(10);
            GUILayout.Label(new GUIContent("Bulk Actions", "Actions that apply to all channels"),
                EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Clear all", "Disable all channels")))
            {
                Undo.RecordObject(loggerConfig, "Clear All Channels");
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

        private void CloneChannel(int index)
        {
            DCLoggerConfig.Channel originalChannel = loggerConfig.channels[index];
            string newChannelName = originalChannel.Name + "_Copy";

            // Ensure the new name is unique
            int copyIndex = 1;
            while (loggerConfig.channels.Exists(channel => channel.Name == newChannelName))
            {
                newChannelName = originalChannel.Name + $"_Copy{copyIndex}";
                copyIndex++;
            }

            DCLoggerConfig.Channel newChannel = new DCLoggerConfig.Channel(newChannelName, originalChannel.ChannelColor,
                originalChannel.LogType);
            loggerConfig.channels.Insert(index + 1, newChannel); // Insert the cloned channel right after the original
        }


        private bool IsValidClassName(string name)
        {
            // A valid C# class name must start with a letter or underscore and contain only letters, digits, or underscores
            return Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }

        private string GenerateEnumPreview()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("public enum " + channelScriptName);
            sb.AppendLine("{");
            for (int i = 0; i < loggerConfig.channels.Count; i++)
            {
                string enumValue = GenerateValidEnumName(loggerConfig.channels[i].Name);
                sb.AppendLine($"    {enumValue} = 1 << {i},");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private string GenerateValidEnumName(string name)
        {
            // Replace spaces with underscores
            return name.Replace(" ", "_");
        }

        private void GenerateEnumAndLoggerClasses()
        {
            string directoryPath = Path.Combine(enumFilePath, loggerFolderName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string enumFullPath = Path.Combine(directoryPath, channelScriptName + ".cs");
            string loggerFullPath = Path.Combine(directoryPath, "Logger.cs");

            // Define the namespace name
            string namespaceName = "DCLogger.Game"; // Replace with your desired namespace

            // Generate the enum with a namespace
            using (StreamWriter file = new StreamWriter(enumFullPath))
            {
                file.WriteLine("using System;");
                file.WriteLine();
                file.WriteLine($"namespace {namespaceName}");
                file.WriteLine("{");
                file.WriteLine("    [Flags]");
                file.WriteLine($"    public enum {channelScriptName}");
                file.WriteLine("    {");

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < loggerConfig.channels.Count; i++)
                {
                    string enumValue = GenerateValidEnumName(loggerConfig.channels[i].Name);
                    sb.AppendLine($"        {enumValue} = 1 << {i},");
                }

                file.Write(sb.ToString());

                file.WriteLine("    }");
                file.WriteLine("}");
            }

            // Generate the non-generic DCLogger class with a namespace
            using (StreamWriter file = new StreamWriter(loggerFullPath))
            {
                file.WriteLine("using DCLogger.Runtime;");
                file.WriteLine();
                file.WriteLine($"namespace {namespaceName}");
                file.WriteLine("{");
                file.WriteLine("    public static class Logger");
                file.WriteLine("    {");

                file.WriteLine($"        public static void Log(string message, {channelScriptName} channels)");
                file.WriteLine("        {");
                file.WriteLine($"            DCLoggerInternal<{channelScriptName}>.Log(message, channels);");
                file.WriteLine("        }");

                file.WriteLine("    }");
                file.WriteLine("}");
            }

            GenerateAsmDefFile(namespaceName);

            AssetDatabase.Refresh();
            isEnumGenerated = true;
            hasEnumChanges = false;
        }

        private void GenerateAsmDefFile(string @namespace)
        {
            var assemblyName = Assembly.GetAssembly(typeof(DCLoggerInternal<>)).GetName().Name;
            string asmdefFilePath = Path.Combine(enumFilePath, loggerFolderName, $"{@namespace}.asmdef");
            using (StreamWriter file = new StreamWriter(asmdefFilePath))
            {
                file.WriteLine("{");
                file.WriteLine($"    \"name\": \"{@namespace}\",");
                file.WriteLine("    \"references\": [");
                file.WriteLine($"        \"{assemblyName}\""); // Add your known reference here
                file.WriteLine("    ],");
                file.WriteLine("    \"includePlatforms\": [],");
                file.WriteLine("    \"excludePlatforms\": [],");
                file.WriteLine("    \"allowUnsafeCode\": false,");
                file.WriteLine("    \"overrideReferences\": false,");
                file.WriteLine("    \"precompiledReferences\": [],");
                file.WriteLine("    \"autoReferenced\": true,");
                file.WriteLine("    \"defineConstraints\": [],");
                file.WriteLine("    \"versionDefines\": [],");
                file.WriteLine("    \"noEngineReferences\": false");
                file.WriteLine("}");
            }

            AssetDatabase.Refresh();
        }

        private bool IsValidChannelName(string name)
        {
            // Check if the name starts with a letter and contains only letters, digits, and underscores
            return Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }

        private void SetAllChannels(bool enabled)
        {
            foreach (var channel in loggerConfig.channels)
            {
                channel.Enabled = enabled;
            }
            // This change doesn't affect the enum, so we don't set hasEnumChanges to true
        }

        private GUIContent GetLogTypeIcon(DCLoggerConfig.LoggingType logType)
        {
            switch (logType)
            {
                case DCLoggerConfig.LoggingType.Log:
                    return EditorGUIUtility.IconContent("console.infoicon");
                case DCLoggerConfig.LoggingType.Warning:
                    return EditorGUIUtility.IconContent("console.warnicon");
                case DCLoggerConfig.LoggingType.Error:
                    return EditorGUIUtility.IconContent("console.erroricon");
                default:
                    return EditorGUIUtility.IconContent("console.infoicon");
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
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}