using Sulakore.Habbo;
using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo2020
{
#nullable enable
    public class HNewItem
    {
        public long RoomItemId { get; set; }
        public long Id { get; set; }
        public HNewProductType Type { get; set; }
        public int TypeId { get; set; }

        public HFurniCategory Category { get; set; }

        public HStuffData StuffData { get; set; }

        public bool IsRecyclable { get; set; }
        public bool IsTradable { get; set; }
        public bool IsGroupable { get; set; }
        public bool IsSellable { get; set; }

        public int SecondsToExpiration { get; set; }

        public bool HasRentPeriodStarted { get; set; }
        public long RoomId { get; set; }

        public string? SlotId { get; set; }
        public int? Extra { get; set; }

        public HNewItem(HPacket packet)
        {
            RoomItemId = packet.ReadInt64();

            Type = (HNewProductType)packet.ReadUInt16();

            Id = packet.ReadInt64();
            TypeId = packet.ReadInt32();
            Category = (HFurniCategory)packet.ReadInt32();

            StuffData = HStuffData.Parse(packet);

            IsRecyclable = packet.ReadBoolean();
            IsTradable = packet.ReadBoolean();
            IsGroupable = packet.ReadBoolean();
            IsSellable = packet.ReadBoolean();

            SecondsToExpiration = packet.ReadInt32();
            HasRentPeriodStarted = packet.ReadBoolean();
            RoomId = packet.ReadInt64();

            if (Type == HNewProductType.Stuff)
            {
                SlotId = packet.ReadUTF8();
                Extra = packet.ReadInt32();
            }
        }

        public static HNewItem[] Parse(HPacket packet)
        {
            packet.ReadInt32();
            packet.ReadInt32();
            var items = new HNewItem[packet.ReadUInt16()];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new HNewItem(packet);
            }
            return items;
        }
    }
}
