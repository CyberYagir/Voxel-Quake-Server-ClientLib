using LightServer.Utils;
using ServerLibrary.Structs;

namespace LightServer.Base.PlayersModule
{
    public class ServerTimerModule : ServerModuleBase
    {
        private int time;
        private Timer timer;

        public int Time => time;

        public event Action<int> OnTimerUpdate;

        public void Init()
        {
            var gameState = server.GetModule<ServerGameStateModule>();


            gameState.OnGameStateChanged += delegate (EGameState state)
            {
                if (state == EGameState.Game)
                {
                    time = server.Config.Time;
                    LogUtils.LogTimer("Timer start " + time);
                    timer = new Timer(UpdateTime, null, 0, 1000);
                }
                else
                {
                    if (timer != null)
                    {
                        timer.Dispose();
                    }
                }
            };

        }

        private void UpdateTime(object state)
        {
            if (time >= 0)
            {
                time--;

                if (time <= 0)
                {
                    LogUtils.LogTimer("Ended " + time);
                }

                OnTimerUpdate?.Invoke(time);
            }
        }
    }
}
