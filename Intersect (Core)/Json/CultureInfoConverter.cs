using Newtonsoft.Json;

using System;
using System.Globalization;

using Intersect.Logging;

namespace Intersect.Json
{
    public class CultureInfoConverter : JsonConverter<CultureInfo>
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, CultureInfo value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Name);
        }

        /// <inheritdoc />
        public override CultureInfo ReadJson(
            JsonReader reader,
            Type objectType,
            CultureInfo existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )

        {
            var localeName = reader.ReadAsString();

            try
            {
                return new CultureInfo(localeName);
            }
            catch (CultureNotFoundException cultureNotFoundException)
            {
                Log.Info(cultureNotFoundException);
            }
            catch (ArgumentNullException)
            {
            }

            return CultureInfo.CurrentCulture;
        }
    }
}
