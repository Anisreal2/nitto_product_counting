using BeeLightModule;
using BeeLightModule.Models;
using BeevisionSolution.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeevisionSolution.Utils
{
    public class LightConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(LightEthernetControlBase));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var type = jo["ModelName"].Value<string>();
            switch (type)
            {
                case "OPT_DPA2024_8":
                    return jo.ToObject<OPT_DPA2024_8>(serializer);
                case "OPT_DPA2024_4":
                    return jo.ToObject<OPT_DPA2024_4>(serializer);
                case "OPT_DPA2024_16":
                    return jo.ToObject<OPT_DPA2024_16>(serializer);
                case "OPT_DPA6024E_2":
                    return jo.ToObject<OPT_DPA6024E_2>(serializer);
                case "ZH_BDKD12024_4K":
                    return jo.ToObject<ZH_BDKD12024_4K>(serializer);
                case "ZH_BDKD12024_8K":
                    return jo.ToObject<ZH_BDKD12024_8K>(serializer);
                case "ZH_BDKD12524_2K":
                    return jo.ToObject<ZH_BDKD12524_2K>(serializer);
                case "ZH_BDKD12524_4K":
                    return jo.ToObject<ZH_BDKD12524_4K>(serializer);
                case "ZH_BDKD20024_12K":
                    return jo.ToObject<ZH_BDKD20024_12K>(serializer);
                case "ZH_BDKD20024_4K":
                    return jo.ToObject<ZH_BDKD20024_4K>(serializer);
                case "ZH_BDKD20024_8K":
                    return jo.ToObject<ZH_BDKD20024_8K>(serializer);
                case "ZH_DCT24200_2K":
                    return jo.ToObject<ZH_DCT24200_2K>(serializer);
            }

            return jo.ToObject<Object>(serializer);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
