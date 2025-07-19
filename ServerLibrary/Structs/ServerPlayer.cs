using System;
using System.Collections.Generic;

namespace LightServer.Base.PlayersModule
{
    public enum EClientState
    {
        Connecting,
        InGame
    }

    public enum EWeaponType
    {
        Drill,
        Machinegun,
        Rocket,
        Rail,
        Shotgun
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

    [Serializable]
    public struct NetVector3Int : IEquatable<NetVector3Int>
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

        public bool Equals(NetVector3Int other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override bool Equals(object obj)
        {
            return obj is NetVector3Int other && Equals(other);
        }

        public override int GetHashCode()
        {
            // Хорошее распределение хеша для 3D координат
            unchecked
            {
                int hash = x;
                hash = (hash * 397) ^ y;
                hash = (hash * 397) ^ z;
                return hash;
            }
        }

        public static bool operator ==(NetVector3Int a, NetVector3Int b) => a.Equals(b);
        public static bool operator !=(NetVector3Int a, NetVector3Int b) => !a.Equals(b);

        public override string ToString() => $"({x}, {y}, {z})";
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
        public EWeaponType SelectedWeapon { get; private set; }
        public EClientState State { get; private set; } = EClientState.Connecting;
        public NetVector3 Position { get; private set; }
        public NetVector3 Rotation { get; private set; }
        public NetVector3 Velocity { get; private set; }
        public NetVector3 SkinColor { get; private set; }

        public float CameraX { get; private set; }

        public float Health { get; private set; }
        public float Armor { get; private set; }
        public string Nickname { get; private set; }

        public bool IsInited { get; private set; }
        public bool IsSpawned { get; private set; }

        public void ChangeState(EClientState state)
        {
            State = state;
        }

        public void Init(int id, string nickName, NetVector3 netColor)
        {
            Nickname = nickName;
            Id = id;
            SkinColor = netColor;
            IsInited = true;
        }

        public void SpawnPlayer()
        {
            ChangeWeapon(EWeaponType.Machinegun);
            IsSpawned = true;
        }

        public void ChangeWeapon(EWeaponType t)
        {
            SelectedWeapon = t;
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
