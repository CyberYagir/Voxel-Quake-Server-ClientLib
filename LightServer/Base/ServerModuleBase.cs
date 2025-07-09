namespace LightServer.Base
{
    public class ServerModuleBase
    {
        protected Server server;

        public void InitModule(Server server)
        {
            this.server = server;
        }
    }
}
