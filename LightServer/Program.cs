using LightServer.Base;

namespace LightServer
{
    internal class Program
    {
        private static Server server;
        static void Main(string[] args)
        {
            server = new Server();

            StartCommandThread();

            server.InitServer();

        }


        static void StartCommandThread()
        {
            // Поток для команд
            new Thread(() =>
            {
                while (server.IsRunning)
                {
                    var input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    HandleCommand(input.Trim());
                }
            })
            { IsBackground = true }.Start();
        }

        static void HandleCommand(string input)
        {

            var command = input.ToLower();

            if (command == "exit" || command == "quit")
            {
                server.Shutdown();
                return;
            }

            if (command.StartsWith("send_data"))
            {
                var data =  command.Replace("send_data", string.Empty);
            }



            //switch (input.ToLower())
            //{
            //    case "exit":
            //    case "quit":
            //        server.Shutdown();
            //        break;
            //}
        }
    }
}
