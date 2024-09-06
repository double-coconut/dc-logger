using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace DCLogger.Runtime.Configs
{
    public class DCLoggerConfig : ScriptableObject
    {
        public List<ModuleConfig> moduleConfigs = new List<ModuleConfig>();

        private Dictionary<string, List<ChannelState>> moduleChannelStates =
            new Dictionary<string, List<ChannelState>>();

        public void SetChannelState(string moduleName, string channelId, bool state)
        {
            if (!moduleChannelStates.TryGetValue(moduleName, out List<ChannelState> channelState))
            {
                moduleChannelStates.Add(moduleName, new List<ChannelState>
                {
                    new ChannelState(channelId, state)
                });
                return;
            }

            ChannelState channel = channelState.Find(channel => channel.Id == channelId);
            if (channel == null)
            {
                moduleChannelStates[moduleName].Add(new ChannelState(channelId, state));
                return;
            }

            channel.Enabled = state;
        }

        public bool? GetChannelState(string moduleName, string channelId)
        {
            if (!moduleChannelStates.TryGetValue(moduleName, out List<ChannelState> channelState))
            {
                return null;
            }

            ChannelState channel = channelState.Find(channel => channel.Id == channelId);

            return channel?.Enabled;
        }

        public void RemoveChannel(string moduleName, string channelId)
        {
            if (!moduleChannelStates.ContainsKey(moduleName))
            {
                return;
            }

            moduleChannelStates[moduleName].RemoveAll(channel => channel.Id == channelId);
        }
    }
}