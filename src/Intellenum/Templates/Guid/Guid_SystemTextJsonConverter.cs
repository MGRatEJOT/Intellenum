﻿
        /// <summary>
        /// Converts an Intellenum instance to or from JSON.
        /// </summary>
        public class VOTYPESystemTextJsonConverter : global::System.Text.Json.Serialization.JsonConverter<VOTYPE>
        {
            public override VOTYPE Read(ref global::System.Text.Json.Utf8JsonReader reader, global::System.Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options)
            {
                return VOTYPE.__Deserialize(reader.GetGuid());
            }

            public override void Write(System.Text.Json.Utf8JsonWriter writer, VOTYPE value, global::System.Text.Json.JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.Value);
            }

#if NET6_0_OR_GREATER // we can't call Read or use GetGuid from JsonReader as it expects a token type of string, but here we have have 'propertyname'.
            public override VOTYPE ReadAsPropertyName(ref global::System.Text.Json.Utf8JsonReader reader, global::System.Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options)
            {
                if (global::System.Guid.TryParse(reader.GetString(), out global::System.Guid g))
                {
                    return VOTYPE.__Deserialize(g);
                }

                throw new global::System.Text.Json.JsonException("Unable to parse the GUID for an instance of VOTYPE");
            }

            public override void WriteAsPropertyName(System.Text.Json.Utf8JsonWriter writer, VOTYPE value, global::System.Text.Json.JsonSerializerOptions options)
            {
                writer.WritePropertyName(value.Value.ToString());
            }
#endif            
        }