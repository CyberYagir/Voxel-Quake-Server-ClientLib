using Newtonsoft.Json;

namespace LightServer.Configs
{
    public class FileObject<T>
    {
        public virtual string GetPath() => string.Empty;

        public void SaveFile()
        {
            var json = JsonConvert.SerializeObject(this);

            File.WriteAllText(GetPath(), json);
        }

        public T LoadFile()
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(GetPath()));
        }
    }
}
