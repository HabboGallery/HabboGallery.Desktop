using Sulakore.Habbo;
using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo2020
{
    public class HCrackableStuffData : HStuffData
    {
        public string State { get; set; }
        public int Hits { get; set; }
        public int Target { get; set; }

        public HCrackableStuffData()
            : base(HStuffDataFormat.Crackable)
        { }
        public HCrackableStuffData(HPacket packet)
            : this()
        {
            State = packet.ReadUTF8();
            Hits = packet.ReadInt32();
            Target = packet.ReadInt32();
        }
    }
}
