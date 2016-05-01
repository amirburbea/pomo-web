using System;
using System.Data;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PoMo.Common.DataObjects;

namespace PoMo.Common.Json
{
    public sealed class JsonContractResolver : DefaultContractResolver
    {
        private static readonly JsonConverter _dataTableConverter = new DataTableJsonConverter();
        private static readonly JsonConverter _rowChangeConverter = new RowChangeJsonConverter();
        private static readonly JsonConverter _stringEnumConverter = new StringEnumConverter { CamelCaseText = true };

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);
            if (typeof(RowChangeBase).IsAssignableFrom(objectType))
            {
                contract.Converter = JsonContractResolver._rowChangeConverter;
            }
            else if (objectType == typeof(DataTable))
            {
                contract = this.CreateObjectContract(typeof(DataTable));
                contract.DefaultCreator = () => new DataTable();
                contract.IsReference = true;
                contract.Converter = JsonContractResolver._dataTableConverter;
            }
            else if (JsonContractResolver.CanApplyChanges(objectType) && objectType.IsValueType && (Nullable.GetUnderlyingType(objectType) ?? objectType).IsEnum)
            {
                contract.Converter = JsonContractResolver._stringEnumConverter;
            }
            return contract;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
            if (JsonContractResolver.CanApplyChanges(member.DeclaringType))
            {
                jsonProperty.PropertyName = JsonContractResolver.ConvertToCamelCase(jsonProperty.PropertyName);
            }
            return jsonProperty;
        }

        private static bool CanApplyChanges(Type type)
        {
            // SignalR specific objects are prefixed with Microsoft and they can not have their properties be camel cased.
            return type?.Namespace != null && !type.Assembly.IsDynamic && !type.Namespace.StartsWith(nameof(Microsoft), StringComparison.Ordinal);
        }

        private static string ConvertToCamelCase(string text)
        {
            if (text.Length == 0 || char.IsLower(text, 0))
            {
                return text;
            }
            char[] characters = text.ToCharArray();
            characters[0] = char.ToLowerInvariant(characters[0]);
            return new string(characters);
        }
    }
}