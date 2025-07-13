using LightServer.Base.PlayersModule;
using LightServer.Configs;
using LightServer.Utils;
using LiteNetLib;
using ServerLibrary.Structs;
namespace LightServer.Base
{

    public partial class Server
    {
        private ConfigData config;
        private EventBasedNetListener listener;
        private NetManager server;
        private WebHandler webHandler;

        List<ServerModuleBase> serverModules = new List<ServerModuleBase>();

        public bool IsRunning => server == null ? true : server.IsRunning;
        public ConfigData Config => config;

        public EventBasedNetListener Listener => listener;

        public event Action<ERPCName, int, NetPacketReader> OnRPCRecieved;

        public void InitServer()
        {
            PacketsManager.OnSendCMD += (state, id) => LogUtils.LogCMD(state, id);
            PacketsManager.OnSendRPC += (state, id) => LogUtils.LogRPC(state, id);

            LoadConfig();
            CreateWebHandler();
            CreatePerfomanceHandler();
            StartBroadcasting();

     


            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                LogUtils.LogServer("Stopping...");
                Shutdown();
            };
        }

        private void CreatePerfomanceHandler()
        {
            var perfLogger = new PerfomanceHandler();
            perfLogger.StartLogging(); // будет логгировать каждую секунду

        }

        private void CreateWebHandler()
        {
            webHandler = new WebHandler();
        }

        public NetPeer GetPeerByID(int id) => server.GetPeerById(id);

        public void Shutdown()
        {
            if (server.IsRunning)
            {
                webHandler.RemoveServer(config.Port);
                server.Stop();
            }
        }

        private void StartBroadcasting()
        {
            listener = new EventBasedNetListener();
            server = new NetManager(Listener);


            var started = server.Start(config.Port /* port */);

            LogUtils.LogServer("Server started: " + started);


            if (!started) return;


            Listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < config.MaxPlayersCount /* max connections */)
                    request.AcceptIfKey("SomeConnectionKey");
                else
                    request.Reject();
            };

            Listener.PeerConnectedEvent += peer =>
            {
                LogUtils.LogServer($"Connected ID: {peer.Id}");
                UpdateServerWebState();
            };

            Listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                LogUtils.LogServer($"Disconnected ID: {peer.Id}");
                UpdateServerWebState();
            };

            Listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;

            SetModules();

            UpdateServerWebState();

            var skipTime = 1000 / config.TickRate;
            while (server.IsRunning)
            {
                server.PollEvents();
                Thread.Sleep(skipTime);
                if (webHandler.IsNeedUpdateWebData(skipTime))
                {
                    UpdateServerWebState();
                }
            }

            Shutdown();
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            var rpcType = (ERPCName)reader.GetByte();
            var id = (int)reader.GetInt();

            LogUtils.LogRPC(rpcType, id);

            OnRPCRecieved?.Invoke(rpcType, id, reader);

        }

        private void SetModules()
        {
            var gameStateModule = AddModule<ServerGameStateModule>(new ServerGameStateModule());
            var timerModule = AddModule<ServerTimerModule>(new ServerTimerModule());
            var playersModule = AddModule<ServerPlayersModule>(new ServerPlayersModule());
            var blocksModule = AddModule<ServerBlocksModule>(new ServerBlocksModule());
            var weaponsSpotsModule = AddModule<ServerWeaponsSpotsModule>(new ServerWeaponsSpotsModule());

            timerModule.Init();
            playersModule.Init();
            blocksModule.Init();
            weaponsSpotsModule.Init();

            gameStateModule.ChangeGameState(EGameState.Warmup);
        }

        private void UpdateServerWebState()
        {
            webHandler.RegisterServer(config.Port, server.ConnectedPeersCount, config.MaxPlayersCount, config.ServerName, config.MapName);
        }

        private void LoadConfig()
        {
            config = ConfigData.Load();
        }

        public T GetModule<T>() where T : ServerModuleBase
        {
            return serverModules.Find(x => x.GetType().Name == typeof(T).Name) as T;
        }

        private T AddModule<T>(T module) where T : ServerModuleBase
        {
            serverModules.Add(module);
            module.InitModule(this);
            return module;
        }
    }
}
