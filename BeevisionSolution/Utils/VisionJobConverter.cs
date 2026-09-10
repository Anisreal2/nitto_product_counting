using BeevisionSolution.Jobs;
using BeevisionSolution.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace BeevisionSolution.Utils
{
    public class VisionJobConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(BaseJob));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var type = jo["VisionType"].Value<int>();
            switch ((JobType)type)
            {
                case JobType.TypeCamera:
                    return jo.ToObject<CameraJob>(serializer);
                case JobType.TypeAlignment:
                    return jo.ToObject<AlignJob>(serializer);
                case JobType.TypeInspection:
                    return jo.ToObject<IspJob>(serializer);
                case JobType.TypeHandEye:
                    return jo.ToObject<HEJob>(serializer);
                case JobType.TypeWatcher:
                    return jo.ToObject<WatcherJob>(serializer);
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