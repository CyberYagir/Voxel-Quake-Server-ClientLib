using LightServer.Utils;

namespace LightServer.Base
{
    public enum WebMode
    {
        WriteOrUpdate = 0,
        Remove = 1,
        GetList = 2
    }
    public class WebHandler
    {
        private const string ServerUrl = "https://yagir.xyz/serverslist/";

        private HttpClient httpClient = new HttpClient();
        private string ip = string.Empty;
        private DateTime updateMilliseconds;

        public WebHandler()
        {
            updateMilliseconds = DateTime.UtcNow;
        }

        public async Task<string> GetPublicIPAsync()
        {
            using HttpClient client = new();
            string ip = await client.GetStringAsync("https://api.ipify.org");
            return ip.Trim();
        }


        public async Task RegisterServerAsync(int port, int players, int maxPlayers, string title, string map)
        {
            ResetTimer();
            if (ip == string.Empty)
            {
                ip = await GetPublicIPAsync();
            }
            var url = $"{ServerUrl}?ip={ip}&port={port}&players={players}&max={maxPlayers}&title={title}&map={map}&mode={(int)WebMode.WriteOrUpdate}";

            //Console.WriteLine(url);
            try
            {
                var response = await httpClient.GetAsync(url);
                string result = await response.Content.ReadAsStringAsync();
                LogUtils.LogWeb("RegisterServer response: " + result);
            }
            catch (Exception ex)
            {
                LogUtils.LogWeb("RegisterServer error: " + ex.Message);
            }
        }
        public async Task RemoveServerAsync(int port)
        {
            ResetTimer();

            var url = $"{ServerUrl}?ip={ip}&port={port}&mode={(int)WebMode.WriteOrUpdate}";
            try
            {
                var response = await httpClient.GetAsync(url);
                string result = await response.Content.ReadAsStringAsync();
                LogUtils.LogServer("RemoveServer response: " + result);
            }
            catch (Exception ex)
            {
                LogUtils.LogServer("RemoveServer error: " + ex.Message);
            }
        }

        public void RegisterServer(int port, int players, int maxPlayers, string title, string map)
        {
            RegisterServerAsync(port, players, maxPlayers, title, map).GetAwaiter().GetResult();
        }

        public void RemoveServer(int port)
        {
            RemoveServerAsync(port).GetAwaiter().GetResult();
        }

        public bool IsNeedUpdateWebData(int skipTime)
        {

            if ((DateTime.UtcNow - updateMilliseconds).TotalSeconds > 25)
            {
                ResetTimer();
                return true;
            }

            return false;
        }

        private void ResetTimer()
        {
            updateMilliseconds = DateTime.UtcNow;
        }
    }
}
