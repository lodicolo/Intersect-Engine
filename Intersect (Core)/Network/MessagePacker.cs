using Intersect.Logging;
using Intersect.Network.Packets.Client;
using Intersect.Network.Packets.Server;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intersect.Network
{
    public partial class MessagePacker
    {
        public static readonly MessagePacker Instance = new MessagePacker();

        private static readonly MessagePackSerializerOptions _options = MessagePackSerializerOptions.Standard.
            WithResolver(MessagePack.Resolvers.CompositeResolver.Create(
                new IFormatterResolver[] {
                    MessagePack.Resolvers.NativeGuidResolver.Instance,
                    MessagePack.Resolvers.NativeDateTimeResolver.Instance,
                    MessagePack.Resolvers.NativeDecimalResolver.Instance,
                    MessagePack.Resolvers.StandardResolver.Instance }
                )
            );

        private readonly MessagePackSerializerOptions _compressionOptions = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);

        public byte[] Serialize(IntersectPacket packet)
        {
            // Log.Error($"{packet.GetType().FullName} Length: {packet.Data.Length} Data: {BitConverter.ToString(packet.Data)}");
            // Log.Error($"{packet.GetType().FullName}");

            var packedPacket = new PackedIntersectPacket(packet)
            {
                Data = MessagePackSerializer.Serialize(packet.GetType(), packet, _options)
            };
            // Log.Error($"{packedPacket.GetType().FullName} Length: {packedPacket.Data.Length} Data: {BitConverter.ToString(packedPacket.Data)}");

            var data = MessagePackSerializer.Serialize(packedPacket, _compressionOptions);
            // Log.Error($"<byte[]> Length: {data.Length} Data: {BitConverter.ToString(data)}");

            return data;
        }

        public object Deserialize(byte[] data)
        {
            try
            {
                // Log.Error($"<byte[]> Length: {data.Length} Data: {BitConverter.ToString(data)}");

                var packedPacket = MessagePackSerializer.Deserialize<PackedIntersectPacket>(data, _compressionOptions);
                // Log.Error($"{packedPacket.GetType().FullName} Length: {packedPacket.Data.Length} Data: {BitConverter.ToString(packedPacket.Data)}");

                var deserializedPacket = MessagePackSerializer.Deserialize(packedPacket.PacketType, packedPacket.Data, _options);
                if (deserializedPacket is IntersectPacket packet)
                {
                    // Log.Error($"{packet.GetType().FullName} Length: {packet.Data.Length} Data: {BitConverter.ToString(packet.Data)}");
                    return packet;
                }
                
                // Log.Error($"Deserialized packet is actually {deserializedPacket.GetType().FullName}");
                return null;
            }
            catch (Exception exception)
            {
                Log.Error(exception);

                return null;
            }
        }

    }
}
