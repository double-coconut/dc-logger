using System.Collections.Generic;
using UnityEngine;

namespace DCLogger.Runtime.Configs
{
    public class DCLoggerConfig : ScriptableObject
    {
        public List<ModuleConfig> moduleConfigs = new List<ModuleConfig>();
    }
}