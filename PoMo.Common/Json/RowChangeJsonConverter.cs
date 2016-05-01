using System;
using Newtonsoft.Json;
using PoMo.Common.DataObjects;

namespace PoMo.Common.Json
{
    internal sealed class RowChangeJsonConverter : JsonConverter
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