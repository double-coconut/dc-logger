using System;
using System.Text;
using UnityEngine;

namespace DCLogger.Runtime
{
    public static class DCLoggerInternal<TEnum> where TEnum : Enum
    {
        private static DCLoggerConfig config;

        static DCLoggerInternal()
        {
            config = Resources.Load<DCLoggerConfig>("DCLoggerConfig"); // Load the ScriptableObject from Resources
        }


        public static void Log(string message, TEnum channels)
        {
            if (config == null)
            {
                Debug.LogWarning("DCLoggerConfig not found. Please ensure the configuration file exists in Resources.");
                return;
            }

            int inputChannelValue = Convert.ToInt32(channels);
            int combinedEnabledChannels = 0;
            StringBuilder channelNames = new StringBuilder();
            DCLoggerConfig.Channel logChannel = null;

            foreach (DCLoggerConfig.Channel channel in config.channels)
            {
                int channelValue = Convert.ToInt32(EnumFromChannelName(channel.Name));
                if (channel.Enabled && (inputChannelValue & channelValue) != 0)
                {
                    combinedEnabledChannels |= channelValue;
                    if (channelNames.Length > 0)
                    {
                        channelNames.Append(", ");
                    }

                    // Add the channel name with its respective color
                    string coloredChannelName = FormatChannelName(channel.Name, channel.ChannelColor);
                    channelNames.Append(coloredChannelName);

                    logChannel = channel; // Keep track of the last channel to determine the log type
                }
            }

            // Log if any of the specified channels are enabled
            if (combinedEnabledChannels != 0 && channelNames.Length > 0 && logChannel != null)
            {
                string formattedMessage = $"{channelNames}: {message}";
                switch (logChannel.LogType)
                {
                    case DCLoggerConfig.LoggingType.Log:
                        Debug.Log(formattedMessage);
                        break;
                    case DCLoggerConfig.LoggingType.Warning:
                        Debug.LogWarning(formattedMessage);
                        break;
                    case DCLoggerConfig.LoggingType.Error:
                        Debug.LogError(formattedMessage);
                        break;
                }
            }
        }

        private static TEnum EnumFromChannelName(string channelName)
        {
            return (TEnum) Enum.Parse(typeof(TEnum), channelName);
        }

        private static string FormatChannelName(string channelName, Color channelColor)
        {
            string colorHex = ColorUtility.ToHtmlStringRGBA(channelColor); // Convert the color to a hex string
            return $"<color=#{colorHex}>[{channelName}]</color>";
        }
    }
}