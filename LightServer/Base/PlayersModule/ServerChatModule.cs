using LiteNetLib;
using ServerLibrary.Structs;

namespace LightServer.Base.PlayersModule
{
    public class ServerChatModule : ServerModuleBase
    {
        ServerPlayersModule playersModule;
        public void Init()
        {
            playersModule = server.GetModule<ServerPlayersModule>();
            server.OnRPCRecieved += Server_OnRPCRecieved;

            playersModule.OnServerPlayerConnected += PlayersModule_OnServerPlayerConnected;
            playersModule.OnServerPlayerDisconnected += PlayersModule_OnServerPlayerDisconnected;
        }

        private void PlayersModule_OnServerPlayerDisconnected(ServerPlayer obj)
        {
            MessageToAll(-256, $"<color=white>{obj.Nickname} is disconnected</color>");
        }

        private void PlayersModule_OnServerPlayerConnected(ServerPlayer obj)
        {
            MessageToAll(-256, $"<color=white>{obj.Nickname} is connected to server</color>");
        }

        private void Server_OnRPCRecieved(ERPCName rpcType, int id, NetPacketReader reader)
        {
            switch (rpcType)
            {
                case ERPCName.SendChatMessage:
                    HandleRPCSendMessage(reader, id);
                    break;
            }
        }

        private void HandleRPCSendMessage(NetPacketReader reader, int id)
        {
            var message = reader.GetString();
            MessageToAll(id, message);
        }

        private void MessageToAll(int id, string message)
        {
            playersModule.CommandToAllClients(delegate (int clientID)
            {
                server.GetPeerByID(clientID).CMDSendChatMessage(message, id);
            });
        }
    }
}
