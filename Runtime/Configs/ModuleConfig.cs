using System.Collections.Generic;
using UnityEngine;

namespace DCLogger.Runtime.Configs
{
    [CreateAssetMenu(fileName = "NewLoggerModuleConfig", menuName = "DCLogger/ModuleConfig")]
    public class ModuleConfig : ScriptableObject
    {
        public string ModuleName;
        public List<Channel> Channels = new List<Channel>();

        public Channel AddChannel(string channelName, Color color)
        {
            var channel = new Channel(channelName, color);
            Channels.Add(channel);
            return channel;
        }
    }
}