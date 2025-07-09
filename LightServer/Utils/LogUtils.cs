using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerLibrary.Structs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LightServer.Utils
{
    public static class LogUtils
    {
        public static void Log(string data)
        {
            Console.WriteLine(data);
        }


        public static void LogRPC(ERPCName rpc, int id)
        {
            Log($"[RPC] form [{id}] " + rpc.ToString());
        }

        public static void LogCMD(ECMDName cmd, int id)
        {
            Log($"[CMD] to [{id}] " + cmd.ToString());
        }

        public static void LogGameState(EGameState gameState)
        {
            Log($"[STATE] " + gameState.ToString());
        }

        public static void LogWeb(string data)
        {
            Log($"[WEB] " + data);
        }

        public static void LogServer(string data)
        {
            Log($"[SRV] " + data);
        }

        public static void LogTimer(string data)
        {
            Log($"[TIME] " + data);
        }

        public static void LogFile(string data)
        {
            Log($"[FILE] " + data);
        }

        internal static void LogHard(string v)
        {
            Log($"[HARD] " + v);
        }
    }
}
