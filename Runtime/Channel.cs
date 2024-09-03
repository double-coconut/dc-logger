using System;
using UnityEngine;

namespace DCLogger.Runtime
{
    [System.Serializable]
    public class Channel
    {
        public string Id;
        public string Name;
        public string OriginalName;
        public bool Enabled;
        public bool IsEditing;
        public Color ChannelColor;

        public Channel(string name, Color color)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            OriginalName = name;
            Enabled = true;
            IsEditing = false;
            ChannelColor = color;
        }
    }
}