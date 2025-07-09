using LightServer.Utils;
using LiteNetLib;
using ServerLibrary.Structs;

namespace LightServer.Base.PlayersModule
{

    public class ServerPlayersModule : ServerModuleBase
    {
        List<string> spawnedProjectiles = new List<string>();

        Dictionary<int, ServerPlayer> players = new Dictionary<int, ServerPlayer>();
        ServerGameStateModule gameStateModule;
        ServerTimerModule timerModule;
        ServerBlocksModule blocksModule;
        
        
        public void Init()
        {
            blocksModule = server.GetModule<ServerBlocksModule>();
            timerModule = server.GetModule<ServerTimerModule>();
            timerModule.OnTimerUpdate += (time) =>
            {
                UpdateTimersAll(time);
            };

            
            gameStateModule = server.GetModule<ServerGameStateModule>();
            gameStateModule.OnGameStateChanged += UpdateGameStateAll;


            server.Listener.PeerConnectedEvent += Listener_PeerConnectedEvent;

            server.Listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;

            server.OnRPCRecieved += Server_OnRPCRecieved; ;
        }

        private void Server_OnRPCRecieved(ERPCName rpcType, int id, NetPacketReader reader)
        {
            switch (rpcType)
            {
                case ERPCName.InitPlayer:
                    HandleRPCInitPlayer(reader, id);
                    break;
                case ERPCName.PlayerRespawn:
                    HandleRPCRespawnPlayer(reader, id);
                    break;
                case ERPCName.ChangePlayerState:
                    HandleRPCChangePlayerState(reader, id);
                    break;
                case ERPCName.UpdatePlayerTransform:
                    HandleRPCUpdatePlayerTransform(reader, id);
                    break;
                case ERPCName.SpawnProjectile:
                    HandleRPCSpawnProjectile(reader, id);
                    break;
                case ERPCName.DestroyProjectile:
                    HandleRPCDestroyProjectile(reader, id);
                    break;
                default:
                    break;
            }
        }

        private void HandleRPCDestroyProjectile(NetPacketReader reader, int id)
        {
            var projectileUID = reader.GetString();
            var pos = PacketsManager.ReadVector(reader);

            spawnedProjectiles.Remove(projectileUID);

            CommandToAllClients(delegate (int clientID)
            {
                server.GetPeerByID(clientID).CMDSendDestroyProjectile(projectileUID, pos);
            });
        }

        private void UpdateTimersAll(int time)
        {
            CommandToAllClients(delegate (int id)
            {
                server.GetPeerByID(id).CMDSendTimer(time);
            });
        }

        private void UpdateGameStateAll(EGameState state)
        {
            CommandToAllClients(delegate (int id)
            {
                server.GetPeerByID(id).CMDSendGameState(state);
            });
        }

        private void HandleRPCSpawnProjectile(NetPacketReader reader, int id)
        {
            EProjectileType projectileType = (EProjectileType)reader.GetByte();
            NetVector3 pos = PacketsManager.ReadVector(reader);
            NetVector3 forward = PacketsManager.ReadVector(reader);
            NetVector3 spawnPoint = PacketsManager.ReadVector(reader);

            if (players.ContainsKey(id))
            {
                var projectileID = Guid.NewGuid().ToString();
                spawnedProjectiles.Add(projectileID);
                CommandToAllClients(delegate (int clientID)
                {
                    server.GetPeerByID(clientID).CMDSendSpawnProjetile(players[id], projectileType, pos, forward, spawnPoint, projectileID);
                });
            }
        }

        private void HandleRPCUpdatePlayerTransform(NetPacketReader reader, int id)
        {
            var pos = PacketsManager.ReadVector(reader);
            var rot = PacketsManager.ReadVector(reader);
            var vel = PacketsManager.ReadVector(reader);
            var cameraX = reader.GetFloat();

            if (players.ContainsKey(id))
            {
                if (players[id].IsInited && players[id].IsSpawned)
                {
                    players[id].UpdatePosition(pos);
                    players[id].UpdateRotation(rot);
                    players[id].UpdateVelocity(vel);
                    players[id].UpdateCameraX(cameraX);


                    CommandToAllClients(delegate (int clientID)
                    {
                        server.GetPeerByID(clientID).CMDSendUpdatePlayerTransform(players[id]);
                    });
                }
            }
        }

        private void HandleRPCChangePlayerState(NetPacketReader reader, int id)
        {
      
            if (players.ContainsKey(id))
            {
                if (players[id].IsInited)
                {
                    var state = (EClientState)reader.GetByte();

                    players[id].ChangeState(state);

                    if (state == EClientState.InGame)
                    {

                        foreach (var player in players)
                        {
                            if (player.Value.IsSpawned && player.Value.IsInited)
                            {
                                server.GetPeerByID(id).CMDSendRespawnPlayer(player.Value);
                                server.GetPeerByID(id).CMDSendUpdatePlayerTransform(player.Value, DeliveryMethod.ReliableOrdered);
                            }
                        }
                    }
                }
            }
        }

        private void HandleRPCRespawnPlayer(NetPacketReader reader, int id)
        {
            var pos = PacketsManager.ReadVector(reader);
            var rot = PacketsManager.ReadVector(reader);

            if (players.ContainsKey(id))
            {
                if (!players[id].IsSpawned)
                {
                    players[id].UpdatePosition(pos);
                    players[id].UpdateRotation(rot);

                    players[id].SpawnPlayer();

                    CommandToAllClients(delegate(int clientID)
                    {
                        server.GetPeerByID(clientID).CMDSendRespawnPlayer(players[id]);
                    });
                }
            }
        }

        private void HandleRPCInitPlayer(NetPacketReader reader, int id)
        {
            var nickName = reader.GetString();

            if (players.ContainsKey(id))
            {
                players[id].Init(id, nickName);

                server.GetPeerByID(id).CMDSendMap(server.Config.MapName);
                server.GetPeerByID(id).CMDSendGameState(gameStateModule.GameState);
                server.GetPeerByID(id).CMDSendTimer(timerModule.Time);
                server.GetPeerByID(id).CMDSendChangedBlocks(blocksModule.ChunksData);


                OnPlayersUpdate();
            }
            
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (players.ContainsKey(peer.Id))
            {
                var player = players[peer.Id];
                players.Remove(peer.Id);

                CommandToAllClients(delegate (int clientID)
                {
                    server.GetPeerByID(clientID).CMDSendDestroyPlayer(player);
                });
            }
            if (players.Count > 0)
            {
                OnPlayersUpdate();
            }
            else
            {
                if (players.Count <= 0 && gameStateModule.GameState == EGameState.Game)
                {
                    gameStateModule.ChangeGameState(EGameState.End);
                }
            }
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            if (players.ContainsKey(peer.Id))
            {
                players.Remove(peer.Id);
            }
            players.Add(peer.Id, new ServerPlayer());

            if (players.Count > 0 && gameStateModule.GameState == EGameState.Warmup)
            {
                gameStateModule.ChangeGameState(EGameState.Game);
            }
        }

        public void OnPlayersUpdate()
        {
            CommandToAllClients(delegate (int id)
            {
                server.GetPeerByID(id).CMDSendPlayerList(players);
            });

            UpdateTimersAll(timerModule.Time);
            UpdateGameStateAll(gameStateModule.GameState);
        }

        public void CommandToAllClients(Action<int> action)
        {
            foreach (var item in players)
            {
                var client = item.Value;
                if (client.IsInited && client.State == EClientState.InGame)
                {
                    action?.Invoke(client.Id);
                }
            }
        }
    }
}
