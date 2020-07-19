using Sulakore.Habbo;
using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo
{
    public class CHItem : HItem
    {
        public string ExtraData { get; set; }

        public CHItem(HPacket packet)
            : base(packet)
        { }

        public new static CHItem[] Parse(HPacket packet)
        {
            packet.ReadInt32();
            packet.ReadInt32();

            var items = new CHItem[packet.ReadInt32()];
            for (int i = 0; i < items.Length; i++)
            {
                int pos = packet.Position + 4;
                packet.ReadUTF8(ref pos);
                pos += 16;

                items[i] = new CHItem(packet)
                {
                    ExtraData = packet.ReadUTF8(pos)
                };
            }
            return items;
        }
    }
}