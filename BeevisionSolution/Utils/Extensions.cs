using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace BeevisionSolution.Utils
{
    public static class Extensions
    {
        public static T DeepCopy<T>(this T obj) where T : class
        {
            T rObj = null;
            if (null != obj)
            {
                try
                {
                    lock (obj)
                    {
                        var Serializer = new BinaryFormatter();
                        using (var ms = new MemoryStream())
                        {
                            Serializer.Serialize(ms, obj);
                            ms.Position = 0;
                            rObj = (T)Serializer.Deserialize(ms);
                        }
                    }
                }
                catch { }
            }
            return rObj;
        }

        public async static Task<T> DeepCopyAsync<T>(this T obj) where T : class => await Task.Run(obj.DeepCopy);
    }
}