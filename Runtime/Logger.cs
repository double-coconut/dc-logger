using System.Collections.Generic;
using System.Linq;
using DCLogger.Runtime.Configs;
using UnityEngine;

namespace DCLogger.Runtime
{
    public static class Logger
    {
        private static readonly Dictionary<string, ChannelInfo> _channelInfo = new Dictionary<string, ChannelInfo>();

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
                    if (!_channelInfo.ContainsKey(channelKey))
                    {
                        _channelInfo[channelKey] = new ChannelInfo
                        {
                            ModuleName = moduleConfig.ModuleName,
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

        private static List<ChannelInfo> GetActiveChannels(string[] channelIds)
        {
            List<ChannelInfo> channels = channelIds
                .Where(id => _channelInfo.ContainsKey(id))
                .Select(id => _channelInfo[id])
                .ToList();

            if (channels.Count == 0)
            {
                channels.Add(new ChannelInfo
                {
                    ModuleName = "null",
                    Name = "Unspecified",
                    Color = ColorUtility.ToHtmlStringRGB(Color.red),
                    IsActive = true
                });
                return channels;
            }

            channels = channels.Where(info => info.IsActive).ToList();
            return channels;
        }

        private static string FormatLogMessage(string message, List<ChannelInfo> activeChannels)
        {
            const string format = "<color=#{0}>[{1}]</color>";
            string combinedChannels = string.Join(", ",
                activeChannels.Select(info => string.Format(format, info.Color, $"{info.ModuleName}.{info.Name}")));
            return $"{combinedChannels}: {message}";
        }

        public static void SetChannelState(string channelName, bool isEnabled)
        {
            if (_channelInfo.ContainsKey(channelName))
            {
                _channelInfo[channelName].IsActive = isEnabled;
            }
        }

        private class ChannelInfo
        {
            public string ModuleName;
            public string Name;
            public bool IsActive;
            public string Color;
        }
    }
}