using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo2020
{
    /// <summary>
    /// Temporary glue and hacks for 64-bit values
    /// </summary>
    public static class HPacketHacks
    {
        public static long ReadInt64(this HPacket packet)
        {
            packet.Position += 4;
            return packet.ReadInt32();
        }
    }
}
