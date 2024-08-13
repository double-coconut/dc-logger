using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCLogger.Runtime
{
    [CreateAssetMenu(fileName = "DCLoggerConfig", menuName = "Logging/DCLoggerConfig", order = 1)]
    public class DCLoggerConfig : ScriptableObject
    {
        [Serializable]
        public class Channel
        {
            public string Name;
            public bool Enabled;
            public bool IsEditing;
            public Color ChannelColor;
            public string OriginalName;
            public LoggingType LogType;

            public Channel(string name, Color color, LoggingType logType, bool enabled = true, bool isEditing = false)
            {
                Name = name;
                ChannelColor = color;
                LogType = logType;
                Enabled = enabled;
                IsEditing = isEditing;
                OriginalName = name;
            }
        }

        public List<Channel> channels = new List<Channel>();

        public enum LoggingType
        {
            Log,
            Warning,
            Error
        }
    }
}