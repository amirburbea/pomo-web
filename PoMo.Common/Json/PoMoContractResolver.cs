using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PoMo.Common.DataObjects;

namespace PoMo.Common.Json
{
    public sealed class PoMoContractResolver : DefaultContractResolver
    {
        private static readonly JsonConverter _stringEnumConverter = new StringEnumConverter { CamelCaseText = true };

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);
            if (typeof(RowChangeBase).IsAssignableFrom(objectType))
            {
                contract.Converter = new RowChangeConverter();
            }
            else if (objectType == typeof(DataTable))
            {
                contract = this.CreateObjectContract(typeof(DataTable));
                contract.DefaultCreator = () => new DataTable();
                contract.IsReference = true;
                contract.Converter = new DataTableJsonConverter();
            }
            else if (PoMoContractResolver.CanApplyChanges(objectType) && objectType.IsValueType && (Nullable.GetUnderlyingType(objectType) ?? objectType).IsEnum)
            {
                contract.Converter = PoMoContractResolver._stringEnumConverter;
            }
            return contract;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
            if (PoMoContractResolver.CanApplyChanges(member.DeclaringType))
            {
                jsonProperty.PropertyName = PoMoContractResolver.ConvertToCamelCase(jsonProperty.PropertyName);
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

        private sealed class DataTableJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(DataTable).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                DataTable dataTable = existingValue as DataTable ?? new DataTable();
                reader.Read();
                dataTable.TableName = reader.ReadAsString();
                reader.Read();
                reader.Read();
                reader.Read();
                while (reader.TokenType != JsonToken.EndArray)
                {
                    reader.Read();
                    DataColumn dataColumn = new DataColumn { ColumnName = reader.ReadAsString() };
                    reader.Read();
                    dataColumn.DataType = DataTableJsonConverter.GetType((TypeCode)reader.ReadAsInt32().GetValueOrDefault());
                    reader.Read();
                    reader.Read();
                    dataTable.Columns.Add(dataColumn);
                }
                reader.Read();
                reader.Read();
                reader.Read();
                List<DataColumn> primaryKey = new List<DataColumn>();
                while (reader.TokenType != JsonToken.EndArray)
                {
                    primaryKey.Add(dataTable.Columns[Convert.ToInt32(reader.Value)]);
                    reader.Read();
                }
                reader.Read();
                reader.Read();
                reader.Read();
                object[] values = new object[dataTable.Columns.Count];
                while (reader.TokenType != JsonToken.EndArray)
                {
                    reader.Read();
                    for (int index = 0; index < values.Length; index++)
                    {
                        values[index] = serializer.Deserialize(reader, dataTable.Columns[index].DataType);
                        reader.Read();
                    }
                    dataTable.Rows.Add(values);
                    reader.Read();
                }
                reader.Read();
                if (primaryKey.Count != 0)
                {
                    dataTable.PrimaryKey = primaryKey.ToArray();
                }
                dataTable.AcceptChanges();
                return dataTable;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                DataTable dataTable = (DataTable)value;
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                writer.WriteValue(dataTable.TableName);
                writer.WritePropertyName("cols");
                writer.WriteStartArray();
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("name");
                    writer.WriteValue(dataColumn.ColumnName);
                    writer.WritePropertyName("type");
                    writer.WriteValue((int)Type.GetTypeCode(dataColumn.DataType));
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                writer.WritePropertyName("key");
                writer.WriteStartArray();
                foreach (DataColumn dataColumn in dataTable.PrimaryKey)
                {
                    writer.WriteValue(dataTable.Columns.IndexOf(dataColumn));
                }
                writer.WriteEndArray();
                writer.WritePropertyName("rows");
                writer.WriteStartArray();
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    writer.WriteStartArray();
                    foreach (object rowValue in dataRow.ItemArray)
                    {
                        serializer.Serialize(writer, rowValue);
                    }
                    writer.WriteEndArray();
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            private static Type GetType(TypeCode typeCode)
            {
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return typeof(bool);
                    case TypeCode.DateTime:
                        return typeof(DateTime);
                    case TypeCode.Byte:
                        return typeof(byte);
                    case TypeCode.SByte:
                        return typeof(sbyte);
                    case TypeCode.Int16:
                        return typeof(short);
                    case TypeCode.UInt16:
                        return typeof(ushort);
                    case TypeCode.Int32:
                        return typeof(int);
                    case TypeCode.UInt32:
                        return typeof(uint);
                    case TypeCode.Int64:
                        return typeof(long);
                    case TypeCode.UInt64:
                        return typeof(ulong);
                    case TypeCode.Single:
                        return typeof(float);
                    case TypeCode.Double:
                        return typeof(double);
                    case TypeCode.Decimal:
                        return typeof(decimal);
                    case TypeCode.Char:
                        return typeof(char);
                    case TypeCode.String:
                        return typeof(string);
                    case TypeCode.DBNull:
                        return typeof(DBNull);
                }
                return typeof(object);
            }
        }

        private sealed class RowChangeConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(RowChangeBase).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                reader.Read();
                reader.Read();
                RowChangeType changeType = serializer.Deserialize<RowChangeType>(reader);
                reader.Read();
                reader.Read();
                object rowKey = serializer.Deserialize(reader);
                reader.Read();
                try
                {
                    if (changeType == RowChangeType.Removed)
                    {
                        return new RowRemoved { RowKey = rowKey };
                    }
                    reader.Read();
                    if (changeType == RowChangeType.Added)
                    {
                        return new RowAdded { RowKey = rowKey, Data = serializer.Deserialize<object[]>(reader) };
                    }
                    ColumnChange[] columnChanges = serializer.Deserialize<ColumnChange[]>(reader);
                    RowColumnsChanged rowColumnsChanged = new RowColumnsChanged { RowKey = rowKey };
                    Array.ForEach(columnChanges, rowColumnsChanged.ColumnChanges.Add);
                    return rowColumnsChanged;
                }
                finally
                {
                    reader.Read();
                }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                RowChangeBase change = (RowChangeBase)value;
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                serializer.Serialize(writer, change.ChangeType);
                writer.WritePropertyName("rowKey");
                serializer.Serialize(writer, change.RowKey);
                switch (change.ChangeType)
                {
                    case RowChangeType.Added:
                        writer.WritePropertyName("data");
                        serializer.Serialize(writer, ((RowAdded)change).Data);
                        break;
                    case RowChangeType.ColumnsChanged:
                        writer.WritePropertyName("changes");
                        serializer.Serialize(writer, ((RowColumnsChanged)change).ColumnChanges);
                        break;
                }
                writer.WriteEndObject();
            }
        }
    }
}