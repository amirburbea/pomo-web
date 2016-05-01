using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace PoMo.Common.Json
{
    internal sealed class DataTableJsonConverter : JsonConverter
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
}