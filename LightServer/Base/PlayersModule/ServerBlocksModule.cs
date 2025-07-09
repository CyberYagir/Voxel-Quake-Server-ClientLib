using LightServer.Utils;
using LiteNetLib;
using ServerLibrary.Structs;

namespace LightServer.Base.PlayersModule
{
    public class ServerBlocksModule : ServerModuleBase
    {
        Dictionary<NetVector3Int, NetChunkData> chunkChanges = new Dictionary<NetVector3Int, NetChunkData>();

        Dictionary<NetVector3Int, NetChunkData> netChunksDataTmp = new Dictionary<NetVector3Int, NetChunkData>();

        public Dictionary<NetVector3Int, NetChunkData> ChunksData => chunkChanges;


        public void Init()
        {
            server.OnRPCRecieved += Server_OnRPCRecieved;
        }

        private void Server_OnRPCRecieved(ERPCName rpcType, int id, NetPacketReader reader)
        {
            switch (rpcType)
            {
                case ERPCName.RemoveBlocks:
                    HandleRPCRemoveBlocks(reader);
                    break;
            }
        }

        private void HandleRPCRemoveBlocks(NetPacketReader reader)
        {
            PacketsManager.ReadNetChunksData(reader, netChunksDataTmp);

            foreach (var chunk in netChunksDataTmp)
            {
                if (!chunkChanges.ContainsKey(chunk.Key))
                {
                    chunkChanges.Add(chunk.Key, chunk.Value);
                    continue;
                }


                foreach (var block in chunk.Value.blocks)
                {
                    if (chunkChanges[chunk.Key].blocks.ContainsKey(block.Key))
                    {
                        chunkChanges[chunk.Key].blocks[block.Key] = block.Value;
                    }
                    else
                    {
                        chunkChanges[chunk.Key].blocks.Add(block.Key, block.Value);
                    }
                }
            }
        }
    }
}
