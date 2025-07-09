using LightServer.Utils;
using ServerLibrary.Structs;

namespace LightServer.Base.PlayersModule
{
    public class ServerGameStateModule : ServerModuleBase
    {
        private EGameState state = EGameState.None;


        public EGameState GameState => state;
        public event Action<EGameState> OnGameStateChanged;


        public void ChangeGameState(EGameState state)
        {
            if (this.state != state)
            {
                LogUtils.LogGameState(state);
                this.state = state;
                OnGameStateChanged?.Invoke(state);
            }
        }
        
    }
}
