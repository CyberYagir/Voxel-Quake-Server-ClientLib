using System;
using System.Collections.Generic;
using LightServer.Base.PlayersModule;
using LiteNetLib;
using LiteNetLib.Utils;

namespace ServerLibrary.Structs
{
    public static class PacketsManager
    {
        private static NetDataWriter writer;

        public static event Action<ERPCName, int> OnSendRPC;
        public static event Action<ECMDName, int> OnSendCMD;

        // RPC - Client > Server
        // Command - Server > Client


        public static NetDataWriter GetWriter()
        {
            if (writer == null)
            {
                writer = new NetDataWriter();
            }
            writer.Reset(writer.Length);

            return writer;
        }

        #region RPC
        public static void RPCInitPlayer(this NetPeer peer, string nickname)
        {
            var writer = GetWriter();

            writer.Put((byte)ERPCName.InitPlayer);
            writer.Put(peer.RemoteId);
            writer.Put(nickname);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            OnSendRPC?.Invoke(ERPCName.InitPlayer, peer.RemoteId);
        }


        public static void RPCSpawnPlayer(this NetPeer peer, NetVector3 position, NetVector3 rotation)
        {
            var writer = GetWriter();
            writer.Put((byte)ERPCName.PlayerRespawn);
            writer.Put(peer.RemoteId);
            WriteVector(writer, position);
            WriteVector(writer, rotation);



            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            OnSendRPC?.Invoke(ERPCName.PlayerRespawn, peer.RemoteId);
        }

        public static void RPCChangeState(this NetPeer peer, EClientState clientState)
        {
            var writer = GetWriter();
            writer.Put((byte)ERPCName.ChangePlayerState);
            writer.Put(peer.RemoteId);
            writer.Put((byte)clientState);


            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            OnSendRPC?.Invoke(ERPCName.ChangePlayerState, peer.RemoteId);
        }


        public static void RPCGetPlayerList(this NetPeer peer)
        {
            var writer = GetWriter();
            writer.Put((byte)ERPCName.GetPlayersList);
            writer.Put(peer.RemoteId);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            OnSendRPC?.Invoke(ERPCName.GetPlayersList, peer.RemoteId);
        }

        public static void RPCSpawnProjectile(this NetPeer peer, EProjectileType projectileID, NetVector3 camPos, NetVector3 camForward, NetVector3 spawnPoint)
        {
            var writer = GetWriter();
            writer.Put((byte)ERPCName.SpawnProjectile);
            writer.Put(peer.RemoteId);

            writer.Put((byte)projectileID);

            WriteVector(writer, camPos);
            WriteVector(writer, camForward);
            WriteVector(writer, spawnPoint);


            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            OnSendRPC?.Invoke(ERPCName.SpawnProjectile, peer.RemoteId);
        }

        public static void RPCUpdatePlayerTransform(this NetPeer peer, NetVector3 pos, NetVector3 rot, NetVector3 vel, float cameraX)
        {
            var writer = GetWriter();
            writer.Put((byte)ERPCName.UpdatePlayerTransform);
            writer.Put(peer.RemoteId);
            WriteVector(writer, pos);
            WriteVector(writer, rot);
            WriteVector(writer, vel);
            writer.Put(cameraX);

            peer.Send(writer, DeliveryMethod.Sequenced);

            OnSendRPC?.Invoke(ERPCName.UpdatePlayerTransform, peer.RemoteId);
        }

        public static void RPCRemoveBlocks(this NetPeer peer, Dictionary<NetVector3Int, NetChunkData> netChunksData)
        {
            var writer = GetWriter();
            writer.Put((byte)ERPCName.RemoveBlocks);
            writer.Put(peer.RemoteId);

            WriteChunksData(netChunksData, writer);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            OnSendRPC?.Invoke(ERPCName.RemoveBlocks, peer.RemoteId);
        }

        public static void RPCDestroyProjectile(this NetPeer peer, string projectileUID, NetVector3 pos)
        {
            var writer = GetWriter();
            writer.Put((byte)ERPCName.DestroyProjectile);
            writer.Put(peer.RemoteId);
            writer.Put(projectileUID);
            WriteVector(writer, pos);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            OnSendRPC?.Invoke(ERPCName.DestroyProjectile, peer.RemoteId);
        }

        public static void WriteChunksData(Dictionary<NetVector3Int, NetChunkData> netChunksData, NetDataWriter writer)
        {
            writer.Put(netChunksData.Count);

            foreach (var netChunkData in netChunksData)
            {
                WriteVectorInt(writer, netChunkData.Key);
                writer.Put(netChunkData.Value.blocks.Count);

                foreach (var block in netChunkData.Value.blocks)
                {
                    writer.Put(block.Key);
                    writer.Put(block.Value.type);
                    writer.Put(block.Value.material);
                }
            }
        }


        public static void ReadNetChunksData(NetPacketReader reader, Dictionary<NetVector3Int, NetChunkData> netChunksDataTmp)
        {
            netChunksDataTmp.Clear();
            var chunksCount = reader.GetInt();

            for (int i = 0; i < chunksCount; i++)
            {
                var pos = PacketsManager.ReadVectorInt(reader);

                var blocksCount = reader.GetInt();

                var blocksData = new Dictionary<int, NetBlockData>();


                for (int j = 0; j < blocksCount; j++)
                {
                    var index = reader.GetInt();
                    var type = reader.GetInt();
                    var material = reader.GetInt();

                    blocksData.Add(index, new NetBlockData(type, material));
                }

                netChunksDataTmp.Add(pos, new NetChunkData(blocksData));
            }
        }

