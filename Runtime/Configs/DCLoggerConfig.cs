using System.Collections.Generic;
using UnityEngine;

namespace DCLogger.Runtime.Configs
{
    public class DCLoggerConfig : ScriptableObject
    {
        public List<ModuleConfig> moduleConfigs = new List<ModuleConfig>();
        [HideInInspector] public List<ChannelState> channelStates = new List<ChannelState>();

        public void SetChannelState(string moduleName, string channelId, bool state)
        {
            ChannelState channelState = channelStates.Find(channelState =>
                channelState.ModuleName == moduleName && channelState.Id == channelId);
            if (channelState == null)
            {
                channelStates.Add(new ChannelState(moduleName, channelId, state));
                return;
            }

            channelState.Enabled = state;
        }

        public bool? GetChannelState(string moduleName, string channelId)
        {
            ChannelState channelState = channelStates.Find(channelState =>
                channelState.ModuleName == moduleName && channelState.Id == channelId);
            return channelState?.Enabled;
        }

        public void RemoveChannel(string moduleName, string channelId)
        {
            channelStates.RemoveAll(channel => channel.ModuleName == moduleName && channel.Id == channelId);
        }
    }
}