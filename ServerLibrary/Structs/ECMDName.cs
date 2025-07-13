using System;
using System.Collections.Generic;
using System.Text;

namespace ServerLibrary.Structs
{
    public enum ECMDName
    {
        GetPlayersList,
        GetMap,
        GetTime,
        GetGameState,
        RespawnPlayer,
        UpdatePlayerTransform,
        DestroyPlayer,
        SpawnProjectile,
        GetChangedBlocks,
        DestroyProjectile,
        PickupWeapon
    }
}
