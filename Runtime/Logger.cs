using System.Collections.Generic;
using System.Linq;
using DCLogger.Runtime.Configs;
using UnityEngine;

namespace DCLogger.Runtime
{
    public static class Logger
    {
        private static readonly Dictionary<string, ChannelInfo> _channelStates = new Dictionary<string, ChannelInfo>();

        static Logger()
        {
            // Initialize channel states from the active DCLoggerConfig
            InitializeChannelStates();
        }

        private static void InitializeChannelStates()
        {
            // Load the main DCLoggerConfig from Resources
            DCLoggerConfig loggerConfig = Resources.Load<DCLoggerConfig>(nameof(DCLoggerConfig));

            if (loggerConfig == null)
            {
                Debug.LogError("DCLoggerConfig not found in Resources. Please ensure it exists.");
                return;
            }

            // Populate the channelStates dictionary with initial states and colors
            foreach (var moduleConfig in loggerConfig.moduleConfigs)
            {
                foreach (var channel in moduleConfig.Channels)
                {
                    string channelKey = channel.Id;
                    if (!_channelStates.ContainsKey(channelKey))
                    {
                        _channelStates[channelKey] = new ChannelInfo
                        {
                            Name = channel.Name,
                            IsActive = loggerConfig.GetChannelState(moduleConfig.ModuleName, channelKey) ?? true,
                            Color = ColorUtility.ToHtmlStringRGB(channel.ChannelColor)
                        };
                    }
                }
            }
        }

        public static void Log(string message, params string[] channelNames)
        {
            var activeChannels = GetActiveChannels(channelNames);

            if (activeChannels.Any())
            {
                string formattedMessage = FormatLogMessage(message, activeChannels);
                Debug.Log(formattedMessage);
            }
        }

        public static void LogWarning(string message, params string[] channelNames)
        {
            var activeChannels = GetActiveChannels(channelNames);

            if (activeChannels.Any())
            {
                string formattedMessage = FormatLogMessage(message, activeChannels);
                Debug.LogWarning(formattedMessage);
            }
        }

        public static void LogError(string message, params string[] channelNames)
        {
            var activeChannels = GetActiveChannels(channelNames);

            if (activeChannels.Any())
            {
                string formattedMessage = FormatLogMessage(message, activeChannels);
                Debug.LogError(formattedMessage);
            }
        }

        private static List<string> GetActiveChannels(params string[] channelNames)
        {
            var activeChannels = new List<string>();

            foreach (var channelName in channelNames)
            {
                if (_channelStates.TryGetValue(channelName, out ChannelInfo info) && info.IsActive)
                {
                    activeChannels.Add($"<color=#{info.Color}>[{channelName}]</color>");
                }
            }

            if (!activeChannels.Any())
            {
                activeChannels.Add($"<color=#{ColorUtility.ToHtmlStringRGB(Color.red)}>[Unspecified]</color>");
            }

            return activeChannels;
        }

        private static string FormatLogMessage(string message, List<string> activeChannels)
        {
            string combinedChannels = string.Join(", ", activeChannels);
            return $"{combinedChannels} {message}";
        }

        public static void SetChannelState(string channelName, bool isEnabled)
        {
            if (_channelStates.ContainsKey(channelName))
            {
                _channelStates[channelName].IsActive = isEnabled;
            }
        }

        private class ChannelInfo
        {
            public string Name;
            public bool IsActive;
            public string Color;
        }
    }
}