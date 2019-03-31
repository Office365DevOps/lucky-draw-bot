using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.Storage.Table
{
    public static class TableEntityExtensions
    {
        private const string ChunkTypePropertyPostfix = "_Chunk";
        private const string ChunkTypeValue = "{\"type\": \"utf8\"}";
        private const int NoChangeLength = 31 * 1024;
        private const int MaxTotalBytes = 1020 * 1024;
        private const int MaxBytesPerProperty = 63 * 1024;

        public static bool ChunkLongString(this TableEntity entity, IDictionary<string, EntityProperty> properties, string propertyName, int maxBytes = MaxTotalBytes)
        {
            var s = properties[propertyName].StringValue;
            if (s.Length <= NoChangeLength)
            {
                return false;
            }

            int maxProperties = maxBytes / MaxBytesPerProperty;
            maxBytes = MaxBytesPerProperty * maxProperties;

            byte[] bytes = Encoding.UTF8.GetBytes(s);
            while (bytes.Length > maxBytes)
            {
                s = s.Substring(0, s.Length - (bytes.Length - maxBytes));
            }

            var propertiesCount = (bytes.Length + MaxBytesPerProperty - 1) / MaxBytesPerProperty;
            for(int i = 0; i < propertiesCount; i++)
            {
                var length = (i < (propertiesCount - 1)) ? MaxBytesPerProperty : bytes.Length - (i * MaxBytesPerProperty);
                var b = new byte[length];
                Array.Copy(bytes, i * MaxBytesPerProperty, b, 0, length);

                properties[propertyName + i.ToString("00")] = EntityProperty.GeneratePropertyForByteArray(b);
            }

            properties[propertyName + ChunkTypePropertyPostfix] = EntityProperty.GeneratePropertyForString(ChunkTypeValue);
            properties.Remove(propertyName);
            return true;
        }

        public static bool ConcatenateLongString(this TableEntity entity, IDictionary<string, EntityProperty> properties, string propertyName)
        {
            var chunkTypePropertyName = propertyName + ChunkTypePropertyPostfix;
            if (!properties.ContainsKey(chunkTypePropertyName))
            {
                return false;
            }

            if (properties[chunkTypePropertyName].StringValue != ChunkTypeValue)
            {
                throw new Exception("Unsupported chunk type: " + properties[chunkTypePropertyName].StringValue);
            }
            properties.Remove(chunkTypePropertyName);

            var maxProperties = MaxTotalBytes / MaxBytesPerProperty;
            List<byte> bytes = new List<byte>();
            for(int i = 0; i < maxProperties; i++)
            {
                var name = propertyName + i.ToString("00");
                if (!properties.ContainsKey(name))
                {
                    break;
                }
                bytes.AddRange(properties[name].BinaryValue);
                properties.Remove(name);
            }

            var s = Encoding.UTF8.GetString(bytes.ToArray());
            properties[propertyName] = EntityProperty.GeneratePropertyForString(s);
            return true;
        }
    }
}
