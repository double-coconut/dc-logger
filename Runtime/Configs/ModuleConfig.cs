using System.Collections.Generic;
using UnityEngine;

namespace DCLogger.Runtime.Configs
{
    [CreateAssetMenu(fileName = "NewLoggerModuleConfig", menuName = "DCLogger/ModuleConfig")]
    public class ModuleConfig : ScriptableObject
    {
        public string ModuleName;
        public List<Channel> Channels = new List<Channel>();

        public void AddChannel(string channelName, Color color)
        {
            Channels.Add(new Channel(channelName, color));
        }
    }
}