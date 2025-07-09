using LightServer.Utils;

namespace LightServer.Configs
{
    [Serializable]
    public class ConfigData: FileObject<ConfigData>
    {
        public int TickRate = 60;
        public int MaxPlayersCount  = 10;
        public int Port  = 9050;
        public string MapName  = "";
        public string ServerName  = "";
        public int Time = 300;
            
        public ConfigData()
        {
        }

        public override string GetPath() => PathUtils.CONFIG_PATH;

        public static ConfigData Load()
        {
            var config = new ConfigData();
            if (File.Exists(config.GetPath())) {
                LogUtils.LogFile("Config Loaded");
                return config.LoadFile();
            }
            config.SaveFile();
            return config;
        }
    }
}
