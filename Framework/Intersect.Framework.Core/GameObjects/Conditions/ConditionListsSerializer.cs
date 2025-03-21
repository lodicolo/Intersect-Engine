﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Intersect.Framework.Core.GameObjects.Conditions;

public partial class ConditionListsSerializer : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer
    )
    {
        var jsonObject = JObject.Load(reader);
        var properties = jsonObject.Properties().ToList();
        var lists = existingValue != null ? (ConditionLists) existingValue : new ConditionLists();
        lists.Load((string) properties[0].Value);

        return lists;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("Lists");
        serializer.Serialize(writer, ((ConditionLists)value).Data());
        writer.WriteEndObject();
    }
}