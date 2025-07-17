using LiteNetLib;
using ServerLibrary.Structs;

namespace LightServer.Base.PlayersModule
{
    public class ServerChatModule : ServerModuleBase
    {
        public void Init()
        {
           
            server.OnRPCRecieved += Server_OnRPCRecieved;
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
            server.GetModule<ServerPlayersModule>().CommandToAllClients(delegate (int clientID)
            {
                server.GetPeerByID(clientID).CMDSendChatMessage(message, id);
            });
        }
    }
}
