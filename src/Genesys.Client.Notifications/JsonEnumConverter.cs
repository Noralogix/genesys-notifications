using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genesys.Client.Notifications
{
    public class JsonEnumConverter<T> : JsonConverter<T>
    {
        // Inspired by http://stackoverflow.com/questions/22752075/how-can-i-ignore-unknown-enum-values-during-json-deserialization

        public override T Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
        {
            var isNullable = IsNullableType(objectType);
            var enumType = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var enumText = reader.GetString();

                    if (!string.IsNullOrEmpty(enumText))
                    {
                        var enumMembers = enumType.GetMembers();
                        string match = null;

                        foreach (var enumMember in enumMembers)
                        {
                            var memberAttributes = enumMember.GetCustomAttributes(typeof(EnumMemberAttribute), false);
                            if (memberAttributes.Length > 0)
                            {
                                var attribute = memberAttributes.FirstOrDefault() as EnumMemberAttribute;
                                if (string.Equals(attribute.Value, enumText, StringComparison.OrdinalIgnoreCase))
                                {
                                    match = enumMember.Name;
                                    break;
                                }
                            }
                        }

                        if (match != null)
                        {
                            return (T)Enum.Parse(enumType, match);
                        }
                    }
                    break;
                case JsonTokenType.Number:
                    var enumVal = Convert.ToInt32(reader.GetInt32());
                    var values = (int[])Enum.GetValues(enumType);
                    if (values.Contains(enumVal))
                    {
                        return (T)Enum.Parse(enumType, enumVal.ToString());
                    }
                    break;
            }

            // See if it has a member named "OUTDATED_SDK_VERSION"
            var names = Enum.GetNames(enumType);
            var outdatedSdkVersionMemberName = names
                .FirstOrDefault(n => string.Equals(n, "OUTDATED_SDK_VERSION", StringComparison.OrdinalIgnoreCase));

            // Return parsed "OUTDATED_SDK_VERSION" member
            return (T)(outdatedSdkVersionMemberName != default(string)
                ? Enum.Parse(enumType, outdatedSdkVersionMemberName)
                : Activator.CreateInstance(enumType));
        }
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var enumMembers = value.GetType().GetMembers();

            foreach (var enumMember in enumMembers)
            {
                var memberAttributes = enumMember.GetCustomAttributes(typeof(EnumMemberAttribute), false);
                if (memberAttributes.Length > 0)
                {
                    var attribute = memberAttributes.FirstOrDefault() as EnumMemberAttribute;
                    if (string.Equals(enumMember.Name, value.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        writer.WriteStringValue(attribute.Value);
                        return;
                    }
                }
            }
        }

        private bool IsNullableType(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
