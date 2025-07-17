using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using ServerLibrary.Structs;

namespace LightServer.Base.PlayersModule
{
    public class ServerWeaponsSpotsModule : ServerModuleBase
    {
        private class WeaponSpotData
        {
            private string weaponName;
            private DateTime lastPickupTime;

            public WeaponSpotData(string weaponName)
            {
                this.weaponName = weaponName;
            }

            public string WeaponName => weaponName;

            public string UpdateTime()
            {
                lastPickupTime = DateTime.UtcNow;

                return GetTime();
            }

            public string GetTime()
            {
                return lastPickupTime.ToString(CultureInfo.InvariantCulture);
            }
        }

        private Dictionary<int, WeaponSpotData> spots = new Dictionary<int, WeaponSpotData>(5);
        ServerPlayersModule playersModule;
        public void Init()
        {
            playersModule = server.GetModule<ServerPlayersModule>();

            server.OnRPCRecieved += Server_OnRPCRecieved;
        }

        private void Server_OnRPCRecieved(ERPCName rpcType, int id, NetPacketReader reader)
        {
            switch (rpcType)
            {
                case ERPCName.InitPlayer:
                    SendWeaponsState(id);
                    break;
                case ERPCName.PickupWeapon:
                    HandlePickupWeaponRPC(id, reader);
                    break;
            }
        }

        private void HandlePickupWeaponRPC(int id, NetPacketReader reader)
        {
            var mapItemId = reader.GetInt();
            var weaponName = reader.GetString();


            if (!spots.ContainsKey(mapItemId))
            {
                spots.Add(mapItemId, new WeaponSpotData(weaponName));
            }

            var time = spots[mapItemId].UpdateTime();

            playersModule.CommandToAllClients(delegate (int clientID)
            {
                server.GetPeerByID(clientID).CMDSendHandlePickUpWeapon(mapItemId, time);
            });
        }

        private void SendWeaponsState(int clientID)
        {

            foreach (var i in spots)
            {
                server.GetPeerByID(clientID).CMDSendHandlePickUpWeapon(i.Key, i.Value.GetTime());
            }
        }
    }
}
