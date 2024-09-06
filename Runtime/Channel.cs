using System;
using UnityEngine;

namespace DCLogger.Runtime
{
    [Serializable]
    public class Channel
    {
        public string Id;
        public string Name;
        public string OriginalName;
        public bool IsEditing;
        public Color ChannelColor;

        public Channel(string name, Color color)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            OriginalName = name;
            IsEditing = false;
            ChannelColor = color;
        }
    }

    [Serializable]
    public class ChannelState
    {
        public string Id;
        public bool Enabled;

        public ChannelState(string id, bool state)
        {
            Id = id;
            Enabled = state;
        }
    }
}