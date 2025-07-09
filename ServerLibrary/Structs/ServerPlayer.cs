using System.Collections.Generic;

namespace LightServer.Base.PlayersModule
{
    public enum EClientState
    {
        Connecting,
        InGame
    }

    public struct NetVector3
    {
        public float x;
        public float y;
        public float z;

        public NetVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
    public struct NetVector3Int
    {
        public int x;
        public int y;
        public int z;

        public NetVector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public struct NetBlockData
    {
        public int type;
        public int material;

        public NetBlockData(int type, int material)
        {
            this.type = type;
            this.material = material;
        }
    }
    public struct NetChunkData
    {
        public Dictionary<int, NetBlockData> blocks;

        public NetChunkData(Dictionary<int, NetBlockData> blocks)
        {
            this.blocks = blocks;
        }
    }

    public class ServerPlayer
    {
        public int Id { get; private set; }
        public EClientState State { get; private set; } = EClientState.Connecting;
        public NetVector3 Position { get; private set; }
        public NetVector3 Rotation { get; private set; }
        public NetVector3 Velocity { get; private set; }
        public float CameraX { get; private set; }

        public int Weapon { get; private set; }
        public float Health { get; private set; }
        public float Armor { get; private set; }
        public string Nickname { get; private set; }

        public bool IsInited { get; private set; }
        public bool IsSpawned { get; private set; }

        public void ChangeState(EClientState state)
        {
            State = state;
        }

        public void Init(int id, string nickName)
        {
            Nickname = nickName;
            Id = id;
            IsInited = true;
        }

        public void SpawnPlayer()
        {
            IsSpawned = true;
        }

        public void UpdateCameraX(float cameraX)
        {
            CameraX = cameraX;
        }

        public void UpdatePosition(NetVector3 pos)
        {
            Position = pos;
        }

        public void UpdateRotation(NetVector3 rot)
        {
            Rotation = rot;
        }

        public void UpdateVelocity(NetVector3 vel)
        {
            Velocity = vel;
        }
    }
}