        #endregion



        #region CMD

        //CMDSendSpawnProjetile
        public static void CMDSendSpawnProjetile(this NetPeer peer, ServerPlayer owner, EProjectileType projectileType, NetVector3 camPos, NetVector3 camForward, NetVector3 spawnPoint, string projectileUID)
        {
            var writer = GetWriter();
            writer.Put((byte)ECMDName.SpawnProjectile);
            writer.Put(peer.Id);
            writer.Put(owner.Id);
            writer.Put((byte)projectileType);
            writer.Put(projectileUID);


            WriteVector(writer, camPos);
            WriteVector(writer, camForward);
            WriteVector(writer, spawnPoint);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);


            OnSendCMD?.Invoke(ECMDName.SpawnProjectile, peer.Id);
        }
        public static void CMDSendPlayerList(this NetPeer peer, Dictionary<int, ServerPlayer> players)
        {
            var writer = GetWriter();
            writer.Put((byte)ECMDName.GetPlayersList);
            writer.Put(peer.Id);
            writer.Put(players.Count);
            foreach (var item in players)
            {
                writer.Put(item.Key);
                writer.Put(item.Value.Nickname);
                writer.Put(item.Value.IsInited);
            }
            peer.Send(writer, DeliveryMethod.ReliableOrdered);


            OnSendCMD?.Invoke(ECMDName.GetPlayersList, peer.Id);
        }

        public static void CMDSendMap(this NetPeer peer, string map)
        {
            var writer = GetWriter();
            writer.Put((byte)ECMDName.GetMap);
            writer.Put(peer.Id);
            writer.Put(map);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            OnSendCMD?.Invoke(ECMDName.GetMap, peer.Id);
        }

        public static void CMDSendTimer(this NetPeer peer, int time)
        {
            var writer = GetWriter();
            writer.Put((byte)ECMDName.GetTime);
            writer.Put(peer.Id);
            writer.Put(time);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            OnSendCMD?.Invoke(ECMDName.GetTime, peer.Id);
        }

        public static void CMDSendGameState(this NetPeer peer, EGameState gameState)
        {
            var writer = GetWriter();
            writer.Put((byte)ECMDName.GetGameState);
            writer.Put(peer.Id);
            writer.Put((byte)gameState);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);


            OnSendCMD?.Invoke(ECMDName.GetGameState, peer.Id);
        }

        public static void CMDSendRespawnPlayer(this NetPeer peer, ServerPlayer player)
        {
            var writer = GetWriter();
            writer.Put((byte)ECMDName.RespawnPlayer);
            writer.Put(peer.Id);
            writer.Put(player.Id);
            WriteVector(writer, player.Position);
            WriteVector(writer, player.Rotation);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);


            OnSendCMD?.Invoke(ECMDName.RespawnPlayer, peer.Id);
        }

        public static void CMDSendUpdatePlayerTransform(this NetPeer peer, ServerPlayer player, DeliveryMethod method = DeliveryMethod.Sequenced)
        {
            var writer = GetWriter();
            writer.Put((byte)ECMDName.UpdatePlayerTransform);
            writer.Put(peer.Id);
            writer.Put(player.Id);
            WriteVector(writer, player.Position);
            WriteVector(writer, player.Rotation);
            WriteVector(writer, player.Velocity);
            writer.Put(player.CameraX);

            peer.Send(writer, DeliveryMethod.Sequenced);


            OnSendCMD?.Invoke(ECMDName.UpdatePlayerTransform, peer.Id);
        }

        public static void CMDSendDestroyPlayer(this NetPeer peer, ServerPlayer player)
        {
            var writer = GetWriter();
            writer.Put((byte)ECMDName.DestroyPlayer);
            writer.Put(peer.Id);
            writer.Put(player.Id);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);


            OnSendCMD?.Invoke(ECMDName.DestroyPlayer, peer.Id);
        }

        public static void CMDSendChangedBlocks(this NetPeer peer, Dictionary<NetVector3Int, NetChunkData> netChunksData)
        {
            var writer = GetWriter();
            writer.Put((byte)ECMDName.GetChangedBlocks);
            writer.Put(peer.Id);

            WriteChunksData(netChunksData, writer);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            OnSendCMD?.Invoke(ECMDName.GetChangedBlocks, peer.Id);
        }

        public static void CMDSendDestroyProjectile(this NetPeer peer, string projectileUID, NetVector3 pos)
        {
            var writer = GetWriter();
            writer.Put((byte)ECMDName.DestroyProjectile);
            writer.Put(peer.Id);
            writer.Put(projectileUID);
            WriteVector(writer, pos);

            peer.Send(writer, DeliveryMethod.ReliableOrdered);

            OnSendCMD?.Invoke(ECMDName.DestroyProjectile, peer.Id);
        }


        #endregion

        public static NetVector3 ReadVector(NetPacketReader reader)
        {
            return new NetVector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
        private static void WriteVector(NetDataWriter writer, NetVector3 position)
        {
            writer.Put(position.x);
            writer.Put(position.y);
            writer.Put(position.z);
        }

        private static void WriteVectorInt(NetDataWriter writer, NetVector3Int position)
        {
            writer.Put(position.x);
            writer.Put(position.y);
            writer.Put(position.z);
        }

        public static NetVector3Int ReadVectorInt(NetPacketReader reader)
        {
            return new NetVector3Int(reader.GetInt(), reader.GetInt(), reader.GetInt());
        }
    }
}
